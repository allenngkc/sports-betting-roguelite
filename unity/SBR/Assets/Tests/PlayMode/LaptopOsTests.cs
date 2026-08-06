using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace SBR.Tests.PlayMode
{
    /// <summary>
    /// F_0.3.0: the laptop OS boots inside the room, apps and tabs switch, phase defaults
    /// land where the plan says, and the causal mirror advances ONLY with the TV's own
    /// presentation — nothing revealed while the couch is empty, terminal truth only after
    /// the round settles. All waits are wall-clock (batch runs unthrottled).
    /// </summary>
    public class LaptopOsTests
    {
        [UnityTest]
        public IEnumerator Os_boots_switches_apps_and_keeps_chrome_over_every_tab()
        {
            yield return LoadRoom();
            var director = UnityEngine.Object.FindAnyObjectByType<RunDirector>();
            var laptop = UnityEngine.Object.FindAnyObjectByType<LaptopScreen>();
            var tv = UnityEngine.Object.FindAnyObjectByType<TvSweatScreen>();
            Assert.IsNotNull(laptop, "LaptopScreen missing");
            Assert.IsNotNull(laptop.tv, "the laptop's TV reference must self-heal at runtime");
            Assert.AreSame(tv, laptop.tv, "one TV, one mirror source");
            yield return WaitUntil(() => director.Run != null, 10f, "no run");
            yield return null; // one OS tick

            // Phase default: a fresh run opens in Betting → SureThing LOBBY.
            Assert.IsFalse(laptop.Os.OnDesktop, "Betting boots straight into the book");
            Assert.AreEqual(SportsbookApp.Tab.Lobby, laptop.Os.CurrentTab);

            // Every tab keeps the chrome header visible (Sol finding 1 regression pin):
            // the header must exist and no full-screen panel may follow it as a sibling.
            foreach (SportsbookApp.Tab tab in new[]
                { SportsbookApp.Tab.Lobby, SportsbookApp.Tab.MyBets, SportsbookApp.Tab.Rewards })
            {
                laptop.Os.OpenSportsbook(tab);
                yield return null;
                Transform app = FindDeep(laptop.transform, "App");
                Assert.IsNotNull(app, $"{tab}: app root missing");
                Transform chrome = app.Find("Chrome");
                Assert.IsNotNull(chrome, $"{tab}: the chrome header must render on every tab");
                Assert.IsNull(app.Find("MirrorPanel"), $"{tab}: no chrome-burying panel may exist");
            }

            // Desktop round-trip.
            laptop.Os.OpenDesktop();
            yield return null;
            Assert.IsTrue(laptop.Os.OnDesktop);
            Assert.IsNotNull(FindDeep(laptop.transform, "Wallpaper"), "the wallpaper child exists");
            laptop.Os.OpenSportsbook(SportsbookApp.Tab.Lobby);
        }

        [UnityTest]
        public IEnumerator Mirror_reveals_nothing_unseated_and_lands_engine_truth_at_settle()
        {
            yield return LoadRoom();
            var director = UnityEngine.Object.FindAnyObjectByType<RunDirector>();
            var screen = UnityEngine.Object.FindAnyObjectByType<TvSweatScreen>();
            var couch = UnityEngine.Object.FindAnyObjectByType<SitSpot>();
            Assert.IsNotNull(director);
            Assert.IsNotNull(screen);
            screen.TimeScaleOverride = 0.0001f;
            if (couch != null) couch.transitionDuration = 0.01f;
            yield return WaitUntil(() => director.Run != null, 10f, "no run");
            Run run = director.Run;

            (IReadOnlyList<Pick> picks, double stake) = DemoTicketPolicy.Choose(run);
            run.PlaceTicket(picks, stake);
            director.LockRound();
            Assert.AreEqual(Phase.Sweat, run.Phase);

            // Nobody is seated: the TV cannot play, so the mirror must hold at pregame —
            // ticket visible (identity is lock-public), clock PRE, and NO leg resolved.
            yield return WaitUntil(() => screen.RevealedView.HasTicket, 10f, "pregame mirror never armed");
            yield return WaitRealtime(0.4f);
            RevealedView view = screen.RevealedView;
            Assert.AreEqual("PRE", view.ClockText, "unseated: the clock must still read PRE");
            double anchor = run.Tickets[0].Legs[0].TrueProb;
            Assert.AreEqual((float)anchor, view.WinProbability, 1e-4f,
                "unseated: the mirror's prob is the pregame anchor, nothing more");
            foreach (RevealedLeg leg in view.Tickets[0].Legs)
                Assert.IsTrue(leg.State == RevealedLegState.Pending || leg.State == RevealedLegState.Live,
                    "unseated: no outcome may be revealed anywhere");

            // Sit at a HUMAN-ish pace first: the hard causal interval (Sol) — during a
            // suspended dangerous scene the engine has already consumed the beat, but the
            // mirror's number must HOLD at the preceding payoff until the scene reveals.
            var laptop = UnityEngine.Object.FindAnyObjectByType<LaptopScreen>();
            screen.TimeScaleOverride = 0.2f;
            screen.ForceSeated(true);
            yield return WaitUntil(() => view.MarketSuspended, 30f, "no dangerous scene ever suspended");
            Assert.GreaterOrEqual(screen.EventsEmitted, 1, "suspension implies a consumed beat");
            Assert.AreEqual(SportsbookApp.Tab.MyBets, laptop.Os.CurrentTab,
                "the Sweat phase defaults the book to MY BETS");
            float heldProb = view.WinProbability;
            float w0 = Time.realtimeSinceStartup;
            while (view.MarketSuspended && Time.realtimeSinceStartup - w0 < 10f)
            {
                Assert.AreEqual(heldProb, view.WinProbability, 1e-5f,
                    "the mirror's number must hold through the suspended scene — the reveal owns it");
                yield return null;
            }
            Assert.IsFalse(view.MarketSuspended, "the payoff must eventually release the market");

            // Fast-forward the rest to settle, snapshotting the mirror on the way (it is
            // cleared the moment the TV leaves the sweat — finding 2).
            screen.TimeScaleOverride = 0.0001f;
            var lastStates = new Dictionary<int, RevealedTicketState>();
            float start = Time.realtimeSinceStartup;
            while (run.Phase == Phase.Sweat)
            {
                if (view.Tickets.Count > 0)
                    foreach (RevealedTicket rt in view.Tickets)
                        lastStates[rt.Index] = rt.State;
                if (Time.realtimeSinceStartup - start > 60f)
                {
                    Assert.Fail("the round never settled (waited 60s)");
                    yield break;
                }
                yield return null;
            }

            Assert.Greater(lastStates.Count, 0, "the mirror must have presented the ticket");
            for (int i = 0; i < run.Tickets.Count; i++)
            {
                if (!lastStates.TryGetValue(i, out RevealedTicketState mirrored)) continue;
                Ticket t = run.Tickets[i];
                bool matches =
                    (t.State == TicketState.CashedOut && mirrored == RevealedTicketState.CashedOut) ||
                    (t.State == TicketState.Lost && mirrored == RevealedTicketState.Lost) ||
                    (t.State != TicketState.Lost && t.State != TicketState.CashedOut
                        && mirrored == RevealedTicketState.Won);
                Assert.IsTrue(matches, $"mirror state {mirrored} must match engine {t.State}");
            }

            // And the finding-2 pin itself: once the TV idles into the shop, the mirror is empty.
            yield return null;
            yield return null;
            Assert.IsFalse(view.HasTicket, "leaving the sweat must clear the mirror");
            if (run.Phase == Phase.Shop)
                Assert.AreEqual(SportsbookApp.Tab.Rewards, laptop.Os.CurrentTab,
                    "the Shop phase defaults the book to REWARDS");
        }

        /// <summary>
        /// S46: one name, SURETHING, everywhere the player sees it. Before this ruling the same app
        /// was called four things on one machine — `Sportsbook` under the desktop icon, `SURETHING.`
        /// in the taskbar, `SURETHING` in the tray, `SURETHING FORM` in the masthead — and a fifth,
        /// `SureThing.`, on the verdict screen.
        ///
        /// This asserts the three the player can reach without ending a run. The verdict screen is
        /// covered by the source guard in SureThingNameTests instead: reaching it here means driving
        /// a run to RunWon/RunLost, which is a far more expensive gate than the defect deserves.
        ///
        /// The retired-spelling sweep does not match the desktop wordmark's `SURE` + `THING.` pair.
        /// That was deliberate when it was written — the pair was two Text objects rather than one
        /// string, and deleting it was S44's ruling, not this one's. S44 has since deleted it
        /// (Desktop_wears_no_house_brand_and_no_biro holds that gate now), so the exclusion is
        /// historical; it stays because a name split across two Texts is still not a spelling this
        /// test can honestly claim to catch.
        /// </summary>
        [UnityTest]
        public IEnumerator One_name_on_every_destination_the_player_can_reach()
        {
            yield return LoadRoom();
            var director = UnityEngine.Object.FindAnyObjectByType<RunDirector>();
            var laptop = UnityEngine.Object.FindAnyObjectByType<LaptopScreen>();
            Assert.IsNotNull(laptop, "LaptopScreen missing");
            yield return WaitUntil(() => director.Run != null, 10f, "no run");
            yield return null; // one OS tick

            // The desktop: the icon's caption, and the taskbar's list of the machine's apps.
            laptop.Os.OpenDesktop();
            yield return null;
            Transform desktop = FindDeep(laptop.transform, "Desktop");
            Assert.IsNotNull(desktop, "desktop root missing");
            Assert.AreEqual("SURETHING", TextUnder(desktop, "SureThing", "Caption"),
                "the desktop icon names the app SURETHING, not Sportsbook");
            // Until S48 this read a "SURETHING   ·   LEDGER" label on the desktop's own taskbar.
            // The fold replaced that label with the tray's real app slots, so the name is now
            // asserted where it actually lives — and it is the same slot, from the same builder,
            // as the one the in-app assertions below read.
            Transform desktopTray = FindDeep(desktop, "NotebookTray");
            Assert.IsNotNull(desktopTray, "the desktop carries the shared tray (S48)");
            Assert.AreEqual("SURETHING", TextUnder(desktopTray, "SureThing", "Label"),
                "the desktop's tray slot names the app");
            AssertNoRetiredName(laptop.transform, "desktop");

            // Inside the app: the tray slot and the masthead brand, on every tab that carries them.
            foreach (SportsbookApp.Tab tab in new[]
                { SportsbookApp.Tab.Lobby, SportsbookApp.Tab.MyBets, SportsbookApp.Tab.Rewards })
            {
                laptop.Os.OpenSportsbook(tab);
                yield return null;
                Transform app = FindDeep(laptop.transform, "App");
                Assert.IsNotNull(app, $"{tab}: app root missing");
                Transform tray = FindDeep(app, "NotebookTray");
                Assert.IsNotNull(tray, $"{tab}: tray missing");
                Assert.AreEqual("SURETHING", TextUnder(tray, "SureThing", "Label"),
                    $"{tab}: the tray slot names the app");
                Assert.AreEqual("SURETHING", TextOn(FindDeep(app, "Brand")),
                    $"{tab}: the masthead brand is the name and nothing else — FORM is a screen");
                AssertNoRetiredName(laptop.transform, tab.ToString());
            }
        }

        /// <summary>
        /// S44 + S45: the machine does not wear the house's brand, and satire never occupies a slot
        /// where a fact belongs. The wordmark and the tagline are deleted, not restyled and not
        /// softened, and the app's icon glyph leaves the player's ink.
        ///
        /// What this instrument reads (C25): every Graphic parented under Desktop, by its `color`
        /// field, and every Text's string. What it cannot see: LaptopWallpaperGraphic emits its four
        /// corner colours as per-vertex data inside OnPopulateMesh, so the ground's own colours are
        /// invisible to a color-field scan — they are Ink/Surface/SurfaceRaised by construction and
        /// carry no biro, but this test does not prove that. It also does not read the toner grain,
        /// which is parented to the canvas root rather than to Desktop, and it says nothing about
        /// whether the wallpaper draws at all — SureThingVisualCaptureTests holds that gate.
        /// </summary>
        [UnityTest]
        public IEnumerator Desktop_wears_no_house_brand_and_no_biro()
        {
            yield return LoadRoom();
            var director = UnityEngine.Object.FindAnyObjectByType<RunDirector>();
            var laptop = UnityEngine.Object.FindAnyObjectByType<LaptopScreen>();
            Assert.IsNotNull(laptop, "LaptopScreen missing");
            yield return WaitUntil(() => director.Run != null, 10f, "no run");
            yield return null;

            laptop.Os.OpenDesktop();
            yield return null;
            Transform desktop = FindDeep(laptop.transform, "Desktop");
            Assert.IsNotNull(desktop, "desktop root missing");

            // Deleted, not restyled: a wordmark drawn in a quieter ink is still the house's mark.
            foreach (string gone in new[] { "DesktopSure", "DesktopThing", "DesktopTagline" })
                Assert.IsNull(FindDeep(desktop, gone),
                    $"'{gone}' is deleted under S44/S45 — a softer version of it is the same claim");

            // S45 by content as well as by node, since the line could reappear anywhere.
            foreach (Text text in desktop.GetComponentsInChildren<Text>())
                Assert.IsFalse(text.text.ToLowerInvariant().Contains("never lies"),
                    $"S45: '{text.text}' promises the player a guaranteed win. Deleted, not softened.");

            // S44's actual mechanism: the house's brand is not in the player's ink, so no biro on
            // the wallpaper or the icons — including the app icon's glyph (S47 names that as S44's).
            //
            // The shared chrome is excluded, and not for convenience. Since S48 the desktop carries
            // the rail, whose PROPERTY OF NOBODY sticker is biro on purpose: it is the one thing on
            // this machine the player did write, which is the same law reaching the opposite
            // answer. That sticker is S8's and Design-verified; failing it here would be this test
            // overruling a ruling it does not hold.
            Transform railToSkip = FindDeep(desktop, "NotebookRail");
            Transform trayToSkip = FindDeep(desktop, "NotebookTray");
            foreach (Graphic graphic in desktop.GetComponentsInChildren<Graphic>())
            {
                if (railToSkip != null && graphic.transform.IsChildOf(railToSkip)) continue;
                if (trayToSkip != null && graphic.transform.IsChildOf(trayToSkip)) continue;
                Assert.IsFalse(SameInk(graphic.color, LaptopOs.Accent),
                    $"S44: '{graphic.name}' is drawn in biro on the player's own desktop");
                Assert.IsFalse(SameInk(graphic.color, LaptopOs.BiroDeep),
                    $"S44: '{graphic.name}' is drawn in biro on the player's own desktop");
            }

            // And positively, because "not biro" alone would also pass if the glyph vanished
            // entirely. The glyph is the icon button's own "Label" child; "Caption" is the app name.
            // A direct child, not a deep search: since S48 the tray below carries a slot with this
            // same name, and a recursive find would return whichever was built first.
            Transform icon = desktop.Find("SureThing");
            Assert.IsNotNull(icon, "the SureThing icon is missing");
            Transform glyph = icon.Find("Label");
            Assert.IsNotNull(glyph, "the SureThing icon has no glyph");
            Assert.AreEqual("S", glyph.GetComponent<Text>().text, "the icon's glyph");
            Assert.IsTrue(SameInk(glyph.GetComponent<Text>().color, LaptopOs.White),
                "S44/S47: an installed app's glyph is full toner, not the player's biro");
        }

        /// <summary>
        /// S47: installed versus not installed is a two-state vocabulary, not a value. Installed is
        /// a full toner glyph and caption over a --ground-3 chip; not installed is both at
        /// --toner-3 with no chip, and no "(soon)" — the treatment already says it does not open.
        ///
        /// The pairing assertion is the one that matters most: an icon that looks installed and
        /// refuses to open, or opens while dressed as not-installed, is the surface lying about
        /// itself in the exact direction this ruling forbids.
        ///
        /// What this instrument reads (C25): the authored `Graphic.color` of each icon's chip,
        /// glyph and caption, and each Button's `interactable`. What it cannot see: Unity tints a
        /// non-interactable Button's target graphic through the CanvasRenderer rather than through
        /// `Graphic.color`, so the disabled dim is invisible here — it is moot for a chip already
        /// at zero alpha, but this test would not notice if it stopped being moot. It also reads
        /// only the four icons by name; an icon added without a test row is uncovered.
        /// </summary>
        [UnityTest]
        public IEnumerator Desktop_icons_speak_a_two_state_vocabulary()
        {
            yield return LoadRoom();
            var director = UnityEngine.Object.FindAnyObjectByType<RunDirector>();
            var laptop = UnityEngine.Object.FindAnyObjectByType<LaptopScreen>();
            Assert.IsNotNull(laptop, "LaptopScreen missing");
            yield return WaitUntil(() => director.Run != null, 10f, "no run");
            yield return null;

            laptop.Os.OpenDesktop();
            yield return null;
            Transform desktop = FindDeep(laptop.transform, "Desktop");
            Assert.IsNotNull(desktop, "desktop root missing");

            foreach ((string node, bool installed) in new[]
                { ("SureThing", true), ("OldSlips", true), ("Mail", false), ("Bank", false) })
            {
                // Direct child: the tray's slots share two of these names since S48.
                Transform icon = desktop.Find(node);
                Assert.IsNotNull(icon, $"'{node}' icon missing");
                string state = installed ? "installed" : "not installed";
                Color ink = installed ? LaptopOs.White : LaptopOs.Muted;

                Text glyph = icon.Find("Label").GetComponent<Text>();
                Text caption = icon.Find("Caption").GetComponent<Text>();
                Assert.IsTrue(SameInk(glyph.color, ink),
                    $"{node} is {state}, so its glyph is {(installed ? "full toner" : "--toner-3")}");
                Assert.IsTrue(SameInk(caption.color, ink),
                    $"{node} is {state}, so its caption is the same ink as its glyph");

                // S56: the chip is gone for BOTH states. S47 gave installed apps a --ground-3 chip
                // and it measured a 3/255 step against the wallpaper — an element that drew and
                // could not be seen, which is not a channel.
                Image chip = icon.GetComponent<Image>();
                Assert.IsNotNull(chip, $"{node} has no graphic");
                Assert.AreEqual(0f, chip.color.a, 1e-3f,
                    $"{node}: S56 removed the chip — a firmer invisible thing is still invisible");

                // S56: the second channel is a printed word, present on exactly one state. Without
                // it the only thing separating launchable from dead is glyph brightness, and status
                // carried by tone alone — no mark, border, label or position — is banned outright.
                Transform stateLine = icon.Find("State");
                if (installed)
                {
                    Assert.IsNull(stateLine, $"{node} launches, so it states nothing");
                }
                else
                {
                    Assert.IsNotNull(stateLine, $"{node} does not launch and must say so in a word");
                    Assert.AreEqual("NOT INSTALLED", TextOn(stateLine),
                        $"{node}: the machine states what is true, not what is planned");
                }

                Assert.AreEqual(installed, icon.GetComponent<Button>().interactable,
                    $"{node} is dressed as {state} and must behave that way — an icon that does not "
                    + "open reads as not-installed by treatment, so the two may never disagree");

                Assert.IsFalse(caption.text.ToLowerInvariant().Contains("soon"),
                    $"{node}: '(soon)' is deleted — the product does not put its roadmap on his desktop");
                Assert.AreEqual(caption.text.ToUpperInvariant(), caption.text,
                    $"{node}: icon captions take the machine's voice — caps");
            }
        }

        /// <summary>
        /// S48: the desktop carries the same NotebookChrome as every other destination — the 34px
        /// rail and the 34px tray — and the wallpaper is the remainder rather than the whole screen.
        /// Its own 54px taskbar is gone. **This changes a Design-verified surface: S8 returns to
        /// review**, and the frame is the evidence, not this test.
        ///
        /// The last two assertions are the ones that would have caught the defects that made the
        /// fold worth ruling. The clock check pins the machine to one time — the desktop's copy
        /// used to read 03:17 AM while the rail one click away read 02:47. The icon/slot check pins
        /// two controls for the same app to the same destination; before the fold the icon set the
        /// app directly and left the tab alone while the tray slot restored the phase's own tab, so
        /// they would have landed the player in different places on the same screen.
        ///
        /// What this instrument reads (C25): object presence, the two chrome heights, the
        /// wallpaper's insets, both slot buttons' authored ink and interactability, and where two
        /// clicks actually land. What it cannot see: whether any of it is laid out correctly on
        /// screen — nothing here would notice the rail drawing over the first icon, or the tray
        /// clipping a caption. Only the desktop frame shows that, which is exactly why S8's
        /// re-verification is a frame and not a suite.
        /// </summary>
        [UnityTest]
        public IEnumerator Desktop_carries_the_shared_chrome()
        {
            yield return LoadRoom();
            var director = UnityEngine.Object.FindAnyObjectByType<RunDirector>();
            var laptop = UnityEngine.Object.FindAnyObjectByType<LaptopScreen>();
            Assert.IsNotNull(laptop, "LaptopScreen missing");
            yield return WaitUntil(() => director.Run != null, 10f, "no run");
            yield return null;

            laptop.Os.OpenDesktop();
            yield return null;
            Transform desktop = FindDeep(laptop.transform, "Desktop");
            Assert.IsNotNull(desktop, "desktop root missing");

            Transform rail = FindDeep(desktop, "NotebookRail");
            Transform tray = FindDeep(desktop, "NotebookTray");
            Assert.IsNotNull(rail, "the desktop carries the shared rail");
            Assert.IsNotNull(tray, "the desktop carries the shared tray");
            Assert.IsNull(desktop.Find("Taskbar"),
                "the desktop's own taskbar is gone — one chrome, built once, consumed everywhere");

            Assert.AreEqual(NotebookChrome.RailHeight, rail.GetComponent<RectTransform>().sizeDelta.y,
                0.01f, "the rail is the shared 34px on the desktop too");
            Assert.AreEqual(NotebookChrome.TrayHeight, tray.GetComponent<RectTransform>().sizeDelta.y,
                0.01f, "the tray is the shared 34px on the desktop too");

            // The wallpaper resizes to what the chrome leaves, rather than running under it.
            RectTransform wallpaper = FindDeep(desktop, "Wallpaper").GetComponent<RectTransform>();
            Assert.AreEqual(NotebookChrome.TrayHeight, wallpaper.offsetMin.y, 0.01f,
                "the wallpaper stops at the tray");
            Assert.AreEqual(-NotebookChrome.RailHeight, wallpaper.offsetMax.y, 0.01f,
                "the wallpaper stops at the rail");

            // Nothing runs on the desktop, so neither slot reads pressed-in and both launch.
            foreach (string slot in new[] { "SureThing", "Ledger" })
            {
                Transform node = tray.Find(slot);
                Assert.IsNotNull(node, $"the tray has no '{slot}' slot");
                Assert.IsTrue(SameInk(node.GetComponent<Image>().color, LaptopOs.SurfaceRaised),
                    $"{slot}: nothing is running on the desktop, so no slot may read pressed-in");
                Assert.IsTrue(node.GetComponent<Button>().interactable,
                    $"{slot}: a backgrounded app's slot launches it");
            }

            // S52: the icon column starts one standard margin (--st-pad-x) below the rail. It used
            // to start 86px down — the wordmark's space, which S44 deleted the wordmark out of.
            // Asserted against LaptopOs' own constant, not a copy of 14: a test holding its own
            // duplicate of the value it is guarding is how the value drifts in the first place.
            RectTransform firstIcon = desktop.Find("SureThing").GetComponent<RectTransform>();
            Assert.AreEqual(-(NotebookChrome.RailHeight + LaptopOs.DesktopIconMarginY),
                firstIcon.anchoredPosition.y, 0.01f,
                "the icon column starts one standard margin below the rail, not the wordmark's old gap");

            // One machine, one time.
            Assert.AreEqual(NotebookChrome.ClockText, TextOn(rail.Find("Clock")),
                "the desktop's clock is the rail's clock, not a second copy of it");

            // Two controls for one app must land in the same place.
            desktop.Find("SureThing").GetComponent<Button>().onClick.Invoke();
            yield return null;
            Assert.IsFalse(laptop.Os.OnDesktop, "the desktop icon launches the app");
            SportsbookApp.Tab fromIcon = laptop.Os.CurrentTab;

            laptop.Os.OpenDesktop();
            yield return null;
            tray.Find("SureThing").GetComponent<Button>().onClick.Invoke();
            yield return null;
            Assert.IsFalse(laptop.Os.OnDesktop, "the desktop tray slot launches the app");
            Assert.AreEqual(fromIcon, laptop.Os.CurrentTab,
                "the icon and the tray slot are two controls for one app and must agree");
        }

        // ---- helpers ----

        /// <summary>Compares two inks at 8-bit precision — the palette is authored as Color32, so a
        /// float-exact comparison would be asserting against rounding rather than against a token.</summary>
        private static bool SameInk(Color a, Color b)
        {
            Color32 x = a;
            Color32 y = b;
            return x.r == y.r && x.g == y.g && x.b == y.b;
        }

        /// Every spelling of the app's name S46 retired. Each is unambiguous copy: none of them can
        /// occur in a resource path or a GameObject name, so a hit is always a rendered defect.
        private static readonly string[] RetiredNames =
            { "Sportsbook", "SureThing", "SURETHING.", "SURETHING FORM", "Sure Thing", "SURE THING" };

        /// <summary>Reads every Text the player is actually being shown — active objects only, so a
        /// tree that has been cleared but not yet collected cannot fail a live surface.</summary>
        private static void AssertNoRetiredName(Transform root, string where)
        {
            foreach (Text text in root.GetComponentsInChildren<Text>())
            {
                if (string.IsNullOrEmpty(text.text)) continue;
                foreach (string retired in RetiredNames)
                    Assert.IsFalse(text.text.Contains(retired),
                        $"{where}: '{text.text}' on '{text.name}' spells the app's name '{retired}'. "
                        + "S46: one name, SURETHING, everywhere the player sees it.");
            }
        }

        /// <summary>Reads one named text child of one named node. Both the child name and the node
        /// name are required because a desktop icon carries two Texts — MakeButton's "Label", which
        /// is the glyph, and MakeDesktopIcon's "Caption", which is the app's name. Asking for
        /// whichever Text turns up first is how the first draft of this test read the icon's "S"
        /// and reported the app was still called Sportsbook.</summary>
        private static string TextUnder(Transform root, string node, string child)
        {
            Transform slot = FindDeep(root, node);
            Assert.IsNotNull(slot, $"'{node}' missing beneath '{root.name}'");
            Transform found = slot.Find(child);
            Assert.IsNotNull(found, $"'{node}' has no '{child}' child");
            return TextOn(found);
        }

        private static string TextOn(Transform node)
        {
            Assert.IsNotNull(node, "expected a text node, found none");
            Text text = node.GetComponent<Text>();
            Assert.IsNotNull(text, $"'{node.name}' carries no Text");
            return text.text;
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform hit = FindDeep(root.GetChild(i), name);
                if (hit != null) return hit;
            }
            return null;
        }

        private static IEnumerator LoadRoom()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("Room", LoadSceneMode.Single);
            Assert.IsNotNull(load, "Room scene not in build settings - run SBR.GrayboxRoomBuilder.Build first.");
            while (!load.isDone) yield return null;
        }

        private static IEnumerator WaitUntil(Func<bool> cond, float maxSeconds, string failMessage)
        {
            float start = Time.realtimeSinceStartup;
            while (!cond())
            {
                if (Time.realtimeSinceStartup - start > maxSeconds)
                {
                    Assert.Fail($"{failMessage} (waited {maxSeconds}s)");
                    yield break;
                }
                yield return null;
            }
        }

        private static IEnumerator WaitRealtime(float seconds)
        {
            float start = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - start < seconds) yield return null;
        }
    }
}
