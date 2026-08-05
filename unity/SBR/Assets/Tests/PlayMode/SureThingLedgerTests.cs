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
    public class SureThingLedgerTests
    {
        [UnityTest, Order(1)]
        public IEnumerator Ledger_is_empty_until_a_truthful_current_run_ticket_settles()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            Run run = laptop.director.Run;
            yield return OpenLedgerThroughTray(laptop);

            Transform app = App(laptop);
            AssertLedgerShell(app);
            Transform board = Required(app, "LedgerBoard");
            Transform margin = Required(app, "LedgerMargin");
            Assert.IsNotNull(Required(board, "LedgerEmpty"));
            Assert.IsNotNull(Required(board, "LedgerEmptyScope"));
            Assert.IsNull(Find(board, "LedgerTicket0"));
            Assert.Zero(board.GetComponentsInChildren<Button>(true).Length,
                "empty ledger board must be read-only");
            Assert.Zero(margin.GetComponentsInChildren<Button>(true).Length,
                "ledger summary must be read-only");

            Invoke(Required(Required(app, "NotebookTray"), "SureThing"));
            yield return WaitForRebuild();
            (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(run);
            Ticket ticket = run.PlaceTicket(picks, stake);
            TvSweatScreen screen = laptop.tv;
            screen.TimeScaleOverride = 0.0001f;
            screen.ForceSeated(true);
            laptop.director.LockRound();
            Assert.AreEqual(Phase.Sweat, run.Phase);
            yield return WaitUntil(() => run.Phase != Phase.Sweat, 60f,
                "truthful current-run ticket never settled");
            Assert.AreNotEqual(TicketState.Open, ticket.State);
            yield return WaitForRebuild();

            yield return OpenLedgerThroughTray(laptop);
            app = App(laptop);
            AssertLedgerShell(app);
            board = Required(app, "LedgerBoard");
            margin = Required(app, "LedgerMargin");
            Assert.IsNull(Find(board, "LedgerEmpty"));
            Transform ledgerTicket = Required(board, "LedgerTicket0");
            string identity = string.IsNullOrEmpty(ticket.Id) ? $"{run.Round}.1" : ticket.Id;
            Assert.AreEqual("TICKET " + identity,
                TextOf(Required(ledgerTicket, "TicketIdentity")));
            Assert.AreEqual(TicketStateText(ticket),
                TextOf(Required(ledgerTicket, "TicketState")));
            // S32: LedgerEntry.jsx's STAKE/RETURNED cells are a key line over a value line, not
            // one "LABEL $n" string — updated from the pre-S32 single-line "STAKE $n"/"PAYOUT $n"
            // pins to match. "RETURNED" also retires "PAYOUT" as the label word, per the ruling's
            // own wording ("the returned figure").
            Assert.AreEqual("STAKE", TextOf(Required(ledgerTicket, "TicketStakeKey")));
            Assert.AreEqual(Money(ticket.Stake), TextOf(Required(ledgerTicket, "TicketStakeValue")));
            Assert.AreEqual("RETURNED", TextOf(Required(ledgerTicket, "TicketReturnedKey")));
            Assert.AreEqual(PayoutText(ticket), TextOf(Required(ledgerTicket, "TicketReturnedValue")));
            Assert.Zero(ledgerTicket.GetComponentsInChildren<Button>(true).Length,
                "settled ledger ticket must expose no action");

            for (int legIndex = 0; legIndex < ticket.Legs.Count; legIndex++)
            {
                Leg leg = ticket.Legs[legIndex];
                Transform ledgerLeg = Required(ledgerTicket, "LedgerLeg" + legIndex);
                // MERGE (markets-2 × main, 2026-08-05): main's assertion supersedes mine and both
                // sides wanted the same thing. Mine asserted the composed label but rebuilt the
                // string by hand, so it did not include FitLabelKeepingSuffix — which is precisely
                // the captured-string-vs-production-formula defect I filed this morning as a flake
                // signature. Main's version routes the whole thing, fit included, through the
                // production formula, so it cannot drift with font-atlas state. Taking main's, and
                // the S22 intent it carries is unchanged: the ledger composes from
                // MatchModel.Fields via CompactLegLabel, never the legacy packed DisplayLabel.
                Text legIdentityText = Required(ledgerLeg, "LegIdentity").GetComponent<Text>();
                Assert.IsNotNull(legIdentityText, "LegIdentity has no Text to measure against");
                Assert.IsNotNull(legIdentityText.font,
                    "LegIdentity has no font; the production face failed to load");
                // F7 (was: raw leg.DisplayLabel, which repeats the picked team a second time —
                // that literal was the bug, not this assertion's job to preserve). The ledger leg
                // label now routes through the same CompactLegLabel/FitLabelKeepingSuffix formula
                // as BuildSlip and BuildStagedReceipt, so it is asserted against that same
                // production formula here — same convention SureThingEntryTests already uses for
                // BuildStagedReceipt's TicketLeg rows — rather than a hand-kept literal that could
                // quietly drift out of sync.
                Assert.AreEqual(
                    LaptopUi.FitLabelKeepingSuffix(legIdentityText.font, $"{legIndex + 1}. ",
                        SportsbookApp.CompactLegLabel(leg.Matchup, leg.Selection),
                        $"  {OddsFormat.American(leg.OfferedOdds)}", 13, 470f),
                    legIdentityText.text);
                Assert.AreEqual(LegStateText(leg),
                    TextOf(Required(ledgerLeg, "LegState")));
                Assert.Zero(ledgerLeg.GetComponentsInChildren<Button>(true).Length,
                    $"settled ledger leg {legIndex} must expose no action");
            }
            // S33: the passive margin's first MarginRow — TICKETS SETTLED, in the kit's own order
            // and wording (app.jsx:95) — replaces the pre-S33 single "SETTLED  N" line this used
            // to pin; label and value are now separate nodes, matching MarginRow.jsx's own split.
            Assert.AreEqual("TICKETS SETTLED",
                TextOf(Required(margin, "RecordRowSettledLabel")));
            Assert.AreEqual(run.Tickets.Count.ToString(CultureInfo.InvariantCulture),
                TextOf(Required(margin, "RecordRowSettledValue")));
        }

        [UnityTest, Order(2)]
        public IEnumerator Ledger_fixed_regions_meet_product_floors_and_describe_only_current_run_records()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            // S31 needs a live Run reference again: the masthead's run figures (BANK/TARGET/
            // TICKETS) and its ROUND-number scope line are both asserted below against the same
            // production formula SportsbookApp.BuildRunFigures/BuildChrome use.
            Run run = laptop.director.Run;
            yield return OpenLedgerThroughTray(laptop);

            Transform app = App(laptop);
            AssertLedgerShell(app);
            Transform chrome = Required(app, "Chrome");
            Transform board = Required(app, "LedgerBoard");
            Transform margin = Required(app, "LedgerMargin");
            Transform tray = Required(app, "NotebookTray");
            Transform summary = Required(margin, "RecordSummary");
            AssertRect(summary as RectTransform, 324f, 530f, "record summary");

            // S31: the masthead's run figures are unchanged from the rest of the surface — reused
            // verbatim from SportsbookApp.BuildRunFigures rather than a parallel condensed string.
            Transform masthead = Required(chrome, "FormMasthead");
            Assert.AreEqual(
                $"BANK {Money(run.Bank)}    TARGET {Money(run.CurrentPayment)}    TICKETS {run.Tickets.Count}/{run.Config.MaxTicketsPerRound}",
                TextOf(Required(masthead, "Figures")));
            Assert.AreEqual($"ROUND {run.Round} OF {run.Config.Rounds}  ·  SETTLED TICKETS ONLY",
                TextOf(Required(masthead, "Scope")));

            // S31: LedgerScreen()'s own 44px board header replaces the old "LedgerScope" caption —
            // same fact (the list below is scoped to settled current-run records), now stated
            // once, in the kit's own words, by the header this ruling mandates.
            Assert.AreEqual("SETTLED TICKETS · THIS RUN",
                TextOf(Required(board, "LedgerBoardHeaderScope")));
            Assert.AreEqual("0 RECORDS",
                TextOf(Required(board, "LedgerBoardHeaderCount")));
            // S33: the passive margin's biro MarginHeader + exactly three MarginRows + one note
            // (app.jsx:94-97) replaces the pre-S33 seven-block panel. The note keeps its pre-S33
            // wording — S35(a) removed the leaked RUN.TICKETS property path from this slot, and
            // this is the kit's own single note (app.jsx:97), not the board header's wording
            // (S31's trap: asserting the header's words here would pin the duplicate S37 forbids).
            Assert.AreEqual("READ-ONLY. THE LEDGER COPIES SETTLED TICKETS AND DERIVES NOTHING.",
                TextOf(Required(summary, "RecordNote")));
            // S33 caps the passive margin at exactly three MarginRows and one note — the separate
            // CashOutDisclosure paragraph this used to pin is retired along with the other four
            // content blocks S33 replaces, not renamed.
            Assert.AreEqual("TICKETS SETTLED", TextOf(Required(summary, "RecordRowSettledLabel")));
            Assert.AreEqual("0", TextOf(Required(summary, "RecordRowSettledValue")));
            // S37: the live round number appears exactly once on the surface, in the masthead's
            // Scope line asserted above. The margin's former "RoundIdentity" — a second,
            // standalone restatement of the same figure — stays gone.
            Assert.IsNull(Find(summary, "RoundIdentity"),
                "S37: the round number belongs to the masthead alone, not this margin too");

            AssertProductFloors(chrome, board, margin, tray);
            AssertChildrenContained(chrome);
            AssertChildrenContained(board);
            AssertChildrenContained(margin);
            AssertChildrenContained(tray);

            Assert.Zero(board.GetComponentsInChildren<Button>(true).Length);
            Assert.Zero(margin.GetComponentsInChildren<Button>(true).Length);
            Assert.IsNull(Find(board, "MirrorMarket"), "ledger must not borrow live mirror styling");
            Assert.IsNull(Find(board, "MirrorTicket0"), "ledger must not expose a live mirror ticket");
            Assert.IsNull(Find(board, "GreenRing"), "ledger records use literal terminal state, not live ink");
            Assert.IsNull(Find(board, "DeadStrike"), "ledger records use literal terminal state, not live ink");
            Assert.IsNull(Find(board, "LedgerTicket0"),
                "no settled current-run record means no invented history row");
        }

        private static IEnumerator OpenLedgerThroughTray(LaptopScreen laptop)
        {
            Transform tray = Required(App(laptop), "NotebookTray");
            Invoke(Required(tray, "Ledger"));
            yield return WaitForRebuild();
            Assert.IsNotNull(Required(App(laptop), "LedgerBoard"),
                "real tray navigation did not open LEDGER");
        }

        private static string TicketStateText(Ticket ticket)
            => ticket.State == TicketState.Won ? "WON"
                : ticket.State == TicketState.Lost ? "LOST"
                : ticket.State == TicketState.CashedOut ? "CASHED OUT" : "OPEN";

        private static string PayoutText(Ticket ticket)
            // S36: the engine retains no cash-out amount; the money column prints an honest
            // absence (an em dash), never a fabricated $0 and never "AMOUNT NOT RETAINED".
            => ticket.State == TicketState.Won ? Money(ticket.PotentialPayout)
                : ticket.State == TicketState.Lost ? Money(0)
                : "—";

        private static string LegStateText(Leg leg)
            => leg.IsVoided ? "VOID"
                : leg.RescuedWon || leg.State == LegState.Won ? "WON"
                : leg.State == LegState.Lost ? "LOST" : "PENDING";

        private static void AssertLedgerShell(Transform app)
        {
            AssertRect(Required(app, "Chrome") as RectTransform, 1024f, 140f, "ledger chrome");
            AssertRect(Required(app, "NotebookRail") as RectTransform, 1024f, 34f, "ledger rail");
            AssertRect(Required(app, "FormTabs") as RectTransform, 1024f, 38f, "ledger tabs");
            AssertRect(Required(app, "FormMasthead") as RectTransform, 1024f, 68f,
                "ledger masthead");
            AssertRect(Required(app, "LedgerBoard") as RectTransform, 700f, 530f, "ledger board");
            AssertRect(Required(app, "LedgerMargin") as RectTransform, 324f, 530f, "ledger margin");
            AssertRect(Required(app, "NotebookTray") as RectTransform, 1024f, 34f, "ledger tray");
        }

        private static void AssertProductFloors(params Transform[] roots)
        {
            foreach (Transform root in roots)
            {
                foreach (Text text in root.GetComponentsInChildren<Text>(true))
                    Assert.GreaterOrEqual(text.fontSize, 13,
                        $"{root.name}/{text.name}: product text must be at least 13px");
                foreach (Button button in root.GetComponentsInChildren<Button>(true))
                {
                    RectTransform rect = button.GetComponent<RectTransform>();
                    Assert.IsNotNull(rect, $"{root.name}/{button.name}: target RectTransform missing");
                    Assert.GreaterOrEqual(rect.sizeDelta.x, 44f,
                        $"{root.name}/{button.name}: target width");
                    Assert.GreaterOrEqual(rect.sizeDelta.y, 32f,
                        $"{root.name}/{button.name}: target height");
                }
            }
        }

        private static void AssertChildrenContained(Transform region)
        {
            RectTransform regionRect = region as RectTransform;
            Assert.IsNotNull(regionRect, $"{region.name}: fixed region RectTransform missing");
            Rect boundsRect = regionRect.rect;
            foreach (RectTransform child in region.GetComponentsInChildren<RectTransform>(true))
            {
                if (ReferenceEquals(child, regionRect)) continue;
                Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(regionRect, child);
                const float tolerance = 1f;
                Assert.GreaterOrEqual(bounds.min.x, boundsRect.xMin - tolerance,
                    $"{region.name}/{child.name}: left overflow");
                Assert.LessOrEqual(bounds.max.x, boundsRect.xMax + tolerance,
                    $"{region.name}/{child.name}: right overflow");
                Assert.GreaterOrEqual(bounds.min.y, boundsRect.yMin - tolerance,
                    $"{region.name}/{child.name}: bottom overflow");
                Assert.LessOrEqual(bounds.max.y, boundsRect.yMax + tolerance,
                    $"{region.name}/{child.name}: top overflow");
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

        private static IEnumerator WaitForRebuild()
        {
            yield return null;
            yield return null;
        }

        private static IEnumerator WaitUntil(Func<bool> condition, float seconds, string failure)
        {
            float start = Time.realtimeSinceStartup;
            while (!condition())
            {
                if (Time.realtimeSinceStartup - start > seconds)
                {
                    Assert.Fail($"{failure} (waited {seconds:0.#}s)");
                    yield break;
                }
                yield return null;
            }
        }

        private static LaptopScreen Laptop()
        {
            LaptopScreen laptop = UnityEngine.Object.FindAnyObjectByType<LaptopScreen>();
            Assert.IsNotNull(laptop, "LaptopScreen missing");
            Assert.IsNotNull(laptop.tv, "Laptop TV reference missing");
            return laptop;
        }

        private static Transform App(LaptopScreen laptop) => Required(laptop.transform, "App");

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
    }
}
