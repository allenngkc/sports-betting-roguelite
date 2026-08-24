using NUnit.Framework;
using SBR.Engine;
using SBR.Game;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// A moneyline DRAW has no side, and the theatre's flavour anchor reported AWAY for it.
    ///
    /// <para><b>Routed here by name</b> from the markets lane's class sweep (`a3d184c`:
    /// *"SweatFlavor:206 — draw counts as away for flavour, ROUTED → tv-sweat"*). It survived that
    /// sweep for the reason worth recording: <b>it lives in this surface's file, not theirs.</b> A
    /// cross-lane sweep scoped by ownership misses exactly the code another lane owns.</para>
    ///
    /// <para><b>The defect was a fall-through rather than a decision.</b> The anchor asked
    /// `Choice == Home` and let everything else be false — correct while a moneyline could only be
    /// Home or Away, and silently wrong the moment `MarketChoice.Draw` existed, because the third
    /// value inherited the second one's branch without the line being touched.</para>
    ///
    /// <para><b>The fix is the rule the anchor already stated:</b> a leg with no picked TEAM anchors
    /// on the home side and lets the market label carry the pick — which is what O/U and BTTS have
    /// always done. A draw is simply a third no-team case.</para>
    ///
    /// <para>Shaped after the markets lane's own pin: assert the draw's behaviour <b>and</b> that the
    /// two team rows still answer, so the fix is specific to the draw rather than a blanket
    /// home-anchor that would quietly break the legs that do have a side.</para>
    /// </summary>
    public class SweatFlavorDrawAnchorTests
    {
        private static (Leg draw, Leg home, Leg away, Matchup m) Legs()
        {
            var run = new Run("FLAVOUR-DRAW");
            Matchup m = run.CurrentSlate.Matchups[0];
            return (new Leg(m, MarketSelection.MoneylineDraw(), 3.20),
                    new Leg(m, MarketSelection.Moneyline(Side.Home), 2.10),
                    new Leg(m, MarketSelection.Moneyline(Side.Away), 2.60),
                    m);
        }

        [Test]
        public void A_moneyline_draw_anchors_home_and_never_reports_away()
        {
            (Leg draw, Leg home, Leg away, Matchup _) = Legs();

            // THIS ASSERTION SURVIVES T163 UNCHANGED, and the split is why. The PROSE goes
            // club-free on a draw; the GEOMETRY — scoreline endpoint, attack direction, scorebug
            // colours — still needs a binary, and falls back to the home convention that
            // ConfigureEndpoint already documents for market-scoped kinds.
            Assert.IsTrue(SweatFlavor.PickedHomeForPresentation(draw),
                "a draw has no picked team, so GEOMETRY takes the same home anchor O/U and BTTS " +
                "take — reporting the AWAY side here is the defect");

            // Specific to the draw, not a blanket: the two team rows still answer their real sides.
            Assert.IsTrue(SweatFlavor.PickedHomeForPresentation(home), "a home moneyline still answers home");
            Assert.IsFalse(SweatFlavor.PickedHomeForPresentation(away), "an away moneyline still answers away");
        }

        /// <summary>RE-BASED BY `T163` — and the original concern is served MORE strongly, not less.
        ///
        /// <para>This used to assert the draw leg's flavour NAMES THE HOME CLUB. That was the
        /// pre-`T163` rule, stated in this class's own summary: *"a leg with no picked TEAM anchors
        /// on the home side and lets the market label carry the pick."* `T163` replaced it — no
        /// picked team means NEITHER, and the engine's `AnchorSide` returns null for
        /// `MoneylineDraw` citing `T96`: <b>the draw is not a team, ever.</b></para>
        ///
        /// <para><b>What this test was written to stop, it still stops.</b> Its defect was the AWAY
        /// club appearing on a draw leg. Under the neither branch NO club appears, which forbids the
        /// away club by construction rather than by anchoring on the other one.</para></summary>
        [Test]
        public void A_draw_legs_flavour_names_NO_club_now_that_T163_rules_the_draw_neither()
        {
            (Leg draw, Leg _, Leg _2, Matchup m) = Legs();

            string homeClub = SweatFlavor.Short(m.Home.Name);
            string awayClub = SweatFlavor.Short(m.Away.Name);
            Assume.That(homeClub, Is.Not.EqualTo(awayClub), "the two clubs must differ for this to test anything");

            Assert.IsNull(MatchModel.AnchorSide(draw),
                "the engine's anchor table must answer NEITHER for a moneyline draw (T96) — if it "
                + "answers a side, this whole branch is unreachable and the assertions below are void");

            // The RENDERED consequence, not just the table: the defect was a wrong club NAME on
            // screen, and the fix is that there is no club name at all.
            string line = SweatFlavor.GoalLine(forPicked: true, leg: draw, step: 0,
                anchor: MatchModel.AnchorSide(draw));

            Assert.IsNotEmpty(line, "no line was produced, so nothing below is being asserted");
            StringAssert.DoesNotContain(awayClub, line,
                $"the away club must never appear on a draw leg — the original defect. Actual: '{line}'");
            StringAssert.DoesNotContain(homeClub, line,
                $"nor the home club: T163 rules the draw NEITHER, so the line is club-free. Actual: '{line}'");
        }
    }
}
