using System;
using System.Collections.Generic;

namespace SBR.Engine;

/// <summary>
/// The shared market model. Slate generation owns the public latent inputs and this class owns the
/// exact finite distributions used both to price a selection and to sample its locked stat line.
/// No method here consumes RNG except SampleStatLine, which has a fixed six-draw contract.
/// </summary>
/// <summary>Per-matchup cache of the exact distributions (built once, lazily, via
/// <c>Matchup.Dist</c>). Score lists are normalized exactly as <see cref="MatchModel.EnumerateScores"/>
/// returns them; count arrays keep the RAW truncated-Poisson terms plus their totals so the
/// sampler's <c>roll × total</c> walk is bit-identical to the uncached path.</summary>
internal sealed class MatchDistributions
{
    public IReadOnlyList<MatchModel.ScoreOutcome> HomeWinScores { get; private set; } = null!;
    public IReadOnlyList<MatchModel.ScoreOutcome> AwayWinScores { get; private set; } = null!;
    /// <summary>The level-score class. It was always computable — the grid enumerated it and the
    /// old truncation threw it away.</summary>
    public IReadOnlyList<MatchModel.ScoreOutcome> DrawScores { get; private set; } = null!;
    /// <summary>P(draw) implied by these latents: the draw class's share of the untruncated grid.</summary>
    public double DrawProb { get; private set; }
    public double[] HomeCornerRaw { get; private set; } = null!;
    public double[] AwayCornerRaw { get; private set; } = null!;
    public double[] HomeCardRaw { get; private set; } = null!;
    public double[] AwayCardRaw { get; private set; } = null!;
    public double HomeCornerTotal { get; private set; }
    public double AwayCornerTotal { get; private set; }
    public double HomeCardTotal { get; private set; }
    public double AwayCardTotal { get; private set; }

    public static MatchDistributions Build(MatchLatents l, RunConfig config)
    {
        // One pass over the grid gives all three classes AND their raw masses, so P(draw) is read
        // off the same enumeration that prices and samples — it is never a separately-maintained
        // number that could drift from the distribution it is supposed to describe.
        (IReadOnlyList<MatchModel.ScoreOutcome> home, double homeMass) =
            MatchModel.EnumerateClass(l, MatchResult.Home, config);
        (IReadOnlyList<MatchModel.ScoreOutcome> draw, double drawMass) =
            MatchModel.EnumerateClass(l, MatchResult.Draw, config);
        (IReadOnlyList<MatchModel.ScoreOutcome> away, double awayMass) =
            MatchModel.EnumerateClass(l, MatchResult.Away, config);
        var d = new MatchDistributions
        {
            HomeWinScores = home,
            AwayWinScores = away,
            DrawScores = draw,
            DrawProb = drawMass / (homeMass + drawMass + awayMass),
        };
        (d.HomeCornerRaw, d.HomeCornerTotal) = MatchModel.RawPoisson(l.HomeCornerRate, config.MaxCornerGrid);
        (d.AwayCornerRaw, d.AwayCornerTotal) = MatchModel.RawPoisson(l.AwayCornerRate, config.MaxCornerGrid);
        (d.HomeCardRaw, d.HomeCardTotal) = MatchModel.RawPoisson(l.HomeCardRate, config.MaxCardGrid);
        (d.AwayCardRaw, d.AwayCardTotal) = MatchModel.RawPoisson(l.AwayCardRate, config.MaxCardGrid);
        return d;
    }
}

public static class MatchModel
{
    public readonly struct ScoreOutcome
    {
        public int HomeGoals { get; }
        public int AwayGoals { get; }
        public double Probability { get; }

        public ScoreOutcome(int homeGoals, int awayGoals, double probability)
        {
            HomeGoals = homeGoals;
            AwayGoals = awayGoals;
            Probability = probability;
        }
    }

    /// <summary>Per-surface composable market fields (S22 ruling, Design Director batch 4): the
    /// DS vocabulary split into named parts instead of one packed string. See <see cref="Fields"/>.</summary>
    public readonly struct MarketFields
    {
        /// <summary>DS market name. Verbatim where the design system enumerates it — "MONEYLINE",
        /// "TOTAL GOALS", "BTTS — YES", "ANYTIME SCORER" (<c>components/margin/MarginLeg.d.ts</c>).
        /// "BTTS — NO" and the corner/card categories are pattern-extensions of that enumerated
        /// set, not DS-verbatim entries — see the case comments in <see cref="MatchModel.Fields"/>.</summary>
        public string Market { get; }

        /// <summary>The thing backed: the picked team for Moneyline, the player for AnytimeScorer,
        /// empty for every other market (BTTS/TotalGoals/TotalCorners/TotalCards back the match
        /// itself, not a single subject).</summary>
        public string Subject { get; }

        /// <summary>DS offer-line form (<c>components/form/MarketOffer.d.ts</c>'s <c>line</c>), e.g.
        /// "OVER 2.5 GOALS". Empty for Moneyline, which has no line.</summary>
        public string Line { get; }

        /// <summary>The matchup, "{Away.Name} v {Home.Name}".</summary>
        public string Fixture { get; }

        /// <summary>Scorer role spelled out as a word ("FORWARD"/"MIDFIELDER"/"DEFENDER") — the
        /// ruling struck the bracketed [FW]/[MF]/[DF] tag. Empty for every non-scorer market.</summary>
        public string Role { get; }

        public MarketFields(string market, string subject, string line, string fixture, string role)
        {
            Market = market;
            Subject = subject;
            Line = line;
            Fixture = fixture;
            Role = role;
        }
    }

