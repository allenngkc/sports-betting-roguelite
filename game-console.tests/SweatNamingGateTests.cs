using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using SBR.Engine;
using SBR.Game;
using Xunit.Abstractions;

namespace SBR.ConsoleGame.Tests;

/// <summary>
/// THE SWEAT'S NAMING GATE — <c>K16</c> and spec-console-surfaces-2026-08-19 §9.3.
///
/// <para><b>What it exists to stop coming back.</b> <c>EventText.Prefix</c> composed
/// <c>$"[{leg.Selection.Kind} {leg.Selection.Line:0.0} {leg.Selection.Choice}] "</c> and put it on
/// every beat of every non-moneyline sweat. Both ends were <c>.ToString()</c>d, so the player read a
/// raw C# enum identifier — <c>[TotalCorners 9.5 Over] the flag goes up</c> — eight beats running,
/// on a verified frame (DD batch 144). Three standing laws at once: <c>S22</c> (the bracketed tag
/// was STRUCK on 2026-07-31), <c>T39</c>/<c>T98</c> (one casing per line — camel case inside
/// sentence case), and <c>T130</c>'s class (a C# identifier reaching the player). §9.3 is the same
/// surface's other half: the sweat named a leg <c>LEG 1: ✘ DEAD</c> and neither leg's market
/// appeared anywhere in it.</para>
///
/// <para><b>Why the assertions below are structural rather than a list of strings.</b> The forbidden
/// set is derived from <c>Enum.GetNames(typeof(MarketKind))</c> and <c>Enum.GetNames(typeof(MarketChoice))</c>
/// at runtime, so a sixteenth kind is in the gate the moment it is in the enum — nobody has to
/// remember to add it. That matters here more than usual: <c>K16</c>'s twin on the TV
/// (<c>T130</c>/<c>T130-vf</c>) was a <c>default:</c> arm returning <c>Kind.ToString()</c>, and both
/// were invisible for as long as the offending kinds were unbettable. <b>This commit opens the pick
/// grammar to all fifteen kinds</b>, so a gate that only knows the six that used to be reachable
/// would prove nothing about the nine that just became reachable.</para>
///
/// <para><b>C29.</b> Every case count this file measures is printed and asserted non-zero: a sweep
/// that swept nothing is a failed run, not a pass.</para>
/// </summary>
public class SweatNamingGateTests
{
    private readonly ITestOutputHelper _output;

    public SweatNamingGateTests(ITestOutputHelper output) => _output = output;

    /// <summary>Several seeds, per §13's *sweep the population, not the suspects*. Fixed strings so
    /// a failure is reproducible from the report alone.</summary>
    private static readonly string[] Seeds = { "GATE-K16-A", "GATE-K16-B", "GATE-K16-C", "GATE-K16-D" };

    private static IReadOnlyList<MarketKind> AllKinds =>
        (MarketKind[])Enum.GetValues(typeof(MarketKind));

    // ---- the forbidden set, derived rather than typed ----

    /// <summary>
    /// Every C# identifier the two enums can produce. Matched as a WHOLE WORD and CASE-SENSITIVELY,
    /// which is what makes the check exact instead of merely noisy: <c>Over</c> must not match the
    /// market vocabulary's <c>OVER</c> (a different casing is a different string, and <c>OVER 9.5
    /// CORNERS</c> is the surface's own words), and it must not match the club noun
    /// <c>Overheads</c> (a longer word that begins with the same letters). Both of those appear in
    /// real sweat lines, so a sloppier matcher would fail on correct output and get relaxed —
    /// which is how a gate stops gating.
    /// </summary>
    private static IReadOnlyList<string> EnumIdentifiers()
    {
        var names = new List<string>();
        names.AddRange(Enum.GetNames(typeof(MarketKind)));
        names.AddRange(Enum.GetNames(typeof(MarketChoice)));
        return names.Distinct().OrderBy(s => s, StringComparer.Ordinal).ToList();
    }

