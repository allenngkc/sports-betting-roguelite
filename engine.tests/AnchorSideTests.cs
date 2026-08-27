using System;
using System.Collections.Generic;
using System.Linq;
using SBR.Engine;

namespace SBR.Engine.Tests;

/// <summary>
/// <c>MatchModel.AnchorSide</c> — <c>T163</c>'s prose anchor, moved to the engine as the single
/// source 2026-08-24 so the TV consumes it instead of keeping a second fifteen-arm table.
///
/// <para>These pin the three things the contract in <c>docs/handoffs/theater-engine.md</c> promises:
/// every kind answered DELIBERATELY, a sixteenth kind THROWS rather than being guessed at
/// (<c>K17-cl</c>), and the divergence from the question <c>EventText.BackedSide</c> answers is real
/// and intended rather than a bug.</para>
/// </summary>
public class AnchorSideTests
{
    // Away roster first, home second — Matchup.PlayerAt's stable board order. So player 0 is AWAY
    // and player 3 is HOME, which is what makes the player-market divergence visible below.
    private static Matchup MakeMatchup()
    {
        var home = new[]
        {
            new Player("Home Forward", PlayerRole.FW, 3),
            new Player("Home Midfielder", PlayerRole.MF, 2),
            new Player("Home Defender", PlayerRole.DF, 1),
        };
        var away = new[]
        {
            new Player("Away Forward", PlayerRole.FW, 3),
            new Player("Away Midfielder", PlayerRole.MF, 2),
            new Player("Away Defender", PlayerRole.DF, 1),
        };
        return new Matchup(0, new Team("Home Team", 1, 0, home), new Team("Away Team", 0, 1, away),
            0.5, 2.0, 2.0);
    }

    private static Leg LegFor(Matchup m, MarketSelection s) => new Leg(m, s, 2.0);

    // ================================================================== the ruled table, asserted

    /// <summary>Every kind answered, and answered with the value the ruling gives it — not merely
    /// "does not throw". A table that returned the wrong side for a kind would pass a
    /// never-throws test and fail this one.</summary>
    [Fact]
    public void Every_market_kind_is_answered_with_the_ruled_value()
    {
        Matchup m = MakeMatchup();

        var expected = new (MarketSelection Selection, Side? Anchor, string Why)[]
        {
            (MarketSelection.Moneyline(Side.Home), Side.Home, "the backed club"),
            (MarketSelection.Moneyline(Side.Away), Side.Away, "the backed club"),
            (MarketSelection.MoneylineDraw(), null, "the draw is not a team, ever (T96)"),

            (MarketSelection.Handicap(Side.Home, -1.5), Side.Home, "the line is applied to the backed side"),
            (MarketSelection.Handicap(Side.Away, 1.5), Side.Away, "the line is applied to the backed side"),

            (MarketSelection.DoubleChance(MarketChoice.HomeOrDraw), Side.Home, "the one club in the union"),
            (MarketSelection.DoubleChance(MarketChoice.AwayOrDraw), Side.Away, "the one club in the union"),
            (MarketSelection.DoubleChance(MarketChoice.HomeOrAway), null, "12 holds both, so neither"),

            (MarketSelection.TeamTotalGoals(Side.Away, 1.5, true), Side.Away, "a named Team field"),
            (MarketSelection.TeamTotalCorners(Side.Home, 4.5, true), Side.Home, "a named Team field"),
            (MarketSelection.TeamTotalCards(Side.Away, 1.5, true), Side.Away, "a named Team field"),

            (MarketSelection.AnytimeScorer(0), Side.Away, "player 0 is on the away roster"),
            (MarketSelection.AnytimeScorer(3), Side.Home, "player 3 is on the home roster"),
            (MarketSelection.PlayerMultiScorer(0), Side.Away, "player 0 is on the away roster"),
            (MarketSelection.PlayerMultiScorer(3), Side.Home, "player 3 is on the home roster"),

            (MarketSelection.TotalGoals(2.5, true), null, "T163 branch (3): names no side"),
            (MarketSelection.BothTeamsToScore(true), null, "T163 branch (3): names no side"),
            (MarketSelection.TotalCorners(9.5, true), null, "T163 branch (3): names no side"),
            (MarketSelection.TotalCards(4.5, true), null, "T163 branch (3): names no side"),
            (MarketSelection.CorrectScore(2, 1), null, "T163 branch (3): names no side"),
            (MarketSelection.WinningMargin(1), null, "T163 branch (3): names no side"),
            (MarketSelection.TotalGoalsOddEven(true), null, "T163 branch (3): names no side"),
        };

        foreach ((MarketSelection selection, Side? anchor, string why) in expected)
            Assert.True(anchor == MatchModel.AnchorSide(LegFor(m, selection)),
                $"{selection.Kind}/{selection.Choice}: expected {anchor?.ToString() ?? "NEITHER"} "
                + $"({why}), got {MatchModel.AnchorSide(LegFor(m, selection))?.ToString() ?? "NEITHER"}");

        // C29-style coverage: the cases above must actually span the enum, or this test could pass
        // while a kind quietly went unasserted.
        var covered = expected.Select(e => e.Selection.Kind).ToHashSet();
        MarketKind[] missing = Enum.GetValues<MarketKind>().Where(k => !covered.Contains(k)).ToArray();
        Assert.True(missing.Length == 0,
            "kinds this fixture never asserts an anchor for: " + string.Join(", ", missing));
    }

