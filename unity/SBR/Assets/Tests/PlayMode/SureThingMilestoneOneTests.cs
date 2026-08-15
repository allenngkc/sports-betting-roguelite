using System;
using System.Collections;
using System.Text;
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
    /// <summary>Milestone 1 contract pins for the fixed Direction-1 lobby. These deliberately use
    /// named visual regions and controls, never builder order or child indexes.</summary>
    public class SureThingMilestoneOneTests
    {
        [UnityTest]
        public IEnumerator Form_lobby_uses_fixed_canvas_bands_and_700_324_split()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();

            RectTransform canvas = Find(laptop.transform, "LaptopOsCanvas") as RectTransform;
            AssertRect(canvas, 1024f, 704f, "world-space laptop canvas");

            Transform app = App(laptop);
            AssertRect(Required(app, "Chrome") as RectTransform, 1024f, 140f, "chrome composition");
            AssertRect(Required(app, "NotebookRail") as RectTransform, 1024f, 34f, "OS rail");
            AssertRect(Required(app, "FormTabs") as RectTransform, 1024f, 38f, "tab strip");
            AssertRect(Required(app, "FormMasthead") as RectTransform, 1024f, 68f, "masthead");
            AssertRect(Required(app, "Board") as RectTransform, 700f, 530f, "house form");
            AssertRect(Required(app, "WorkingMargin") as RectTransform, 324f, 530f, "player margin");
            AssertRect(Required(app, "NotebookTray") as RectTransform, 1024f, 34f, "OS tray");
        }

        [UnityTest]
        public IEnumerator Form_chrome_persists_and_exposes_the_personal_os_tabs()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();

            Transform app = App(laptop);
            Assert.IsNotNull(Required(app, "NotebookRail"));
            Assert.IsNotNull(Required(app, "FormTabs"));
            Assert.IsNotNull(Required(app, "FormMasthead"));
            Assert.IsNotNull(Required(app, "NotebookTray"));
            StringAssert.Contains("NOTEBOOK", TextOf(Required(app, "Machine")));
            StringAssert.Contains("PROPERTY OF NOBODY", TextOf(Required(app, "Sticker")));
            StringAssert.Contains("02:47", TextOf(Required(app, "Clock")));
            Assert.IsNotNull(Find(app, "FORM"), "FORM tab missing");
            Assert.IsNotNull(Find(app, "ENTRY"), "ENTRY tab missing");
            Assert.IsNotNull(Find(app, "MY BETS"), "MY BETS tab missing");
            Assert.IsNotNull(Find(app, "REWARDS"), "REWARDS tab missing");

            laptop.Os.OpenSportsbook(SportsbookApp.Tab.MyBets);
            yield return null;
            app = App(laptop);
            Assert.IsNotNull(Required(app, "NotebookRail"));
            Assert.IsNotNull(Required(app, "FormTabs"));
            Assert.IsNotNull(Required(app, "FormMasthead"));
            Assert.IsNotNull(Required(app, "NotebookTray"));
        }

        [UnityTest]
        public IEnumerator Form_select_replace_and_rub_out_follow_the_betslip_model()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            Run run = laptop.director.Run;

            Invoke(Required(Required(App(laptop), "Matchup0"), "AwayOdds"));
            yield return WaitForRebuild();
            Assert.AreEqual(MarketSelection.Moneyline(Side.Away), laptop.Slip.SelectionOn(0));

            Invoke(Required(Required(App(laptop), "Matchup0"), "HomeOdds"));
            yield return WaitForRebuild();
            Assert.AreEqual(1, laptop.Slip.Picks.Count, "replacement must keep one leg per matchup");
            Assert.AreEqual(MarketSelection.Moneyline(Side.Home), laptop.Slip.SelectionOn(0));

            Invoke(Required(Required(App(laptop), "WorkingMargin"), "Remove0"));
            yield return WaitForRebuild();
            Assert.IsNull(laptop.Slip.SelectionOn(0));
            Assert.AreEqual(0, laptop.Slip.Picks.Count);
            Assert.AreEqual(run.Phase, Phase.Betting);
        }

        [UnityTest]
        public IEnumerator Form_place_stages_then_lock_requires_an_empty_working_margin()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            Run run = laptop.director.Run;

            Transform margin = Required(App(laptop), "WorkingMargin");
            Assert.IsFalse(Required(margin, "Place").GetComponent<Button>().interactable);
            StringAssert.Contains("PICK A SIDE", TextOf(Required(margin, "PlaceReason")));
            Assert.IsFalse(Required(margin, "Lock").GetComponent<Button>().interactable);
            Assert.AreEqual("PLACE AT LEAST ONE TICKET", TextOf(Required(margin, "LockReason")));

            Invoke(Required(Required(App(laptop), "Matchup0"), "AwayOdds"));
            yield return WaitForRebuild();
            margin = Required(App(laptop), "WorkingMargin");
            Assert.IsFalse(Required(margin, "Lock").GetComponent<Button>().interactable);
            Assert.AreEqual("PLACE OR CLEAR THIS WORKING SLIP", TextOf(Required(margin, "LockReason")));

            Invoke(Required(margin, "Place"));
            yield return WaitForRebuild();
            margin = Required(App(laptop), "WorkingMargin");
            Assert.AreEqual(1, run.Tickets.Count);
            Assert.AreEqual(0, laptop.Slip.Picks.Count);
            Assert.IsTrue(Required(margin, "Lock").GetComponent<Button>().interactable);

            Invoke(Required(margin, "Lock"));
            yield return WaitForRebuild();
            Assert.AreNotEqual(Phase.Betting, run.Phase, "LOCK must commit the staged ticket's round");
        }

        [UnityTest]
        public IEnumerator Form_skip_needs_two_presses_before_committing_an_empty_round()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            Run run = laptop.director.Run;
            Transform margin = Required(App(laptop), "WorkingMargin");

            Invoke(Required(margin, "Skip"));
            yield return WaitForRebuild();
            Assert.AreEqual(Phase.Betting, run.Phase);
            margin = Required(App(laptop), "WorkingMargin");
            Assert.AreEqual("PRESS AGAIN TO SKIP", TextOf(Required(margin, "Skip")));

            Invoke(Required(margin, "Skip"));
            yield return WaitForRebuild();
            Assert.AreNotEqual(Phase.Betting, run.Phase, "second skip press must commit the round");
        }

        [UnityTest]
        public IEnumerator Form_product_text_and_controls_meet_the_contract_floor()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            Transform app = App(laptop);
            Transform margin = Required(app, "WorkingMargin");
            var failures = new StringBuilder();

            AssertTextFloor(Required(app, "BoardTitle"), 13, failures);
            // The margin's "Rule" status line was DELETED by S50 §1 — it restated the board header,
            // which S37 forbids. Its fact-floor duty passes to the header count, which is the
            // margin's other 13px product-fact line.
            AssertTextFloor(Required(margin, "Count"), 13, failures);
            AssertTextFloor(Required(margin, "Empty"), 13, failures);
            AssertTextFloor(Required(margin, "Combined"), 13, failures);
            AssertMinTarget(Required(Required(app, "Matchup0"), "AwayOdds"), failures);
            AssertMinTarget(Required(Required(app, "Matchup0"), "HomeOdds"), failures);
            AssertMinTarget(Required(Required(app, "Matchup0"), "Details"), failures);
            AssertMinTarget(Required(margin, "Chip10%"), failures);
            AssertMinTarget(Required(margin, "Chip25%"), failures);
            AssertMinTarget(Required(margin, "Chip50%"), failures);
            AssertMinTarget(Required(margin, "ChipMAX"), failures);
            AssertMinTarget(Required(margin, "Place"), failures);
            AssertMinTarget(Required(margin, "Lock"), failures);
            AssertMinTarget(Required(margin, "Skip"), failures);
            Assert.IsEmpty(failures.ToString(), failures.ToString());
        }

        [UnityTest]
        public IEnumerator Form_selected_ink_is_a_nonraycasting_deterministic_price_sprite()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();

            Invoke(Required(Required(App(laptop), "Matchup0"), "AwayOdds"));
            yield return WaitForRebuild();
            Transform firstRing = Required(Required(App(laptop), "Matchup0"), "BiroRing");
            Image firstImage = firstRing.GetComponent<Image>();
            Assert.IsNotNull(firstImage, "selected ink must use the imported white-alpha price-ring sprite, not a font glyph");
            Assert.IsNotNull(firstImage.sprite, "selected ring sprite missing");
            StringAssert.StartsWith("ring-price-", firstImage.sprite.name);
            Assert.IsFalse(firstImage.raycastTarget, "decorative ink must not intercept the price button");
            // Ring overshoots the real 112x32 odds-button cell by 8px per edge (ASSETS.md), not the
            // stale 96x30 cell the ring sprite was originally sized against — see SportsbookApp
            // BuildMatchupCard for the defect this corrects (the ring's edge used to cross the price).
            AssertRect(firstImage.rectTransform, 128f, 48f, "price-ring sprite");
            string variant = firstImage.sprite.name;

            Invoke(Required(Required(App(laptop), "WorkingMargin"), "Chip25%"));
            yield return WaitForRebuild();
            Image rebuiltImage = Required(Required(App(laptop), "Matchup0"), "BiroRing").GetComponent<Image>();
            Assert.IsNotNull(rebuiltImage);
            Assert.AreEqual(variant, rebuiltImage.sprite.name, "stake rebuild must preserve matchup-index ink variant");
        }

        /// <summary>S74-am (batch 65) — THE BOARD'S DRAW ROW. The draw takes its own line, between
        /// the two teams, with the word in the PRICE CELL and the matchup column EMPTY.
        ///
        /// <para>Every assertion here is a clause of the ruling, and the two that matter most are
        /// the ones a later change would break silently: that the draw's line sits <b>physically
        /// between</b> the two teams (S74 ruled the middle position is meaning, not borrowed
        /// convention, so its y-order IS the ruling), and that the matchup column stays <b>empty</b>
        /// — naming anything there would invent the third competitor `Side` exists to refuse, and it
        /// is the one clause whose correct implementation is the ABSENCE of a call.</para>
        ///
        /// <para>Named controls only, never builder order or child indexes, per this file's own
        /// contract.</para></summary>
        [UnityTest]
        public IEnumerator Form_board_prices_the_draw_on_its_own_line_between_the_two_teams()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            Transform card = Required(App(laptop), "Matchup0");

            // The block is THREE LINES — a design-time constant, never a response to what this
            // matchup priced (§2: a zone resizing to content at runtime is forbidden).
            AssertRect(card as RectTransform, 700f, 116f, "three-line matchup block");

            Transform draw = Required(card, "DrawOdds");
            StringAssert.StartsWith("DRAW", TextOf(draw),
                "S74-am: the outcome word belongs in the PRICE CELL, which is the cell that names " +
                "WHICH OUTCOME — the board already reads `AWAY -156`, not `NOTARIES -156`");

            // THE MIDDLE POSITION IS LITERAL. Higher on screen is a less negative y.
            float awayY = (Required(card, "AwayOdds") as RectTransform).anchoredPosition.y;
            float drawY = (draw as RectTransform).anchoredPosition.y;
            float homeY = (Required(card, "HomeOdds") as RectTransform).anchoredPosition.y;
            Assert.Less(drawY, awayY, "the draw's line must sit below AWAY");
            Assert.Less(homeY, drawY, "the draw's line must sit above HOME - it is BETWEEN the two, " +
                "attached to neither, which is exactly what the outcome is");

            // THE MATCHUP COLUMN IS EMPTY ON THAT LINE, and empty is the correct rendering of
            // "neither". TeamLine builds `Team{side}` + `Record{side}`; the draw has neither because
            // it has no team. This is not S24's dead cell - S24 refused an offer slot with no OFFER,
            // where here the SUBJECT slot has no subject.
            Assert.IsNotNull(Find(card, "TeamAway"), "the two teams still name themselves");
            Assert.IsNotNull(Find(card, "TeamHome"), "the two teams still name themselves");
            Assert.IsNull(Find(card, "TeamDraw"),
                "S74-am: naming a team on the draw's line invents the third competitor `Side` refuses");
            Assert.IsNull(Find(card, "RecordDraw"),
                "a draw has no season record because it has no team");

            // MORE spans the block unchanged - it is centre-anchored, so a third line must not have
            // needed it re-placed. Asserted because "unchanged" is a claim, not an observation.
            Assert.IsNotNull(Required(card, "Details"), "MORE spans the block, now three lines");
        }

        private static IEnumerator Boot()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("Room", LoadSceneMode.Single);
            Assert.IsNotNull(load, "Room scene is not available");
            while (!load.isDone) yield return null;
            LaptopScreen laptop = Laptop();
            Assert.IsNotNull(laptop, "LaptopScreen missing");
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

        private static LaptopScreen Laptop()
        {
            LaptopScreen laptop = UnityEngine.Object.FindAnyObjectByType<LaptopScreen>();
            Assert.IsNotNull(laptop, "LaptopScreen missing");
            return laptop;
        }

        private static IEnumerator WaitForRebuild()
        {
            // SportsbookApp rebuilds through Update and uses Destroy for old children. Two frame
            // boundaries ensure the pending-destroy tree is retired, including in batch mode where
            // WaitForEndOfFrame is not resumed.
            yield return null;
            yield return null;
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

        private static void AssertTextFloor(Transform node, int minimum, StringBuilder failures)
        {
            TMP_Text text = node.GetComponent<TMP_Text>();
            if (text == null) text = node.GetComponentInChildren<TMP_Text>();
            if (text == null || text.fontSize < minimum)
                failures.AppendLine($"{node.name}: product fact is {((text == null) ? 0 : text.fontSize)}px; minimum is {minimum}px.");
        }

        private static void AssertMinTarget(Transform node, StringBuilder failures)
        {
            RectTransform rect = node as RectTransform;
            if (rect == null || rect.sizeDelta.x < 44f || rect.sizeDelta.y < 32f)
                failures.AppendLine($"{node.name}: target is {rect?.sizeDelta}; minimum is 44x32.");
        }

        private static void AssertRect(RectTransform rect, float width, float height, string label)
        {
            Assert.IsNotNull(rect, $"{label} RectTransform missing");
            Assert.AreEqual(width, rect.sizeDelta.x, 0.01f, $"{label} width");
            Assert.AreEqual(height, rect.sizeDelta.y, 0.01f, $"{label} height");
        }
    }
}
