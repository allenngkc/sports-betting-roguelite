using System.Collections.Generic;
using NUnit.Framework;
using SBR.Game;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// Phase 2A (PRD §4.3, §9): pure, EditMode-only coverage for <see cref="PresentationSceneKey"/>.
    /// No scene playback, no engine RNG, no UnityEngine.Random, no wall clock — every case below
    /// constructs the key from plain literals and asserts on its pure query surface.
    /// </summary>
    public class PresentationSceneKeyTests
    {
        private static PresentationSceneKey Key(
            string runSeed = "SEED-1",
            int round = 3,
            int ticketIndex = 1,
            int matchIndex = 2,
            int eventStep = 7,
            SceneTemplate template = SceneTemplate.GoalFor,
            bool beneficiary = true)
            => new PresentationSceneKey(runSeed, round, ticketIndex, matchIndex, eventStep, template, beneficiary);

        [Test]
        public void Same_key_material_yields_identical_channel_values_across_repeated_construction()
        {
            PresentationSceneKey a = Key();
            PresentationSceneKey b = Key(); // fresh construction, same material

            foreach (string channel in new[]
            {
                PresentationSceneKey.Channels.Grammar,
                PresentationSceneKey.Channels.Lane,
                PresentationSceneKey.Channels.Pressure,
                PresentationSceneKey.Channels.Payoff,
                PresentationSceneKey.Channels.Reaction,
            })
            {
                Assert.AreEqual(a.Channel(channel), b.Channel(channel), $"channel '{channel}' diverged across construction");
                Assert.AreEqual(a.Normalized(channel), b.Normalized(channel), $"normalized '{channel}' diverged across construction");
                Assert.AreEqual(a.Pick(channel, 11), b.Pick(channel, 11), $"pick '{channel}' diverged across construction");
            }
        }

        [Test]
        public void Different_event_steps_yield_different_channel_values()
        {
            ulong prev = Key(eventStep: 0).Channel(PresentationSceneKey.Channels.Grammar);
            var seen = new HashSet<ulong> { prev };

            for (int step = 1; step < 50; step++)
            {
                ulong h = Key(eventStep: step).Channel(PresentationSceneKey.Channels.Grammar);
                Assert.IsFalse(seen.Contains(h), $"event step {step} collided with an earlier step's grammar channel");
                seen.Add(h);
            }
        }

        [Test]
        public void Adding_a_novel_channel_does_not_perturb_the_five_named_channels()
        {
            PresentationSceneKey key = Key();

            ulong grammarBefore = key.Channel(PresentationSceneKey.Channels.Grammar);
            ulong laneBefore = key.Channel(PresentationSceneKey.Channels.Lane);
            ulong pressureBefore = key.Channel(PresentationSceneKey.Channels.Pressure);
            ulong payoffBefore = key.Channel(PresentationSceneKey.Channels.Payoff);
            ulong reactionBefore = key.Channel(PresentationSceneKey.Channels.Reaction);

            // A sixth channel nobody has heard of yet.
            key.Channel("formation-density");
            key.Pick("crowd-noise-flavor", 4);
            key.Normalized("future-channel-nobody-invented-yet");

            Assert.AreEqual(grammarBefore, key.Channel(PresentationSceneKey.Channels.Grammar));
            Assert.AreEqual(laneBefore, key.Channel(PresentationSceneKey.Channels.Lane));
            Assert.AreEqual(pressureBefore, key.Channel(PresentationSceneKey.Channels.Pressure));
            Assert.AreEqual(payoffBefore, key.Channel(PresentationSceneKey.Channels.Payoff));
            Assert.AreEqual(reactionBefore, key.Channel(PresentationSceneKey.Channels.Reaction));
        }

        [Test]
        public void Distinct_channel_names_are_independent_even_when_material_is_identical()
        {
            // A stream-based design (seed one RNG from the key, draw 5 times for the 5 channels)
            // would make each channel's value depend on the ones drawn before it. Here, deriving
            // channels in a different order — or deriving extra ones in between — must not change
            // any channel's value versus deriving it alone.
            PresentationSceneKey key = Key();

            ulong grammarAlone = key.Channel(PresentationSceneKey.Channels.Grammar);
            ulong payoffAlone = key.Channel(PresentationSceneKey.Channels.Payoff);

            // Interleave a pile of unrelated channel queries between them.
            for (int i = 0; i < 20; i++) key.Channel("noise-" + i);

            Assert.AreEqual(grammarAlone, key.Channel(PresentationSceneKey.Channels.Grammar));
            Assert.AreEqual(payoffAlone, key.Channel(PresentationSceneKey.Channels.Payoff));
        }

        [Test]
        public void Pick_is_stable_and_distributes_across_the_candidate_range()
        {
            const int candidateCount = 5;
            var counts = new int[candidateCount];

            for (int step = 0; step < 400; step++)
            {
                int pick = Key(eventStep: step).Pick(PresentationSceneKey.Channels.Lane, candidateCount);
                Assert.GreaterOrEqual(pick, 0);
                Assert.Less(pick, candidateCount);
                counts[pick]++;

                // Stability: asking twice for the same key gives the same pick.
                int pickAgain = Key(eventStep: step).Pick(PresentationSceneKey.Channels.Lane, candidateCount);
                Assert.AreEqual(pick, pickAgain);
            }

            for (int i = 0; i < candidateCount; i++)
                Assert.Greater(counts[i], 0, $"candidate index {i} was never chosen across 400 samples — selection favors other indices");

            // No single index should dominate a roughly-uniform 400-sample draw over 5 buckets
            // (expected ~80 each); a broken hash tends to collapse everything onto one bucket.
            foreach (int count in counts)
                Assert.Less(count, 250, "one candidate index dominates far past what a well-mixed hash should produce");
        }

        [Test]
        public void Query_order_does_not_affect_results()
        {
            PresentationSceneKey forward = Key();
            PresentationSceneKey backward = Key();

            ulong grammarFirst = forward.Channel(PresentationSceneKey.Channels.Grammar);
            ulong laneFirst = forward.Channel(PresentationSceneKey.Channels.Lane);
            ulong pressureFirst = forward.Channel(PresentationSceneKey.Channels.Pressure);
            ulong payoffFirst = forward.Channel(PresentationSceneKey.Channels.Payoff);
            ulong reactionFirst = forward.Channel(PresentationSceneKey.Channels.Reaction);

            ulong reactionSecond = backward.Channel(PresentationSceneKey.Channels.Reaction);
            ulong payoffSecond = backward.Channel(PresentationSceneKey.Channels.Payoff);
            ulong pressureSecond = backward.Channel(PresentationSceneKey.Channels.Pressure);
            ulong laneSecond = backward.Channel(PresentationSceneKey.Channels.Lane);
            ulong grammarSecond = backward.Channel(PresentationSceneKey.Channels.Grammar);

            Assert.AreEqual(grammarFirst, grammarSecond);
            Assert.AreEqual(laneFirst, laneSecond);
            Assert.AreEqual(pressureFirst, pressureSecond);
            Assert.AreEqual(payoffFirst, payoffSecond);
            Assert.AreEqual(reactionFirst, reactionSecond);
        }

        // ---- The leg-index-under-concurrency design decision, pinned. ----
        //
        // PresentationSceneKey deliberately has NO leg-index constructor parameter. PRD §8.2A
        // establishes that two or more legs can be live on the same match at once; a scene beat
        // belongs to the match (the shared stage), not to whichever leg happens to be asking.
        // MatchIndex — not leg index — is the key material that identifies "which match." These
        // tests pin that decision directly: two legs riding the same match must be UNABLE to
        // produce different scene keys for the same beat, and two different matches (even inside
        // the same ticket) must be able to.

        [Test]
        public void Two_concurrently_live_legs_on_the_same_match_yield_the_identical_key()
        {
            // Nothing distinguishes "leg 0's view of this beat" from "leg 2's view of this beat"
            // in the constructor below — there is no leg index to supply. Both legs, asking about
            // the same match/round/ticket/step/template/beneficiary, necessarily build the exact
            // same key. This IS the fix: it is structurally impossible for two legs on one match
            // to disagree on the scene key, because the type gives them no field to disagree on.
            var fromLegZerosPerspective = new PresentationSceneKey(
                "SEED-CONCURRENT", round: 4, ticketIndex: 2, matchIndex: 9,
                eventStep: 12, SceneTemplate.CornerFor, beneficiary: true);

            var fromLegTwosPerspective = new PresentationSceneKey(
                "SEED-CONCURRENT", round: 4, ticketIndex: 2, matchIndex: 9,
                eventStep: 12, SceneTemplate.CornerFor, beneficiary: true);

            Assert.AreEqual(fromLegZerosPerspective, fromLegTwosPerspective);
            foreach (string channel in new[]
            {
                PresentationSceneKey.Channels.Grammar, PresentationSceneKey.Channels.Lane,
                PresentationSceneKey.Channels.Pressure, PresentationSceneKey.Channels.Payoff,
                PresentationSceneKey.Channels.Reaction,
            })
                Assert.AreEqual(fromLegZerosPerspective.Channel(channel), fromLegTwosPerspective.Channel(channel));
        }

        [Test]
        public void A_leg_settling_mid_match_does_not_change_the_key_for_the_leg_still_live()
        {
            // Model: leg A settles (its own presentation moves on), leg B is still live on the
            // same match. Because the key never depended on leg identity, a later beat on the
            // same match/step/template still reproduces byte-identical channel values regardless
            // of what happened to the other leg in the meantime — there is nothing in the key for
            // "leg A already settled" to perturb.
            var beforeLegASettles = new PresentationSceneKey(
                "SEED-SETTLE", round: 6, ticketIndex: 0, matchIndex: 3,
                eventStep: 20, SceneTemplate.TerritoryFor, beneficiary: false);

            // ... leg A settles here in the real system; nothing about the match identity changes ...

            var afterLegASettles = new PresentationSceneKey(
                "SEED-SETTLE", round: 6, ticketIndex: 0, matchIndex: 3,
                eventStep: 20, SceneTemplate.TerritoryFor, beneficiary: false);

            Assert.AreEqual(beforeLegASettles, afterLegASettles);
            Assert.AreEqual(
                beforeLegASettles.Channel(PresentationSceneKey.Channels.Grammar),
                afterLegASettles.Channel(PresentationSceneKey.Channels.Grammar));
        }

        [Test]
        public void Two_different_matches_in_the_same_ticket_still_diverge()
        {
            // Ticket index alone is not enough to tell two matches in one multi-match parlay
            // apart — MatchIndex is what disambiguates them. Same ticket, same step/template/
            // beneficiary, different match: the key (and its channels) must differ.
            var matchNine = new PresentationSceneKey(
                "SEED-MULTI", round: 5, ticketIndex: 1, matchIndex: 9,
                eventStep: 3, SceneTemplate.GoalFor, beneficiary: true);

            var matchTwelve = new PresentationSceneKey(
                "SEED-MULTI", round: 5, ticketIndex: 1, matchIndex: 12,
                eventStep: 3, SceneTemplate.GoalFor, beneficiary: true);

            Assert.AreNotEqual(matchNine, matchTwelve);
            Assert.AreNotEqual(
                matchNine.Channel(PresentationSceneKey.Channels.Grammar),
                matchTwelve.Channel(PresentationSceneKey.Channels.Grammar));
        }
    }
}
