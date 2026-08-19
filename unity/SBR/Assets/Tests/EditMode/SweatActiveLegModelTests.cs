using System.Collections.Generic;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;
using static SBR.Game.SweatActiveLegModel;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// Phase 3A (PRD §8.2, §9): table-driven, EditMode-only coverage for
    /// <see cref="SweatActiveLegModel"/> — pure NEED/LIVE market copy. No Unity types, no
    /// scene playback, no engine RNG. Covers every market's NEED string exactly, LIVE
    /// progress at several revealed states (including zero and at-the-line), the BTTS-No
    /// causal-reveal gate, the anytime-scorer WAITING/SCORED gate, the non-team
    /// "MARKET PICK" treatment, and a dedicated no-leak pin using the real
    /// <see cref="ScoreLedger"/>/<see cref="CountLedger"/> revealed/target split.
    /// </summary>
    public class SweatActiveLegModelTests
    {
        // ------------------------------------------------------------------------- 1. NEED, exact, all 8 markets

        [TestCase(true, "OVER 2.5 GOALS")]
        [TestCase(false, "UNDER 2.5 GOALS")]
        public void Total_goals_need_is_exact(bool over, string expected)
        {
            ActiveLegCopy copy = Describe(ActiveLegInput.TotalGoals(over, 2.5, 0, 0));
            Assert.AreEqual(expected, copy.Need);
        }

        [TestCase(true, "OVER 9.5 CORNERS")]
        [TestCase(false, "UNDER 9.5 CORNERS")]
        public void Total_corners_need_is_exact(bool over, string expected)
        {
            ActiveLegCopy copy = Describe(ActiveLegInput.TotalCorners(over, 9.5, 0, 0));
            Assert.AreEqual(expected, copy.Need);
        }

        [TestCase(true, "OVER 4.5 CARDS")]
        [TestCase(false, "UNDER 4.5 CARDS")]
        public void Total_cards_need_is_exact(bool over, string expected)
        {
            ActiveLegCopy copy = Describe(ActiveLegInput.TotalCards(over, 4.5, 0, 0));
            Assert.AreEqual(expected, copy.Need);
        }

        [Test]
        public void Moneyline_need_is_the_backed_team_to_win()
        {
            Assert.AreEqual("ARSENAL TO WIN", Describe(ActiveLegInput.Moneyline("Arsenal", 0, 0)).Need);
            Assert.AreEqual("SUNDERLAND TO WIN", Describe(ActiveLegInput.Moneyline("sunderland", 1, 2)).Need);
        }

        [Test]
        public void Btts_yes_need_is_exact()
        {
            // G1 re-authored: "BOTH TEAMS TO SCORE" (19) was a permanently marginal CONSTANT against
            // the 249px NEED column — no variable in it, so it was over budget on every frame.
            Assert.AreEqual("BOTH TEAMS SCORE", Describe(ActiveLegInput.BothTeamsToScore(true, 0, 0)).Need);
        }

        [Test]
        public void Btts_no_need_is_exact()
        {
            // G1 re-authored: "KEEP ONE TEAM SCORELESS" (23) was over budget as a constant, and
            // "KEEP" was a §8 register problem too — an instruction about a thing he cannot
            // influence. The requirement is a state of the match, so the copy names the state.
            ActiveLegCopy no = Describe(ActiveLegInput.BothTeamsToScore(false, 0, 0));
            Assert.AreEqual("ONE TEAM SCORELESS", no.Need);
            Assert.AreEqual("ONE TEAM BLANKED", no.NeedFallback,
                "G1 authored a shorter line for this form; truncation is the backstop, never the remedy.");
        }

        [Test]
        public void Anytime_scorer_need_is_the_backed_player_to_score()
        {
            // G1 re-authored: players are named by SURNAME — the convention the progress line
            // already used. This is the T69 case itself: "RICO LANYARD TO SCORE" is the string that
            // rendered as "RICO LANYARD TO".
            // G1-am8 (batch 63): the fallback is rung 2 of a two-rung ladder, NOT the bare form.
            // `TO SCORE` named no player — and worse on this arm than on the moneyline one, because
            // the backed-side marker renders only on moneyline legs, so a scorer leg has no marker at
            // all and nothing else on the surface names him. G1's pair-defect ruling is what makes it
            // decisive: the progress line reads NOT YET/SCORED precisely BECAUSE the surname is named
            // once, by this line. Retire the surname here and it is named nowhere.
            //
            // This assertion caught the change when the ruling landed, which is the point of pinning
            // a fallback: the contract moved, so the pin moves with it — deliberately, not silently.
            ActiveLegCopy copy = Describe(ActiveLegInput.AnytimeScorer("Harry Kane", false));
            Assert.AreEqual("KANE TO SCORE", copy.Need);
            Assert.AreEqual("KANE SCORES", copy.NeedFallback,
                "G1-am8: rung 2 conjugates to the subject and keeps the surname — bare `TO SCORE` is retired");
        }

        // ------------------------------------------------------------------------- 2. LIVE progress: zero, mid, at-the-line

        [TestCase(0, 0, "LEVEL 0–0")]
        [TestCase(2, 1, "LEADING 2–1")]
        [TestCase(1, 2, "TRAILING 1–2")]
        [TestCase(3, 3, "LEVEL 3–3")]
        public void Moneyline_live_progress(int revealedFor, int revealedAgainst, string expected)
        {
            ActiveLegCopy copy = Describe(ActiveLegInput.Moneyline("Home Side", revealedFor, revealedAgainst));
            Assert.AreEqual(expected, copy.Live);
        }

        [TestCase(0, 0, "0 GOALS • 3 MORE")]     // zero revealed
        [TestCase(1, 1, "2 GOALS • 1 MORE")]     // mid
        [TestCase(2, 1, "3 GOALS • WON")]        // exactly at the line: outcome selects WON, never "0 MORE"
        [TestCase(3, 2, "5 GOALS • WON")]        // already cleared — still WON, never a negative "more"
        public void Total_goals_over_live_progress_at_line_2_5(int revealedFor, int revealedAgainst, string expected)
        {
            ActiveLegCopy copy = Describe(ActiveLegInput.TotalGoals(true, 2.5, revealedFor, revealedAgainst));
            Assert.AreEqual(expected, copy.Live);
        }

        [TestCase(0, 0, "0 GOALS • LIMIT 2")]    // zero revealed
        [TestCase(1, 0, "1 GOALS • LIMIT 1")]    // mid
        [TestCase(1, 1, "2 GOALS • LIMIT 0")]    // exactly at the line — LIMIT 0 is TRUE and stays; still live
        [TestCase(2, 1, "3 GOALS • LOST")]       // already busted — outcome selects LOST, never a negative limit
        public void Total_goals_under_live_progress_at_line_2_5(int revealedFor, int revealedAgainst, string expected)
        {
            ActiveLegCopy copy = Describe(ActiveLegInput.TotalGoals(false, 2.5, revealedFor, revealedAgainst));
            Assert.AreEqual(expected, copy.Live);
        }

        [TestCase(0, 0, "0/2 TEAMS SCORED")]
        [TestCase(1, 0, "1/2 TEAMS SCORED")]
        [TestCase(0, 1, "1/2 TEAMS SCORED")]
        [TestCase(1, 1, "2/2 TEAMS SCORED")]
        [TestCase(3, 2, "2/2 TEAMS SCORED")]
        public void Btts_yes_live_progress(int revealedFor, int revealedAgainst, string expected)
        {
            ActiveLegCopy copy = Describe(ActiveLegInput.BothTeamsToScore(true, revealedFor, revealedAgainst));
            Assert.AreEqual(expected, copy.Live);
        }

        [TestCase(0, 0, "0 CORNERS • NEED 10")]
        [TestCase(3, 2, "5 CORNERS • NEED 5")]
        [TestCase(5, 5, "10 CORNERS • WON")]        // at the line — NEED 0 is unconstructible; WON instead
        [TestCase(7, 6, "13 CORNERS • WON")]        // already cleared
        public void Total_corners_over_live_progress_at_line_9_5(int revealedHome, int revealedAway, string expected)
        {
            ActiveLegCopy copy = Describe(ActiveLegInput.TotalCorners(true, 9.5, revealedHome, revealedAway));
            Assert.AreEqual(expected, copy.Live);
        }

        [TestCase(0, 0, "0 CORNERS • LIMIT 9")]
        [TestCase(4, 4, "8 CORNERS • LIMIT 1")]
        [TestCase(5, 4, "9 CORNERS • LIMIT 0")]     // at the line — LIMIT 0 is TRUE and stays; still live
        [TestCase(6, 5, "11 CORNERS • LOST")]       // already busted — outcome selects LOST
        public void Total_corners_under_live_progress_at_line_9_5(int revealedHome, int revealedAway, string expected)
        {
            ActiveLegCopy copy = Describe(ActiveLegInput.TotalCorners(false, 9.5, revealedHome, revealedAway));
            Assert.AreEqual(expected, copy.Live);
        }

        [TestCase(0, 0, "0 CARDS • NEED 5")]
        [TestCase(2, 2, "4 CARDS • NEED 1")]
        [TestCase(3, 2, "5 CARDS • WON")]           // NEED 0 is unconstructible; WON instead
        public void Total_cards_over_live_progress_at_line_4_5(int revealedHome, int revealedAway, string expected)
        {
            ActiveLegCopy copy = Describe(ActiveLegInput.TotalCards(true, 4.5, revealedHome, revealedAway));
            Assert.AreEqual(expected, copy.Live);
        }

        [TestCase(0, 0, "0 CARDS • LIMIT 4")]
        [TestCase(2, 1, "3 CARDS • LIMIT 1")]
        [TestCase(2, 2, "4 CARDS • LIMIT 0")]
        public void Total_cards_under_live_progress_at_line_4_5(int revealedHome, int revealedAway, string expected)
        {
            ActiveLegCopy copy = Describe(ActiveLegInput.TotalCards(false, 4.5, revealedHome, revealedAway));
            Assert.AreEqual(expected, copy.Live);
        }

        [Test]
        public void Whole_number_line_declines_to_fabricate_an_exact_remaining_count()
        {
            // No generator in this codebase produces a whole-number line (RunConfig lines are
            // all x.5), but the class must not guess at an exact "more"/"limit" count where a
            // push makes the boundary ambiguous — defensive fallback, not the common path.
            ActiveLegCopy over = Describe(ActiveLegInput.TotalGoals(true, 3.0, 1, 1));
            ActiveLegCopy under = Describe(ActiveLegInput.TotalGoals(false, 3.0, 1, 1));
            Assert.AreEqual("2 GOALS", over.Live);
            Assert.AreEqual("2 GOALS", under.Live);
        }

        // ------------------------------------------------------------------------- 3. BTTS No never claims success early

        [TestCase(0, 0, "CLEAN-SHEET PATH LIVE")]
        [TestCase(1, 0, "CLEAN-SHEET PATH LIVE")] // one side scored — the OTHER side's clean sheet is still live
        [TestCase(0, 1, "CLEAN-SHEET PATH LIVE")]
        [TestCase(1, 1, "BOTH HAVE SCORED")]      // only now is it causally true
        public void Btts_no_only_claims_both_scored_once_both_are_revealed(int revealedFor, int revealedAgainst, string expected)
        {
            ActiveLegCopy copy = Describe(ActiveLegInput.BothTeamsToScore(false, revealedFor, revealedAgainst));
            Assert.AreEqual(expected, copy.Live);
        }

        // ------------------------------------------------------------------------- 4. Anytime scorer gate

        [Test]
        public void Anytime_scorer_waits_for_surname_until_revealed_then_says_scored_only_at_payoff()
        {
            // G1's pair-defect: this line used to read "WAITING FOR KANE" directly under a NEED of
            // "KANE TO SCORE" — the surname twice, three lines apart, both saying the same thing.
            // T69's "a fact named twice" reproduced vertically. The player is named ONCE, above.
            ActiveLegCopy waiting = Describe(ActiveLegInput.AnytimeScorer("Harry Kane", scorerRevealed: false));
            Assert.AreEqual("NOT YET", waiting.Live);
            Assert.AreEqual("KANE TO SCORE", waiting.Need, "the surname belongs to NEED, and only to NEED");

            ActiveLegCopy scored = Describe(ActiveLegInput.AnytimeScorer("Harry Kane", scorerRevealed: true));
            Assert.AreEqual("SCORED", scored.Live);
        }

        [Test]
        public void Anytime_scorer_surname_handles_a_single_word_name()
        {
            // The surname rule still has to handle a one-word name — it now lands in NEED, which is
            // where G1 moved the player's name. The progress line no longer carries it at all.
            ActiveLegCopy copy = Describe(ActiveLegInput.AnytimeScorer("Neymar", scorerRevealed: false));
            Assert.AreEqual("NEYMAR TO SCORE", copy.Need);
            Assert.AreEqual("NOT YET", copy.Live);
        }

        // ------------------------------------------------------------------------- 5. Team identity vs MARKET PICK

        [Test]
        public void Moneyline_is_the_only_team_market_and_carries_the_backed_team_identity()
        {
            ActiveLegCopy copy = Describe(ActiveLegInput.Moneyline("Sunderland", 0, 0));
            Assert.IsTrue(copy.IsTeamMarket);
            Assert.AreEqual("SUNDERLAND", copy.Identity);
        }

        [Test]
        public void Non_team_markets_never_fabricate_a_backed_team()
        {
            var inputs = new[]
            {
                ActiveLegInput.TotalGoals(true, 2.5, 0, 0),
                ActiveLegInput.TotalGoals(false, 2.5, 0, 0),
                ActiveLegInput.BothTeamsToScore(true, 0, 0),
                ActiveLegInput.BothTeamsToScore(false, 0, 0),
                ActiveLegInput.TotalCorners(true, 9.5, 0, 0),
                ActiveLegInput.TotalCorners(false, 9.5, 0, 0),
                ActiveLegInput.TotalCards(true, 4.5, 0, 0),
                ActiveLegInput.TotalCards(false, 4.5, 0, 0),
                ActiveLegInput.AnytimeScorer("Harry Kane", false),
            };

            foreach (ActiveLegInput input in inputs)
            {
                ActiveLegCopy copy = Describe(input);
                Assert.IsFalse(copy.IsTeamMarket, $"{input.Kind}/{input.Choice} must not present as a team market");
                Assert.AreEqual("MARKET PICK", copy.Identity, $"{input.Kind}/{input.Choice} must use MARKET PICK, never a fabricated team");
            }
        }

        // ------------------------------------------------------------------------- 6. No-leak pin

        [Test]
        public void Moneyline_live_progress_reflects_only_the_revealed_score_never_the_locked_endpoint()
        {
            // Locked endpoint: picked side wins 5-1. Nothing revealed onto the ledger yet
            // except a single completed goal for the picked side.
            var statLine = new MatchStatLine(homeGoals: 5, awayGoals: 1,
                homeCorners: 6, awayCorners: 4, homeCards: 2, awayCards: 1);
            var ledger = new ScoreLedger();
            ledger.ConfigureEndpoint(statLine, pickedHome: true);

            ScoreLedger.StagedGoal goal = ledger.StageBeatGoal(DramaEventType.Score, up: true, 0.1, 0.6).Value;
            ledger.CompleteGoal(goal); // exactly ONE goal has actually been revealed

            Assert.AreEqual(1, ledger.Picked, "sanity: only one goal is revealed so far");
            Assert.AreEqual(5, ledger.TargetPicked, "sanity: the locked endpoint really is 5, not 1");

            // The model receives ONLY the revealed counters — its signature has no way to
            // reach ledger.TargetPicked/TargetOpponent at all.
            ActiveLegCopy copy = Describe(ActiveLegInput.Moneyline("Home Side", ledger.Picked, ledger.Opponent));

            Assert.AreEqual("LEADING 1–0", copy.Live);
            StringAssert.DoesNotContain("5", copy.Live, "the locked endpoint (5 goals) must never appear in revealed-only copy");
        }

        [Test]
        public void Corners_live_progress_reflects_only_the_revealed_count_never_the_locked_endpoint()
        {
            // Locked endpoint: 9 home corners / 3 away corners = 12 total. Plan across many
            // beats so only a small slice is revealed by the time we read it.
            var statLine = new MatchStatLine(homeGoals: 2, awayGoals: 0,
                homeCorners: 9, awayCorners: 3, homeCards: 1, awayCards: 1);
            var ledger = new CountLedger();
            ledger.ConfigureEndpoint(statLine, MarketKind.TotalCorners, beatCount: 12);

            // Complete exactly one staged beat.
            CountLedger.StagedCount staged = ledger.StageBeat();
            ledger.CompleteCount(staged);

            Assert.AreEqual(12, ledger.TargetTotal, "sanity: the locked endpoint really is 12 total corners");
            Assert.Less(ledger.Total, ledger.TargetTotal, "sanity: far fewer than the endpoint has been revealed");

            ActiveLegCopy copy = Describe(ActiveLegInput.TotalCorners(true, 9.5, ledger.Home, ledger.Away));

            string expectedRevealedTotal = ledger.Total.ToString();
            StringAssert.StartsWith($"{expectedRevealedTotal} CORNERS", copy.Live);
            StringAssert.DoesNotContain("12 CORNERS", copy.Live, "the locked 12-corner endpoint must never leak into revealed-only copy");
        }

        // ------------------------------------------------------------------------- concurrency (PRD §8.2A)

        [Test]
        public void DescribeAll_formats_every_concurrent_live_leg_independently_in_order()
        {
            var legs = new List<ActiveLegInput>
            {
                ActiveLegInput.Moneyline("Arsenal", 1, 0),
                ActiveLegInput.TotalCorners(true, 9.5, 3, 2),
                ActiveLegInput.AnytimeScorer("Harry Kane", false),
            };

            IReadOnlyList<ActiveLegCopy> copies = DescribeAll(legs);

            Assert.AreEqual(3, copies.Count);
            Assert.AreEqual("ARSENAL TO WIN", copies[0].Need);
            Assert.AreEqual("LEADING 1–0", copies[0].Live);
            Assert.AreEqual("OVER 9.5 CORNERS", copies[1].Need);
            Assert.AreEqual("5 CORNERS • NEED 5", copies[1].Live);
            Assert.AreEqual("KANE TO SCORE", copies[2].Need);   // G1: surname, and named only here
            Assert.AreEqual("NOT YET", copies[2].Live);         // G1: the pair names the player once
        }

        [Test]
        public void DescribeAll_tolerates_zero_and_one_live_legs()
        {
            Assert.AreEqual(0, DescribeAll(new List<ActiveLegInput>()).Count);
            Assert.AreEqual(0, DescribeAll(null).Count);

            IReadOnlyList<ActiveLegCopy> single = DescribeAll(new List<ActiveLegInput>
            {
                ActiveLegInput.Moneyline("Arsenal", 0, 0),
            });
            Assert.AreEqual(1, single.Count);
            Assert.AreEqual("LEVEL 0–0", single[0].Live);
        }

        // ------------------------------------------------------------------------- 7. NEED 0 is unconstructible (C46 state-lie fix)

        [Test]
        public void Need_0_is_unconstructible_across_the_full_half_line_and_total_sweep()
        {
            // The defect: `Math.Max(0, threshold - total)` clamped a cleared requirement to zero
            // and kept printing it forever after — "10 CORNERS • NEED 0" stayed on screen for
            // eighteen minutes of match time in the capture that found it. This sweeps every
            // corners/cards/total-goals leg, both directions, across every half-line this run's
            // config can generate (0.5 through 12.5) and every revealed total from 0 to 20, and
            // asserts NEED 0 / 0 MORE / any stray negative number can never be produced.
            int cases = 0;
            for (double halfLine = 0.5; halfLine <= 12.5 + 1e-9; halfLine += 1.0)
            {
                for (int total = 0; total <= 20; total++)
                {
                    var legs = new[]
                    {
                        ActiveLegInput.TotalCorners(true, halfLine, total, 0),
                        ActiveLegInput.TotalCorners(false, halfLine, total, 0),
                        ActiveLegInput.TotalCards(true, halfLine, total, 0),
                        ActiveLegInput.TotalCards(false, halfLine, total, 0),
                        ActiveLegInput.TotalGoals(true, halfLine, total, 0),
                        ActiveLegInput.TotalGoals(false, halfLine, total, 0),
                    };
                    foreach (ActiveLegInput leg in legs)
                    {
                        cases++;
                        string live = Describe(leg).Live;
                        StringAssert.DoesNotContain("NEED 0", live,
                            $"case {cases} ({leg.Kind}/{leg.Choice}, line {halfLine}, total {total}): " +
                            "NEED 0 must be unconstructible, got '" + live + "'");
                        // Anchored to the bullet: a bare "0 MORE" substring check false-positives on
                        // legitimate output — "3 GOALS • 10 MORE" contains "0 MORE" as the tail of
                        // "10 MORE", and remaining=10 is reached inside this exact sweep (e.g. line
                        // 9.5, total 0). The number is always immediately preceded by "{Bullet} ", so
                        // anchoring the check there makes "0" a token boundary instead of a digit tail.
                        StringAssert.DoesNotContain("• 0 MORE", live,
                            $"case {cases} ({leg.Kind}/{leg.Choice}, line {halfLine}, total {total}): " +
                            "0 MORE must be unconstructible, got '" + live + "'");
                        Assert.IsFalse(ContainsNegativeNumber(live),
                            $"case {cases} ({leg.Kind}/{leg.Choice}, line {halfLine}, total {total}): " +
                            $"'{live}' must never contain a negative number");
                    }
                }
            }
            Assert.AreEqual(13 * 21 * 6, cases, $"NEED-0 sweep executed {cases} cases (13 half-lines x 21 totals x 6 market/direction pairs)");
        }

        // ------------------------------------------------------------------------- 8. the form is selected by the outcome, not a clamp

        [Test]
        public void The_form_is_selected_by_the_outcome_not_by_a_clamp()
        {
            // Consequence, not mechanism: this does not inspect DescribeCount's/DescribeTotalGoals*'s
            // internals — it only checks that whatever outcome came out, the STRING shape that goes
            // with it is the one the outcome rule promises. If a later change ever reintroduces a
            // clamp that leaves the wrong form selected, this is the assertion that catches it.
            int cases = 0;
            for (double halfLine = 0.5; halfLine <= 12.5 + 1e-9; halfLine += 1.0)
            {
                for (int total = 0; total <= 20; total++)
                {
                    var overLegs = new[]
                    {
                        ActiveLegInput.TotalCorners(true, halfLine, total, 0),
                        ActiveLegInput.TotalCards(true, halfLine, total, 0),
                        ActiveLegInput.TotalGoals(true, halfLine, total, 0),
                    };
                    var underLegs = new[]
                    {
                        ActiveLegInput.TotalCorners(false, halfLine, total, 0),
                        ActiveLegInput.TotalCards(false, halfLine, total, 0),
                        ActiveLegInput.TotalGoals(false, halfLine, total, 0),
                    };

                    foreach (ActiveLegInput leg in overLegs)
                    {
                        cases++;
                        ActiveLegCopy copy = Describe(leg);
                        if (copy.Outcome == RevealedLegOutcome.Won)
                        {
                            Assert.IsTrue(copy.Live.EndsWith("WON"),
                                $"case {cases}: Won must end WON, got '{copy.Live}'");
                        }
                        else
                        {
                            Assert.AreEqual(RevealedLegOutcome.Undecided, copy.Outcome,
                                $"case {cases}: the over/NEED-MORE family is only ever Won or Undecided");
                            bool hasExpectedToken = copy.Live.Contains("NEED ") || copy.Live.Contains(" MORE");
                            Assert.IsTrue(hasExpectedToken,
                                $"case {cases}: Undecided must contain 'NEED ' or ' MORE', got '{copy.Live}'");

                            // Asserted as the CONSEQUENCE of the outcome selection, not the mechanism:
                            // there is no clamp any more, so this is not "the clamp never produces
                            // k < 1" — it is "the selection never emits NEED alongside k < 1".
                            if (copy.Live.Contains("NEED "))
                            {
                                int needValue = ExtractTrailingInt(copy.Live, "NEED ");
                                Assert.GreaterOrEqual(needValue, 1,
                                    $"case {cases}: NEED must never appear with k < 1, got '{copy.Live}'");
                            }
                        }
                    }

                    foreach (ActiveLegInput leg in underLegs)
                    {
                        cases++;
                        ActiveLegCopy copy = Describe(leg);
                        if (copy.Outcome == RevealedLegOutcome.Lost)
                        {
                            Assert.IsTrue(copy.Live.EndsWith("LOST"),
                                $"case {cases}: Lost must end LOST, got '{copy.Live}'");
                        }
                        else
                        {
                            Assert.AreEqual(RevealedLegOutcome.Undecided, copy.Outcome,
                                $"case {cases}: the under/LIMIT family is only ever Lost or Undecided");
                            Assert.IsTrue(copy.Live.Contains("LIMIT "),
                                $"case {cases}: Undecided must contain 'LIMIT ', got '{copy.Live}'");
                        }
                    }
                }
            }
            Assert.AreEqual(13 * 21 * 6, cases, $"outcome-selection sweep executed {cases} cases (13 half-lines x 21 totals x 6 market/direction pairs)");
        }

        // ------------------------------------------------------------------------- 9. LIMIT 0 survives (not the same defect as NEED 0)

        [Test]
        public void Limit_0_survives_an_under_leg_at_its_maximum_is_still_undecided()
        {
            // This exists so a later seat does not "fix" LIMIT 0 into looking like NEED 0. They
            // are not the same defect: NEED 0 named a requirement that had already stopped
            // existing; LIMIT 0 names an allowance that is still real and still live — one more of
            // the stat kills the leg, but none has happened yet.
            ActiveLegCopy corners = Describe(ActiveLegInput.TotalCorners(false, 9.5, 5, 4)); // total 9 = maxAllowed
            Assert.AreEqual("9 CORNERS • LIMIT 0", corners.Live);
            Assert.AreEqual(RevealedLegOutcome.Undecided, corners.Outcome);

            ActiveLegCopy cards = Describe(ActiveLegInput.TotalCards(false, 4.5, 2, 2)); // total 4 = maxAllowed
            Assert.AreEqual("4 CARDS • LIMIT 0", cards.Live);
            Assert.AreEqual(RevealedLegOutcome.Undecided, cards.Outcome);

            ActiveLegCopy goals = Describe(ActiveLegInput.TotalGoals(false, 2.5, 1, 1)); // total 2 = maxAllowed
            Assert.AreEqual("2 GOALS • LIMIT 0", goals.Live);
            Assert.AreEqual(RevealedLegOutcome.Undecided, goals.Outcome);
        }

        // ------------------------------------------------------------------------- 10. the ticket-word trap, exhaustively

        [Test]
        public void TicketCannotLose_is_true_iff_every_leg_is_won_or_voided()
        {
            var allOutcomes = new[]
            {
                RevealedLegOutcome.Undecided, RevealedLegOutcome.Won,
                RevealedLegOutcome.Lost, RevealedLegOutcome.Voided,
            };

            int cases = 0;
            for (int length = 1; length <= 3; length++)
            {
                foreach (List<RevealedLegOutcome> combo in AllCombinations(allOutcomes, length))
                {
                    cases++;
                    bool expected = combo.TrueForAll(o => o == RevealedLegOutcome.Won || o == RevealedLegOutcome.Voided);
                    bool actual = TicketCannotLose(combo);
                    Assert.AreEqual(expected, actual,
                        $"case {cases} [{string.Join(", ", combo)}]: TicketCannotLose must be true iff every leg is Won or Voided");
                }
            }
            Assert.AreEqual(4 + 16 + 64, cases,
                $"exhaustive ticket-word sweep executed {cases} cases (4^1 + 4^2 + 4^3 over lengths 1..3)");

            // THE TRAP CASE, named on its own: one leg won and one still live does not change the
            // ticket's word. RISK is a TICKET word — every leg has to clear it.
            Assert.IsFalse(
                TicketCannotLose(new List<RevealedLegOutcome> { RevealedLegOutcome.Won, RevealedLegOutcome.Undecided }),
                "[Won, Undecided]: one leg won and one still live must NOT flip the ticket word to STAKE");

            Assert.IsFalse(TicketCannotLose(null), "null must be false, never a NullReferenceException masquerading as true");
            Assert.IsFalse(TicketCannotLose(new List<RevealedLegOutcome>()),
                "empty must be false — a ticket with no legs cannot be said to have none that can lose it");
        }

        // ------------------------------------------------------------------------- 11. StakeWord: STAKE iff TicketCannotLose, else RISK

        [Test]
        public void StakeWord_is_stake_exactly_when_the_ticket_cannot_lose()
        {
            Assert.AreEqual("STAKE", StakeWord(new List<RevealedLegOutcome> { RevealedLegOutcome.Won }));
            Assert.AreEqual("STAKE", StakeWord(new List<RevealedLegOutcome> { RevealedLegOutcome.Won, RevealedLegOutcome.Voided }));
            Assert.AreEqual("RISK", StakeWord(new List<RevealedLegOutcome> { RevealedLegOutcome.Undecided }));
            Assert.AreEqual("RISK", StakeWord(new List<RevealedLegOutcome> { RevealedLegOutcome.Won, RevealedLegOutcome.Undecided }));

            // [Lost] => RISK is the DELIBERATE unbuilt dead-ticket state, not an oversight: the
            // spec rules only the principle (no word may name a jeopardy or payout that no longer
            // exists) and leaves the actual dead-ticket strings to a frame, because the capture
            // contains no losing ticket. Today's RISK is what a ticket with a Lost leg keeps until
            // that frame lands. Do not "fix" this pin without new evidence.
            Assert.AreEqual("RISK", StakeWord(new List<RevealedLegOutcome> { RevealedLegOutcome.Lost }));

            Assert.AreEqual("RISK", StakeWord(null));
            Assert.AreEqual("RISK", StakeWord(new List<RevealedLegOutcome>()));
        }

        // ------------------------------------------------------------------------- 12. count significance classifier (spec-count-theater-2026-08-17.md §3)

        [Test]
        public void Classify_matches_the_ruled_distance_precedence_across_a_full_sweep()
        {
            // spec-count-theater-2026-08-17.md §3: independently RE-DERIVES the expected case
            // from the SPEC TEXT's own precedence (Decided > Turn > Approach > Ordinary) —
            // never by calling a production helper to compute "expected", which would only
            // prove Classify agrees with itself. HalfLineThreshold is `internal`
            // (TheaterChoreographer needs it; this EditMode assembly has no InternalsVisibleTo
            // grant — see Runtime/AssemblyInfo.cs), so `threshold` below inlines that same
            // trivial formula rather than calling it — this is not a re-test of that method.
            //
            // Same half-line sweep as the NEED-0 sweep above (0.5 through 12.5 — every line
            // this run's config can generate) and the same 0..20 total range. stagedDelta
            // sweeps 0..3: production never calls Classify with a zero delta (the caller gates
            // TotalDelta > 0 first), but the classifier's own signature does not forbid it, so
            // it is swept anyway for completeness; real batches are small per-beat increments
            // (PlanForBeats spreads the target across many beats), so 1-3 covers the realistic
            // range without pretending to sweep an unbounded delta.
            int cases = 0;
            var seen = new Dictionary<CountSignificance, int>
            {
                [CountSignificance.Ordinary] = 0, [CountSignificance.Approach] = 0,
                [CountSignificance.Turn] = 0, [CountSignificance.Decided] = 0,
            };
            for (double halfLine = 0.5; halfLine <= 12.5 + 1e-9; halfLine += 1.0)
            {
                int threshold = (int)System.Math.Floor(halfLine) + 1;
                for (int total = 0; total <= 20; total++)
                {
                    for (int delta = 0; delta <= 3; delta++)
                    {
                        cases++;
                        int distanceBefore = threshold - total;
                        int distanceAfter = threshold - (total + delta);
                        CountSignificance expected =
                            distanceBefore <= 0 ? CountSignificance.Decided
                            : distanceAfter <= 0 ? CountSignificance.Turn
                            : distanceAfter == ApproachDistance ? CountSignificance.Approach
                            : CountSignificance.Ordinary;

                        CountSignificance actual = Classify(total, delta, halfLine);
                        Assert.AreEqual(expected, actual,
                            $"case {cases}: line={halfLine}, total={total}, delta={delta} - " +
                            $"distanceBefore={distanceBefore}, distanceAfter={distanceAfter}");

                        // Decided never yields Approach/Turn: the per-case comparison above
                        // already proves this (the ruled precedence says Decided, so actual
                        // must equal Decided too), named here explicitly rather than left
                        // implicit in the loop.
                        if (distanceBefore <= 0)
                            Assert.AreEqual(CountSignificance.Decided, actual,
                                $"case {cases}: distanceBefore<=0 must always classify Decided, " +
                                $"never Approach/Turn, got {actual}");

                        seen[actual]++;
                    }
                }
            }
            Assert.AreEqual(13 * 21 * 4, cases,
                $"count-significance sweep executed {cases} cases (13 half-lines x 21 totals x 4 deltas)");
            // C29: the case count alone is not enough if one whole classification were
            // unreachable across the entire sweep — assert every one of the four actually fired.
            Assert.Greater(seen[CountSignificance.Ordinary], 0, "Ordinary never produced across the sweep");
            Assert.Greater(seen[CountSignificance.Approach], 0, "Approach never produced across the sweep");
            Assert.Greater(seen[CountSignificance.Turn], 0, "Turn never produced across the sweep");
            Assert.Greater(seen[CountSignificance.Decided], 0, "Decided never produced across the sweep");
        }

        [Test]
        public void Decided_wins_precedence_even_when_the_after_distance_would_also_match_turn()
        {
            // spec-count-theater-2026-08-17.md §3: "Decided ... distanceBefore <= 0. The leg
            // was already won before this beat" — and it wins PRECEDENCE, not merely the case
            // where no other rule could also match. Named separately from the sweep above
            // because the property being pinned is the ORDER of the checks, not just their
            // union (the sweep already covers this arithmetically; this pins it by name).
            double line = 8.5; // threshold = 9

            // distanceBefore = 9 - 10 = -1 (Decided on its own); distanceAfter = 9 - (10+0) =
            // -1 too (would read Turn taken alone) — Decided must still win.
            Assert.AreEqual(CountSignificance.Decided, Classify(revealedTotal: 10, stagedDelta: 0, line: line),
                "already past the line, zero further batch: distanceAfter also reads <= 0 " +
                "(Turn's own test) but Decided must win");

            // A REALISTIC post-win corner: total already AT the line (distanceBefore == 0,
            // Decided), one more corner arrives (distanceAfter = -1, Turn's own test again) —
            // still Decided. This is §3.4's "corpse stretch": corners keep arriving after the
            // leg is already won and must keep being counted (QuietCount), never re-dramatized
            // as a crossing.
            Assert.AreEqual(CountSignificance.Decided, Classify(revealedTotal: 9, stagedDelta: 1, line: line),
                "at the line already (distanceBefore == 0) plus a further batch: still Decided, not Turn");

            // Defensive-only (StagedCount deltas are never negative in production —
            // CountLedger.Distribute never emits one): a negative delta could in principle
            // land distanceAfter exactly on ApproachDistance while distanceBefore <= 0. Even
            // then, Decided must still win — the ORDER of the checks is what this test pins,
            // not merely their reachable union under a realistic non-negative delta.
            Assert.AreEqual(CountSignificance.Decided, Classify(revealedTotal: 9, stagedDelta: -1, line: line),
                "distanceBefore <= 0 with distanceAfter landing exactly on ApproachDistance: " +
                "Decided must still win over Approach too");
        }

        // ------------------------------------------------------------------------- 13. the decisive-beat pool is disjoint from the ordinary pool (spec-count-theater-2026-08-17.md §3.5, gate item 4)

        [Test]
        public void Decisive_beat_pool_is_disjoint_from_the_ordinary_count_event_pool()
        {
            // spec-count-theater-2026-08-17.md §3.5 / strings-owed-2026-08-17.md §4: "the pool
            // is disjoint, so recycling onto a decisive beat is unconstructible rather than
            // unlikely" (T108 clause 1's standard). This is a property of the two POOLS, not a
            // probe for a collision: every reachable member of each pool is enumerated over its
            // full domain through SweatFlavor's own PUBLIC surface (never a re-typed copy of its
            // private arrays, which could silently drift from the authored decks) and the actual
            // string SETS are compared directly.
            //
            // The ORDINARY pool: CornerFor/CornerAgainst/BookingFor/BookingAgainst, each a
            // 3-variant deck selected by `step % 3` (SweatFlavor.CornerLine/BookingLine). `leg`
            // is accepted but never dereferenced by either method (SweatFlavor.cs), so null is a
            // faithful call, not a workaround. forPicked x step x {corner, booking} exhausts all
            // 12 slots — the complete deck, not a sample of it.
            var ordinary = new HashSet<string>();
            int ordinaryCases = 0;
            foreach (bool forPicked in new[] { true, false })
            {
                for (int step = 0; step < 3; step++)
                {
                    ordinary.Add(SweatFlavor.CornerLine(forPicked, null, step));
                    ordinaryCases++;
                    ordinary.Add(SweatFlavor.BookingLine(forPicked, null, step));
                    ordinaryCases++;
                }
            }

            // The DECISIVE pool: the four §3.5/§4.2 cells (Approach/Turn x Over/Under) plus each
            // cell's fallback rung, via SweatFlavor.DecisiveLine/DecisiveShortLine — the selector
            // this unit adds. Both rungs are swept: a fallback that collided with the ordinary
            // pool would be exactly as unconstructible a defect as the primary line colliding,
            // and §4.4 puts both rungs into the same C46 sweep for the same reason.
            var decisive = new HashSet<string>();
            int decisiveCases = 0;
            foreach (CountSignificance significance in new[] { CountSignificance.Approach, CountSignificance.Turn })
            {
                foreach (bool over in new[] { true, false })
                {
                    decisive.Add(SweatFlavor.DecisiveLine(significance, over));
                    decisiveCases++;
                    decisive.Add(SweatFlavor.DecisiveShortLine(significance, over));
                    decisiveCases++;
                }
            }

            // C29: the executed case count is reported and a zero-case sweep fails outright,
            // rather than an empty pool silently reading as "no collision found".
            Assert.AreEqual(12, ordinaryCases,
                $"ordinary-pool sweep executed {ordinaryCases} cases (2 forPicked x 3 steps x " +
                "{corner, booking}), expected 12");
            Assert.AreEqual(8, decisiveCases,
                $"decisive-pool sweep executed {decisiveCases} cases (2 significance x 2 " +
                "valence x {full, short}), expected 8");
            Assert.Greater(ordinary.Count, 0, "the ordinary pool must not be empty");
            Assert.Greater(decisive.Count, 0, "the decisive pool must not be empty");

            // THE ASSERTION: not "no collision was observed this run" but the sets themselves
            // have empty intersection — disjoint, not merely distinct.
            var intersection = new HashSet<string>(decisive);
            intersection.IntersectWith(ordinary);
            Assert.AreEqual(0, intersection.Count,
                "the decisive pool must be DISJOINT from the ordinary pool (spec §3.5) so a " +
                "decisive beat can never recycle an ordinary line and vice versa — shared " +
                $"line(s) found: {string.Join(", ", intersection)}");
        }

        // ------------------------------------------------------------------------- sweep helpers

        /// <summary>True iff <paramref name="s"/> contains a '-' immediately followed by a digit —
        /// the shape a stray negative number would take from unclamped subtraction.</summary>
        private static bool ContainsNegativeNumber(string s)
        {
            for (int i = 0; i < s.Length - 1; i++)
                if (s[i] == '-' && char.IsDigit(s[i + 1])) return true;
            return false;
        }

        /// <summary>Parses the integer immediately following the last occurrence of
        /// <paramref name="marker"/> in <paramref name="s"/>. Used to pull the "k" out of "NEED k",
        /// which is always the trailing token in this file's corners/cards copy.</summary>
        private static int ExtractTrailingInt(string s, string marker)
        {
            int idx = s.IndexOf(marker, System.StringComparison.Ordinal);
            Assert.GreaterOrEqual(idx, 0, $"expected to find '{marker}' in '{s}'");
            string tail = s.Substring(idx + marker.Length);
            return int.Parse(tail);
        }

        /// <summary>Every ordered tuple of length <paramref name="length"/> drawn from
        /// <paramref name="values"/> (the full Cartesian product, values.Length^length tuples) —
        /// the exhaustive enumeration item 10 requires.</summary>
        private static IEnumerable<List<RevealedLegOutcome>> AllCombinations(RevealedLegOutcome[] values, int length)
        {
            int total = 1;
            for (int i = 0; i < length; i++) total *= values.Length;
            for (int index = 0; index < total; index++)
            {
                var combo = new List<RevealedLegOutcome>(length);
                int remainder = index;
                for (int slot = 0; slot < length; slot++)
                {
                    combo.Add(values[remainder % values.Length]);
                    remainder /= values.Length;
                }
                yield return combo;
            }
        }
    }
}
