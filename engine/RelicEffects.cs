using System;
using System.Collections.Generic;

namespace SBR.Engine;

/// <summary>One row of the visible effect-state snapshot (ratchet stacks, streaks, factors) —
/// the UI-facing view design/10 mandates for persistent item state (PLAN.md rev 5 §20).</summary>
public readonly struct EffectStat
{
    public string Id { get; }
    public string Label { get; }
    public double Value { get; }

    public EffectStat(string id, string label, double value)
    {
        Id = id;
        Label = label;
        Value = value;
    }
}

/// <summary>The RoundResolution payload (PLAN.md rev 5 §3): emitted after the terminal
/// ledger (Free Bet refunds resolved) and BEFORE the payment. The System reads PrePaymentPnl;
/// the Bad Beat Jar reads the ticket terminal counts.</summary>
public readonly struct RoundResolution
{
    public int Round { get; }
    public double PrePaymentPnl { get; }
    public int TicketsPlaced { get; }
    public int TicketsWon { get; }
    public int TicketsLost { get; }
    public int TicketsCashedOut { get; }
    public double RefundsIssued { get; }

    public RoundResolution(int round, double prePaymentPnl, int placed, int won, int lost,
        int cashedOut, double refundsIssued)
    {
        Round = round;
        PrePaymentPnl = prePaymentPnl;
        TicketsPlaced = placed;
        TicketsWon = won;
        TicketsLost = lost;
        TicketsCashedOut = cashedOut;
        RefundsIssued = refundsIssued;
    }
}

/// <summary>
/// Owned passives and their behaviors, in acquisition (purchase) order. Charm expansion
/// (PLAN.md rev 5): a typed hook pipeline — OnAcquire, OnSell, OnTicketPlaced, OnLock,
/// OnLegResolved (post-window, final ticket-local grade), OnBust, OnTicketRealized,
/// OnRoundResolved, OnShopEnter, CashOutQuoteScale — with every payout effect writing one
/// NAMED ×(1+x) factor into the ticket's factor map (the one-product-slot law, design/10 B2).
/// Behaviors are constructed on Add and discarded on RemoveAt: stateful passives reset on
/// sale, start fresh on reacquisition, and accrue nothing while unowned.
/// </summary>
public sealed class EffectEngine
{
    private readonly List<RelicDefinition> _owned = new List<RelicDefinition>();
    private readonly List<RelicBehavior> _behaviors = new List<RelicBehavior>();
    private readonly Action<double> _grantComps;

    public IReadOnlyList<RelicDefinition> Owned => _owned;

    /// <param name="grantComps">Comps seam (Comp'd Suite pays through it; quantized by Run).</param>
    public EffectEngine(Action<double>? grantComps = null)
        => _grantComps = grantComps ?? (_ => { });

    public void Add(RelicDefinition def)
    {
        _owned.Add(def);
        RelicBehavior b = RelicBehavior.Create(def);
        _behaviors.Add(b);
        b.OnAcquire();
    }