    public static MatchLatents LatentsFor(double trueHomeProb, double goalTempo,
        double cornerTempo, double disciplineTempo, RunConfig config)
    {
        double homeAttack = 0.6 + 0.8 * trueHomeProb;
        double awayAttack = 0.6 + 0.8 * (1.0 - trueHomeProb);
        double homeDiscipline = 1.15 - 0.3 * trueHomeProb;
        double awayDiscipline = 1.15 - 0.3 * (1.0 - trueHomeProb);
        return new MatchLatents(
            config.BaseGoalRate * goalTempo * homeAttack,
            config.BaseGoalRate * goalTempo * awayAttack,
            config.BaseCornerRate * cornerTempo * homeAttack,
            config.BaseCornerRate * cornerTempo * awayAttack,
            config.BaseCardRate * disciplineTempo * homeDiscipline,
            config.BaseCardRate * disciplineTempo * awayDiscipline);
    }

    public static IReadOnlyList<MarketOffer> BuildOffers(Matchup matchup, RunConfig config)
    {
        // 1X2 — the draw is emitted BETWEEN the two sides, the order a book prints it.
        var offers = new List<MarketOffer>
        {
            Offer(matchup, MarketSelection.Moneyline(Side.Home), config),
            Offer(matchup, MarketSelection.MoneylineDraw(), config),
            Offer(matchup, MarketSelection.Moneyline(Side.Away), config),
        };
        foreach (double line in config.GoalLines)
        {
            offers.Add(Offer(matchup, MarketSelection.TotalGoals(line, true), config));
            offers.Add(Offer(matchup, MarketSelection.TotalGoals(line, false), config));
        }
        offers.Add(Offer(matchup, MarketSelection.BothTeamsToScore(true), config));
        offers.Add(Offer(matchup, MarketSelection.BothTeamsToScore(false), config));
        foreach (double line in config.CornerLines)
        {
            offers.Add(Offer(matchup, MarketSelection.TotalCorners(line, true), config));
            offers.Add(Offer(matchup, MarketSelection.TotalCorners(line, false), config));
        }
        foreach (double line in config.CardLines)
        {
            offers.Add(Offer(matchup, MarketSelection.TotalCards(line, true), config));
            offers.Add(Offer(matchup, MarketSelection.TotalCards(line, false), config));
        }
        // The scorer board is one-way YES-only. PlayerIndex is the stable board index: away
        // roster first, home roster second (see Matchup.PlayerAt).
        int boardSize = matchup.Away.Players.Count + matchup.Home.Players.Count;
        for (int i = 0; i < boardSize; i++)
            offers.Add(Offer(matchup, MarketSelection.AnytimeScorer(i), config));

        // ---- F_0.5.0 V1 ----
        offers.Add(Offer(matchup, MarketSelection.DoubleChance(MarketChoice.HomeOrDraw), config));
        offers.Add(Offer(matchup, MarketSelection.DoubleChance(MarketChoice.AwayOrDraw), config));
        offers.Add(Offer(matchup, MarketSelection.DoubleChance(MarketChoice.HomeOrAway), config));

        // ±1.5 ONLY. ±0.5 duplicates the 1X2 home/away price and ±2.5 crashes Offer() at reachable
        // seeds (favourite side p 0.984 against the 0.95238 ceiling) — both measured, §12.3.
        foreach (double line in config.HandicapLines)
        {
            offers.Add(Offer(matchup, MarketSelection.Handicap(Side.Home, -line), config));
            offers.Add(Offer(matchup, MarketSelection.Handicap(Side.Away, line), config));
            offers.Add(Offer(matchup, MarketSelection.Handicap(Side.Away, -line), config));
            offers.Add(Offer(matchup, MarketSelection.Handicap(Side.Home, line), config));
        }

        foreach (Side team in new[] { Side.Home, Side.Away })
        {
            foreach (double line in config.TeamGoalLines)
            {
                offers.Add(Offer(matchup, MarketSelection.TeamTotalGoals(team, line, true), config));
                offers.Add(Offer(matchup, MarketSelection.TeamTotalGoals(team, line, false), config));
            }
            foreach (double line in config.TeamCornerLines)
            {
                offers.Add(Offer(matchup, MarketSelection.TeamTotalCorners(team, line, true), config));
                offers.Add(Offer(matchup, MarketSelection.TeamTotalCorners(team, line, false), config));
            }
            foreach (double line in config.TeamCardLines)
            {
                offers.Add(Offer(matchup, MarketSelection.TeamTotalCards(team, line, true), config));
                offers.Add(Offer(matchup, MarketSelection.TeamTotalCards(team, line, false), config));
            }
        }

        // Correct score, capped at the RATIFIED probability floor (Allen 2026-08-12). The cap is
        // load-bearing twice over: it keeps the row count readable (12–16), and it keeps the
        // longest price (~47×) inside the tail the economy is already gated on. Without it the
        // grid's own edge cells price past 190× and the vocabulary becomes an economy change.
        for (int h = 0; h <= config.MaxGoalsGrid; h++)
            for (int a = 0; a <= config.MaxGoalsGrid; a++)
            {
                var cs = MarketSelection.CorrectScore(h, a);
                if (TrueProbability(matchup, cs) >= config.CorrectScoreFloor)
                    offers.Add(Offer(matchup, cs, config));
            }

        for (int m = 1; m <= TopMarginBucket; m++)
            offers.Add(Offer(matchup, MarketSelection.WinningMargin(m), config));

        offers.Add(Offer(matchup, MarketSelection.TotalGoalsOddEven(true), config));
        offers.Add(Offer(matchup, MarketSelection.TotalGoalsOddEven(false), config));

        for (int i = 0; i < boardSize; i++)
        {
            var multi = MarketSelection.PlayerMultiScorer(i);
            // A 2+ price for a fourth-choice defender runs to thousands and would dwarf every
            // other payout on the board. Same floor discipline as the score grid.
            if (TrueProbability(matchup, multi) >= config.CorrectScoreFloor)
                offers.Add(Offer(matchup, multi, config));
        }

        return offers;
    }

