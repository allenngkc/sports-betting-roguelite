namespace SBR.Engine;

/// <summary>Tuning defaults. The /sim harness exists to move these numbers — the economy rework's
/// gate campaign (PLAN.md 2026-07-13, gates G1–G6) owns the payment curve.</summary>
public sealed class RunConfig
{
    public double StartingBank { get; set; } = 750;

    /// <summary>The debt-payment schedule (economy rework, design/10): DEDUCTED from the bank at
    /// each settle — miss a payment and the run is over (unless the Totem fires). Two-phase convex
    /// (the campaign's discovered shape, 2026-07-13): gentle ×1.2 through R4 — the build window —
    /// then the ×1.9 cliff. Grid candidate 750/90/1.2/1.9; the gate report carries the evidence.</summary>
    public double[] Payments { get; set; } = { 90, 110, 130, 155, 295, 560, 1065, 2025 };

    public double Overround { get; set; } = 0.05;
    public double CashOutMargin { get; set; } = 0.08;
    public double MinStake { get; set; } = 10;

    /// <summary>Cap on a single ticket's stake as a fraction of the current bank. 1.0 = uncapped (all-in
    /// allowed) — lifted 2026-07-08 after playtest #1. Kept as a dial for /sim experiments. Boundary
    /// inclusive: a stake exactly at the cap is allowed.</summary>
    public double MaxStakeFraction { get; set; } = 1.0;

    public int MaxTicketsPerRound { get; set; } = 3;
    public int MatchupsPerSlate { get; set; } = 6;
    public int MaxLegs { get; set; } = 6;
    public int PriorGames { get; set; } = 9;
    public double MinTrueProb { get; set; } = 0.25;
    public double MaxTrueProb { get; set; } = 0.75;

    /// <summary>Max passive relics owned at once. The rework catalog has 3 passives; slots stay
    /// roomier for the committed item-growth direction (design/10 B2).</summary>
    public int RelicSlots { get; set; } = 5;

    /// <summary>Max consumables held at once (separate pool from relics — playtest #1 split).</summary>
    public int ConsumableSlots { get; set; } = 2;

    /// <summary>How many consumable offers the shop shows per visit (drawn from the catalog).</summary>
    public int ConsumableOfferCount { get; set; } = 2;

    /// <summary>Sell-back fraction of list price, both pools (design/10, Allen 2026-07-12).</summary>
    public double SellBackFraction { get; set; } = 0.5;

    /// <summary>Consecutive net-losing rounds before the bookie texts a free consumable
    /// (the gift/pity channel, design/10 D), and the minimum rounds between gifts.</summary>
    public int GiftAfterLosingRounds { get; set; } = 2;
    public int GiftCooldownRounds { get; set; } = 2;

    /// <summary>Totem of Undying: the covered shortfall is added to the NEXT payment at this
    /// multiple (the old float juice, itemized — design/10 B).</summary>
    public double TotemJuiceRate { get; set; } = 0.5;

    /// <summary>Pacing dials for the drama generator (design/04); flows through Run into every SweatSession.</summary>
    public DramaConfig Drama { get; set; } = new DramaConfig();

    public int Rounds => Payments.Length;
}
