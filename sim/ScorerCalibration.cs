using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>
/// The scorer calibration instrument (--scorer-ev). Every bot is policy-excluded from pricing
/// AnytimeScorer (RandomStrategy.RandomBotSelection, SkilledStrategy's candidate scan, and
/// SkilledStrategy.Opposite's "Bots do not price {Kind}" throw) — a declared human-agency market.
/// For every two-way market the sharp bot recovers truth by exact de-vig and the gates verify the
/// economy that way; a one-sided market has no de-vig, so G7's coverage gate can name the hole but
/// no gate can ever close it — scorer pricing has no instrument at all. What survives without a
/// strategy is calibration: does the probability the engine PRICES a scorer at
/// (MatchModel.TrueProbability, read here through Matchup.TrueProb) match the frequency the
/// engine's OWN sampler REALISES for that player (MatchModel.SampleStatLine + SampleScorers)? A
/// player priced at p who scores at frequency q ≠ p is mispriced regardless of what any bot does
/// or doesn't do with the offer.
///
/// Sampling builds a slate exactly as the rest of the sim does (new Run(seed, cfg) — see
/// RunPlayer.Play / Harness.RunBatch), then measures every AnytimeScorer offer on that slate
/// WITHOUT ever calling Run.LockRound: each resample is drawn from RngHub.DeriveMatch, never
/// Rng.Outcomes or Rng.Slate, so no amount of measurement can perturb the slate that was priced.
/// Grading goes through the static MatchModel.Grades(matchup, line, selection) overload rather
/// than assigning Matchup.StatLine, so the Run this file touches is never mutated either — it is
/// left exactly as SlateGenerator produced it.
/// </summary>
public sealed class ScorerCalibrationData
{
    /// <summary>Resamples per MATCHUP, not per offer: one sampled match outcome (a score line plus
    /// who scored it) is checked against every scorer offer on that matchup at once, so a
    /// 14-offer board costs one sample, not fourteen. Fixed rather than tied to --runs: the
    /// report's statistical power comes from pooling many matchups into each bucket (the --runs
    /// loop below), not from grinding one matchup — --runs already means "independent seeded
    /// draws" everywhere else in this file, and overloading it here to also mean "resamples of one
    /// matchup" would make the same flag mean two different things depending on who's reading it.</summary>
    public const int SamplesPerMatchup = 50;

    public int Runs;
    public int MatchupsSampled;
    public readonly List<Offer> Offers = new();

    /// <summary>One AnytimeScorer offer's priced terms plus its realised hit rate.</summary>
    public sealed class Offer
    {
        public double PricedProb;
        public double Odds;
        public PlayerRole Role;
        public int Hits;
        public int Samples;
    }

    public static ScorerCalibrationData Compute(int runs, string seedPrefix, RunConfig cfg)
    {
        var data = new ScorerCalibrationData { Runs = runs };
        // Sequential by design: a Run build plus its resample batch is cheap (SamplesPerMatchup
        // is small and no ticket/sweat/settle machinery ever runs), and a plain loop makes the
        // determinism check trivially true rather than dependent on carrying Harness.RunBatch's
        // parallel-then-reduce-by-index discipline into a file that has no other reason to need it.
        for (int i = 0; i < runs; i++)
        {
            var run = new Run($"{seedPrefix}-{i}", cfg);
            foreach (Matchup matchup in run.CurrentSlate.Matchups)
            {
                data.MatchupsSampled++;
                SampleMatchup(run, matchup, data.Offers);
            }
        }
        return data;
    }

