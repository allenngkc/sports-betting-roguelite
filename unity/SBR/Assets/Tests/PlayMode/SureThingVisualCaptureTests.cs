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
        /// <summary>The surfaces §8 evidence set. R38: 8 digits, scattered — the state name lives
        /// in the filename, never in the seed.</summary>
        private const string SeedMarketSheet = "54435761";
        /// <summary>The WORST-CASE row's slate (`S101`). Chosen, not stumbled on: the club pool was
        /// enumerated whole and measured, and this seed is one of the slates that seats a co-widest
        /// club on MATCHUP 0 — `San Francisco Spreadsheets`, home, so its team-total rows sit near
        /// the top of the CORNERS destination's second group rather than at the foot of a list.
        ///
        /// <para>R38: 8 digits, scattered — and the state name lives in the FILENAME, never here.
        /// The slate is `Fresno Notaries @ San Francisco Spreadsheets` and five ordinary matchups
        /// behind it; nothing about this run is rigged except which of the 320 reachable clubs the
        /// draw happened to seat.</para></summary>
        private const string SeedWorstCaseRow = "49768152";
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
            InvokeView(view, "Reset", laptop.director.Run, ticket, 0, TicketProbAtStart(ticket));
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

        /// <summary>The §8 evidence for `spec-market-surfaces-2026-08-17` — the four things that
        /// spec says must be shown before Design-verified, in five states.
        ///
        /// <para><b>Seed 54435761 is R38-shaped on purpose</b> — 8 digits, scattered, an ordinary
        /// member of `NewSeed`'s own space. A seed spelled like a label ("SHEET-EMPTY") would be a
        /// rig string in a player slot, which is R38's whole subject. The STATE NAME lives in the
        /// filename, which is also where the empty-group frame declares its non-shipped config.</para>
        ///
        /// <para><b>Why the empty group needs a config at all.</b> `no prices offered` (§5.3/`S89`)
        /// is UNREACHABLE at the shipped `RunConfig` — measured, zero empty groups across 18,000
        /// matchups, because `CorrectScoreFloor` 0.02 always leaves CORRECT SCORE ≥ 11 rows and
        /// MULTI SCORER ≥ 3. Raising the floor to 0.08 empties MULTI SCORER. Per `S57`, a capture
        /// whose figures are arbitrary cannot be read as evidence by anyone who was not told how it
        /// was made — so the floor is named in the frame's filename. It is NOT captioned into the
        /// pixels: that would put rig state in a player slot, the very defect `R38` records.</para>
        ///
        /// <para><b>Second disclosure on that frame:</b> the raised floor also thins CORRECT SCORE
        /// from 13 rows to 6. Two things differ between the shipped sheet and that frame, not one,
        /// and the thinner CORRECT SCORE must not be read as a defect.</para>
        ///
        /// <para><b>The amber pair is DISCHARGED, and is now one frame.</b> §4.4 shot the same
        /// sheet twice — same seed, same scroll position, `PriceTakesAmber` off and on — so `S91`
        /// half two could be decided on the frame rather than argued. It was: `S97` (DD batch 113)
        /// rules that the price does NOT take the amber. The switch is gone with the question it
        /// existed to put, and what remains is the single shipped toner frame with its ink still
        /// asserted.</para>
        ///
        /// <para>§8's "every destination populated" is not shot here — it is already covered by the
        /// destinations walk, which reads `MarketDestinations.All` and shoots all six. Every matchup
        /// prices all fifteen kinds at the shipped config, so that walk is a full-vocabulary walk.</para></summary>
        [UnityTest]
        public IEnumerator Capture_the_market_sheet_evidence_for_the_surfaces_spec()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            AssertPinnedSeed(SeedMarketSheet);

            string outputDirectory = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "..", "..", "artifacts", "surething-ui"));
            Directory.CreateDirectory(outputDirectory);
            string runPrefix = DateTime.UtcNow.ToString(
                "yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture);
            var capturedPaths = new List<string>();

            // ---- the shipped sheet ----
            yield return DriveToMarketSheet(laptop, SeedMarketSheet, new RunConfig());

            // FRAMES 1a/1b — the contents block, §8 item 2: every destination, with DERIVED ranges.
            //
            // TWO frames, because twenty-one printed lines do not fit one 378px viewport. The first
            // cut of this shot asserted only that each destination line EXISTED IN THE HIERARCHY,
            // which is not the same claim as being in the picture — it passed on a frame where
            // PLAYERS was below the fold. A capture whose subject is off-screen is not evidence of
            // its subject, so presence is now asserted IN FRAME, and the two shots must between
            // them show all six.
            Invoke(Required(Required(App(laptop), "FolioBand"), "ContentsToggle"));
            yield return WaitForRebuild();
            Transform contents = Required(App(laptop), "ContentsBlock");
            var contentsScroll = contents.GetComponentInChildren<ScrollRect>();
            Assert.IsNotNull(contentsScroll, "the contents list scrolls (§5.4)");

            // World corners are meaningless until the layout has been flushed, and the first cut of
            // this read them straight off WaitForRebuild — so RESULT, which is the very first line
            // of the list and plainly in the picture, recorded as not-in-frame. CaptureState forces
            // the canvases itself before the shutter; the measurement has to do the same or it is
            // measuring a different layout from the one being photographed.
            UnityEngine.Canvas.ForceUpdateCanvases();
            var shown = new HashSet<MarketDestination>();
            var diagnostics = new List<string> { "contents HEAD:" };
            RecordDestinationsInFrame(contents, contentsScroll, shown, diagnostics);
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "S1a-entry-contents-head", capturedPaths);

            contentsScroll.verticalNormalizedPosition = 0f;
            yield return WaitForRebuild();
            UnityEngine.Canvas.ForceUpdateCanvases();
            diagnostics.Add("contents FOOT:");
            RecordDestinationsInFrame(contents, contentsScroll, shown, diagnostics);
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "S1b-entry-contents-foot", capturedPaths);

            foreach (MarketDestination d in MarketDestinations.All)
                Assert.IsTrue(shown.Contains(d),
                    $"{d} never appears IN FRAME across the contents pair — §5.3 prints a "
                    + "destination whether or not it is priced, and a frame that does not show it "
                    + "cannot evidence that. MEASURED: "
                    + string.Join(" · ", diagnostics));

            Invoke(Required(Required(App(laptop), "FolioBand"), "ContentsToggle"));
            yield return WaitForRebuild();

            // FRAME 2 — the folio at a scroll extent, §8 item 4. The frame is worth nothing unless
            // the number MOVED, so that is asserted: a folio reading the same at both extents is
            // authored, which is exactly what §5.1 and S74-am3 forbid.
            Invoke(Required(Required(App(laptop), "MarketDestinations"), "DetailTabPlayers"));
            yield return WaitForRebuild();
            var sheetScroll = Required(App(laptop), "MarketBody")
                .GetComponentInChildren<ScrollRect>();
            Assert.IsNotNull(sheetScroll, "PLAYERS is the deepest list on the sheet and must scroll");
            string folioAtRest = Required(App(laptop), "Folio").GetComponent<TMP_Text>().text;

            sheetScroll.verticalNormalizedPosition = 0f;
            yield return WaitForRebuild();
            UnityEngine.Canvas.ForceUpdateCanvases();
            string folioAtEnd = Required(App(laptop), "Folio").GetComponent<TMP_Text>().text;
            Assert.AreNotEqual(folioAtRest, folioAtEnd,
                $"the folio must be DERIVED from the rendered window — it read '{folioAtRest}' at "
                + $"rest and '{folioAtEnd}' at the extent, and a folio that does not move is a "
                + "constant wearing a fact's clothes (§5.1, S74-am3)");
            Debug.Log($"[surfaces §8] folio at rest '{folioAtRest}' -> at extent '{folioAtEnd}'");
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "S2-entry-folio-at-extent", capturedPaths);

            // FRAME 3 — the empty group. The floor is in the FILENAME, never in the pixels.
            //
            // MULTI SCORER is the LAST group in PLAYERS, behind fourteen ANYTIME SCORER rows, so at
            // rest it sits below the fold. The first cut of this shot asserted the form existed in
            // the hierarchy and passed on a frame that did not contain it. The list is scrolled to
            // its foot and the form's own rect is asserted to be IN the viewport.
            yield return DriveToMarketSheet(laptop, SeedMarketSheet,
                new RunConfig { CorrectScoreFloor = 0.08 });
            Invoke(Required(Required(App(laptop), "MarketDestinations"), "DetailTabPlayers"));
            yield return WaitForRebuild();
            Transform emptyBody = Required(App(laptop), "MarketBody");
            var emptyScroll = emptyBody.GetComponentInChildren<ScrollRect>();
            Assert.IsNotNull(emptyScroll, "PLAYERS scrolls, and the empty group is at its foot");
            emptyScroll.verticalNormalizedPosition = 0f;
            yield return WaitForRebuild();
            UnityEngine.Canvas.ForceUpdateCanvases();

            TMP_Text form = null;
            foreach (TMP_Text t in emptyBody.GetComponentsInChildren<TMP_Text>(true))
                if (t.text == MarketSheet.NoPricesOffered) { form = t; break; }
            Assert.IsNotNull(form,
                $"this frame exists to show S89's '{MarketSheet.NoPricesOffered}' form and the "
                + "state does not print it — the frame would be evidence of nothing");
            Assert.IsTrue(IsInFrame(
                    emptyScroll.viewport != null
                        ? emptyScroll.viewport : (RectTransform)emptyScroll.transform,
                    (RectTransform)form.transform),
                $"'{MarketSheet.NoPricesOffered}' is printed but is NOT IN THE FRAME — the whole "
                + "subject of this capture would be below the fold");
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "S3-entry-empty-group-correctscorefloor-0p08-NOT-SHIPPED", capturedPaths);

            // FRAME 4 — the price ink. ONE frame, not two.
            //
            // This was §4.4's amber COMPARISON: the same sheet shot with `PriceTakesAmber` off and
            // on, so `S91` half two could be decided on the frame rather than argued. That pair was
            // shot, it was read, and S97 (DD batch 113) decided it — THE PRICE STAYS IN TONER. The
            // comparison is DISCHARGED, the switch it was driven by is gone, and a comparison frame
            // for a settled question is not evidence of anything.
            //
            // What survives is the single toner frame, kept because §8 still wants the shipped
            // sheet's price column on the record, and the ink is still ASSERTED rather than assumed
            // — a frame that quietly went amber would now be a regression rather than a variant.
            yield return DriveToMarketSheet(laptop, SeedMarketSheet, new RunConfig());
            yield return WaitForRebuild();
            AssertPriceInk(laptop, LaptopOs.White, "toner");
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "S4-entry-price-ink-toner", capturedPaths);

            // Five states now (was six): the amber half of the price-ink pair retired with S97. The
            // contents block still takes two frames of its own, because its twenty-one lines do not
            // fit one viewport and one frame could only ever have shown part of it.
            AssertCaptureOutput(capturedPaths, 10);
        }

        /// <summary>The row name's own size, from <c>MakeOfferRow</c>'s MakeText call. Whatever a
        /// slot renders with, it measures with — measuring at another size would report a width no
        /// reader will ever see. (<c>MarketRowNameWidthTests.RowNameSize</c> is the same number for
        /// the same reason; both are read against the render below rather than trusted.)</summary>
        private const int WorstCaseRowSize = 19;

        /// <summary>THE row this whole capture exists for, spelled out so the frame cannot silently
        /// catch a different one. Asserted against the engine's own composition path below, not
        /// merely typed — see the flow for why both halves are needed.</summary>
        private const string WorstCaseRowName = "SAN FRANCISCO SPREADSHEETS UNDER 4.5 CORNERS";

        /// <summary>Its width, MEASURED (not asserted from the register): 493.68px in a 496px cell,
        /// 2.32px of headroom. This is the number `C46` reports and the number `S101` exists to
        /// photograph.</summary>
        private const float WorstCaseRowWidth = 493.68f;

        /// <summary>
        /// <c>S101</c> and <c>S102</c>, one forced matchup, two states.
        ///
        /// <para><b>Why this frame exists.</b> The Design Director accepted §4.3's leader-dot
        /// residual FROM A DISTRIBUTION TABLE: 59 rows print fewer than six dots and 5 print none.
        /// Every frame in the docked evidence set is <c>SeedMarketSheet</c>, whose longest row is
        /// <c>MOOSE JAW OVERHEADS OR DRAW</c> — 318.06px with 161.94px spare, the COMFORTABLE case.
        /// The DD's own words: the worst case is a 493.69px name in a 496px cell — the full width,
        /// no leaders, the price immediately after — and nobody has looked at it. <b>It could READ
        /// as a collision while MEASURING as none.</b> That is the whole question, and this is the
        /// only frame that puts it.</para>
        ///
        /// <para><b>The club was MEASURED out of the pool, not assumed.</b> The 16 cities × 20 nouns
        /// were enumerated whole and every one of the 320 clubs measured through the row's own face,
        /// size and tracking. The result corrects a standing assumption: <c>SAN FRANCISCO
        /// SPREADSHEETS</c> is not THE widest club, it is a widest club. <c>SAN FRANCISCO
        /// GRAVEDIGGERS</c> ties it EXACTLY — both nouns sum to 6653 font units, so both clubs
        /// measure 298.38px and both corners rows measure 493.68px, to the last representable
        /// digit. <c>C46</c> reports SPREADSHEETS only because <c>Widest</c> keeps the first of an
        /// equal pair and SPREADSHEETS is the earlier noun in the pool. Either club is the true
        /// worst case; the seed search accepted both and this seed happened to draw SPREADSHEETS.
        /// (If a future pool edit breaks the tie, the width assertion below moves — which is the
        /// point of asserting it.)</para>
        ///
        /// <para><b>Why CORNERS and why UNDER.</b> <c>{CLUB} UNDER 4.5 CORNERS</c> is the widest
        /// form the sheet can print on the widest club: it beats OVER by 12.75px (a five-character
        /// word against a four), TEAM TOTAL CARDS by 26.41px and TEAM TOTAL GOALS by 27.25px, and
        /// the next-widest kind on the same club — <c>{CLUB} OR DRAW</c> — by 103.51px. The 4.5 line
        /// is <c>RunConfig.TeamCornerLines</c>' only member, and the row is asserted PRICED below
        /// rather than assumed from the config.</para>
        ///
        /// <para><b>Judged on the SCROLLING row.</b> CORNERS holds 10 rows in two groups — 592px of
        /// content against a body under 400 — so it overflows, the position rail takes its 8px out
        /// of the 700, and the name cell is 496 rather than 504. The row's own rect is asserted to
        /// be the scrolling width: a frame shot on the fitting case would be photographing 8px of
        /// headroom the player almost never has.</para>
        ///
        /// <para><b>Second state, <c>S102</c>.</b> The contents stutter fix landed on the same
        /// surface and asked to be confirmed on the same shoot, so it is: CORRECT SCORE prints ONCE
        /// in the contents block, its redundant child suppressed entirely (no line, no reserved
        /// gap). Asserted two ways — the child node is gone, AND the printed label appears exactly
        /// once — because either alone would pass on a fix that moved the defect rather than
        /// removing it.</para>
        ///
        /// <para><b><c>C55</c>: both subjects are asserted IN FRAME, not merely present.</b> This
        /// file's own fault promoted that to law — two of the §8 captures passed their first cut on
        /// frames that did not contain their own subject. Both lists are scrolled to CENTRE their
        /// subject (not merely to reveal it at an edge) and both are then judged by
        /// <see cref="IsInFrame"/>, in the viewport's LOCAL space.</para>
        /// </summary>
        [UnityTest]
        public IEnumerator Capture_the_worst_case_row_name_and_the_suppressed_contents_child()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            AssertPinnedSeed(SeedWorstCaseRow);

            string outputDirectory = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "..", "..", "artifacts", "surething-ui"));
            Directory.CreateDirectory(outputDirectory);
            string runPrefix = DateTime.UtcNow.ToString(
                "yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture);
            var capturedPaths = new List<string>();

            yield return DriveToMarketSheet(laptop, SeedWorstCaseRow, new RunConfig());

            // The subject is a fact about the SLATE before it is a fact about the render, so it is
            // established from the engine first. Two halves, and both are load-bearing:
            //
            //   1. the row must be PRICED — a seed that seats the club but does not offer the line
            //      would give a frame of a row that does not exist;
            //   2. the engine's own composed wording must MATCH the literal above — so a reworded
            //      row fails loudly here with both strings, instead of quietly moving this
            //      capture's subject to whatever the sheet now prints.
            Run run = laptop.director.Run;
            Matchup matchup = run.CurrentSlate.Matchups[0];
            MarketSelection worstSelection =
                MarketSelection.TeamTotalCorners(Side.Home, 4.5, over: false);
            bool priced = false;
            foreach (MarketOffer offer in matchup.Markets)
                if (offer.Selection == worstSelection) { priced = true; break; }
            Assert.IsTrue(priced,
                $"'{WorstCaseRowName}' is not PRICED on seed {SeedWorstCaseRow} — RunConfig."
                + "TeamCornerLines no longer holds 4.5, or the slate moved. This capture would be "
                + "a frame of a row that does not exist.");
            string composed = SportsbookApp.PrintedRowName(
                MatchModel.Fields(matchup, worstSelection).Line);
            Assert.AreEqual(WorstCaseRowName, composed,
                $"the engine now composes the home team-total corners row as '{composed}', not "
                + $"'{WorstCaseRowName}'. Either the slate moved off seed {SeedWorstCaseRow}'s "
                + $"{matchup.Away.Name} @ {matchup.Home.Name}, or §3's wording changed — and in "
                + "either case the widest reachable name must be re-measured against the pool "
                + "before this frame means anything.");

            // ── STATE W1 · the worst-case row ────────────────────────────────────────────────────
            Invoke(Required(Required(App(laptop), "MarketDestinations"),
                "DetailTab" + MarketDestination.Corners));
            yield return WaitForRebuild();
            UnityEngine.Canvas.ForceUpdateCanvases();

            Transform body = Required(App(laptop), "MarketBody");
            var sheetScroll = body.GetComponentInChildren<ScrollRect>();
            Assert.IsNotNull(sheetScroll,
                "CORNERS overflows the market body at the shipped config (10 rows in two groups), "
                + "so it must scroll — and the 496px name cell this frame is judged against is the "
                + "SCROLLING one. A CORNERS that fits would mean the geometry moved.");

            TMP_Text worst = null;
            var widestRival = 0f;
            string widestRivalName = null;
            foreach (TMP_Text t in body.GetComponentsInChildren<TMP_Text>(true))
            {
                if (!t.name.StartsWith("MarketLabel", StringComparison.Ordinal)) continue;
                if (t.text == WorstCaseRowName) { worst = t; continue; }
                float w = LaptopUi.MeasureWidth(t.font, t.text, WorstCaseRowSize, LaptopTrack.Records);
                if (w <= widestRival) continue;
                widestRival = w;
                widestRivalName = t.text;
            }
            Assert.IsNotNull(worst,
                $"'{WorstCaseRowName}' is not printed on the CORNERS sheet — the frame would show a "
                + "merely-long row and evidence nothing about the residual the DD accepted");

            // Whatever it renders with, it is measured with. Asserting the size against the render
            // is what stops this becoming a measurement of a row nobody sees.
            Assert.AreEqual(WorstCaseRowSize, worst.fontSize, 0.01f,
                "the row name's rendered size moved away from MakeOfferRow's 19px, so every width "
                + "below is a measurement of a different row than the one in the picture");
            float measured = LaptopUi.MeasureWidth(
                worst.font, worst.text, WorstCaseRowSize, LaptopTrack.Records);

            // The row this frame is shot on must be the SCROLLING row: the narrower cell is the
            // worst case, and the fitting row would quietly hand it 8px it does not have.
            RectTransform row = null;
            for (Transform t = worst.transform.parent; t != null; t = t.parent)
                if (t.name.StartsWith("MarketOffer", StringComparison.Ordinal))
                { row = (RectTransform)t; break; }
            Assert.IsNotNull(row, "the worst-case name is not inside an offer row");
            Assert.AreEqual(SportsbookApp.ScrollingOfferRowWidth, row.rect.width, 0.5f,
                "this frame must be shot on the SCROLLING row — the position rail's 8px is what "
                + "makes 496 the cell rather than 504, and the fitting case is one the player "
                + "almost never sees");
            float cell = SportsbookApp.OfferNameCellWidth(row.rect.width);

            Assert.AreEqual(WorstCaseRowWidth, measured, 0.25f,
                $"'{WorstCaseRowName}' measures {measured.ToString("0.##", CultureInfo.InvariantCulture)}px, "
                + $"not the {WorstCaseRowWidth.ToString("0.##", CultureInfo.InvariantCulture)}px this "
                + "frame is captioned with. The pool, the face, the size or the tracking moved — "
                + "re-measure the 320 clubs before shooting, because the widest may no longer be "
                + "this one (SAN FRANCISCO SPREADSHEETS and SAN FRANCISCO GRAVEDIGGERS tie exactly "
                + "at the shipped pool, and a tie is one edit away from becoming a change).");
            Assert.LessOrEqual(measured, cell,
                "C46 restated at the frame: the worst-case row name does not fit its cell, so this "
                + "capture would be photographing a defect rather than the accepted residual");

            // The one thing on this sheet that could make the frame lie: a row WIDER than the
            // subject would mean the picture's worst case is not the row named in its filename.
            Assert.Less(widestRival, measured,
                $"'{widestRivalName}' measures {widestRival.ToString("0.##", CultureInfo.InvariantCulture)}px "
                + $"against the subject's {measured.ToString("0.##", CultureInfo.InvariantCulture)}px "
                + "— the frame's own filename would name the wrong row as the worst case");

            // §4.3 IN PIXELS — the DD's actual question. `nameEnd` is where MakeOfferRow starts the
            // leaders (the rendered end of the type, not of the cell) and `priceX` is the price
            // cell's left edge. MakeLeaders emits NO node at all when there is no room, so an
            // absent OfferLeaders is the five-rows-print-none case, photographed.
            float nameEnd = SportsbookApp.OfferLeftPad + worst.preferredWidth;
            float priceX = row.rect.width - SportsbookApp.OfferRightPad
                - SportsbookApp.OfferPriceCellWidth;
            string dots = null;
            foreach (TMP_Text t in row.GetComponentsInChildren<TMP_Text>(true))
                if (t.name.StartsWith("OfferLeaders", StringComparison.Ordinal))
                { dots = t.text; break; }
            Debug.Log($"[S101] '{WorstCaseRowName}' measured "
                + measured.ToString("0.##", CultureInfo.InvariantCulture) + "px in a "
                + cell.ToString("0.##", CultureInfo.InvariantCulture) + "px cell on a "
                + row.rect.width.ToString("0.##", CultureInfo.InvariantCulture)
                + "px row · headroom " + (cell - measured).ToString("0.##", CultureInfo.InvariantCulture)
                + "px · name ends at " + nameEnd.ToString("0.##", CultureInfo.InvariantCulture)
                + ", price cell begins at " + priceX.ToString("0.##", CultureInfo.InvariantCulture)
                + " · leader dots printed: " + (dots == null ? "NONE (no node)" : dots.Length.ToString(
                    CultureInfo.InvariantCulture)));
            Assert.Less(nameEnd, priceX,
                "the printed name OVERRUNS the price cell — that is a collision by MEASURE, not the "
                + "residual the DD accepted, and it is a Design Director matter rather than a frame");

            yield return ScrollIntoFrame(sheetScroll, row, $"the '{WorstCaseRowName}' row");
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "W1-entry-worst-case-row-493p68-in-a-496px-cell", capturedPaths);

            // ── STATE W2 · S102's suppressed contents child ──────────────────────────────────────
            Invoke(Required(Required(App(laptop), "FolioBand"), "ContentsToggle"));
            yield return WaitForRebuild();
            UnityEngine.Canvas.ForceUpdateCanvases();

            Transform contents = Required(App(laptop), "ContentsBlock");
            var contentsScroll = contents.GetComponentInChildren<ScrollRect>();
            Assert.IsNotNull(contentsScroll, "the contents list scrolls (§5.4)");

            Transform correctScore = Required(contents,
                "ContentsDestination" + MarketDestination.CorrectScore);
            Assert.IsNull(Find(contents, "ContentsKind" + MarketKind.CorrectScore),
                "S102: CORRECT SCORE's redundant child line is still in the tree. The ruling is "
                + "that it is suppressed ENTIRELY — no line, no reserved gap — so a child that is "
                + "merely blank or merely transparent is the defect wearing a fix's clothes.");

            // The second half, stated as behaviour rather than as a node name: whatever the fix is
            // implemented as, the printed page must name CORRECT SCORE once. Counting catches a fix
            // that suppressed the wrong line just as surely as one that suppressed nothing.
            string label = MarketDestinations.Label(MarketDestination.CorrectScore);
            int printed = 0;
            foreach (TMP_Text t in contents.GetComponentsInChildren<TMP_Text>(true))
                if (t.name == "ContentsLabel" && t.text == label) printed++;
            Assert.AreEqual(1, printed,
                $"S102: the contents block prints '{label}' {printed} times. Once is the ruling — "
                + "twice is the stutter this frame exists to show closed, and none would mean the "
                + "destination line itself was suppressed, which §5.3 forbids.");

            yield return ScrollIntoFrame(contentsScroll, (RectTransform)correctScore,
                "the CORRECT SCORE contents line");
            yield return CaptureState(laptop, outputDirectory, runPrefix,
                "W2-entry-contents-correct-score-child-suppressed", capturedPaths);

            AssertCaptureOutput(capturedPaths, 4);
        }

        /// <summary>Scrolls <paramref name="scroll"/> until <paramref name="target"/> is CENTRED in
        /// the viewport, then proves it is actually in the picture.
        ///
        /// <para><c>C55</c> requires the subject to be in frame, and "scroll to an extent and hope"
        /// is how two of the §8 captures came to be shot on frames that did not contain their own
        /// subject. Centring rather than merely revealing is deliberate: a subject clinging to the
        /// top or bottom edge is in frame by the letter and unreadable by the eye.</para>
        ///
        /// <para>The placement is arithmetic and the verdict is not. The analytic position is
        /// computed in the CONTENT's own local space — the authored-pixel space the rows were laid
        /// out in — and then <see cref="IsInFrame"/> is asked, in the VIEWPORT's local space,
        /// whether it worked. If it did not, the list is swept and the assertion still has to pass:
        /// a placement that silently missed would leave the flow shooting exactly as before while
        /// every comment here claimed otherwise.</para></summary>
        private static IEnumerator ScrollIntoFrame(ScrollRect scroll, RectTransform target,
            string what)
        {
            RectTransform viewport = scroll.viewport != null
                ? scroll.viewport
                : (RectTransform)scroll.transform;
            RectTransform content = scroll.content;
            Assert.IsNotNull(content, $"{what}: the list has no scroll content to move");

            yield return WaitForRebuild();
            UnityEngine.Canvas.ForceUpdateCanvases();

            float viewHeight = viewport.rect.height;
            float travel = content.rect.height - viewHeight;
            if (travel > 0.5f)
            {
                var corners = new Vector3[4];
                target.GetWorldCorners(corners);
                float min = float.MaxValue, max = float.MinValue;
                for (int i = 0; i < corners.Length; i++)
                {
                    float y = content.InverseTransformPoint(corners[i]).y;
                    min = Mathf.Min(min, y);
                    max = Mathf.Max(max, y);
                }
                float centreFromTop = content.rect.yMax - (min + max) * 0.5f;
                float top = Mathf.Clamp(centreFromTop - viewHeight * 0.5f, 0f, travel);
                scroll.verticalNormalizedPosition = 1f - top / travel;
                yield return WaitForRebuild();
                UnityEngine.Canvas.ForceUpdateCanvases();
            }

            if (!IsInFrame(viewport, target))
                for (int step = 0; step <= 40; step++)
                {
                    scroll.verticalNormalizedPosition = 1f - step / 40f;
                    yield return WaitForRebuild();
                    UnityEngine.Canvas.ForceUpdateCanvases();
                    if (IsInFrame(viewport, target)) break;
                }

            Assert.IsTrue(IsInFrame(viewport, target),
                $"{what} could not be brought INTO THE FRAME — it measures "
                + LocalExtent(viewport, target) + " against a viewport of "
                + $"{viewport.rect.yMin:0.0}..{viewport.rect.yMax:0.0}. A capture whose subject is "
                + "off-screen is not evidence of its subject (C55).");
            Debug.Log($"[C55] {what} is IN FRAME at {LocalExtent(viewport, target)} "
                + $"(viewport {viewport.rect.yMin:0.0}..{viewport.rect.yMax:0.0})");
        }

        /// <summary>Whether <paramref name="target"/> is wholly inside <paramref name="viewport"/>
        /// vertically and overlaps it horizontally — i.e. whether a reader would actually SEE it in
        /// the frame.
        ///
        /// <para>This exists because `GetComponentsInChildren` finds rows that scrolled out of view,
        /// and two of this set's captures passed their first cut on frames that did not contain
        /// their own subject. Existence in the hierarchy is not the claim a capture makes.</para></summary>
        private static bool IsInFrame(RectTransform viewport, RectTransform target)
        {
            // MEASURED IN THE VIEWPORT'S OWN LOCAL SPACE, not in world space.
            //
            // The first cut compared world-space corners and rejected every line on a list that was
            // plainly in the picture. This is a WORLD-SPACE canvas on a physical laptop in a room:
            // the whole 704px viewport spans about 0.1 world units, so every row rounds to the same
            // two digits — and the screen is TILTED, so a world-space x comparison is being taken
            // across a rotated plane and means nothing. Local space is the space the layout was
            // authored in, and its units are the pixels every other constant in this build is
            // written in.
            //
            // Vertical only, deliberately: these lists clip vertically, the lines span the block's
            // width by construction, and the horizontal term is what was producing the false
            // negative rather than catching anything.
            var corners = new Vector3[4];
            target.GetWorldCorners(corners);
            float min = float.MaxValue, max = float.MinValue;
            for (int i = 0; i < corners.Length; i++)
            {
                float y = viewport.InverseTransformPoint(corners[i]).y;
                min = Mathf.Min(min, y);
                max = Mathf.Max(max, y);
            }
            Rect r = viewport.rect;
            return min >= r.yMin - 0.5f && max <= r.yMax + 0.5f;
        }

        /// <summary>The vertical extent of <paramref name="target"/> in <paramref name="viewport"/>'s
        /// local space, for diagnostics — the same space <see cref="IsInFrame"/> judges in, so a
        /// printed number and a verdict can never disagree.</summary>
        private static string LocalExtent(RectTransform viewport, RectTransform target)
        {
            var corners = new Vector3[4];
            target.GetWorldCorners(corners);
            float min = float.MaxValue, max = float.MinValue;
            for (int i = 0; i < corners.Length; i++)
            {
                float y = viewport.InverseTransformPoint(corners[i]).y;
                min = Mathf.Min(min, y);
                max = Mathf.Max(max, y);
            }
            return $"{min:0.0}..{max:0.0}";
        }

        /// <summary>Adds every destination whose contents line is IN FRAME to
        /// <paramref name="shown"/>. Called once per contents shot so the pair can be required to
        /// cover all six between them.</summary>
        private static void RecordDestinationsInFrame(Transform contents, ScrollRect scroll,
            ISet<MarketDestination> shown, IList<string> diagnostics)
        {
            RectTransform viewport = scroll.viewport != null
                ? scroll.viewport
                : (RectTransform)scroll.transform;
            Rect r = viewport.rect;
            diagnostics.Add($"  viewport '{viewport.name}' local y {r.yMin:0.0}..{r.yMax:0.0}");
            foreach (MarketDestination d in MarketDestinations.All)
            {
                Transform line = Find(contents, "ContentsDestination" + d);
                if (line == null)
                {
                    diagnostics.Add($"    {d}: NOT FOUND in the tree");
                    continue;
                }
                bool inFrame = IsInFrame(viewport, (RectTransform)line);
                if (inFrame) shown.Add(d);
                diagnostics.Add($"    {d}: {LocalExtent(viewport, (RectTransform)line)} -> "
                    + (inFrame ? "IN FRAME" : "out"));
            }
        }

        /// <summary>Puts a pinned run under the laptop and opens ENTRY on matchup 0. Split out
        /// because the §8 set drives it three times with two different configs, and a capture whose
        /// states were reached by three hand-copied sequences is a capture whose states differ in
        /// ways nobody wrote down.
        ///
        /// <para><b>The seed is a PARAMETER, not <c>SeedMarketSheet</c> baked in.</b> `S101`'s
        /// worst-case frame is taken on the same ENTRY surface by the same sequence but on a
        /// deliberately different slate, and re-typing the four lines for it would reintroduce
        /// exactly the hand-copied divergence this helper exists to prevent.</para></summary>
        private static IEnumerator DriveToMarketSheet(LaptopScreen laptop, string seed,
            RunConfig config)
        {
            laptop.director.StartNewRun(seed);
            SetDirectorRun(laptop.director, new Run(seed, config));
            AssertShootingSeed(laptop, seed);
            yield return WaitForRebuild();

            Invoke(Required(Required(App(laptop), "Matchup0"), "Details"));
            yield return WaitForRebuild();
            Assert.AreEqual(SportsbookApp.Tab.Detail, laptop.Os.CurrentTab,
                "the market sheet is ENTRY, and every §8 frame is taken on it");
        }

        /// <summary>Reads the ink actually on a price cell. This began as §4.4's guard — a
        /// comparison is worth nothing if both frames came out the same colour — and outlived the
        /// comparison: with `S97` closing the question in favour of toner, the same read is now a
        /// REGRESSION check on the shipped ink rather than a check that a switch reached the
        /// render.</summary>
        private static void AssertPriceInk(LaptopScreen laptop, Color expected, string what)
        {
            Button price = FirstNamedButton(Required(App(laptop), "MarketBody"), "Market");
            Assert.IsNotNull(price, "the sheet must have a price cell for the ink to be read from");
            var text = price.GetComponentInChildren<TMP_Text>();
            Assert.IsNotNull(text, "the price cell must carry its figure");
            Assert.AreEqual(expected, text.color,
                $"the {what} half of §4.4's comparison did not reach the render — both frames would "
                + "be the same sheet and the comparison would answer nothing");
        }

        /// <summary>S83's scroll, on the state that ACTUALLY SCROLLS (DD batch 81).
        ///
        /// <para><b>RE-CUT after the nudge row was deleted (S82-am2/S80-am2-cl2, batch 107).</b> The
        /// state this captured no longer scrolls, and that is the deletion working rather than a
        /// regression: B returned 32px to the flow, so `SlipViewportHeight` went 177.9 → 209.9 and
        /// the old subject — four legs plus a held consumable, 202.0 of content — now CLEARS by
        /// 7.9px instead of scrolling by 24.1.</para>
        ///
        /// <para>That 7.9 is the register's own "roughly 8px standing" (`S80-am2-cl2`: 404.10 −
        /// 10.00 − 32 = 362.10 against a 370 budget) arrived at from the other end — the first
        /// independent confirmation of that arithmetic, since 404.10 is bookkeeping that cannot be
        /// re-derived from the layout constants alone.</para>
        ///
        /// <para><b>So the state is heavier now: four legs, a held consumable, AND the relation
        /// statement.</b> S80-am's measured bill was 4 legs +0.1 · +consumable +34.1 · +statement
        /// +36.1 · +both +70.1 against the budget; A and B together returned 42, so BOTH is the one
        /// case still over — by ~28.1px. Two of the four legs are therefore a same-match pair that
        /// states a relation, which is an ordinary shipped slip, not a rig.</para>
        ///
        /// <para><b>This is why taking B did not retire C</b> (`S82-am2` says so in words; this
        /// says it in pixels). The flow still overflows at its heaviest reachable state, so option
        /// C — scrolling the flow region — still has a live case.</para>
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
            BetslipModel slip = laptop.Slip;

            // Two of the four legs must be a same-match pair that STATES a relation, because the
            // statement's 36.1px is what still carries this state over the reclaimed viewport.
            // Searched rather than authored: which pairs correlate is a property of a board that is
            // re-priced every boot, and S79 records that 46.1% of same-match slips correctly state
            // nothing — so the first pair found is usually not one of them.
            MarketSelection selA = default, selB = default;
            bool found = false;
            IReadOnlyList<MarketOffer> offers = run.CurrentSlate.Matchups[0].Markets;
            for (int a = 0; a < offers.Count && !found; a++)
                for (int b = a + 1; b < offers.Count; b++)
                {
                    slip.Clear();
                    if (!slip.AddLeg(0, offers[a].Selection)) continue;
                    if (!slip.AddLeg(0, offers[b].Selection)) continue;
                    if (slip.Refusal != null) continue;
                    if (SportsbookApp.RelationStatement(slip.SameMatchPricing, slip.Picks) == null)
                        continue;
                    selA = offers[a].Selection;
                    selB = offers[b].Selection;
                    found = true;
                    break;
                }
            Assert.IsTrue(found,
                "no same-match pair on matchup 0 states a relation, so the heaviest reachable flow "
                + "cannot be built and this capture cannot mean what it claims");

            // That search churned thousands of two-leg slips with no frame between them, and the OS
            // rebuilds off a SIGNATURE — so clear to a state that cannot share one with the pair
            // below, let it draw, then build the real slip.
            slip.Clear();
            yield return WaitForRebuild();
            Assert.IsTrue(slip.AddLeg(0, selA), "leg 1 of the stating same-match pair");
            Assert.IsTrue(slip.AddLeg(0, selB), "leg 2 of the stating same-match pair");
            for (int i = 1; slip.Picks.Count < maxLegs; i++)
                Assert.IsTrue(slip.AddLeg(i, MarketSelection.Moneyline(Side.Away)),
                    $"leg on matchup {i} to fill the slip to its cap");
            yield return WaitForRebuild();
            Assert.AreEqual(maxLegs, slip.Picks.Count, "the captured state must be a full slip");

            // The frame is only worth docking if it IS the scrolling state, so that is asserted
            // rather than assumed — a capture that silently caught the non-scrolling case would be
            // exactly the evidence the DD said proves nothing.
            Transform margin = Required(App(laptop), "WorkingMargin");
            Assert.IsNotNull(Find(margin, "RelationStatement"),
                "the statement is the 36.1px this state now depends on — without it the flow fits");
            var scroll = Required(margin, "SlipScroll").GetComponent<ScrollRect>();

            // MEASURED, not inferred. The old numbers in this file's summary were invalidated by a
            // 32px reclaim, and the replacements are printed rather than asserted so the next reader
            // gets the figures from the run instead of from a comment that may have gone stale the
            // same way.
            Debug.Log($"[S83 flow] content {scroll.content.rect.height:0.0} into viewport "
                + $"{SportsbookApp.SlipViewportHeight:0.0} — over by "
                + $"{scroll.content.rect.height - SportsbookApp.SlipViewportHeight:0.0}px");

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

        /// <summary>T164: the mirror's pregame seed is the TICKET's probability, never a leg's —
        /// the product of the legs' TrueProb, which IS SweatSession.TicketWinProbability at t=0.
        /// <see cref="RevealedView.Reset"/> takes it as an argument because the view holds no
        /// session handle. The honest value matters here specifically: these are the SIX TRUTHFUL
        /// states, and a state seeded with a number the engine would never produce is not one.</summary>
        private static double TicketProbAtStart(Ticket ticket)
        {
            double p = 1.0;
            foreach (Leg leg in ticket.Legs) p *= leg.TrueProb;
            return p;
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