    private static string? FirstIdentifierIn(string line)
    {
        foreach (string id in EnumIdentifiers())
        {
            if (Regex.IsMatch(line, @"(?<![A-Za-z0-9])" + Regex.Escape(id) + @"(?![A-Za-z0-9])"))
                return id;
        }
        return null;
    }

    // ---- what a rendered sweat line is ----

    private enum LineKind { Beat, Meter, Verdict }

    private sealed class Rendered
    {
        public Rendered(LineKind kind, string text, int legIndex, MarketKind market, string seed)
        {
            Kind = kind;
            Text = text;
            LegIndex = legIndex;
            Market = market;
            Seed = seed;
        }

        public LineKind Kind { get; }
        public string Text { get; }
        public int LegIndex { get; }
        public MarketKind Market { get; }
        public string Seed { get; }
    }

    // =====================================================================================
    // 1. THE REAL SWEAT — every line a multi-leg sweat renders, over several seeds, with all
    //    fifteen kinds reached.
    // =====================================================================================

    [Fact]
    public void No_rendered_sweat_line_carries_a_raw_enum_identifier_or_the_struck_bracketed_tag()
    {
        List<Rendered> lines = SweepRealSweats(out HashSet<MarketKind> sweated, out int sessions);

        _output.WriteLine($"seeds        : {Seeds.Length}");
        _output.WriteLine($"sweat sessions stepped : {sessions}");
        _output.WriteLine($"rendered lines swept   : {lines.Count}");
        _output.WriteLine($"market kinds SWEATED   : {sweated.Count} of {AllKinds.Count}");
        _output.WriteLine("  " + string.Join(", ", sweated.OrderBy(k => (int)k).Select(k => k.ToString())));

        // C29 — a sweep that swept nothing proves nothing.
        Assert.True(sessions > 0, "C29: no sweat session was stepped");
        Assert.True(lines.Count > 0, "C29: no rendered line was collected");

        // The gate's whole premise: this commit opens the grammar to all fifteen kinds, so the
        // assertions below must have actually run over all fifteen. Reported by name on failure
        // rather than left as a count, because WHICH kind went unreached is the finding.
        List<MarketKind> missed = AllKinds.Where(k => !sweated.Contains(k)).ToList();
        Assert.True(missed.Count == 0,
            "these market kinds never reached a real sweat, so nothing below was asserted about "
            + "them: " + string.Join(", ", missed));

        var offenders = new List<string>();
        foreach (Rendered line in lines)
        {
            string? id = FirstIdentifierIn(line.Text);
            if (id != null)
                offenders.Add($"[{line.Seed} {line.Market} {line.Kind}] identifier '{id}' in: {line.Text}");
        }
        AssertNone(offenders, "K16/T130: a C# enum identifier reached a rendered sweat line");

        // S22 — the bracketed tag. Scoped to its FORM rather than to the character: the meter line's
        // bar ([####----]) and its cash-out control ([C]) are this surface's own vocabulary and were
        // never the tag. What is struck is a bracketed run naming a market, and the beat line — the
        // line the tag lived on — carries no bracket at all now.
        var bracketed = new List<string>();
        foreach (Rendered line in lines)
        {
            foreach (Match m in Regex.Matches(line.Text, @"\[[^\]]*\]"))
            {
                string inner = m.Value;
                if (FirstIdentifierIn(inner) != null || Regex.IsMatch(inner, @"[0-9]+\.[0-9]"))
                    bracketed.Add($"[{line.Seed} {line.Market} {line.Kind}] bracketed tag {inner} in: {line.Text}");
            }
            if (line.Kind == LineKind.Beat && line.Text.Contains('['))
                bracketed.Add($"[{line.Seed} {line.Market}] a beat line carries a bracket: {line.Text}");
        }
        AssertNone(bracketed, "S22: the struck bracketed tag is back on a rendered sweat line");
    }

