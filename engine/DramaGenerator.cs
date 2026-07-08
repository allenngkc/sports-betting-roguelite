using System;
using System.Collections.Generic;

namespace SBR.Engine;

/// <summary>
/// The outcome-first drama generator (design/04): each leg's result is already sampled at
/// lock, and this writes a plausible win-probability path that ARRIVES at it. The path is
/// "approximately honest" for v0 — it drifts toward the known target with volatility noise
/// and an optional authored near-miss — not an exact martingale/Doob bridge. That is a
/// documented v0 simplification; the cash-out math it feeds is still the real design/02 formula.
/// </summary>
public static class DramaGenerator
{
    // Presentation heuristics, not gameplay: thresholds that classify a beat by |Δp|.
    public const double BigPlayDelta = 0.15;
    public const double ScoreDelta = 0.07;
    public const double SwingDelta = 0.10;

    // Clamp band for intermediate live probabilities (the endpoints 0/1 are only the final beat).
    public const double FloorProb = 0.03;
    public const double CeilProb = 0.97;

    // Near-miss injection lifts a losing leg to at least this / dips a winning leg to at most (1 - this).
    private const double NearMissHope = 0.75;
    private const double NearMissScare = 0.25;

    /// <summary>
    /// Builds every leg's event path for a ticket, all up front, consuming the Drama stream in a
    /// fixed order (legs in ticket order; within a leg: event count, then per-step noise, then the
    /// near-miss roll, then — only if it fires — the near-miss step). Generating all paths at once,
    /// even for legs that a loss will keep from ever being played, is what makes cursor stepping and
    /// cash-out decisions consume no RNG — so WHETHER a player cashes out can never perturb the seed.
    /// </summary>
    public static IReadOnlyList<IReadOnlyList<DramaEvent>> BuildTicketPaths(Ticket ticket, Pcg32 drama, DramaConfig config)
    {
        if (ticket == null) throw new ArgumentNullException(nameof(ticket));
        if (drama == null) throw new ArgumentNullException(nameof(drama));
        if (config == null) throw new ArgumentNullException(nameof(config));

        int legCount = ticket.Legs.Count;
        var paths = new List<IReadOnlyList<DramaEvent>>(legCount);
        for (int legIndex = 0; legIndex < legCount; legIndex++)
        {
            bool isFinalLeg = legIndex == legCount - 1;
            paths.Add(BuildLegPath(legIndex, ticket.Legs[legIndex], isFinalLeg, drama, config));
        }
        return paths;
    }

    private static IReadOnlyList<DramaEvent> BuildLegPath(int legIndex, Leg leg, bool isFinalLeg, Pcg32 drama, DramaConfig config)
    {
        LegState outcome = leg.State;
        if (outcome == LegState.Pending)
            throw new InvalidOperationException("Drama paths require a resolved leg outcome; lock the round first.");
        double target = outcome == LegState.Won ? 1.0 : 0.0;

        int baseCount = drama.NextInt(config.MinEventsPerLeg, config.MaxEventsPerLeg + 1);
        int k = isFinalLeg
            ? (int)Math.Round(baseCount * config.FinalLegBudgetMultiplier, MidpointRounding.AwayFromZero)
            : baseCount;
        // A leg must emit at least its LegFinal beat, or the session cursor would index an
        // empty path. Degenerate configs (multiplier 0, min events 0) are clamped, not crashed —
        // the /sim harness sweeps config values programmatically.
        if (k < 1) k = 1;

        // p[0] = the leg's true prob (the pre-event anchor, never emitted); p[1..k] are the beats.
        var p = new double[k + 1];
        p[0] = leg.TrueProb;
        for (int i = 1; i <= k - 1; i++)
        {
            double drift = (target - p[i - 1]) / (k - i + 1);
            double lateBias = (1.0 - config.LateDecisiveBias) + config.LateDecisiveBias * 2.0 * (i / (double)k);
            double envelope = Math.Sqrt((k - i) / (double)k); // bridge-like: variance shrinks toward the reveal
            double noise = (2.0 * drama.NextDouble() - 1.0) * config.Volatility * lateBias * envelope;
            p[i] = Math.Clamp(p[i - 1] + drift + noise, FloorProb, CeilProb);
        }
        p[k] = target; // the final beat snaps to the known outcome

        // Near-miss / scare post-pass: one authored beat in the last third (never the final beat).
        double nearMissRoll = drama.NextDouble();
        int nearMissStep = -1;
        if (nearMissRoll < config.NearMissChance)
        {
            int lastThirdStart = Math.Max(1, (int)Math.Ceiling(2.0 * k / 3.0));
            if (lastThirdStart > k - 1) lastThirdStart = k - 1;
            nearMissStep = drama.NextInt(lastThirdStart, k); // inclusive [lastThirdStart, k-1]
            p[nearMissStep] = outcome == LegState.Lost
                ? Math.Clamp(Math.Max(p[nearMissStep], NearMissHope), FloorProb, CeilProb)
                : Math.Clamp(Math.Min(p[nearMissStep], NearMissScare), FloorProb, CeilProb);
        }

        var events = new List<DramaEvent>(k);
        for (int i = 1; i <= k; i++)
        {
            double delta = p[i] - p[i - 1];
            double absDelta = Math.Abs(delta);
            bool isFinal = i == k;

            DramaEventType type =
                isFinal ? DramaEventType.LegFinal
                : absDelta >= BigPlayDelta ? DramaEventType.BigPlay
                : absDelta >= ScoreDelta ? DramaEventType.Score
                : DramaEventType.Momentum;

            // Tag precedence: the final beat is always Decisive; then the authored near-miss; then a
            // lead change (p crosses 0.5); then a plain swing; else calm.
            TensionTag tag =
                isFinal ? TensionTag.Decisive
                : i == nearMissStep ? TensionTag.NearMiss
                : (p[i - 1] - 0.5) * (p[i] - 0.5) < 0.0 ? TensionTag.LeadChange
                : absDelta >= SwingDelta ? TensionTag.Swing
                : TensionTag.Calm;

            events.Add(new DramaEvent(legIndex, i, k, type, p[i], tag));
        }
        return events;
    }
}
