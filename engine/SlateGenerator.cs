using System;
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
        => Generate(round, rng, config, null);

    /// <summary>The Run supplies its hub so roster generation can use isolated match streams.
    /// The legacy stream-only overload remains for slate draw-order tests and produces empty
    /// rosters because it has no run seed from which to derive them.</summary>
    public static Slate Generate(int round, RngHub hub, RunConfig config)
        => Generate(round, hub.Slate, config, hub);

    private static Slate Generate(int round, Pcg32 rng, RunConfig config, RngHub? hub)
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
            if (hub != null)
            {
                Pcg32 rosterRng = hub.DeriveMatch(round, i, "roster");
                // The anytime-scorer board is one flat list spanning both rosters (M-01,
                // MatchModel.BuildOffers), so this set must span both MakeRoster calls below —
                // unique names within a single roster is not enough.
                var usedNames = new HashSet<string>();
                // Scoring-weight jitter draws from its OWN derived stream, never the roster stream.
                // Sharing would shift every subsequent name draw, so before/after slates would
                // differ in names as well as weights and no comparison could isolate the change.
                // One stream per concern is also what makes the jitter re-tunable without moving
                // anything else on the board.
                Pcg32 weightRng = hub.DeriveMatch(round, i, "weights");
                home = WithRoster(home, MakeRoster(rosterRng, config, usedNames, weightRng));
                away = WithRoster(away, MakeRoster(rosterRng, config, usedNames, weightRng));
            }

            // The nine latent/signal draws are deliberately sequential and independent of the
            // Outcomes stream: three shared match tempos, then home/away GF/COR/CRD signals.
            double goalTempo = 1.0 - config.GoalTempoSpread
                + 2.0 * config.GoalTempoSpread * rng.NextDouble();
            double cornerTempo = 1.0 - config.CornerTempoSpread
                + 2.0 * config.CornerTempoSpread * rng.NextDouble();
            double disciplineTempo = 1.0 - config.DisciplineSpread
                + 2.0 * config.DisciplineSpread * rng.NextDouble();
            MatchLatents latents = MatchModel.LatentsFor(p, goalTempo, cornerTempo, disciplineTempo, config);
            TeamStats homeStats = DisplayStats(latents.HomeGoalRate, latents.HomeCornerRate,
                latents.HomeCardRate, rng, config);
            TeamStats awayStats = DisplayStats(latents.AwayGoalRate, latents.AwayCornerRate,
                latents.AwayCardRate, rng, config);

            (double homeOdds, double awayOdds) = OddsMath.OverroundOdds(p, config.Overround);
            var matchup = new Matchup(i, home, away, p, homeOdds, awayOdds, latents,
                homeStats, awayStats, System.Array.Empty<MarketOffer>(), config);
            matchups.Add(matchup);
            matchup.SetMarkets(MatchModel.BuildOffers(matchup, config));
        }

        return new Slate(round, matchups);
    }

    private static TeamStats DisplayStats(double goalRate, double cornerRate, double cardRate,
        Pcg32 rng, RunConfig config)
    {
        double sigma(double rate) => config.SignalNoise * rate / Math.Sqrt(config.PriorGames);
        double noisy(double rate) => rate + (2.0 * rng.NextDouble() - 1.0) * sigma(rate);
        return new TeamStats(noisy(goalRate), noisy(cornerRate), noisy(cardRate));
    }

    private static Team MakeTeam(int nounIndex, Pcg32 rng, RunConfig config, double strength)
    {
        string name = $"{Cities[rng.NextInt(0, Cities.Length)]} {Nouns[nounIndex]}";
        int wins = Binomial(rng, config.PriorGames, strength);
        return new Team(name, wins, config.PriorGames - wins);
    }

    private static Team WithRoster(Team team, IReadOnlyList<Player> players)
        => new Team(team.Name, team.Wins, team.Losses, players);

    private static readonly string[] PlayerFirst =
        { "Biff", "Chaz", "Marty", "Deke", "Rico", "Darryl", "Gus", "Ned", "Troy", "Lance", "Skip", "Vince" };
    private static readonly string[] PlayerLast =
        { "Ledger", "Cinder", "Muffin", "Pavement", "Coupon", "Wobble", "Gasket", "Pylon", "Ketchup", "Lanyard", "Racket", "Stapler" };

    private static IReadOnlyList<Player> MakeRoster(Pcg32 rng, RunConfig config,
        HashSet<string> usedNames, Pcg32 weightRng)
    {
        if (config.PlayersPerTeam <= 0) throw new InvalidOperationException("PlayersPerTeam must be positive");
        // Both rosters share usedNames (see call site), so the exhaustion floor is the combined
        // board, not one roster: PlayersPerTeam * 2 must fit the 144 first/last combinations.
        if (config.PlayersPerTeam * 2 > PlayerFirst.Length * PlayerLast.Length)
            throw new InvalidOperationException(
                "PlayersPerTeam is too large: both rosters together must fit within the 144 available name combinations");
        var players = new List<Player>(config.PlayersPerTeam);
        for (int i = 0; i < config.PlayersPerTeam; i++)
        {
            // A stable role mix puts attackers up front while still listing midfielders and defenders.
            PlayerRole role = i % 7 < 3 ? PlayerRole.FW : i % 7 < 5 ? PlayerRole.MF : PlayerRole.DF;
            double roleWeight = role == PlayerRole.FW ? config.ForwardScoringWeight
                : role == PlayerRole.MF ? config.MidfielderScoringWeight : config.DefenderScoringWeight;
            // Per-player scoring variety. Weight was PURELY role-derived, so every forward on a
            // team priced identically and a 14-row scorer board carried at most six distinct
            // prices (2 teams × 3 roles) — on that tab the player was choosing a name, not a
            // price. The jitter is multiplicative and symmetric about the role weight, so the
            // ROLE ORDER still holds on average (a forward is still the striker) while individual
            // players separate. Bounded by ScoringWeightJitter and clamped strictly positive: a
            // zero or negative weight would make a listed player unscoreable while still being
            // offered, which is worse than the flatness it replaces.
            double jitter = config.ScoringWeightJitter;
            double weight = roleWeight;
            if (jitter > 0.0)
                weight = Math.Max(roleWeight * 0.05,
                    roleWeight * (1.0 + jitter * (2.0 * weightRng.NextDouble() - 1.0)));
            // Rejection-resample on the same stream: a name collision (within or across
            // rosters) is re-rolled until unique, so replay stays byte-identical and role/weight
            // (index-derived above) never shift.
            string name;
            do
            {
                name = $"{PlayerFirst[rng.NextInt(0, PlayerFirst.Length)]} {PlayerLast[rng.NextInt(0, PlayerLast.Length)]}";
            } while (!usedNames.Add(name));
            players.Add(new Player(name, role, weight));
        }
        return players;
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