    /// <summary>Sell-back: removes the passive at the index. The behavior instance is discarded —
    /// stacks, streaks, and charges die with it (PLAN.md rev 5 §1: reset on sale).</summary>
    public void RemoveAt(int index)
    {
        _behaviors[index].OnSell();
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

    // ---- lock (after the comps accrual commit; factors snapshot here) ----

    public void ApplyLock(Run run)
    {
        foreach (RelicBehavior b in _behaviors)
            b.OnLock(run);
    }

    // ---- leg resolution (exactly once per leg, AFTER any pending window closed) ----

    public void OnLegResolved(Ticket ticket, Leg leg, LegGrade finalGrade)
    {
        foreach (RelicBehavior b in _behaviors)
            b.OnLegResolved(ticket, leg, finalGrade);
    }

    /// <summary>Photo's designed toggle (PLAN.md rev 5 §2): after a void changes the active-leg
    /// set, only the photo factor is re-evaluated — nothing else recomputes.</summary>
    public void RefreshPhotoFactor(Ticket ticket)
    {
        var photo = (LongshotPhotoBehavior?)Find("LegBandProductFlag");
        photo?.Refresh(ticket);
    }

    // ---- bust chain (scar growth), acquisition order ----

    public void OnBust(Ticket ticket)
    {
        foreach (RelicBehavior b in _behaviors) b.OnBust(ticket);
    }

    /// <summary>A ticket realized value (graded Won at FinishSweat, or cash-out accepted).</summary>
    public void OnTicketRealized(Ticket ticket)
    {
        foreach (RelicBehavior b in _behaviors) b.OnTicketRealized(ticket, _grantComps);
    }

    // ---- round resolution (after refunds, before the payment) ----

    public void OnRoundResolved(RoundResolution res)
    {
        foreach (RelicBehavior b in _behaviors) b.OnRoundResolved(res);
    }

    // ---- shop entry (one-time per visit; NEVER fired by a Manager redeal) ----

    public void OnShopEnter(Run run)
    {
        foreach (RelicBehavior b in _behaviors) b.OnShopEnter(run);
    }

    // ---- quote scaling (Golden Parachute) ----

    public double CashOutQuoteScale
    {
        get
        {
            double s = 1.0;
            foreach (RelicBehavior b in _behaviors) s *= b.CashOutQuoteScale;
            return s;
        }
    }

    /// <summary>House Key's payment-view factor: getters apply it, nothing mutates (rev 5 §9).</summary>
    public double PaymentFactor
    {
        get
        {
            double f = 1.0;
            foreach (RelicBehavior b in _behaviors) f *= b.PaymentFactor;
            return f;
        }
    }

    // ---- visible state (UI snapshot; design/10's ratchet-visibility mandate) ----

    public List<EffectStat> StateSnapshot()
    {
        var stats = new List<EffectStat>();
        foreach (RelicBehavior b in _behaviors) b.AddStats(stats);
        return stats;
    }

    // ---- scar tissue (visible ratchet state, pre-expansion API kept) ----

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

    /// <summary>True when the owned Totem's charge is spent — its resale value is zero.</summary>
    public bool TotemSpent(RelicDefinition def)
        => def.Op == "TotemOfUndying" && !HasTotemCharge;

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

    // Hook sites (default no-op), fired in the documented order.
    public virtual void OnAcquire() { }
    public virtual void OnSell() { }
    public virtual void OnTicketPlaced(Ticket ticket, double stake, double bankBeforeDeduction,
        bool isFirstTicketThisRound) { }
    public virtual void OnLock(Run run) { }
    public virtual void OnLegResolved(Ticket ticket, Leg leg, LegGrade finalGrade) { }
    public virtual void OnBust(Ticket ticket) { }
    public virtual void OnTicketRealized(Ticket ticket, Action<double> grantComps) { }
    public virtual void OnRoundResolved(RoundResolution res) { }
    public virtual void OnShopEnter(Run run) { }
    public virtual double CashOutQuoteScale => 1.0;
    public virtual double PaymentFactor => 1.0;
    public virtual void AddStats(List<EffectStat> stats) { }

    public static RelicBehavior Create(RelicDefinition def)
    {
        switch (def.Op)
        {
            case "ParlayPayoutMult": return new ParlayPayoutMultBehavior(def);
            case "ScarTissue": return new ScarTissueBehavior(def);
            case "TotemOfUndying": return new TotemBehavior(def);
            case "LegBandRatchet": return new ChalkEaterBehavior(def);
            case "LegBandProductFlag": return new LongshotPhotoBehavior(def);
            case "FullRideRatchet": return new IronHandsBehavior(def);
            case "CashOutQuoteScale": return new GoldenParachuteBehavior(def);
            case "ShopEnterCompsInterest": return new RakesRebateBehavior(def);
            case "CompsHeldProduct": return new WhaleCardBehavior(def);
            case "AllLossRoundRatchet": return new BadBeatJarBehavior(def);
            case "ProductPlusPaymentFactor": return new HouseKeyBehavior(def);
            case "StreakRatchet": return new TheSystemBehavior(def);
            case "LegCountCompsPay": return new CompdSuiteBehavior(def);
            case "NoOp": return new NoOpBehavior(def);
            case "ResaleValueProduct": return new TheCollectionBehavior(def);
            default: throw new ArgumentException($"Unknown relic op '{def.Op}'");
        }
    }
}

/// <summary>The Multiplier: parlays of minLegs+ legs multiply the payout product.</summary>
internal sealed class ParlayPayoutMultBehavior : RelicBehavior
{
    public ParlayPayoutMultBehavior(RelicDefinition def) : base(def) { }