    /// <summary>NO SILENT DEFAULT. A kind outside the enum throws rather than inheriting some other
    /// kind's side — <c>K17-cl</c>'s whole point, and the reason the table is exhaustive rather than
    /// a predicate.</summary>
    [Fact]
    public void A_kind_outside_the_enum_throws_rather_than_guessing()
    {
        Matchup m = MakeMatchup();
        var unknown = new MarketSelection((MarketKind)9999, 0.0, MarketChoice.Over);

        Assert.Throws<ArgumentOutOfRangeException>(() => MatchModel.AnchorSide(LegFor(m, unknown)));
    }

    [Fact]
    public void A_null_leg_is_rejected_rather_than_dereferenced()
        => Assert.Throws<ArgumentNullException>(() => MatchModel.AnchorSide(null!));

    // ============================================ the divergence, and the function this replaces

    /// <summary>
    /// <b>THE DIVERGENCE, PINNED.</b> On the player markets the anchor names the club the man PLAYS
    /// FOR, while the side he BACKED is neither — a man can score in a 3–1 defeat and the leg still
    /// wins. Both answers are correct for their own question, which is why the console's
    /// <c>BackedSide</c> is not superseded by this table.
    /// </summary>
    [Fact]
    public void Player_markets_anchor_on_the_players_club_not_on_a_backed_side()
    {
        Matchup m = MakeMatchup();

        foreach (int playerIndex in new[] { 0, 1, 2, 3, 4, 5 })
        {
            Side expected = m.PlayerSide(playerIndex);
            Assert.Equal(expected, MatchModel.AnchorSide(LegFor(m, MarketSelection.AnytimeScorer(playerIndex))));
            Assert.Equal(expected, MatchModel.AnchorSide(LegFor(m, MarketSelection.PlayerMultiScorer(playerIndex))));
        }

        // And the anchor is never NEITHER here — which is exactly where BackedSide answers null.
        Assert.NotNull(MatchModel.AnchorSide(LegFor(m, MarketSelection.AnytimeScorer(0))));
        Assert.NotNull(MatchModel.AnchorSide(LegFor(m, MarketSelection.PlayerMultiScorer(0))));
    }

