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
    /// Builds every TELLING's event path for a ticket, all up front, consuming the Drama stream in a
    /// fixed order (legs in ticket order; within a leg: event count, then per-step noise, then the
    /// near-miss roll, then — only if it fires — the near-miss step). Generating all paths at once,
    /// even for legs that a loss will keep from ever being played, is what makes cursor stepping and
    /// cash-out decisions consume no RNG — so WHETHER a player cashes out can never perturb the seed.
    /// The 1-based round selects the progressive-density event bounds (design/04: early sweats
    /// are shorter; the draw structure per leg is round-independent, only the bounds move).
    ///
    /// <para><b>A path is now one per (ticket, FIXTURE), not one per leg</b> — <c>T140</c> arm A. A
    /// fixture is broadcast ONCE per ticket and every leg riding it resolves at that single whistle, so
    /// <c>paths.Count</c> is the ticket's FIXTURE count. On a ticket with at most one leg per matchup
    /// that is the leg count and the two shapes coincide.</para>
    ///
    /// <para><b>THE DRAMA STREAM IS NOT TOUCHED BY THAT CHANGE, AND THAT IS THE WHOLE DESIGN.</b> The
    /// draws below still run per LEG, in ticket order, in the same four-draw sequence as before arm A;
    /// only the ASSEMBLY changed. So a ticket with no same-match pair produces bit-identical beats, and
    /// — because the stream position after a ticket is unchanged — so does every LATER ticket in the
    /// round, which a regenerating implementation would have silently shifted. Byte-identity here is
    /// structural, not measured and hoped for. The same discipline as
    /// <c>SweatSession.RawCashOutFair</c> and <c>Ticket.PotentialPayout</c>, for the same reason: the
    /// golden seeds and the whole gate baseline sit downstream of these bits.</para>
    /// </summary>
    public static IReadOnlyList<IReadOnlyList<DramaEvent>> BuildTicketPaths(Ticket ticket, Pcg32 drama, DramaConfig config, int round)
    {
        if (ticket == null) throw new ArgumentNullException(nameof(ticket));
        if (drama == null) throw new ArgumentNullException(nameof(drama));
        if (config == null) throw new ArgumentNullException(nameof(config));

        (int minEvents, int maxEvents) = config.EventBoundsForRound(round);

        int legCount = ticket.Legs.Count;

        // ---- THE DRAWS. Per leg, ticket order, unchanged from before arm A. Do not reorder, do not
        // make conditional on the fixture grouping, do not skip a leg that shares a fixture: every one
        // of those would move the stream and every ticket in the round with it.
        var tracks = new LegTrack[legCount];
        for (int legIndex = 0; legIndex < legCount; legIndex++)
        {
            bool isFinalLeg = legIndex == legCount - 1;
            tracks[legIndex] = BuildLegTrack(ticket.Legs[legIndex], isFinalLeg, drama, config, minEvents, maxEvents);
        }

        // ---- THE ASSEMBLY. Fixtures in first-appearance order over the ticket's legs — the same
        // grouping, through the same helper, in the same order the joint price factorises through.
        (_, List<List<int>> groups) = SameMatchModel.GroupByMatchup(ticket.Legs);

        var paths = new List<IReadOnlyList<DramaEvent>>(groups.Count);
        for (int f = 0; f < groups.Count; f++)
        {
            List<int> members = groups[f];
            paths.Add(members.Count == 1
                ? EmitSoloTelling(f, members[0], tracks[members[0]])
                : EmitSharedTelling(f, members, tracks));
        }
        return paths;
    }

    /// <summary>One leg's probability track before it is assembled into a telling: <c>P[0]</c> is the
    /// leg's true prob (the pre-event anchor, never emitted), <c>P[1..k]</c> are its beats, and
    /// <c>NearMissStep</c> is the authored beat's index in that space (-1 if none fired).</summary>
    private readonly struct LegTrack
    {
        public readonly double[] P;
        public readonly int NearMissStep;
        public LegTrack(double[] p, int nearMissStep) { P = p; NearMissStep = nearMissStep; }
        public int Steps => P.Length - 1;
    }

    /// <summary>The draws for one leg. Byte-identical to the pre-arm-A <c>BuildLegPath</c> up to the
    /// point where it began emitting events — same four draws, same order, same arithmetic.</summary>
    private static LegTrack BuildLegTrack(Leg leg, bool isFinalLeg, Pcg32 drama, DramaConfig config,
        int minEvents, int maxEvents)
    {
        LegState outcome = leg.State;
        if (outcome == LegState.Pending)
            throw new InvalidOperationException("Drama paths require a resolved leg outcome; lock the round first.");
        double target = outcome == LegState.Won ? 1.0 : 0.0;

        int baseCount = drama.NextInt(minEvents, maxEvents + 1);
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

        return new LegTrack(p, nearMissStep);
    }

    /// <summary>A telling with ONE leg on the fixture — every telling on a ticket without a same-match
    /// pair. This is the pre-arm-A emission loop, verbatim, and it is kept as its own branch rather
    /// than folded into the shared case precisely so the identity is structural: an algebraically equal
    /// rewrite could still differ in the last bits, and <c>GoldenReplay</c>'s pin sits on those bits.</summary>
    private static IReadOnlyList<DramaEvent> EmitSoloTelling(int fixtureIndex, int legIndex, LegTrack track)
    {
        double[] p = track.P;
        int k = track.Steps;
        int nearMissStep = track.NearMissStep;

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

            events.Add(new DramaEvent(legIndex, fixtureIndex, i, k, type, p[i], tag, null, null));
        }
        return events;
    }

    /// <summary>
    /// A telling with N legs on one fixture (<c>T140</c> arm A): ONE clock, ONE whistle, every leg live
    /// throughout and each arriving at its OWN outcome at the final beat.
    ///
    /// <para><b>The shared clock is <c>K = max(kᵢ)</c> and each leg's track is RESAMPLED onto it.</b>
    /// The alternative — drawing one event count for the fixture — would have consumed the drama stream
    /// differently and shifted every later ticket in the round. Resampling buys the shared clock for
    /// nothing: the draws stay per-leg and untouched, and <c>K == kᵢ</c> makes the resample the identity,
    /// which is why the solo branch above is bit-exact.</para>
    ///
    /// <para><b>The anchor leg drives the beat's SHAPE</b> — its delta picks the type and the tag, and
    /// its near-miss beat keeps its tag — because a beat has one shape and the legs can disagree about
    /// what just happened. The anchor is the lowest ticket-order leg on the fixture. Each leg's own
    /// probability rides on <see cref="DramaEvent.LegProbs"/>, so nothing is lost; what the anchor
    /// decides is only which of N honest descriptions the cut is made on.</para>
    ///
    /// <para><b>Cost, named rather than discovered: a shorter leg's noise is smoothed</b> by the
    /// resample onto a longer clock. This is a v0 drama property on the one ticket shape that has no
    /// telling at all today, and it is strictly better than the rewind it replaces.</para>
    /// </summary>
    private static IReadOnlyList<DramaEvent> EmitSharedTelling(int fixtureIndex, List<int> members, LegTrack[] tracks)
    {
        int k = 0;
        foreach (int m in members)
            if (tracks[m].Steps > k) k = tracks[m].Steps;

        int count = members.Count;
        var rails = new double[count][];
        for (int i = 0; i < count; i++)
            rails[i] = Resample(tracks[members[i]].P, k);

        // The index array is shared by every beat on this path: it is the same list of legs each time,
        // and a per-beat copy would allocate N arrays per telling for nothing.
        var legIndices = members.ToArray();
        int anchorLeg = members[0];
        double[] anchor = rails[0];
        int nearMissStep = RemapStep(tracks[anchorLeg].NearMissStep, tracks[anchorLeg].Steps, k);

        var events = new List<DramaEvent>(k);
        for (int i = 1; i <= k; i++)
        {
            double delta = anchor[i] - anchor[i - 1];
            double absDelta = Math.Abs(delta);
            bool isFinal = i == k;

            DramaEventType type =
                isFinal ? DramaEventType.LegFinal
                : absDelta >= BigPlayDelta ? DramaEventType.BigPlay
                : absDelta >= ScoreDelta ? DramaEventType.Score
                : DramaEventType.Momentum;

            TensionTag tag =
                isFinal ? TensionTag.Decisive
                : i == nearMissStep ? TensionTag.NearMiss
                : (anchor[i - 1] - 0.5) * (anchor[i] - 0.5) < 0.0 ? TensionTag.LeadChange
                : absDelta >= SwingDelta ? TensionTag.Swing
                : TensionTag.Calm;

            var legProbs = new double[count];
            for (int r = 0; r < count; r++) legProbs[r] = rails[r][i];

            events.Add(new DramaEvent(anchorLeg, fixtureIndex, i, k, type, anchor[i], tag, legIndices, legProbs));
        }
        return events;
    }

    /// <summary>Piecewise-linear resample of a track of <c>k</c> beats onto a clock of <c>K</c>. Both
    /// endpoints are carried across EXACTLY — <c>q[0]</c> is still the leg's true prob and <c>q[K]</c>
    /// is still its outcome, so the whistle remains a snap to 0 or 1 rather than an interpolation
    /// toward one. <c>K == k</c> returns the source array itself: the identity is the same doubles, not
    /// an arithmetic reconstruction of them.</summary>
    private static double[] Resample(double[] p, int k)
    {
        int source = p.Length - 1;
        if (source == k) return p;

        var q = new double[k + 1];
        q[0] = p[0];
        for (int i = 1; i < k; i++)
        {
            double x = i * (double)source / k;
            int lo = (int)Math.Floor(x);
            if (lo >= source) lo = source - 1;
            double t = x - lo;
            q[i] = p[lo] + (p[lo + 1] - p[lo]) * t;
        }
        q[k] = p[source];
        return q;
    }

    /// <summary>Carries an authored beat's index from its own track onto the shared clock, keeping it
    /// inside the emitted range and off the final beat (whose tag is always Decisive). -1 stays -1.</summary>
    private static int RemapStep(int step, int source, int k)
    {
        if (step < 0) return -1;
        if (source == k) return step;
        int mapped = (int)Math.Round(step * (double)k / source, MidpointRounding.AwayFromZero);
        if (mapped < 1) mapped = 1;
        if (mapped > k - 1) mapped = k - 1;
        return mapped;
    }
}
