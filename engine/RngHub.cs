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

    /// <summary>
    /// Derived one-shot stream (charm expansion, PLAN.md rev 5 §6): fully keyed by run seed,
    /// round, ticket, leg, action, and use ordinal, so no two derivations collide and NONE of
    /// the named streams above is ever consumed — consumable timing can never perturb the run.
    /// Whistle rolls key (round, ticketId, legIndex, "whistle", 0); Manager redeals key
    /// (round, "", -1, "redeal", useOrdinal).
    /// </summary>
    public Pcg32 Derive(int round, string ticketId, int legIndex, string action, int ordinal)
        => new Pcg32(Fnv1a64(RunSeed), Fnv1a64($"{round}#{ticketId}#{legIndex}#{action}#{ordinal}"));

    /// <summary>Match-scoped universe data (rosters and scorer attribution) lives on its own
    /// stream. It must never consume either the sequential slate or outcomes streams.</summary>
    public Pcg32 DeriveMatch(int round, int matchupIndex, string purpose)
        => new Pcg32(Fnv1a64(RunSeed), Fnv1a64($"{round}#M{matchupIndex}#{purpose}"));
}