    public override void OnTicketPlaced(Ticket ticket, double stake, double bankBeforeDeduction,
        bool isFirstTicketThisRound)
    {
        if (ticket.Legs.Count >= (int)Param("minLegs"))
            ticket.SetFactor("multiplier", Param("mult"));
    }
}

/// <summary>
/// Scar Tissue: the ratchet (design/10 B). Every bust adds stacks scaled by the busted ticket's
/// stake fraction at placement; the FIRST ticket placed each round carries the stacks (its
/// product gets the "scar" factor) and burns them when it realizes value.
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
            ticket.SetFactor("scar", 1.0 + Stacks / 100.0);
        }
    }

    public override void OnBust(Ticket ticket) => Stacks += ticket.ScarStacksIfBust;

    public override void OnTicketRealized(Ticket ticket, Action<double> grantComps)
    {
        if (ticket.ScarCarrier)
        {
            Stacks = 0;
            ticket.ScarCarrier = false;
        }
    }

    public override void AddStats(List<EffectStat> stats)
        => stats.Add(new EffectStat(Def.Id, "SCAR", Stacks));
}

/// <summary>Totem of Undying: one charge; consumed by Run.Settle on a non-final shortfall.</summary>
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

/// <summary>Chalk Eater: permanent ratchet — every leg that FINALLY grades Won (post-window,
/// whistle rescues included, voids excluded) at offered odds ≤ band adds ppPerLeg, forever.
/// The factor applies to every ticket at lock.</summary>
internal sealed class ChalkEaterBehavior : RelicBehavior
{
    public double Stacks { get; private set; }

    public ChalkEaterBehavior(RelicDefinition def) : base(def) { }

    public override void OnLegResolved(Ticket ticket, Leg leg, LegGrade finalGrade)
    {
        if (finalGrade == LegGrade.Won && leg.OfferedOdds <= Param("maxOdds"))
            Stacks += Param("ppPerLeg");
    }

    public override void OnLock(Run run)
    {
        if (Stacks <= 0) return;
        foreach (Ticket t in run.Tickets) t.SetFactor("chalk", 1.0 + Stacks / 100.0);
    }

    public override void AddStats(List<EffectStat> stats)
        => stats.Add(new EffectStat(Def.Id, "CHALK", Stacks));
}

/// <summary>Longshot Larry's Photo: a lock-time flag factor — the ticket holds ≥1 ACTIVE leg at
/// offered odds ≥ band → ×mult. Prices into cash-outs (design/02 law). The ONLY post-lock
/// change is the designed toggle: a void that removes the last qualifying leg drops the factor.</summary>
internal sealed class LongshotPhotoBehavior : RelicBehavior
{
    public LongshotPhotoBehavior(RelicDefinition def) : base(def) { }

    public override void OnLock(Run run)
    {
        foreach (Ticket t in run.Tickets) Refresh(t);
    }

    public void Refresh(Ticket ticket)
    {
        bool qualifies = false;
        foreach (Leg l in ticket.ActiveLegs)
            if (l.OfferedOdds >= Param("minOdds")) { qualifies = true; break; }

        if (qualifies) ticket.SetFactor("photo", Param("mult"));
        else ticket.RemoveFactor("photo");
    }
}

/// <summary>Iron Hands: +pp per ticket that WINS at full ride; ANY cash-out resets to zero.</summary>
internal sealed class IronHandsBehavior : RelicBehavior
{
    public int Wins { get; private set; }

    public IronHandsBehavior(RelicDefinition def) : base(def) { }

    public override void OnTicketRealized(Ticket ticket, Action<double> grantComps)
    {
        if (ticket.State == TicketState.Won) Wins++;
        else if (ticket.State == TicketState.CashedOut) Wins = 0;
    }

    public override void OnLock(Run run)
    {
        if (Wins <= 0) return;
        double stacks = Wins * Param("ppPerWin");
        foreach (Ticket t in run.Tickets) t.SetFactor("iron", 1.0 + stacks / 100.0);
    }

    public override void AddStats(List<EffectStat> stats)
        => stats.Add(new EffectStat(Def.Id, "IRON", Wins * Param("ppPerWin")));
}

/// <summary>Golden Parachute: cash-outs credit ×scale (the book waives its margin; the hard
/// ceiling lives in the catalog params — never above the margin reciprocal).</summary>
internal sealed class GoldenParachuteBehavior : RelicBehavior
{
    public GoldenParachuteBehavior(RelicDefinition def) : base(def) { }

    public override double CashOutQuoteScale => Param("scale");
}

/// <summary>The Rake's Rebate: +rate interest on comps held at each shop OPEN (never on a
/// Manager redeal — the EnterShop/DealOffers split, PLAN.md rev 5 §8).</summary>
internal sealed class RakesRebateBehavior : RelicBehavior
{
    public RakesRebateBehavior(RelicDefinition def) : base(def) { }

    public override void OnShopEnter(Run run) => run.ApplyCompsInterest(Param("rate"));
}

