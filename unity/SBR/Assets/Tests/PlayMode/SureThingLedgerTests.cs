using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace SBR.Tests.PlayMode
{
    public class SureThingLedgerTests
    {
        /// <summary>The void arm's contract (F_0.6.0 step 5). The ledger's terminal word used to be
        /// an inline ternary whose final `else` returned "OPEN", so <c>TicketState.Voided</c> — a
        /// settled, refunded ticket — printed as though it were still live. It reached this list;
        /// the ledger collects on <c>State != Open</c>.
        ///
        /// <para>Driven over <c>Enum.GetValues</c> rather than spot-checking VOID, because the defect
        /// was never about VOID specifically: it was a fallthrough that would swallow any state added
        /// after the branch was written. A sixth member gets the same failure this one did.</para></summary>
        [Test, Order(0)]
        public void Ledger_terminal_word_names_every_settled_state_and_only_Open_says_OPEN()
        {
            foreach (TicketState state in Enum.GetValues(typeof(TicketState)))
            {
                string word = OldSlipsApp.LedgerTicketStateWord(state);
                Assert.IsNotEmpty(word, $"{state} must have a word");
                if (state != TicketState.Open)
                    Assert.AreNotEqual("OPEN", word,
                        $"{state} is a SETTLED state and must not print OPEN — that is the "
                        + "fallthrough the void arm closed, and it reads as still-live to the player");
            }

            Assert.AreEqual("VOID", OldSlipsApp.LedgerTicketStateWord(TicketState.Voided),
                "C47: a market that returns the stake is a VOID, and LegStateWord already prints "
                + "exactly that for a voided leg — a ticket takes its legs' vocabulary");
            Assert.AreEqual("OPEN", OldSlipsApp.LedgerTicketStateWord(TicketState.Open));
            Assert.AreEqual("WON", OldSlipsApp.LedgerTicketStateWord(TicketState.Won));
            Assert.AreEqual("LOST", OldSlipsApp.LedgerTicketStateWord(TicketState.Lost));
            Assert.AreEqual("CASHED OUT", OldSlipsApp.LedgerTicketStateWord(TicketState.CashedOut));

            // S23's separation, extended to this function: RIDING is the TV mirror's word for a live
            // ticket and this list holds none, so it must never appear here whatever the state.
            foreach (TicketState state in Enum.GetValues(typeof(TicketState)))
                Assert.AreNotEqual("RIDING", OldSlipsApp.LedgerTicketStateWord(state),
                    $"the settled ledger must never say RIDING (checked for {state})");
        }

        /// <summary>S76's binding negatives (DD batch 67, approved by Allen). VOID is a third
        /// TERMINAL STATE, not a third result — so it must not borrow DEAD's treatment in either
        /// channel, and it must not borrow WON's either.
        ///
        /// <para>Asserted against predicates rather than a rendered row because a Voided ticket
        /// cannot be constructed from this assembly; the predicates are what the row reads, so the
        /// rule has something to fail against either way.</para></summary>
        [Test, Order(0)]
        public void Void_takes_neither_the_oxide_strike_nor_DEADs_ink_nor_WONs_wax()
        {
            Assert.IsFalse(OldSlipsApp.LedgerShowsDeadStrike(TicketState.Voided),
                "S76: never the oxide strike. The strike is what DEAD means on this row (S15 put the "
                + "oxide in the strike alone) and a void is not a loss");
            Assert.IsTrue(OldSlipsApp.LedgerShowsDeadStrike(TicketState.Lost),
                "the strike must still mark the state it belongs to, or the negative above is vacuous");
            foreach (TicketState state in Enum.GetValues(typeof(TicketState)))
                if (state != TicketState.Lost)
                    Assert.IsFalse(OldSlipsApp.LedgerShowsDeadStrike(state),
                        $"only LOST wears the strike (checked for {state})");

            Assert.AreEqual(LaptopOs.TonerSecondary, OldSlipsApp.LedgerTicketStateInk(TicketState.Voided),
                "S76: VOID is --toner-2, the weight of a fact that is neither a win nor a loss");
            Assert.AreNotEqual(LaptopOs.Muted, OldSlipsApp.LedgerTicketStateInk(TicketState.Voided),
                "S76: never drained to DEAD's tone — a void is a terminal state, not a losing one");
            Assert.AreNotEqual(LaptopOs.MoneyGold, OldSlipsApp.LedgerTicketStateInk(TicketState.Voided),
                "and never wax: wax is money the player CAME AWAY WITH, and a refund is being made "
                + "whole rather than coming out ahead");

            // The alpha channel carries the other half of "never drained to DEAD's .55" — LaptopOs.Dim
            // is that value, and nothing on this row may hand it to a void.
            Assert.AreEqual(1f, OldSlipsApp.LedgerTicketStateInk(TicketState.Voided).a, 0.001f,
                "S76: a VOID is not dimmed — LaptopOs.Dim's .55 is DEAD's, and it stays DEAD's");
        }

        // WHAT THIS GATE CANNOT SEE (T53): the RETURNED cell and the RETURNED total for a voided
        // ticket. Both were changed with the word — the cell prints the stake instead of S41's em
        // dash, and the total adds that stake, which it previously omitted while still counting the
        // stake in STAKE, so a refunded ticket read on the totals row as one the player had lost.
        // Neither is asserted here because a Voided ticket cannot be constructed from this assembly:
        // `Ticket.State` is `internal set`, and the only path that reaches Voided is a same-match
        // ticket whose survivors re-price at or below evens — which needs the leg-addressed slip that
        // arrives with sgp's model. Covered when that lands, not claimed before it.

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
            // S62: the ledger prints R{round} · TICKET {nn}, one-indexed and zero-padded, never the
            // engine's zero-indexed RNG key. Asserted through the production formatter, and pinned
            // below against the literal so this cannot silently agree with a regression.
            string identity = LaptopUi.TicketIdentity(ticket.Id, run.Round, 0, withRound: true);
            Assert.AreEqual($"R{run.Round}  ·  TICKET 01", identity,
                "the first ticket of a round is TICKET 01, not 1.0");
            Assert.AreEqual(identity, TextOf(Required(ledgerTicket, "TicketIdentity")));
            Assert.AreEqual(TicketStateText(ticket),
                TextOf(Required(ledgerTicket, "TicketState")));
            // S38+S39 (DD 2026-08-02 batch 7, one change): STAKE/RETURNED are no longer a key line
            // over a value line inside every row — the keys moved out to the board header once
            // (asserted below, board/"LedgerBoardHeaderStake"+"LedgerBoardHeaderReturned"), so each
            // row now carries only the condensed tabular figure, on the same baseline as identity
            // and the terminal word. Updated from the pre-S38 "TicketStakeKey"/"TicketReturnedKey"
            // pins, which this build no longer creates at all (asserted gone, just below).
            Assert.IsNull(Find(ledgerTicket, "TicketStakeKey"),
                "S38: the per-row STAKE key is gone — it lives once in the board header now");
            Assert.IsNull(Find(ledgerTicket, "TicketReturnedKey"),
                "S38: the per-row RETURNED key is gone — it lives once in the board header now");
            Assert.AreEqual(Money(ticket.Stake), TextOf(Required(ledgerTicket, "TicketStakeValue")));
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
                TMP_Text legIdentityText = Required(ledgerLeg, "LegIdentity").GetComponent<TMP_Text>();
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
            // S37's live instance (DD 2026-08-02 batch 7): the subline used to restate the board
            // header's own scope ("· SETTLED TICKETS ONLY") in the masthead's slot. Deleted —
            // updated from the pre-batch-7 pin, which included that clause.
            Assert.AreEqual($"ROUND {run.Round} OF {run.Config.Rounds}",
                TextOf(Required(masthead, "Scope")));

            // S31: LedgerScreen()'s own 44px board header replaces the old "LedgerScope" caption —
            // same fact (the list below is scoped to settled current-run records), now stated
            // once, in the kit's own words, by the header this ruling mandates.
            Assert.AreEqual("SETTLED TICKETS · THIS RUN",
                TextOf(Required(board, "LedgerBoardHeaderScope")));
            // S38: STAKE/RETURNED print once, here, instead of once per record — added by batch 7,
            // aligned (same x/width) with each record's own TicketStakeValue/TicketReturnedValue.
            Assert.AreEqual("STAKE", TextOf(Required(board, "LedgerBoardHeaderStake")));
            Assert.AreEqual("RETURNED", TextOf(Required(board, "LedgerBoardHeaderReturned")));
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

        [Test]
        public void Pending_leg_state_exists_by_construction_and_the_render_path_resolves_it()
        {
            // S43 (DD 2026-08-02 batch 7): a PENDING leg is legal only inside a CASHED OUT ticket
            // — he left before the match ended — and must print the literal word "PENDING", never
            // a fabricated terminal word. LegState.Pending is a real, constructible data state
            // (Leg.State reads Matchup.StatLine directly: null means Pending) even though nothing
            // in this engine currently lets a SETTLED ticket's leg keep it (see the guard test
            // below and OldSlipsApp.LegStateWord's own doc comment). The DD ruled the render path
            // must still resolve it correctly rather than treat the branch as dead code.
            var matchup = new Matchup(0, new Team("HOME", 0, 0), new Team("AWAY", 0, 0), 0.5, 2.0, 2.0);
            var leg = new Leg(matchup, Side.Home, 2.0);
            Assert.AreEqual(LegState.Pending, leg.State,
                "an unsampled matchup (no StatLine) leaves a fresh leg Pending by construction");
            Assert.IsFalse(leg.IsVoided);
            Assert.IsFalse(leg.RescuedWon);
            Assert.AreEqual("PENDING", OldSlipsApp.LegStateWord(leg),
                "S43: the render mapping must resolve a Pending leg to its literal word");
        }

        [Test]
        public void Won_and_lost_tickets_never_carry_a_pending_leg()
        {
            // S43: PENDING is legal ONLY inside a CASHED OUT ticket. This is the regression guard
            // for the illegal side — a WON or LOST ticket's legs are all terminal by construction
            // (Run.LockRound samples every matchup's StatLine, bet or not, before a single
            // SweatSession exists, so no ticket this engine settles straight to Won/Lost can carry
            // a Pending leg). If this ever fails, the render path is about to show an illegal
            // PENDING leg on a settled, non-cashed-out ticket.
            var run = new Run("S43-GUARD", new RunConfig());
            Ticket ticket = run.PlaceTicket(new[] { new Pick(0, Side.Home) }, 10);
            run.LockRound();
            run.FastForwardRound();

            Assert.IsTrue(ticket.State == TicketState.Won || ticket.State == TicketState.Lost,
                "fixture assumption: an un-cashed-out ticket settles Won or Lost");
            foreach (Leg leg in ticket.Legs)
                Assert.AreNotEqual("PENDING", OldSlipsApp.LegStateWord(leg),
                    $"S43: a {ticket.State} ticket must never carry a PENDING leg");
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
            // S41: S36's designed absence expired when engine retention landed. A cashed-out
            // ticket's retained figure PRINTS; the em dash is left only for a record whose amount
            // is genuinely unknowable, which is an absence rather than a missing feature.
            => ticket.State == TicketState.Won ? Money(ticket.PotentialPayout)
                : ticket.State == TicketState.Lost ? Money(0)
                : ticket.State == TicketState.CashedOut && ticket.CashedOutFor.HasValue
                    ? Money(ticket.CashedOutFor.Value)
                    : "—";

        // S43: was a hand-kept copy of the render mapping (VOID/WON/LOST/PENDING) that could drift
        // out of sync with production the same way F7's comment above worries about for leg
        // labels. OldSlipsApp.LegStateWord was factored out of BuildLedgerTicket for exactly this
        // — this now asserts against the production formula itself, not a second copy of it.
        private static string LegStateText(Leg leg) => OldSlipsApp.LegStateWord(leg);

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
                foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
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
            TMP_Text text = node.GetComponent<TMP_Text>();
            if (text == null) text = node.GetComponentInChildren<TMP_Text>();
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
