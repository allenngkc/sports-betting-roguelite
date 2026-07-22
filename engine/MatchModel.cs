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
        var d = new MatchDistributions
        {
            HomeWinScores = MatchModel.EnumerateScores(l, true, config),
            AwayWinScores = MatchModel.EnumerateScores(l, false, config),
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
        var offers = new List<MarketOffer>
        {
            Offer(matchup, MarketSelection.Moneyline(Side.Home), config),
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
        bool homeWon = outcomes.NextDouble() < matchup.TrueHomeProb;
        ScoreOutcome score = SampleScore(homeWon ? d.HomeWinScores : d.AwayWinScores, outcomes);
        int homeCorners = SampleFromRaw(d.HomeCornerRaw, d.HomeCornerTotal, outcomes);
        int awayCorners = SampleFromRaw(d.AwayCornerRaw, d.AwayCornerTotal, outcomes);
        int homeCards = SampleFromRaw(d.HomeCardRaw, d.HomeCardTotal, outcomes);
        int awayCards = SampleFromRaw(d.AwayCardRaw, d.AwayCardTotal, outcomes);
        return new MatchStatLine(score.HomeGoals, score.AwayGoals, homeCorners, awayCorners, homeCards, awayCards);
    }

    public static double TrueProbability(Matchup matchup, MarketSelection selection)
    {
        MatchLatents l = matchup.Latents;
        RunConfig config = matchup.ModelConfig;
        switch (selection.Kind)
        {
            case MarketKind.Moneyline:
                RequireChoice(selection, MarketChoice.Home, MarketChoice.Away);
                return selection.Choice == MarketChoice.Home ? matchup.TrueHomeProb : 1.0 - matchup.TrueHomeProb;

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
                throw new NotSupportedException("Anytime Scorer is a Phase 4 market");

            default:
                throw new ArgumentOutOfRangeException(nameof(selection));
        }
    }

    public static bool Grades(MatchStatLine line, MarketSelection selection)
    {
        switch (selection.Kind)
        {
            case MarketKind.Moneyline:
                return selection.Choice == (line.Winner == Side.Home ? MarketChoice.Home : MarketChoice.Away);
            case MarketKind.TotalGoals:
                return Compare(line.HomeGoals + line.AwayGoals, selection.Line, selection.Choice);
            case MarketKind.BothTeamsToScore:
                return (line.HomeGoals >= 1 && line.AwayGoals >= 1) == (selection.Choice == MarketChoice.Yes);
            case MarketKind.TotalCorners:
                return Compare(line.HomeCorners + line.AwayCorners, selection.Line, selection.Choice);
            case MarketKind.TotalCards:
                return Compare(line.HomeCards + line.AwayCards, selection.Line, selection.Choice);
            case MarketKind.AnytimeScorer:
                throw new NotSupportedException("Anytime Scorer is a Phase 4 market");
            default:
                throw new ArgumentOutOfRangeException(nameof(selection));
        }
    }

    public static string DisplayLabel(Matchup matchup, MarketSelection selection)
    {
        string match = $"{matchup.Away.Name} v {matchup.Home.Name}";
        switch (selection.Kind)
        {
            case MarketKind.Moneyline:
                return $"{(selection.Choice == MarketChoice.Home ? matchup.Home.Name : matchup.Away.Name)} ML — {match}";
            case MarketKind.TotalGoals:
                return $"{selection.Choice.ToString().ToUpperInvariant()} {selection.Line:0.0} GOALS — {match}";
            case MarketKind.BothTeamsToScore:
                return $"BTTS {selection.Choice.ToString().ToUpperInvariant()} — {match}";
            case MarketKind.TotalCorners:
                return $"{selection.Choice.ToString().ToUpperInvariant()} {selection.Line:0.0} CORNERS — {match}";
            case MarketKind.TotalCards:
                return $"{selection.Choice.ToString().ToUpperInvariant()} {selection.Line:0.0} CARDS — {match}";
            default:
                return selection.Kind.ToString();
        }
    }

    public static IReadOnlyList<ScoreOutcome> EnumerateScores(MatchLatents latents, bool homeWon, RunConfig config)
    {
        var values = new List<ScoreOutcome>();
        double total = 0.0;
        for (int h = 0; h <= config.MaxGoalsGrid; h++)
            for (int a = 0; a <= config.MaxGoalsGrid; a++)
                if ((homeWon && h > a) || (!homeWon && a > h))
                {
                    double p = PoissonPmf(h, latents.HomeGoalRate) * PoissonPmf(a, latents.AwayGoalRate);
                    values.Add(new ScoreOutcome(h, a, p));
                    total += p;
                }

        for (int i = 0; i < values.Count; i++)
            values[i] = new ScoreOutcome(values[i].HomeGoals, values[i].AwayGoals, values[i].Probability / total);
        return values;
    }

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

    private static double ScoreProbability(Matchup matchup, Func<int, int, bool> predicate, RunConfig config)
    {
        double p = 0.0;
        foreach (ScoreOutcome x in matchup.Dist.HomeWinScores)
            if (predicate(x.HomeGoals, x.AwayGoals)) p += matchup.TrueHomeProb * x.Probability;
        foreach (ScoreOutcome x in matchup.Dist.AwayWinScores)
            if (predicate(x.HomeGoals, x.AwayGoals)) p += (1.0 - matchup.TrueHomeProb) * x.Probability;
        return p;
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
