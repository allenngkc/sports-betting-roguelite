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
            string[] titleNames =
            {
                "BttsTitle",
                "MarketTitle",
                "MarketTitle",
                "PlayersTitle",
                "MarketTitle",
            };
            string[] titles =
            {
                "BOTH TEAMS TO SCORE",
                "CORNERS TOTAL",
                "CARDS TOTAL",
                "ANYTIME GOALSCORER",
                "GOALS TOTAL",
            };

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
                Assert.AreEqual(titles[i], TextOf(Required(body, titleNames[i])));
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
            // 46 was the ring-wide sprite's native height, which happened to sit 2px under the rule.
            // The rule is cell + 16 on both axes (ASSETS.md, and the design system's InkMark.rect):
            // the real market cell is 160x32, so the ring is 176x48.
            AssertRect(firstRing.rectTransform, 176f, 48f, "WideBiroRing");
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
            AssertReceipts(Required(App(laptop), "WorkingMargin"), run, expected);

            Transform margin = Required(App(laptop), "WorkingMargin");
            Assert.IsTrue(Required(margin, "Lock").GetComponent<Button>().interactable,
                "staged receipts with no working marks must enable LOCK");
            Assert.IsNull(Find(margin, "LockReason"));

            Invoke(Required(Required(App(laptop), "Matchup0"), "HomeOdds"));
            yield return WaitForRebuild();
            Assert.AreEqual(1, laptop.Slip.Picks.Count);
            margin = Required(App(laptop), "WorkingMargin");
            AssertReceipts(margin, run, expected);
            Assert.IsFalse(Required(margin, "Lock").GetComponent<Button>().interactable,
                "a new unplaced mark must disable LOCK without erasing receipts");
            Assert.AreEqual("PLACE OR CLEAR THIS WORKING SLIP",
                TextOf(Required(margin, "LockReason")));
        }

        [UnityTest, Order(4)]
        public IEnumerator Market_offer_rows_stay_fully_within_the_MarketBody_panel_on_every_destination()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            yield return OpenEntry(laptop);

            // Pins the offer-container overflow invariant: BuildMarketLines/BuildBothTeamsScore/
            // BuildPlayerLines all lay "MarketOffer"+index rows directly into MarketBody (700x412,
            // MakeMarketOffer's cell 160x32, 42px pitch, two columns). At the shipped roster size
            // every destination's rows fit, but nothing clamps or scrolls them — this test measures
            // each rendered offer row's real corners against MarketBody's real corners so a future
            // change that pushes row count past capacity (e.g. a larger PlayersPerTeam) fails here
            // instead of silently rendering offers outside the panel with no scroll/clamp/"N not
            // shown". It does not judge how that should eventually be treated — only whether the
            // invariant holds right now.
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
                RectTransform body = bodyTransform as RectTransform;
                Assert.IsNotNull(body, $"{destinationName} MarketBody must be a RectTransform");

                int offerCount = 0;
                for (int i = 0; i < bodyTransform.childCount; i++)
                {
                    Transform child = bodyTransform.GetChild(i);
                    if (!child.name.StartsWith("MarketOffer", StringComparison.Ordinal)) continue;
                    offerCount++;
                    AssertWithinContainer(body, child as RectTransform, $"{destinationName} {child.name}");
                }
                Assert.Greater(offerCount, 0,
                    $"{destinationName} must render at least one market offer row for this invariant to mean anything");
            }
        }

        [UnityTest, Order(5)]
        public IEnumerator Players_destination_renders_single_column_while_ladder_destinations_stay_two_column()
        {
            // S24 ruling: the scorer board is single column (never a paired row with a dead
            // second cell) — distinguished here from a ladder destination, which legitimately
            // stays two-column (paired OVER/UNDER offers).
            yield return Boot();
            LaptopScreen laptop = Laptop();
            yield return OpenEntry(laptop);

            Invoke(Required(Required(App(laptop), "MarketDestinations"), "DetailTabPLAYERS"));
            yield return WaitForRebuild();
            List<float> playersX = OfferPriceCellX(Required(App(laptop), "MarketBody"));
            Assert.Greater(playersX.Count, 1,
                "PLAYERS must render more than one offer for a single-column claim to mean anything");
            for (int i = 1; i < playersX.Count; i++)
                Assert.AreEqual(playersX[0], playersX[i], 0.01f,
                    "S24: every PLAYERS offer row must share the same column (price-cell x position)");

            Invoke(Required(Required(App(laptop), "MarketDestinations"), "DetailTabGOALS"));
            yield return WaitForRebuild();
            List<float> goalsX = OfferPriceCellX(Required(App(laptop), "MarketBody"));
            var distinctGoalsX = new HashSet<float>();
            foreach (float x in goalsX) distinctGoalsX.Add(Mathf.Round(x * 10f) / 10f);
            Assert.AreEqual(2, distinctGoalsX.Count,
                "GOALS legitimately remains two-column (paired OVER/UNDER offers), unlike PLAYERS");
        }

        [Test, Order(6)]
        public void VisibleOfferCapacity_holds_as_pure_arithmetic_independent_of_any_config_dial()
        {
            // S25 ruling ("S17 binds the PLAYERS tab"): container correctness never depends on a
            // config dial. Shipped geometry is MarketBody 700x412, first row at -48, 42px pitch.
            // At PlayersPerTeam's shipped value of 7 the scorer board is 14 offers; single-column
            // capacity below is 8, so today 6 offers are hidden behind "N NOT SHOWN".
            Assert.AreEqual(8, SportsbookApp.VisibleOfferCapacity(412f, 48f, 42f, 1),
                "shipped single-column PLAYERS capacity");
            Assert.AreEqual(16, SportsbookApp.VisibleOfferCapacity(412f, 48f, 42f, 2),
                "capacity scales with column count for the same row geometry");
            Assert.AreEqual(24, SportsbookApp.VisibleOfferCapacity(412f, 48f, 42f, 3),
                "capacity scales with column count for the same row geometry");
            Assert.AreEqual(0, SportsbookApp.VisibleOfferCapacity(0f, 48f, 42f, 1),
                "container shorter than the first row fits nothing");
            Assert.AreEqual(1, SportsbookApp.VisibleOfferCapacity(100f, 48f, 42f, 1),
                "a container that fits only the first row");
            Assert.AreEqual(0, SportsbookApp.VisibleOfferCapacity(412f, 5000f, 42f, 1),
                "a first-row offset past the container fits nothing");
            Assert.AreEqual(0, SportsbookApp.VisibleOfferCapacity(412f, 48f, 42f, 0),
                "zero columns fits nothing");
            Assert.AreEqual(0, SportsbookApp.VisibleOfferCapacity(412f, 48f, 42f, -3),
                "negative columns must clamp to zero, not negative");
            Assert.AreEqual(0, SportsbookApp.VisibleOfferCapacity(412f, 48f, -1f, 1),
                "negative pitch must clamp to zero, not throw or go negative");
            Assert.AreEqual(0, SportsbookApp.VisibleOfferCapacity(412f, 48f, 0f, 1),
                "zero pitch must clamp to zero rather than divide by zero");
            Assert.AreEqual(0, SportsbookApp.VisibleOfferCapacity(-100f, 48f, 42f, 1),
                "negative container height fits nothing");
            Assert.AreEqual(11, SportsbookApp.VisibleOfferCapacity(412f, -48f, 42f, 1),
                "an unusual but valid negative offset still computes correctly, not just non-negatively");

            // Broad sweep: whatever the inputs, capacity must never be negative.
            for (float height = -50f; height <= 1000f; height += 37f)
                for (float pitch = -10f; pitch <= 100f; pitch += 13f)
                    for (int columns = -2; columns <= 5; columns++)
                        Assert.GreaterOrEqual(SportsbookApp.VisibleOfferCapacity(height, 48f, pitch, columns), 0,
                            $"capacity must never be negative (height={height}, pitch={pitch}, columns={columns})");
        }

        [Test, Order(7)]
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

        [UnityTest, Order(8)]
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

        /// <summary>Every "MarketOffer"+index panel's price-cell x position within
        /// <paramref name="body"/>, used to distinguish single-column (S24) from two-column
        /// destinations without depending on any particular label-width choice.</summary>
        private static List<float> OfferPriceCellX(Transform body)
        {
            var xs = new List<float>();
            for (int i = 0; i < body.childCount; i++)
            {
                Transform child = body.GetChild(i);
                if (!child.name.StartsWith("MarketOffer", StringComparison.Ordinal)) continue;
                var rect = child as RectTransform;
                Assert.IsNotNull(rect, $"{child.name} must be a RectTransform");
                xs.Add(rect.anchoredPosition.x);
            }
            return xs;
        }

        /// <summary>Fails if any part of <paramref name="child"/>'s rendered rect falls outside
        /// <paramref name="container"/>'s rendered rect, measured in world space via
        /// GetWorldCorners so it holds regardless of anchor/pivot plumbing on either transform.
        /// corners[0]/[2] are the bottom-left/top-right corners for an unrotated rect.</summary>
        private static void AssertWithinContainer(RectTransform container, RectTransform child, string label)
        {
            Assert.IsNotNull(child, $"{label} RectTransform missing");
            var containerCorners = new Vector3[4];
            var childCorners = new Vector3[4];
            container.GetWorldCorners(containerCorners);
            child.GetWorldCorners(childCorners);
            const float epsilon = 0.5f;
            Assert.GreaterOrEqual(childCorners[0].x, containerCorners[0].x - epsilon,
                $"{label} left edge escapes MarketBody");
            Assert.LessOrEqual(childCorners[2].x, containerCorners[2].x + epsilon,
                $"{label} right edge escapes MarketBody");
            Assert.GreaterOrEqual(childCorners[0].y, containerCorners[0].y - epsilon,
                $"{label} bottom edge escapes MarketBody");
            Assert.LessOrEqual(childCorners[2].y, containerCorners[2].y + epsilon,
                $"{label} top edge escapes MarketBody");
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
                MarginRule = TextOf(Required(margin, "Rule")),
                WorkingLeg = TextOf(Required(margin, "Leg0")),
                Stake = TextOf(Required(margin, "Stake")),
                Payout = TextOf(Required(margin, "Payout")),
                ReceiptHeader = TextOf(Required(margin, "ReceiptHeader")),
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
            Assert.AreEqual(expected.MarginRule, TextOf(Required(margin, "Rule")),
                $"{destination} working-margin rule");
            Assert.AreEqual(expected.WorkingLeg, TextOf(Required(margin, "Leg0")),
                $"{destination} working mark");
            Assert.AreEqual(expected.Stake, TextOf(Required(margin, "Stake")),
                $"{destination} working stake");
            Assert.AreEqual(expected.Payout, TextOf(Required(margin, "Payout")),
                $"{destination} working payout");
            Assert.AreEqual(expected.ReceiptHeader, TextOf(Required(margin, "ReceiptHeader")),
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

        private static void AssertReceipts(Transform margin, Run run,
            IReadOnlyList<ReceiptExpectation> expected)
        {
            Transform receipts = Required(margin, "StagedTickets");
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
                const float receiptTextWidth = 280f;
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
            public string MarginRule;
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