    /// <summary>
    /// §9.3 — <b>a leg is named when its state changes</b>, in the same name the ledger prints
    /// (<c>T69</c>/<c>T70</c>). Asserted two ways: the line is not the bare ordinal it used to be,
    /// and the name on it is <see cref="MarketSheet"/>'s own — read back out of the composer rather
    /// than compared against a string this test authored, which is how §13 gate 6's composer parity
    /// is kept honest.
    /// </summary>
    [Fact]
    public void A_legs_state_change_names_its_market_and_never_stands_on_its_ordinal_alone()
    {
        List<Rendered> lines = SweepRealSweats(out _, out _);
        List<Rendered> verdicts = lines.Where(l => l.Kind == LineKind.Verdict).ToList();

        _output.WriteLine($"state changes rendered : {verdicts.Count}");
        foreach (Rendered v in verdicts.Take(8)) _output.WriteLine("  " + v.Text);
        Assert.True(verdicts.Count > 0, "C29: no leg state change was rendered");

        var bare = new List<string>();
        foreach (Rendered v in verdicts)
        {
            // The struck form, exactly: "LEG 1: ✘ DEAD" — an ordinal, a colon, a verdict, no market.
            foreach (string verdict in SweatLines.Verdicts)
            {
                if (v.Text.Trim() == $"{SweatLines.LegAddress(v.LegIndex)}: {verdict}")
                    bare.Add($"[{v.Seed}] leg named by its ordinal alone: {v.Text}");
            }

            string body = v.Text.Trim();
            int colon = body.IndexOf(':');
            Assert.True(colon > 0, $"a verdict line has no leg label: {v.Text}");
            int sep = body.LastIndexOf(SweatLines.Separator, StringComparison.Ordinal);
            Assert.True(sep > colon, $"a verdict line has no verdict: {v.Text}");

            string named = body.Substring(colon + 1, sep - colon - 1).Trim();
            Assert.False(string.IsNullOrWhiteSpace(named), $"a verdict line names nothing: {v.Text}");
            Assert.Null(FirstIdentifierIn(named));
        }
        AssertNone(bare, "spec §9.3: a leg's state change still prints a bare ordinal");
    }

    /// <summary>
    /// <c>T39</c>'s other half — <b>one dash per line</b>, and it is the em dash. This is the arm
    /// that caught a real defect in this very change: the first draft of the verdict line joined the
    /// name to the verdict with an em dash, which reads correctly for fourteen kinds and prints
    /// <c>LEG 1: BTTS — YES — ✘ DEAD</c> for the fifteenth, because <c>MatchModel.Fields</c> composes
    /// BTTS's name with a dash already in it. A rule asserted over all fifteen kinds found it; reading
    /// the line for the kind in front of me would not have.
    /// </summary>
    [Fact]
    public void No_rendered_sweat_line_carries_more_than_one_em_dash()
    {
        List<Rendered> lines = SweepRealSweats(out _, out _);
        var offenders = new List<string>();
        int dashed = 0;

        foreach (Rendered line in lines)
        {
            int n = line.Text.Count(ch => ch == '—');
            if (n > 0) dashed++;
            if (n > 1) offenders.Add($"[{line.Seed} {line.Market} {line.Kind}] {n} em dashes: {line.Text}");
        }

        _output.WriteLine($"lines swept : {lines.Count}   lines carrying an em dash : {dashed}");
        Assert.True(lines.Count > 0, "C29: nothing was swept");
        AssertNone(offenders, "T39: one dash per line");
    }

    // =====================================================================================
    // 2. THE COMPOSER, ACROSS ALL FIFTEEN KINDS — parity with MarketSheet and the casing rule.
    // =====================================================================================

