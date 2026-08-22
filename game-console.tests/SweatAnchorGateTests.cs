using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using SBR.Engine;
using SBR.Game;
using Xunit.Abstractions;

namespace SBR.ConsoleGame.Tests;

/// <summary>
/// THE SWEAT'S ANCHOR GATE — <c>K17-cl</c> (DD 2026-08-21 batch 170, VIOLATION) and <c>T163</c>.
///
/// <para><b>What it exists to stop coming back.</b> <c>EventText.For</c> computed
/// <c>bool pickedHome = leg.Selection.Kind != MarketKind.Moneyline || leg.Selection.Choice ==
/// MarketChoice.Home;</c> and interpolated the two clubs as <c>{picked}</c> and <c>{other}</c> off
/// it. That comment above it — <i>"market legs have no picked TEAM, anchor on the home side"</i> —
/// was TRUE when it was written: every non-moneyline kind then was team-agnostic. <b>F_0.5.0 added
/// five kinds that DO carry a side</b> (DoubleChance, Handicap, the three team totals) and the pick
/// grammar made them bettable, and for every one of them that predicate returns true. <b>Back the
/// AWAY side and the beat named the OPPONENT as the player's team</b> while the leg's own verdict
/// row named the club he actually backed — two zones of one surface disagreeing about whose side
/// he is on.</para>
///
/// <para><b>Why the assertions are reconstructions rather than string sightings.</b> A beat may
/// legitimately name the opponent — <c>ScoreDown</c> is <c>"{other} answer right back."</c> — so
/// "the opponent's noun appears" is not the defect and a containment check cannot see the defect at
/// all. What is asserted instead is the SLOT: the tables are reflected out of <c>EventText</c>, the
/// expected line is rebuilt by filling <c>{picked}</c> with the backed club and <c>{other}</c> with
/// its opponent, and the rendered beat must equal it exactly. The struck predicate fails that on
/// every AWAY-backed leg of all five kinds.</para>
///
/// <para><b>C29.</b> Every case count below is printed and asserted non-zero.</para>
/// </summary>
public class SweatAnchorGateTests
{
    private readonly ITestOutputHelper _output;

    public SweatAnchorGateTests(ITestOutputHelper output) => _output = output;

    private static readonly string[] Seeds = { "GATE-K17-A", "GATE-K17-B", "GATE-K17-C", "GATE-K17-D" };

    private static IReadOnlyList<MarketKind> AllKinds => (MarketKind[])Enum.GetValues(typeof(MarketKind));

    /// <summary>The five kinds the ruling is about: the ones F_0.5.0 made bettable that CARRY a
    /// side, and for which the HOME anchor was not arbitrary but wrong.</summary>
    private static readonly MarketKind[] SideCarrying =
    {
        MarketKind.DoubleChance,
        MarketKind.Handicap,
        MarketKind.TeamTotalGoals,
        MarketKind.TeamTotalCorners,
        MarketKind.TeamTotalCards,
    };

    // =====================================================================================
    // 1. THE TABLE — exhaustive over MarketKind, and loud rather than silent on a sixteenth.
    // =====================================================================================

