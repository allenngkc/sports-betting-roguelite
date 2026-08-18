using System;
using System.Collections.Generic;
using System.Linq;
using SBR.Engine;

namespace SBR.Engine.Tests;

/// <summary>
/// CALM-BEAT REACHABILITY PROBE — a manual diagnostic, not a regression test.
///
/// <para>Settles the one thing a DD exploration into widening the count-market scene gate named in
/// its §8 but did not check: <b>are <see cref="DramaEventType.Momentum"/> beats tagged
/// <see cref="TensionTag.Calm"/> actually SCHEDULED during a corners sweat?</b> Quoting §8 directly:
/// "I have read the decision path; I have NOT verified that Momentum beats tagged Calm are actually
/// scheduled during a corners sweat. If the drama stream for a corners leg never emits them, widening
/// the gate yields Territory or Fallback — not calm." Naming a reachable branch is not checking that
/// anything reaches it, so this probe measures the real beat stream instead of re-deriving the
/// classification rule from its own thresholds.</para>
///
/// <para><b>Why the docked capture can't answer this instead.</b> Its frame filenames carry the SCENE
/// token, not the beat's (Type, Tag) pair. <c>TheaterChoreographer.ResolveBeat</c> takes its count
/// branch (unity/SBR/Assets/SBR/Runtime/TheaterChoreographer.cs:56-95) before the generic
/// (Type, Tag) → scene table (ibid. 116-127) ever runs, so a <c>Calm</c>-tagged Momentum beat and a
/// <c>Swing</c>-tagged one can both render <c>CornerFor</c>/<c>CornerAgainst</c> whenever that beat
/// coincides with a nonzero staged count. The frame stream is blind to exactly the distinction §8
/// asks about. The evidence has to come from the beat stream itself — pure engine, deterministic, and
/// upstream of any presentation routing.</para>
///
/// <para><b>Reproduction.</b> Both arms share ONE seed, <c>CORNERS-SWEAT-1</c> — the same seed the
/// docked capture used (<c>unity/SBR/Assets/Tests/PlayMode/TvSweatCaptureHarness.cs</c>) — and differ
/// only in which market is picked, which is the matched pair the whole read rests on:</para>
/// <list type="bullet">
/// <item><description>CORNERS arm: the first matchup on the slate offering
/// <see cref="MarketKind.TotalCorners"/>, its first such offer.</description></item>
/// <item><description>GOALS arm (control): the first matchup on the slate offering
/// <see cref="MarketKind.TotalGoals"/>, its first such offer.</description></item>
/// </list>
/// <para>Both selections are read OFF the generated board (<see cref="Matchup.Markets"/>), never
/// constructed — the docked harness calls an invented selection out by name as the exact class of
/// error that has cost this lane findings before. One leg, stake 25.0, each. If this seed's slate
/// offers neither market, that is a reported result (the assertion below says so by name) — never a
/// reason to substitute a different market.</para>
///
/// <para><b>Call order.</b> The path is built by driving <see cref="Run"/>'s own public API —
/// construct, <see cref="Run.PlaceTicket"/>, <see cref="Run.LockRound"/> — exactly as
/// <c>RunDirector.StartNewRun</c> and a real sweat would, rather than calling
/// <see cref="DramaGenerator.BuildTicketPaths"/> directly: <see cref="Run.LockRound"/> already calls it
/// once per ticket (engine/Run.cs:405-414); calling it again ourselves would draw a SECOND, different
/// path off the same <c>Pcg32</c> stream and silently stop matching what a real run actually shows.
/// The built path is then read back through <see cref="SweatSession.MoveNext"/> — the same draining
/// idiom <c>engine.tests/GoldenSeedTests.cs</c>'s <c>DrainAll</c> already uses in this project.</para>
///
/// <para><b>Framework note.</b> <c>engine.tests</c> is xUnit 2.9.3 (SBR.Engine.Tests.csproj), which has
/// no NUnit-style <c>[Explicit]</c> — that attribute does not exist here and would not compile. No file
/// in <c>engine.tests</c> marks a test out-of-band today either (no <c>Skip =</c>, no <c>[Trait]</c>
/// category anywhere in the directory), so there is no prior in-project convention beyond the
/// framework's own mechanism for "exists, does not run in the routine suite": <see cref="FactAttribute.Skip"/>.
/// A bare <c>dotnet test engine.tests</c> reports this method as skipped and never executes its body or
/// prints anything — the routine green count is unaffected. <b>To actually run it:</b> comment out the
/// <c>Skip = "..."</c> argument below (or use an IDE test explorer's "run anyway" on a skipped xUnit
/// Fact, where offered), then
/// <c>dotnet test engine.tests --filter "FullyQualifiedName~CalmBeatReachabilityProbe"</c>, then restore
/// the <c>Skip</c> argument so the routine suite goes back to not running it.</para>
///
/// <para><b>What this does NOT do.</b> It never recomputes <see cref="DramaEventType"/> or
/// <see cref="TensionTag"/> from the win-probability thresholds in <see cref="DramaGenerator"/> — it
/// only reads the two properties straight off each emitted <see cref="DramaEvent"/>. Recomputing them
/// would only prove the classification rule can be reimplemented, which was never the open question.
/// It also asserts nothing about whether Momentum+Calm beats exist: that count IS the §8 finding, and
/// a probe that fails on "zero" would destroy the evidence it exists to collect. Nor does it simulate
/// <c>TheaterChoreographer</c>'s count-branch routing (which needs a live <c>CountLedger</c> this
/// probe never constructs) — §8 asks only whether the drama stream schedules the beat at all, which is
/// upstream of, and independent from, whatever a scene resolver later does with it.</para>
/// </summary>
public class CalmBeatReachabilityProbe
{
    private readonly Xunit.Abstractions.ITestOutputHelper _output;

