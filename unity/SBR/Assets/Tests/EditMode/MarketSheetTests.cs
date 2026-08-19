using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// Spec §7's gate for <see cref="MarketSheet"/> (spec-market-surfaces-2026-08-17.md): the sheet
    /// loses nothing and duplicates nothing, its line numbering is exactly 1..N, its folio and
    /// contents ranges are DERIVED from those rows rather than authored beside them, and the six
    /// destinations print whatever happens to be priced.
    ///
    /// <para><c>C51</c>: a cross-element invariant is an assertion or it does not exist. Every test
    /// below runs across several run seeds and every matchup on each slate — <see cref="Sheets"/> —
    /// because the two conditionally-priced kinds (<see cref="MarketKind.CorrectScore"/> and
    /// <see cref="MarketKind.PlayerMultiScorer"/>, both gated on <c>RunConfig.CorrectScoreFloor</c>)
    /// make the row count vary matchup to matchup, and a one-seed gate would pin a number instead of
    /// an invariant.</para>
    ///
    /// <para>Matchups come from the engine's own public API (<see cref="Run"/>, which drives
    /// <c>SlateGenerator</c>) and are never hand-faked: a hand-made <see cref="Matchup"/> would not
    /// have gone through <c>MatchModel.BuildOffers</c>, so it would test the sheet against a market
    /// set the game never produces.</para>
    /// </summary>
    public class MarketSheetTests
    {
        /// <summary>Several seeds, not one — see the class comment.</summary>
        private static readonly string[] Seeds =
        {
            "SHEET-A", "SHEET-B", "SHEET-C", "SHEET-D",
            "SHEET-E", "SHEET-F", "SHEET-G", "SHEET-H",
        };

        // The rail order, restated here rather than read back from MarketDestinations.All so this
        // file asserts the ORDER instead of agreeing with whatever that list happens to say.
        //
        // S95 (DD 2026-08-18, batch 113) moved CORRECT SCORE from third to FIFTH, resolving the
        // spec's self-contradiction — §3's table seated it third, §5.2's contents example fifth, and
        // the illustration was the considered half. The order now runs the three countable
        // statistics adjacently (GOALS · CORNERS · CARDS), which is the same statistic-vs-bet-type
        // seam §3 uses to justify the taxonomy, and it stops seating one of the least-bet markets
        // third.
        //
        // This array being INDEPENDENT is what made it worth having: it went stale the moment the
        // enum moved, and it compiles clean — so it fails at run time, loudly, instead of silently
        // agreeing with the change. That is the whole point of not reading it back.
        private static readonly MarketDestination[] RailOrder =
        {
            MarketDestination.Result,
            MarketDestination.Goals,
            MarketDestination.Corners,
            MarketDestination.Cards,
            MarketDestination.CorrectScore,
            MarketDestination.Players,
        };

        private struct Case
        {
            public string Witness;
            public Matchup Matchup;
            public MarketSheet Sheet;
        }

        /// <summary>Every matchup on every seed's opening slate, with its sheet built.</summary>
        private static IEnumerable<Case> Sheets()
        {
            foreach (string seed in Seeds)
            {
                var run = new Run(seed, new RunConfig());
                IReadOnlyList<Matchup> matchups = run.CurrentSlate.Matchups;
                for (int i = 0; i < matchups.Count; i++)
                {
                    yield return new Case
                    {
                        Witness = seed + " matchup " + i.ToString(CultureInfo.InvariantCulture),
                        Matchup = matchups[i],
                        Sheet = MarketSheet.Build(matchups[i]),
                    };
                }
            }
        }

        // -------------------------------------------------------- nothing lost, nothing duplicated

        [Test]
        public void Every_priced_offer_reaches_exactly_one_row()
        {
            // C19 ("every priced offer reachable on the betting surface") made STRUCTURAL rather
            // than promised: the sheet is a re-ordering of matchup.Markets, so a market kind that
            // loses its home stops being bettable and this fails.
            int checkedMatchups = 0;
            foreach (Case c in Sheets())
            {
                checkedMatchups++;
                Assert.AreEqual(c.Matchup.Markets.Count, c.Sheet.AllRows.Count,
                    c.Witness + ": AllRows must hold one row per priced offer");
                Assert.AreEqual(c.Matchup.Markets.Count, c.Sheet.TotalRows,
                    c.Witness + ": TotalRows is the folio's denominator and must be the real count");

                // Reference identity, not value equality — two offers on the same kind can price
                // identically, and a set keyed on value would hide a duplicated row as a match.
                var seen = new HashSet<MarketOffer>(new ByReference());
                foreach (MarketSheetRow row in c.Sheet.AllRows)
                {
                    Assert.IsTrue(seen.Add(row.Offer),
                        c.Witness + ": line " + row.Line + " repeats an offer already on the sheet");
                    Assert.AreEqual(row.Offer.Selection.Kind, row.Kind,
                        c.Witness + ": row Kind disagrees with the offer it carries");
                }
                foreach (MarketOffer offer in c.Matchup.Markets)
                {
                    Assert.IsTrue(seen.Contains(offer),
                        c.Witness + ": " + offer.Selection.Kind
                        + " is priced but reaches no row — it cannot be bet");
                }

                // And every row is seated under a destination that claims its kind.
                foreach (MarketSheetSection section in c.Sheet.Sections)
                {
                    foreach (MarketSheetRow row in section.Rows)
                    {
                        Assert.AreEqual(section.Destination, MarketDestinations.For(row.Kind),
                            c.Witness + ": line " + row.Line + " (" + row.Kind + ") sits under "
                            + section.Label + ", which is not its destination");
                    }
                }
            }
            Assert.AreEqual(Seeds.Length * new RunConfig().MatchupsPerSlate, checkedMatchups,
                "the gate must run across every seed's whole slate");
        }

        // ------------------------------------------------------------------------- the numbering

        [Test]
        public void Line_numbers_are_exactly_one_through_TotalRows()
        {
            // The folio's "of 80" and the contents block's "12–29" are two readings of ONE
            // numbering (§5.1, §5.2). A gap or a repeat here makes them disagree with each other
            // and with the page.
            foreach (Case c in Sheets())
            {
                var seen = new HashSet<int>();
                for (int i = 0; i < c.Sheet.AllRows.Count; i++)
                {
                    MarketSheetRow row = c.Sheet.AllRows[i];
                    Assert.AreEqual(i + 1, row.Line,
                        c.Witness + ": AllRows must be in printed order, so AllRows[i].Line == i + 1");
                    Assert.IsTrue(seen.Add(row.Line),
                        c.Witness + ": line " + row.Line + " is used twice");
                }
                for (int n = 1; n <= c.Sheet.TotalRows; n++)
                {
                    Assert.IsTrue(seen.Contains(n),
                        c.Witness + ": line " + n + " is missing — the numbering has a gap");
                }
                Assert.AreEqual(c.Sheet.TotalRows, seen.Count,
                    c.Witness + ": the line numbers must be exactly 1.." + c.Sheet.TotalRows);
            }
        }

        // ------------------------------------------------- the ranges are derived, not authored

        [Test]
        public void Section_and_group_ranges_are_the_min_and_max_of_their_own_rows()
        {
            // S74-am3: a constant that happens to equal the right answer is a constant that will
            // stop equalling it. FirstLine/LastLine are asserted against the rows themselves, so a
            // range carried alongside the rows rather than read off them fails here.
            foreach (Case c in Sheets())
            {
                foreach (MarketSheetSection section in c.Sheet.Sections)
                {
                    AssertRangeMatchesRows(c.Witness + " / " + section.Label,
                        section.Rows, section.FirstLine, section.LastLine, section.Count,
                        section.IsEmpty, section.RangeText);

                    foreach (MarketSheetGroup group in section.Groups)
                    {
                        AssertRangeMatchesRows(c.Witness + " / " + section.Label + " / " + group.Label,
                            group.Rows, group.FirstLine, group.LastLine, group.Count,
                            group.IsEmpty, group.RangeText);

                        // Spec §7.4: the contents block's two levels must agree — a group's range
                        // nests inside the range of the destination that prints it.
                        if (!group.IsEmpty)
                        {
                            Assert.IsFalse(section.IsEmpty,
                                c.Witness + ": " + section.Label + " prints empty while its group "
                                + group.Label + " has rows");
                            Assert.GreaterOrEqual(group.FirstLine, section.FirstLine,
                                c.Witness + ": " + group.Label + " starts before its section");
                            Assert.LessOrEqual(group.LastLine, section.LastLine,
                                c.Witness + ": " + group.Label + " ends after its section");
                        }
                    }
                }
            }
        }

        [Test]
        public void Sections_partition_the_whole_sheet_in_rail_order()
        {
            // No overlap and no gap: walking the rail top to bottom must consume 1..N exactly once,
            // which is what lets the contents block be read as a page rather than a lookup table.
            foreach (Case c in Sheets())
            {
                int expectedNext = 1;
                foreach (MarketSheetSection section in c.Sheet.Sections)
                {
                    // An empty destination still prints (§3.1/§5.3); it simply consumes no lines.
                    if (section.IsEmpty) continue;

                    Assert.AreEqual(expectedNext, section.FirstLine,
                        c.Witness + ": " + section.Label + " starts at " + section.FirstLine
                        + " but the previous section ended at " + (expectedNext - 1)
                        + " — the rail must partition 1.." + c.Sheet.TotalRows + " with no gap or overlap");
                    Assert.AreEqual(section.Count, section.LastLine - section.FirstLine + 1,
                        c.Witness + ": " + section.Label + "'s range is not contiguous");
                    expectedNext = section.LastLine + 1;

                    // Same walk one level down: the groups fill their section in KindsIn order.
                    int groupNext = section.FirstLine;
                    foreach (MarketSheetGroup group in section.Groups)
                    {
                        if (group.IsEmpty) continue;
                        Assert.AreEqual(groupNext, group.FirstLine,
                            c.Witness + ": " + section.Label + " / " + group.Label
                            + " does not follow the previous group");
                        groupNext = group.LastLine + 1;
                    }
                    Assert.AreEqual(section.LastLine + 1, groupNext,
                        c.Witness + ": " + section.Label + "'s groups do not fill it");
                }
                Assert.AreEqual(c.Sheet.TotalRows + 1, expectedNext,
                    c.Witness + ": the sections stop short of the sheet's " + c.Sheet.TotalRows + " rows");
            }
        }

        // ------------------------------------------------------ the destination set is a constant

        [Test]
        public void All_six_destinations_print_on_every_matchup_in_rail_order()
        {
            // Spec §3.1 / §7.2. Every book generates its rail per event; ours cannot and must not,
            // because empty groups print — which is what buys the no-reflow read for free.
            foreach (Case c in Sheets())
            {
                Assert.AreEqual(RailOrder.Length, c.Sheet.Sections.Count,
                    c.Witness + ": the rail is a six-entry constant, not a function of what is priced");
                for (int i = 0; i < RailOrder.Length; i++)
                {
                    Assert.AreEqual(RailOrder[i], c.Sheet.Sections[i].Destination,
                        c.Witness + ": rail position " + i + " is out of spec §3 order");
                    Assert.AreEqual(MarketDestinations.Label(RailOrder[i]), c.Sheet.Sections[i].Label,
                        c.Witness + ": rail position " + i + " prints the wrong label");
                    Assert.AreSame(c.Sheet.Sections[i], c.Sheet.Section(RailOrder[i]),
                        c.Witness + ": Section(" + RailOrder[i] + ") must return that same section");
                }

                // §5.3: EVERY kind the destination holds is printed, empty ones included — that is
                // the clause §3.1's constancy actually rests on.
                foreach (MarketSheetSection section in c.Sheet.Sections)
                {
                    IReadOnlyList<MarketKind> kinds = MarketDestinations.KindsIn(section.Destination);
                    Assert.AreEqual(kinds.Count, section.Groups.Count,
                        c.Witness + ": " + section.Label + " must print all " + kinds.Count
                        + " of its kinds, including any with no offers");
                    for (int i = 0; i < kinds.Count; i++)
                    {
                        Assert.AreEqual(kinds[i], section.Groups[i].Kind,
                            c.Witness + ": " + section.Label + " group " + i + " is out of KindsIn order");
                        Assert.AreEqual(MarketDestinations.KindLabel(kinds[i]), section.Groups[i].Label,
                            c.Witness + ": " + section.Label + " group " + i + " prints the wrong label");
                    }
                }
            }
        }

        // ---------------------------------------------------------------------------- the row text

        [Test]
        public void Every_row_prints_a_name()
        {
            // Catches any kind whose MarketFields leaves BOTH Line and Subject blank: such a row
            // would render as leader dots into a price with nothing to lead from.
            foreach (Case c in Sheets())
            {
                foreach (MarketSheetRow row in c.Sheet.AllRows)
                {
                    Assert.IsFalse(string.IsNullOrEmpty(row.Name),
                        c.Witness + ": line " + row.Line + " (" + row.Kind
                        + ") has no printable name — MarketFields gave neither Line nor Subject");
                    Assert.IsNotNull(row.Role,
                        c.Witness + ": line " + row.Line + " has a null Role (empty, never null)");
                }
            }
        }

        [Test]
        public void Row_names_are_composed_from_the_engine_fields()
        {
            // S22 / DD batch 4: the engine emits fields, the surface composes. Asserted against
            // MatchModel.Fields directly — MatchModel.DisplayLabel is the legacy packed form and is
            // forbidden for this surface, so a row that started matching it would fail here.
            bool sawLine = false, sawSubject = false;
            foreach (Case c in Sheets())
            {
                foreach (MarketSheetRow row in c.Sheet.AllRows)
                {
                    MatchModel.MarketFields fields = MatchModel.Fields(c.Matchup, row.Offer.Selection);

                    // BTTS is the one ruled exception, and it is A2's, not this build's: both BTTS
                    // offers emit Line "BOTH TEAMS TO SCORE" with an empty Subject, so the general
                    // rule below would print two identical rows at two different prices. The choice
                    // lives in Fields.Market ("BTTS — YES"/"BTTS — NO"), which is exactly what
                    // SportsbookApp.BuildBothTeamsScore already prints on the shipped surface for
                    // the same stated reason. Pinned here so the exception stays deliberate — and
                    // narrow: every other kind still has to obey Line-else-Subject.
                    string expected = row.Kind == MarketKind.BothTeamsToScore
                        ? fields.Market
                        : string.IsNullOrEmpty(fields.Line) ? fields.Subject : fields.Line;
                    Assert.AreEqual(expected, row.Name,
                        c.Witness + ": line " + row.Line + " (" + row.Kind + ") must print the "
                        + "engine's offer LINE, or its SUBJECT where there is no line"
                        + " (BTTS excepted: its choice is only in Fields.Market, per A2)");
                    Assert.AreEqual(fields.Role, row.Role,
                        c.Witness + ": line " + row.Line + " must carry MarketFields.Role verbatim");
                    if (string.IsNullOrEmpty(fields.Line)) sawSubject = true; else sawLine = true;
                }
            }
            // Both arms of the composition rule are actually exercised: the moneyline's home and
            // away rows are the only offers the engine gives no line for.
            Assert.IsTrue(sawLine, "no row exercised the LINE arm of the naming rule");
            Assert.IsTrue(sawSubject, "no row exercised the SUBJECT arm (moneyline home/away)");
        }

        [Test]
        public void No_two_rows_on_a_sheet_print_the_same_name()
        {
            // ============================ THIS GATE CURRENTLY FAILS ============================
            // MEASURED on 3,000 engine-generated matchups: it fails on 100% of them, always the
            // same single pair, and nothing else on the sheet ever collides.
            //
            //     line 30  "BOTH TEAMS TO SCORE"  @ 1.92     <- BTTS YES
            //     line 31  "BOTH TEAMS TO SCORE"  @ 1.89     <- BTTS NO
            //
            // MatchModel.Fields gives BOTH BTTS offers the SAME Line ("BOTH TEAMS TO SCORE") and
            // an empty Subject; the YES/NO distinction lives only in MarketFields.Market
            // ("BTTS — YES" / "BTTS — NO"). The ruled composition — Line, else Subject — therefore
            // drops the only field that tells the two offers apart, and the group heading above
            // them reads "BOTH TEAMS TO SCORE" as well, so the sheet states the same sentence three
            // times and never says YES or NO.
            //
            // It is left FAILING rather than waived, because it is not a defect in this file: two
            // identical rows at different prices, on a surface where §4.1 rules one offer per row
            // and §4.3 rules the row is one statement, is the §7.4 "NEED 0" class — the player
            // cannot bet the offer they mean. The wording is DD's call, so this lane does not
            // invent it. Two ways out, neither of them an engine change to grading:
            //   (A) surface-side, one arm in MarketSheet.NameOf: take Fields.Market for
            //       BothTeamsToScore, giving "BTTS — YES" / "BTTS — NO". Keeps S22 intact — the
            //       engine emitted the distinguishing field, the surface just has to compose it.
            //   (B) engine-side: have Fields emit the choice in Line for BTTS.
            // ===================================================================================
            foreach (Case c in Sheets())
            {
                var byName = new Dictionary<string, MarketSheetRow>();
                foreach (MarketSheetRow row in c.Sheet.AllRows)
                {
                    if (byName.TryGetValue(row.Name, out MarketSheetRow first))
                    {
                        Assert.Fail(c.Witness + ": lines " + first.Line + " and " + row.Line
                            + " both print \"" + row.Name + "\" at different prices ("
                            + first.Offer.Odds.ToString("0.00", CultureInfo.InvariantCulture) + " and "
                            + row.Offer.Odds.ToString("0.00", CultureInfo.InvariantCulture)
                            + "). Kinds " + first.Kind + " / " + row.Kind
                            + ". The player cannot tell the two offers apart, so they cannot bet the "
                            + "one they mean — see this test's comment for the two ways out.");
                    }
                    byName.Add(row.Name, row);
                }
            }
        }

        // --------------------------------------------------------------------------- the folio

        [Test]
        public void Folio_counts_the_rows_that_are_actually_on_the_sheet()
        {
            // Spec §7.3. A folio that lies is worse than no folio, because its whole value is that
            // it is true inside a game about being lied to.
            foreach (Case c in Sheets())
            {
                int total = c.Sheet.TotalRows;
                Assert.AreEqual(c.Matchup.Markets.Count, total,
                    c.Witness + ": the denominator must be the real priced count, never a constant");

                Assert.AreEqual("1" + MarketSheet.EnDash + total + " of " + total,
                    c.Sheet.Folio(1, total), c.Witness + ": full-extent folio");

                int last = Math.Min(8, total);
                Assert.AreEqual("1" + MarketSheet.EnDash + last + " of " + total,
                    c.Sheet.Folio(1, last), c.Witness + ": first-screen folio");

                if (total >= 66)
                {
                    Assert.AreEqual("46" + MarketSheet.EnDash + "66 of " + total,
                        c.Sheet.Folio(46, 66), c.Witness + ": §5.1's worked example window");
                }
            }
        }

        [Test]
        public void Folio_and_contents_ranges_are_set_with_an_en_dash()
        {
            // §5.1 prints "46–66 of 80" with U+2013. Asserted as a CODEPOINT so a hyphen-minus
            // pasted in, or a source re-encoding, fails rather than passing on a lookalike.
            Assert.AreEqual(1, MarketSheet.EnDash.Length, "EN DASH is one character");
            Assert.AreEqual(0x2013, (int)MarketSheet.EnDash[0],
                "the folio and the contents ranges take an EN DASH (U+2013), not a hyphen-minus (U+002D)");

            foreach (Case c in Sheets())
            {
                StringAssert.Contains(MarketSheet.EnDash, c.Sheet.Folio(1, c.Sheet.TotalRows),
                    c.Witness + ": the folio's range is not set with an EN DASH");
                foreach (MarketSheetSection section in c.Sheet.Sections)
                {
                    if (section.IsEmpty) continue;
                    StringAssert.Contains(MarketSheet.EnDash, section.RangeText,
                        c.Witness + ": " + section.Label + "'s contents range is not set with an EN DASH");
                }
            }
        }

        [Test]
        public void Folio_refuses_a_window_that_is_not_on_the_sheet()
        {
            // The guard is the honesty rule expressed as code: an impossible claim fails here
            // rather than being printed.
            MarketSheet sheet = MarketSheet.Build(new Run(Seeds[0], new RunConfig()).CurrentSlate.Matchups[0]);
            int total = sheet.TotalRows;

            Assert.Throws<ArgumentOutOfRangeException>(() => sheet.Folio(0, 10), "line 0 does not exist");
            Assert.Throws<ArgumentOutOfRangeException>(() => sheet.Folio(1, total + 1),
                "the window cannot run past the last row");
            Assert.Throws<ArgumentOutOfRangeException>(() => sheet.Folio(20, 10),
                "the window cannot end before it starts");
            Assert.DoesNotThrow(() => sheet.Folio(1, 1), "a one-row window is a real window");
        }

        // --------------------------------------------------------------------- empty groups (S89)

        [Test]
        public void A_group_with_offers_prints_its_count_and_a_group_without_prints_no_prices_offered()
        {
            // §5.3 / S89: a racecard prints the race even when it is abandoned.
            Assert.AreEqual("no prices offered", MarketSheet.NoPricesOffered,
                "S89's wording is lowercase and verbatim");
            Assert.AreEqual(MarketSheet.NoPricesOffered, MarketSheet.CountText(0));
            Assert.AreEqual(MarketSheet.NoPricesOffered, MarketSheet.RangeText(0, 0));
            Assert.AreEqual("11", MarketSheet.CountText(11));
            Assert.AreEqual("12" + MarketSheet.EnDash + "29", MarketSheet.RangeText(12, 29));

            foreach (Case c in Sheets())
            {
                foreach (MarketSheetSection section in c.Sheet.Sections)
                {
                    AssertCountForm(c.Witness + " / " + section.Label,
                        section.Count, section.IsEmpty, section.CountText, section.RangeText,
                        section.FirstLine, section.LastLine);
                    foreach (MarketSheetGroup group in section.Groups)
                    {
                        AssertCountForm(c.Witness + " / " + section.Label + " / " + group.Label,
                            group.Count, group.IsEmpty, group.CountText, group.RangeText,
                            group.FirstLine, group.LastLine);
                    }
                }
            }
        }

        [Test]
        public void An_empty_group_still_prints_and_the_sheet_around_it_still_numbers_cleanly()
        {
            // MEASURED, and worth stating plainly: at the SHIPPED RunConfig no group is ever empty
            // — swept 3,000 run seeds (18,000 matchups), CORRECT SCORE floors at 11 rows and MULTI
            // SCORER at 3. So the S89 state is real code but unreached data, and the only dial that
            // reaches it is CorrectScoreFloor, which gates both conditional kinds.
            //
            // This test therefore raises that floor to reach the state on a pinned seed rather than
            // hand-faking an empty group: the sheet under assertion is still one the engine priced.
            var config = new RunConfig { CorrectScoreFloor = 0.08 };
            var run = new Run("SHEET-EMPTY", config);

            MarketSheetGroup empty = null;
            MarketSheet sheet = null;
            foreach (Matchup matchup in run.CurrentSlate.Matchups)
            {
                MarketSheet candidate = MarketSheet.Build(matchup);
                foreach (MarketSheetSection section in candidate.Sections)
                {
                    foreach (MarketSheetGroup group in section.Groups)
                    {
                        if (!group.IsEmpty) continue;
                        empty = group;
                        sheet = candidate;
                        break;
                    }
                    if (empty != null) break;
                }
                if (empty != null) break;
            }

            Assert.IsNotNull(empty,
                "raising CorrectScoreFloor to 0.08 must empty a conditionally-priced group — if it "
                + "no longer does, S89's 'no prices offered' state has become unreachable and the "
                + "spec's §8 evidence item 3 cannot be shot at all");

            Assert.AreEqual(0, empty.Count);
            Assert.IsTrue(empty.IsEmpty);
            Assert.AreEqual(0, empty.FirstLine, "an empty group claims no line");
            Assert.AreEqual(0, empty.LastLine, "an empty group claims no line");
            Assert.AreEqual(MarketSheet.NoPricesOffered, empty.CountText);
            Assert.AreEqual(MarketSheet.NoPricesOffered, empty.RangeText);

            // The empty group is still PRINTED — it is in its section's Groups list, in KindsIn
            // order — and it costs the numbering nothing.
            MarketSheetSection home = sheet.Section(MarketDestinations.For(empty.Kind));
            CollectionAssert.Contains(home.Groups, empty,
                "an empty group is printed, not dropped (§5.3)");
            Assert.AreEqual(6, sheet.Sections.Count, "the rail is unchanged by an empty group (§3.1)");
            for (int i = 0; i < sheet.AllRows.Count; i++)
            {
                Assert.AreEqual(i + 1, sheet.AllRows[i].Line,
                    "the numbering skips no line and leaves no hole where the empty group sits");
            }
            Assert.AreEqual(sheet.AllRows.Count, sheet.TotalRows);
        }

        // ------------------------------------------------------------------------------- helpers

        private static void AssertRangeMatchesRows(string witness, IReadOnlyList<MarketSheetRow> rows,
            int firstLine, int lastLine, int count, bool isEmpty, string rangeText)
        {
            Assert.AreEqual(rows.Count, count, witness + ": Count must be the row count");
            Assert.AreEqual(rows.Count == 0, isEmpty, witness + ": IsEmpty must agree with Count");

            if (rows.Count == 0)
            {
                Assert.AreEqual(0, firstLine, witness + ": nothing printed, so no first line");
                Assert.AreEqual(0, lastLine, witness + ": nothing printed, so no last line");
                Assert.AreEqual(MarketSheet.NoPricesOffered, rangeText, witness);
                return;
            }

            int min = int.MaxValue, max = int.MinValue;
            foreach (MarketSheetRow row in rows)
            {
                if (row.Line < min) min = row.Line;
                if (row.Line > max) max = row.Line;
            }
            Assert.AreEqual(min, firstLine, witness + ": FirstLine must be the min over its own rows");
            Assert.AreEqual(max, lastLine, witness + ": LastLine must be the max over its own rows");
            Assert.AreEqual(min + MarketSheet.EnDash + max, rangeText,
                witness + ": the printed contents range must be read off those same rows");
        }

        private static void AssertCountForm(string witness, int count, bool isEmpty,
            string countText, string rangeText, int firstLine, int lastLine)
        {
            if (count == 0)
            {
                Assert.IsTrue(isEmpty, witness + ": Count 0 must report IsEmpty");
                Assert.AreEqual(MarketSheet.NoPricesOffered, countText, witness + ": S89's count form");
                Assert.AreEqual(MarketSheet.NoPricesOffered, rangeText, witness + ": S89's range form");
            }
            else
            {
                Assert.IsFalse(isEmpty, witness + ": a group with offers is not empty");
                Assert.AreEqual(count.ToString(CultureInfo.InvariantCulture), countText,
                    witness + ": a group with offers prints its count (§5.3)");
                Assert.AreEqual(firstLine + MarketSheet.EnDash + lastLine, rangeText, witness);
            }
        }

        /// <summary>Identity, not value: two offers on one kind can price identically, and a
        /// value-keyed set would read a duplicated row as a match instead of a defect.</summary>
        private sealed class ByReference : IEqualityComparer<MarketOffer>
        {
            public bool Equals(MarketOffer a, MarketOffer b) => ReferenceEquals(a, b);
            public int GetHashCode(MarketOffer o)
                => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(o);
        }
    }
}