    private static MarketOffer Offer(Matchup matchup, MarketSelection selection, RunConfig config)
    {
        double p = TrueProbability(matchup, selection);
        double odds = 1.0 / (p * (1.0 + config.Overround));
        // Locked odds are the contract, and OddsMath rejects decimals <= 1 everywhere downstream —
        // fail at pricing time, not when a parlay product first touches the leg.
        if (odds <= 1.0)
            throw new InvalidOperationException(
                $"{selection.Kind} {selection.Line} {selection.Choice} prices at {odds:0.000} <= 1.0 " +
                "(true prob too high for the configured overround — retune lines or grids)");
        return new MarketOffer(selection, p, odds);
    }

    public static MatchStatLine SampleStatLine(Matchup matchup, Pcg32 outcomes)
    {
        MatchDistributions d = matchup.Dist;
        // THE SIX-DRAW CONTRACT IS UNCHANGED BY DRAWS. This was one NextDouble compared against
        // TrueHomeProb; it is still ONE NextDouble, now walked across three cumulative bounds.
        // The draw ruling therefore re-pins the golden seeds on VALUES only — no later draw shifts
        // position in the Outcomes stream, so nothing downstream of a matchup moves.
        MatchResult result = SampleResult(matchup, outcomes.NextDouble());
        ScoreOutcome score = SampleScore(ScoresFor(d, result), outcomes);
        int homeCorners = SampleFromRaw(d.HomeCornerRaw, d.HomeCornerTotal, outcomes);
        int awayCorners = SampleFromRaw(d.AwayCornerRaw, d.AwayCornerTotal, outcomes);
        int homeCards = SampleFromRaw(d.HomeCardRaw, d.HomeCardTotal, outcomes);
        int awayCards = SampleFromRaw(d.AwayCardRaw, d.AwayCardTotal, outcomes);
        return new MatchStatLine(score.HomeGoals, score.AwayGoals, homeCorners, awayCorners, homeCards, awayCards);
    }

    /// <summary>Attributes every already-baked goal from the same categorical weights used by
    /// the anytime price. This consumes only the caller-provided derived match stream.</summary>
    public static void SampleScorers(MatchStatLine line, IReadOnlyList<Player> homeRoster,
        IReadOnlyList<Player> awayRoster, Pcg32 rng)
    {
        if (line == null) throw new ArgumentNullException(nameof(line));
        var home = new List<Player>(line.HomeGoals);
        var away = new List<Player>(line.AwayGoals);
        for (int i = 0; i < line.HomeGoals; i++) home.Add(SamplePlayer(homeRoster, rng));
        for (int i = 0; i < line.AwayGoals; i++) away.Add(SamplePlayer(awayRoster, rng));
        line.SetScorers(home, away);
    }

