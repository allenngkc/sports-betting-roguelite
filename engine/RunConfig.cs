namespace SBR.Engine;

/// <summary>Tuning defaults from PRD §8. The /sim harness exists to move these numbers.</summary>
public sealed class RunConfig
{
    public double StartingBank { get; set; } = 500;
    public double[] Targets { get; set; } = { 400, 460, 520, 650, 800, 1000, 1500, 2800 };
    public double Overround { get; set; } = 0.05;
    public double CashOutMargin { get; set; } = 0.08;
    public double MinStake { get; set; } = 10;

    /// <summary>Debt-as-HP (DECISIONS.md 2026-07-09): missing a settle with no debt outstanding makes
    /// the bookie float you — the bank is topped up to the target and the shortfall is booked as debt
    /// at shortfall × (1 + this rate). Interest is baked once at borrow time; the balance never
    /// compounds. A calibration dial.</summary>
    public double DebtJuiceRate { get; set; } = 0.5;

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

    /// <summary>Max relics a run may own at once (PRD F8).</summary>
    public int RelicSlots { get; set; } = 5;

    /// <summary>How many distinct relics the between-rounds shop offers (PRD F8).</summary>
    public int ShopOfferCount { get; set; } = 3;

    /// <summary>Pacing dials for the drama generator (design/04); flows through Run into every SweatSession.</summary>
    public DramaConfig Drama { get; set; } = new DramaConfig();

    public int Rounds => Targets.Length;
}
