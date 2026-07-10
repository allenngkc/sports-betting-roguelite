using System;
using System.Collections.Generic;

namespace SBR.Engine;

/// <summary>How a leg's revealed loss resolves once the relics have their say.</summary>
public enum LossResolution
{
    /// <summary>No relic intervened: the ticket busts.</summary>
    Bust,

    /// <summary>Mulligan voided the leg; the sweat continues on the remaining legs.</summary>
    Mulligan,

    /// <summary>Lucky charm's second chance fired on the final leg; this ticket grades it Won.</summary>
    LuckyFlip,
}

/// <summary>The money seams a bust-triggered relic reaches for, so the engine keeps Bank/piggy state.</summary>
public sealed class BustContext
{
    public Ticket Ticket { get; }
    public Action<double> CreditBank { get; }
    public Action SmashPiggy { get; }

    public BustContext(Ticket ticket, Action<double> creditBank, Action smashPiggy)
    {
        Ticket = ticket;
        CreditBank = creditBank;
        SmashPiggy = smashPiggy;
    }
}

/// <summary>
/// Owned relics and their behaviors, held strictly in acquisition (purchase) order (PRD F8). The
/// order is load-bearing: compose-time effects and bust-time effects resolve in it, so promo-then-boost
/// differs from boost-then-promo. Behaviors are keyed on <see cref="RelicDefinition.Op"/>.
/// </summary>
public sealed class EffectEngine
{
    private readonly List<RelicDefinition> _owned = new List<RelicDefinition>();
    private readonly List<RelicBehavior> _behaviors = new List<RelicBehavior>();

    public IReadOnlyList<RelicDefinition> Owned => _owned;

    public void Add(RelicDefinition def)
    {
        RelicBehavior behavior = RelicBehavior.Create(def);
        behavior.ResetRound(); // a just-bought relic starts the next round charged
        _owned.Add(def);
        _behaviors.Add(behavior);
    }

    public bool Owns(string id)
    {
        foreach (RelicDefinition d in _owned)
            if (d.Id == id) return true;
        return false;
    }

    /// <summary>Recharge every per-round charge (called when a new round begins).</summary>
    public void ResetRoundCharges()
    {
        foreach (RelicBehavior b in _behaviors) b.ResetRound();
    }

    // ---- compose-time (PlaceTicket) ----

    public void ApplyCompose(IReadOnlyList<Leg> legs, bool isFirstTicketThisRound)
    {
        foreach (RelicBehavior b in _behaviors) b.Compose(legs, isFirstTicketThisRound);
    }

    /// <summary>Placement effects (high_roller): run after compose, with the bank pre-deduction.</summary>
    public void ApplyTicketPlaced(Ticket ticket, double stake, double bankBeforeDeduction)
    {
        foreach (RelicBehavior b in _behaviors) b.OnTicketPlaced(ticket, stake, bankBeforeDeduction);
    }

    // ---- early_payout ----

    public bool HasEarlyPayout => Find("PayPerGreenLeg") != null;

    public double EarlyPayoutFraction
    {
        get
        {
            var b = Find("PayPerGreenLeg");
            return b == null ? 0.0 : b.Param("fractionOfStake");
        }
    }

    // ---- lucky_charm (baked second-chance roll) ----

    public bool HasLuckyCharm => Find("SecondChanceFinalLeg") != null;

    /// <summary>Bakes the final-leg second-chance roll at lock. Draws the Relics stream only when owned.</summary>
    public bool BakeLuckyRoll(double finalLegTrueProb, Pcg32 relics)
    {
        var b = (SecondChanceFinalLegBehavior?)Find("SecondChanceFinalLeg");
        return b != null && b.BakeRoll(finalLegTrueProb, relics);
    }

    // ---- piggy_bank (accrual) ----

    public bool HasPiggyBank => Find("VigRebate") != null;

    public double PiggyRebateMult
    {
        get
        {
            var b = Find("VigRebate");
            return b == null ? 0.0 : b.Param("mult");
        }
    }

    // ---- loss resolution (SweatSession), acquisition order ----

    public LossResolution ResolveLoss(bool isFinalLeg, bool luckyBakedSuccess, int legCount)
    {
        foreach (RelicBehavior b in _behaviors)
        {
            if (b.Op == "SecondChanceFinalLeg" && isFinalLeg && luckyBakedSuccess)
                return LossResolution.LuckyFlip;
            if (b.Op == "VoidDeadLeg" && legCount >= 2 && ((VoidDeadLegBehavior)b).TryUse())
                return LossResolution.Mulligan;
        }
        return LossResolution.Bust;
    }

    // ---- bust chain (insurance + piggy), acquisition order ----

    public void OnBust(BustContext ctx)
    {
        foreach (RelicBehavior b in _behaviors) b.OnBust(ctx);
    }

    private RelicBehavior? Find(string op)
    {
        foreach (RelicBehavior b in _behaviors)
            if (b.Op == op) return b;
        return null;
    }
}

/// <summary>A parameterized relic behavior, constructed from its <see cref="RelicDefinition"/>.</summary>
internal abstract class RelicBehavior
{
    protected readonly RelicDefinition Def;
    private int _chargesLeft;

    protected RelicBehavior(RelicDefinition def) => Def = def;

    public string Op => Def.Op;

    public double Param(string key) => Def.Params.TryGetValue(key, out double v) ? v : 0.0;

