using System.Collections.Generic;
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
        // import a CSS custom property, so §4A's rule (docs/handoffs/tv-sweat.md) applies: CITE
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
        public void The_glass_clips_every_layer_and_nothing_is_built_outside_it()
        {
            // T25.1, widened by Allen's direct observation: it was never only the stage actors —
            // charts and plain text were passing in and out of the panel too. Three different
            // causes (a misanchored stage, MomentumTape's unbounded dot cursor, overflow-enabled
            // Text), which is why the answer is structural rather than per-layer.
            //
            // Two claims, because they fail differently:
            //   1. The glass CLIPS. A RectMask2D on the canvas means nothing can be drawn outside
            //      it at runtime, whatever a layer does mid-sweat.
            //   2. Nothing is BUILT outside. Clipping hides an escape; it does not make a
            //      mispositioned element correct, and content silently cut off is still a defect.
            var go = new GameObject("GlassContainment");
            try
            {
                go.SetActive(false);
                var screen = go.AddComponent<TvSweatScreen>();
                screen.theaterEnabled = true; // audit the stage layer too, not just the chrome
                screen.referencePixelsWide = 980;
                InvokePrivate(screen, "Awake");

                var canvas = screen.GetComponentInChildren<Canvas>(true);
                Assert.IsNotNull(canvas, "no canvas was built");
                var canvasRt = canvas.GetComponent<RectTransform>();

                Assert.IsNotNull(canvas.GetComponent<RectMask2D>(),
                    "the canvas has no RectMask2D — the TV's glass does not clip. Without it, any " +
                    "layer that overflows its rect renders into the room, which is what Allen saw.");

                float halfW = canvasRt.sizeDelta.x * 0.5f, halfH = canvasRt.sizeDelta.y * 0.5f;
                const float eps = 0.75f;
                var corners = new Vector3[4];
                var escapees = new List<string>();

                foreach (Graphic g in screen.GetComponentsInChildren<Graphic>(true))
                {
                    if (g.rectTransform == canvasRt) continue;
                    g.rectTransform.GetWorldCorners(corners);
                    float minX = float.MaxValue, maxX = float.MinValue;
                    float minY = float.MaxValue, maxY = float.MinValue;
                    foreach (Vector3 wc in corners)
                    {
                        Vector3 lc = canvasRt.InverseTransformPoint(wc);
                        minX = Mathf.Min(minX, lc.x); maxX = Mathf.Max(maxX, lc.x);
                        minY = Mathf.Min(minY, lc.y); maxY = Mathf.Max(maxY, lc.y);
                    }
                    if (minX < -halfW - eps || maxX > halfW + eps ||
                        minY < -halfH - eps || maxY > halfH + eps)
                        escapees.Add($"{g.name} [x {minX:F0}..{maxX:F0}, y {minY:F0}..{maxY:F0}]");
                }

                Assert.IsEmpty(escapees,
                    "these layers are built outside the TV's glass (canvas is " +
                    $"{canvasRt.sizeDelta.x}x{canvasRt.sizeDelta.y}, so x is ±{halfW} and y is ±{halfH}):\n  " +
                    string.Join("\n  ", escapees) +
                    "\nThe mask stops them being DRAWN outside, but an element positioned off the " +
                    "panel is still cut off content. Fix the placement; do not relax this test.");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void The_theater_stage_sits_wholly_inside_the_TV_glass()
        {
            // T25.1 regression guard. Phase 3C's Layout B rebuild started passing
            // AnchorCenter(grid.Stage) — a TOP-LEFT space coordinate — to a TheaterStage that
            // anchored itself CENTRE, so the pitch and every actor drew roughly half a canvas down
            // and right, entirely OUTSIDE the TV's glass. Five commits shipped over it with every
            // suite green, because nothing asserted where the stage actually was; it took seated
            // capture frames to see it at all.
            //
            // This is that assertion. It is deliberately about CONTAINMENT rather than an exact
            // rect: the stage may be repositioned by a later layout pass, but it may never leave
            // the panel, because anything outside the glass is rendering into the room.
            var go = new GameObject("StageInsideGlass");
            try
            {
                go.SetActive(false);
                var screen = go.AddComponent<TvSweatScreen>();
                screen.theaterEnabled = true; // the whole point: the stage must exist to be placed
                screen.referencePixelsWide = 980;
                InvokePrivate(screen, "Awake");

                var stage = screen.GetComponentInChildren<TheaterStage>(true);
                Assert.IsNotNull(stage, "no TheaterStage was built with theaterEnabled = true");
                var rt = (RectTransform)stage.transform;

                // Canvas extents, read off the built canvas rather than recomputed from constants.
                var canvasRt = screen.GetComponentInChildren<Canvas>(true).GetComponent<RectTransform>();
                float cw = canvasRt.sizeDelta.x, ch = canvasRt.sizeDelta.y;
                Assert.Greater(cw, 0f, "canvas has no width");

                // The stage is anchored top-left with a centre pivot: x right-positive, y negative
                // downward from the canvas's top-left corner.
                Vector2 c = rt.anchoredPosition;
                Vector2 half = rt.sizeDelta * 0.5f;
                float left = c.x - half.x, right = c.x + half.x;
                float top = -c.y - half.y, bottom = -c.y + half.y;

                Assert.GreaterOrEqual(left, -0.5f,
                    $"the stage's left edge ({left}) is off the glass — actors would render outside the TV");
                Assert.LessOrEqual(right, cw + 0.5f,
                    $"the stage's right edge ({right}) runs past the canvas width ({cw}). This is the " +
                    "T25.1 signature: a top-left coordinate consumed as a centre-relative one.");
                Assert.GreaterOrEqual(top, -0.5f,
                    $"the stage's top edge ({top}) is above the glass");
                Assert.LessOrEqual(bottom, ch + 0.5f,
                    $"the stage's bottom edge ({bottom}) runs past the canvas height ({ch})");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>T24's re-measure, done in the production face rather than estimated.
        ///
        /// <para>T24 authored the slot at 76px "measured from the live row in the production face" —
        /// but that figure came from a THREE-line row (eyebrow 15 + NEED 28 + progress 19), and the
        /// same ruling removes the meta line: "the live row carries no market/price/state meta line,
        /// and that is now specified rather than tolerated." So the measured row and the specified
        /// row are not the same row.</para>
        ///
        /// <para>This measures what the ruling actually specifies — NEED above progress, in Encode
        /// Sans — using Unity's own <c>preferredHeight</c> rather than the LineBox estimate the
        /// build sizes with. If the real stack fits the current slot, the 40px deficit does not
        /// survive and risk/pays does not need to move.</para></summary>
        [Test]
        public void T24_the_specified_live_row_measured_in_the_production_face_fits_its_slot()
        {
            var go = new GameObject("T24Remeasure");
            try
            {
                var screen = BuildScreen(go);
                Text need = FindChild<Text>(screen, "LegRowNeed0");
                Text progress = FindChild<Text>(screen, "LegRowProgress0");
                Text need1 = FindChild<Text>(screen, "LegRowNeed1");
                Assert.IsNotNull(need); Assert.IsNotNull(progress); Assert.IsNotNull(need1);

                Assert.IsNotNull(need.font, "no font resolved — a measurement in the fallback face is void");
                Assert.IsTrue(need.font.name.Contains("Encode"),
                    $"measured in '{need.font.name}', not Encode Sans. T24 asks for the PRODUCTION " +
                    "face; a measurement in any other face is the mistake T20 already made once.");

                // Real rendered heights, with the longest authored strings §6 permits.
                need.text = "MARCUS VALE TO SCORE";
                progress.text = "LIVE • 0 GOALS • 3 MORE";
                float measured = need.preferredHeight + progress.preferredHeight;

                float slot = Mathf.Abs(need1.rectTransform.anchoredPosition.y
                                     - need.rectTransform.anchoredPosition.y);

                Debug.Log($"[T24] measured live row = {measured:0.0}px " +
                          $"(NEED {need.preferredHeight:0.0} + progress {progress.preferredHeight:0.0}) " +
                          $"in '{need.font.name}'; slot = {slot:0.0}px");

                Assert.LessOrEqual(measured + 8f, slot,
                    $"the specified live row measures {measured:0.0}px in the production face and the " +
                    $"slot is {slot:0.0}px. If this fails the T24 deficit is real and risk/pays moves " +
                    "to the ticket card, which returns exactly 40px (416 + 40 = 456 = 6 x 76).");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>TV-15's measurement, taken before the rebuild rather than reasoned from LineBox.
        ///
        /// <para>Canon's TvRiskPays is FOUR elements — an eyebrow label ABOVE a value, twice
        /// (`TvRiskPays.jsx:7-18`). Estimated, that stack is 18+29 = 47px against a 40px footer, and
        /// growing the footer to 48 leaves the live row a ONE pixel margin (slot 68.0 vs a measured
        /// need of 67). One pixel is not a margin, and the T24 re-measure just showed the estimate
        /// and the measurement differ by 2px — enough to swallow it entirely.</para>
        ///
        /// <para>So this measures the real cell in the production faces (label regular, value
        /// condensed, per canon) and reports both geometries: STACKED, which canon specifies, and
        /// SIDE-BY-SIDE, which costs no vertical space and may fit a 265px column. TV-14 and TV-15
        /// build from these numbers. It asserts only the thing that is not a judgement call — that
        /// the measurement happened in Encode Sans — and leaves the layout choice to the DD.</para></summary>
        [Test]
        public void T15_measure_the_risk_pays_cell_in_the_production_face()
        {
            var go = new GameObject("T15Measure");
            try
            {
                var screen = BuildScreen(go);
                Text riskPays = FindChild<Text>(screen, "RiskPays");   // condensed, per TvRiskPays.jsx:14
                Text eventLine = FindChild<Text>(screen, "Flavor");    // regular, per TvEventStrip.jsx:10
                Assert.IsNotNull(riskPays, "RiskPays not found");
                Assert.IsNotNull(eventLine, "Flavor not found");
                Assert.IsNotNull(riskPays.font, "no font resolved — a measurement in the fallback is void");
                Assert.IsTrue(riskPays.font.name.Contains("Encode"),
                    $"measured in '{riskPays.font.name}', not Encode Sans — the same mistake T20 made once");

                Text label = MeasureText(riskPays.transform.parent, eventLine.font, 15, "PAYS");
                Text value = MeasureText(riskPays.transform.parent, riskPays.font, 24, "$1,234");

                float stackedH = label.preferredHeight + value.preferredHeight;
                float cellW = Mathf.Max(label.preferredWidth, value.preferredWidth);
                float sideBySideH = Mathf.Max(label.preferredHeight, value.preferredHeight);
                float sideBySideW = label.preferredWidth + 8f + value.preferredWidth;

                var columnZone = FindChild<Image>(screen, "TicketColumnZone");
                float columnW = columnZone != null ? columnZone.rectTransform.sizeDelta.x : 0f;

                Debug.Log($"[T15] label '{label.text}' {label.preferredWidth:0.0}x{label.preferredHeight:0.0} " +
                          $"({eventLine.font.name}) | value '{value.text}' " +
                          $"{value.preferredWidth:0.0}x{value.preferredHeight:0.0} ({riskPays.font.name})");
                Debug.Log($"[T15] STACKED cell = {cellW:0.0}w x {stackedH:0.0}h — two cells need " +
                          $"{cellW * 2 + 24:0.0}w, footer must be >= {stackedH + 8:0.0}h (is 40)");
                Debug.Log($"[T15] SIDE-BY-SIDE cell = {sideBySideW:0.0}w x {sideBySideH:0.0}h — two cells need " +
                          $"{sideBySideW * 2 + 24:0.0}w of the {columnW:0.0}px column, footer unchanged at 40");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>A throwaway Text used only to ask Unity what a string actually measures.</summary>
        private static Text MeasureText(Transform parent, Font font, int size, string content)
        {
            var go = new GameObject("Measure", typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = font;
            t.fontSize = size;
            t.fontStyle = FontStyle.Bold;
            t.text = content;
            return t;
        }

        /// <summary>T41 (C3 violation, blocking): nothing on the STAGE may sit above L3.
        ///
        /// <para>Measured off delivered frames, the pitch ran at <b>1.000</b> while the actionable
        /// cash-out band — "the surface's only L4 element" — measured <b>0.671</b>. The one-full-
        /// brightness law did not fail because the band was dim. It failed because everything else
        /// was brighter than the one thing the player can act on.</para>
        ///
        /// <para>§7 puts markings at L1–L2 and actors at L3, and permits the ball L4 "only at a
        /// payoff" — that punch is a separate overlay the screen raises through the HDR material, so
        /// the persistent stage must never already be there. This asserts the ceiling at build time,
        /// because the violation shipped for weeks while every suite was green: no test looked at
        /// the stage's brightness, only at its geometry.</para></summary>
        [Test]
        public void T41_nothing_on_the_stage_sits_above_L3()
        {
            var go = new GameObject("T41StageCeiling");
            try
            {
                go.SetActive(false);
                var screen = go.AddComponent<TvSweatScreen>();
                screen.theaterEnabled = true;   // the stage must exist to be measured
                screen.referencePixelsWide = 980;
                InvokePrivate(screen, "Awake");

                var stage = screen.GetComponentInChildren<TheaterStage>(true);
                Assert.IsNotNull(stage, "no TheaterStage was built — this test cannot pass vacuously");

                // MEASURE LUMINANCE, NOT ALPHA. The first version of this guard asserted alpha and
                // flagged PitchBg at 0.95 — a near-black background at high opacity, which is dark
                // by any reading. Alpha is not brightness: a dark colour at full opacity is dim, and
                // a white one at half opacity is not. The DD's table is brightest-pixel luminance,
                // so this measures the same quantity: the brightest channel, scaled by opacity.
                const float l3 = 0.7f, eps = 0.002f;
                var offenders = new List<string>();
                foreach (Graphic g in stage.GetComponentsInChildren<Graphic>(true))
                {
                    // The momentary payoff overlays are the sanctioned L4 path and are raised
                    // through RequestL4's one-token invariant, not by sitting bright. They are
                    // disabled at build; anything ENABLED and above L3 is a persistent occupant.
                    if (!g.enabled) continue;
                    Color c = g.color;
                    float luminance = Mathf.Max(c.r, Mathf.Max(c.g, c.b)) * c.a;
                    if (luminance > l3 + eps)
                        offenders.Add($"{g.name} L={luminance:0.###}");
                }

                Assert.IsEmpty(offenders,
                    "these stage elements are built above L3, so the pitch outranks the cash-out " +
                    "band and §3's one-full-brightness law is broken before a frame is drawn:\n  " +
                    string.Join("\n  ", offenders) +
                    "\nMarkings belong at L1–L2, actors at L3. The ball reaches L4 only at a payoff, " +
                    "and that is the separate flash overlay — not the persistent ball.");
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