    /// <summary>
    /// <c>EventText.BackedSide</c> answers for all fifteen kinds and <b>guesses for none</b>. The
    /// second half is the half that matters: <c>K17-cl</c> exists because a predicate let every
    /// unlisted case fall through to one arm, so a table that merely answers is not enough — an
    /// unknown kind must FAIL, not inherit some other kind's side.
    /// </summary>
    [Fact]
    public void BackedSide_answers_every_market_kind_and_guesses_for_none()
    {
        var answered = new Dictionary<MarketKind, List<Side?>>();
        int selections = 0;

        foreach (MarketSelection s in EveryPricedSelection())
        {
            selections++;
            Side? backed = BackedSide(s);
            if (!answered.TryGetValue(s.Kind, out List<Side?>? seen))
                answered[s.Kind] = seen = new List<Side?>();
            if (!seen.Contains(backed)) seen.Add(backed);
        }

        _output.WriteLine($"priced selections put through BackedSide : {selections}");
        foreach (MarketKind k in AllKinds)
        {
            string answers = answered.TryGetValue(k, out List<Side?>? v)
                ? string.Join(" / ", v.Select(x => x?.ToString() ?? "neither"))
                : "NEVER PRICED";
            _output.WriteLine($"  {k,-20} -> {answers}");
        }

        Assert.True(selections > 0, "C29: no selection was put through the table");

        List<MarketKind> missed = AllKinds.Where(k => !answered.ContainsKey(k)).ToList();
        Assert.True(missed.Count == 0,
            "these kinds were never priced on any seed, so their anchor is unasserted: "
            + string.Join(", ", missed));

        // The five the ruling is about must be able to answer AWAY. If a kind can only ever answer
        // Home the gate below is measuring nothing for it.
        foreach (MarketKind k in SideCarrying)
            Assert.True(answered[k].Contains(Side.Away),
                $"{k} never answered AWAY over {Seeds.Length} seeds — the case K17-cl rules on is "
                + "not reachable from this pool and this gate would be vacuous for it");

        // The nine team-agnostic kinds answer NEITHER — T163 branch (3) names the set outright.
        foreach (MarketKind k in AllKinds.Except(SideCarrying).Where(k => k != MarketKind.Moneyline))
            Assert.True(answered[k].All(a => a == null),
                $"{k} names no side in its selection, so the honest answer is neither; got "
                + string.Join("/", answered[k].Select(a => a?.ToString() ?? "neither")));

        // NO SILENT DEFAULT. A kind outside the enum must throw, not be answered.
        MarketSelection unknown = new MarketSelection((MarketKind)9999, 0.0, MarketChoice.Home);
        Exception? thrown = Record.Exception(() => BackedSide(unknown));
        Assert.True(thrown is ArgumentOutOfRangeException,
            "BackedSide answered for a kind it has no arm for. A default that guesses a side is "
            + "exactly how K17-cl happened; the arm must be added deliberately. Got: "
            + (thrown?.GetType().Name ?? "no exception"));
    }

    /// <summary>The moneyline DRAW is neither, and it is asserted separately because no board yet
    /// builds one — the same reason <c>BetslipModel.SideOn</c>'s draw arm is unreachable-but-pinned.
    /// The struck predicate answered AWAY for it (<c>"not Home"</c> meant Away only while
    /// <c>Choice</c> had two values), which is the defect that ruling records fixing.</summary>
    [Fact]
    public void The_X_of_1X2_backs_neither_club()
    {
        Assert.Null(BackedSide(MarketSelection.MoneylineDraw()));
        Assert.Equal(Side.Home, BackedSide(MarketSelection.Moneyline(Side.Home)));
        Assert.Equal(Side.Away, BackedSide(MarketSelection.Moneyline(Side.Away)));

        // 12 backs both clubs, so neither of them is HIS club.
        Assert.Null(BackedSide(MarketSelection.DoubleChance(MarketChoice.HomeOrAway)));
        Assert.Equal(Side.Home, BackedSide(MarketSelection.DoubleChance(MarketChoice.HomeOrDraw)));
        Assert.Equal(Side.Away, BackedSide(MarketSelection.DoubleChance(MarketChoice.AwayOrDraw)));
    }

    // =====================================================================================
    // 2. THE HEADLINE — the AWAY side of every side-carrying kind, which is the case that was wrong.
    // =====================================================================================

    /// <summary>
    /// <b>The anchor names the side the player actually backed</b>, asserted on the AWAY side of all
    /// five side-carrying kinds, against the real priced pool and through the real renderer
    /// (<c>SweatLines.BeatLine</c>). Every beat is reconstructed from <c>EventText</c>'s own tables
    /// with the AWAY club in <c>{picked}</c>, so a HOME anchor cannot pass by accident.
    /// </summary>
    [Fact]
    public void The_anchor_names_the_backed_side_on_an_AWAY_backed_leg_of_every_side_carrying_kind()
    {
        var offenders = new List<string>();
        var covered = new HashSet<MarketKind>();
        int beats = 0;

        foreach ((Matchup m, MarketSelection sel) in EveryPricedSelectionWithMatchup())
        {
            if (!SideCarrying.Contains(sel.Kind)) continue;
            if (BackedSide(sel) != Side.Away) continue;
            if (Short(m.Home.Name) == Short(m.Away.Name)) continue; // the two nouns must be tellable apart

            covered.Add(sel.Kind);
            var leg = new Leg(m, sel, 2.0);

            foreach (string offence in CheckEveryBeat(leg, Side.Away, m, ref beats))
                offenders.Add($"[{sel.Kind} AWAY on {m.Away.Name} at {m.Home.Name}] {offence}");
        }

        _output.WriteLine($"AWAY-backed legs' beats reconstructed : {beats}");
        _output.WriteLine($"side-carrying kinds covered AWAY      : {covered.Count} of {SideCarrying.Length}");
        Assert.True(beats > 0, "C29: no AWAY-backed beat was rendered");

        List<MarketKind> missed = SideCarrying.Where(k => !covered.Contains(k)).ToList();
        Assert.True(missed.Count == 0,
            "these side-carrying kinds never produced an AWAY-backed leg, so K17-cl's own case is "
            + "unasserted for them: " + string.Join(", ", missed));

        AssertNone(offenders, "K17-cl: the beat anchors on a club the player did not back");
    }

