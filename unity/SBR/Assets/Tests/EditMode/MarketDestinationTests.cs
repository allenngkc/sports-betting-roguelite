using System;
using System.Collections.Generic;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// Spec §7.1's gate for <see cref="MarketDestinations"/> (spec-market-surfaces-2026-08-17.md):
    /// every <see cref="MarketKind"/> resolves to exactly one <see cref="MarketDestination"/>, the
    /// rail is a fixed six-entry constant, the kind/destination sets agree in both directions, and
    /// every printed label is non-empty and (for destinations) unique. <c>C51</c>: a cross-element
    /// invariant is an assertion or it does not exist — this file is that assertion for the
    /// destination map, and it is the gate that fails when a 16th <see cref="MarketKind"/> lands
    /// without a home.
    /// </summary>
    public class MarketDestinationTests
    {
        private static readonly MarketKind[] AllKinds = (MarketKind[])Enum.GetValues(typeof(MarketKind));

        // Spec §3: rail order, RESULT through PLAYERS. S95: CORRECT SCORE moved from third to
        // fifth — GOALS, CORNERS, CARDS now run adjacently as the three countable statistics.
        private static readonly MarketDestination[] ExpectedRailOrder =
        {
            MarketDestination.Result,
            MarketDestination.Goals,
            MarketDestination.Corners,
            MarketDestination.Cards,
            MarketDestination.CorrectScore,
            MarketDestination.Players,
        };

        // ---------------------------------------------------------------------- exhaustiveness

        [Test]
        public void Every_MarketKind_resolves_to_a_destination()
        {
            // Spec §7.1 gate 1: exhaustive over the enum. A 16th MarketKind added without a
            // matching arm in For() throws here (ArgumentOutOfRangeException naming the unmapped
            // member) instead of silently landing nowhere on the surface.
            foreach (MarketKind kind in AllKinds)
            {
                Assert.DoesNotThrow(() => MarketDestinations.For(kind), $"MarketKind.{kind}");
            }
        }

        // ---------------------------------------------------------------------------- totality

        [Test]
        public void KindsIn_union_covers_every_MarketKind_exactly_once()
        {
            var union = new List<MarketKind>();
            foreach (MarketDestination d in MarketDestinations.All)
                union.AddRange(MarketDestinations.KindsIn(d));

            // A count check alone passes a duplicate-plus-omission (e.g. AnytimeScorer counted
            // twice AND CorrectScore missing still totals 15) — assert set equality too, not just
            // the count.
            Assert.AreEqual(15, union.Count,
                "the union of KindsIn() across all six destinations must total exactly 15 rows");

            var unionSet = new HashSet<MarketKind>(union);
            var fullSet = new HashSet<MarketKind>(AllKinds);
            Assert.IsTrue(unionSet.SetEquals(fullSet),
                "the union of KindsIn() across all six destinations must equal the full "
                + "MarketKind vocabulary, no more and no less");
        }

        // --------------------------------------------------------------------------- constancy

        [Test]
        public void All_is_exactly_six_destinations_in_rail_order()
        {
            Assert.AreEqual(6, MarketDestinations.All.Count,
                "the rail is spec §3's six destinations, never more or fewer");
            CollectionAssert.AreEqual(ExpectedRailOrder, MarketDestinations.All,
                "MarketDestinations.All must print RESULT, GOALS, CORNERS, CARDS, CORRECT SCORE, "
                + "PLAYERS in that order (spec §3, S95) — the rail is authored once and never reflows");
        }

        // --------------------------------------------------------------------------- agreement

        [Test]
        public void KindsIn_and_For_agree_for_every_destination_and_kind()
        {
            foreach (MarketDestination d in MarketDestinations.All)
            {
                foreach (MarketKind kind in MarketDestinations.KindsIn(d))
                {
                    Assert.AreEqual(d, MarketDestinations.For(kind),
                        $"KindsIn({d}) returned {kind}, but For({kind}) resolves elsewhere");
                }
            }
        }

        // ------------------------------------------------------------------------------ labels

        [Test]
        public void Destination_labels_are_all_non_empty_and_pairwise_distinct()
        {
            var seenLabels = new HashSet<string>();
            foreach (MarketDestination d in MarketDestinations.All)
            {
                string label = MarketDestinations.Label(d);
                Assert.IsFalse(string.IsNullOrEmpty(label), $"{d} has an empty label");
                Assert.IsTrue(seenLabels.Add(label),
                    $"{d}'s label \"{label}\" duplicates an earlier destination's label");
            }
        }

        [Test]
        public void Every_MarketKind_has_a_non_empty_label()
        {
            foreach (MarketKind kind in AllKinds)
            {
                string label = MarketDestinations.KindLabel(kind);
                Assert.IsFalse(string.IsNullOrEmpty(label), $"MarketKind.{kind} has an empty label");
            }
        }

        // ------------------------------------------------------ exact wording (spec-pinned)

        [TestCase(MarketKind.Moneyline, "MONEYLINE")]
        [TestCase(MarketKind.TotalGoals, "TOTAL GOALS")]
        [TestCase(MarketKind.BothTeamsToScore, "BOTH TEAMS TO SCORE")]
        [TestCase(MarketKind.TotalCorners, "TOTAL CORNERS")]
        [TestCase(MarketKind.TotalCards, "TOTAL CARDS")]
        [TestCase(MarketKind.AnytimeScorer, "ANYTIME SCORER")]
        [TestCase(MarketKind.DoubleChance, "DOUBLE CHANCE")]
        [TestCase(MarketKind.Handicap, "HANDICAP")]
        [TestCase(MarketKind.TeamTotalGoals, "TEAM TOTAL GOALS")]
        [TestCase(MarketKind.CorrectScore, "CORRECT SCORE")]
        [TestCase(MarketKind.WinningMargin, "WINNING MARGIN")]
        [TestCase(MarketKind.TotalGoalsOddEven, "ODD / EVEN")]
        [TestCase(MarketKind.TeamTotalCorners, "TEAM TOTAL CORNERS")]
        [TestCase(MarketKind.TeamTotalCards, "TEAM TOTAL CARDS")]
        [TestCase(MarketKind.PlayerMultiScorer, "MULTI SCORER")]
        public void KindLabel_matches_spec_wording(MarketKind kind, string expected)
            => Assert.AreEqual(expected, MarketDestinations.KindLabel(kind));

        [TestCase(MarketDestination.Result, "RESULT")]
        [TestCase(MarketDestination.Goals, "GOALS")]
        [TestCase(MarketDestination.CorrectScore, "CORRECT SCORE")]
        [TestCase(MarketDestination.Corners, "CORNERS")]
        [TestCase(MarketDestination.Cards, "CARDS")]
        [TestCase(MarketDestination.Players, "PLAYERS")]
        public void Label_matches_spec_wording(MarketDestination d, string expected)
            => Assert.AreEqual(expected, MarketDestinations.Label(d));

        // ------------------------------------------------------------- table order (spec §3)

        [Test]
        public void KindsIn_returns_each_destinations_rows_in_the_spec_table_order()
        {
            // Not derivable from MarketKind's own declaration order — BothTeamsToScore declares
            // 3rd of 15 (engine/Domain.cs) but the table seats it LAST within GOALS. Pins the one
            // hand-authored ordering (MarketDestinations.TableOrder) against the spec §3 table.
            CollectionAssert.AreEqual(
                new[] { MarketKind.Moneyline, MarketKind.DoubleChance, MarketKind.Handicap, MarketKind.WinningMargin },
                MarketDestinations.KindsIn(MarketDestination.Result));
            CollectionAssert.AreEqual(
                new[] { MarketKind.TotalGoals, MarketKind.TeamTotalGoals, MarketKind.TotalGoalsOddEven, MarketKind.BothTeamsToScore },
                MarketDestinations.KindsIn(MarketDestination.Goals));
            CollectionAssert.AreEqual(
                new[] { MarketKind.CorrectScore },
                MarketDestinations.KindsIn(MarketDestination.CorrectScore));
            CollectionAssert.AreEqual(
                new[] { MarketKind.TotalCorners, MarketKind.TeamTotalCorners },
                MarketDestinations.KindsIn(MarketDestination.Corners));
            CollectionAssert.AreEqual(
                new[] { MarketKind.TotalCards, MarketKind.TeamTotalCards },
                MarketDestinations.KindsIn(MarketDestination.Cards));
            CollectionAssert.AreEqual(
                new[] { MarketKind.AnytimeScorer, MarketKind.PlayerMultiScorer },
                MarketDestinations.KindsIn(MarketDestination.Players));
        }
    }
}