    /// <summary>
    /// §13 gate 6 (composer parity) applied to the sweat: for every kind, the name the sweat prints
    /// at a state change is <c>MarketSheet</c>'s row name, uppercased at the presentation layer
    /// exactly as <c>RowGeometry.OfferRow</c> does it (<c>S96</c>, §6.5). <b>Read off the sheet, not
    /// off a table in this file</b> — a hand-written expectation would be testing its own
    /// transcription, which is the failure <c>PoolGateTests</c> already names.
    /// </summary>
    [Fact]
    public void Every_market_kinds_leg_name_is_MarketSheets_own_name_uppercased()
    {
        int checkedRows = 0;
        var seen = new HashSet<MarketKind>();
        var offenders = new List<string>();

        foreach (string seed in Seeds)
        {
            var run = new Run(seed);
            foreach (Matchup m in run.CurrentSlate.Matchups)
            {
                MarketSheet sheet = MarketSheet.Build(m);
                foreach (MarketSheetRow row in sheet.AllRows)
                {
                    string composed = SweatLines.LegName(m, row.Offer.Selection);
                    seen.Add(row.Kind);
                    checkedRows++;

                    if (composed != row.Name.ToUpperInvariant())
                        offenders.Add($"{row.Kind}: sweat '{composed}' vs sheet '{row.Name.ToUpperInvariant()}'");
                    if (composed.Length == 0)
                        offenders.Add($"{row.Kind}: the sweat would name a leg with an empty string");
                    string? id = FirstIdentifierIn(composed);
                    if (id != null) offenders.Add($"{row.Kind}: identifier '{id}' in leg name '{composed}'");
                }
            }
        }

        _output.WriteLine($"sheet rows composed through SweatLines.LegName : {checkedRows}");
        _output.WriteLine($"market kinds covered : {seen.Count} of {AllKinds.Count}");
        Assert.True(checkedRows > 0, "C29: no row was composed");
        AssertNone(offenders, "§13 gate 6: the sweat and the sheet disagree about a market's name");

        List<MarketKind> missed = AllKinds.Where(k => !seen.Contains(k)).ToList();
        Assert.True(missed.Count == 0,
            "these kinds were never priced on any seed, so their leg name is unasserted: "
            + string.Join(", ", missed));
    }

    /// <summary>
    /// <b>THE CASING RULE, AS CHOSEN AND AS ASSERTED.</b> <c>T39</c>/<c>T98</c> is *one casing per
    /// line*, and the market vocabulary is UPPERCASE (<c>S96</c>) while a beat is sentence case. The
    /// two therefore cannot share a line, and the name cannot be lowered to join it — the vocabulary
    /// holds an initialism (<c>BTTS — YES</c>) and proper nouns, so <c>ToLowerInvariant</c> would
    /// print <c>btts — yes</c> and <c>san francisco spreadsheets</c>, and authoring a second
    /// sentence-case vocabulary is what §6.6/<c>K8</c> forbid.
    ///
    /// <para><b>So the rule is: a sweat line is PROSE or it is DISPLAY, never both.</b> The two
    /// lines that carry the leg's identity — the meter line and the verdict line — hold <b>no
    /// lowercase letter at all</b>. The beat line holds prose and <b>no market tag of any kind</b>.
    /// That is the assertion below, and it is the one that makes the fix a rule rather than an
    /// edit.</para>
    ///
    /// <para><b>What this rule deliberately does NOT reach:</b> the DD's authored beat copy. <c>FINAL
    /// WHISTLE</c> and the scorer line's uppercased surname are inside <c>EventText</c>'s tables and
    /// are copy, not a composed tag; <c>K16</c> ruled on the tag and re-authoring copy without a
    /// ruling is what <c>A2</c>/<c>S22</c> forbid. The chrome lines around the sweat
    /// (<c>MULLIGAN — leg voided…</c>, <c>REVIEWED — the call is CONFIRMED.</c>) mix registers for
    /// the same reason and are reported in the lane's findings rather than silently swept in here.</para>
    /// </summary>
    [Fact]
    public void The_legs_identity_lines_carry_one_casing_and_the_beat_line_carries_no_market_tag()
    {
        List<Rendered> lines = SweepRealSweats(out _, out _);
        var offenders = new List<string>();
        int display = 0;
        int prose = 0;

        foreach (Rendered line in lines)
        {
            if (line.Kind == LineKind.Beat)
            {
                prose++;
                // No market tag: not the name the sheet would give this leg, and not an identifier
                // (already covered above, re-checked here because THIS is the casing claim's basis).
                foreach (string name in NamesOnSheetsFor(line.Seed))
                {
                    if (name.Length >= 4 && line.Text.Contains(name, StringComparison.Ordinal))
                        offenders.Add($"[{line.Seed} {line.Market}] a beat line carries the market name "
                            + $"'{name}' — that is a second casing on a sentence-case line: {line.Text}");
                }
                continue;
            }

            display++;
            foreach (char c in line.Text)
            {
                if (c >= 'a' && c <= 'z')
                {
                    offenders.Add($"[{line.Seed} {line.Market} {line.Kind}] lowercase '{c}' on an "
                        + $"identity line — two casings: {line.Text}");
                    break;
                }
            }
        }

        _output.WriteLine($"prose (beat) lines swept   : {prose}");
        _output.WriteLine($"display (identity) lines   : {display}");
        Assert.True(prose > 0 && display > 0, "C29: one of the two line registers was never rendered");
        AssertNone(offenders, "T39/T98: a sweat line carries two casings");
    }