    /// <summary>
    /// <b>WHAT CHANGES ON SCREEN, MEASURED RATHER THAN ASSUMED.</b> <c>T163</c> branch (1) claims
    /// "this subsumes today's single-leg case exactly, so nothing on screen changes before arm A
    /// lands." <b>That claim does not hold, and the TV lane measured why:</b>
    /// <c>SweatFlavor.PickedHomeForPresentation</c> AS IT STOOD BEFORE <c>c24b32c</c> returned HOME
    /// unconditionally for every kind except Moneyline and AnytimeScorer, so it named the home club
    /// on legs that back no side at all. <b>That table is gone:</b> since <c>c24b32c</c> the TV
    /// function reads <c>MatchModel.AnchorSide</c> (DD batch 200/201 — a comment asserting what a
    /// function does is a citation and goes stale like any other). The transcription below is the
    /// HISTORICAL table, kept so the intended disagreement list stays pinned.
    ///
    /// <para>This test transcribes that function and pins BOTH halves: the kinds where the two agree
    /// (so the migration is safe there), and the exact set where they disagree (so the change to
    /// shipped copy is a recorded, intended list rather than a surprise). If someone "fixes"
    /// <c>AnchorSide</c> to match the old behaviour, this fails.</para>
    /// </summary>
    [Fact]
    public void The_disagreements_with_the_TV_function_it_replaces_are_an_intended_list()
    {
        Matchup m = MakeMatchup();

        // Transcribed verbatim from the PRE-c24b32c unity/SBR/Assets/SBR/Runtime/SweatFlavor.cs:403 —
        // the function this table superseded. It no longer exists in that form (the live one reads
        // AnchorSide); this is the historical baseline. Kept as a local so this file does not depend on unity/.
        static bool PickedHomeForPresentation(Leg leg)
            => leg.Selection.Kind == MarketKind.AnytimeScorer
                ? leg.Matchup.PlayerSide(leg.Selection.PlayerIndex) == Side.Home
                : leg.Selection.Kind != MarketKind.Moneyline
                    || leg.Selection.Choice == MarketChoice.Home
                    || leg.Selection.Choice == MarketChoice.Draw;

        var sample = new[]
        {
            MarketSelection.Moneyline(Side.Home), MarketSelection.Moneyline(Side.Away),
            MarketSelection.MoneylineDraw(),
            MarketSelection.Handicap(Side.Home, -1.5), MarketSelection.Handicap(Side.Away, 1.5),
            MarketSelection.DoubleChance(MarketChoice.HomeOrDraw),
            MarketSelection.DoubleChance(MarketChoice.HomeOrAway),
            MarketSelection.TeamTotalGoals(Side.Away, 1.5, true),
            MarketSelection.AnytimeScorer(0), MarketSelection.AnytimeScorer(3),
            MarketSelection.PlayerMultiScorer(0),
            MarketSelection.TotalGoals(2.5, true), MarketSelection.BothTeamsToScore(true),
            MarketSelection.CorrectScore(2, 1), MarketSelection.WinningMargin(1),
            MarketSelection.TotalGoalsOddEven(true),
        };

        var agree = new List<string>();
        var disagree = new List<string>();
        foreach (MarketSelection s in sample)
        {
            Leg leg = LegFor(m, s);
            Side? anchor = MatchModel.AnchorSide(leg);
            // The old function can only ever say HOME or AWAY — it has no way to say NEITHER, which
            // is the structural reason it could not carry T163.
            Side old = PickedHomeForPresentation(leg) ? Side.Home : Side.Away;
            (anchor == old ? agree : disagree).Add($"{s.Kind}/{s.Choice}");
        }

        // WHERE THEY AGREE — the migration is a no-op on these, which is the half of T163 branch (1)
        // that does hold.
        Assert.Contains("Moneyline/Home", agree);
        Assert.Contains("Moneyline/Away", agree);
        Assert.Contains("AnytimeScorer/Yes", agree.Concat(disagree)); // shape guard: the case exists
        foreach (int p in new[] { 0, 3 })
            Assert.Equal(
                PickedHomeForPresentation(LegFor(m, MarketSelection.AnytimeScorer(p))) ? Side.Home : Side.Away,
                MatchModel.AnchorSide(LegFor(m, MarketSelection.AnytimeScorer(p))));

        // WHERE THEY DISAGREE — every one is the old function naming HOME where no side is backed,
        // or naming HOME on an AWAY-backed leg. This list IS the shipped-copy change.
        Assert.NotEmpty(disagree);
        Assert.Contains("Moneyline/Draw", disagree);          // T96: the draw is not a team
        Assert.Contains("Handicap/Away", disagree);           // away-backed, named HOME today
        Assert.Contains("TotalGoals/Over", disagree);         // no side backed, named HOME today
        Assert.Contains("PlayerMultiScorer/Yes", disagree);   // anchors on the club, not HOME

        // And the disagreements are ONLY ever the old function saying HOME — never the reverse.
        // If that stops being true, the defect is not the one that was measured.
        foreach (MarketSelection s in sample)
        {
            Leg leg = LegFor(m, s);
            if (MatchModel.AnchorSide(leg) == (PickedHomeForPresentation(leg) ? Side.Home : Side.Away))
                continue;
            Assert.True(PickedHomeForPresentation(leg) || s.Kind == MarketKind.PlayerMultiScorer
                        || s.Kind == MarketKind.AnytimeScorer,
                $"{s.Kind}/{s.Choice}: the old function disagreed by saying AWAY, which is not the "
                + "defect the TV lane measured");
        }
    }
}