    public CalmBeatReachabilityProbe(Xunit.Abstractions.ITestOutputHelper output) => _output = output;

    private const string Seed = "CORNERS-SWEAT-1";
    private const double Stake = 25.0;
    private const string LogPrefix = "[CALM-PROBE]";

    /// <summary>One arm's full outcome: the drained beat stream plus the three §8 counts.</summary>
    private sealed class ArmResult
    {
        public string Label { get; }
        public IReadOnlyList<DramaEvent> Events { get; }
        public int TotalBeats { get; }
        public int MomentumBeats { get; }
        public int MomentumCalmBeats { get; }

        public ArmResult(string label, IReadOnlyList<DramaEvent> events, int totalBeats,
            int momentumBeats, int momentumCalmBeats)
        {
            Label = label;
            Events = events;
            TotalBeats = totalBeats;
            MomentumBeats = momentumBeats;
            MomentumCalmBeats = momentumCalmBeats;
        }
    }

    /// <summary>The first offer of <paramref name="kind"/> on the slate, in slate/board order — OFF
    /// the board, never constructed (engine/Domain.cs: <see cref="Matchup.Markets"/> /
    /// <see cref="MarketOffer.Selection"/>). Returns matchupIndex -1 when the slate offers none, which
    /// the caller reports rather than papering over with a substitute market.</summary>
    private static (int matchupIndex, MarketSelection selection) FirstOffer(Slate slate, MarketKind kind)
    {
        foreach (Matchup m in slate.Matchups)
            foreach (MarketOffer offer in m.Markets)
                if (offer.Selection.Kind == kind)
                    return (m.Index, offer.Selection);
        return (-1, default);
    }