    // =====================================================================================
    // 3. GEOMETRY — §13 gate 1, measured against the POOL (S84, S96-am), not against a seed.
    // =====================================================================================

    /// <summary>
    /// The widest constructible beat line and the widest constructible verdict line both fit §3's
    /// 80-column page — <b>and the struck prefix did not</b>, which is the second, independent
    /// reason it could not stay. Built from the enumerated pool exactly as <c>PoolGateTests</c>
    /// does it: <c>SlateGenerator</c>'s own noun array by reflection, and <c>EventText</c>'s own
    /// flavour tables by reflection, never a champion picked off a frame.
    /// </summary>
    [Fact]
    public void The_widest_constructible_sweat_lines_fit_the_page_and_the_struck_prefix_did_not()
    {
        const int PageColumns = 80;

        string widestNoun = ReflectPool("Nouns").OrderByDescending(s => s.Length).First();
        string widestTemplate = ReflectFlavourTables()
            .OrderByDescending(s => s.Replace("{picked}", widestNoun).Replace("{other}", widestNoun).Length)
            .First();
        string widestBeatText = widestTemplate.Replace("{picked}", widestNoun).Replace("{other}", widestNoun);

        // The widest name the composer can produce, taken from PoolGateTests' own measured champion
        // and RE-DERIVED here through the real sheet on a forced matchup would be a second copy of
        // that fixture; the number it lands on (44) is the one §3 is ruled on, so it is used as a
        // constant with its source named rather than re-measured a third time.
        const int WidestNameColumns = 44; // SAN FRANCISCO SPREADSHEETS UNDER 4.5 CORNERS — §3, S96-am
        string widestName = new string('X', WidestNameColumns);

        int widestBeat = SweatLines.Gutter("Q1 00:00").Length + widestBeatText.Length;
        int widestVerdict = SweatLines.LeftPad
            + SweatLines.LegAddress(9).Length + 2 + WidestNameColumns + SweatLines.Separator.Length
            + SweatLines.Verdicts.Max(v => v.Length);

        // The struck form, reconstructed by REPLAYING it over the (kind, choice, line) triples the
        // engine actually prices. NOT the enum cross product: `[TotalGoalsOddEven 10.5 HomeOrDraw]`
        // is 36 columns and is not a selection any board can carry, and sizing against a shape that
        // cannot exist is the same error S96-am corrects in the other direction. This measures what
        // players would really have seen.
        int widestStruckPrefix = 0;
        string widestStruck = string.Empty;
        foreach (string seed in Seeds)
        {
            var run = new Run(seed);
            foreach (Matchup m in run.CurrentSlate.Matchups)
            {
                foreach (MarketSheetRow row in MarketSheet.Build(m).AllRows)
                {
                    MarketSelection s = row.Offer.Selection;
                    if (s.Kind == MarketKind.Moneyline) continue; // the struck prefix was empty here
                    string tag = $"[{s.Kind} {s.Line:0.0} {s.Choice}] ";
                    if (tag.Length > widestStruckPrefix) { widestStruckPrefix = tag.Length; widestStruck = tag; }
                }
            }
        }
        Assert.True(widestStruckPrefix > 0, "C29: the struck prefix was replayed over no offer");
        int widestOldBeat = SweatLines.Gutter("Q1 00:00").Length + widestStruckPrefix + widestBeatText.Length;

        _output.WriteLine($"widest club noun            : {widestNoun} ({widestNoun.Length})");
        _output.WriteLine($"widest beat text            : {widestBeatText} ({widestBeatText.Length})");
        _output.WriteLine($"widest BEAT line    (now)   : {widestBeat} columns");
        _output.WriteLine($"widest VERDICT line (now)   : {widestVerdict} columns  (name field {WidestNameColumns})");
        _output.WriteLine($"widest STRUCK prefix        : {widestStruck.Trim()} ({widestStruckPrefix})");
        _output.WriteLine($"widest BEAT line   (struck) : {widestOldBeat} columns  <-- over the {PageColumns}-column page");

        Assert.True(widestBeat <= PageColumns,
            $"the widest constructible beat line is {widestBeat} columns against §3's {PageColumns}");
        Assert.True(widestVerdict <= PageColumns,
            $"the widest constructible verdict line is {widestVerdict} columns against §3's {PageColumns}");

        // The finding, asserted so it cannot be quietly reversed: the tag this commit removed could
        // not have been kept even if its casing and its vocabulary had been right.
        Assert.True(widestOldBeat > PageColumns,
            "the struck prefix is asserted here to have overflowed the page; if this now passes, "
            + "the page or the flavour tables changed and K16's geometric half needs re-deriving");

        // A real line from a real sweep, as the population check §13 gate 1 asks for.
        List<Rendered> lines = SweepRealSweats(out _, out _);
        Rendered widestSwept = lines.OrderByDescending(l => l.Text.Length).First();
        _output.WriteLine($"widest line actually swept  : {widestSwept.Text.Length} — {widestSwept.Text}");
        foreach (Rendered l in lines)
            Assert.True(l.Text.Length <= PageColumns, $"a rendered sweat line is {l.Text.Length} columns: {l.Text}");
    }