    public static double TrueProbability(Matchup matchup, MarketSelection selection)
    {
        MatchLatents l = matchup.Latents;
        RunConfig config = matchup.ModelConfig;
        switch (selection.Kind)
        {
            case MarketKind.Moneyline:
                // 1X2. NOT TrueHomeProb, which is the CONDITIONAL P(home | decisive) and is what
                // this line returned while every match was decisive.
                return matchup.TrueProb(ResultOf(selection.Choice));

            case MarketKind.TotalGoals:
                RequireChoice(selection, MarketChoice.Over, MarketChoice.Under);
                double overGoals = GoalTotalProbability(matchup, selection.Line, config);
                return selection.Choice == MarketChoice.Over ? overGoals : 1.0 - overGoals;

            case MarketKind.BothTeamsToScore:
                RequireChoice(selection, MarketChoice.Yes, MarketChoice.No);
                double btts = ScoreProbability(matchup, (h, a) => h >= 1 && a >= 1, config);
                return selection.Choice == MarketChoice.Yes ? btts : 1.0 - btts;

            case MarketKind.TotalCorners:
                RequireChoice(selection, MarketChoice.Over, MarketChoice.Under);
                double overCorners = CountTotalProbability(
                    matchup.Dist.HomeCornerRaw, matchup.Dist.HomeCornerTotal,
                    matchup.Dist.AwayCornerRaw, matchup.Dist.AwayCornerTotal, selection.Line);
                return selection.Choice == MarketChoice.Over ? overCorners : 1.0 - overCorners;

            case MarketKind.TotalCards:
                RequireChoice(selection, MarketChoice.Over, MarketChoice.Under);
                double overCards = CountTotalProbability(
                    matchup.Dist.HomeCardRaw, matchup.Dist.HomeCardTotal,
                    matchup.Dist.AwayCardRaw, matchup.Dist.AwayCardTotal, selection.Line);
                return selection.Choice == MarketChoice.Over ? overCards : 1.0 - overCards;

            case MarketKind.AnytimeScorer:
                if (selection.Choice != MarketChoice.Yes)
                    throw new ArgumentException("Anytime scorer is a YES-only market");
                if (selection.Line != 0.0) throw new ArgumentException("Anytime scorer has no line");
                Player player = matchup.PlayerAt(selection.PlayerIndex);
                Side team = matchup.PlayerSide(selection.PlayerIndex);
                IReadOnlyList<Player> roster = team == Side.Home ? matchup.Home.Players : matchup.Away.Players;
                double totalWeight = 0.0;
                foreach (Player p in roster) totalWeight += p.ScoringWeight;
                if (totalWeight <= 0.0) throw new InvalidOperationException("Scorer roster has no positive weights");
                double weight = player.ScoringWeight / totalWeight;
                // The scorer formula uses the same unconditional score enumeration as every
                // other goal market, but evaluates a per-outcome miss value rather than a predicate.
                double miss = ScoreExpectation(matchup, (h, a) => Math.Pow(1.0 - weight,
                    team == Side.Home ? h : a));
                return 1.0 - miss;

            // ---- F_0.5.0 V1. Every one of these is an exact function of the SAME distributions
            // that price the older markets and sample the stat line, so pricing and grading cannot
            // diverge — the failure mode is killed structurally, not by test.

            case MarketKind.DoubleChance:
                return selection.Choice switch
                {
                    MarketChoice.HomeOrDraw => matchup.TrueProb(MatchResult.Home) + matchup.TrueProb(MatchResult.Draw),
                    MarketChoice.AwayOrDraw => matchup.TrueProb(MatchResult.Away) + matchup.TrueProb(MatchResult.Draw),
                    MarketChoice.HomeOrAway => 1.0 - matchup.TrueProb(MatchResult.Draw),
                    _ => throw new ArgumentException($"Invalid double-chance choice {selection.Choice}"),
                };

            case MarketKind.Handicap:
            {
                RequireChoice(selection, MarketChoice.Home, MarketChoice.Away);
                double line = selection.Line;
                return selection.Choice == MarketChoice.Home
                    ? ScoreProbability(matchup, (h, a) => h + line > a, config)
                    : ScoreProbability(matchup, (h, a) => a + line > h, config);
            }

            case MarketKind.TeamTotalGoals:
            {
                RequireChoice(selection, MarketChoice.Over, MarketChoice.Under);
                Side ttTeam = RequireTeam(selection);
                double lineG = selection.Line;
                double over = ScoreProbability(matchup,
                    (h, a) => (ttTeam == Side.Home ? h : a) > lineG, config);
                return selection.Choice == MarketChoice.Over ? over : 1.0 - over;
            }

            case MarketKind.CorrectScore:
                return ScoreProbability(matchup,
                    (h, a) => h == selection.ScoreHome && a == selection.ScoreAway, config);

            case MarketKind.WinningMargin:
            {
                int bucket = (int)selection.Line;
                // The top bucket is "or more" so the buckets partition the space (see the factory).
                return ScoreProbability(matchup,
                    (h, a) => bucket >= TopMarginBucket
                        ? Math.Abs(h - a) >= bucket
                        : Math.Abs(h - a) == bucket,
                    config);
            }

            case MarketKind.TotalGoalsOddEven:
            {
                double odd = ScoreProbability(matchup, (h, a) => ((h + a) & 1) == 1, config);
                return selection.Choice == MarketChoice.Odd ? odd
                    : selection.Choice == MarketChoice.Even ? 1.0 - odd
                    : throw new ArgumentException($"Odd/even takes Odd or Even, got {selection.Choice}");
            }

            case MarketKind.TeamTotalCorners:
            case MarketKind.TeamTotalCards:
            {
                RequireChoice(selection, MarketChoice.Over, MarketChoice.Under);
                Side t = RequireTeam(selection);
                bool corners = selection.Kind == MarketKind.TeamTotalCorners;
                (double[] raw, double total) = (corners, t) switch
                {
                    (true, Side.Home) => (matchup.Dist.HomeCornerRaw, matchup.Dist.HomeCornerTotal),
                    (true, _) => (matchup.Dist.AwayCornerRaw, matchup.Dist.AwayCornerTotal),
                    (false, Side.Home) => (matchup.Dist.HomeCardRaw, matchup.Dist.HomeCardTotal),
                    (false, _) => (matchup.Dist.AwayCardRaw, matchup.Dist.AwayCardTotal),
                };
                double overCount = TeamCountProbability(raw, total, selection.Line);
                return selection.Choice == MarketChoice.Over ? overCount : 1.0 - overCount;
            }

            case MarketKind.PlayerMultiScorer:
            {
                if (selection.Choice != MarketChoice.Yes)
                    throw new ArgumentException("Multi-scorer is a YES-only market");
                Player mp = matchup.PlayerAt(selection.PlayerIndex);
                Side mteam = matchup.PlayerSide(selection.PlayerIndex);
                IReadOnlyList<Player> mroster = mteam == Side.Home ? matchup.Home.Players : matchup.Away.Players;
                double mtotal = 0.0;
                foreach (Player p in mroster) mtotal += p.ScoringWeight;
                if (mtotal <= 0.0) throw new InvalidOperationException("Scorer roster has no positive weights");
                double w = mp.ScoringWeight / mtotal;
                int need = (int)selection.Line;
                // P(at least `need`) = 1 − P(0) − … − P(need−1), each term binomial in that team's
                // goals — the same categorical weights the anytime price and the attribution use.
                return 1.0 - ScoreExpectation(matchup, (h, a) =>
                {
                    int goals = mteam == Side.Home ? h : a;
                    double below = 0.0;
                    for (int k = 0; k < need; k++) below += Binomial(goals, k) * Math.Pow(w, k) * Math.Pow(1.0 - w, goals - k);
                    return below;
                });
            }

            default:
                throw new ArgumentOutOfRangeException(nameof(selection));
        }
    }

