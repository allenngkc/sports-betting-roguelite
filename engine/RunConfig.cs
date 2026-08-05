namespace SBR.Engine;

/// <summary>Tuning defaults. The /sim harness exists to move these numbers — the economy rework's
/// gate campaign (PLAN.md 2026-07-13, gates G1–G6) owns the payment curve.</summary>
public sealed class RunConfig
{
    public double StartingBank { get; set; } = 350;

    /// <summary>The debt-payment schedule (economy rework, design/10): DEDUCTED from the bank at
    /// each settle — miss a payment and the run is over (unless the Totem fires). Two-phase convex
    /// (the campaign's discovered shape): gentle build window, then the cliff. Under the COMPS
    /// currency (design/10 F) the cash bank starts at only ~2–3 payments, so BETTING IS MANDATORY
    /// from round 1 — idling is death (Allen's round-1 ruling). The grid re-tunes these.</summary>
    public double[] Payments { get; set; } = { 60, 70, 85, 105, 155, 375, 710, 1350 };

    /// <summary>COMPS (design/10 F): the second currency, earned per dollar STAKED — the book's
    /// loyalty program, and items are bought with it. Chasing comps is −EV cash, exactly like a
    /// real VIP program; the shop no longer competes with payments for bankroll.</summary>
    public double CompsPerDollarStaked { get; set; } = 0.12;

    public double Overround { get; set; } = 0.05;
    public double CashOutMargin { get; set; } = 0.08;
    public double MinStake { get; set; } = 10;

    /// <summary>Cap on a single ticket's stake as a fraction of the current bank. 1.0 = uncapped (all-in
    /// allowed) — lifted 2026-07-08 after playtest #1. Kept as a dial for /sim experiments. Boundary
    /// inclusive: a stake exactly at the cap is allowed.</summary>
    public double MaxStakeFraction { get; set; } = 1.0;

    public int MaxTicketsPerRound { get; set; } = 3;
    public int MatchupsPerSlate { get; set; } = 6;
    /// <summary>Max legs on one ticket. Allen, 2026-08-02: lowered 6 → 4. The working margin
    /// renders a leg as the kit's two-line MarginLeg, and at 6 legs that overflowed the fixed
    /// 324×530 panel — capping the slip closes the overflow at the source rather than shrinking
    /// type below the fact floor or adding a scroll the reference margin does not have.
    /// Bot-neutral by construction: skilled already caps at 3 (4 with Comp'd Suite) and random at
    /// min(4, MaxLegs), so no strategy loses a shape it was using.
    /// The container's correctness must NOT depend on this dial's value (S25's general clause) —
    /// the margin-fits invariant is guarded by a test that reads MaxLegs rather than assuming 4.</summary>
    public int MaxLegs { get; set; } = 4;
    public int PriorGames { get; set; } = 9;
    public double MinTrueProb { get; set; } = 0.25;
    public double MaxTrueProb { get; set; } = 0.75;

    // Soccer-market foundation. Rates are latent per-team match rates; the signal noise is the
    // public, deliberately imperfect detail-page information. The grid caps keep the exact
    // pricing distribution finite and give the sampler a constant draw count.
    public double BaseGoalRate { get; set; } = 1.35;
    public double BaseCornerRate { get; set; } = 5.0;
    public double BaseCardRate { get; set; } = 2.2;
    public double GoalTempoSpread { get; set; } = 0.15;
    public double CornerTempoSpread { get; set; } = 0.20;
    public double DisciplineSpread { get; set; } = 0.20;
    public double SignalNoise { get; set; } = 0.35;
    public int MaxGoalsGrid { get; set; } = 8;
    public int MaxCornerGrid { get; set; } = 20;
    public int MaxCardGrid { get; set; } = 12;
    public double[] GoalLines { get; set; } = { 1.5, 2.5, 3.5 };
    public double[] CornerLines { get; set; } = { 8.5, 9.5, 10.5 };
    public double[] CardLines { get; set; } = { 3.5, 4.5, 5.5 };

    // Anytime-scorer roster dials. There are deliberately no goalkeeper offers: these are the
    // listed outfield players whose normalized weights feed both price and attribution.
    public int PlayersPerTeam { get; set; } = 7;
    public double ForwardScoringWeight { get; set; } = 3.0;
    public double MidfielderScoringWeight { get; set; } = 1.5;
    public double DefenderScoringWeight { get; set; } = 0.5;

    /// <summary>Max passive relics owned at once. The rework catalog has 3 passives; slots stay
    /// roomier for the committed item-growth direction (design/10 B2).</summary>
    public int RelicSlots { get; set; } = 5;

    /// <summary>Max consumables held at once (separate pool from relics — playtest #1 split).
    /// 3 since playtest #8: with one shop offer per visit, a third slot lets a player bank saves
    /// for the cliff — the G3 band's buy-back for the tighter offer draw.</summary>
    public int ConsumableSlots { get; set; } = 3;

    /// <summary>Passive offers dealt per shop visit (charm expansion: the DEALT HAND — a random
    /// draw from the unowned pool replaces show-everything at catalog size 15). Tuned to 4:
    /// at 3 the engine-finding variance starved too many builds; at 5 the win rate rode the
    /// band ceiling. A specific item shows ~27%/visit at 4-of-15 — scarcity stays real.</summary>
    public int PassiveOfferCount { get; set; } = 4;

    /// <summary>Consumable offers dealt per shop visit. History: 2 → 1 at playtest #8 (a 2-item
    /// pool with 2 slots guaranteed the strongest item every shop) → 3 for the charm expansion:
    /// the 7-item pool does the dilution (a specific tool shows ~43% of visits), and the wider
    /// hand is where the toolbox decisions live.</summary>
    public int ConsumableOfferCount { get; set; } = 3;

    /// <summary>Sell-back fraction of list price, both pools (design/10, Allen 2026-07-12).</summary>
    public double SellBackFraction { get; set; } = 0.5;

    /// <summary>Consecutive net-losing rounds before the bookie texts a free consumable
    /// (the gift/pity channel, design/10 D), and the minimum rounds between gifts.</summary>
    public int GiftAfterLosingRounds { get; set; } = 2;
    public int GiftCooldownRounds { get; set; } = 2;

    /// <summary>Totem of Undying: the DEFERRED payment is added to the NEXT payment at
    /// payment × (1 + this rate). Playtest #8 made the totem leave the bank untouched; the G3
    /// campaign showed the juice barely moves win rate (the offer draw is the binding knob), so
    /// the ratified 0.5 stands.</summary>
    public double TotemJuiceRate { get; set; } = 0.5;

    /// <summary>Pacing dials for the drama generator (design/04); flows through Run into every SweatSession.</summary>
    public DramaConfig Drama { get; set; } = new DramaConfig();

    public int Rounds => Payments.Length;
}