    // =====================================================================================
    // Construction — a real Run, real tickets, a real SweatSession stepped exactly as
    // SweatRenderer steps it. No Console is touched: SweatRenderer writes the SweatLines
    // composers segment by segment, so composing them here is composing what it writes.
    // =====================================================================================

    private List<Rendered> SweepRealSweats(out HashSet<MarketKind> sweated, out int sessions)
    {
        var lines = new List<Rendered>();
        sweated = new HashSet<MarketKind>();
        sessions = 0;

        foreach (string seed in Seeds)
        {
            // Three tickets a round (RunConfig.MaxTicketsPerRound) and four legs a ticket
            // (MaxLegs) is twelve legs, and a ticket stops sweating at its first dead leg — so a
            // single round cannot reach fifteen kinds however it is arranged. A FRESH Run per
            // chunk is how every kind gets to be a ticket's FIRST leg, which is the only position
            // guaranteed to sweat.
            var chunks = Chunk(AllKinds.ToList(), 3).ToList();
            for (int c = 0; c < chunks.Count; c++)
            {
                var run = new Run($"{seed}#{c}");
                var placed = new List<Ticket>();

                foreach (MarketKind lead in chunks[c])
                {
                    List<Pick>? picks = BuildTicket(run, lead);
                    if (picks == null) continue;
                    try { placed.Add(run.PlaceTicket(picks, 10.0)); }
                    catch (Exception) { /* a combination the engine refuses is not this gate's subject */ }
                }
                if (placed.Count == 0) continue;

                run.LockRound();
                for (int i = 0; i < run.Sweats.Count; i++)
                {
                    sessions++;
                    StepOne(run.Sweats[i], run.Tickets[i], seed, lines, sweated);
                }
            }
        }
        return lines;
    }