    /// <summary>The margin bucket at which the criterion becomes "or more". Buckets 1..N−1 are
    /// exact and N is the tail, so the set partitions the outcome space.</summary>
    internal const int TopMarginBucket = 3;

    private static Side RequireTeam(MarketSelection selection)
        => selection.Team ?? throw new ArgumentException(
            $"{selection.Kind} is a team-scoped market and carries no Team");

    private static double Binomial(int n, int k)
    {
        if (k < 0 || k > n) return 0.0;
        double c = 1.0;
        for (int i = 0; i < k; i++) c = c * (n - i) / (i + 1);
        return c;
    }

    private static double TeamCountProbability(double[] raw, double total, double line)
    {
        double p = 0.0;
        for (int i = 0; i < raw.Length; i++) if (i > line) p += raw[i] / total;
        return p;
    }

    /// <summary>Roster-blind grading for every <see cref="MarketKind"/> except AnytimeScorer.
    /// Private: the safe public entry point is <see cref="Grades(Matchup, MatchStatLine, MarketSelection)"/>,
    /// which has the roster context scorer legs need and forwards here for the other five kinds.
    /// A caller outside this class can no longer reach the AnytimeScorer branch below by picking
    /// the wrong overload (M-02).</summary>
    private static bool Grades(MatchStatLine line, MarketSelection selection)
    {
        switch (selection.Kind)
        {
            case MarketKind.Moneyline:
                return ResultOf(selection.Choice) == line.Result;
            case MarketKind.TotalGoals:
                return Compare(line.HomeGoals + line.AwayGoals, selection.Line, selection.Choice);
            case MarketKind.BothTeamsToScore:
                return (line.HomeGoals >= 1 && line.AwayGoals >= 1) == (selection.Choice == MarketChoice.Yes);
            case MarketKind.TotalCorners:
                return Compare(line.HomeCorners + line.AwayCorners, selection.Line, selection.Choice);
            case MarketKind.TotalCards:
                return Compare(line.HomeCards + line.AwayCards, selection.Line, selection.Choice);
            case MarketKind.DoubleChance:
                return selection.Choice switch
                {
                    MarketChoice.HomeOrDraw => line.Result != MatchResult.Away,
                    MarketChoice.AwayOrDraw => line.Result != MatchResult.Home,
                    MarketChoice.HomeOrAway => line.Result != MatchResult.Draw,
                    _ => throw new ArgumentException($"Invalid double-chance choice {selection.Choice}"),
                };

            case MarketKind.Handicap:
                // The same sentence as the price: backed + line > other.
                return selection.Choice == MarketChoice.Home
                    ? line.HomeGoals + selection.Line > line.AwayGoals
                    : selection.Choice == MarketChoice.Away
                        ? line.AwayGoals + selection.Line > line.HomeGoals
                        : throw new ArgumentException($"Handicap takes Home or Away, got {selection.Choice}");

            case MarketKind.TeamTotalGoals:
                return Compare(RequireTeam(selection) == Side.Home ? line.HomeGoals : line.AwayGoals,
                    selection.Line, selection.Choice);

            case MarketKind.CorrectScore:
                return line.HomeGoals == selection.ScoreHome && line.AwayGoals == selection.ScoreAway;

            case MarketKind.WinningMargin:
            {
                int margin = Math.Abs(line.HomeGoals - line.AwayGoals);
                int bucket = (int)selection.Line;
                return bucket >= TopMarginBucket ? margin >= bucket : margin == bucket;
            }

            case MarketKind.TotalGoalsOddEven:
                return (((line.HomeGoals + line.AwayGoals) & 1) == 1)
                    == (selection.Choice == MarketChoice.Odd);

            case MarketKind.TeamTotalCorners:
                return Compare(RequireTeam(selection) == Side.Home ? line.HomeCorners : line.AwayCorners,
                    selection.Line, selection.Choice);

            case MarketKind.TeamTotalCards:
                return Compare(RequireTeam(selection) == Side.Home ? line.HomeCards : line.AwayCards,
                    selection.Line, selection.Choice);

            case MarketKind.AnytimeScorer:
            case MarketKind.PlayerMultiScorer:
                // Unreachable now that this overload is private: the only call site is the 3-arg
                // overload's guard clause below, which never forwards a player market here. Left in
                // as a defensive invariant in case that guard is ever changed.
                throw new ArgumentException("Player-market grading requires matchup roster context");
            default:
                throw new ArgumentOutOfRangeException(nameof(selection));
        }
    }

    public static bool Grades(Matchup matchup, MatchStatLine line, MarketSelection selection)
    {
        if (selection.Kind != MarketKind.AnytimeScorer && selection.Kind != MarketKind.PlayerMultiScorer)
            return Grades(line, selection);
        if (selection.Choice != MarketChoice.Yes)
            throw new ArgumentException("Player scorer markets are YES-only");
        Player player = matchup.PlayerAt(selection.PlayerIndex);
        IReadOnlyList<Player> scorers = matchup.PlayerSide(selection.PlayerIndex) == Side.Home
            ? line.HomeScorers : line.AwayScorers;
        int scored = 0;
        foreach (Player scorer in scorers)
            if (ReferenceEquals(scorer, player)) scored++;
        // Anytime is the 1+ case of the same count, so both markets grade off one traversal and
        // cannot disagree about whether a player scored.
        int needed = selection.Kind == MarketKind.AnytimeScorer ? 1 : (int)selection.Line;
        return scored >= needed;
    }

