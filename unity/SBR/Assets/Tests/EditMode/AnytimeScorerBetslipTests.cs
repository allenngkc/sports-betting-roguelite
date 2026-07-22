using NUnit.Framework;
using SBR.Engine;
using SBR.Game;

namespace SBR.Tests.EditMode
{
    /// <summary>Phase 4 board-model coverage: the Players tab is a thin renderer over these
    /// selections, so it must preserve the engine's scorer identity and one-leg-per-match rule.</summary>
    public class AnytimeScorerBetslipTests
    {
        [Test]
        public void Scorer_selection_toggles_like_every_other_market_and_keeps_its_label()
        {
            var run = new Run("UNITY-SCORER-SLIP", new RunConfig { StartingBank = 500 });
            var slip = new BetslipModel(run);
            MarketSelection first = MarketSelection.AnytimeScorer(0);
            MarketSelection second = MarketSelection.AnytimeScorer(1);

            Assert.IsTrue(slip.Toggle(0, first));
            Assert.AreEqual(first, slip.SelectionOn(0));
            Assert.IsTrue(slip.Toggle(0, second), "another player replaces the match leg");
            Assert.AreEqual(1, slip.Picks.Count);
            Assert.AreEqual(second, slip.SelectionOn(0));

            Ticket ticket = slip.Place();
            Assert.AreEqual(second, ticket.Legs[0].Selection);
            StringAssert.Contains("ANYTIME", ticket.Legs[0].DisplayLabel);
            StringAssert.Contains(run.CurrentSlate.Matchups[0].PlayerAt(1).Name.ToUpperInvariant(),
                ticket.Legs[0].DisplayLabel);
        }
    }
}