    /// <summary>The renderer's own loop, minus the ink, the sleeps and the keyboard. Mirrors
    /// <c>SweatRenderer.SweatOne</c>: the per-leg probability anchor, the event, the two lines it
    /// renders, and the resolution line on a <c>LegFinal</c> beat.</summary>
    private static void StepOne(SweatSession session, Ticket ticket, string seed,
        List<Rendered> lines, HashSet<MarketKind> sweated)
    {
        double prevProb = 0.0;
        int legSeen = -1;

        while (session.MoveNext(out DramaEvent? e))
        {
            DramaEvent evt = e!;
            Leg leg = ticket.Legs[evt.LegIndex];
            if (evt.LegIndex != legSeen)
            {
                legSeen = evt.LegIndex;
                prevProb = leg.TrueProb;
            }

            MarketKind kind = leg.Selection.Kind;
            sweated.Add(kind);

            lines.Add(new Rendered(LineKind.Beat, SweatLines.BeatLine(evt, leg, prevProb), evt.LegIndex, kind, seed));
            lines.Add(new Rendered(LineKind.Meter, SweatLines.MeterLine(evt, session.CashOutOffer()), evt.LegIndex, kind, seed));

            if (evt.Type == DramaEventType.LegFinal)
            {
                string verdict = leg.IsVoided ? SweatLines.VoidVerdict
                    : session.RevealedLegState(evt.LegIndex) == LegState.Won ? SweatLines.WonVerdict
                    : SweatLines.LostVerdict;
                lines.Add(new Rendered(LineKind.Verdict,
                    SweatLines.VerdictLine(evt.LegIndex, leg, verdict), evt.LegIndex, kind, seed));
            }

            prevProb = evt.WinProbAfter;

            // No consumable is held on a fresh run, so no save window opens; declined defensively so
            // a future default loadout cannot hang this sweep.
            if (session.HasPendingLoss) session.DeclinePendingLoss();
            if (session.IsComplete) break;
        }
    }

    /// <summary>
    /// A multi-leg ticket whose FIRST leg is <paramref name="lead"/> — the position that always
    /// sweats — filled out from other matchups so the sweat under test is genuinely multi-leg.
    /// Legs are taken from DISTINCT matchups so nothing here depends on same-game pricing.
    /// Returns null when the slate does not price <paramref name="lead"/> at all.
    /// </summary>
    private static List<Pick>? BuildTicket(Run run, MarketKind lead)
    {
        IReadOnlyList<Matchup> matchups = run.CurrentSlate.Matchups;
        var picks = new List<Pick>();
        var usedMatchups = new HashSet<int>();

        for (int i = 0; i < matchups.Count; i++)
        {
            MarketSelection? found = FirstOfKind(matchups[i], lead);
            if (found == null) continue;
            picks.Add(new Pick(i, found.Value));
            usedMatchups.Add(i);
            break;
        }
        if (picks.Count == 0) return null;

        for (int i = 0; i < matchups.Count && picks.Count < run.Config.MaxLegs; i++)
        {
            if (usedMatchups.Contains(i)) continue;
            // The favourite on this matchup: a ticket whose later legs are longshots dies early and
            // the legs behind it never speak. Picking the shortest price is what lets a three-leg
            // sweat actually render three legs often enough to be worth sweeping.
            MarketSelection? best = ShortestPrice(matchups[i]);
            if (best == null) continue;
            picks.Add(new Pick(i, best.Value));
            usedMatchups.Add(i);
        }
        return picks;
    }

