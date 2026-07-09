namespace SBR.Engine;

/// <summary>
/// Named RNG streams per design/05: outcomes, drama, slate, and shop draw from
/// independent generators so that, e.g., which relics the shop offers can never
/// perturb match results for the same run seed.
/// </summary>
public sealed class RngHub
{
    public string RunSeed { get; }

    /// <summary>Match results.</summary>
    public Pcg32 Outcomes { get; }

    /// <summary>Drama-generator event sequences (Week 2).</summary>
    public Pcg32 Drama { get; }

    /// <summary>Slate, team, and odds generation.</summary>
    public Pcg32 Slate { get; }

    /// <summary>Shop offers (Week 3).</summary>
    public Pcg32 Shop { get; }

    /// <summary>Relic effects: tout-sheet intel and lucky-charm second-chance rolls (Week 3).</summary>
    public Pcg32 Relics { get; }

    public RngHub(string runSeed)
    {
        RunSeed = runSeed;
        ulong seed = Fnv1a64(runSeed);
        Outcomes = new Pcg32(seed, Fnv1a64("outcomes"));
        Drama = new Pcg32(seed, Fnv1a64("drama"));
        Slate = new Pcg32(seed, Fnv1a64("slate"));
        Shop = new Pcg32(seed, Fnv1a64("shop"));
        Relics = new Pcg32(seed, Fnv1a64("relics"));
    }

    public static ulong Fnv1a64(string s)
    {
        ulong hash = 14695981039346656037UL;
        foreach (char c in s)
        {
            hash ^= c;
            hash *= 1099511628211UL;
        }
        return hash;
    }
}