    // =====================================================================================
    // 3. THE POPULATION — every kind, both sides, every priced selection on the sheets.
    // =====================================================================================

    /// <summary>
    /// The same reconstruction over the whole priced pool, both sides, plus the other half of the
    /// ruling: <b>where the leg backs neither club the beat names no club at all</b>. The second
    /// assertion is <c>T163</c>'s trap made mechanical — a *neither* path that still computed
    /// <c>picked</c>/<c>other</c> would ship a HOME anchor under a neutral name, silently, on
    /// precisely these kinds.
    /// </summary>
    [Fact]
    public void No_beat_ever_names_the_opponent_as_the_players_team_and_a_neither_leg_names_no_club()
    {
        var offenders = new List<string>();
        int anchored = 0;
        int neither = 0;
        var neitherKinds = new HashSet<MarketKind>();

        foreach ((Matchup m, MarketSelection sel) in EveryPricedSelectionWithMatchup())
        {
            if (Short(m.Home.Name) == Short(m.Away.Name)) continue;
            var leg = new Leg(m, sel, 2.0);
            Side? backed = BackedSide(sel);

            if (backed is Side side)
            {
                foreach (string offence in CheckEveryBeat(leg, side, m, ref anchored))
                    offenders.Add($"[{sel.Kind} {side}] {offence}");
                continue;
            }

            neitherKinds.Add(sel.Kind);
            foreach ((DramaEvent e, double prev) in EveryBeat())
            {
                neither++;
                string beat = Beat(e, leg, prev);
                foreach (string noun in new[] { Short(m.Home.Name), Short(m.Away.Name) })
                {
                    if (noun.Length >= 4 && beat.Contains(noun, StringComparison.Ordinal))
                        offenders.Add($"[{sel.Kind} backs neither] the beat names the club '{noun}' "
                            + $"— T163's trap, a HOME anchor under a neutral name: {beat}");
                }
            }
        }

        _output.WriteLine($"anchored beats reconstructed : {anchored}");
        _output.WriteLine($"neither-branch beats swept   : {neither}");
        _output.WriteLine($"kinds reaching the neither branch : {neitherKinds.Count} — "
            + string.Join(", ", neitherKinds.OrderBy(k => k.ToString(), StringComparer.Ordinal)));
        Assert.True(anchored > 0 && neither > 0,
            "C29: one of the two branches was never rendered");
        AssertNone(offenders, "K17-cl / T163: a beat named a club the leg does not back");
    }

    // =====================================================================================
    // 4. THE PROHIBITION — the sibling on the TV must not have been widened into this one.
    // =====================================================================================

