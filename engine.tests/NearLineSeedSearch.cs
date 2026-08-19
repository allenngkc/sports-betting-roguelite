using System;
using System.Collections.Generic;
using System.Linq;
using SBR.Engine;

namespace SBR.Engine.Tests;

/// <summary>
/// NEAR-LINE SEED SEARCH — a manual instrument, not a ruling.
///
/// <para>spec-count-theater-2026-08-17.md §8 item 3 owes a near-line watch: <i>"a leg that lands
/// close to its line, or loses. Every frame we hold is a comfortable winner, and the ramp's whole
/// value is in the case we have never seen."</i> The Design Director has asked for two shots and
/// named no seed for either:</para>
/// <list type="bullet">
/// <item><description><b>A</b> — an OVER corners leg that FAILS TO CROSS, ideally exactly one
/// short of its line, so the watch reaches the approach and never gets its turn.</description></item>
/// <item><description><b>B</b> — the UNDER mirror: an UNDER corners leg near its
/// allowance.</description></item>
/// </list>
///
/// <para><b>This is a SEARCH. It rules nothing and shoots nothing.</b> It prints one
/// <c>[NEAR-LINE]</c>-prefixed line per direction found on each candidate seed, then a SUMMARY
/// naming the closest-to-the-line candidate for A and for B, by smallest absolute margin. The lead
/// reads the table and picks two seeds; nothing here commits to either.</para>
///
/// <para><b>A and B are not judged by the same rule.</b> A's own wording is a hard requirement —
/// "FAILS TO CROSS" — so its candidate pool is filtered to OVER findings that actually lost
/// (margin &lt; 0) before ranking by proximity; an OVER leg that cleared, however narrowly, is not
/// a miss and is excluded rather than mis-scored as a good A. B's wording carries no such
/// requirement — "near its allowance" reads as proximity to the boundary from either side — so B's
/// pool is every UNDER finding, win or lose, ranked the same way. A margin-0 UNDER (held with
/// nothing to spare) is therefore a legitimate best-B exactly as a margin-(-1) UNDER (busted by
/// one) would be. This choice is the search's own reading of an ambiguous brief, not a ruling —
/// the lead can re-rank the printed table differently if they read "near its allowance"
/// otherwise.</para>
///
/// <para><b>Call order</b> mirrors <see cref="CalmBeatReachabilityProbe"/>, this project's
/// established idiom for driving <see cref="Run"/> by its own public API: construct
/// <c>new Run(seed)</c>, read offers OFF <see cref="Matchup.Markets"/> (engine/Domain.cs:384)
/// before locking, <see cref="Run.PlaceTicket"/> (engine/Run.cs:184), then
/// <see cref="Run.LockRound"/> (engine/Run.cs:382). Selections are NEVER constructed —
/// <see cref="Matchup"/>'s selection index throws "Market selection is not offered" for anything
/// not read off the board first (engine/Domain.cs:526), which has already cost this lane a run. A
/// seed whose slate never prices <see cref="MarketKind.TotalCorners"/> at all — or prices only one
/// of Over/Under — is a reported result, printed by name, never a reason to invent the missing
/// side.</para>
///
/// <para><b>The arithmetic is SweatActiveLegModel's, reproduced rather than called.</b>
/// <c>unity/SBR/Assets/SBR/Runtime/SweatActiveLegModel.cs:629-640</c> defines
/// <c>HalfLineThreshold</c> (OVER clears at <c>floor(line) + 1</c>) and <c>HalfLineMaxAllowed</c>
/// (UNDER holds at <c>floor(line)</c>) — but that file is Unity runtime code, and
/// <c>engine.tests/SBR.Engine.Tests.csproj</c> references only <c>engine/SBR.Engine.csproj</c> plus
/// one explicitly Unity-free source file (<c>BetslipModel.cs</c>), so it is not reachable from
/// here. The same two formulas are re-typed below, not re-derived: they match
/// <c>MatchModel.Compare</c>'s own grading (<c>engine/MatchModel.cs:873-878</c>, <c>Over</c> wins
/// iff <c>value &gt; line</c>, <c>Under</c> wins iff <c>value &lt; line</c>), which is exactly
/// <c>value &gt;= floor(line)+1</c> / <c>value &lt;= floor(line)</c> for the half-integer lines
/// this engine actually offers (<c>RunConfig.CornerLines</c> defaults to 8.5/9.5/10.5 — see
/// <c>engine/RunConfig.cs:68</c> — so no whole-number push case exists on the default board this
/// search runs against).</para>
///
/// <para><b>The match's own total</b> is <c>leg.Matchup.StatLine.HomeCorners +
/// StatLine.AwayCorners</c> (engine/Domain.cs:271-272), the same field
/// <c>CountLedger.ConfigureEndpoint</c> reads and <c>ScoreLedgerTests</c> already asserts against
/// (<c>unity/SBR/Assets/Tests/EditMode/ScoreLedgerTests.cs:707-708</c>).</para>
///
/// <para><b>What this does NOT do.</b> It never drains a <see cref="SweatSession"/> — no beat
/// stream is read, so nothing here "runs a sweat" in this codebase's own sense of the word
/// (<see cref="Phase.Sweat"/>, <see cref="Run.Sweats"/>). It never asserts that a near-line seed
/// exists for A or B: whether one does, among these ~40 candidates, IS the finding this search
/// exists to collect, and an assertion that presupposes the answer would destroy the evidence the
/// moment the real answer was "none here" (see the single assertion at the bottom, which checks
/// only that the search mechanism itself worked). It also never counts corners EVENTS — see the
/// per-seed print below and the paragraph after this one for why that number is named absent
/// rather than guessed.</para>
///
/// <para><b>THE NAMED GAP — corners EVENTS.</b> The task asks, per seed, for the number of corners
/// events if obtainable without running a sweat. It is not obtainable from here. In this engine a
/// corners LEG's progressive on-screen count comes from <c>CountLedger</c>
/// (<c>unity/SBR/Assets/SBR/Runtime/SweatPresentationModel.cs:501</c>), which distributes the
/// match's final corner total across a subset of narrative beats
/// (<c>TheaterChoreographer.ResolveBeat</c> callers pass it a <c>beatCount</c> — see
/// <c>ScoreLedgerTests.cs:476</c>, <c>:502</c> for the shape of that call). Both types are Unity
/// runtime code this project's own <c>.csproj</c> does not compile (see the call-order paragraph
/// above) — and even if they were reachable, deriving a beat-level count would mean building a
/// <see cref="DramaGenerator"/> path and a live <c>CountLedger</c>, i.e. running a sweat, which the
/// task asks this search to avoid. <see cref="MatchStatLine"/> itself stores only the FINAL corner
/// total, never a per-event log (engine/Domain.cs:267-280) — there is no cheaper number hiding in
/// the engine to read instead. So: not established, left out, named here rather than guessed.</para>
/// </summary>
public class NearLineSeedSearch
{
    private readonly Xunit.Abstractions.ITestOutputHelper _output;