    /// <summary>Places the one-leg ticket, locks the round, and drains the leg's full beat stream via
    /// <see cref="SweatSession.MoveNext"/> — the same public stepping API a real sweat uses, so the
    /// stream position matches a real run. Prints every required line, prefixed
    /// <c>[CALM-PROBE]</c>, along the way.</summary>
    /// <summary>The first offer of <paramref name="kind"/> on ONE named matchup, optionally
    /// constrained to a direction. Exists because the control arm must ride the SAME MATCH as the
    /// corners arm — see <see cref="Momentum_beats_tagged_Calm_are_measured_directly_for_both_arms"/>.
    /// Returns false when that matchup offers no such line, which the caller reports rather than
    /// silently falling back to another match.</summary>
    private static bool FirstOfferOnMatchup(Matchup m, MarketKind kind, MarketChoice? choice,
        out MarketSelection selection)
    {
        foreach (MarketOffer offer in m.Markets)
        {
            if (offer.Selection.Kind != kind) continue;
            if (choice.HasValue && offer.Selection.Choice != choice.Value) continue;
            selection = offer.Selection;
            return true;
        }
        selection = default;
        return false;
    }

    private ArmResult RunArm(Run run, MarketKind kind, string label,
        int forceMatchupIndex = -1, MarketChoice? choice = null)
    {
        int matchupIndex;
        MarketSelection selection;
        if (forceMatchupIndex >= 0)
        {
            matchupIndex = forceMatchupIndex;
            bool ok = FirstOfferOnMatchup(run.CurrentSlate.Matchups[matchupIndex], kind, choice, out selection);
            Assert.True(ok,
                $"{LogPrefix} {label}: matchup #{matchupIndex} offers no {kind}" +
                (choice.HasValue ? $"/{choice.Value}" : "") +
                " line, so the control cannot be built on the same match — a reported result, never a " +
                "reason to move the control to a different match.");
        }
        else
        {
            (matchupIndex, selection) = FirstOffer(run.CurrentSlate, kind);
        }
        _output.WriteLine($"{LogPrefix} {label}: first {kind} offer on seed '{Seed}' -> " +
            (matchupIndex >= 0 ? $"matchup #{matchupIndex}" : "NONE FOUND"));
        Assert.True(matchupIndex >= 0,
            $"{LogPrefix} {label}: no matchup on seed '{Seed}' offers {kind} — a reported result, " +
            "never a reason to substitute a different market.");

        Matchup matchup = run.CurrentSlate.Matchups[matchupIndex];
        Ticket ticket = run.PlaceTicket(new[] { new Pick(matchupIndex, selection) }, Stake);
        run.LockRound();

        _output.WriteLine($"{LogPrefix} {label}: {matchup.Home.Name} vs {matchup.Away.Name}, " +
            $"leg = {ticket.Legs[0].DisplayLabel}, stake = {Stake}, outcome = {ticket.Legs[0].State}");

        var events = new List<DramaEvent>();
        SweatSession session = run.Sweats[0];
        while (session.MoveNext(out var e)) events.Add(e);

        // p_0 — the leg's pre-event anchor DramaGenerator computes but never emits as a beat
        // (engine/DramaGenerator.cs:72-74) — is the correct "previous" probability for beat 1's delta.
        double prev = ticket.Legs[0].TrueProb;
        var tally = new Dictionary<(DramaEventType Type, TensionTag Tag), int>();

        foreach (DramaEvent e in events)
        {
            double absDelta = Math.Abs(e.WinProbAfter - prev);
            _output.WriteLine($"{LogPrefix} {label} beat step={e.Step,3}/{e.TotalSteps,-3} " +
                $"type={e.Type,-8} tag={e.Tag,-10} winProbAfter={e.WinProbAfter:F4} absDelta={absDelta:F4}");
            prev = e.WinProbAfter;

            var key = (e.Type, e.Tag);
            tally[key] = tally.TryGetValue(key, out int c) ? c + 1 : 1;
        }

        _output.WriteLine($"{LogPrefix} {label} tally by (type, tag):");
        foreach (var kv in tally.OrderBy(p => p.Key.Type).ThenBy(p => p.Key.Tag))
            _output.WriteLine($"{LogPrefix} {label}   {kv.Key.Type,-8} / {kv.Key.Tag,-10} = {kv.Value}");

        int totalBeats = events.Count;
        int momentumBeats = events.Count(e => e.Type == DramaEventType.Momentum);
        int momentumCalmBeats = events.Count(e => e.Type == DramaEventType.Momentum && e.Tag == TensionTag.Calm);

        _output.WriteLine($"{LogPrefix} {label} totalBeats={totalBeats} momentumBeats={momentumBeats} " +
            $"momentumCalmBeats={momentumCalmBeats}");

        return new ArmResult(label, events, totalBeats, momentumBeats, momentumCalmBeats);
    }

