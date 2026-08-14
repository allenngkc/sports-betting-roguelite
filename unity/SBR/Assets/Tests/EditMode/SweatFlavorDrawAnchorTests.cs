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

            Assert.IsTrue(SweatFlavor.PickedHomeForPresentation(draw),
                "a draw has no picked team, so it takes the same home anchor O/U and BTTS take — " +
                "reporting the AWAY side here is the defect");

            // Specific to the draw, not a blanket: the two team rows still answer their real sides.
            Assert.IsTrue(SweatFlavor.PickedHomeForPresentation(home), "a home moneyline still answers home");
            Assert.IsFalse(SweatFlavor.PickedHomeForPresentation(away), "an away moneyline still answers away");
        }

        [Test]
        public void The_draw_legs_flavour_names_the_home_club_not_the_away_one()
        {
            (Leg draw, Leg _, Leg _2, Matchup m) = Legs();

            string homeClub = SweatFlavor.Short(m.Home.Name);
            string awayClub = SweatFlavor.Short(m.Away.Name);
            Assume.That(homeClub, Is.Not.EqualTo(awayClub), "the two clubs must differ for this to test anything");

            // The RENDERED consequence, not just the boolean: the defect was a wrong club NAME on
            // screen, so that is what this asserts.
            string line = SweatFlavor.GoalLine(forPicked: true, leg: draw, step: 0);

            StringAssert.Contains(homeClub, line,
                $"the draw leg's flavour should anchor on the home club. Actual: '{line}'");
            Assert.IsFalse(line.StartsWith(awayClub),
                $"the away club must not lead a draw leg's flavour — that was the defect. Actual: '{line}'");
        }
    }
}