    public NearLineSeedSearch(Xunit.Abstractions.ITestOutputHelper output) => _output = output;

    private const double Stake = 25.0;
    private const string LogPrefix = "[NEAR-LINE]";

    /// <summary>~40 candidate seeds, invented and varied, plus the two knowns named in the brief
    /// (<c>CORNERS-SWEAT-1</c> — the docked capture seed, <c>TvSweatCaptureHarness.cs</c> — and
    /// <c>STATS-COUNT-1</c>). None of these is known in advance to be near-line for either
    /// direction; that is exactly what the search below is for.</summary>
    private static readonly string[] CandidateSeeds =
    {
        "CORNERS-SWEAT-1", "STATS-COUNT-1",
        "NEAR-LINE-01", "NEAR-LINE-02", "NEAR-LINE-03", "NEAR-LINE-04",
        "NEAR-LINE-05", "NEAR-LINE-06", "NEAR-LINE-07", "NEAR-LINE-08",
        "APPROACH-WATCH-1", "APPROACH-WATCH-2", "APPROACH-WATCH-3",
        "QUIET-CORNER-1", "QUIET-CORNER-2", "QUIET-CORNER-3",
        "RAMP-SEED-01", "RAMP-SEED-02", "RAMP-SEED-03", "RAMP-SEED-04",
        "DD-SHOT-A", "DD-SHOT-B", "DD-SHOT-C", "DD-SHOT-D",
        "TV-SWEAT-2", "TV-SWEAT-3", "TV-SWEAT-4", "TV-SWEAT-5",
        "BOARD-SIDE-01", "BOARD-SIDE-02", "BOARD-SIDE-03",
        "FLOOR-PLUS-ONE-1", "FLOOR-PLUS-ONE-2",
        "CAP-HOLD-1", "CAP-HOLD-2", "CAP-HOLD-3",
        "SECOND-OVERRUN-1", "S80-NEAR-1", "H112-NEAR-1", "T113-CORNERS-1",
    };