    // RUN 2026-08-17 by the lead with the Skip temporarily removed; the measurement is docked in
    // docs/handoffs/tv-theater.md. Restored to Skip so it stays an opt-in instrument, like every
    // other evidence pin on this surface, and so the engine baseline stays 306.
    [Fact(Skip = "Manual diagnostic probe, not a regression test — see the class doc comment for why " +
        "Skip (xUnit has no [Explicit]) and exactly how to run it on demand.")]
    public void Momentum_beats_tagged_Calm_are_measured_directly_for_both_arms()
    {
        var cornersRun = new Run(Seed);
        var goalsRun = new Run(Seed);

        // Guards the PROBE, not the finding: if a seed typo ever threaded a different seed into one
        // arm, the two slates would not match and this says so structurally, rather than the two arms
        // silently comparing different matches. Re-asserting the shared `Seed` constant against itself
        // would prove nothing — this checks what the seed actually produced.
        Assert.Equal(cornersRun.CurrentSlate.Matchups.Count, goalsRun.CurrentSlate.Matchups.Count);
        for (int i = 0; i < cornersRun.CurrentSlate.Matchups.Count; i++)
        {
            Assert.Equal(cornersRun.CurrentSlate.Matchups[i].Home.Name, goalsRun.CurrentSlate.Matchups[i].Home.Name);
            Assert.Equal(cornersRun.CurrentSlate.Matchups[i].Away.Name, goalsRun.CurrentSlate.Matchups[i].Away.Name);
        }

        // THE CONTROL RIDES THE SAME MATCH. The docked pair's own harness pins the goals leg to the
        // CORNERS matchup and filters for Over (TvSweatCaptureHarness.cs:1578-1589, "the same match
        // and the pair would not be a control"), so one variable separates the arms: the market. An
        // independently-chosen goals matchup would be a different fixture, a different probability
        // path, and no control at all — the first cut of this probe did exactly that.
        (int cornersMatchup, _) = FirstOffer(cornersRun.CurrentSlate, MarketKind.TotalCorners);
        Assert.True(cornersMatchup >= 0,
            $"{LogPrefix} no matchup on seed '{Seed}' offers TotalCorners — a reported result.");

        ArmResult corners = RunArm(cornersRun, MarketKind.TotalCorners, "CORNERS", cornersMatchup);
        ArmResult goals = RunArm(goalsRun, MarketKind.TotalGoals, "GOALS", cornersMatchup, MarketChoice.Over);

        _output.WriteLine($"{LogPrefix} ---- comparison (the section-8 count) ----");
        _output.WriteLine($"{LogPrefix} Momentum+Calm beats:  CORNERS={corners.MomentumCalmBeats}   " +
            $"GOALS={goals.MomentumCalmBeats}");

        // Only what cannot be false without the probe itself being broken. Whether Momentum+Calm
        // beats exist on the corners arm is the §8 finding this probe exists to collect — asserting a
        // nonzero count (or a zero one) would presuppose the answer and break the probe the moment the
        // real answer was the other one.
        Assert.NotEmpty(corners.Events);
        Assert.NotEmpty(goals.Events);
    }
}