    /// <summary>Legacy single-string market label, one packed string per selection. Retained
    /// behaviourally unchanged for <c>TvSweatScreen.cs</c> (another lead's surface, forbidden to
    /// this ruling's batch) pending that lead's own migration to <see cref="Fields"/>. New surfaces
    /// should compose their own display text from <see cref="Fields"/> instead of parsing or
    /// extending this packed string (S22, Design Director batch 4).</summary>
    public static string DisplayLabel(Matchup matchup, MarketSelection selection)
    {
        string match = $"{matchup.Away.Name} v {matchup.Home.Name}";
        switch (selection.Kind)
        {
            case MarketKind.Moneyline:
                return ResultOf(selection.Choice) switch
                {
                    MatchResult.Home => $"{matchup.Home.Name} ML — {match}",
                    MatchResult.Away => $"{matchup.Away.Name} ML — {match}",
                    _ => $"DRAW ML — {match}",
                };
            case MarketKind.TotalGoals:
                return $"{selection.Choice.ToString().ToUpperInvariant()} {selection.Line:0.0} GOALS — {match}";
            case MarketKind.BothTeamsToScore:
                return $"BTTS {selection.Choice.ToString().ToUpperInvariant()} — {match}";
            case MarketKind.TotalCorners:
                return $"{selection.Choice.ToString().ToUpperInvariant()} {selection.Line:0.0} CORNERS — {match}";
            case MarketKind.TotalCards:
                return $"{selection.Choice.ToString().ToUpperInvariant()} {selection.Line:0.0} CARDS — {match}";
            case MarketKind.AnytimeScorer:
                return $"{matchup.PlayerAt(selection.PlayerIndex).Name.ToUpperInvariant()} ANYTIME — {match}";
            default:
                return selection.Kind.ToString();
        }
    }

    /// <summary>Per-surface composable market fields (S22 ruling, Design Director batch 4): the
    /// DS vocabulary split into named parts instead of one packed string, so each surface composes
    /// its own display text (<see cref="SBR.Engine"/> callers include the laptop's compact leg
    /// label/ledger row and the console's market list). <see cref="DisplayLabel"/> keeps returning
    /// the legacy packed form for the surface that has not migrated yet.</summary>
    public static MarketFields Fields(Matchup matchup, MarketSelection selection)
    {
        string fixture = $"{matchup.Away.Name} v {matchup.Home.Name}";
        switch (selection.Kind)
        {
            case MarketKind.Moneyline:
            {
                // The DRAW's subject is the match itself, not a team — the same shape BTTS and the
                // counting markets already use. Surface wording for the draw row is Phase S / DD's
                // call; "DRAW" is the DS-neutral placeholder and is deliberately not invented past
                // that (S22: the engine emits fields, the surface composes).
                MatchResult result = ResultOf(selection.Choice);
                string subject = result switch
                {
                    MatchResult.Home => matchup.Home.Name,
                    MatchResult.Away => matchup.Away.Name,
                    _ => "",
                };
                return new MarketFields("MONEYLINE", subject,
                    result == MatchResult.Draw ? "DRAW" : "", fixture, "");
            }

            case MarketKind.TotalGoals:
            {
                RequireChoice(selection, MarketChoice.Over, MarketChoice.Under);
                string overUnder = selection.Choice == MarketChoice.Over ? "OVER" : "UNDER";
                return new MarketFields("TOTAL GOALS", "", $"{overUnder} {selection.Line:0.0} GOALS", fixture, "");
            }

            case MarketKind.BothTeamsToScore:
                RequireChoice(selection, MarketChoice.Yes, MarketChoice.No);
                // Pattern-extension: the DS enumerates only "BTTS — YES"; "BTTS — NO" follows the
                // same em-dash pattern for the mirrored choice (not DS-verbatim).
                string bttsMarket = selection.Choice == MarketChoice.Yes ? "BTTS — YES" : "BTTS — NO";
                return new MarketFields(bttsMarket, "", "BOTH TEAMS TO SCORE", fixture, "");

            case MarketKind.TotalCorners:
            {
                // Pattern-extension of TOTAL GOALS / "OVER n.n GOALS" — the DS does not separately
                // enumerate a corners market.
                RequireChoice(selection, MarketChoice.Over, MarketChoice.Under);
                string overUnder = selection.Choice == MarketChoice.Over ? "OVER" : "UNDER";
                return new MarketFields("TOTAL CORNERS", "", $"{overUnder} {selection.Line:0.0} CORNERS", fixture, "");
            }

            case MarketKind.TotalCards:
            {
                // Pattern-extension, same basis as TotalCorners above.
                RequireChoice(selection, MarketChoice.Over, MarketChoice.Under);
                string overUnder = selection.Choice == MarketChoice.Over ? "OVER" : "UNDER";
                return new MarketFields("TOTAL CARDS", "", $"{overUnder} {selection.Line:0.0} CARDS", fixture, "");
            }

            case MarketKind.AnytimeScorer:
            {
                if (selection.Choice != MarketChoice.Yes)
                    throw new ArgumentException("Anytime scorer is a YES-only market");
                Player player = matchup.PlayerAt(selection.PlayerIndex);
                string name = player.Name.ToUpperInvariant();
                return new MarketFields("ANYTIME SCORER", name, $"{name} ANYTIME", fixture, RoleWord(player.Role));
            }

            // ---- F_0.5.0 V1. Wording is DS-neutral and deliberately unpolished: the engine emits
            // fields and the surface composes (S22), and the vocabulary for these rows is Phase S /
            // DD's call, not this lane's to invent.
            case MarketKind.DoubleChance:
            {
                string dc = selection.Choice switch
                {
                    MarketChoice.HomeOrDraw => $"{matchup.Home.Name} OR DRAW",
                    MarketChoice.AwayOrDraw => $"{matchup.Away.Name} OR DRAW",
                    MarketChoice.HomeOrAway => "EITHER TEAM",
                    _ => throw new ArgumentException($"Invalid double-chance choice {selection.Choice}"),
                };
                return new MarketFields("DOUBLE CHANCE", "", dc, fixture, "");
            }

            case MarketKind.Handicap:
            {
                RequireChoice(selection, MarketChoice.Home, MarketChoice.Away);
                string hteam = selection.Choice == MarketChoice.Home ? matchup.Home.Name : matchup.Away.Name;
                return new MarketFields("HANDICAP", hteam, $"{hteam} {selection.Line:+0.0;-0.0}", fixture, "");
            }

            case MarketKind.TeamTotalGoals:
            case MarketKind.TeamTotalCorners:
            case MarketKind.TeamTotalCards:
            {
                RequireChoice(selection, MarketChoice.Over, MarketChoice.Under);
                Side t = RequireTeam(selection);
                string tname = t == Side.Home ? matchup.Home.Name : matchup.Away.Name;
                string noun = selection.Kind == MarketKind.TeamTotalGoals ? "GOALS"
                    : selection.Kind == MarketKind.TeamTotalCorners ? "CORNERS" : "CARDS";
                string ou = selection.Choice == MarketChoice.Over ? "OVER" : "UNDER";
                return new MarketFields($"TEAM {noun}", tname,
                    $"{tname} {ou} {selection.Line:0.0} {noun}", fixture, "");
            }

            case MarketKind.CorrectScore:
                return new MarketFields("CORRECT SCORE", "",
                    $"{selection.ScoreHome}-{selection.ScoreAway}", fixture, "");

            case MarketKind.WinningMargin:
            {
                int b = (int)selection.Line;
                return new MarketFields("WINNING MARGIN", "",
                    b >= TopMarginBucket ? $"{b}+ GOALS" : $"{b} GOAL{(b == 1 ? "" : "S")}", fixture, "");
            }

            case MarketKind.TotalGoalsOddEven:
                return new MarketFields("TOTAL GOALS", "",
                    selection.Choice == MarketChoice.Odd ? "ODD" : "EVEN", fixture, "");

            case MarketKind.PlayerMultiScorer:
            {
                Player mp = matchup.PlayerAt(selection.PlayerIndex);
                string mname = mp.Name.ToUpperInvariant();
                return new MarketFields($"{(int)selection.Line}+ GOALS", mname,
                    $"{mname} {(int)selection.Line}+", fixture, RoleWord(mp.Role));
            }

            default:
                throw new ArgumentOutOfRangeException(nameof(selection));
        }
    }