    private enum Direction { Over, Under }

    /// <summary>One direction's finding on one seed — everything the SUMMARY ranks on.</summary>
    private sealed class Finding
    {
        public string Seed = "";
        public int MatchupIndex;
        public Direction Direction;
        public double Line;
        public int MatchTotal;
        public int DecidingNumber; // threshold for Over, maxAllowed for Under
        public int Margin;
        public LegState State;
        public string Tag = "";
    }

    /// <summary>The first matchup on the slate, in slate order, offering ANY TotalCorners
    /// selection (either choice) — read OFF <see cref="Matchup.Markets"/>, never constructed.
    /// -1 when the slate offers none, which the caller reports rather than substituting another
    /// market.</summary>
    private static int FirstMatchupOfferingCorners(Slate slate)
    {
        foreach (Matchup m in slate.Matchups)
            foreach (MarketOffer offer in m.Markets)
                if (offer.Selection.Kind == MarketKind.TotalCorners)
                    return m.Index;
        return -1;
    }

    /// <summary>The first TotalCorners offer on ONE matchup at the given Over/Under choice — read
    /// OFF the board, never constructed. False when this matchup prices no such direction, which
    /// the caller reports rather than inventing a line.</summary>
    private static bool FirstCornersOffer(Matchup matchup, MarketChoice choice, out MarketSelection selection)
    {
        foreach (MarketOffer offer in matchup.Markets)
        {
            if (offer.Selection.Kind != MarketKind.TotalCorners) continue;
            if (offer.Selection.Choice != choice) continue;
            selection = offer.Selection;
            return true;
        }
        selection = default;
        return false;
    }

    /// <summary>spec-count-theater-2026-08-17.md §3 distance-to-the-line arithmetic, OVER side:
    /// margin = matchTotal - threshold (0 = cleared exactly, negative = short).</summary>
    private static string ClassifyOver(int margin)
    {
        if (margin >= 0) return "OVER-CLEARS";
        if (margin == -1) return "OVER-MISS-BY-1";
        if (margin == -2) return "OVER-MISS-BY-2";
        return "OTHER";
    }

    /// <summary>UNDER side: margin = maxAllowed - matchTotal (0 = held with nothing to spare,
    /// negative = busted).</summary>
    private static string ClassifyUnder(int margin)
    {
        if (margin == 0) return "UNDER-HOLDS-EXACTLY";
        if (margin == -1) return "UNDER-BUSTS-BY-1";
        if (margin > 0) return "UNDER-COMFORTABLE";
        return "OTHER";
    }

