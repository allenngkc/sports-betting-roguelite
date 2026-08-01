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
                Text legRowLine = FindChild<Text>(screen, "LegRowLine0");
                Text ticketHeader = FindChild<Text>(screen, "TicketHeader");
                Assert.IsNotNull(cashOut, "CashOut text not found — canvas layout changed?");
                Assert.IsNotNull(legRowLine, "LegRowLine0 not found — canvas layout changed?");
                Assert.IsNotNull(ticketHeader, "TicketHeader not found — canvas layout changed?");

                Vector2 cashOutPosBefore = cashOut.rectTransform.anchoredPosition;
                Vector2 cashOutSizeBefore = cashOut.rectTransform.sizeDelta;
                Vector2 legRowPosBefore = legRowLine.rectTransform.anchoredPosition;
                Vector2 legRowSizeBefore = legRowLine.rectTransform.sizeDelta;
                Vector2 headerPosBefore = ticketHeader.rectTransform.anchoredPosition;

                // Dramatically different content: empty, then a single wide char, then a long run.
                cashOut.text = string.Empty;
                legRowLine.text = "W";
                ticketHeader.text = new string('X', 200);

                AssertRectUnchanged(cashOut.rectTransform, cashOutPosBefore, cashOutSizeBefore, "CashOut");
                AssertRectUnchanged(legRowLine.rectTransform, legRowPosBefore, legRowSizeBefore, "LegRowLine0");
                Assert.AreEqual(headerPosBefore, ticketHeader.rectTransform.anchoredPosition,
                    "TicketHeader's rect moved when its text grew from empty to 200 characters — a " +
                    "zone whose position derives from content length is exactly the defect DESIGN.md " +
                    "§6 warns against, even if it happens to still look right.");

                // And the reverse direction: shrink back down. The rect must still not move.
                cashOut.text = "CASH OUT $184,000,000   [E]";
                legRowLine.text = string.Empty;
                AssertRectUnchanged(cashOut.rectTransform, cashOutPosBefore, cashOutSizeBefore, "CashOut");
                AssertRectUnchanged(legRowLine.rectTransform, legRowPosBefore, legRowSizeBefore, "LegRowLine0");
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

        // ---------------------------------------------------------------------------------------
        // 4. T20 — the canon type scale, and the geometry it has to survive.
        // ---------------------------------------------------------------------------------------

        // Mirrored from main-2/docs/design/design-system/tokens/typography.css. A C# test cannot
        // import a CSS custom property, so handoff.md §4A's rule applies: restate the value and CITE
        // the source. Never a threshold reverse-engineered from whatever the code currently renders.
        private const int CanonScore = 36, CanonCashOut = 29, CanonClock = 28, CanonNeed = 28;
        private const int CanonRisk = 24, CanonEvent = 22, CanonProgress = 19, CanonEyebrow = 15;

        [Test]
        public void Leg_row_type_sizes_are_the_canon_scale_not_a_local_invention()
        {
            var go = new GameObject("T20Scale");
            try
            {
                var screen = BuildScreen(go);

                Text need = FindChild<Text>(screen, "LegRowNeed0");
                Text progress = FindChild<Text>(screen, "LegRowProgress0");
                Text line = FindChild<Text>(screen, "LegRowLine0");
                Assert.IsNotNull(need, "LegRowNeed0 not found — T20 split the row's Detail element into NEED + progress");
                Assert.IsNotNull(progress, "LegRowProgress0 not found");
                Assert.IsNotNull(line, "LegRowLine0 not found");

                Assert.AreEqual(CanonNeed, need.fontSize,
                    "the NEED statement must render at the canon --tv-size-need. T20 left it unchanged " +
                    "at 28 by name; if this fails, something re-derived it locally.");
                Assert.AreEqual(CanonProgress, progress.fontSize,
                    "the live progress line must render at the canon --tv-size-progress of 19. It was " +
                    "23, written against a ~37% ticket column; DESIGN.md §6 corrected the column to " +
                    "26-28% and at that width §6's own authored strings no longer fit one line at 23.");
                Assert.AreEqual(CanonEyebrow, line.fontSize,
                    "a resolved/pending row compresses to the canon --tv-size-eyebrow of 15.");

                // The ordering is the part that actually carries meaning: DESIGN.md §5's ratio table
                // is the law and the px are only its instantiation, so the ladder must hold even if
                // every absolute value is someday re-derived again.
                Assert.Greater(need.fontSize, progress.fontSize,
                    "NEED must outrank the progress line beneath it — the statement is the headline.");
                Assert.Greater(progress.fontSize, line.fontSize,
                    "a live row's progress must outrank a compressed row: live rows are DISPLAY, " +
                    "resolved and pending rows are INDEX.");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void A_live_rows_two_lines_fit_inside_its_fixed_row_height()
        {
            // The row height is FIXED (DESIGN.md §6) and the live form stacks NEED above progress
            // inside it. If that stack ever outgrows the row, glyphs clip on the production face —
            // which a -nographics run rasterises nothing to reveal, so it is asserted geometrically
            // here instead. This is also why the canon three-line row (market/price/state eyebrow +
            // NEED + progress) is NOT built: it needs ~73px against this row and would clip.
            var go = new GameObject("T20RowFit");
            try
            {
                var screen = BuildScreen(go);

                Text need0 = FindChild<Text>(screen, "LegRowNeed0");
                Text need1 = FindChild<Text>(screen, "LegRowNeed1");
                Text progress0 = FindChild<Text>(screen, "LegRowProgress0");
                Assert.IsNotNull(need0, "LegRowNeed0 not found");
                Assert.IsNotNull(need1, "LegRowNeed1 not found");
                Assert.IsNotNull(progress0, "LegRowProgress0 not found");

                // Row pitch, read off the grid itself rather than recomputed from the constants the
                // code under test uses — two adjacent slots are exactly one row apart.
                float rowPitch = Mathf.Abs(need1.rectTransform.anchoredPosition.y
                                         - need0.rectTransform.anchoredPosition.y);
                Assert.Greater(rowPitch, 0f, "two adjacent row slots occupy the same y — the grid collapsed");

                float topPad = Mathf.Abs(progress0.rectTransform.anchoredPosition.y
                                       - need0.rectTransform.anchoredPosition.y) - need0.rectTransform.sizeDelta.y;
                float stack = need0.rectTransform.sizeDelta.y + progress0.rectTransform.sizeDelta.y;

                Assert.LessOrEqual(stack, rowPitch,
                    $"a live row's NEED ({need0.rectTransform.sizeDelta.y}px) + progress " +
                    $"({progress0.rectTransform.sizeDelta.y}px) = {stack}px overflows its {rowPitch}px " +
                    "row. Either the row grew or a type size did; both clip the authored statement, " +
                    "and DESIGN.md §6 forbids shortening the statement to fit.");
                Assert.AreEqual(0f, topPad, 0.01f,
                    "progress must sit immediately beneath NEED — a gap here means the two lines are " +
                    "no longer one stacked block and the row's budget is being spent on whitespace.");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Nothing_in_the_sweat_surface_outgrows_the_score()
        {
            // DESIGN.md §5: "the score is the largest element on the surface at all times and nothing
            // outgrows it, including cash-out." Scoped deliberately to the five sweat zones — the
            // attract/payout takeover screens are different states, not this surface, and their type
            // is sized against a full-screen moment rather than the scorebug.
            var go = new GameObject("T20Ladder");
            try
            {
                var screen = BuildScreen(go);

                Text score = FindChild<Text>(screen, "Matchup"); // the persistent score line
                Assert.IsNotNull(score, "Matchup (the score) not found");
                Assert.AreEqual(CanonScore, score.fontSize, "the score must render at the canon --tv-size-score");

                foreach (string name in new[]
                    { "Clock", "CashOut", "RiskPays", "Flavor", "TicketHeader", "Leg",
                      "LegRowNeed0", "LegRowProgress0", "LegRowLine0" })
                {
                    Text t = FindChild<Text>(screen, name);
                    Assert.IsNotNull(t, $"{name} not found — canvas layout changed?");
                    Assert.LessOrEqual(t.fontSize, score.fontSize,
                        $"{name} renders at {t.fontSize}px against the score's {score.fontSize}px. " +
                        "DESIGN.md §5's ratio table makes the score the thing nothing may outgrow.");
                }

                // Spot-check the rungs that carry a named ruling rather than every pair.
                Assert.AreEqual(CanonCashOut, FindChild<Text>(screen, "CashOut").fontSize,
                    "cash-out sits at .70 of the score and must never reach it (DESIGN.md §5).");
                Assert.AreEqual(CanonClock, FindChild<Text>(screen, "Clock").fontSize);
                Assert.AreEqual(CanonRisk, FindChild<Text>(screen, "RiskPays").fontSize,
                    "C8 put risk/pays in the protected set; it sits at the canon --tv-size-risk.");
                Assert.AreEqual(CanonEvent, FindChild<Text>(screen, "Flavor").fontSize,
                    "the event strip is one line at the canon --tv-size-event.");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
