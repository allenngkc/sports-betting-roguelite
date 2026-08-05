using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace SBR.Tests.PlayMode
{
    public class SureThingEntryTests
    {
        [UnityTest, Order(1)]
        public IEnumerator Entry_keeps_its_fixed_shell_while_each_named_destination_replaces_only_market_body()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            yield return OpenEntry(laptop);

            // Carry one staged ticket and one working mark through every destination. Persistence
            // is a user-visible state/content law, not a Unity object-identity law.
            Button stagedOffer = FirstNamedButton(Required(App(laptop), "MarketBody"), "Market");
            Invoke(stagedOffer.transform);
            yield return WaitForRebuild();
            Invoke(Required(Required(App(laptop), "WorkingMargin"), "Place"));
            yield return WaitForRebuild();
            Button workingOffer = FirstNamedButton(Required(App(laptop), "MarketBody"), "Market");
            Invoke(workingOffer.transform);
            yield return WaitForRebuild();

            Transform app = App(laptop);
            AssertEntryShell(app);
            EntryPersistenceSnapshot persistent = CaptureEntryPersistence(laptop);
            string previousBodyContent = AllText(Required(app, "MarketBody"));

            string[] destinationNames =
            {
                "DetailTabBTTS",
                "DetailTabCORNERS",
                "DetailTabCARDS",
                "DetailTabPLAYERS",
                "DetailTabGOALS",
            };
            // A2 ruling: the per-destination panel title ("BOTH TEAMS TO SCORE" etc.) is deleted —
            // each row now names its own market, so there is no longer a fixed title node to pin
            // per destination. The body-content diff below is what actually proves the destination
            // switched.
            for (int i = 0; i < destinationNames.Length; i++)
            {
                Transform destinations = Required(App(laptop), "MarketDestinations");
                Assert.IsNotNull(Required(destinations, destinationNames[i]).GetComponent<Button>(),
                    $"{destinationNames[i]} must be an independently named destination");
                Invoke(Required(destinations, destinationNames[i]));
                yield return WaitForRebuild();

                app = App(laptop);
                AssertEntryShell(app);
                AssertEntryPersistence(laptop, persistent, destinationNames[i]);

                Transform body = Required(app, "MarketBody");
                string currentBodyContent = AllText(body);
                Assert.AreNotEqual(previousBodyContent, currentBodyContent,
                    $"{destinationNames[i]} must change the displayed MarketBody destination/content");
                previousBodyContent = currentBodyContent;
            }
        }

        [UnityTest, Order(2)]
        public IEnumerator Entry_replacement_stays_selectable_and_wide_ink_is_deterministic_across_rebuilds()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            yield return OpenEntry(laptop);

            Button firstOffer = FirstNamedButton(Required(App(laptop), "MarketBody"), "Market");
            Invoke(firstOffer.transform);
            yield return WaitForRebuild();

            MarketSelection? original = laptop.Slip.SelectionOn(0);
            Assert.IsTrue(original.HasValue, "the ENTRY offer must write through to BetslipModel");
            Assert.AreEqual(1, laptop.Slip.Picks.Count);

            Image firstRing = WideRing(App(laptop));
            Assert.IsNotNull(firstRing.sprite, "WideBiroRing sprite missing");
            StringAssert.StartsWith("ring-wide-", firstRing.sprite.name,
                "wide selection ink must be prefix-filtered from the wide family");
            Assert.IsFalse(firstRing.raycastTarget, "decorative wide ink must not intercept the price");
            // The rule is cell + 16 on both axes (ASSETS.md, and the design system's InkMark.rect):
            // A1 widened the market cell from 160 to 176 wide (32 tall, unchanged), so the ring is
            // 192x48.
            AssertRect(firstRing.rectTransform, 192f, 48f, "WideBiroRing");
            string variant = firstRing.sprite.name;

            Invoke(Required(Required(App(laptop), "MarketDestinations"), "DetailTabBTTS"));
            yield return WaitForRebuild();
            Transform bttsBody = Required(App(laptop), "MarketBody");
            Button replacement = FirstNamedButton(bttsBody, "MarketBothTeamsToScore");
            Assert.IsTrue(replacement.interactable, "same-match replacement must remain selectable");
            StringAssert.Contains("⇄", TextOf(replacement.transform));
            Assert.IsNotNull(FindPrefix(bttsBody, "ReplacementUnderline"),
                "replacement offer needs its named underline");

            Invoke(replacement.transform);
            yield return WaitForRebuild();
            MarketSelection? replaced = laptop.Slip.SelectionOn(0);
            Assert.IsTrue(replaced.HasValue);
            Assert.AreNotEqual(original.Value, replaced.Value);
            Assert.AreEqual(1, laptop.Slip.Picks.Count,
                "replacement must retain the one-leg-per-matchup invariant");
            Assert.AreEqual(variant, WideRing(App(laptop)).sprite.name,
                "same matchup must retain its deterministic wide-ink variant");

            Invoke(Required(Required(App(laptop), "WorkingMargin"), "Chip25%"));
            yield return WaitForRebuild();
            Image rebuilt = WideRing(App(laptop));
            Assert.AreEqual(variant, rebuilt.sprite.name,
                "stake rebuild must retain the deterministic wide-ink variant");
            Assert.IsFalse(rebuilt.raycastTarget);
        }

        [UnityTest, Order(3)]
        public IEnumerator Working_margin_renders_every_staged_receipt_and_lock_tracks_only_current_marks()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            Run run = laptop.director.Run;
            var expected = new List<ReceiptExpectation>();

            Invoke(Required(Required(App(laptop), "Matchup0"), "AwayOdds"));
            yield return WaitForRebuild();
            Invoke(Required(Required(App(laptop), "Matchup1"), "HomeOdds"));
            yield return WaitForRebuild();
            expected.Add(Capture(laptop.Slip));
            Invoke(Required(Required(App(laptop), "WorkingMargin"), "Place"));
            yield return WaitForRebuild();

            Invoke(Required(Required(App(laptop), "Matchup2"), "AwayOdds"));
            yield return WaitForRebuild();
            expected.Add(Capture(laptop.Slip));
            Invoke(Required(Required(App(laptop), "WorkingMargin"), "Place"));
            yield return WaitForRebuild();

            Assert.AreEqual(expected.Count, run.Tickets.Count,
                "every placed model ticket must be staged by the engine");
            Assert.AreEqual(0, laptop.Slip.Picks.Count, "PLACE must clear the working marks");
            AssertReceipts(App(laptop), run, expected);

            Transform margin = Required(App(laptop), "WorkingMargin");
            Assert.IsTrue(Required(margin, "Lock").GetComponent<Button>().interactable,
                "staged receipts with no working marks must enable LOCK");
            Assert.IsNull(Find(margin, "LockReason"));

            Invoke(Required(Required(App(laptop), "Matchup0"), "HomeOdds"));
            yield return WaitForRebuild();
            Assert.AreEqual(1, laptop.Slip.Picks.Count);
            margin = Required(App(laptop), "WorkingMargin");
            AssertReceipts(App(laptop), run, expected);
            Assert.IsFalse(Required(margin, "Lock").GetComponent<Button>().interactable,
                "a new unplaced mark must disable LOCK without erasing receipts");
            Assert.AreEqual("PLACE OR CLEAR THIS WORKING SLIP",
                TextOf(Required(margin, "LockReason")));
        }

        [UnityTest, Order(4)]
        public IEnumerator Market_offer_rows_stay_within_the_market_viewport_horizontally_on_every_destination()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            yield return OpenEntry(laptop);

            // A4/S27: the list now scrolls, so a row is expected to sit below the visible viewport
            // once content overflows — that is exactly what RectMask2D/ScrollRect exist to clip, so
            // vertical containment is no longer the invariant (see the S27 rail test for the
            // overflow/clip behaviour itself). What must always hold, on every destination, whether
            // it scrolls or not, is horizontal containment against the viewport — A4 requires
            // content to stay clear of the S27 rail, so a row (and its price cell) may never run
            // wider than the viewport it is masked by.
            string[] destinationNames =
            {
                "DetailTabBTTS",
                "DetailTabCORNERS",
                "DetailTabCARDS",
                "DetailTabPLAYERS",
                "DetailTabGOALS",
            };

            foreach (string destinationName in destinationNames)
            {
                Invoke(Required(Required(App(laptop), "MarketDestinations"), destinationName));
                yield return WaitForRebuild();

                Transform bodyTransform = Required(App(laptop), "MarketBody");
                RectTransform viewport = Required(bodyTransform, "MarketViewport") as RectTransform;
                Assert.IsNotNull(viewport, $"{destinationName} MarketViewport must be a RectTransform");

                List<Transform> rows = AllNamed(bodyTransform, "MarketOffer");
                foreach (Transform row in rows)
                    AssertWithinContainerHorizontally(viewport, row as RectTransform, $"{destinationName} {row.name}");
                Assert.Greater(rows.Count, 0,
                    $"{destinationName} must render at least one market offer row for this invariant to mean anything");
            }
        }

        [UnityTest, Order(5)]
        public IEnumerator Every_destination_renders_a_single_column_of_offer_rows()
        {
            // S25 amended / A1: the fixed-body two-up ladder layout is withdrawn. Every destination
            // — including the ladders, which used to legitimately stay two-column — now renders one
            // offer per row, full body width. Inverts the old two-column-ladder assertion this
            // replaced.
            yield return Boot();
            LaptopScreen laptop = Laptop();
            yield return OpenEntry(laptop);

            string[] destinationNames =
            {
                "DetailTabGOALS",
                "DetailTabBTTS",
                "DetailTabCORNERS",
                "DetailTabCARDS",
                "DetailTabPLAYERS",
            };

            foreach (string destinationName in destinationNames)
            {
                Invoke(Required(Required(App(laptop), "MarketDestinations"), destinationName));
                yield return WaitForRebuild();

                List<float> rowX = OfferRowX(Required(App(laptop), "MarketBody"));
                Assert.Greater(rowX.Count, 0,
                    $"{destinationName} must render at least one offer for a single-column claim to mean anything");
                var distinctX = new HashSet<float>();
                foreach (float x in rowX) distinctX.Add(Mathf.Round(x * 10f) / 10f);
                Assert.AreEqual(1, distinctX.Count,
                    $"A1: every {destinationName} offer row must share the same column (row x position)");
            }
        }

        [UnityTest, Order(6)]
        public IEnumerator Every_engine_priced_offer_is_reachable_on_every_destination_C19()
        {
            // C19 (law): "an offer the engine prices is reachable on the surface." For every
            // destination, the number of rendered market-offer rows must equal the number of
            // offers the engine actually priced for that destination on this matchup — derived
            // from the engine (matchup.Markets filtered by kind), never a hardcoded number, so a
            // silently hidden offer fails here.
            yield return Boot();
            LaptopScreen laptop = Laptop();
            yield return OpenEntry(laptop);
            Run run = laptop.director.Run;
            Matchup matchup = run.CurrentSlate.Matchups[0];

            (string destination, MarketKind kind)[] destinations =
            {
                ("DetailTabGOALS", MarketKind.TotalGoals),
                ("DetailTabBTTS", MarketKind.BothTeamsToScore),
                ("DetailTabCORNERS", MarketKind.TotalCorners),
                ("DetailTabCARDS", MarketKind.TotalCards),
                ("DetailTabPLAYERS", MarketKind.AnytimeScorer),
            };

            foreach ((string destinationName, MarketKind kind) in destinations)
            {
                Invoke(Required(Required(App(laptop), "MarketDestinations"), destinationName));
                yield return WaitForRebuild();

                int expected = 0;
                foreach (MarketOffer offer in matchup.Markets)
                    if (offer.Selection.Kind == kind) expected++;
                Assert.Greater(expected, 0,
                    $"{destinationName} must have at least one engine-priced offer for C19 to mean anything");

                int rendered = AllNamed(Required(App(laptop), "MarketBody"), "MarketOffer").Count;
                Assert.AreEqual(expected, rendered,
                    $"C19: {destinationName} must render exactly the offers the engine priced ({expected}) — none hidden");
            }

            // Ladder counts must also match the run's own line configuration, not just an internal
            // engine-list/render-count agreement, so a config change is caught too.
            Assert.AreEqual(run.Config.GoalLines.Length * 2,
                CountByKind(matchup, MarketKind.TotalGoals), "GOALS offer count must track GoalLines");
            Assert.AreEqual(run.Config.CornerLines.Length * 2,
                CountByKind(matchup, MarketKind.TotalCorners), "CORNERS offer count must track CornerLines");
            Assert.AreEqual(run.Config.CardLines.Length * 2,
                CountByKind(matchup, MarketKind.TotalCards), "CARDS offer count must track CardLines");
        }

        [UnityTest, Order(7)]
        public IEnumerator S27_position_rail_appears_only_when_the_list_overflows()
        {
            // S27 ruling: a scrolling interior list carries a printed position rail — exactly two
            // images, present only when the content overflows the viewport, absent when it fits;
            // the thumb is clamped to a 24px floor, never taller than the track, and always lies
            // within the track.
            yield return Boot();
            LaptopScreen laptop = Laptop();
            yield return OpenEntry(laptop);

            // PLAYERS: S25 amended removed the capacity cap, and the shipped roster (PlayersPerTeam
            // per side, both teams) comfortably exceeds the ~7-row viewport at 54px/row, so PLAYERS
            // overflows and must carry the rail.
            Invoke(Required(Required(App(laptop), "MarketDestinations"), "DetailTabPLAYERS"));
            yield return WaitForRebuild();
            Transform overflowingBody = Required(App(laptop), "MarketBody");
            var trackRect = Required(overflowingBody, "PositionRailTrack") as RectTransform;
            var thumbRect = Required(overflowingBody, "PositionRailThumb") as RectTransform;
            Assert.IsNotNull(trackRect, "S27 track must be a RectTransform");
            Assert.IsNotNull(thumbRect, "S27 thumb must be a RectTransform");
            Assert.GreaterOrEqual(thumbRect.sizeDelta.y, 24f, "S27 thumb must never be shorter than its 24px floor");
            Assert.LessOrEqual(thumbRect.sizeDelta.y, trackRect.sizeDelta.y, "S27 thumb must never exceed the track");

            var trackCorners = new Vector3[4];
            var thumbCorners = new Vector3[4];
            trackRect.GetWorldCorners(trackCorners);
            thumbRect.GetWorldCorners(thumbCorners);
            const float epsilon = 0.5f;
            Assert.GreaterOrEqual(thumbCorners[0].y, trackCorners[0].y - epsilon,
                "S27 thumb bottom must lie within the track");
            Assert.LessOrEqual(thumbCorners[2].y, trackCorners[2].y + epsilon,
                "S27 thumb top must lie within the track");

            // BTTS is always exactly 2 rows (108px of content against a 412px viewport) — it never
            // overflows, so the rail must be entirely absent.
            Invoke(Required(Required(App(laptop), "MarketDestinations"), "DetailTabBTTS"));
            yield return WaitForRebuild();
            Transform fittingBody = Required(App(laptop), "MarketBody");
            Assert.IsNull(Find(fittingBody, "PositionRailTrack"), "S27 rail track must be absent when the list fits");
            Assert.IsNull(Find(fittingBody, "PositionRailThumb"), "S27 rail thumb must be absent when the list fits");
        }

        private static int CountByKind(Matchup matchup, MarketKind kind)
        {
            int count = 0;
            foreach (MarketOffer offer in matchup.Markets)
                if (offer.Selection.Kind == kind) count++;
            return count;
        }

        [Test, Order(8)]
        public void TicketStateWord_and_LegStateWord_never_cross_contaminate_their_vocabularies()
        {
            // S23 ruling: RIDING is ticket-level only, LIVE is leg-level only — contractual.
            // Driven over every enum member via Enum.GetValues so a future enum addition cannot
            // slip through unchecked.
            foreach (RevealedTicketState state in Enum.GetValues(typeof(RevealedTicketState)))
                Assert.AreNotEqual("LIVE", SportsbookApp.TicketStateWord(state),
                    $"TicketStateWord must never say LIVE (checked for {state})");

            foreach (RevealedLegState state in Enum.GetValues(typeof(RevealedLegState)))
                Assert.AreNotEqual("RIDING", SportsbookApp.LegStateWord(state),
                    $"LegStateWord must never say RIDING (checked for {state})");
        }

        [UnityTest, Order(9)]
        public IEnumerator CompactLegLabel_is_unique_for_every_distinct_selection_on_one_matchup()
        {
            // Composer uniqueness guard: a composer that reaches for the wrong field collapses
            // e.g. OVER 2.5 and UNDER 3.5 into one identical row while settlement still grades
            // them differently. Builds the full selection set for one matchup — both moneylines,
            // both BTTS sides, over+under for every goal/corner/card line in run.Config, and every
            // scorer index — and asserts CompactLegLabel never collides.
            yield return Boot();
            LaptopScreen laptop = Laptop();
            Run run = laptop.director.Run;
            Matchup matchup = run.CurrentSlate.Matchups[0];

            var selections = new List<MarketSelection>
            {
                MarketSelection.Moneyline(Side.Home),
                MarketSelection.Moneyline(Side.Away),
                MarketSelection.BothTeamsToScore(true),
                MarketSelection.BothTeamsToScore(false),
            };
            foreach (double line in run.Config.GoalLines)
            {
                selections.Add(MarketSelection.TotalGoals(line, true));
                selections.Add(MarketSelection.TotalGoals(line, false));
            }
            foreach (double line in run.Config.CornerLines)
            {
                selections.Add(MarketSelection.TotalCorners(line, true));
                selections.Add(MarketSelection.TotalCorners(line, false));
            }
            foreach (double line in run.Config.CardLines)
            {
                selections.Add(MarketSelection.TotalCards(line, true));
                selections.Add(MarketSelection.TotalCards(line, false));
            }
            int scorerCount = matchup.Away.Players.Count + matchup.Home.Players.Count;
            for (int i = 0; i < scorerCount; i++)
                selections.Add(MarketSelection.AnytimeScorer(i));

            Assert.Greater(selections.Count, 10, "the selection set must be non-trivial for this guard to mean anything");

            var seen = new HashSet<string>();
            foreach (MarketSelection selection in selections)
            {
                string label = SportsbookApp.CompactLegLabel(matchup, selection);
                Assert.IsTrue(seen.Add(label),
                    "CompactLegLabel collision: '" + label + "' for selection Kind=" + selection.Kind
                        + " Choice=" + selection.Choice + " Line=" + selection.Line
                        + " PlayerIndex=" + selection.PlayerIndex);
            }
        }

        /// <summary>Every "MarketOffer"+index row's own x position (anchoredPosition.x, relative to
        /// its MarketContent parent), used to prove a destination renders a single column (A1)
        /// without depending on any particular label-width choice. Recursive — under A4 the rows
        /// are nested inside MarketScroll/MarketViewport/MarketContent, not direct children of
        /// <paramref name="body"/>.</summary>
        private static List<float> OfferRowX(Transform body)
        {
            var xs = new List<float>();
            foreach (Transform row in AllNamed(body, "MarketOffer"))
            {
                var rect = row as RectTransform;
                Assert.IsNotNull(rect, $"{row.name} must be a RectTransform");
                xs.Add(rect.anchoredPosition.x);
            }
            return xs;
        }

        /// <summary>Every descendant of <paramref name="root"/> (root included) whose name starts
        /// with <paramref name="prefix"/>, depth-first. Unlike <see cref="FindPrefix"/> (first
        /// match only), this collects all of them — needed once offer rows can nest arbitrarily
        /// deep under a scroll/viewport/content hierarchy (A4).</summary>
        private static List<Transform> AllNamed(Transform root, string prefix)
        {
            var results = new List<Transform>();
            CollectNamed(root, prefix, results);
            return results;
        }

        private static void CollectNamed(Transform root, string prefix, List<Transform> results)
        {
            if (root.name.StartsWith(prefix, StringComparison.Ordinal)) results.Add(root);
            for (int i = 0; i < root.childCount; i++) CollectNamed(root.GetChild(i), prefix, results);
        }

        /// <summary>Fails if any part of <paramref name="child"/>'s rendered rect falls outside
        /// <paramref name="container"/>'s rendered rect on the X axis, measured in world space via
        /// GetWorldCorners so it holds regardless of anchor/pivot plumbing on either transform.
        /// corners[0]/[2] are the bottom-left/top-right corners for an unrotated rect. Horizontal
        /// only (A4): under a scrolling list a row may legitimately sit above/below the viewport's
        /// visible Y range — RectMask2D exists precisely to clip that — but a row must never run
        /// wider than the viewport, since A4 requires content to stay clear of the S27 rail.</summary>
        /// <summary>S25's general clause: "a container's correctness may not depend on a config
        /// dial's current value. Guard it with a test, not a convention." Allen capped MaxLegs at 4
        /// (2026-08-02) because the kit's two-line MarginLeg overflowed the fixed 324x530 margin at
        /// 6. That cap closes the overflow, but only for as long as nobody raises the dial — so this
        /// reads MaxLegs rather than assuming 4, fills a slip to it, and fails if the margin stops
        /// containing its own content. Both halves of the reported symptom are covered: content
        /// escaping the panel, and the PLACE button colliding with the bottom-fixed LOCK/SKIP band.
        /// </summary>
        [UnityTest, Order(9)]
        public IEnumerator Working_margin_contains_its_content_at_the_legal_maximum_leg_count()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            Run run = laptop.director.Run;
            int maxLegs = run.Config.MaxLegs;

            Assert.GreaterOrEqual(run.Config.MatchupsPerSlate, maxLegs,
                "the slate must be able to supply one leg per matchup up to the cap");

            // One selection per matchup — a second pick on the same matchup replaces rather than
            // adds — so filling to the cap means touching maxLegs distinct entries.
            for (int i = 0; i < maxLegs; i++)
            {
                Invoke(Required(Required(App(laptop), "Matchup" + i), "AwayOdds"));
                yield return WaitForRebuild();
            }

            // S50's named consequence: a STAGED RECEIPT adds flow the bare 4-leg figure never
            // contained, so the worst case is a full slip standing on top of a staged ticket, not a
            // full slip alone. Place once to stage a receipt (which clears the working slip), then
            // refill to the cap. Measuring only the empty-receipt state is what let the original
            // 414px figure understate the flow.
            Invoke(Required(Required(App(laptop), "WorkingMargin"), "Place"));
            yield return WaitForRebuild();
            Assert.Greater(run.Tickets.Count, 0, "a receipt must actually be staged");
            for (int i = 0; i < maxLegs; i++)
            {
                Invoke(Required(Required(App(laptop), "Matchup" + i), "AwayOdds"));
                yield return WaitForRebuild();
            }
            Assert.AreEqual(maxLegs, laptop.Slip.Picks.Count,
                "the slip must actually reach the cap for this invariant to mean anything");

            var margin = Required(App(laptop), "WorkingMargin") as RectTransform;
            Assert.IsNotNull(margin, "WorkingMargin must be a RectTransform");

            // MEASURE IN CANVAS-LOCAL PIXELS, never world units. The laptop is a world-space canvas
            // on a 3D quad: the whole 530px margin spans about 0.043 world units, so a pixel-shaped
            // tolerance like 0.5f is ~12x the entire panel and silently swallows every violation.
            // An earlier version of this test compared world corners with a 0.5f epsilon and passed
            // while the margin was visibly overlapping itself in the captures.
            const float epsilonPx = 0.5f;
            float marginTop = LocalTop(margin, margin);
            float marginBottom = LocalBottom(margin, margin);

            foreach (Graphic graphic in margin.GetComponentsInChildren<Graphic>(true))
            {
                var rect = graphic.rectTransform;
                if (rect == margin) continue;
                Assert.GreaterOrEqual(LocalBottom(rect, margin), marginBottom - epsilonPx,
                    $"{PathOf(rect, margin)} escapes the margin's bottom edge at {maxLegs} legs");
                Assert.LessOrEqual(LocalTop(rect, margin), marginTop + epsilonPx,
                    $"{PathOf(rect, margin)} escapes the margin's top edge at {maxLegs} legs");
            }

            // The reported 4-leg symptom was the action stack colliding, which containment alone
            // would not catch — PLACE is top-anchored below the legs, LOCK/SKIP are bottom-fixed.
            // Every element in the action stack, in the order they must appear down the margin.
            // Comparing only PLACE against LOCK is not enough: the blocked-action reason is its own
            // absolutely-positioned element and can collide with the payout figure above it while
            // PLACE and LOCK stay clear of each other.
            string[] stack = { "Payout", "PlaceReason", "Place", "LockReason", "Lock", "Skip" };
            var present = new List<(string Name, float Top, float Bottom)>();
            foreach (string name in stack)
            {
                Transform node = margin.Find(name);
                if (node == null) continue;          // not every element exists in every state
                var rt = (RectTransform)node;
                present.Add((name, LocalTop(rt, margin), LocalBottom(rt, margin)));
            }

            // Sorted by position, so the check is "nothing occupies the same band as anything else"
            // rather than "the authored order happens to hold" — an element that jumps the stack is
            // still a collision.
            present.Sort((a, b) => b.Top.CompareTo(a.Top));
            for (int i = 1; i < present.Count; i++)
            {
                var above = present[i - 1];
                var below = present[i];
                Assert.GreaterOrEqual(above.Bottom, below.Top - epsilonPx,
                    $"{above.Name} (bottom {above.Bottom:F1}px) overlaps {below.Name} "
                    + $"(top {below.Top:F1}px) at {maxLegs} legs — the action stack collides");
            }

            // T47's reservation, checked directly rather than inferred from the absence of overlap:
            // the flow region must fit its budget, and the anchored band must actually be anchored.
            float flowBottom = float.MaxValue;
            foreach (Graphic graphic in margin.GetComponentsInChildren<Graphic>(true))
            {
                var rect = graphic.rectTransform;
                if (rect == margin) continue;
                // Anything parented into an action control belongs to the band, not the flow.
                if (rect.GetComponentInParent<Button>() != null) continue;
                // The ruled-paper ground (S34) is the panel's SUBSTRATE, not flow content: it is a
                // stretch-fill Graphic spanning the full 530px by design, so counting it reports the
                // panel's own height as the flow's depth and the budget check can never pass. Caught
                // by this assertion firing at exactly -530.0px — the panel's bottom edge, not a
                // coincidence. Excluded by stretch, not by name, so any future full-bleed ground is
                // excluded too.
                // Excluded by MEASURED COVERAGE, not by anchoring and not by name. The first
                // version of this test excluded "anchor-stretched" graphics — and the very next
                // change to the ground (main's CanvasRenderer/explicit-size fix, which had to stop
                // anchor-stretching because a stretched rect reads zero on this imperatively-built
                // canvas) made the predicate stop matching the one thing it existed to skip. The
                // assertion then reported the panel's own height, -530.0px, as the flow's depth.
                // A ground is a thing that covers the whole panel; that is what is tested here, so
                // it holds however the ground happens to be anchored.
                bool coversWholePanel = LocalTop(rect, margin) >= marginTop - epsilonPx
                    && LocalBottom(rect, margin) <= marginBottom + epsilonPx;
                if (coversWholePanel) continue;
                flowBottom = Mathf.Min(flowBottom, LocalBottom(rect, margin));
            }
            // S51 — SIGNED, EXPIRING DEVIATION (DD 2026-08-04). The flow's lowest element sits 2.6px
            // outside its reservation with a staged receipt at MaxLegs. Named cost: one UN-OWNED
            // 2.6px excursion in the margin's reserved region. Expiry: when the owner of the 2.6px
            // is identified — at which point it is FIXED, not re-signed.
            //
            // This is RECORDED here, deliberately, rather than made to disappear. The reservation is
            // not slackened and no element is excluded from the measurement: the ruling forbids both,
            // because either would have gone green while the real overrun continued. The wax
            // highlight was the lead's candidate and the frames falsified it — it measures 23–24px,
            // and at 0.5° a 24px band grows 0.21px, twelve times too small. Excluding it would have
            // been the fortnight's fifth vacuous gate.
            //
            // Asserted as an EQUALITY so the pin is two-sided: this fails if the overrun grows, and
            // it also fails if it shrinks. A silent improvement is not a win here — it means someone
            // changed the thing nobody has identified, and the register entry must be closed by
            // whoever did it rather than quietly going green.
            const float signedOverrunPx = 2.6f;
            const float signedOverrunTolerancePx = 0.15f;
            float overrunPx = -SportsbookApp.MarginFlowBudget - flowBottom;
            Assert.AreEqual(signedOverrunPx, overrunPx, signedOverrunTolerancePx,
                $"the margin flow's overrun moved: measured {overrunPx:F2}px against the signed "
                + $"{signedOverrunPx:F1}px (S51). Lowest flow element {flowBottom:F1}px, budget "
                + $"-{SportsbookApp.MarginFlowBudget:F0}px, action band reserves "
                + $"{SportsbookApp.ActionBandReservedHeight:F0}px. If this SHRANK, the 2.6px owner "
                + "has been found — fix it and close S51 rather than re-signing. If it GREW, "
                + "something entered the margin flow: staged receipts live in the 700px sheet and "
                + "must never re-enter it (both-screens kit amendment, DD 2026-08-04).");

            // T53 — every gate states what it cannot see. THIS ONE CANNOT SEE:
            //  · rendered glyphs. It measures RectTransforms, so text bleeding outside its own rect
            //    (MakeText falls back to Overflow rather than clipping) is invisible here.
            //  · anything without a Graphic — empty layout containers contribute no bounds.
            //  · horizontal collisions. It is a vertical-stack check only.
            //  · z-order. Two elements sharing a band are caught; one correctly drawn over another
            //    on purpose is not distinguished from one accidentally buried.
            //  · any leg count other than MaxLegs. It now exercises a full slip ON TOP OF a staged
            //    receipt (S50's named consequence), but not multiple staged receipts, and not the
            //    board-frozen state, whose copy differs.
            //  · the REWARDS and MY BETS passive margins, which have their own content.
        }

        /// <summary>A RectTransform's top/bottom edge expressed in <paramref name="basis"/>'s local
        /// pixels. Every containment check on this surface must go through these: the laptop canvas
        /// is world-space on a 3D quad, so GetWorldCorners yields metres and any pixel-shaped
        /// tolerance compared against them is orders of magnitude too large to catch anything.</summary>
        private static float LocalTop(RectTransform rect, RectTransform basis)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return basis.InverseTransformPoint(corners[1]).y;
        }

        private static float LocalBottom(RectTransform rect, RectTransform basis)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return basis.InverseTransformPoint(corners[3]).y;
        }

        private static string PathOf(Transform node, Transform root)
        {
            string path = node.name;
            for (Transform t = node.parent; t != null && t != root; t = t.parent) path = t.name + "/" + path;
            return path;
        }

        private static void AssertWithinContainerHorizontally(RectTransform container, RectTransform child, string label)
        {
            Assert.IsNotNull(child, $"{label} RectTransform missing");
            // Canvas-local pixels, for the same reason as LocalTop/LocalBottom: this comparison used
            // to run against world corners with a 0.5f tolerance, which on a world-space canvas is
            // larger than the whole surface — it could not have failed for any layout.
            var childCorners = new Vector3[4];
            child.GetWorldCorners(childCorners);
            float childLeft = container.InverseTransformPoint(childCorners[0]).x;
            float childRight = container.InverseTransformPoint(childCorners[2]).x;
            Rect bounds = container.rect;
            const float epsilonPx = 0.5f;
            Assert.GreaterOrEqual(childLeft, bounds.xMin - epsilonPx,
                $"{label} left edge escapes the viewport ({childLeft:F1}px vs {bounds.xMin:F1}px)");
            Assert.LessOrEqual(childRight, bounds.xMax + epsilonPx,
                $"{label} right edge escapes the viewport ({childRight:F1}px vs {bounds.xMax:F1}px)");
        }

        private static ReceiptExpectation Capture(BetslipModel slip)
            => new ReceiptExpectation(slip.Stake, slip.CombinedOdds, slip.ToWin);

        private static EntryPersistenceSnapshot CaptureEntryPersistence(LaptopScreen laptop)
        {
            Transform app = App(laptop);
            Transform margin = Required(app, "WorkingMargin");
            Assert.AreEqual(1, laptop.director.Run.Tickets.Count,
                "persistence setup needs one staged ticket");
            Assert.AreEqual(1, laptop.Slip.Picks.Count,
                "persistence setup needs one working mark");
            return new EntryPersistenceSnapshot
            {
                Machine = TextOf(Required(app, "Machine")),
                Sticker = TextOf(Required(app, "Sticker")),
                Clock = TextOf(Required(app, "Clock")),
                EventIdentity = TextOf(Required(app, "EventIdentity")),
                EventRecords = TextOf(Required(app, "EventRecords")),
                EventForm = TextOf(Required(app, "EventForm")),
                MarginTitle = TextOf(Required(margin, "Title")),
                MarginCount = TextOf(Required(margin, "Count")),
                WorkingLeg = TextOf(Required(margin, "Leg0")),
                Stake = TextOf(Required(margin, "Stake")),
                Payout = TextOf(Required(margin, "Payout")),
                ReceiptHeader = TextOf(Required(app, "ReceiptHeader")),
                TicketCount = laptop.director.Run.Tickets.Count,
                PickCount = laptop.Slip.Picks.Count,
                ModelStake = laptop.Slip.Stake,
                Selection = laptop.Slip.SelectionOn(0),
            };
        }

        private static void AssertEntryPersistence(LaptopScreen laptop,
            EntryPersistenceSnapshot expected, string destination)
        {
            Transform app = App(laptop);
            Transform margin = Required(app, "WorkingMargin");
            AssertRect(Required(app, "EventIdentity") as RectTransform, 398f, 32f,
                $"{destination} ENTRY header identity");
            AssertRect(Required(app, "EventRecords") as RectTransform, 150f, 32f,
                $"{destination} ENTRY header records");
            AssertRect(Required(app, "EventForm") as RectTransform, 670f, 32f,
                $"{destination} ENTRY header form");

            Assert.AreEqual(expected.Machine, TextOf(Required(app, "Machine")),
                $"{destination} machine chrome");
            Assert.AreEqual(expected.Sticker, TextOf(Required(app, "Sticker")),
                $"{destination} ownership chrome");
            Assert.AreEqual(expected.Clock, TextOf(Required(app, "Clock")),
                $"{destination} clock chrome");
            Assert.AreEqual(expected.EventIdentity, TextOf(Required(app, "EventIdentity")),
                $"{destination} ENTRY identity");
            Assert.AreEqual(expected.EventRecords, TextOf(Required(app, "EventRecords")),
                $"{destination} ENTRY records");
            Assert.AreEqual(expected.EventForm, TextOf(Required(app, "EventForm")),
                $"{destination} ENTRY form");

            Assert.AreEqual(expected.MarginTitle, TextOf(Required(margin, "Title")),
                $"{destination} working-margin title");
            Assert.AreEqual(expected.MarginCount, TextOf(Required(margin, "Count")),
                $"{destination} working-margin rule");
            Assert.AreEqual(expected.WorkingLeg, TextOf(Required(margin, "Leg0")),
                $"{destination} working mark");
            Assert.AreEqual(expected.Stake, TextOf(Required(margin, "Stake")),
                $"{destination} working stake");
            Assert.AreEqual(expected.Payout, TextOf(Required(margin, "Payout")),
                $"{destination} working payout");
            Assert.AreEqual(expected.ReceiptHeader, TextOf(Required(app, "ReceiptHeader")),
                $"{destination} staged receipt");
            Assert.AreEqual(expected.TicketCount, laptop.director.Run.Tickets.Count,
                $"{destination} staged-ticket model count");
            Assert.AreEqual(expected.PickCount, laptop.Slip.Picks.Count,
                $"{destination} working-pick model count");
            Assert.AreEqual(expected.ModelStake, laptop.Slip.Stake, 1e-9,
                $"{destination} working stake model");
            Assert.AreEqual(expected.Selection, laptop.Slip.SelectionOn(0),
                $"{destination} working selection model");
        }

        /// <summary>Takes the app root, not the margin: E-07 moved staged receipts out of the 324px
        /// working margin and into the 700px sheet, where the kit puts them (screens.jsx:50-57).
        /// Required searches recursively, so the app root spans both regions.</summary>
        private static void AssertReceipts(Transform app, Run run,
            IReadOnlyList<ReceiptExpectation> expected)
        {
            Transform receipts = Required(app, "StagedTickets");
            int rendered = 0;
            for (int i = 0; i < receipts.childCount; i++)
                if (receipts.GetChild(i).name.StartsWith("StagedTicket", StringComparison.Ordinal))
                    rendered++;
            Assert.AreEqual(run.Tickets.Count, rendered, "rendered staged-ticket count");

            for (int ticketIndex = 0; ticketIndex < run.Tickets.Count; ticketIndex++)
            {
                Ticket ticket = run.Tickets[ticketIndex];
                ReceiptExpectation model = expected[ticketIndex];
                Assert.AreEqual(model.Stake, ticket.Stake, 1e-9, $"ticket {ticketIndex} stake");

                double combined = 1.0;
                for (int legIndex = 0; legIndex < ticket.Legs.Count; legIndex++)
                    combined *= ticket.Legs[legIndex].OfferedOdds;
                Assert.AreEqual(model.Combined, combined, 1e-9,
                    $"ticket {ticketIndex} combined odds");
                Assert.AreEqual(model.Payout, ticket.PotentialPayout, 1e-9,
                    $"ticket {ticketIndex} potential payout");

                Transform receipt = Required(receipts, "StagedTicket" + ticketIndex);
                string identity = string.IsNullOrEmpty(ticket.Id)
                    ? $"{run.Round}.{ticketIndex + 1}" : ticket.Id;
                // Matches BuildStagedReceipt's own width-fitting formula rather than a duplicated
                // literal, so a legitimate format change (fixing the mid-word truncation defect)
                // can never quietly desync the fixture from the render code.
                Font font = TestFont(receipt);
                // Read the fitting width off the rendered header instead of restating it. This
                // constant was 280f — the value when staged receipts lived in the 324px margin —
                // and E-07 moved them into the 700px sheet without it following. The comment above
                // already claimed the fixture matched the render code's own formula rather than a
                // duplicated literal; it did not, and this makes that true.
                //
                // The failure mode it hid is worth naming, because a green suite is not evidence
                // it was absent: the test and the renderer only disagree when a label's fitted
                // width falls BETWEEN the two numbers. Shorter than 280 and both return the string
                // untouched; longer than the real width and both truncate identically. Only the
                // band between them fails, so this passed on every run whose generated team names
                // happened to be short enough, and failed on the first one that was not.
                float receiptTextWidth =
                    Required(receipt, "ReceiptHeader").GetComponent<RectTransform>().rect.width;
                // "STAGED" was dropped from the header on purpose: the block itself is the staged
                // receipt, and the word was what pushed "PAYS $167" past the 280px fit and into a
                // mid-word ellipsis. The payout is a product fact and outranks a redundant label.
                Assert.AreEqual(
                    LaptopUi.FitText(font,
                        $"TICKET {identity} · {Money(model.Stake)} · " +
                        $"{OddsFormat.American(model.Combined)} · PAYS {Money(model.Payout)}",
                        13, receiptTextWidth),
                    TextOf(Required(receipt, "ReceiptHeader")));

                for (int legIndex = 0; legIndex < ticket.Legs.Count; legIndex++)
                {
                    Leg leg = ticket.Legs[legIndex];
                    Assert.AreEqual(
                        LaptopUi.FitLabelKeepingSuffix(font, $"{legIndex + 1}. ",
                            SportsbookApp.CompactLegLabel(leg.Matchup, leg.Selection),
                            $"  {OddsFormat.American(leg.OfferedOdds)}", 13, receiptTextWidth),
                        TextOf(Required(receipt, "TicketLeg" + legIndex)));
                }
            }
        }

        private static IEnumerator Boot()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("Room", LoadSceneMode.Single);
            Assert.IsNotNull(load, "Room scene is not available");
            while (!load.isDone) yield return null;

            LaptopScreen laptop = Laptop();
            float start = Time.realtimeSinceStartup;
            while (laptop.director == null || laptop.director.Run == null || laptop.Os.OnDesktop)
            {
                if (Time.realtimeSinceStartup - start > 10f)
                {
                    Assert.Fail("SureThing did not reach the betting lobby within 10 seconds");
                    yield break;
                }
                yield return null;
            }
            yield return null;
        }

        private static IEnumerator OpenEntry(LaptopScreen laptop)
        {
            Invoke(Required(Required(App(laptop), "Matchup0"), "Details"));
            yield return WaitForRebuild();
            Assert.AreEqual(SportsbookApp.Tab.Detail, laptop.Os.CurrentTab);
        }

        private static IEnumerator WaitForRebuild()
        {
            yield return null;
            yield return null;
        }

        private static LaptopScreen Laptop()
        {
            LaptopScreen laptop = UnityEngine.Object.FindAnyObjectByType<LaptopScreen>();
            Assert.IsNotNull(laptop, "LaptopScreen missing");
            return laptop;
        }

        private static Transform App(LaptopScreen laptop) => Required(laptop.transform, "App");

        private static void AssertEntryShell(Transform app)
        {
            AssertRect(Required(app, "Chrome") as RectTransform, 1024f, 140f, "chrome");
            AssertRect(Required(app, "NotebookRail") as RectTransform, 1024f, 34f, "rail");
            AssertRect(Required(app, "FormTabs") as RectTransform, 1024f, 38f, "tabs");
            AssertRect(Required(app, "FormMasthead") as RectTransform, 1024f, 68f, "masthead");
            AssertRect(Required(app, "EntryBoard") as RectTransform, 700f, 530f, "ENTRY board");
            AssertRect(Required(app, "WorkingMargin") as RectTransform, 324f, 530f,
                "working margin");
            AssertRect(Required(app, "NotebookTray") as RectTransform, 1024f, 34f, "tray");
        }

        private static Image WideRing(Transform app)
        {
            Transform ring = Required(Required(app, "MarketBody"), "WideBiroRing");
            Image image = ring.GetComponent<Image>();
            Assert.IsNotNull(image, "WideBiroRing must be an Image");
            return image;
        }

        private static Button FirstNamedButton(Transform root, string prefix)
        {
            var matches = new List<Button>();
            foreach (Button button in root.GetComponentsInChildren<Button>(true))
                if (button.name.StartsWith(prefix, StringComparison.Ordinal))
                    matches.Add(button);
            matches.Sort((left, right) => string.CompareOrdinal(left.name, right.name));
            Assert.Greater(matches.Count, 0,
                $"No button beginning '{prefix}' exists beneath '{root.name}'");
            return matches[0];
        }

        private static Transform Required(Transform root, string name)
        {
            Transform found = Find(root, name);
            Assert.IsNotNull(found, $"Required named UI node '{name}' missing beneath '{root.name}'");
            return found;
        }

        private static Transform Find(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = Find(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }

        private static Transform FindPrefix(Transform root, string prefix)
        {
            if (root.name.StartsWith(prefix, StringComparison.Ordinal)) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindPrefix(root.GetChild(i), prefix);
                if (found != null) return found;
            }
            return null;
        }

        private static void Invoke(Transform node)
        {
            Button button = node.GetComponent<Button>();
            Assert.IsNotNull(button, $"{node.name} must be a button");
            Assert.IsTrue(button.interactable, $"{node.name} must be interactable");
            button.onClick.Invoke();
        }

        private static string TextOf(Transform node)
        {
            Text text = node.GetComponent<Text>();
            if (text == null) text = node.GetComponentInChildren<Text>();
            Assert.IsNotNull(text, $"{node.name} has no readable text");
            return text.text;
        }

        /// <summary>The same built-in font LaptopScreen loads, fetched independently so fixture-side
        /// width calculations measure glyphs identically to production.</summary>
        /// <summary>
        /// The face the receipt is actually rendered in, read off the rendered element rather than
        /// assumed. This fixture recomputes expected strings through the same FitText the runtime
        /// uses, so it has to measure with the same font: it was pinned to LegacyRuntime and started
        /// failing the moment the production faces were wired, because Archivo Narrow is narrower
        /// and more characters now fit before the ellipsis. That was the fixture being wrong about
        /// the font, not the UI being wrong about the text.
        /// </summary>
        private static Font TestFont(Transform receipt)
        {
            var sample = Required(receipt, "ReceiptHeader").GetComponent<Text>();
            Assert.IsNotNull(sample, "ReceiptHeader must carry a Text to measure against");
            Assert.IsNotNull(sample.font, "ReceiptHeader has no font; the production face failed to load");
            return sample.font;
        }

        private static string AllText(Transform root)
        {
            var content = new List<string>();
            foreach (Text text in root.GetComponentsInChildren<Text>(true))
                content.Add(text.name + "=" + text.text);
            content.Sort(StringComparer.Ordinal);
            return string.Join("\n", content);
        }

        private static string Money(double value)
        {
            long rounded = (long)Math.Round(value, MidpointRounding.AwayFromZero);
            return "$" + rounded.ToString("N0", CultureInfo.InvariantCulture);
        }

        private static void AssertRect(RectTransform rect, float width, float height, string label)
        {
            Assert.IsNotNull(rect, $"{label} RectTransform missing");
            Assert.AreEqual(width, rect.sizeDelta.x, 0.01f, $"{label} width");
            Assert.AreEqual(height, rect.sizeDelta.y, 0.01f, $"{label} height");
        }

        private sealed class ReceiptExpectation
        {
            public readonly double Stake;
            public readonly double Combined;
            public readonly double Payout;

            public ReceiptExpectation(double stake, double combined, double payout)
            {
                Stake = stake;
                Combined = combined;
                Payout = payout;
            }
        }

        private sealed class EntryPersistenceSnapshot
        {
            public string Machine;
            public string Sticker;
            public string Clock;
            public string EventIdentity;
            public string EventRecords;
            public string EventForm;
            public string MarginTitle;
            public string MarginCount;
            public string WorkingLeg;
            public string Stake;
            public string Payout;
            public string ReceiptHeader;
            public int TicketCount;
            public int PickCount;
            public double ModelStake;
            public MarketSelection? Selection;
        }
    }
}