    /// <summary>Run this on demand with
    /// <c>dotnet test engine.tests --filter "FullyQualifiedName~NearLineSeedSearch"</c> to see the
    /// full table on stdout; it is a plain (non-Skip) Fact — like its nearest sibling,
    /// <see cref="GoallessDrawSeedTests"/> — because its only assertion is structural (the search
    /// mechanism produced a corners market at all) and cannot flip on the near-line finding
    /// itself, so it is safe to leave in the routine suite.</summary>
    [Fact]
    public void Search_for_near_line_corners_seeds_for_the_DDs_two_shots()
    {
        var findings = new List<Finding>();
        int seedsWithCornersMarket = 0;

        foreach (string seed in CandidateSeeds)
        {
            var run = new Run(seed);
            int matchupIndex = FirstMatchupOfferingCorners(run.CurrentSlate);
            if (matchupIndex < 0)
            {
                _output.WriteLine($"{LogPrefix} seed={seed,-22} NO TotalCorners MARKET on this " +
                    "slate -- reported, never a reason to substitute another market");
                continue;
            }
            seedsWithCornersMarket++;
            Matchup matchup = run.CurrentSlate.Matchups[matchupIndex];

            bool haveOver = FirstCornersOffer(matchup, MarketChoice.Over, out MarketSelection overSel);
            bool haveUnder = FirstCornersOffer(matchup, MarketChoice.Under, out MarketSelection underSel);

            // Corners EVENTS: NOT ESTABLISHED for every seed, structurally -- see the class doc
            // comment's "THE NAMED GAP" paragraph. Printed once per seed candidate as asked,
            // rather than guessed or fabricated.
            _output.WriteLine($"{LogPrefix} seed={seed,-22} matchup=#{matchupIndex} " +
                $"{matchup.Home.Name} vs {matchup.Away.Name} -- Over offered={haveOver} " +
                $"Under offered={haveUnder} -- corners EVENTS: NOT ESTABLISHED (Unity-only " +
                "CountLedger, unreachable from engine.tests -- see class doc comment)");

            if (!haveOver)
                _output.WriteLine($"{LogPrefix} seed={seed,-22} matchup=#{matchupIndex}: no Over " +
                    "TotalCorners offer on this matchup -- reported, never substituted");
            if (!haveUnder)
                _output.WriteLine($"{LogPrefix} seed={seed,-22} matchup=#{matchupIndex}: no Under " +
                    "TotalCorners offer on this matchup -- reported, never substituted");

            // Believed unreachable -- FirstMatchupOfferingCorners only returned this index because
            // SOME offer here has Kind == TotalCorners, and TotalCorners offers are only ever
            // priced Over or Under (MatchModel.Compare requires one of the two). Kept as a named
            // defensive branch rather than assumed away.
            if (!haveOver && !haveUnder)
            {
                _output.WriteLine($"{LogPrefix} seed={seed,-22} matchup=#{matchupIndex}: offered " +
                    "TotalCorners but neither Over nor Under choice was found -- unexpected, skipping");
                continue;
            }

            Ticket? overTicket = haveOver
                ? run.PlaceTicket(new[] { new Pick(matchupIndex, overSel) }, Stake)
                : null;
            Ticket? underTicket = haveUnder
                ? run.PlaceTicket(new[] { new Pick(matchupIndex, underSel) }, Stake)
                : null;

            run.LockRound();

            MatchStatLine? statLine = matchup.StatLine;
            if (statLine == null)
            {
                // LockRound sets StatLine for every matchup on the slate unconditionally
                // (engine/Run.cs:397-401), bet or not -- this should be unreachable. Named rather
                // than assumed, matching this file's own "report, don't invent" rule for itself.
                _output.WriteLine($"{LogPrefix} seed={seed,-22} matchup=#{matchupIndex}: StatLine " +
                    "is null after LockRound -- unexpected, skipping");
                continue;
            }
            int matchTotal = statLine.HomeCorners + statLine.AwayCorners;

            if (overTicket != null)
            {
                Leg leg = overTicket.Legs[0];
                int threshold = (int)Math.Floor(overSel.Line) + 1;
                int margin = matchTotal - threshold;
                string tag = ClassifyOver(margin);
                _output.WriteLine($"{LogPrefix} seed={seed,-22} matchup=#{matchupIndex} OVER  " +
                    $"line={overSel.Line,5:0.0} matchTotal={matchTotal,3} threshold={threshold,3} " +
                    $"margin={margin,4} state={leg.State,-7} tag={tag}");
                findings.Add(new Finding
                {
                    Seed = seed, MatchupIndex = matchupIndex, Direction = Direction.Over,
                    Line = overSel.Line, MatchTotal = matchTotal, DecidingNumber = threshold,
                    Margin = margin, State = leg.State, Tag = tag,
                });
            }
            if (underTicket != null)
            {
                Leg leg = underTicket.Legs[0];
                int maxAllowed = (int)Math.Floor(underSel.Line);
                int margin = maxAllowed - matchTotal;
                string tag = ClassifyUnder(margin);
                _output.WriteLine($"{LogPrefix} seed={seed,-22} matchup=#{matchupIndex} UNDER " +
                    $"line={underSel.Line,5:0.0} matchTotal={matchTotal,3} maxAllowed={maxAllowed,3} " +
                    $"margin={margin,4} state={leg.State,-7} tag={tag}");
                findings.Add(new Finding
                {
                    Seed = seed, MatchupIndex = matchupIndex, Direction = Direction.Under,
                    Line = underSel.Line, MatchTotal = matchTotal, DecidingNumber = maxAllowed,
                    Margin = margin, State = leg.State, Tag = tag,
                });
            }
        }

        // ---------------------------------------------------------------------------- SUMMARY
        _output.WriteLine($"{LogPrefix} ---- SUMMARY ----");
        _output.WriteLine($"{LogPrefix} seeds searched             : {CandidateSeeds.Length}");
        _output.WriteLine($"{LogPrefix} seeds with a corners market : {seedsWithCornersMarket}");
        _output.WriteLine($"{LogPrefix} OVER findings               : {findings.Count(f => f.Direction == Direction.Over)}");
        _output.WriteLine($"{LogPrefix} UNDER findings              : {findings.Count(f => f.Direction == Direction.Under)}");

        // A: an OVER leg that FAILS TO CROSS -- margin < 0 is not optional here, see the class
        // doc comment's "A and B are not judged by the same rule" paragraph. Among losses only,
        // closest to the line wins (smallest |margin|), which naturally prefers MISS-BY-1 over
        // MISS-BY-2 without hard-coding the tag.
        List<Finding> overMisses = findings.Where(f => f.Direction == Direction.Over && f.Margin < 0).ToList();
        if (overMisses.Count > 0)
        {
            Finding best = overMisses.OrderBy(f => Math.Abs(f.Margin)).First();
            _output.WriteLine($"{LogPrefix} BEST FOR A (OVER fails to cross, smallest |margin|): " +
                $"seed={best.Seed} matchup=#{best.MatchupIndex} line={best.Line:0.0} " +
                $"matchTotal={best.MatchTotal} threshold={best.DecidingNumber} margin={best.Margin} " +
                $"state={best.State} tag={best.Tag}");
        }
        else
        {
            _output.WriteLine($"{LogPrefix} BEST FOR A: NONE -- no OVER leg in this seed list " +
                "failed to cross its line. Widen the candidate list rather than reading a clear " +
                "as a miss.");
        }

        // B: the UNDER mirror -- "near its allowance" is read as proximity from either side of the
        // boundary (no loss requirement), so this pool is every UNDER finding. See the class doc
        // comment for why A and B are deliberately not filtered the same way.
        List<Finding> underAll = findings.Where(f => f.Direction == Direction.Under).ToList();
        if (underAll.Count > 0)
        {
            Finding best = underAll.OrderBy(f => Math.Abs(f.Margin)).First();
            _output.WriteLine($"{LogPrefix} BEST FOR B (UNDER nearest its allowance, smallest " +
                $"|margin|): seed={best.Seed} matchup=#{best.MatchupIndex} line={best.Line:0.0} " +
                $"matchTotal={best.MatchTotal} maxAllowed={best.DecidingNumber} margin={best.Margin} " +
                $"state={best.State} tag={best.Tag}");
        }
        else
        {
            _output.WriteLine($"{LogPrefix} BEST FOR B: NONE -- no UNDER offer was found on any " +
                "candidate seed's corners matchup. Widen the candidate list rather than reading " +
                "absence as a finding.");
        }

        // Only what cannot be false without the search itself being broken. Whether a near-line
        // seed exists for A or for B is the finding this search exists to collect (see BEST FOR A
        // / BEST FOR B above) -- asserting either would presuppose the answer and break the search
        // the moment the real answer was "none in this list". This checks only that the mechanism
        // worked at all: SOME candidate seed's slate priced TotalCorners.
        Assert.True(seedsWithCornersMarket > 0,
            $"{LogPrefix} none of the {CandidateSeeds.Length} candidate seeds offered " +
            "TotalCorners at all -- widen the candidate list before concluding anything about " +
            "near-line proximity.");
    }
}