    /// <summary>Spells the scorer's role out as a word — the DS ruling struck the bracketed
    /// [FW]/[MF]/[DF] tag (S22).</summary>
    private static string RoleWord(PlayerRole role) => role switch
    {
        PlayerRole.FW => "FORWARD",
        PlayerRole.MF => "MIDFIELDER",
        PlayerRole.DF => "DEFENDER",
        _ => throw new ArgumentOutOfRangeException(nameof(role)),
    };

    public static IReadOnlyList<ScoreOutcome> EnumerateScores(MatchLatents latents, MatchResult result,
        RunConfig config) => EnumerateClass(latents, result, config).scores;

    /// <summary>One result class's score distribution, normalized, PLUS its raw mass before
    /// normalization. The mass is what makes P(draw) a read rather than a dial — see
    /// <see cref="MatchDistributions.DrawProb"/>.</summary>
    internal static (IReadOnlyList<ScoreOutcome> scores, double mass) EnumerateClass(
        MatchLatents latents, MatchResult result, RunConfig config)
    {
        var values = new List<ScoreOutcome>();
        double total = 0.0;
        for (int h = 0; h <= config.MaxGoalsGrid; h++)
            for (int a = 0; a <= config.MaxGoalsGrid; a++)
                if (ClassOf(h, a) == result)
                {
                    double p = PoissonPmf(h, latents.HomeGoalRate) * PoissonPmf(a, latents.AwayGoalRate);
                    values.Add(new ScoreOutcome(h, a, p));
                    total += p;
                }

        for (int i = 0; i < values.Count; i++)
            values[i] = new ScoreOutcome(values[i].HomeGoals, values[i].AwayGoals, values[i].Probability / total);
        return (values, total);
    }

    private static MatchResult ClassOf(int homeGoals, int awayGoals)
        => homeGoals > awayGoals ? MatchResult.Home
        : homeGoals < awayGoals ? MatchResult.Away
        : MatchResult.Draw;

    /// <summary>Home / Draw / Away from a single uniform, in that order. The ordering is part of
    /// the replay contract — reordering these branches would silently re-map every seed.</summary>
    private static MatchResult SampleResult(Matchup matchup, double roll)
    {
        double pHome = matchup.TrueProb(MatchResult.Home);
        if (roll < pHome) return MatchResult.Home;
        return roll < pHome + matchup.TrueProb(MatchResult.Draw) ? MatchResult.Draw : MatchResult.Away;
    }

