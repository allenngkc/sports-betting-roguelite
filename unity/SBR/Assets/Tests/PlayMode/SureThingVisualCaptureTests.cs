using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
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
    /// <summary>
    /// Test-only production UGUI reference capture. The fixture deliberately drives the same named
    /// controls and presentation seams as the behavioral PlayMode suite, then renders both a
    /// canvas-aligned reference and the real Main Camera at the laptop's authored focus pose.
    ///
    /// Fourteen states are captured across five UnityTests. The first continues the single-run,
    /// ticket-carrying flow through six states (the original five plus the shared Ledger/Old
    /// Slips screen reached from the tray). The second boots a fresh run to reach REWARDS —
    /// which requires the deterministic zero-ticket lock seam SureThingRewardsTests.EnterShop
    /// uses, and so cannot share the first run's already-placed ticket — and from there also
    /// reaches the same Ledger/Old Slips screen via its other entry point, the desktop icon, and
    /// the shop again with comps to spend so an enabled BUY is actually photographed. It also stops
    /// on the desktop on the way past, which is the only state that shows the machine's wallpaper. The third
    /// runs a real place-lock-sweat cycle purely so the LEDGER can be photographed with a settled
    /// ticket in it — every other capture of that screen shows it empty, which left its entire
    /// settled-record treatment unphotographed and readable only from source. The fourth pushes
    /// three 2-leg tickets through a real place-lock-sweat cycle, cashing one out the instant the
    /// round locks, so the LEDGER can be photographed with several settled rows in as many
    /// different terminal states as the engine will honestly produce — the third fixture's single
    /// row cannot show the record's rhythm across neighbours or the CASHED OUT treatment at all.
    /// The fifth ends a run twice, once each way, to photograph the run-verdict screen — the last
    /// destination on this surface that had never been captured at all, and therefore the last one
    /// whose treatment was readable only from source.
    /// </summary>
    public class SureThingVisualCaptureTests
    {
        private const int FlatWidth = 1024;
        private const int FlatHeight = 704;
        private const int AngledWidth = 1280;
        private const int AngledHeight = 720;
        private const int CaptureLayer = 30;

        // Every capture flow's pinned run seed, named once and referenced everywhere. The scene ships
        // `RunDirector.seed = ""`, which the director reads as "roll a fresh random 8-char A-Z0-9
        // seed" — so until these existed, no two runs of this fixture ever shot the same slate and no
        // two submissions of "the same states" were ever the same frames.
        //
        // **One constant per flow, and nothing restates one.** A seed literal typed at both the
        // StartNewRun and the assertion is two renderings of one value, which is the exact drift S62
        // was ruled against and which this surface is still carrying one unfixed instance of (the MY
        // BETS mirror hand-builds the ticket identity a shared formatter already produces). The rule
        // costs nothing to keep here and the alternative has now cost four items.
        private const string SeedLobby = "52830174";
        private const string SeedMaxLegs = "31468052";
        private const string SeedShop = "70925314";
        private const string SeedLedgerOne = "48137690";
        private const string SeedLedgerMulti = "26580943";
        private const string SeedVerdictWon = "40719355";
        private const string SeedVerdictLost = "68204137";

        // The one seed on this surface that is NOT numeric, and it is exempt by ruling rather than by
        // oversight. Batch 14 accepted the recommendation to leave it: the seed renders on the verdict
        // screen and nowhere else, and leg count is a function of the seed — so re-seeding this flow
        // would re-roll the content of an already-granted frame at no visible benefit. The DD named
        // the asymmetry rather than hiding it, and so does this constant.
        //
        // **It is still a pin, and C34 is about pinning, not about spelling.** This flow was always
        // reproducible; what it lacked was the assert.
        private const string SeedLedgerAcrossRounds = "ledger-across-rounds";

        [UnityTest]
        public IEnumerator Capture_six_truthful_surething_states_as_flat_and_angled_pngs()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();

            // R38 applied to the flow rather than to one state, and it turns out to matter more here
            // than it did on the verdict. The scene ships `RunDirector.seed = ""`, which the director
            // reads as "roll a fresh random 8-char A-Z0-9 seed" — so **this set has never been
            // reproducible.** Every re-shoot deals a different slate, and two submissions of "the same
            // twelve states" have never once been the same frames.
            //
            // Measured, not assumed: batch 11's `05-my-bets-green-dead` reads
            // `Tulsa Plumbers v Pawtucket Ferrets  −516 · PAYS $71`; the very next run of the same
            // state reads `Sheboygan Bricklayers v Waterloo Zambonis  −410 · PAYS $85`.
            //
            // This has not been producing wrong verdicts — treatment is what gets ruled and treatment
            // is stable across seeds. What it produces is a set where a CONTENT-dependent finding can
            // appear and vanish between runs with nothing changed. The recorded instance is the latent
            // width flake in SureThingEntryTests, which passed on every run whose generated team names
            // happened to be short enough and failed on the first long one.
            //
            // Pinned numeric per R38's own rule: 8 digits, an ordinary member of NewSeed's A-Z0-9
            // space. Nothing is lost by pinning, because nothing was stable to lose.
            yield return PinRun(laptop, SeedLobby);

            string outputDirectory = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "..", "..", "artifacts", "surething-ui"));
            Directory.CreateDirectory(outputDirectory);
            string runPrefix = DateTime.UtcNow.ToString(
                "yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture);
            var capturedPaths = new List<string>();

            Assert.AreEqual(SportsbookApp.Tab.Lobby, laptop.Os.CurrentTab);
            Assert.IsNotNull(Required(App(laptop), "Board"));
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "01-form-lobby", capturedPaths);

            Invoke(Required(Required(App(laptop), "Matchup0"), "Details"));
            yield return WaitForRebuild();
            Assert.AreEqual(SportsbookApp.Tab.Detail, laptop.Os.CurrentTab);
            Button entryOffer = FirstNamedButton(
                Required(App(laptop), "MarketBody"), "Market");
            Invoke(entryOffer.transform);
            yield return WaitForRebuild();
            Assert.AreEqual(1, laptop.Slip.Picks.Count);
            Image wideRing = Required(
                Required(App(laptop), "MarketBody"), "WideBiroRing").GetComponent<Image>();
            Assert.IsNotNull(wideRing);
            Assert.IsNotNull(wideRing.sprite);
            StringAssert.StartsWith("ring-wide-", wideRing.sprite.name);

            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "02-entry-selected-wide-ring", capturedPaths);

            // C17: no rebuild verdict on a state no capture shows. This shot exists because the
            // scrolling branch and S27's rail once had no photograph at all — every captured ENTRY
            // state was GOALS, which fitted its body and so correctly rendered no rail.
            //
            // The surfaces build (spec-market-surfaces-2026-08-17) changed the premise but not the
            // point: the body is now 378px with a folio band under it, and MEASURED, every one of
            // the six destinations overflows it except an EMPTY CorrectScore. PLAYERS is still the
            // right subject — it is the deepest list on the sheet (17–24 rows measured) — but it is
            // no longer the only overflowing one, so this is now a rail-is-present shot rather than
            // the only place the rail can be seen.
            Invoke(Required(Required(App(laptop), "MarketDestinations"), "DetailTabPlayers"));
            yield return WaitForRebuild();
            Assert.IsNotNull(Required(App(laptop), "PositionRailTrack"),
                "PLAYERS overflows its body, so S27's rail must be present");
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "02b-entry-players-scrolling-rail", capturedPaths);

            Invoke(Required(App(laptop), "BackToForm"));
            yield return WaitForRebuild();
            Assert.AreEqual(SportsbookApp.Tab.Lobby, laptop.Os.CurrentTab);
            Invoke(Required(Required(App(laptop), "Matchup1"), "AwayOdds"));
            yield return WaitForRebuild();
            Assert.AreEqual(2, laptop.Slip.Picks.Count);
            Invoke(Required(Required(App(laptop), "WorkingMargin"), "Place"));
            yield return WaitForRebuild();

            Transform margin = Required(App(laptop), "WorkingMargin");
            Assert.AreEqual(1, laptop.director.Run.Tickets.Count);
            Assert.AreEqual(0, laptop.Slip.Picks.Count);
            // E-07 moved staged receipts out of the margin into the 700px sheet, so this looks
            // beneath the app root rather than the margin (screens.jsx:50-57).
            Assert.IsNotNull(Required(
                Required(App(laptop), "StagedTickets"), "StagedTicket0"));
            Assert.IsTrue(Required(margin, "Lock").GetComponent<Button>().interactable);
            Assert.IsNull(Find(margin, "LockReason"));
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "03-staged-receipt-lock-enabled", capturedPaths);

            Invoke(Required(Required(App(laptop), "Matchup2"), "AwayOdds"));
            yield return WaitForRebuild();
            margin = Required(App(laptop), "WorkingMargin");
            Assert.AreEqual(1, laptop.Slip.Picks.Count);
            Assert.IsFalse(Required(margin, "Lock").GetComponent<Button>().interactable);
            Assert.AreEqual("PLACE OR CLEAR THIS WORKING SLIP",
                TextOf(Required(margin, "LockReason")));
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "04-working-mark-lock-disabled", capturedPaths);

            Invoke(Required(margin, "Remove0"));
            yield return WaitForRebuild();
            Assert.AreEqual(0, laptop.Slip.Picks.Count);

            Ticket ticket = laptop.director.Run.Tickets[0];
            Assert.AreEqual(2, ticket.Legs.Count);
            RevealedView view = laptop.tv.RevealedView;
            InvokeView(view, "Reset", laptop.director.Run, ticket, 0);
            yield return WaitForRebuild();

            // PENDING is the leg-level word for "released by the TV, not yet started", and it is the
            // last ruled treatment on this surface with no frame behind it. This is the mirror at the
            // instant the broadcast hands the ticket over: ticket RIDING, both legs PENDING, and the
            // tally already carrying the whole stake because nothing has resolved yet.
            //
            // **Reading order for the three MY BETS states is 04b -> 04a -> 05, which the letters do
            // NOT encode.** They are insertion order: 04a was shot and filed before this state was
            // asked for, and renaming delivered evidence is worse than a suffix that needs one
            // sentence of explanation.
            laptop.Os.OpenSportsbook(SportsbookApp.Tab.MyBets);
            yield return WaitForRebuild();
            Transform pendingTicket = Required(
                Required(App(laptop), "MyBetsBoard"), "MirrorTicket0");
            StringAssert.Contains("RIDING", TextOf(Required(pendingTicket, "TicketTitle")),
                "S23: RIDING is ticket-level and holds from the moment the TV releases the ticket");
            Assert.AreEqual("PENDING",
                TextOf(Required(Required(pendingTicket, "MirrorLeg0"), "LegState")));
            Assert.AreEqual("PENDING",
                TextOf(Required(Required(pendingTicket, "MirrorLeg1"), "LegState")));

            // The tally reads the same $35/$85 it reads on 04a — nothing has resolved, so nothing has
            // left the at-risk column. Asserted so the two frames cannot silently disagree.
            Transform pendingMargin = Required(App(laptop), "MyBetsMargin");
            StringAssert.Contains("1 RIDING", TextOf(Required(pendingMargin, "TallyAtRiskLabel")),
                "both legs pending still means exactly one ticket riding");

            // S65: PENDING is `--toner-3`. This frame is what got it ruled — shot as built while the
            // build rendered `--toner-2`, so the violation had a photograph rather than being fixed
            // out of existence before anyone could confirm it. The DD measured 158,154,138 against
            // the token's 110,107,94 on this exact state and ruled it a violation, so the assertion
            // that was deliberately withheld here now belongs here.
            Assert.IsTrue(SameInk(
                    Required(Required(pendingTicket, "MirrorLeg0"), "LegState").GetComponent<TMP_Text>().color,
                    LaptopOs.Muted),
                "S65: a PENDING leg prints --toner-3, not the --toner-2 it shared with VOID");
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "04b-my-bets-pending", capturedPaths);

            InvokeView(view, "BeginLeg", 0, ticket.Legs[0]);
            InvokeView(view, "ResolveLeg", 0, LegGrade.Won);
            yield return WaitForRebuild();
            InvokeView(view, "BeginLeg", 1, ticket.Legs[1]);
            yield return WaitForRebuild();

            // RIDING is the last unphotographed state on this surface. It is the ticket-level word,
            // and S23 makes that split contractual — RIDING never appears on a leg, LIVE never on a
            // ticket — but the only MY BETS capture in the set shot a ticket whose legs had BOTH
            // already resolved. So the word a ticket wears for its entire life until it settles had
            // never been in a frame, on the one screen whose whole subject is tickets in flight.
            //
            // The same frame settles what the tally has been owed since batch 10. AT RISK and IF
            // EVERYTHING LANDS sum over RIDING tickets only (BuildMirrorMargin) — so on a fully
            // resolved round they correctly print `0 RIDING · $0 · $0`, which is the true answer to
            // "what is still live" and also a photograph of the column doing nothing. Both figures
            // carry real money here, which makes this the first frame to show the ratified
            // AT-RISK-toner / IF-EVERYTHING-LANDS-wax split (owning doc §3.1) on non-zero figures.
            //
            // Shot one reveal BEFORE 05, on the same ticket and the same run: 05 is a granted frame
            // and nothing about it moves. This state is 05 one step earlier, which is also why it
            // takes a letter rather than a number — 02b's precedent — instead of renumbering a set
            // the register, the owning doc and the handoff all cite by name.
            laptop.Os.OpenSportsbook(SportsbookApp.Tab.MyBets);
            yield return WaitForRebuild();
            Transform ridingTicket = Required(
                Required(App(laptop), "MyBetsBoard"), "MirrorTicket0");
            StringAssert.Contains("RIDING", TextOf(Required(ridingTicket, "TicketTitle")),
                "S23: a ticket with an unresolved leg is RIDING");
            Assert.AreEqual("GREEN",
                TextOf(Required(Required(ridingTicket, "MirrorLeg0"), "LegState")));
            Assert.AreEqual("LIVE",
                TextOf(Required(Required(ridingTicket, "MirrorLeg1"), "LegState")));

            // Asserted before the shot, because a tally reading $0 would make this frame a second
            // photograph of the nothing 05 already shows.
            Transform ridingMargin = Required(App(laptop), "MyBetsMargin");
            StringAssert.Contains("1 RIDING", TextOf(Required(ridingMargin, "TallyAtRiskLabel")),
                "the tally counts the riding ticket");
            Assert.AreNotEqual(LaptopUi.Money(0d),
                TextOf(Required(ridingMargin, "TallyAtRiskValue")),
                "this state exists to show AT RISK carrying real money");
            Assert.AreNotEqual(LaptopUi.Money(0d),
                TextOf(Required(ridingMargin, "TallyIfAllLandValue")),
                "this state exists to show IF EVERYTHING LANDS carrying real money");
            Assert.IsTrue(SameInk(
                    Required(ridingMargin, "TallyIfAllLandValue").GetComponent<TMP_Text>().color,
                    LaptopOs.MoneyGold),
                "§3.1: stake reads toner and payout reads wax — the payout side is wax");
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "04a-my-bets-riding", capturedPaths);

            InvokeView(view, "ResolveLeg", 1, LegGrade.Lost);
            yield return WaitForRebuild();

            laptop.Os.OpenSportsbook(SportsbookApp.Tab.MyBets);
            yield return WaitForRebuild();
            Transform mirrorTicket = Required(
                Required(App(laptop), "MyBetsBoard"), "MirrorTicket0");
            Transform mirrorLeg0 = Required(mirrorTicket, "MirrorLeg0");
            Transform mirrorLeg1 = Required(mirrorTicket, "MirrorLeg1");
            Assert.AreEqual("GREEN", TextOf(Required(mirrorLeg0, "LegState")));
            Assert.AreEqual("DEAD", TextOf(Required(mirrorLeg1, "LegState")));
            Assert.IsNotNull(Required(mirrorLeg0, "GreenRing").GetComponent<Image>());
            Assert.IsNotNull(Required(mirrorLeg1, "DeadStrike").GetComponent<Image>());
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "05-my-bets-green-dead", capturedPaths);

            // The rail and tray are now built once by NotebookChrome and shared between the
            // sportsbook and the ledger screen (LaptopOs.OpenLedger routes to the same
            // App.OldSlips state the desktop's "Old Slips" icon does — OldSlipsApp.Render is
            // the one screen both entry points share). Reach it exactly the way
            // SureThingLedgerTests.OpenLedgerThroughTray does: through the tray, not the API.
            Invoke(Required(Required(App(laptop), "NotebookTray"), "Ledger"));
            yield return WaitForRebuild();
            Assert.IsNotNull(Required(App(laptop), "LedgerBoard"),
                "real tray navigation did not open LEDGER");
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "06-ledger", capturedPaths);

            AssertCaptureOutput(capturedPaths, 18);
        }

        /// <summary>C17: no rebuild verdict on a state no capture shows. B1 rebuilt the margin's
        /// leg row to the kit's two-line MarginLeg, and the state that decides whether that rebuild
        /// is correct is a FULL slip — which no capture showed. Allen capped MaxLegs at 4
        /// (2026-08-02) to close the overflow; this photographs the cap actually holding. Fills to
        /// run.Config.MaxLegs rather than a literal 4, so raising the dial changes what is captured
        /// instead of quietly capturing a state that is no longer the maximum. Its own Boot(),
        /// because the other two capture flows each depend on their starting ticket count.</summary>
        [UnityTest]
        public IEnumerator Capture_the_working_margin_at_the_legal_maximum_leg_count()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            // This flow's frame is the one S51 is still owed — the margin's reservation overrun with
            // a full slip standing on a staged receipt. An overrun measured in pixels against a slate
            // that was different on every run is the least reproducible measurement on the surface,
            // so this is the flow where pinning is worth the most.
            yield return PinRun(laptop, SeedMaxLegs);
            string outputDirectory = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "..", "..", "artifacts", "surething-ui"));
            Directory.CreateDirectory(outputDirectory);
            string runPrefix = DateTime.UtcNow.ToString(
                "yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture);
            var capturedPaths = new List<string>();

            int maxLegs = laptop.director.Run.Config.MaxLegs;
            for (int i = 0; i < maxLegs; i++)
            {
                Invoke(Required(Required(App(laptop), "Matchup" + i), "AwayOdds"));
                yield return WaitForRebuild();
            }

            // S51 owes a frame of THIS state specifically: a full slip standing on top of a staged
            // receipt is where the margin's 2.6px reservation overrun occurs, and no capture showed
            // it — 09-margin-max-legs staged no ticket. The frame is not here to illustrate the
            // number; it is here because the next person hunting the 2.6px owner needs to see the
            // state, and because C17 says the capture settles what a source read only suggests.
            // Placing clears the working slip, so refill to the cap afterwards.
            Invoke(Required(Required(App(laptop), "WorkingMargin"), "Place"));
            yield return WaitForRebuild();
            Assert.Greater(laptop.director.Run.Tickets.Count, 0,
                "a receipt must actually be staged for this to be the overrun state");
            for (int i = 0; i < maxLegs; i++)
            {
                Invoke(Required(Required(App(laptop), "Matchup" + i), "AwayOdds"));
                yield return WaitForRebuild();
            }
            Assert.AreEqual(maxLegs, laptop.Slip.Picks.Count,
                "the captured state must actually be a full slip");

            // ONE STATE, THREE NUMBERS — read this before following any reference to it.
            //   09  the original, and still what the S51 filing (dd-import/markets-26px-residual.md),
            //       the batch-10 register entry and every frame already in evidence/ call it.
            //   11  the first renumber, which cleared a genuine 09/09 collision with
            //       "09-rewards-affordable" — and then collided again: the migration branch carries
            //       "11-desktop", which is register-cited (S47/S56, owning doc §5.2) and cannot move.
            //       That renumber relocated the collision instead of resolving it.
            //   16  here. Free on every branch; 01–15 are all taken and every one of 10, 11, 12 and
            //       13 is cited somewhere, so nothing below this could be shifted to make room.
            //
            // 16 is a free SLOT, not a reading position: this is a betting-surface state and belongs
            // conceptually beside 03/04, not after the verdict screens. Nothing in the harness reads
            // the ordinal as an order, so the cost is a reader's expectation, and that is the cheaper
            // of the two costs available — the other was a set with two 11s in it.
            //
            // If a third collision ever forces this again: the numbers are load-bearing for citation
            // and worthless for sequence, and the fix is to stop encoding order in them, not to keep
            // hunting for free integers.
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "16-margin-max-legs-staged-receipt", capturedPaths);

            AssertCaptureOutput(capturedPaths, 2);
        }

        /// <summary>THE BOARD AT THE CAP, under the additive gesture (DD batch 84).
        ///
        /// <para><b>The state the dead-click treatment has to be authored against.</b> Before the
        /// gesture this could not arise: a second pick on a match REPLACED the first, so the slip
        /// could always take the click. Now a pick STICKS, `MaxLegs` binds, and every unmarked offer
        /// on the board is a click that does nothing — silently.</para>
        ///
        /// <para>The frame is the whole point, so the capture ASSERTS THE DEAD CLICK rather than
        /// assuming it: it fills to the cap, clicks an unmarked offer, and proves the slip did not
        /// move. A frame of a board that could still take a pick would be authored against the wrong
        /// state.</para>
        ///
        /// <para>Two things the ruled general rule says this frame is NOT allowed to be read as
        /// already solving: a refusal knowable before the act must show BEFORE it, and S2 bars
        /// reusing the board-frozen dim to say it. Neither is built here — this seat authors no
        /// treatment.</para></summary>
        [UnityTest]
        public IEnumerator Capture_the_board_at_the_leg_cap_where_every_further_pick_is_a_dead_click()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            yield return PinRun(laptop, SeedMaxLegs);
            Run run = laptop.director.Run;
            int maxLegs = run.Config.MaxLegs;

            for (int i = 0; i < maxLegs; i++)
            {
                Invoke(Required(Required(App(laptop), "Matchup" + i), "AwayOdds"));
                yield return WaitForRebuild();
            }
            Assert.AreEqual(maxLegs, laptop.Slip.Picks.Count, "the board must be at the cap");

            // THE ACT NEVER HAPPENS — which is the ruled general rule, and it is now what this
            // capture asserts. The first version of this test CLICKED a capped cell and proved the
            // slip did not move; under S85's treatment that click is structurally impossible, so
            // the assertion moved to where the rule actually lives: the control has stopped
            // offering BEFORE it is touched.
            Assert.Greater(run.CurrentSlate.Matchups.Count, maxLegs,
                "this frame needs an unpicked matchup left on the board");
            Transform capped = Required(Required(App(laptop), "Matchup" + maxLegs), "AwayOdds");
            Assert.IsFalse(capped.GetComponent<Button>().interactable,
                "the capped cell must have stopped offering before it is touched");
            Assert.AreEqual(0f, capped.GetComponent<Image>().color.a, 0.001f,
                "and it must have lost its field — the offer's own channel");
            Assert.AreEqual(LaptopOs.White,
                Required(capped, "Label").GetComponent<TMP_Text>().color,
                "while the price stays a legible FACT — a dim here would be frozen's meaning");

            // A second market on an already-marked match is inert for the same reason, and that case
            // is new to the gesture: before it, this click replaced rather than added.
            Assert.IsFalse(Required(Required(App(laptop), "Matchup0"), "HomeOdds")
                .GetComponent<Button>().interactable,
                "a second market on a marked match is inert at the cap too");
            Assert.IsTrue(Required(Required(App(laptop), "Matchup0"), "AwayOdds")
                .GetComponent<Button>().interactable,
                "but the MARKED cell stays live — un-picking is the remedy and a remedy is never "
                + "disabled (S73-am4)");

            string outputDirectory = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "..", "..", "artifacts", "surething-ui"));
            Directory.CreateDirectory(outputDirectory);
            string runPrefix = DateTime.UtcNow.ToString(
                "yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture);
            var capturedPaths = new List<string>();
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "18-board-at-the-leg-cap", capturedPaths);

            AssertCaptureOutput(capturedPaths, 2);
        }

        /// <summary>S83's scroll, on the state that ACTUALLY SCROLLS (DD batch 81).
        ///
        /// <para><b>Four legs plus a held consumable.</b> A non-scrolling capture proves nothing
        /// about a scroll, and four legs ALONE does not scroll — measured, it is 168.0 of content
        /// into a 177.9 viewport and clears by 9.9px. The modifiers row is the 34px that puts it
        /// over: 202.0 into 177.9, scrolling by 24.1px. That row is gated on pure RUN state, so the
        /// capture grants the consumable rather than hoping for one.</para>
        ///
        /// <para>Seeded on the same pin as the max-legs frame, so the board underneath is the board
        /// that set is already read against.</para></summary>
        [UnityTest]
        public IEnumerator Capture_the_working_margin_where_the_slip_actually_scrolls()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            yield return PinRun(laptop, SeedMaxLegs);
            Run run = laptop.director.Run;

            ConsumableDefinition freeBet = null;
            foreach (ConsumableDefinition c in RelicCatalog.Consumables)
                if (c.Id == "free_bet") { freeBet = c; break; }
            Assert.IsNotNull(freeBet, "free_bet must exist in the catalog for this state to be built");
            run.GrantConsumable(freeBet);

            int maxLegs = run.Config.MaxLegs;
            for (int i = 0; i < maxLegs; i++)
            {
                Invoke(Required(Required(App(laptop), "Matchup" + i), "AwayOdds"));
                yield return WaitForRebuild();
            }
            Assert.AreEqual(maxLegs, laptop.Slip.Picks.Count, "the captured state must be a full slip");

            // The frame is only worth docking if it IS the scrolling state, so that is asserted
            // rather than assumed — a capture that silently caught the non-scrolling case would be
            // exactly the evidence the DD said proves nothing.
            Transform margin = Required(App(laptop), "WorkingMargin");
            var scroll = Required(margin, "SlipScroll").GetComponent<ScrollRect>();
            Assert.IsTrue(scroll.vertical,
                "this capture exists to show the scroll engaged; it is not engaged in this state");
            Assert.IsNotNull(Find(Required(margin, "SlipScroll"), "RailTrack"),
                "S27's printed rail must be drawn when the body scrolls");

            string outputDirectory = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "..", "..", "artifacts", "surething-ui"));
            Directory.CreateDirectory(outputDirectory);
            string runPrefix = DateTime.UtcNow.ToString(
                "yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture);
            var capturedPaths = new List<string>();
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "17-margin-scrolling-four-legs-consumable", capturedPaths);

            AssertCaptureOutput(capturedPaths, 2);
        }

        [UnityTest]
        public IEnumerator Capture_four_more_truthful_surething_states_as_flat_and_angled_pngs()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            // The flow with the most to lose from a rolling slate: `09-rewards-affordable` exists
            // because a BUY-in-biro Law Two violation survived weeks of review while every capture
            // happened to show the control greyed out. That state asserts an affordable BUY before
            // shooting, so it cannot regress the same way again — but on a rolled seed, WHICH offers
            // were affordable changed every run, and an offer's rule text is what S17 and S26 are
            // about. Pinning makes the shop a fixed slate that a finding can actually be made against.
            yield return PinRun(laptop, SeedShop);
            string outputDirectory = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "..", "..", "artifacts", "surething-ui"));
            Directory.CreateDirectory(outputDirectory);
            string runPrefix = DateTime.UtcNow.ToString(
                "yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture);
            var capturedPaths = new List<string>();

            // REWARDS is gated to Phase.Shop (SportsbookApp.BuildChrome: the REWARDS tab is
            // disabled whenever run.Phase != Phase.Shop). The only deterministic, already-
            // established way there is the zero-ticket lock seam
            // SureThingRewardsTests.EnterShop uses: on a fresh boot Run.Tickets.Count is 0, so
            // LockRound settles the round without any TV ceremony and lands directly in Shop
            // with the book defaulted to REWARDS (LaptopOs.ApplyPhaseDefault). This state must
            // come from a fresh Boot() rather than continuing the first test's run, because
            // that run already carries a placed ticket and would not take the zero-ticket path.
            Assert.AreEqual(0, laptop.director.Run.Tickets.Count,
                "zero-ticket lock is the deterministic shop-entry test seam");
            laptop.director.LockRound();
            yield return WaitForRebuild();
            Assert.AreEqual(Phase.Shop, laptop.director.Run.Phase);
            Assert.AreEqual(SportsbookApp.Tab.Rewards, laptop.Os.CurrentTab,
                "the Shop phase default did not land on REWARDS");
            Assert.IsNotNull(Required(App(laptop), "RewardsBoard"),
                "REWARDS did not render its board");
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "07-rewards", capturedPaths);

            // The same shop with the comps to spend. This state exists because its absence hid a
            // real defect: every earlier rewards capture was taken at zero comps, so every BUY
            // rendered in its disabled grey and a Law Two violation — an affordable BUY drawn in
            // the player's biro rather than in wax — was invisible in every screenshot we had. A
            // control's enabled appearance is not evidence unless something captures it enabled.
            laptop.director.Run.GrantComps(1000);
            laptop.Os.OpenSportsbook(SportsbookApp.Tab.Rewards);
            yield return WaitForRebuild();
            Transform affordableBoard = Required(App(laptop), "RewardsBoard");
            Button firstBuy = FirstNamedButton(affordableBoard, "Buy");
            Assert.IsNotNull(firstBuy, "no BUY control on the rewards board");
            Assert.IsTrue(firstBuy.interactable,
                "1000 comps must make at least one offer affordable, or this state proves nothing");
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "09-rewards-affordable", capturedPaths);

            // Old Slips is LaptopOs's App.OldSlips reached from the desktop icon rather than the
            // in-app tray — the same screen as 06-ledger above (OldSlipsApp.Render), just via
            // its other named entry point (LaptopOs.MakeDesktopIcon("OldSlips", ...)).
            laptop.Os.OpenDesktop();
            yield return WaitForRebuild();
            Assert.IsTrue(laptop.Os.OnDesktop, "OpenDesktop did not leave the sportsbook");

            // The desktop itself, which no capture has ever shown. It carries the machine's
            // wallpaper, and that wallpaper spent this entire project inert: LaptopWallpaperGraphic
            // was constructed without a CanvasRenderer, so UGUI never asked it for geometry and it
            // drew nothing while every test stayed green. It draws now, and nobody has seen it.
            //
            // The assertions below are the point of the state, not ceremony. A capture that cannot
            // fail proves nothing — this is the same lesson as the rewards board, where every frame
            // was taken at zero comps so a BUY-in-biro violation stayed invisible for weeks because
            // the control was always greyed out. So: confirm the wallpaper object exists, that it
            // carries the CanvasRenderer whose absence made it invisible, and that it is actually
            // enabled — then shoot.
            Transform wallpaper = Required(laptop.transform, "Wallpaper");
            var wallpaperGraphic = wallpaper.GetComponent<Graphic>();
            Assert.IsNotNull(wallpaperGraphic, "the wallpaper carries no Graphic to render");
            Assert.IsNotNull(wallpaper.GetComponent<CanvasRenderer>(),
                "the wallpaper has no CanvasRenderer, so it cannot draw and this capture proves nothing");
            Assert.IsTrue(wallpaperGraphic.enabled && wallpaper.gameObject.activeInHierarchy,
                "the wallpaper is disabled or inactive, so this capture proves nothing");
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "11-desktop", capturedPaths);

            Invoke(Required(laptop.transform, "OldSlips"));
            yield return WaitForRebuild();
            Assert.IsFalse(laptop.Os.OnDesktop,
                "the desktop Old Slips icon did not leave the desktop");
            Assert.IsNotNull(Required(App(laptop), "LedgerBoard"),
                "the desktop Old Slips icon did not open the ledger screen");
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "08-old-slips", capturedPaths);

            AssertCaptureOutput(capturedPaths, 8);
        }

        /// <summary>
        /// The ledger with a genuinely settled ticket in it.
        ///
        /// Every other capture of this screen shows it empty, which means the entire settled-record
        /// treatment — the terminal word, the strike, the returned figure, the row's recession —
        /// has never been photographed once. The C14 audit had to read all of it out of source.
        ///
        /// That gap is not hypothetical. A Law Two violation on the rewards BUY control survived
        /// weeks of review on this surface because no capture ever showed an affordable offer, and
        /// every reviewer looked at a screenshot where the control was greyed out. This is the same
        /// shape of blind spot, one screen over.
        ///
        /// It needs its own run rather than riding along with the others: the ledger only shows
        /// tickets the engine has actually settled, so it needs a real place-lock-sweat cycle. The
        /// other fixtures deliberately fake the TV mirror instead, which populates MY BETS and
        /// leaves the ledger empty. Sequence is the one SureThingLedgerTests already proves.
        /// </summary>
        [UnityTest]
        public IEnumerator Capture_the_populated_ledger_so_settled_states_are_photographed()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            // A settled record's whole subject is its figures, and this flow settles a real ticket
            // through a real sweat — so the stake, the returned figure and the terminal word were all
            // re-rolled on every run. The LEDGER's grant was checked on figures (`$8` cashed out,
            // `$29` won, `$0` lost, total `$37`); pinning is what makes a sentence like that mean the
            // same thing the next time anyone looks.
            yield return PinRun(laptop, SeedLedgerOne);
            string outputDirectory = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "..", "..", "artifacts", "surething-ui"));
            Directory.CreateDirectory(outputDirectory);
            string runPrefix = DateTime.UtcNow.ToString(
                "yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture);
            var capturedPaths = new List<string>();

            Run run = laptop.director.Run;
            (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(run);
            Ticket ticket = run.PlaceTicket(picks, stake);

            // Collapse the sweat rather than skip it: the ledger reads engine state, so the ticket
            // has to travel the real path to a terminal state, not be written into one.
            TvSweatScreen screen = laptop.tv;
            screen.TimeScaleOverride = 0.0001f;
            screen.ForceSeated(true);
            laptop.director.LockRound();
            Assert.AreEqual(Phase.Sweat, run.Phase);

            float start = Time.realtimeSinceStartup;
            while (run.Phase == Phase.Sweat)
            {
                if (Time.realtimeSinceStartup - start > 60f)
                {
                    Assert.Fail("the ticket never settled, so there is no populated ledger to shoot");
                    yield break;
                }
                yield return null;
            }
            Assert.AreNotEqual(TicketState.Open, ticket.State,
                "a settled ledger row is the entire point of this capture");
            yield return WaitForRebuild();

            Invoke(Required(Required(App(laptop), "NotebookTray"), "Ledger"));
            yield return WaitForRebuild();
            Transform board = Required(App(laptop), "LedgerBoard");
            Assert.IsNull(Find(board, "LedgerEmpty"),
                "ledger still rendered its empty state after a ticket settled");
            Assert.IsNotNull(Required(board, "LedgerTicket0"));

            // Which terminal state this run produced decides what the capture can actually prove.
            // The LOST treatment (word in toner-3, strike in oxide, returned figure in toner-3)
            // is only verifiable from a capture that contains a lost ticket.
            Debug.Log($"[LedgerCapture] ticket settled as {ticket.State} — "
                + "LOST colour work is only verifiable from this capture if that reads Lost");

            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "10-ledger-populated", capturedPaths);

            AssertCaptureOutput(capturedPaths, 2);
        }

        /// <summary>
        /// The ledger with three settled tickets in as many different terminal states as a single
        /// honest run will produce.
        ///
        /// 10-ledger-populated above proves the settled-record treatment exists, but with exactly
        /// one row in whichever terminal state the engine happened to land on that run. It cannot
        /// show the row's rhythm against a neighbour, and it cannot show CASHED OUT at all — nothing
        /// in that fixture ever reaches the cash-out window. A Design Director cannot rule on the
        /// record row's layout from a single, arbitrary row.
        ///
        /// Three 2-leg tickets are placed on disjoint matchups — (0,1), (2,3), (4,5) — the widest
        /// spread Config.MaxTicketsPerRound (3) allows across the six-matchup slate without doubling
        /// up. CASHED OUT is the one terminal state this fixture can GUARANTEE: the first ticket's
        /// session is cashed out the instant LockRound() returns, before anything yields and before
        /// any leg has resolved — the only moment SweatSession's CashOutAvailable guard (not
        /// DoubleOrNothing, >=2 legs, not complete, no pending dead leg, ticket still Open, no
        /// already-lost leg) is certain to hold, since a single further frame could resolve a leg
        /// Lost and close the window for good. The other two tickets run the real sweat and settle
        /// however the slate decides — WON and LOST are engine truth, never forced here, and if both
        /// land the same way that is the honest result, logged rather than hidden.
        /// </summary>
        [UnityTest]
        public IEnumerator Capture_the_ledger_with_three_settled_tickets_in_distinct_terminal_states()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            // Last of the four. This flow's frame is the one the record row's rhythm across
            // neighbours is read from — three rows in as many terminal states as the engine will
            // honestly produce — and "as many as it will produce" was a function of the rolled seed.
            // A row-to-row composition judged on a set of neighbours that never recurs is the exact
            // thing C11 asks to be made against a frame someone else can look at again.
            yield return PinRun(laptop, SeedLedgerMulti);
            string outputDirectory = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "..", "..", "artifacts", "surething-ui"));
            Directory.CreateDirectory(outputDirectory);
            string runPrefix = DateTime.UtcNow.ToString(
                "yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture);
            var capturedPaths = new List<string>();

            Run run = laptop.director.Run;
            Assert.GreaterOrEqual(run.CurrentSlate.Matchups.Count, 6,
                "three disjoint 2-leg tickets need six matchups on the slate");

            // Three 2-leg tickets on disjoint matchups: every matchup on the six-slot slate is used
            // exactly once, so no ticket can share a matchup with another.
            Ticket ticketA = run.PlaceTicket(
                new[] { new Pick(0, Side.Home), new Pick(1, Side.Home) }, run.Config.MinStake);
            Ticket ticketB = run.PlaceTicket(
                new[] { new Pick(2, Side.Home), new Pick(3, Side.Home) }, run.Config.MinStake);
            Ticket ticketC = run.PlaceTicket(
                new[] { new Pick(4, Side.Home), new Pick(5, Side.Home) }, run.Config.MinStake);
            Assert.AreEqual(3, run.Tickets.Count, "three tickets must be on the board before locking");
            Assert.AreEqual(2, ticketA.Legs.Count, "ticket A must carry two legs to be cash-out eligible");
            Assert.AreEqual(2, ticketB.Legs.Count);
            Assert.AreEqual(2, ticketC.Legs.Count);

            // Collapse the sweat rather than skip it, exactly as the single-ticket fixture above
            // does: the ledger reads engine state, so every ticket has to travel the real path to a
            // terminal state.
            TvSweatScreen screen = laptop.tv;
            screen.TimeScaleOverride = 0.0001f;
            screen.ForceSeated(true);
            laptop.director.LockRound();
            Assert.AreEqual(Phase.Sweat, run.Phase);

            // Cash out ticket A's session in this same instant, before a single yield — before the
            // TV's own coroutine has even picked up director.CurrentSession, and before any drama
            // event has been emitted. Nothing has resolved yet, so CashOutAvailable's guard holds:
            // not Double or Nothing, two legs, session not complete, no pending dead leg, ticket
            // still Open, no already-lost leg. One frame later would not be good enough — a revealed
            // Lost leg on this very ticket would close the window permanently.
            Assert.AreEqual(3, run.Sweats.Count, "one sweat session per ticket, in placement order");
            run.Sweats[0].AcceptCashOut();
            Assert.AreEqual(TicketState.CashedOut, ticketA.State,
                "the cash-out must land the instant the round locks, or this guard can't be trusted");

            float start = Time.realtimeSinceStartup;
            while (run.Phase == Phase.Sweat)
            {
                if (Time.realtimeSinceStartup - start > 60f)
                {
                    Assert.Fail("the round never finished settling, so there is no multi-state ledger to shoot");
                    yield break;
                }
                yield return null;
            }
            yield return WaitForRebuild();

            // Assert before capturing: a capture that cannot fail proves nothing. This fixture has
            // already learned that lesson twice — once on the rewards board, where every frame was
            // taken at zero comps so a Law Two violation on BUY stayed invisible for weeks.
            Assert.AreEqual(3, run.Tickets.Count, "placing three tickets must not have silently dropped one");
            bool anyOpen = false;
            bool anyCashedOut = false;
            foreach (Ticket t in run.Tickets)
            {
                if (t.State == TicketState.Open) anyOpen = true;
                if (t.State == TicketState.CashedOut) anyCashedOut = true;
            }
            Assert.IsFalse(anyOpen, "every ticket must be settled before this capture means anything");
            Assert.IsTrue(anyCashedOut, "the guaranteed cash-out must have survived to settlement");

            // Which terminal states B and C actually landed on decides what this capture can prove
            // about WON and LOST — that is engine truth, read here rather than guessed from pixels.
            Debug.Log($"[LedgerCapture] ticket A (index 0) settled as {ticketA.State} — guaranteed cash-out");
            Debug.Log($"[LedgerCapture] ticket B (index 1) settled as {ticketB.State} — engine truth");
            Debug.Log($"[LedgerCapture] ticket C (index 2) settled as {ticketC.State} — engine truth");

            Invoke(Required(Required(App(laptop), "NotebookTray"), "Ledger"));
            yield return WaitForRebuild();
            Transform board = Required(App(laptop), "LedgerBoard");
            Assert.IsNull(Find(board, "LedgerEmpty"),
                "ledger still rendered its empty state after three tickets settled");
            Assert.IsNotNull(Required(board, "LedgerTicket0"), "ledger did not render a first settled row");
            Assert.IsNotNull(Required(board, "LedgerTicket1"), "ledger did not render a second settled row");
            Assert.IsNotNull(Required(board, "LedgerTicket2"), "ledger did not render a third settled row");

            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "12-ledger-populated-multi", capturedPaths);

            AssertCaptureOutput(capturedPaths, 2);
        }

        /// <summary>
        /// S52 (batch 9): the run-verdict screen, in both terminal states, so the DD can rule its
        /// ground. Nobody has ever seen this screen — it is reachable only by ending a run, so no
        /// capture in the project's life has included it, and every finding about it so far was read
        /// from source. That is the same blindness that let a BUY-in-biro violation survive weeks of
        /// review, and it is why the two S52 fixes it does carry (the losing headline off oxide, and
        /// NEW RUN as a wax primary) are asserted here before either frame is shot.
        ///
        /// Both states are forced through the payment schedule rather than played. `RunConfig.Rounds`
        /// is `Payments.Length`, so a ONE-element schedule makes round 1 the final round: settle it
        /// with a payment the bank covers and the run is won, with one it cannot and the run is lost.
        /// No RNG, no eight-round grind, and no dependence on a lucky seed. The real schedule is
        /// {60, 70, 85, 105, 155, 375, 710, 1350} against a 350 bank, so an honestly-played win needs
        /// about +2560 of betting profit — not something a capture fixture can produce.
        ///
        /// The run is swapped onto the director through its private setter. That is deliberate and it
        /// is the lesser of two evils: the alternative is a test seam on `RunDirector`, which three
        /// seats share and which is about to take a 159-commit merge from main. A one-line reflection
        /// call in a test file cannot conflict; a new property on RunDirector can. `StartNewRun` is
        /// called first so the director's own sweat bookkeeping is reset rather than left stale.
        /// </summary>
        [UnityTest]
        public IEnumerator Capture_the_run_verdict_in_both_terminal_states()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            string outputDirectory = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "..", "..", "artifacts", "surething-ui"));
            Directory.CreateDirectory(outputDirectory);
            string runPrefix = DateTime.UtcNow.ToString(
                "yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture);
            var capturedPaths = new List<string>();

            // S57 — answered: **capture data.** The first cut of these two frames ended the LOSS
            // holding $350 and the WIN holding $290, so the losing run was richer than the winning
            // one. Nothing is wrong with the verdict: it derives from bank-versus-payment, and the
            // engine does not deduct a payment the bank cannot meet, so a forced loss keeps its
            // whole bank. The banks were arbitrary because both states are forced through a
            // schedule rather than played.
            //
            // Arbitrary is still not acceptable in a capture set, whose entire job is to be
            // readable as evidence by someone who was not told how it was made. So the figures are
            // now chosen to read: a win that paid its way, and a bust that could not.
            yield return DriveToVerdict(laptop, bank: 350d, payment: 60d, expected: Phase.RunWon);
            Assert.AreEqual("THE HOUSE BLINKS FIRST", TextOf(Required(App(laptop), "Verdict")));
            string wonFigures = TextOf(Required(App(laptop), "Final"));
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "13-verdict-run-won", capturedPaths);

            // $40 against a $155 payment — a real figure from the shipped schedule
            // {60, 70, 85, 105, 155, 375, 710, 1350}, and a bank that plainly cannot meet it.
            yield return DriveToVerdict(laptop, bank: 40d, payment: 155d, expected: Phase.RunLost);
            Assert.AreEqual("THE BOOKIE COLLECTS", TextOf(Required(App(laptop), "Verdict")));
            string lostFigures = TextOf(Required(App(laptop), "Final"));
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "14-verdict-run-lost", capturedPaths);

            // The gate S57 actually wants: the two frames must be legible as themselves without a
            // caption. A set where the loser ends richer than the winner teaches the reader the
            // opposite of what it is evidence for.
            Assert.AreNotEqual(wonFigures, lostFigures, "the two verdicts must not print the same figures");
            StringAssert.Contains("$290", wonFigures, "the won frame should end holding more");
            StringAssert.Contains("$40", lostFigures, "the lost frame should end holding less");

            AssertCaptureOutput(capturedPaths, 4);
        }

        /// <summary>
        /// The LEDGER carrying a ticket from a round that is over — the one claim in the re-submit
        /// set that was proven by construction and by nothing photographic.
        ///
        /// Engine retention is the whole reason this screen was blocked for a fortnight. `ExitShop`
        /// does `Round++; _tickets.Clear()`, so before retention a player who bet in rounds 1-3 and
        /// opened the LEDGER in round 4 met an empty screen captioned SETTLED TICKETS · THIS RUN.
        /// Every ledger frame ever shot is a ROUND 1 frame, which cannot tell that story: a
        /// single-round ledger looks identical whether the screen reads the retained history or the
        /// current round.
        ///
        /// So this drives a real round boundary. Round 1 settles a ticket, ExitShop clears it, round
        /// 2 settles another, and the assertion is the point: **the board renders more rows than the
        /// current round holds.** That is only possible if the screen is reading retention, and it
        /// is exactly what no previous frame could show.
        /// </summary>
        [UnityTest]
        public IEnumerator Capture_the_ledger_carrying_a_finished_round()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            string outputDirectory = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "..", "..", "artifacts", "surething-ui"));
            Directory.CreateDirectory(outputDirectory);
            string runPrefix = DateTime.UtcNow.ToString(
                "yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture);
            var capturedPaths = new List<string>();

            // A run that cannot bust before round 2. The shipped schedule against a 350 bank means
            // two rounds of real betting can end the run — the first cut of this fixture did exactly
            // that and went to RunLost with no ledger to open. Whether the player survives is not
            // what this state is about; the round BOUNDARY is.
            //
            // **The schedule is the real one and only the bank is deepened.** An earlier cut used
            // {1, 1, 1, …}, which worked and printed TARGET $1 in the masthead — visibly a test
            // rig, and S57 is the ruling that a capture whose figures are arbitrary cannot be read
            // as evidence by anyone who was not told how it was made. With the shipped payments the
            // masthead reads $60 then $70, which is what a player would actually see.
            laptop.director.StartNewRun(SeedLedgerAcrossRounds);
            var run = new Run(SeedLedgerAcrossRounds, new RunConfig { StartingBank = 5000d });
            SetDirectorRun(laptop.director, run);
            // C34: this flow was already reproducible — a fixed label, not a rolled seed — but nothing
            // checked that the run being shot was the run that was pinned. The seed is stated once
            // above and asserted here; it used to be typed twice with nothing comparing the two.
            AssertShootingSeed(laptop, SeedLedgerAcrossRounds);

            TvSweatScreen screen = laptop.tv;
            screen.TimeScaleOverride = 0.0001f;
            screen.ForceSeated(true);

            yield return SettleOneRound(laptop, run, "round 1");
            Assert.AreEqual(1, run.SettledTickets.Count, "round 1 must have retained its ticket");
            string firstRoundTicketId = run.SettledTickets[0].Id;

            // The round boundary this whole state exists to cross.
            Assert.AreEqual(Phase.Shop, run.Phase, "a settled non-final round lands in the shop");
            laptop.director.ExitShop();
            yield return WaitForRebuild();
            Assert.AreEqual(2, run.Round, "ExitShop advances the round");
            Assert.AreEqual(0, run.Tickets.Count,
                "ExitShop clears the round's tickets — this is the clearing retention exists to survive");
            Assert.AreEqual(1, run.SettledTickets.Count,
                "and the retained history keeps round 1's ticket after that clearing");

            yield return SettleOneRound(laptop, run, "round 2");

            // Navigated through the OS rather than by clicking the tray slot. The tray's wiring is
            // already covered by two other fixtures and by LaptopOsTests; what this state is
            // evidence for is what the ledger CONTAINS, and routing through the control adds a
            // failure mode that has nothing to do with the claim.
            // Navigated through the OS rather than by clicking the tray slot: the tray's wiring is
            // covered by two other fixtures, and what this state is evidence for is what the ledger
            // CONTAINS. SettleOneRound has already let the OS absorb the phase change — navigating
            // before it does gets silently undone (see the note there).
            laptop.Os.OpenOldSlips();
            yield return WaitForRebuild();
            Transform board = Required(App(laptop), "LedgerBoard");

            int rendered = 0;
            while (Find(board, "LedgerTicket" + rendered) != null) rendered++;

            // The assertion the set was missing. run.Tickets holds round 2 alone; the board shows
            // both rounds, so it is reading SettledTickets and not the current round.
            Assert.AreEqual(1, run.Tickets.Count, "round 2 holds exactly its own ticket");
            Assert.Greater(rendered, run.Tickets.Count,
                "the ledger renders more rows than the current round holds — that is retention, "
                + "and a round-1-only frame can never demonstrate it");
            Assert.AreEqual(2, rendered, "one settled ticket from each round");
            Assert.IsNotEmpty(firstRoundTicketId, "round 1's ticket must carry an identity to be found by");

            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "15-ledger-across-rounds", capturedPaths);

            AssertCaptureOutput(capturedPaths, 2);
        }

        /// <summary>CAPTURE CHARTER 2026-08-16, shoot 1 — THE ENTRY SHEET AT HEAD, ALL FIVE MARKET
        /// DESTINATIONS, ONE CLEAN FRAME EACH.
        ///
        /// <para><b>CLEAN means nothing staged.</b> No pick is placed, so no wide biro ring, no
        /// staged receipt and no working-margin state rides into the frame. Every ENTRY state this
        /// fixture already shoots carries a selection (<c>02-entry-selected-wide-ring</c>) or exists
        /// to photograph the scroll rail (<c>02b-entry-players-scrolling-rail</c>) — so the sheet's
        /// own RESTING treatment has never been captured on four of the five destinations at all,
        /// and GOALS has only ever been shot with a ring on it.</para>
        ///
        /// <para><b>Five is the surface's own number, not a target.</b> <c>BuildDetail</c> builds
        /// exactly GOALS/BTTS/CORNERS/CARDS/PLAYERS, and the loop below drives that same authored
        /// order BY BUTTON NAME — so a sixth destination added later fails here as a missing button
        /// rather than shipping a silently short set.</para>
        ///
        /// <para><b>The body is asserted to have rebuilt before each shutter.</b> A frame of the
        /// previous destination filed under the next destination's name is C50's shape exactly, and
        /// a set carrying one would be unreadable without a second instrument.</para>
        ///
        /// <para><b>This fixture makes no claim about density, grouping or ordering.</b> It is
        /// evidence for a read that has not been made.</para></summary>
        [UnityTest]
        public IEnumerator Capture_the_entry_sheet_across_all_five_market_destinations()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            yield return PinRun(laptop, SeedLobby);

            string outputDirectory = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "..", "..", "artifacts", "surething-ui"));
            Directory.CreateDirectory(outputDirectory);
            string runPrefix = DateTime.UtcNow.ToString(
                "yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture);
            var capturedPaths = new List<string>();

            Assert.AreEqual(SportsbookApp.Tab.Lobby, laptop.Os.CurrentTab);
            Invoke(Required(Required(App(laptop), "Matchup0"), "Details"));
            yield return WaitForRebuild();
            Assert.AreEqual(SportsbookApp.Tab.Detail, laptop.Os.CurrentTab);

            // The rail's own order, READ from MarketDestinations.All rather than transcribed. The
            // transcribed list this replaces named five destinations that no longer exist, and it
            // could only ever have been found by running the capture — a capture set that walks the
            // rail must walk the rail the build actually prints (S74-am3: a constant that happens to
            // equal the right answer is a constant that will stop equalling it). Tab GameObjects are
            // named by enum member, so ToString() is the name.
            var destinations = MarketDestinations.All;
            int states = 0;
            for (int i = 0; i < destinations.Count; i++)
            {
                string label = destinations[i].ToString();
                Invoke(Required(Required(App(laptop), "MarketDestinations"), "DetailTab" + label));
                yield return WaitForRebuild();

                Assert.IsNotNull(Required(App(laptop), "MarketBody"),
                    $"{label}: the market body must have rebuilt before the shutter");
                Assert.AreEqual(0, laptop.Slip.Picks.Count,
                    $"{label}: the sheet must be CLEAN — a staged pick puts a ring and a receipt "
                    + "into a frame whose whole subject is the resting sheet");

                yield return CaptureState(laptop, outputDirectory, runPrefix,
                    $"E{i + 1:00}-entry-{label.ToLowerInvariant()}", capturedPaths);
                states++;

                // THE DD'S BINDING CONDITION 1.1 (capture pre-commitment, 2026-08-16). A tab's first
                // screen alone cannot answer "does market kind X appear anywhere on ENTRY" — a block
                // nested below the fold is, in one unscrolled frame, INDISTINGUISHABLE FROM AN ABSENT
                // ONE. So a tab is either shown whole (S27's rail absent, which is itself the
                // evidence of absence C37 requires) or it is shot at top AND at scroll-bottom.
                Transform rail = Find(App(laptop), "PositionRailTrack");
                Debug.Log($"[entry-extent] {label}: rail={(rail != null ? "PRESENT — scrolls" : "ABSENT — whole content visible")}");
                if (rail == null) continue;

                ScrollRect scroll = Required(App(laptop), "MarketBody")
                    .GetComponentInChildren<ScrollRect>(true);
                Assert.IsNotNull(scroll, $"{label}: a rail is present so a ScrollRect must be too");
                scroll.verticalNormalizedPosition = 0f;   // 0 == bottom
                Canvas.ForceUpdateCanvases();
                yield return WaitForRebuild();
                Assert.IsNotNull(Find(App(laptop), "PositionRailTrack"),
                    $"{label}: the rail must still be present at the extent");

                yield return CaptureState(laptop, outputDirectory, runPrefix,
                    $"E{i + 1:00}-entry-{label.ToLowerInvariant()}-bottom", capturedPaths);
                states++;
            }

            AssertCaptureOutput(capturedPaths, states * 2);
        }

        /// <summary>Places one ticket and travels the real place-lock-sweat path to a terminal
        /// state. The ledger reads engine state, so a ticket has to be settled rather than written
        /// into a settled-looking shape.</summary>
        private static IEnumerator SettleOneRound(LaptopScreen laptop, Run run, string which)
        {
            Assert.AreEqual(Phase.Betting, run.Phase, $"{which}: must start in betting");
            (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(run);
            Ticket ticket = run.PlaceTicket(picks, stake);
            laptop.director.LockRound();
            Assert.AreEqual(Phase.Sweat, run.Phase, $"{which}: locking must enter the sweat");

            float start = Time.realtimeSinceStartup;
            while (run.Phase == Phase.Sweat)
            {
                if (Time.realtimeSinceStartup - start > 60f)
                {
                    Assert.Fail($"{which}: the ticket never settled");
                    yield break;
                }
                yield return null;
            }
            Assert.AreNotEqual(TicketState.Open, ticket.State, $"{which}: the ticket must be terminal");
            // Named loudly because the first cut of this fixture failed here silently and surfaced
            // 40 lines later as a missing LedgerBoard: the run had simply ended.
            Assert.AreEqual(Phase.Shop, run.Phase,
                $"{which}: settled into {run.Phase}, not Shop — the run ended and there is no next round");

            // Let the OS absorb the phase change before the caller does anything else. The loop
            // above exits the instant the ENGINE leaves Sweat, which is one or more frames before
            // LaptopOs notices and runs ApplyPhaseDefault — and that default sets _activeApp itself.
            // Navigating in that window looks like it worked and is then silently overwritten on the
            // next tick, which is exactly how this fixture first failed: OpenOldSlips ran, the phase
            // default fired afterwards, and the screen sat on REWARDS with no ledger to shoot.
            yield return WaitForRebuild();
        }

        /// <summary>Ends a run in one settle, and refuses to shoot a frame that cannot show what it
        /// claims to — the two S52 fixes are asserted on the live tree, not assumed from the diff.</summary>
        private static IEnumerator DriveToVerdict(LaptopScreen laptop, double bank, double payment,
            Phase expected)
        {
            // R38: this screen PRINTS the seed — `FINAL BANK $x   ·   SEED {run.Rng.RunSeed}`, the
            // subline built in LaptopOs.RenderVerdict. These forced states were seeded
            // `verdict-RunWon` and `verdict-RunLost`, so both frames photographed the rig's own name
            // for the state in the one slot on the screen where a product fact belongs: the capture
            // apparatus visible inside the capture, on evidence the DD then rules against.
            //
            // A forced state's seed is a legal production seed and nothing else — what that means is
            // stated once, in AssertPinnedSeed, rather than restated here. This selects between two
            // named constants; it does not rebuild a seed inline.
            //
            // The run is swapped in below rather than played, so StartNewRun's own run is discarded —
            // but it is seeded identically anyway. A director left holding a different seed from the
            // run being shot is precisely the two-values-for-one-thing drift this file keeps paying
            // for elsewhere.
            string runSeed = expected == Phase.RunWon ? SeedVerdictWon : SeedVerdictLost;
            AssertPinnedSeed(runSeed);

            laptop.director.StartNewRun(runSeed);
            var run = new Run(runSeed,
                new RunConfig { Payments = new[] { payment }, StartingBank = bank });
            Assert.AreEqual(1, run.Config.Rounds, "a one-element schedule must make round 1 the last");
            SetDirectorRun(laptop.director, run);
            AssertShootingSeed(laptop, runSeed);

            laptop.director.LockRound(); // no tickets: FinishAndSettle runs on the spot
            Assert.AreEqual(expected, run.Phase,
                $"a payment of {payment} against a bank of {run.Config.StartingBank} must end {expected}");
            yield return WaitForRebuild();

            Transform app = App(laptop);

            // S55: the machine stays. These frames had no rail and no tray while every other
            // destination carried both, which made the last screen of the run a game-over card
            // instead of an app on his laptop. Asserted before the shot, because "the chrome is
            // missing" is exactly the kind of absence a frame proves and a suite normally cannot.
            Assert.IsNotNull(Find(app, "NotebookRail"), "S55: the verdict renders inside the chrome — rail missing");
            Assert.IsNotNull(Find(app, "NotebookTray"), "S55: the verdict renders inside the chrome — tray missing");

            // S53-am: the ground is --ground, like every other destination. The bespoke value that
            // stood here rendered aubergine and darker than --ink.
            Assert.IsTrue(SameInk(Required(app, "VerdictBg").GetComponent<Image>().color, LaptopOs.Ink),
                "S53-am: the verdict ground is --ground, not a bespoke value");

            // R38: and the frame must actually print that seed, not merely hold it. This is the
            // assertion whose absence let the rig's own label sit in a photographed subline through
            // two rounds of review — the suite read the headline's colour and never read the line
            // underneath it.
            StringAssert.Contains(runSeed, TextOf(Required(app, "Final")),
                "R38: the verdict subline prints the run's own seed");

            Transform verdict = Required(app, "Verdict");

            // S52: the loss is carried by value, not by oxide. Oxide is the house's mark, and this
            // is exactly the assertion whose absence let the violation live unseen on an
            // unphotographed screen.
            Color headline = verdict.GetComponent<TMP_Text>().color;
            Color subline = Required(app, "Final").GetComponent<TMP_Text>().color;
            if (expected == Phase.RunLost)
            {
                Assert.IsFalse(SameInk(headline, LaptopOs.MoneyBad),
                    "S52: the losing verdict is never oxide — that is the house's mark");
                Assert.IsTrue(SameInk(headline, LaptopOs.TonerSecondary),
                    "S59: the losing headline is --toner-2");
                Assert.IsTrue(SameInk(subline, LaptopOs.Muted),
                    "S59: its subline steps down with it, to --toner-3");
            }
            else
            {
                Assert.IsTrue(SameInk(headline, LaptopOs.MoneyGold), "the winning headline is wax");
                Assert.IsTrue(SameInk(subline, LaptopOs.White), "its subline is --toner");
            }

            // The invariant S59 is really about, and the one a per-element value check missed: the
            // statement must outrank the facts that qualify it. --toner-3 on the headline was 1:1
            // with S53 as written and still produced a composition whose dimmest element was the
            // sentence, with its own context line twice as bright.
            //
            // **Losing screen only, and the first cut of this assertion got that wrong.** Applied to
            // the winning screen it fails: wax (D9A441) has LOWER Rec.709 luminance than toner
            // (D9D4C5) — 0.66 against 0.83 — because on this surface emphasis is not one scalar.
            // Wax outranks toner by chroma; toner-2 outranks toner-3 by value. The losing screen is
            // where both elements are neutral and value alone does the ranking, which is exactly
            // why that is the screen the inversion happened on. The DD measured and ratified the
            // winning screen as correct, so its ranking is asserted by token above, not by weight.
            if (expected == Phase.RunLost)
                Assert.Greater(Luminance(headline), Luminance(subline),
                    "S59: on the drained screen the headline must still outrank its own subline");

            // S18/S52: NEW RUN is a wax primary — wax field, wax ink, and the 2px --wax-deep edge
            // that MakeWaxPrimary adds and a plain MakeButton does not.
            Transform newRun = Required(app, "NewRun");
            Assert.IsTrue(SameInk(newRun.GetComponent<Image>().color, LaptopOs.MoneyGold),
                "NEW RUN is a wax field — it was a biro-filled one, which is Law Two and S18 at once");
            Assert.IsTrue(SameInk(Required(newRun, "Label").GetComponent<TMP_Text>().color, LaptopOs.WaxInk),
                "type on wax is wax ink");
            foreach (string edge in new[] { "WaxEdgeTop", "WaxEdgeBottom", "WaxEdgeLeft", "WaxEdgeRight" })
                Assert.IsNotNull(Find(newRun, edge), $"NEW RUN is missing its wax edge '{edge}'");
        }

        /// <summary>Swaps the director's run. See the fixture note above for why this is reflection
        /// against a private setter rather than a seam added to RunDirector itself.</summary>
        private static void SetDirectorRun(RunDirector director, Run run)
        {
            PropertyInfo property = typeof(RunDirector).GetProperty(
                nameof(RunDirector.Run), BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(property, "RunDirector.Run is gone — this capture drove the run through it");
            MethodInfo setter = property.GetSetMethod(nonPublic: true);
            Assert.IsNotNull(setter, "RunDirector.Run has no setter for this capture to drive through");
            try
            {
                setter.Invoke(director, new object[] { run });
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException ?? exception;
            }
        }

        /// <summary>Compares two inks at 8-bit precision — the palette is authored as Color32, so an
        /// exact float comparison would be asserting against rounding rather than against a token.</summary>
        /// <summary>Rec.709 relative luminance. Used to compare two authored inks by weight rather
        /// than by token identity — S59 is a ranking, and a ranking cannot be asserted one colour
        /// at a time, which is exactly how it came to be inverted.</summary>
        private static float Luminance(Color c) => 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;

        private static bool SameInk(Color a, Color b)
        {
            Color32 x = a;
            Color32 y = b;
            return x.r == y.r && x.g == y.g && x.b == y.b;
        }

        /// <summary>Whether this run is a DELIBERATE SHOOT. Set `SBR_SHOOT` to write frames.
        ///
        /// <para><b>THE GUARD IS ON THE WRITE, NOT ON THE ENTRY POINT — and that is a ruling, not a
        /// shortcut.</b> The routed defect is TV's class: capture flows running inside every routine
        /// suite, so a dock could scope against frames a suite happened to write rather than a
        /// deliberate shoot. TV closed it with `[Explicit]` on the entry points, which is right for
        /// THAT file — it says so itself: *"DELETE THIS FILE once evidence review is done — it is not
        /// production coverage."*</para>
        ///
        /// <para><b>This file is the opposite.</b> Its nine flows carry ~130 substantive assertions
        /// and are the ONLY routine coverage of the run verdict's copy, the ledger's retention across
        /// rounds, the leg-cap treatment and the scroll actually engaging. Guarding the entry points
        /// would close the defect by deleting the coverage.</para>
        ///
        /// <para>So the two concerns are separated where they actually differ. The defect is not
        /// "these tests run" — it is "these tests WRITE FRAMES". Routine runs verify and write
        /// nothing; a shoot is opt-in and therefore deliberate. **A frame can now only exist because
        /// someone asked for one**, which is the provenance the docks were arguing from timestamps
        /// before.</para></summary>
        internal static bool ShootRequested =>
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SBR_SHOOT"));

        /// <summary>The paired tail every capture flow ends with. Positive in BOTH directions: on a
        /// shoot it requires the frames; on a routine run it requires that none were written, so the
        /// guard cannot silently regress into writing again.</summary>
        private static void AssertCaptureOutput(ICollection<string> capturedPaths, int expectedPaths)
        {
            if (!ShootRequested)
            {
                Assert.IsEmpty(capturedPaths,
                    "a routine suite must write no frames — set SBR_SHOOT for a deliberate shoot. "
                    + "A frame written here could be scoped against by a dock as though it were one");
                return;
            }
            Assert.AreEqual(expectedPaths, capturedPaths.Count, "states must emit paired captures");
            foreach (string path in capturedPaths)
            {
                Assert.IsTrue(File.Exists(path), $"capture missing: {path}");
                Assert.Greater(new FileInfo(path).Length, 0L, $"capture is empty: {path}");
            }
        }

        private static IEnumerator CaptureState(LaptopScreen laptop, string outputDirectory,
            string runPrefix, string stateName, ICollection<string> capturedPaths)
        {
            yield return WaitForRebuild();
            UnityEngine.Canvas.ForceUpdateCanvases();

            RectTransform canvasRect = Required(
                laptop.transform, "LaptopOsCanvas") as RectTransform;
            Assert.IsNotNull(canvasRect, "LaptopOsCanvas RectTransform missing");

            // The state was still BUILT and still asserted above — only the frames are withheld.
            // That is the whole point of guarding here rather than at the entry point.
            if (!ShootRequested)
            {
                Debug.Log($"[SureThingCapture] skipped '{stateName}' — routine run, not a shoot. "
                    + "Set SBR_SHOOT to write frames.");
                yield break;
            }

            string flatPath = Path.Combine(outputDirectory,
                $"{runPrefix}-{stateName}-flat-{FlatWidth}x{FlatHeight}.png");
            CaptureFlatCanvas(canvasRect, flatPath);
            capturedPaths.Add(flatPath);
            Debug.Log($"[SureThingCapture] {flatPath}");

            string angledPath = Path.Combine(outputDirectory,
                $"{runPrefix}-{stateName}-main-camera-{AngledWidth}x{AngledHeight}.png");
            CaptureAngledMainCamera(laptop, canvasRect, angledPath);
            capturedPaths.Add(angledPath);
            Debug.Log($"[SureThingCapture] {angledPath}");
        }

        private static void CaptureFlatCanvas(RectTransform canvasRect, string outputPath)
        {
            UnityEngine.Canvas canvas = canvasRect.GetComponent<UnityEngine.Canvas>();
            Assert.IsNotNull(canvas, "LaptopOsCanvas Canvas missing");
            Camera mainCamera = Camera.main;
            Assert.IsNotNull(mainCamera, "Main Camera missing");

            var corners = new Vector3[4];
            canvasRect.GetWorldCorners(corners);
            Vector3 rightVector = corners[3] - corners[0];
            Vector3 upVector = corners[1] - corners[0];
            float worldWidth = rightVector.magnitude;
            float worldHeight = upVector.magnitude;
            Assert.Greater(worldWidth, 0f, "LaptopOsCanvas world width must be positive");
            Assert.Greater(worldHeight, 0f, "LaptopOsCanvas world height must be positive");

            Vector3 right = rightVector.normalized;
            Vector3 up = upVector.normalized;
            Vector3 normal = Vector3.Cross(right, up).normalized;
            Vector3 center = (corners[0] + corners[2]) * 0.5f;
            if (Vector3.Dot(normal, mainCamera.transform.position - center) < 0f)
                normal = -normal;

            var cameraObject = new GameObject(
                "SureThingFlatCaptureCamera", typeof(Camera));
            cameraObject.hideFlags = HideFlags.HideAndDontSave;
            Camera flatCamera = cameraObject.GetComponent<Camera>();
            flatCamera.enabled = false;
            flatCamera.orthographic = true;
            flatCamera.aspect = FlatWidth / (float)FlatHeight;
            flatCamera.orthographicSize = Mathf.Max(
                worldHeight * 0.5f,
                worldWidth / (2f * flatCamera.aspect));
            flatCamera.nearClipPlane = 0.001f;
            flatCamera.farClipPlane = 10f;
            flatCamera.clearFlags = CameraClearFlags.SolidColor;
            flatCamera.backgroundColor = Color.black;
            flatCamera.cullingMask = 1 << CaptureLayer;
            flatCamera.allowHDR = false;
            flatCamera.allowMSAA = false;
            flatCamera.useOcclusionCulling = false;
            cameraObject.transform.SetPositionAndRotation(
                center + normal * 0.35f,
                Quaternion.LookRotation(-normal, up));

            Transform[] canvasHierarchy =
                canvasRect.GetComponentsInChildren<Transform>(true);
            var originalLayers = new int[canvasHierarchy.Length];
            Camera originalWorldCamera = canvas.worldCamera;
            try
            {
                for (int i = 0; i < canvasHierarchy.Length; i++)
                {
                    originalLayers[i] = canvasHierarchy[i].gameObject.layer;
                    canvasHierarchy[i].gameObject.layer = CaptureLayer;
                }
                canvas.worldCamera = flatCamera;
                UnityEngine.Canvas.ForceUpdateCanvases();
                RenderCameraToPng(flatCamera, FlatWidth, FlatHeight, outputPath);
            }
            finally
            {
                canvas.worldCamera = originalWorldCamera;
                for (int i = 0; i < canvasHierarchy.Length; i++)
                    if (canvasHierarchy[i] != null)
                        canvasHierarchy[i].gameObject.layer = originalLayers[i];
                UnityEngine.Object.DestroyImmediate(cameraObject);
            }
        }

        private static void CaptureAngledMainCamera(LaptopScreen laptop,
            RectTransform canvasRect, string outputPath)
        {
            Camera mainCamera = Camera.main;
            Assert.IsNotNull(mainCamera, "Main Camera missing");
            DeskFocus focus = laptop.GetComponent<DeskFocus>();
            if (focus == null) focus = laptop.GetComponentInParent<DeskFocus>();
            Assert.IsNotNull(focus, "Laptop DeskFocus missing");
            Assert.IsNotNull(focus.focusAnchor, "Laptop DeskFocus focus anchor missing");
            UnityEngine.Canvas canvas = canvasRect.GetComponent<UnityEngine.Canvas>();
            Assert.IsNotNull(canvas, "LaptopOsCanvas Canvas missing");

            Vector3 originalPosition = mainCamera.transform.position;
            Quaternion originalRotation = mainCamera.transform.rotation;
            float originalFieldOfView = mainCamera.fieldOfView;
            float originalAspect = mainCamera.aspect;
            Camera originalWorldCamera = canvas.worldCamera;
            try
            {
                mainCamera.transform.SetPositionAndRotation(
                    focus.focusAnchor.position, focus.focusAnchor.rotation);
                mainCamera.fieldOfView = focus.focusFov;
                mainCamera.aspect = AngledWidth / (float)AngledHeight;
                canvas.worldCamera = mainCamera;
                UnityEngine.Canvas.ForceUpdateCanvases();
                RenderCameraToPng(
                    mainCamera, AngledWidth, AngledHeight, outputPath);
            }
            finally
            {
                canvas.worldCamera = originalWorldCamera;
                mainCamera.fieldOfView = originalFieldOfView;
                mainCamera.aspect = originalAspect;
                mainCamera.transform.SetPositionAndRotation(
                    originalPosition, originalRotation);
            }
        }

        private static void RenderCameraToPng(
            Camera camera, int width, int height, string outputPath)
        {
            RenderTexture originalTarget = camera.targetTexture;
            RenderTexture originalActive = RenderTexture.active;
            var target = new RenderTexture(
                width, height, 24, RenderTextureFormat.ARGB32)
            {
                name = "SureThingCaptureTarget",
                hideFlags = HideFlags.HideAndDontSave,
            };
            var image = new Texture2D(
                width, height, TextureFormat.RGB24, false)
            {
                name = "SureThingCaptureImage",
                hideFlags = HideFlags.HideAndDontSave,
            };

            try
            {
                target.Create();
                camera.targetTexture = target;
                RenderTexture.active = target;
                camera.Render();
                image.ReadPixels(new Rect(0f, 0f, width, height), 0, 0, false);
                image.Apply(false, false);
                File.WriteAllBytes(outputPath, image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = originalTarget;
                RenderTexture.active = originalActive;
                target.Release();
                UnityEngine.Object.DestroyImmediate(image);
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static void InvokeView(
            RevealedView view, string methodName, params object[] args)
        {
            MethodInfo method = typeof(RevealedView).GetMethod(
                methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"RevealedView test seam '{methodName}' missing");
            try
            {
                method.Invoke(view, args);
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException ?? exception;
            }
        }

        /// <summary>R38: what makes a string a legal pinned capture seed, defined once. Both the
        /// flow-level pin and the forced verdict runs check against this rather than each restating
        /// the rule — a guard stated twice is a guard that can disagree with itself.</summary>
        private static void AssertPinnedSeed(string seed)
        {
            // 8 characters from A-Z0-9 is what RunDirector.NewSeed emits, so an all-digit seed of
            // that length is an ordinary member of the space a player is dealt from — it reads as a
            // real run rather than as anything belonging to the rig. Numeric specifically, because
            // T31 is the precedent one surface over: a harness seed shaped like a label was read as
            // a debug token and cost a withdrawn finding.
            Assert.AreEqual(8, seed.Length,
                $"R38: a pinned capture seed is 8 characters (got '{seed}')");
            foreach (char c in seed)
                Assert.IsTrue(char.IsDigit(c),
                    $"R38: a pinned capture seed is numeric (got '{seed}')");
        }

        /// <summary>Pins a capture flow to a fixed run seed and proves the run is actually carrying
        /// it before a single frame is shot.
        ///
        /// The proof is the point. Pinning that silently failed would leave the flow rolling exactly
        /// as before while every comment in the file claimed otherwise — the same shape as a filter
        /// that matches nothing and exits green (C29).</summary>
        private static IEnumerator PinRun(LaptopScreen laptop, string seed)
        {
            AssertPinnedSeed(seed);
            laptop.director.StartNewRun(seed);
            yield return WaitForRebuild();
            AssertShootingSeed(laptop, seed);
        }

        /// <summary>C34, consequence 1: a flow proves the director is carrying the seed it pinned,
        /// before it shoots anything. **An unasserted pin is a comment** — and a pin that silently
        /// failed would leave the flow rolling exactly as before while the file claimed otherwise.
        ///
        /// Separate from <see cref="AssertPinnedSeed"/> on purpose: every flow is asserted here, but
        /// only the numeric ones are asserted there. `15-ledger-across-rounds` is pinned and exempt
        /// from R38's spelling by ruling, and one helper doing both jobs would have no way to say
        /// that.</summary>
        private static void AssertShootingSeed(LaptopScreen laptop, string seed)
            => Assert.AreEqual(seed, laptop.director.Run.Rng.RunSeed,
                "C34: the flow must be shooting the seed it pinned, not one the director rolled");

        private static IEnumerator Boot()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "Room", LoadSceneMode.Single);
            Assert.IsNotNull(load, "Room scene is not available");
            while (!load.isDone) yield return null;

            LaptopScreen laptop = Laptop();
            float start = Time.realtimeSinceStartup;
            while (laptop.director == null
                || laptop.director.Run == null
                || laptop.Os.OnDesktop)
            {
                if (Time.realtimeSinceStartup - start > 10f)
                {
                    Assert.Fail(
                        "SureThing did not reach the betting lobby within 10 seconds");
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

        private static LaptopScreen Laptop()
        {
            LaptopScreen laptop =
                UnityEngine.Object.FindAnyObjectByType<LaptopScreen>();
            Assert.IsNotNull(laptop, "LaptopScreen missing");
            Assert.IsNotNull(laptop.tv, "Laptop TV reference missing");
            return laptop;
        }

        private static Transform App(LaptopScreen laptop)
            => Required(laptop.transform, "App");

        private static Button FirstNamedButton(Transform root, string prefix)
        {
            var matches = new List<Button>();
            foreach (Button button in root.GetComponentsInChildren<Button>(true))
                if (button.name.StartsWith(prefix, StringComparison.Ordinal))
                    matches.Add(button);
            matches.Sort((left, right)
                => string.CompareOrdinal(left.name, right.name));
            Assert.Greater(matches.Count, 0,
                $"No button beginning '{prefix}' exists beneath '{root.name}'");
            return matches[0];
        }

        private static Transform Required(Transform root, string name)
        {
            Transform found = Find(root, name);
            Assert.IsNotNull(found,
                $"Required named UI node '{name}' missing beneath '{root.name}'");
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
    }
}
