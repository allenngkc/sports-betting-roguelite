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
            AssertRect(firstRing.rectTransform, 176f, 46f, "WideBiroRing");
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
                Font font = TestFont();
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
        private static Font TestFont() => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

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
