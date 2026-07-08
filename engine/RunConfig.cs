namespace SBR.Engine;

/// <summary>Tuning defaults from PRD §8. The /sim harness exists to move these numbers.</summary>
public sealed class RunConfig
{
    public double StartingBank { get; set; } = 500;
    public double[] Targets { get; set; } = { 800, 1200, 1900, 3000, 4800, 7800, 12500, 20000 };
    public double Overround { get; set; } = 0.05;
    public double CashOutMargin { get; set; } = 0.08;
    public double MinStake { get; set; } = 10;
    public int MaxTicketsPerRound { get; set; } = 3;
    public int MatchupsPerSlate { get; set; } = 6;
    public int MaxLegs { get; set; } = 6;
    public int PriorGames { get; set; } = 9;
    public double MinTrueProb { get; set; } = 0.25;
    public double MaxTrueProb { get; set; } = 0.75;

    public int Rounds => Targets.Length;
}