    /// <summary>
    /// <b><c>K17-cl</c> forbids fixing this by widening <c>SweatFlavor.PickedHomeForPresentation</c></b>:
    /// its docstring states in terms that it answers a DIFFERENT question and that <i>"neither" would
    /// leave the flavour with no names</i>. The two are two correct shapes for two questions and
    /// collapsing them re-creates the conflation <c>T143-am</c> split apart one batch ago.
    ///
    /// <para>The console's test assembly cannot load a Unity runtime type, so this asserts on the
    /// SOURCE: the function still returns <c>bool</c>, still answers HOME for every non-moneyline
    /// kind, and has not grown a <c>Side?</c>. Paired with a behavioural half — the console's own
    /// table answers where that function does not — so the claim is not purely textual.</para>
    /// </summary>
    [Fact]
    public void PickedHomeForPresentation_was_not_widened_and_the_two_shapes_still_differ()
    {
        string path = Path.Combine(RepoRoot(), "unity", "SBR", "Assets", "SBR", "Runtime", "SweatFlavor.cs");
        Assert.True(File.Exists(path), $"SweatFlavor.cs was not found at {path}");
        string flat = Regex.Replace(File.ReadAllText(path), @"\s+", " ");

        const string Pinned =
            "public static bool PickedHomeForPresentation(Leg leg) "
            + "=> leg.Selection.Kind == MarketKind.AnytimeScorer "
            + "? leg.Matchup.PlayerSide(leg.Selection.PlayerIndex) == Side.Home "
            + ": leg.Selection.Kind != MarketKind.Moneyline "
            + "|| leg.Selection.Choice == MarketChoice.Home "
            + "|| leg.Selection.Choice == MarketChoice.Draw;";

        Assert.True(flat.Contains(Pinned, StringComparison.Ordinal),
            "SweatFlavor.PickedHomeForPresentation is not the function K17-cl was ruled against. "
            + "If the TV lane changed it deliberately, re-pin this string and say so; if the change "
            + "came from someone fixing K17-cl by widening it, that is the thing the ruling forbids.");

        Assert.False(flat.Contains("Side? PickedHomeForPresentation", StringComparison.Ordinal),
            "PickedHomeForPresentation now returns Side? — it has been widened into the backed-side "
            + "question, which K17-cl forbids (T143-am's conflation).");

        // The behavioural half: on the nine team-agnostic kinds that function answers HOME (it
        // returns true for every non-moneyline kind), and the console's table answers NEITHER. On
        // an AWAY-backed side-carrying kind it still answers HOME and the console answers AWAY.
        // Two questions, two answers — if these ever coincide the shapes have been collapsed.
        int diverged = 0;
        foreach (MarketSelection s in EveryPricedSelection())
        {
            bool tvSaysHome = s.Kind == MarketKind.AnytimeScorer
                || s.Kind != MarketKind.Moneyline
                || s.Choice == MarketChoice.Home
                || s.Choice == MarketChoice.Draw;
            if (!tvSaysHome) continue;
            if (BackedSide(s) != Side.Home) diverged++;
        }
        _output.WriteLine($"selections where the two shapes give different answers : {diverged}");
        Assert.True(diverged > 0,
            "the console's anchor now agrees with PickedHomeForPresentation everywhere, which means "
            + "one of the two questions stopped being asked");
    }

    // =====================================================================================
    // Construction
    // =====================================================================================

    /// <summary>Every beat shape the base tables can be reached through. <c>TensionTag.Calm</c>
    /// throughout: <c>NearMiss</c> is a tag override with its own club-free pair and is not the
    /// anchor's subject. Six steps so every variant of every three-line table is reached.</summary>
    private static IEnumerable<(DramaEvent, double)> EveryBeat()
    {
        DramaEventType[] types = { DramaEventType.Momentum, DramaEventType.Score, DramaEventType.BigPlay };
        foreach (DramaEventType t in types)
        {
            for (int step = 0; step < 6; step++)
            {
                // up: WinProbAfter >= prevProb. down: strictly below.
                yield return (new DramaEvent(0, step, 8, t, 0.60, TensionTag.Calm), 0.50);
                yield return (new DramaEvent(0, step, 8, t, 0.40, TensionTag.Calm), 0.50);
            }
        }
    }

    /// <summary>The beat as the renderer writes it, with the gutter stripped — the gutter is
    /// <c>SweatLines</c>' and carries no prose.</summary>
    private static string Beat(DramaEvent e, Leg leg, double prevProb)
    {
        string full = SweatLines.BeatLine(e, leg, prevProb);
        string gutter = SweatLines.BeatGutter(e);
        return full.StartsWith(gutter, StringComparison.Ordinal) ? full.Substring(gutter.Length) : full;
    }

