using System.Collections.Generic;

namespace SBR.Engine;

public static class SlateGenerator
{
    private static readonly string[] Cities =
    {
        "Atlanta", "San Francisco", "Tuscaloosa", "Moose Jaw", "Denver", "Sheboygan",
        "Fresno", "Waterloo", "Pawtucket", "Duluth", "Reno", "Scranton",
        "Boise", "Tulsa", "Omaha", "Yonkers",
    };

    private static readonly string[] Nouns =
    {
        "Yams", "Startups", "Bricklayers", "Longhaulers", "Mallards", "Spreadsheets",
        "Turnips", "Middlemen", "Regulators", "Plumbers", "Meatballs", "Auditors",
        "Ferrets", "Overheads", "Gravediggers", "Notaries", "Muskrats", "Zambonis",
        "Loopholes", "Refunds",
    };

    public static Slate Generate(int round, Pcg32 rng, RunConfig config)
    {
        // Unique noun per team keeps names distinct within a slate; cities may repeat.
        int[] nounOrder = ShuffledIndices(Nouns.Length, rng);

        var matchups = new List<Matchup>(config.MatchupsPerSlate);
        for (int i = 0; i < config.MatchupsPerSlate; i++)
        {
            double p = config.MinTrueProb + (config.MaxTrueProb - config.MinTrueProb) * rng.NextDouble();

            // Records are the round's only free information: a noisy binomial signal of true strength.
            Team home = MakeTeam(nounOrder[i * 2], rng, config, p);
            Team away = MakeTeam(nounOrder[i * 2 + 1], rng, config, 1.0 - p);

            (double homeOdds, double awayOdds) = OddsMath.OverroundOdds(p, config.Overround);
            matchups.Add(new Matchup(i, home, away, p, homeOdds, awayOdds));
        }

        return new Slate(round, matchups);
    }

    private static Team MakeTeam(int nounIndex, Pcg32 rng, RunConfig config, double strength)
    {
        string name = $"{Cities[rng.NextInt(0, Cities.Length)]} {Nouns[nounIndex]}";
        int wins = Binomial(rng, config.PriorGames, strength);
        return new Team(name, wins, config.PriorGames - wins);
    }

    private static int Binomial(Pcg32 rng, int n, double p)
    {
        int k = 0;
        for (int i = 0; i < n; i++)
            if (rng.NextDouble() < p) k++;
        return k;
    }

    private static int[] ShuffledIndices(int length, Pcg32 rng)
    {
        var indices = new int[length];
        for (int i = 0; i < length; i++) indices[i] = i;
        for (int i = length - 1; i > 0; i--)
        {
            int j = rng.NextInt(0, i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }
        return indices;
    }
}
