using System;
using System.Collections.Generic;

namespace SBR.Engine;

/// <summary>
/// Owned passives and their behaviors, in acquisition (purchase) order. The economy rework
/// (PLAN.md 2026-07-13) slimmed this to three behaviors on three power curves: The Multiplier
/// (static engine), Scar Tissue (ratchet), Totem of Undying (survival). Every payout effect
/// multiplies into the single Ticket.PayoutMultiplier product — composition law, design/10 B2.
/// The old 8-relic behavior set is retired (git history is the archive).
/// </summary>
public sealed class EffectEngine
{
    private readonly List<RelicDefinition> _owned = new List<RelicDefinition>();
    private readonly List<RelicBehavior> _behaviors = new List<RelicBehavior>();

    public IReadOnlyList<RelicDefinition> Owned => _owned;

    public void Add(RelicDefinition def)
    {
        _owned.Add(def);
        _behaviors.Add(RelicBehavior.Create(def));
    }

    /// <summary>Sell-back support: removes the passive at the index. A sold Scar Tissue loses its
    /// stacks; a sold Totem does not refund its once-per-run purchase right.</summary>
    public void RemoveAt(int index)
    {
        _owned.RemoveAt(index);
        _behaviors.RemoveAt(index);
    }

    public bool Owns(string id)
    {
        foreach (RelicDefinition d in _owned)
            if (d.Id == id) return true;
        return false;
    }

    // ---- placement (PlaceTicket, after compose, bank pre-deduction) ----

    public void ApplyTicketPlaced(Ticket ticket, double stake, double bankBeforeDeduction, bool isFirstTicketThisRound)
    {
        foreach (RelicBehavior b in _behaviors)
            b.OnTicketPlaced(ticket, stake, bankBeforeDeduction, isFirstTicketThisRound);
    }

    // ---- bust chain (scar growth), acquisition order ----

    public void OnBust(Ticket ticket)
    {
        foreach (RelicBehavior b in _behaviors) b.OnBust(ticket);
    }

    /// <summary>A ticket realized value (graded Won at FinishSweat, or cash-out accepted): the scar
    /// carrier burns its stacks here.</summary>
    public void OnTicketRealized(Ticket ticket)
    {
        foreach (RelicBehavior b in _behaviors) b.OnTicketRealized(ticket);
    }

    // ---- scar tissue (visible ratchet state) ----

    /// <summary>Current scar stacks in percentage points (0 when Scar Tissue is not owned).</summary>
    public double ScarStacks
    {
        get
        {
            var b = (ScarTissueBehavior?)Find("ScarTissue");
            return b?.Stacks ?? 0.0;
        }
    }

    // ---- totem of undying ----

    public bool HasTotemCharge
    {
        get
        {
            var b = (TotemBehavior?)Find("TotemOfUndying");
            return b != null && b.HasCharge;
        }
    }

    /// <summary>Consumes the totem's single charge (the relic stays owned as a spent trophy).</summary>
    public bool TryConsumeTotem()
    {
        var b = (TotemBehavior?)Find("TotemOfUndying");
        return b != null && b.TryConsume();
    }

    private RelicBehavior? Find(string op)
    {
        foreach (RelicBehavior b in _behaviors)
            if (b.Op == op) return b;
        return null;
    }
}

/// <summary>A parameterized passive behavior, constructed from its <see cref="RelicDefinition"/>.</summary>
internal abstract class RelicBehavior
{
    protected readonly RelicDefinition Def;

    protected RelicBehavior(RelicDefinition def) => Def = def;

    public string Op => Def.Op;

    public double Param(string key) => Def.Params.TryGetValue(key, out double v) ? v : 0.0;

    // Hook sites (default no-op).
    public virtual void OnTicketPlaced(Ticket ticket, double stake, double bankBeforeDeduction,
        bool isFirstTicketThisRound) { }
    public virtual void OnBust(Ticket ticket) { }
    public virtual void OnTicketRealized(Ticket ticket) { }

    public static RelicBehavior Create(RelicDefinition def)
    {
        switch (def.Op)
        {
            case "ParlayPayoutMult": return new ParlayPayoutMultBehavior(def);
            case "ScarTissue": return new ScarTissueBehavior(def);
            case "TotemOfUndying": return new TotemBehavior(def);
            default: throw new ArgumentException($"Unknown relic op '{def.Op}'");
        }
    }
}

/// <summary>The Multiplier: parlays of minLegs+ legs multiply the payout product. Full power at
/// purchase — the static engine of the three power curves.</summary>
internal sealed class ParlayPayoutMultBehavior : RelicBehavior
{
    public ParlayPayoutMultBehavior(RelicDefinition def) : base(def) { }

    public override void OnTicketPlaced(Ticket ticket, double stake, double bankBeforeDeduction,
        bool isFirstTicketThisRound)
    {
        if (ticket.Legs.Count >= (int)Param("minLegs"))
            ticket.PayoutMultiplier *= Param("mult");
    }
}

/// <summary>
/// Scar Tissue: the ratchet (design/10 B, Allen's spec + carrier semantics). Every bust adds
/// stacks scaled by the busted ticket's stake fraction at placement — full ppPerBust for stakes
/// ≥ fullStakeFraction of the bank, proportionally less below (the farming guard). The FIRST
/// ticket placed each round carries the current stacks: its payout product gets ×(1 + stacks/100),
/// and when it realizes value (win or cash-out) the stacks burn to zero. A busted carrier feeds
/// the scar like any bust. Stacks are uncapped, persist across rounds, and never unwind on their
/// own — ratchets ratchet.
/// </summary>
internal sealed class ScarTissueBehavior : RelicBehavior
{
    public ScarTissueBehavior(RelicDefinition def) : base(def) { }

    public double Stacks { get; private set; }

    public override void OnTicketPlaced(Ticket ticket, double stake, double bankBeforeDeduction,
        bool isFirstTicketThisRound)
    {
        double fullStake = Param("fullStakeFraction") * bankBeforeDeduction;
        double fraction = fullStake <= 0 ? 1.0 : Math.Min(1.0, stake / fullStake);
        ticket.ScarStacksIfBust = Param("ppPerBust") * fraction;

        if (isFirstTicketThisRound && Stacks > 0)
        {
            ticket.ScarCarrier = true;
            ticket.PayoutMultiplier *= 1.0 + Stacks / 100.0;
        }
    }

    public override void OnBust(Ticket ticket) => Stacks += ticket.ScarStacksIfBust;

    public override void OnTicketRealized(Ticket ticket)
    {
        if (ticket.ScarCarrier)
        {
            Stacks = 0;
            ticket.ScarCarrier = false;
        }
    }
}

/// <summary>Totem of Undying: one charge; consumed by Run.Settle when a non-final payment cannot
/// be met. The definition stays in the owned list after burning (a spent trophy) — the shop's
/// once-per-run rule lives on Run, not here.</summary>
internal sealed class TotemBehavior : RelicBehavior
{
    private int _charges;

    public TotemBehavior(RelicDefinition def) : base(def) => _charges = (int)Param("charges");

    public bool HasCharge => _charges > 0;

    public bool TryConsume()
    {
        if (_charges <= 0) return false;
        _charges--;
        return true;
    }
}
