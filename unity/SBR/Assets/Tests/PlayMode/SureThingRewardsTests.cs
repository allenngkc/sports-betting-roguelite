using System;
using System.Collections;
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
    public class SureThingRewardsTests
    {
        [UnityTest, Order(1)]
        public IEnumerator Rewards_reports_affordability_truth_and_a_successful_purchase()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            yield return EnterShop(laptop);
            Run run = laptop.director.Run;

            Transform board = Required(App(laptop), "RewardsBoard");
            Transform margin = Required(App(laptop), "RewardsMargin");
            AssertRect(board as RectTransform, 700f, 530f, "REWARDS board");
            AssertRect(margin as RectTransform, 324f, 530f, "REWARDS margin");
            Assert.AreEqual(FormatComps(run.Comps) + " COMPS",
                TextOf(Required(margin, "RewardsTally")));
            Assert.IsNotNull(Required(margin, "LeaveRewards"));
            Assert.IsTrue(Required(margin, "LeaveRewards").GetComponent<Button>().interactable);
            Assert.Greater(run.ShopOffers.Count, 0, "deterministic shop must expose a relic offer");

            for (int i = 0; i < run.ShopOffers.Count; i++)
            {
                RelicDefinition offer = run.ShopOffers[i];
                Transform row = Required(board, "RewardOffer" + i);
                // S9 defect 1/3: price is wax regardless of affordability, and grammatically agreed
                // ("1 COMPS" was the reported defect — ask_manager's price is exactly 1).
                Assert.AreEqual(CompsLabel(offer.Price),
                    TextOf(Required(row, "Affordability")));
                Assert.AreEqual("NEED " + CompsLabel(offer.Price - run.Comps),
                    TextOf(Required(row, "BuyReason")));
                Assert.IsFalse(Required(row, "Buy").GetComponent<Button>().interactable,
                    $"RewardOffer{i} BUY must be disabled at {run.Comps} comps");
            }
            // Allen ruled 2026-07-31 that an offer keeps its whole rule text and the board shows
            // however many offers fit, so a row is no longer guaranteed to exist for every dealt
            // offer. What must still hold is stricter than "row i exists": every row that IS shown
            // states affordability truthfully, and every offer that is NOT shown is accounted for
            // on screen rather than silently dropped.
            int consumablesShown = 0;
            for (int i = 0; i < run.ConsumableOffers.Count; i++)
            {
                Transform row = Find(board, "ConsumableOffer" + i);
                if (row == null) continue;
                consumablesShown++;
                ConsumableDefinition offer = run.ConsumableOffers[i];
                Assert.AreEqual(CompsLabel(offer.Price),
                    TextOf(Required(row, "Affordability")));
                Assert.AreEqual("NEED " + CompsLabel(offer.Price - run.Comps),
                    TextOf(Required(row, "BuyReason")));
                Assert.IsFalse(Required(row, "Buy").GetComponent<Button>().interactable,
                    $"ConsumableOffer{i} BUY must be disabled at {run.Comps} comps");
            }

            int relicsShown = 0;
            for (int i = 0; i < run.ShopOffers.Count; i++)
                if (Find(board, "RewardOffer" + i) != null) relicsShown++;
            int hidden = run.ShopOffers.Count + run.ConsumableOffers.Count
                - relicsShown - consumablesShown;
            Transform notShown = Find(board, "OffersNotShown");
            if (hidden > 0)
            {
                Assert.IsNotNull(notShown,
                    $"{hidden} offer(s) did not fit and the board must say so, not drop them silently");
                StringAssert.Contains(hidden.ToString(), TextOf(notShown));
            }
            else
            {
                Assert.IsNull(notShown, "nothing was hidden, so the board must not claim otherwise");
            }
            AssertProductFloors(board, margin);

            run.GrantComps(1000);
            laptop.Os.OpenSportsbook(SportsbookApp.Tab.Rewards);
            yield return WaitForRebuild();
            board = Required(App(laptop), "RewardsBoard");
            margin = Required(App(laptop), "RewardsMargin");
            RelicDefinition purchased = run.ShopOffers[0];
            double compsBefore = run.Comps;
            int ownedBefore = run.OwnedRelics.Count;
            Transform purchasable = Required(board, "RewardOffer0");
            Assert.AreEqual("AFFORDABLE", TextOf(Required(purchasable, "BuyReason")));
            Assert.IsTrue(Required(purchasable, "Buy").GetComponent<Button>().interactable);

            Invoke(Required(purchasable, "Buy"));
            yield return WaitForRebuild();
            margin = Required(App(laptop), "RewardsMargin");
            Assert.AreEqual(ownedBefore + 1, run.OwnedRelics.Count);
            Assert.AreEqual(purchased.Id, run.OwnedRelics[run.OwnedRelics.Count - 1].Id);
            Assert.AreEqual(compsBefore - purchased.Price, run.Comps, 1e-9);
            Assert.AreEqual(FormatComps(run.Comps) + " COMPS",
                TextOf(Required(margin, "RewardsTally")));
            Assert.AreEqual("RELIC PURCHASE RECORDED",
                TextOf(Required(margin, "ShopResult")));
            AssertProductFloors(Required(App(laptop), "RewardsBoard"), margin);
        }

        [UnityTest, Order(2)]
        public IEnumerator Rewards_surfaces_the_existing_manager_once_per_visit_error_verbatim()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            yield return EnterShop(laptop);
            Run run = laptop.director.Run;
            ConsumableDefinition manager = ManagerDefinition();
            run.GrantConsumable(manager);
            run.GrantConsumable(manager);
            laptop.Os.OpenSportsbook(SportsbookApp.Tab.Rewards);
            yield return WaitForRebuild();

            Transform margin = Required(App(laptop), "RewardsMargin");
            Invoke(Required(margin, "Manager"));
            yield return WaitForRebuild();
            margin = Required(App(laptop), "RewardsMargin");
            Assert.IsNotNull(Required(margin, "ShopResult"),
                "first Manager use must be recorded as a successful redeal");
            Assert.IsTrue(run.OwnsConsumable("ask_manager"),
                "the second held Manager charm must remain available to exercise the visit latch");

            Invoke(Required(margin, "Manager"));
            yield return WaitForRebuild();
            margin = Required(App(laptop), "RewardsMargin");
            Assert.AreEqual("The Manager comes out once per visit",
                TextOf(Required(margin, "ShopError")));
            Assert.IsTrue(run.OwnsConsumable("ask_manager"),
                "the rejected second use must not consume the held charm");
        }

        private static ConsumableDefinition ManagerDefinition()
        {
            foreach (ConsumableDefinition definition in RelicCatalog.Consumables)
                if (definition.Id == "ask_manager") return definition;
            Assert.Fail("RelicCatalog is missing the existing ask_manager definition");
            return null;
        }

        private static IEnumerator EnterShop(LaptopScreen laptop)
        {
            Assert.AreEqual(0, laptop.director.Run.Tickets.Count,
                "zero-ticket lock is the deterministic shop-entry test seam");
            laptop.director.LockRound();
            yield return WaitForRebuild();
            Assert.AreEqual(Phase.Shop, laptop.director.Run.Phase);
            Assert.AreEqual(SportsbookApp.Tab.Rewards, laptop.Os.CurrentTab);
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

        private static LaptopScreen Laptop()
        {
            LaptopScreen laptop = UnityEngine.Object.FindAnyObjectByType<LaptopScreen>();
            Assert.IsNotNull(laptop, "LaptopScreen missing");
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

        private static string FormatComps(double value)
            => value.ToString("0.#", CultureInfo.InvariantCulture);

        // Mirrors SportsbookApp's private FormatComps (S9 defect 3): RewardsTally intentionally
        // keeps the old unconditional " COMPS" suffix (asserted above) since run.Comps landing on
        // exactly 1 is not the reported defect and not exercised by this deterministic run; only the
        // per-offer price/reason lines route through the pluralized helper.
        private static string CompsLabel(double value)
        {
            string formatted = FormatComps(value);
            return formatted + (formatted == "1" ? " COMP" : " COMPS");
        }

        private static void AssertRect(RectTransform rect, float width, float height, string label)
        {
            Assert.IsNotNull(rect, $"{label} RectTransform missing");
            Assert.AreEqual(width, rect.sizeDelta.x, 0.01f, $"{label} width");
            Assert.AreEqual(height, rect.sizeDelta.y, 0.01f, $"{label} height");
        }
    }
}