    private static IReadOnlyList<ScoreOutcome> ScoresFor(MatchDistributions d, MatchResult result)
        => result switch
        {
            MatchResult.Home => d.HomeWinScores,
            MatchResult.Draw => d.DrawScores,
            MatchResult.Away => d.AwayWinScores,
            _ => throw new ArgumentOutOfRangeException(nameof(result)),
        };

    private static ScoreOutcome SampleScore(IReadOnlyList<ScoreOutcome> scores, Pcg32 rng)
    {
        double roll = rng.NextDouble();
        for (int i = 0; i < scores.Count; i++)
        {
            roll -= scores[i].Probability;
            if (roll < 0.0) return scores[i];
        }
        return scores[scores.Count - 1];
    }

    private static int SampleFromRaw(double[] raw, double total, Pcg32 rng)
    {
        double roll = rng.NextDouble() * total;
        for (int i = 0; i < raw.Length; i++)
        {
            roll -= raw[i];
            if (roll < 0.0) return i;
        }
        return raw.Length - 1;
    }

    /// <summary>Raw truncated-Poisson terms plus their sum — the cache keeps them unnormalized
    /// so the sampler's roll × total walk stays bit-identical to the original uncached math.</summary>
    internal static (double[] raw, double total) RawPoisson(double lambda, int max)
    {
        var p = new double[max + 1];
        double total = 0.0;
        for (int i = 0; i <= max; i++) { p[i] = PoissonPmf(i, lambda); total += p[i]; }
        return (p, total);
    }

    private static double GoalTotalProbability(Matchup matchup, double line, RunConfig config)
        => ScoreProbability(matchup, (h, a) => h + a > line, config);

    /// <summary>The unconditional law of the generative model, now a THREE-class mixture. Pricing
    /// and the sampler share it, so every goal market reprices consistently with what gets sampled
    /// — the divergence failure mode (priced off one distribution, graded off another) stays killed
    /// structurally rather than by test.</summary>
    private static double ScoreProbability(Matchup matchup, Func<int, int, bool> predicate, RunConfig config)
    {
        double p = 0.0;
        foreach ((MatchResult result, IReadOnlyList<ScoreOutcome> scores) in Classes(matchup))
        {
            double weight = matchup.TrueProb(result);
            foreach (ScoreOutcome x in scores)
                if (predicate(x.HomeGoals, x.AwayGoals)) p += weight * x.Probability;
        }
        return p;
    }

    private static double ScoreExpectation(Matchup matchup, Func<int, int, double> value)
    {
        double sum = 0.0;
        foreach ((MatchResult result, IReadOnlyList<ScoreOutcome> scores) in Classes(matchup))
        {
            double weight = matchup.TrueProb(result);
            foreach (ScoreOutcome x in scores)
                sum += weight * x.Probability * value(x.HomeGoals, x.AwayGoals);
        }
        return sum;
    }

    private static IEnumerable<(MatchResult, IReadOnlyList<ScoreOutcome>)> Classes(Matchup matchup)
    {
        MatchDistributions d = matchup.Dist;
        yield return (MatchResult.Home, d.HomeWinScores);
        yield return (MatchResult.Draw, d.DrawScores);
        yield return (MatchResult.Away, d.AwayWinScores);
    }

    private static Player SamplePlayer(IReadOnlyList<Player> roster, Pcg32 rng)
    {
        if (roster == null || roster.Count == 0) throw new ArgumentException("Scoring team has no roster");
        double total = 0.0;
        foreach (Player player in roster) total += player.ScoringWeight;
        if (total <= 0.0) throw new ArgumentException("Scoring roster has no positive weights");
        double roll = rng.NextDouble() * total;
        foreach (Player player in roster)
        {
            roll -= player.ScoringWeight;
            if (roll < 0.0) return player;
        }
        return roster[roster.Count - 1];
    }

    private static double CountTotalProbability(double[] homeRaw, double homeTotal,
        double[] awayRaw, double awayTotal, double line)
    {
        double p = 0.0;
        for (int h = 0; h < homeRaw.Length; h++)
            for (int a = 0; a < awayRaw.Length; a++)
                if (h + a > line) p += (homeRaw[h] / homeTotal) * (awayRaw[a] / awayTotal);
        return p;
    }

    private static double PoissonPmf(int k, double lambda)
    {
        double p = Math.Exp(-lambda);
        for (int i = 1; i <= k; i++) p *= lambda / i;
        return p;
    }

    /// <summary>The 1X2 choice vocabulary as a result. Throws rather than defaulting: a moneyline
    /// selection carrying Over/Under/Yes/No is a construction bug, and answering "Away" for it —
    /// which the old two-way ternaries did — is how it would have shipped unnoticed.</summary>
    internal static MatchResult ResultOf(MarketChoice choice) => choice switch
    {
        MarketChoice.Home => MatchResult.Home,
        MarketChoice.Draw => MatchResult.Draw,
        MarketChoice.Away => MatchResult.Away,
        _ => throw new ArgumentException($"Invalid moneyline choice {choice}; expected Home, Draw or Away"),
    };

    private static void RequireChoice(MarketSelection selection, MarketChoice first, MarketChoice second)
    {
        if (selection.Choice != first && selection.Choice != second)
            throw new ArgumentException($"Invalid choice {selection.Choice} for {selection.Kind}");
    }

    private static bool Compare(int value, double line, MarketChoice choice)
    {
        if (choice != MarketChoice.Over && choice != MarketChoice.Under)
            throw new ArgumentException($"Counting market requires Over or Under, got {choice}");
        return choice == MarketChoice.Over ? value > line : value < line;
    }
}