    /// <summary>Reconstructs every beat of <paramref name="leg"/> from <c>EventText</c>'s own tables
    /// with <paramref name="backed"/> in the <c>{picked}</c> slot, and reports each mismatch.
    /// Count-narrated and scorer kinds never reach the base tables and are excluded by their caller
    /// (they back neither side, so they are the other test's subject).</summary>
    private static IEnumerable<string> CheckEveryBeat(Leg leg, Side backed, Matchup m, ref int beats)
    {
        // Kinds whose beats are intercepted before the base tables. All of them back neither side,
        // so they never arrive here through a caller that filters on a non-null backed side; the
        // guard is kept so a future kind cannot make this silently measure the wrong branch.
        if (leg.Selection.Kind is MarketKind.TotalCorners or MarketKind.TotalCards
            or MarketKind.AnytimeScorer)
            return new[] { $"{leg.Selection.Kind} reached the anchored reconstruction, which does not "
                + "model its intercepting branch — the gate needs updating, not relaxing." };

        string picked = Short(NameOf(m, backed));
        string other = Short(NameOf(m, backed == Side.Home ? Side.Away : Side.Home));
        var found = new List<string>();
        int n = 0;

        foreach ((DramaEvent e, double prev) in EveryBeat())
        {
            n++;
            bool up = e.WinProbAfter >= prev;
            string[] table = TableFor(e.Type, up);
            string template = table[e.Step % table.Length];

            // T163-am §1 rests on this and it is asserted rather than assumed: no template carries
            // both slots, which is what lets the neither branch move the referent at all.
            if (template.Contains("{picked}", StringComparison.Ordinal)
                && template.Contains("{other}", StringComparison.Ordinal))
            {
                found.Add($"the template '{template}' carries BOTH slots — T163-am §1's premise is broken");
                continue;
            }

            string expected = template.Replace("{picked}", picked).Replace("{other}", other);
            string actual = Beat(e, leg, prev);

            if (!string.Equals(expected, actual, StringComparison.Ordinal))
            {
                found.Add($"{e.Type} {(up ? "up" : "down")} step {e.Step}: expected '{expected}' got '{actual}'");
                continue;
            }

            // Said the other way round, because this is the sentence the ruling is written in: the
            // club in the {picked} slot is the club he backed, and his opponent is never it.
            if (template.Contains("{picked}", StringComparison.Ordinal))
            {
                if (!actual.Contains(picked, StringComparison.Ordinal))
                    found.Add($"{e.Type} {(up ? "up" : "down")} step {e.Step}: the backed club "
                        + $"'{picked}' is not in the beat: {actual}");
                if (other.Length >= 4 && actual.Contains(other, StringComparison.Ordinal))
                    found.Add($"{e.Type} {(up ? "up" : "down")} step {e.Step}: the OPPONENT '{other}' "
                        + $"stands where the player's team belongs: {actual}");
            }
        }

        beats += n;
        return found;
    }

    private static string NameOf(Matchup m, Side side) => side == Side.Home ? m.Home.Name : m.Away.Name;

    private static IEnumerable<MarketSelection> EveryPricedSelection()
        => EveryPricedSelectionWithMatchup().Select(t => t.Item2);

    private static IEnumerable<(Matchup, MarketSelection)> EveryPricedSelectionWithMatchup()
    {
        foreach (string seed in Seeds)
        {
            var run = new Run(seed);
            foreach (Matchup m in run.CurrentSlate.Matchups)
            {
                foreach (MarketSheetRow row in MarketSheet.Build(m).AllRows)
                    yield return (m, row.Offer.Selection);
            }
        }
    }

    // ---- reflection into EventText, for the same reason SweatNamingGateTests does it ----

    private static Type EventTextType =>
        typeof(SweatLines).Assembly.GetType("SBR.ConsoleGame.EventText")
        ?? throw new InvalidOperationException("SBR.ConsoleGame.EventText was not found by reflection.");

    private static Side? BackedSide(MarketSelection s)
    {
        MethodInfo m = EventTextType.GetMethod("BackedSide",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException(
                "EventText.BackedSide was not found — K17-cl's fix is the existence of this table.");
        try { return (Side?)m.Invoke(null, new object[] { s }); }
        catch (TargetInvocationException tie) when (tie.InnerException != null)
        {
            throw tie.InnerException;
        }
    }

    private static string Short(string teamName)
    {
        MethodInfo m = EventTextType.GetMethod("Short", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("EventText.Short was not found by reflection.");
        return (string)m.Invoke(null, new object[] { teamName })!;
    }

    private static string[] TableFor(DramaEventType type, bool up)
    {
        string field = (type, up) switch
        {
            (DramaEventType.Score, true) => "ScoreUp",
            (DramaEventType.Score, false) => "ScoreDown",
            (DramaEventType.BigPlay, true) => "BigUp",
            (DramaEventType.BigPlay, false) => "BigDown",
            (DramaEventType.Momentum, true) => "MomUp",
            _ => "MomDown",
        };
        FieldInfo f = EventTextType.GetField(field, BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException(
                $"EventText.{field} was not found by reflection — report it as a finding rather than "
                + "relaxing this gate; the reconstruction is what makes the slot assertion exact.");
        return (string[])f.GetValue(null)!;
    }

    /// <summary>Walks up from the test binaries to the directory holding <c>SBR.slnx</c>.</summary>
    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "SBR.slnx")))
            dir = dir.Parent;
        return dir?.FullName
            ?? throw new InvalidOperationException("SBR.slnx was not found above " + AppContext.BaseDirectory);
    }

    private void AssertNone(List<string> offenders, string what)
    {
        if (offenders.Count == 0) return;
        foreach (string o in offenders.Take(40)) _output.WriteLine("  " + o);
        Assert.Fail($"{what} — {offenders.Count} offender(s):\n"
            + string.Join("\n", offenders.Take(40))
            + (offenders.Count > 40 ? $"\n… and {offenders.Count - 40} more" : string.Empty));
    }
}
