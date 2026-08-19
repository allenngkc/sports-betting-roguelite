using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// <c>S102</c> (batch 117)'s gate. The defect, found on a rendered frame: ENTRY's contents
    /// block printed <c>CORRECT SCORE 52–64</c> and then, indented directly beneath it,
    /// <c>CORRECT SCORE 52–64</c> again — the same name AND the same range, one line apart —
    /// because the contents renders destination-then-markets unconditionally and CORRECT SCORE is
    /// the only destination today holding exactly one market that carries its own name.
    ///
    /// <para>The ruling: suppress the child line when it duplicates BOTH its parent's name AND its
    /// parent's range. Both conditions, deliberately — <see cref="SportsbookApp.ContentsChildIsRedundant"/>
    /// is that rule, exposed as a pure predicate precisely so it can be asserted here on cases that
    /// do not exist on today's sheet, not only on the one case that does. This file calls that SAME
    /// method the renderer calls (<c>SportsbookApp.BuildContentsBlock</c>) rather than re-deriving
    /// its logic, so the gate tracks production rather than agreeing with itself.</para>
    ///
    /// <para>Covers, in order: the four combinations of the rule's two conditions (the whole reason
    /// it checks two things instead of one — see <c>ContentsChildIsRedundant</c>'s doc comment for
    /// the S98 contrast), the empty-destination (S89) decision, ordinal/no-normalisation comparison,
    /// and finally a real <see cref="MarketSheet"/> built from a real engine slate (never a
    /// hand-faked <see cref="Matchup"/> — see <c>MarketSheetTests</c> for why) asserting that
    /// today's sheet suppresses exactly one child, CORRECT SCORE's, and nothing else.</para>
    /// </summary>
    public class ContentsSuppressionTests
    {
        // ------------------------------------------------------ the rule's two conditions, in full

        [Test]
        public void Same_name_and_same_range_is_suppressed()
        {
            string range = "70" + MarketSheet.EnDash + "80";
            Assert.IsTrue(
                SportsbookApp.ContentsChildIsRedundant("CORRECT SCORE", range, "CORRECT SCORE", range),
                "a child that repeats both its parent's name and its parent's range tells the "
                + "reader nothing the parent line didn't already say — S102 suppresses it");
        }

        [Test]
        public void Same_name_but_different_range_still_prints()
        {
            Assert.IsFalse(
                SportsbookApp.ContentsChildIsRedundant("CORRECT SCORE",
                    "70" + MarketSheet.EnDash + "80", "CORRECT SCORE",
                    "12" + MarketSheet.EnDash + "17"),
                "a differing range is new information even under a repeated name — the rule fires "
                + "on identity of BOTH fields, never on the name alone");
        }

        [Test]
        public void Different_name_but_same_range_still_prints()
        {
            string range = "70" + MarketSheet.EnDash + "80";
            Assert.IsFalse(
                SportsbookApp.ContentsChildIsRedundant("CORRECT SCORE", range, "MULTI SCORER", range),
                "a differing name is new information even under a repeated range — the rule fires "
                + "on identity of BOTH fields, never on the range alone");
        }

        [Test]
        public void Different_name_and_different_range_prints()
        {
            Assert.IsFalse(
                SportsbookApp.ContentsChildIsRedundant("GOALS", "12" + MarketSheet.EnDash + "29",
                    "TOTAL GOALS", "12" + MarketSheet.EnDash + "17"),
                "the ordinary case: a child whose name AND range both differ from its parent is "
                + "telling the reader something, and must print");
        }

        [Test]
        public void A_future_destinations_lone_but_differently_named_market_still_prints()
        {
            // The DD's own words: "a future destination holding one market under a DIFFERENT name
            // still prints the child, because then the child is telling the reader something." This
            // case does not exist on today's sheet — CARDS holds two kinds, not one — but the rule
            // must hold for it anyway, because the whole point of checking identity rather than
            // childlessness is that a lone child is not automatically redundant.
            Assert.IsFalse(
                SportsbookApp.ContentsChildIsRedundant("CARDS", "40" + MarketSheet.EnDash + "40",
                    "TOTAL CARDS", "40" + MarketSheet.EnDash + "40"),
                "a lone child under a DIFFERENT name than its parent must print even though the "
                + "range matches and the (hypothetical) section holds only that one market");
        }

        // ------------------------------------------------------------------ empty destinations (S89)

        [Test]
        public void An_empty_destination_with_its_lone_market_under_the_SAME_name_is_still_suppressed()
        {
            // S89: an empty section and its one empty group both print MarketSheet.NoPricesOffered
            // rather than a range. Decision (see the long comment on ContentsChildIsRedundant): the
            // rule still fires here. Repeating "no prices offered" under a repeated name is exactly
            // as redundant as repeating a real range under a repeated name — the heading line alone
            // already completes the statement of the destination's emptiness, so the child would add
            // nothing were it printed.
            Assert.IsTrue(
                SportsbookApp.ContentsChildIsRedundant("CORRECT SCORE", MarketSheet.NoPricesOffered,
                    "CORRECT SCORE", MarketSheet.NoPricesOffered),
                "S102's empty-destination decision: both parent and child collapse to \"no prices "
                + "offered\" under the SAME name, which is still an identical repeat and is still "
                + "suppressed");
        }

        [Test]
        public void An_empty_destinations_lone_but_differently_named_market_still_prints()
        {
            Assert.IsFalse(
                SportsbookApp.ContentsChildIsRedundant("CORRECT SCORE", MarketSheet.NoPricesOffered,
                    "MULTI SCORER", MarketSheet.NoPricesOffered),
                "even empty, a differently-named lone child still prints — emptiness does not "
                + "change which two conditions the rule checks");
        }

        // --------------------------------------------------------------- exact string equality only

        [Test]
        public void Comparison_is_ordinal_with_no_normalisation_invented()
        {
            Assert.IsFalse(
                SportsbookApp.ContentsChildIsRedundant("CORRECT SCORE", "70-80", "CORRECT SCORE",
                    "70" + MarketSheet.EnDash + "80"),
                "a hyphen-minus range is not the SAME string as an en-dash range, even though a "
                + "reader might see them as the same shape — no normalisation is invented for the "
                + "occasion");
            Assert.IsFalse(
                SportsbookApp.ContentsChildIsRedundant("Correct Score", "1-1", "CORRECT SCORE", "1-1"),
                "case is not folded — a differently-cased name is a different string");
        }

        // ---------------------------------------------------------------- today's sheet (real engine)

        private static readonly string[] Seeds = { "S102-GATE-A", "S102-GATE-B", "S102-GATE-C" };

        [Test]
        public void Todays_sheet_suppresses_exactly_CORRECT_SCOREs_child_and_nothing_else()
        {
            // Matchups come from the engine's own public API (Run -> CurrentSlate.Matchups), never
            // hand-faked — the same construction MarketSheetTests uses, and for the same reason: a
            // hand-made Matchup would not have gone through MatchModel.BuildOffers, so it would
            // test the rule against a market set the game never produces.
            int sheetsChecked = 0;
            foreach (string seed in Seeds)
            {
                var run = new Run(seed, new RunConfig());
                IReadOnlyList<Matchup> matchups = run.CurrentSlate.Matchups;
                Assert.IsTrue(matchups.Count > 0, seed + ": the slate must produce matchups to test against");

                foreach (Matchup matchup in matchups)
                {
                    sheetsChecked++;
                    MarketSheet sheet = MarketSheet.Build(matchup);
                    string witness = seed + " matchup " + matchup.Index.ToString(CultureInfo.InvariantCulture);

                    MarketSheetSection suppressedSection = null;
                    MarketSheetGroup suppressedGroup = null;
                    int suppressedCount = 0;

                    foreach (MarketSheetSection section in sheet.Sections)
                    {
                        foreach (MarketSheetGroup group in section.Groups)
                        {
                            if (!SportsbookApp.ContentsChildIsRedundant(section.Label,
                                section.RangeText, group.Label, group.RangeText))
                                continue;
                            suppressedCount++;
                            suppressedSection = section;
                            suppressedGroup = group;
                        }
                    }

                    Assert.AreEqual(1, suppressedCount, witness
                        + ": today's sheet holds exactly one destination whose sole market shares "
                        + "its name — CORRECT SCORE — so exactly one child line must be suppressed, "
                        + "no more and no fewer");
                    Assert.AreEqual(MarketDestination.CorrectScore, suppressedSection.Destination, witness);
                    Assert.AreEqual(MarketKind.CorrectScore, suppressedGroup.Kind, witness);

                    // Every OTHER destination must still print every one of its children.
                    foreach (MarketSheetSection section in sheet.Sections)
                    {
                        if (section.Destination == MarketDestination.CorrectScore) continue;
                        foreach (MarketSheetGroup group in section.Groups)
                        {
                            Assert.IsFalse(SportsbookApp.ContentsChildIsRedundant(section.Label,
                                section.RangeText, group.Label, group.RangeText),
                                witness + ": " + section.Label + " / " + group.Label + " must "
                                + "print — only CORRECT SCORE's lone child is redundant with its "
                                + "parent today");
                        }
                    }
                }
            }

            Assert.IsTrue(sheetsChecked > 0, "the gate must actually walk at least one real sheet");
        }
    }
}
