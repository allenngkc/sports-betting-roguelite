using System.Reflection;
using NUnit.Framework;
using SBR.Game;
using UnityEngine;
using UnityEngine.UI;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// Phase 3C (PRD §8.1, DESIGN.md §6/§7, VISUAL-DESIGN.md §2): geometry coverage for the Layout
    /// B, "Ticket Rail" canvas rebuild. DESIGN.md §6 is explicit that the load-bearing discipline
    /// here is an EXPLICIT FIXED GRID — "every zone position comes from an explicit fixed layout
    /// grid defined once in code, never computed from content" — and that this is "the thing most
    /// likely to erode during implementation. Reviewers should check it specifically." These tests
    /// are that check: they never construct a real ticket/session (BuildCanvas's zone geometry does
    /// not depend on one), they just build the canvas (mirroring
    /// TvSweatScreenPaletteTests.cs's established `theaterEnabled = false` + reflected `Awake()`
    /// pattern) and inspect the resulting RectTransforms.
    /// </summary>
    public class TvSweatScreenLayoutGridTests
    {
        private static void InvokePrivate(object target, string method)
        {
            MethodInfo m = target.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(m, $"{target.GetType().Name}.{method} not found by reflection — was it renamed?");
            m.Invoke(target, null);
        }

        private static T FindChild<T>(Component root, string childName) where T : Component
        {
            foreach (T c in root.GetComponentsInChildren<T>(true))
                if (c.name == childName) return c;
            return null;
        }

        private static TvSweatScreen BuildScreen(GameObject go, int referencePixelsWide = 980)
        {
            go.SetActive(false); // field defaults only — never let Awake/OnEnable fire on its own
            var screen = go.AddComponent<TvSweatScreen>();
            screen.theaterEnabled = false; // isolation: BuildCanvas must not touch TheaterStage/audio
            screen.referencePixelsWide = referencePixelsWide;
            InvokePrivate(screen, "Awake");
            return screen;
        }

        // ---------------------------------------------------------------------------------------
        // 1. Geometry comes from the grid, not from content.
        // ---------------------------------------------------------------------------------------

        [Test]
        public void Zone_rects_are_unchanged_when_text_content_changes_dramatically()
        {
            var go = new GameObject("GridNotContent");
            try
            {
                var screen = BuildScreen(go);

                Text cashOut = FindChild<Text>(screen, "CashOut");
                Text legRowLabel = FindChild<Text>(screen, "LegRowLabel0");
                Text ticketHeader = FindChild<Text>(screen, "TicketHeader");
                Assert.IsNotNull(cashOut, "CashOut text not found — canvas layout changed?");
                Assert.IsNotNull(legRowLabel, "LegRowLabel0 not found — canvas layout changed?");
                Assert.IsNotNull(ticketHeader, "TicketHeader not found — canvas layout changed?");

                Vector2 cashOutPosBefore = cashOut.rectTransform.anchoredPosition;
                Vector2 cashOutSizeBefore = cashOut.rectTransform.sizeDelta;
                Vector2 legRowPosBefore = legRowLabel.rectTransform.anchoredPosition;
                Vector2 legRowSizeBefore = legRowLabel.rectTransform.sizeDelta;
                Vector2 headerPosBefore = ticketHeader.rectTransform.anchoredPosition;

                // Dramatically different content: empty, then a single wide char, then a long run.
                cashOut.text = string.Empty;
                legRowLabel.text = "W";
                ticketHeader.text = new string('X', 200);

                AssertRectUnchanged(cashOut.rectTransform, cashOutPosBefore, cashOutSizeBefore, "CashOut");
                AssertRectUnchanged(legRowLabel.rectTransform, legRowPosBefore, legRowSizeBefore, "LegRowLabel0");
                Assert.AreEqual(headerPosBefore, ticketHeader.rectTransform.anchoredPosition,
                    "TicketHeader's rect moved when its text grew from empty to 200 characters — a " +
                    "zone whose position derives from content length is exactly the defect DESIGN.md " +
                    "§6 warns against, even if it happens to still look right.");

                // And the reverse direction: shrink back down. The rect must still not move.
                cashOut.text = "CASH OUT $184,000,000   [E]";
                legRowLabel.text = string.Empty;
                AssertRectUnchanged(cashOut.rectTransform, cashOutPosBefore, cashOutSizeBefore, "CashOut");
                AssertRectUnchanged(legRowLabel.rectTransform, legRowPosBefore, legRowSizeBefore, "LegRowLabel0");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        private static void AssertRectUnchanged(RectTransform rt, Vector2 posBefore, Vector2 sizeBefore, string label)
        {
            Assert.AreEqual(posBefore, rt.anchoredPosition,
                $"{label}'s anchoredPosition moved after a content-only change — positions must come " +
                "from the fixed grid, never from what is currently displayed (DESIGN.md §6).");
            Assert.AreEqual(sizeBefore, rt.sizeDelta,
                $"{label}'s sizeDelta changed after a content-only change — reserved space must stay " +
                "reserved (DESIGN.md §6: \"No zone resizes in response to content\").");
        }

        // ---------------------------------------------------------------------------------------
        // 2. Ticket column width: 26-28% of the surface, fixed across every market.
        // ---------------------------------------------------------------------------------------

        [Test]
        public void Ticket_column_width_is_26_to_28_percent_and_scales_proportionally()
        {
            // Two different canvas configurations (nothing market-related feeds LayoutGrid at all —
            // its only inputs are the canvas's own configured pixel width/height) demonstrate the
            // width is a genuine FRACTION of the surface, not a value coupled to one specific
            // resolution, which is the closest EditMode-observable proxy for "identical across
            // every market": no market or ticket state is even reachable from BuildCanvas.
            float lastFraction = -1f;
            foreach (int refWidth in new[] { 980, 1200 })
            {
                var go = new GameObject($"TicketWidth_{refWidth}");
                try
                {
                    var screen = BuildScreen(go, refWidth);

                    Image ticketZone = FindChild<Image>(screen, "TicketColumnZone");
                    Image cashOutZone = FindChild<Image>(screen, "CashOutZone");
                    Assert.IsNotNull(ticketZone, "TicketColumnZone panel not found — canvas layout changed?");
                    Assert.IsNotNull(cashOutZone, "CashOutZone panel not found — canvas layout changed?");

                    float ticketWidth = ticketZone.rectTransform.sizeDelta.x;
                    float fraction = ticketWidth / refWidth;

                    Assert.GreaterOrEqual(fraction, 0.26f,
                        $"ticket column must be at least 26% of the surface (DESIGN.md §6); got " +
                        $"{fraction:P1} at referencePixelsWide={refWidth}");
                    Assert.LessOrEqual(fraction, 0.28f,
                        $"ticket column must be at most 28% of the surface (DESIGN.md §6); got " +
                        $"{fraction:P1} at referencePixelsWide={refWidth}");

                    // DESIGN.md §6: "Cash-out anchored at the foot of the ticket column" and "§6:
                    // the ticket column has a fixed width across every market" — the cash-out slot
                    // shares that exact width, not an independently-tuned one.
                    Assert.AreEqual(ticketWidth, cashOutZone.rectTransform.sizeDelta.x, 0.01f,
                        "CashOutZone must share the ticket column's width exactly — it is anchored " +
                        "at the foot of the SAME column, not a differently-sized slot.");

                    if (lastFraction >= 0f)
                        Assert.AreEqual(lastFraction, fraction, 0.001f,
                            "the ticket column's width fraction changed between two canvas " +
                            "resolutions — it must be a fixed proportion of the surface, not a " +
                            "value tied to one specific reference width.");
                    lastFraction = fraction;
                }
                finally
                {
                    Object.DestroyImmediate(go);
                }
            }
        }

        // ---------------------------------------------------------------------------------------
        // 3. No zone reflow across the six cash-out states (PRD §8.5, DESIGN.md §8).
        // ---------------------------------------------------------------------------------------

        [Test]
        public void CashOut_zone_does_not_reflow_across_the_six_states()
        {
            var go = new GameObject("CashOutSixStates");
            try
            {
                var screen = BuildScreen(go);

                Text cashOut = FindChild<Text>(screen, "CashOut");
                Assert.IsNotNull(cashOut, "CashOut text not found — canvas layout changed?");
                Vector2 posBefore = cashOut.rectTransform.anchoredPosition;
                Vector2 sizeBefore = cashOut.rectTransform.sizeDelta;

                // Representative text/enabled combinations for the six states in PRD §8.5 /
                // DESIGN.md §8's cash-out slot table. These are set directly (mirroring what
                // RenderCashOut/SuspendMarket/PendingWindowBeat/CashOutFloodBeat each write) rather
                // than driven through a live session, since the geometric claim under test — the
                // rectangle never moves — does not depend on how the session got there.
                var states = new (string label, string text, bool enabled)[]
                {
                    ("Actionable",       "CASH OUT $184   [E]", true),
                    ("Price animating",  "CASH OUT $176   •   UPDATING", true),
                    ("Suspended",        "MARKET SUSPENDED", true),
                    ("Pending window",   "MARKET SUSPENDED", true),
                    ("Unavailable",      string.Empty, false),
                    ("Accepted",         "CASHED OUT $184", true),
                };

                foreach ((string label, string text, bool enabled) in states)
                {
                    cashOut.text = text;
                    cashOut.enabled = enabled;
                    Assert.AreEqual(posBefore, cashOut.rectTransform.anchoredPosition,
                        $"CashOut's rect moved entering the '{label}' state — DESIGN.md §6: reserved " +
                        "space stays reserved so all six states can share one rectangle with no reflow.");
                    Assert.AreEqual(sizeBefore, cashOut.rectTransform.sizeDelta,
                        $"CashOut's size changed entering the '{label}' state.");
                }
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