/// <summary>Whale Card: payout ×(1 + perComp × comps held), snapshotted at LOCK (after the
/// round's comps accrual commit) — every ticket in the round shares one factor.</summary>
internal sealed class WhaleCardBehavior : RelicBehavior
{
    public WhaleCardBehavior(RelicDefinition def) : base(def) { }

    public override void OnLock(Run run)
    {
        double factor = 1.0 + Param("perComp") * run.Comps;
        if (factor <= 1.0) return;
        foreach (Ticket t in run.Tickets) t.SetFactor("whale", factor);
    }
}

/// <summary>Bad Beat Jar: +pp permanent per round with ≥1 ticket placed and EVERY ticket Lost
/// (a cash-out disqualifies; refunded Free Bet losses still count — refunds are cash flow).</summary>
internal sealed class BadBeatJarBehavior : RelicBehavior
{
    public double Stacks { get; private set; }

    public BadBeatJarBehavior(RelicDefinition def) : base(def) { }

    public override void OnRoundResolved(RoundResolution res)
    {
        if (res.TicketsPlaced >= 1 && res.TicketsLost == res.TicketsPlaced)
            Stacks += Param("ppPerRound");
    }

    public override void OnLock(Run run)
    {
        if (Stacks <= 0) return;
        foreach (Ticket t in run.Tickets) t.SetFactor("jar", 1.0 + Stacks / 100.0);
    }

    public override void AddStats(List<EffectStat> stats)
        => stats.Add(new EffectStat(Def.Id, "JAR", Stacks));
}

/// <summary>House Key: all payouts ×mult — and every UNPAID payment reads ×paymentFactor
/// through the getters while owned (never a mutation; selling just drops the factor).</summary>
internal sealed class HouseKeyBehavior : RelicBehavior
{
    public HouseKeyBehavior(RelicDefinition def) : base(def) { }

    public override void OnLock(Run run)
    {
        foreach (Ticket t in run.Tickets) t.SetFactor("housekey", Param("mult"));
    }

    public override double PaymentFactor => Param("paymentFactor");
}

/// <summary>The System: +pp per consecutive PROFITABLE round (pre-payment PnL &gt; 0);
/// PnL ≤ 0 — including zero-bet rounds — resets the streak.</summary>
internal sealed class TheSystemBehavior : RelicBehavior
{
    public int Streak { get; private set; }

    public TheSystemBehavior(RelicDefinition def) : base(def) { }

    public override void OnRoundResolved(RoundResolution res)
    {
        if (res.PrePaymentPnl > 0) Streak++;
        else Streak = 0;
    }

    public override void OnLock(Run run)
    {
        if (Streak <= 0) return;
        double stacks = Streak * Param("ppPerRound");
        foreach (Ticket t in run.Tickets) t.SetFactor("system", 1.0 + stacks / 100.0);
    }

    public override void AddStats(List<EffectStat> stats)
        => stats.Add(new EffectStat(Def.Id, "STREAK", Streak));
}

/// <summary>Comp'd Suite: a winning ticket with ≥ minLegs ACTIVE legs pays comps instantly.</summary>
internal sealed class CompdSuiteBehavior : RelicBehavior
{
    public CompdSuiteBehavior(RelicDefinition def) : base(def) { }

    public override void OnTicketRealized(Ticket ticket, Action<double> grantComps)
    {
        if (ticket.State != TicketState.Won) return;
        int active = 0;
        foreach (Leg l in ticket.Legs) if (!l.IsVoided) active++;
        if (active >= (int)Param("minLegs")) grantComps(Param("comps"));
    }
}

/// <summary>Unopened Bobblehead: does nothing. Sells back at its resale override (3× list) —
/// the override lives in GetResaleValue via the catalog's resaleMult param.</summary>
internal sealed class NoOpBehavior : RelicBehavior
{
    public NoOpBehavior(RelicDefinition def) : base(def) { }
}

/// <summary>The Collection: payout ×(1 + perComp × total resale comps of owned PASSIVES,
/// itself included; consumables excluded), snapshotted at LOCK.</summary>
internal sealed class TheCollectionBehavior : RelicBehavior
{
    public TheCollectionBehavior(RelicDefinition def) : base(def) { }

    public override void OnLock(Run run)
    {
        double resale = 0;
        foreach (RelicDefinition d in run.OwnedRelics) resale += run.GetResaleValue(d);
        double factor = 1.0 + Param("perComp") * resale;
        if (factor <= 1.0) return;
        foreach (Ticket t in run.Tickets) t.SetFactor("collection", factor);
    }
}