    protected int MaxCharges => (int)Param("usesPerRound");

    public virtual void ResetRound() => _chargesLeft = MaxCharges;

    protected bool TryConsumeCharge()
    {
        if (_chargesLeft <= 0) return false;
        _chargesLeft--;
        return true;
    }

    // Hook sites (default no-op).
    public virtual void Compose(IReadOnlyList<Leg> legs, bool isFirstTicketThisRound) { }
    public virtual void OnTicketPlaced(Ticket ticket, double stake, double bankBeforeDeduction) { }
    public virtual void OnBust(BustContext ctx) { }

    public static RelicBehavior Create(RelicDefinition def)
    {
        switch (def.Op)
        {
            case "BoostLegOdds": return new BoostLegOddsBehavior(def);
            case "FairOddsFirstTicket": return new FairOddsFirstTicketBehavior(def);
            case "AllInPayoutBonus": return new AllInPayoutBonusBehavior(def);
            case "RefundBustedStake": return new RefundBustedStakeBehavior(def);
            case "VoidDeadLeg": return new VoidDeadLegBehavior(def);
            case "SecondChanceFinalLeg": return new SecondChanceFinalLegBehavior(def);
            case "PayPerGreenLeg": return new PayPerGreenLegBehavior(def);
            case "VigRebate": return new VigRebateBehavior(def);
            default: throw new ArgumentException($"Unknown relic op '{def.Op}'");
        }
    }
}

/// <summary>boosted_odds: multiply a fixed leg's offered odds by a factor at compose time.</summary>
internal sealed class BoostLegOddsBehavior : RelicBehavior
{
    public BoostLegOddsBehavior(RelicDefinition def) : base(def) { }

    public override void Compose(IReadOnlyList<Leg> legs, bool isFirstTicketThisRound)
    {
        int legIndex = (int)Param("legIndex");
        if (legIndex >= 0 && legIndex < legs.Count)
            legs[legIndex].OfferedOdds *= Param("mult");
    }
}

/// <summary>promo_code: the round's first ticket is priced at fair odds (no vig).</summary>
internal sealed class FairOddsFirstTicketBehavior : RelicBehavior
{
    public FairOddsFirstTicketBehavior(RelicDefinition def) : base(def) { }

    public override void Compose(IReadOnlyList<Leg> legs, bool isFirstTicketThisRound)
    {
        if (!isFirstTicketThisRound) return;
        foreach (Leg leg in legs)
            leg.OfferedOdds = OddsMath.FairDecimal(leg.TrueProb);
    }
}

/// <summary>high_roller: staking at least the threshold fraction of the bank boosts the ticket's payout.
/// Redesigned 2026-07-08 alongside lifting the max-stake cap (which made the old cap-doubling effect
/// a no-op); this rewards the uncapped all-in play the cap removal enables.</summary>
internal sealed class AllInPayoutBonusBehavior : RelicBehavior
{
    public AllInPayoutBonusBehavior(RelicDefinition def) : base(def) { }

    public override void OnTicketPlaced(Ticket ticket, double stake, double bankBeforeDeduction)
    {
        if (stake >= Param("thresholdFraction") * bankBeforeDeduction)
            ticket.PayoutMultiplier *= Param("payoutMult");
    }
}

/// <summary>bankroll_insurance: the first bust each round refunds a fraction of its stake.</summary>
internal sealed class RefundBustedStakeBehavior : RelicBehavior
{
    public RefundBustedStakeBehavior(RelicDefinition def) : base(def) { }

    public override void OnBust(BustContext ctx)
    {
        if (TryConsumeCharge())
            ctx.CreditBank(Param("fraction") * ctx.Ticket.Stake);
    }
}

/// <summary>mulligan: once per round, void a dead leg on a multi-leg ticket instead of busting.</summary>
internal sealed class VoidDeadLegBehavior : RelicBehavior
{
    public VoidDeadLegBehavior(RelicDefinition def) : base(def) { }
    public bool TryUse() => TryConsumeCharge();
}

/// <summary>lucky_charm: bake a final-leg second chance worth exactly +boostPp of win probability.</summary>
internal sealed class SecondChanceFinalLegBehavior : RelicBehavior
{
    public SecondChanceFinalLegBehavior(RelicDefinition def) : base(def) { }

    public bool BakeRoll(double finalLegTrueProb, Pcg32 relics)
    {
        // roll < boostPp / (1 - p) lifts a p-prob leg's win chance to p + boostPp exactly.
        double gap = 1.0 - finalLegTrueProb;
        if (gap < 1e-9) return false; // already ~certain: no roll needed, draw nothing
        double roll = relics.NextDouble();
        return roll < Param("boostPp") / gap;
    }
}

/// <summary>early_payout: credit a fraction of stake per green leg, and price it into cash-out.</summary>
internal sealed class PayPerGreenLegBehavior : RelicBehavior
{
    public PayPerGreenLegBehavior(RelicDefinition def) : base(def) { }
}

/// <summary>piggy_bank: accrue a multiple of paid vig at lock; smash into the bank on any bust.</summary>
internal sealed class VigRebateBehavior : RelicBehavior
{
    public VigRebateBehavior(RelicDefinition def) : base(def) { }
    public override void OnBust(BustContext ctx) => ctx.SmashPiggy();
}
