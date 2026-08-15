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
        [TestCase(2, 1, "3 GOALS • 0 MORE")]     // exactly at the line (threshold reached)
        [TestCase(3, 2, "5 GOALS • 0 MORE")]     // already cleared — never a negative "more"
        public void Total_goals_over_live_progress_at_line_2_5(int revealedFor, int revealedAgainst, string expected)
        {
            ActiveLegCopy copy = Describe(ActiveLegInput.TotalGoals(true, 2.5, revealedFor, revealedAgainst));
            Assert.AreEqual(expected, copy.Live);
        }

        [TestCase(0, 0, "0 GOALS • LIMIT 2")]    // zero revealed
        [TestCase(1, 0, "1 GOALS • LIMIT 1")]    // mid
        [TestCase(1, 1, "2 GOALS • LIMIT 0")]    // exactly at the line (no more room)
        [TestCase(2, 1, "3 GOALS • LIMIT 0")]    // already busted — never a negative limit
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
        [TestCase(5, 5, "10 CORNERS • NEED 0")]     // at the line
        [TestCase(7, 6, "13 CORNERS • NEED 0")]     // already cleared
        public void Total_corners_over_live_progress_at_line_9_5(int revealedHome, int revealedAway, string expected)
        {
            ActiveLegCopy copy = Describe(ActiveLegInput.TotalCorners(true, 9.5, revealedHome, revealedAway));
            Assert.AreEqual(expected, copy.Live);
        }

        [TestCase(0, 0, "0 CORNERS • LIMIT 9")]
        [TestCase(4, 4, "8 CORNERS • LIMIT 1")]
        [TestCase(5, 4, "9 CORNERS • LIMIT 0")]     // at the line
        [TestCase(6, 5, "11 CORNERS • LIMIT 0")]    // already busted
        public void Total_corners_under_live_progress_at_line_9_5(int revealedHome, int revealedAway, string expected)
        {
            ActiveLegCopy copy = Describe(ActiveLegInput.TotalCorners(false, 9.5, revealedHome, revealedAway));
            Assert.AreEqual(expected, copy.Live);
        }

        [TestCase(0, 0, "0 CARDS • NEED 5")]
        [TestCase(2, 2, "4 CARDS • NEED 1")]
        [TestCase(3, 2, "5 CARDS • NEED 0")]
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
    }
}