    private static MarketSelection? FirstOfKind(Matchup m, MarketKind kind)
    {
        foreach (MarketSheetRow row in MarketSheet.Build(m).AllRows)
        {
            if (row.Kind == kind) return row.Offer.Selection;
        }
        return null;
    }

    private static MarketSelection? ShortestPrice(Matchup m)
    {
        MarketSheetRow? best = null;
        foreach (MarketSheetRow row in MarketSheet.Build(m).AllRows)
        {
            if (best == null || row.Offer.Odds < best.Offer.Odds) best = row;
        }
        return best?.Offer.Selection;
    }

    /// <summary>Every name the composer can print on this seed's slate — the beat line must contain
    /// none of them, which is the casing rule's operational form.</summary>
    private static IReadOnlyList<string> NamesOnSheetsFor(string seed)
    {
        if (_nameCache.TryGetValue(seed, out IReadOnlyList<string>? cached)) return cached;

        var names = new HashSet<string>(StringComparer.Ordinal);
        var run = new Run(seed);
        foreach (Matchup m in run.CurrentSlate.Matchups)
        {
            foreach (MarketSheetRow row in MarketSheet.Build(m).AllRows)
                names.Add(row.Name.ToUpperInvariant());
        }
        IReadOnlyList<string> list = names.ToList();
        _nameCache[seed] = list;
        return list;
    }

    private static readonly Dictionary<string, IReadOnlyList<string>> _nameCache = new();

    private static IEnumerable<List<T>> Chunk<T>(List<T> source, int size)
    {
        for (int i = 0; i < source.Count; i += size)
            yield return source.GetRange(i, Math.Min(size, source.Count - i));
    }

    // ---- reflection into the pools this gate measures against (PoolGateTests' own discipline) ----

    private static string[] ReflectPool(string fieldName)
    {
        FieldInfo? field = typeof(SlateGenerator).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
        if (field is null)
            throw new InvalidOperationException(
                $"SlateGenerator.{fieldName} was not found by reflection — report it as a finding, "
                + "do not hand-type a replacement list to make the gate pass.");
        return field.GetValue(null) as string[]
            ?? throw new InvalidOperationException($"SlateGenerator.{fieldName} reflected as null.");
    }

    /// <summary>
    /// <c>EventText</c>'s authored flavour tables, read out of the real type. They are
    /// <c>private static readonly string[]</c> on an <c>internal</c> class by design, and reflecting
    /// them is the only way to measure the ACTUAL widest beat rather than a transcription of it —
    /// the same reason <c>PoolGateTests</c> reflects <c>SlateGenerator</c>'s arrays.
    /// </summary>
    private static IReadOnlyList<string> ReflectFlavourTables()
    {
        Type? eventText = typeof(SweatLines).Assembly.GetType("SBR.ConsoleGame.EventText");
        if (eventText is null)
            throw new InvalidOperationException("SBR.ConsoleGame.EventText was not found by reflection.");

        var all = new List<string>();
        foreach (FieldInfo f in eventText.GetFields(BindingFlags.NonPublic | BindingFlags.Static))
        {
            if (f.GetValue(null) is string[] table) all.AddRange(table);
        }
        if (all.Count == 0)
            throw new InvalidOperationException(
                "EventText exposed no string[] flavour table — the widest-beat measurement would be "
                + "measuring nothing (C29).");
        return all;
    }

    private static void AssertNone(List<string> offenders, string what)
    {
        if (offenders.Count == 0) return;
        throw new Xunit.Sdk.XunitException(
            $"{what} — {offenders.Count} line(s):{Environment.NewLine}  "
            + string.Join(Environment.NewLine + "  ", offenders.Take(20))
            + (offenders.Count > 20 ? $"{Environment.NewLine}  … and {offenders.Count - 20} more" : string.Empty));
    }
}