    private static void SampleMatchup(Run run, Matchup matchup, List<Offer> sink)
    {
        var offers = new List<MarketOffer>();
        foreach (MarketOffer offer in matchup.Markets)
            if (offer.Selection.Kind == MarketKind.AnytimeScorer)
                offers.Add(offer);
        if (offers.Count == 0) return; // PlayersPerTeam misconfigured to 0 — nothing to measure

        var hits = new int[offers.Count];
        for (int k = 0; k < SamplesPerMatchup; k++)
        {
            // One derived stream per sample, shared by every offer on this matchup: it is ONE
            // sampled match, and every offer reads the same realisation — exactly what the real
            // engine's own scorer board would show, not 14 unrelated coin flips. The purpose
            // string carries the sample ordinal so consecutive samples are independent draws,
            // never the same draw read twice.
            Pcg32 rng = run.Rng.DeriveMatch(run.Round, matchup.Index, $"scorers#{k}");
            MatchStatLine line = MatchModel.SampleStatLine(matchup, rng);
            MatchModel.SampleScorers(line, matchup.Home.Players, matchup.Away.Players, rng);

            for (int oi = 0; oi < offers.Count; oi++)
                if (MatchModel.Grades(matchup, line, offers[oi].Selection))
                    hits[oi]++;
        }

        for (int oi = 0; oi < offers.Count; oi++)
        {
            MarketSelection selection = offers[oi].Selection;
            sink.Add(new Offer
            {
                // Re-read through Matchup's own accessors rather than the cached MarketOffer
                // fields — this is the exact pair (priced truth vs. sampler) the instrument exists
                // to compare, so it reads both sides the way any other caller in this codebase would.
                PricedProb = matchup.TrueProb(selection),
                Odds = matchup.Odds(selection),
                Role = matchup.PlayerAt(selection.PlayerIndex).Role,
                Hits = hits[oi],
                Samples = SamplesPerMatchup,
            });
        }
    }

    // ---- report aggregation — lives here so Report.cs stays pure rendering (house style: see
    // GateData/AuditData in Analysis.cs, which own their own statistics for the same reason) ----

    public sealed class Bucket
    {
        public string Label = "";
        public int OfferCount;
        public int SampleCount;
        public double MeanPricedProb;
        public double RealizedFreq;
        public double EvFraction;
        public double SeFraction;
    }

    private static readonly (double Upper, string Label)[] ProbabilityBands =
    {
        (0.05, "0–5%"),
        (0.10, "5–10%"),
        (0.20, "10–20%"),
        (0.35, "20–35%"),
        (double.PositiveInfinity, "35%+"),
    };

    public IReadOnlyList<Bucket> ByProbabilityBand()
    {
        var acc = new Accumulator[ProbabilityBands.Length];
        for (int i = 0; i < acc.Length; i++) acc[i] = new Accumulator();
        foreach (Offer o in Offers)
        {
            int bi = 0;
            while (o.PricedProb >= ProbabilityBands[bi].Upper) bi++;
            acc[bi].Add(o);
        }
        var rows = new List<Bucket>(ProbabilityBands.Length);
        for (int i = 0; i < ProbabilityBands.Length; i++)
            rows.Add(acc[i].ToBucket(ProbabilityBands[i].Label));
        return rows;
    }

    /// <summary>Same aggregation, grouped by role instead of priced probability — scoring weight
    /// (and so priced probability) is assigned purely by role, so this is the fastest possible
    /// check for whether a miscalibration is role-shaped rather than a general drift.</summary>
    public IReadOnlyList<Bucket> ByRole()
    {
        var fw = new Accumulator();
        var mf = new Accumulator();
        var df = new Accumulator();
        foreach (Offer o in Offers)
        {
            Accumulator a = o.Role switch { PlayerRole.FW => fw, PlayerRole.MF => mf, _ => df };
            a.Add(o);
        }
        return new[] { fw.ToBucket("FW"), mf.ToBucket("MF"), df.ToBucket("DF") };
    }

    /// <summary>Pools at the SAMPLE level, not the offer level: q = total hits / total samples,
    /// and the EV numerator sums each offer's hits×odds before dividing by the pooled sample
    /// count. Averaging every offer's own (q×odds−1) instead would weight a thinly-sampled offer
    /// the same as a well-sampled one; pooling first means every individual (offer, sample) draw
    /// counts exactly once.</summary>
    private sealed class Accumulator
    {
        private int _offerCount;
        private int _sampleCount;
        private int _hitCount;
        private double _pricedProbSum;
        private double _hitOddsSum;

        public void Add(Offer o)
        {
            _offerCount++;
            _sampleCount += o.Samples;
            _hitCount += o.Hits;
            _pricedProbSum += o.PricedProb;
            _hitOddsSum += o.Hits * o.Odds;
        }

        public Bucket ToBucket(string label)
        {
            if (_offerCount == 0) return new Bucket { Label = label };
            double q = (double)_hitCount / _sampleCount;
            return new Bucket
            {
                Label = label,
                OfferCount = _offerCount,
                SampleCount = _sampleCount,
                MeanPricedProb = _pricedProbSum / _offerCount,
                RealizedFreq = q,
                EvFraction = _hitOddsSum / _sampleCount - 1.0,
                SeFraction = Math.Sqrt(q * (1.0 - q) / _sampleCount),
            };
        }
    }
}
