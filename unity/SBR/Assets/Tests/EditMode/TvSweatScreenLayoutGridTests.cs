using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using SBR.Game;
using TMPro;
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

        /// <summary>The nearest <see cref="RectMask2D"/> at or above <paramref name="t"/>, walked by
        /// hand. <c>GetComponentInParent&lt;T&gt;()</c> skips inactive objects and this harness keeps
        /// the whole hierarchy inactive, so the built-in would report "no mask" on a correctly masked
        /// tree.</summary>
        private static RectMask2D NearestMaskAbove(Transform t)
        {
            for (Transform p = t; p != null; p = p.parent)
            {
                var m = p.GetComponent<RectMask2D>();
                if (m != null) return m;
            }
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

                TMP_Text cashOut = FindChild<TMP_Text>(screen, "CashOut");
                TMP_Text legRowLine = FindChild<TMP_Text>(screen, "LegRowLine0");
                TMP_Text ticketHeader = FindChild<TMP_Text>(screen, "TicketHeader");
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

                TMP_Text cashOut = FindChild<TMP_Text>(screen, "CashOut");
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
                    ("Suspended",        "SUSPENDED", true),  // T112: re-authored, was "MARKET SUSPENDED"
                    ("Pending window",   "SUSPENDED", true),  // T112: re-authored, was "MARKET SUSPENDED"
                    ("Unavailable",      string.Empty, false),
                    ("Accepted",         "CASHED OUT", true),   // T114-am: the banner DROPPED its amount
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

                TMP_Text need = FindChild<TMP_Text>(screen, "LegRowNeed0");
                TMP_Text progress = FindChild<TMP_Text>(screen, "LegRowProgress0");
                TMP_Text line = FindChild<TMP_Text>(screen, "LegRowLine0");
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

                TMP_Text need0 = FindChild<TMP_Text>(screen, "LegRowNeed0");
                TMP_Text need1 = FindChild<TMP_Text>(screen, "LegRowNeed1");
                TMP_Text progress0 = FindChild<TMP_Text>(screen, "LegRowProgress0");
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
                TMP_Text need = FindChild<TMP_Text>(screen, "LegRowNeed0");
                TMP_Text progress = FindChild<TMP_Text>(screen, "LegRowProgress0");
                TMP_Text need1 = FindChild<TMP_Text>(screen, "LegRowNeed1");
                Assert.IsNotNull(need); Assert.IsNotNull(progress); Assert.IsNotNull(need1);

                Assert.IsNotNull(need.font, "no font resolved — a measurement in the fallback face is void");
                Assert.IsTrue(need.font.name.Contains("Encode"),
                    $"measured in '{need.font.name}', not Encode Sans. T24 asks for the PRODUCTION " +
                    "face; a measurement in any other face is the mistake T20 already made once.");

                // Real rendered heights, with the longest strings the production model can actually
                // emit today — not the longest §6's wireframe once sketched. Two phantoms retired here.
                //
                // NOT 'MARCUS VALE TO SCORE': PHANTOM. T69/G1 retired the full-name NEED —
                // SweatActiveLegModel.cs:551 emits $"{Surname(l.BackedPlayerName).ToUpperInvariant()} TO
                // SCORE", surname only, the exact substitution that turned T69's overrun
                // "RICO LANYARD TO SCORE" into "RICO LANYARD TO". The full name cannot reach this field.
                //
                // NOT 'LIVE • 0 GOALS • 3 MORE': PHANTOM, and never anything but. No live-progress arm
                // in SweatActiveLegModel.cs carries a `LIVE •` prefix, and none ever has — this file's
                // whole git history never adds or removes that substring. The form traces to
                // docs/tv-sweat-refinement/VISUAL-DESIGN.md's wireframe mockup, which the build never
                // implemented literally: `LIVE` became the leg row's own pulsing state badge instead
                // (DESIGN.md §8, "Leg states" — "the surface's only slow pulse"), and the progress field
                // itself stayed bare.
                //
                // Replaced with DescribeBttsNo's own pair (SweatActiveLegModel.cs:466,474): the
                // clean-sheet arm's live string and its NEED fallback are ONE method's two return
                // values — a real co-occurring row, not two worst cases stitched together.
                //
                // `preferredHeight` is a function of face and size, never of string content — the
                // assertion below never depended on either phantom, which is exactly why two
                // unproducible strings sat here unnoticed. A fixture nothing asserts against content is
                // where a phantom survives.
                need.text = "ONE TEAM BLANKED";
                progress.text = "CLEAN-SHEET PATH LIVE";
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
        /// <summary>T95: the punch overlay and the scoreline must occupy THE SAME RECT.
        ///
        /// <para>`Score` is a brightness event on the same string — its own build comment says "Same
        /// text, SAME RECT, same face as _tMatchup ... so superimposing it and boosting to L4 can only
        /// make the existing scoreline brighter." Both are UpperCenter, so each centres its string in
        /// ITS OWN box: two centred layers with different boxes do not superimpose, they offset by the
        /// difference of their centres, and the scoreline renders as two copies.</para>
        ///
        /// <para><b>This is a defect that shipped.</b> T91-am re-bounded `Matchup` and the mirror was
        /// not re-derived, so the boxes went 593.0 against 675.0 and the centres 92.7 against 133.7 —
        /// a 41.0px doubled scoreline on every beat the punch fired, found by the DD on frames at
        /// review distance and invisible to every instrument this surface had. The rects are shared by
        /// construction now; this is the pin that says so, because a shared local is a convention and
        /// an assertion is a contract.</para></summary>
        [Test]
        public void T95_the_punch_overlay_and_the_scoreline_share_one_rect()
        {
            var go = new GameObject("T95Rect");
            try
            {
                var screen = BuildScreen(go);
                TMP_Text matchup = FindChild<TMP_Text>(screen, "Matchup");
                TMP_Text punch = FindChild<TMP_Text>(screen, "Score");
                Assert.IsNotNull(matchup, "Matchup not found");
                Assert.IsNotNull(punch, "Score (the punch overlay) not found");

                Rect m = matchup.rectTransform.rect, p = punch.rectTransform.rect;
                Assert.AreEqual(m.width, p.width, 0.01f,
                    $"T95: the punch overlay's box is {p.width:0.0} against the scoreline's {m.width:0.0} — " +
                    "two centred layers with different boxes render the scoreline twice");
                Assert.AreEqual(m.height, p.height, 0.01f, "T95: the punch overlay's height must match too");
                Assert.AreEqual(matchup.rectTransform.anchoredPosition.x, punch.rectTransform.anchoredPosition.x, 0.01f,
                    "T95: same box, different position, is the same defect — the layers must superimpose");
                Assert.AreEqual(matchup.alignment, punch.alignment,
                    "T95: a shared rect only superimposes while the alignment is shared too");
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void T15_measure_the_risk_pays_cell_in_the_production_face()
        {
            var go = new GameObject("T15Measure");
            try
            {
                var screen = BuildScreen(go);
                TMP_Text riskPays = FindChild<TMP_Text>(screen, "RiskPays");   // condensed, per TvRiskPays.jsx:14
                TMP_Text eventLine = FindChild<TMP_Text>(screen, "Flavor");    // regular, per TvEventStrip.jsx:10
                Assert.IsNotNull(riskPays, "RiskPays not found");
                Assert.IsNotNull(eventLine, "Flavor not found");
                Assert.IsNotNull(riskPays.font, "no font resolved — a measurement in the fallback is void");
                Assert.IsTrue(riskPays.font.name.Contains("Encode"),
                    $"measured in '{riskPays.font.name}', not Encode Sans — the same mistake T20 made once");

                TMP_Text label = MeasureText(riskPays.transform.parent, eventLine.font, 15, "PAYS");
                TMP_Text value = MeasureText(riskPays.transform.parent, riskPays.font, 24, "$1,234");

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

        /// <summary>The §3.3 height re-derivation gated by §4 of
        /// <c>docs/design/spec-ticket-footer-2026-08-19.md</c>. The ruling is Allen's — T144 takes
        /// T74-am3's separate rows — and the identical ruling was WITHDRAWN once already, at batch 59
        /// (T74-am5), precisely because the height was never re-derived.
        ///
        /// <para>This is that re-derivation, taken at the real face rather than reasoned from
        /// <c>LineBox</c>. It REPORTS rather than RULES — every number below is logged, not asserted,
        /// because the layout call belongs to the Design Director and a failing assert here would
        /// fail the suite for a design reason, not a code defect (T15's own precedent).</para></summary>
        [Test]
        public void T144_the_two_row_footer_height_is_re_derived_against_the_live_row()
        {
            var go = new GameObject("T144FooterHeight");
            try
            {
                var screen = BuildScreen(go);
                TMP_Text riskPays = FindChild<TMP_Text>(screen, "RiskPays");
                TMP_Text need = FindChild<TMP_Text>(screen, "LegRowNeed0");
                TMP_Text need1 = FindChild<TMP_Text>(screen, "LegRowNeed1");
                TMP_Text progress = FindChild<TMP_Text>(screen, "LegRowProgress0");
                Image columnZone = FindChild<Image>(screen, "TicketColumnZone");
                Assert.IsNotNull(riskPays, "RiskPays not found");
                Assert.IsNotNull(need, "LegRowNeed0 not found");
                Assert.IsNotNull(need1, "LegRowNeed1 not found");
                Assert.IsNotNull(progress, "LegRowProgress0 not found");
                Assert.IsNotNull(columnZone, "TicketColumnZone not found");

                Assert.IsNotNull(riskPays.font, "no font resolved — a measurement in the fallback face is void");
                Assert.IsTrue(riskPays.font.name.Contains("Encode"),
                    $"measured in '{riskPays.font.name}', not Encode Sans — the same mistake T20 made once");

                // Geometry read off the built objects, never recomputed from the constants the code
                // under test uses (T20RowFit's own discipline).
                float columnH = columnZone.rectTransform.sizeDelta.y;   // the ticket column's whole vertical budget
                float rowPitch = Mathf.Abs(need1.rectTransform.anchoredPosition.y
                                         - need.rectTransform.anchoredPosition.y);
                // Read off the built objects. NOT `riskPays.sizeDelta.y + 8` — that inference died
                // with the one-row footer (see FooterHeight) — and NOT a hard-coded slot count,
                // which T147-am moved from 6 to 4.
                TMP_Text paysRow = FindChild<TMP_Text>(screen, "Pays");
                TMP_Text headerRow = FindChild<TMP_Text>(screen, "TicketHeader");
                Assert.IsNotNull(paysRow, "Pays not found");
                Assert.IsNotNull(headerRow, "TicketHeader not found");
                float footerH = FooterHeight(riskPays, paysRow);
                float headerH = HeaderHeight(headerRow);
                int slots = Mathf.RoundToInt((columnH - headerH - footerH) / rowPitch);
                float boxW = riskPays.rectTransform.sizeDelta.x;   // the footer's inner box

                Debug.Log($"[T144] column budget = {columnH:0.0}px, row pitch = {rowPitch:0.0}px, " +
                          $"footer (today) = {footerH:0.0}px, derived header = {headerH:0.0}px, " +
                          $"footer inner box = {boxW:0.0}px");

                // Measured on the REAL RiskPays component via GetPreferredValues, not the MeasureText
                // throwaway T15 uses: the throwaway sets fontStyle = Bold on a face that is already
                // Bold 700 and carries none of the slot's characterSpacing, so its width would be a
                // faux-bold approximation of a slot that does not exist. GetPreferredValues on the
                // built component measures through the real face, the real weight and the real
                // characterSpacing — the same path TvSweatScreen.MakeText sets and TvExtentSweep
                // measures through. Height is unaffected either way (TMP takes line height from the
                // font asset, not the style), but width is the number spec §4.3 wants against the
                // enumerated pool, so it must come from the real slot.
                //
                // Unconstrained is not defined in this file — it is TvExtentSweep's own constant,
                // mirrored in TvSweatScreen.cs, TvPromptComposition.cs and TvSweatScreenTests.cs.
                // Declared locally, not at class scope, so this change stays exactly one test. Not
                // float.PositiveInfinity: TMP multiplies the width constraint into its layout maths,
                // and TvSweatScreen.cs's own comment on this constant says a value that large returns
                // infinities — 100000f is the value the rest of this codebase already uses for "no
                // string on this surface can reach this width."
                const float Unconstrained = 100000f;

                // Row 1, the stake fact — two candidates (spec §3.2 puts STAKE above RETURNED in the
                // settled state). Row 2, the return fact — three candidates, because T133's word is
                // still open with the Design Director.
                Vector2 riskV = riskPays.GetPreferredValues("RISK $13,639", Unconstrained, 0f);
                Vector2 stakeV = riskPays.GetPreferredValues("STAKE $13,639", Unconstrained, 0f);
                Vector2 paysV = riskPays.GetPreferredValues("PAYS $73,318,376,502", Unconstrained, 0f);
                Vector2 returnedV = riskPays.GetPreferredValues("RETURNED $73,318,376,502", Unconstrained, 0f);
                Vector2 paidV = riskPays.GetPreferredValues("PAID $73,318,376,502", Unconstrained, 0f);

                Debug.Log($"[T144] RISK worst case 'RISK $13,639' = {riskV.x:0.0}w x {riskV.y:0.0}h — " +
                          (riskV.x <= boxW
                              ? $"fits the {boxW:0.0}px footer box"
                              : $"OVERRUNS the {boxW:0.0}px footer box by {riskV.x - boxW:0.0}px"));
                Debug.Log($"[T144] STAKE worst case 'STAKE $13,639' = {stakeV.x:0.0}w x {stakeV.y:0.0}h — " +
                          (stakeV.x <= boxW
                              ? $"fits the {boxW:0.0}px footer box"
                              : $"OVERRUNS the {boxW:0.0}px footer box by {stakeV.x - boxW:0.0}px"));
                Debug.Log($"[T144] PAYS worst case 'PAYS $73,318,376,502' = {paysV.x:0.0}w x {paysV.y:0.0}h — " +
                          (paysV.x <= boxW
                              ? $"fits the {boxW:0.0}px footer box"
                              : $"OVERRUNS the {boxW:0.0}px footer box by {paysV.x - boxW:0.0}px"));
                Debug.Log($"[T144] RETURNED worst case 'RETURNED $73,318,376,502' = {returnedV.x:0.0}w x {returnedV.y:0.0}h — " +
                          (returnedV.x <= boxW
                              ? $"fits the {boxW:0.0}px footer box"
                              : $"OVERRUNS the {boxW:0.0}px footer box by {returnedV.x - boxW:0.0}px"));
                Debug.Log($"[T144] PAID worst case 'PAID $73,318,376,502' = {paidV.x:0.0}w x {paidV.y:0.0}h — " +
                          (paidV.x <= boxW
                              ? $"fits the {boxW:0.0}px footer box"
                              : $"OVERRUNS the {boxW:0.0}px footer box by {paidV.x - boxW:0.0}px"));

                // §2 of the spec claims separate rows lets both facts clear their enumerated worst
                // case at full width — true only for the word that actually ends up on the row.
                Debug.Log("[T144] row 2 at full width: " +
                          $"PAYS {(paysV.x <= boxW ? "CLEARS" : "OVERRUNS")}, " +
                          $"RETURNED {(returnedV.x <= boxW ? "CLEARS" : "OVERRUNS")}, " +
                          $"PAID {(paidV.x <= boxW ? "CLEARS" : "OVERRUNS")}");

                // Word choice changes WIDTH (measured above), never height — TMP takes line height
                // from the font asset, not from string content — so the taller of the two rows' line
                // boxes serves all five candidates; this is not five different heights.
                float row1H = Mathf.Max(riskV.y, stakeV.y);
                float row2H = Mathf.Max(paysV.y, Mathf.Max(returnedV.y, paidV.y));
                float lineBox = Mathf.Max(row1H, row2H);
                float observedRatio = lineBox / 24f;
                Debug.Log($"[T144] line box = {lineBox:0.0}px (word choice changes width, not line " +
                          "height, so one height serves all five candidates) at size 24 -> observed " +
                          $"ratio {observedRatio:0.00}, against the LineBox design constant 1.18 and " +
                          "the real advance ratio 1.25 established at T74-am3 — the spec requires the " +
                          "1.25 measurement, never the 1.18 constant.");

                float twoRowBare = 2f * lineBox;              // two rows, zero padding
                float twoRowPadded = 8f + 2f * lineBox;       // two rows, keeping today's 8px top inset
                Debug.Log($"[T144] two-row bare (0 padding) = {twoRowBare:0.0}px; " +
                          $"two-row padded (today's 8px top inset) = {twoRowPadded:0.0}px");

                float footerRowHeight = (columnH - headerH - footerH) / slots;
                float bareRowHeight = (columnH - headerH - twoRowBare) / slots;
                float paddedRowHeight = (columnH - headerH - twoRowPadded) / slots;
                Debug.Log($"[T144] footer (today) F={footerH:0.0} -> row height {footerRowHeight:0.0}");
                Debug.Log($"[T144] two-row bare F={twoRowBare:0.0} -> row height {bareRowHeight:0.0}");
                Debug.Log($"[T144] two-row padded F={twoRowPadded:0.0} -> row height {paddedRowHeight:0.0}");

                // The live row, measured exactly as T24 does — including T24's fixture, copied
                // verbatim, which means it carried T24's same two phantoms: 'MARCUS VALE TO SCORE'
                // (full name; T69/G1 retired it — SweatActiveLegModel.cs:551 emits surname only) and
                // 'LIVE • 0 GOALS • 3 MORE' (no `LIVE •` prefix has ever existed on any live-progress
                // arm in SweatActiveLegModel.cs — see T24's fixture comment above for the full account).
                // Replaced here with the same honest pair: DescribeBttsNo's NEED fallback and live
                // string (SweatActiveLegModel.cs:466,474), one method's two return values.
                //
                // This matters MORE here than at T24: T144 is the gate instrument, and the numbers it
                // reports below (liveInk, footerCeiling, the CLEARS/SHORT verdicts) are Debug.Log output
                // quoted to the Design Director as evidence. `preferredHeight` is a function of face and
                // size, never of string content, so those reported numbers were never wrong and do not
                // move now. But a phantom sitting in an INSTRUMENT's fixture is worse than one sitting in
                // a plain test's: this method asserts nothing against the string either (only the
                // font-face check above does), so a ruling could be made on numbers this instrument
                // produced while naming a player and a progress line the surface cannot actually show.
                // That is exactly how it went unnoticed.
                need.text = "ONE TEAM BLANKED";
                progress.text = "CLEAN-SHEET PATH LIVE";
                float liveInk = need.preferredHeight + progress.preferredHeight;
                float liveNeed = liveInk + 8f;   // T24's own margin: 4px top pad + the row's bottom breathing
                Debug.Log($"[T144] live row ink = {liveInk:0.0}px (NEED {need.preferredHeight:0.0} + " +
                          $"progress {progress.preferredHeight:0.0}) with T24's margin = {liveNeed:0.0}px");

                float footerCeiling = columnH - headerH - slots * liveNeed;
                Debug.Log("[T144] footer ceiling (largest footer that still leaves every live row its " +
                          $"T24 margin) = {footerCeiling:0.0}px");

                float deficitBare = twoRowBare - footerCeiling;
                float deficitPadded = twoRowPadded - footerCeiling;
                Debug.Log("[T144] two-row bare vs ceiling: " + (deficitBare <= 0f
                    ? $"CLEARS by {-deficitBare:0.0}px"
                    : $"SHORT by {deficitBare:0.0}px"));
                Debug.Log("[T144] two-row padded vs ceiling: " + (deficitPadded <= 0f
                    ? $"CLEARS by {-deficitPadded:0.0}px"
                    : $"SHORT by {deficitPadded:0.0}px"));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>§4.1 of <c>spec-ticket-footer-2026-08-19.md</c>, as re-ruled at batch 133
        /// (<c>T147-am</c>): the live leg row re-derived against the row height the composition
        /// leaves behind. The ruling drops <c>TicketRowSlots</c> 6 → 4 to match
        /// <c>RunConfig.MaxLegs</c> and grows the footer 40 → 60.
        ///
        /// <para>§4.1 asks for THREE lines — the compact statement, the NEED and the progress.
        /// Today exactly one FORM carries text at a time: compact for resolved/pending rows, NEED +
        /// progress for the live one. All three are measured here anyway, because the canon
        /// three-line row T24 CUT for want of ~73px against a 70px slot becomes a different question
        /// at a 99px slot, and the gate should say so rather than leave it to be rediscovered.</para>
        ///
        /// <para>REPORTS, never rules — same standing as T15 and T144. Measured at the real face on
        /// the real components, so the widths carry the slot's own tracking and weight.</para></summary>
        [Test]
        public void T147_the_live_rows_three_lines_are_re_derived_at_the_four_slot_row_height()
        {
            var go = new GameObject("T147RowDerive");
            try
            {
                var screen = BuildScreen(go);
                TMP_Text compact = FindChild<TMP_Text>(screen, "LegRowLine0");
                TMP_Text need = FindChild<TMP_Text>(screen, "LegRowNeed0");
                TMP_Text progress = FindChild<TMP_Text>(screen, "LegRowProgress0");
                Image columnZone = FindChild<Image>(screen, "TicketColumnZone");
                TMP_Text riskPays = FindChild<TMP_Text>(screen, "RiskPays");
                Assert.IsNotNull(compact, "LegRowLine0 not found");
                Assert.IsNotNull(need, "LegRowNeed0 not found");
                Assert.IsNotNull(progress, "LegRowProgress0 not found");
                Assert.IsNotNull(columnZone, "TicketColumnZone not found");
                Assert.IsNotNull(riskPays, "RiskPays not found");
                Assert.IsNotNull(need.font, "no font resolved — a measurement in the fallback face is void");
                Assert.IsTrue(need.font.name.Contains("Encode"),
                    $"measured in '{need.font.name}', not Encode Sans — the same mistake T20 made once");

                // Geometry read off the built objects, never recomputed from the constants under test.
                float columnH = columnZone.rectTransform.sizeDelta.y;
                TMP_Text paysRow = FindChild<TMP_Text>(screen, "Pays");
                TMP_Text headerRow = FindChild<TMP_Text>(screen, "TicketHeader");
                Assert.IsNotNull(paysRow, "Pays not found");
                Assert.IsNotNull(headerRow, "TicketHeader not found");
                float footerH = FooterHeight(riskPays, paysRow);
                float headerH = HeaderHeight(headerRow);
                TMP_Text need1 = FindChild<TMP_Text>(screen, "LegRowNeed1");
                float pitchNow = need1 != null
                    ? Mathf.Abs(need1.rectTransform.anchoredPosition.y - need.rectTransform.anchoredPosition.y)
                    : 0f;
                int slotsNow = pitchNow > 0f ? Mathf.RoundToInt((columnH - headerH - footerH) / pitchNow) : 0;

                // The longest authored forms §6 permits — T24's own strings for the live pair, and
                // T90-am's widest compact statement for the compact line.
                // Bound to locals and logged FROM the locals: the first cut of this instrument logged
                // one string name while measuring another, which is the phantom problem one layer down.
                const string CompactProbe = "LANYARD TO SCORE";
                const string NeedWidest = "ONE TEAM BLANKED";
                const string ProgressWidest = "CLEAN-SHEET PATH LIVE";
                Vector2 compactV = compact.GetPreferredValues(CompactProbe, 100000f, 0f);
                // NOT T24's 'MARCUS VALE TO SCORE': T69/G1 retired the full-name form and
                // SweatActiveLegModel.cs:551 emits $"{Surname(...)} TO SCORE", so that string is a
                // PHANTOM the surface can no longer produce (it measures 300.3 against a 261.0 box and
                // would read as a 39.3px overrun that cannot occur). T90-am's widest EMITTABLE NEED is
                // used instead. Height is string-independent either way; the WIDTH is why this matters.
                //
                // NOT T24's 'LIVE • 0 GOALS • 3 MORE' either: that one was never emittable at all, not
                // retired but never built — no live-progress arm in SweatActiveLegModel.cs has ever
                // carried a `LIVE •` prefix (they build `{total} GOALS {Bullet} {remaining} MORE` and
                // its siblings; the form traces only to docs/tv-sweat-refinement/VISUAL-DESIGN.md's
                // wireframe, where `LIVE` was a separate pulsing state badge, DESIGN.md §8, not text
                // glued onto the progress field). The T84 sweep's widest EMITTABLE progress form is used
                // instead: DescribeBttsNo's own live string (SweatActiveLegModel.cs:466), the same
                // method that returns T90-am's NEED fallback above (:474) — one market's real row, not
                // two worst cases from different markets. Same as the NEED phantom, height here is
                // string-independent; the WIDTH is why this one matters too.
                Vector2 needV = need.GetPreferredValues(NeedWidest, 100000f, 0f);
                Vector2 progV = progress.GetPreferredValues(ProgressWidest, 100000f, 0f);

                Debug.Log($"[T147] column {columnH:0.0} = header {headerH:0.0} + {slotsNow} x pitch "
                          + $"{pitchNow:0.0} + footer {footerH:0.0}");
                // HEIGHT PROBE ONLY. A TMP line box is a function of face and size, never of the
                // string, so any string gives the compact line's true height — but this one is T90-am's
                // NEED worst case, NOT the compact line's, and its width here is therefore not an
                // extent verdict. The compact statement's own extent belongs to the T84 sweep.
                Debug.Log($"[T147] compact  line box {compactV.y:0.0}h at the compact size (height probe; "
                          + $"width {compactV.x:0.0} is NOT this slot's worst case — see the T84 sweep)");
                Debug.Log($"[T147] NEED     '{NeedWidest}' {needV.x:0.0}w x {needV.y:0.0}h "
                          + $"against box {need.rectTransform.sizeDelta.x:0.0}w (T90's band)");
                Debug.Log($"[T147] progress '{ProgressWidest}' {progV.x:0.0}w x {progV.y:0.0}h "
                          + $"against box {progress.rectTransform.sizeDelta.x:0.0}w");

                const float T24Margin = 8f;   // T24's pinned margin: 4px top pad + the row's breathing
                float liveTwoLine = needV.y + progV.y;
                float canonThreeLine = compactV.y + needV.y + progV.y;

                // The ruled geometry: four slots, a 60px footer, everything else derived.
                const float RuledSlots = 4f, RuledFooter = 60f;
                float ruledPitch = (columnH - headerH - RuledFooter) / RuledSlots;

                // Once the composition is BUILT these two agree, and saying so is the point: the
                // ruling's arithmetic and the surface's geometry are then one number, not two.
                Debug.Log($"[T147] RULED geometry: {RuledSlots:0} slots, footer {RuledFooter:0.0} "
                          + $"-> row pitch {ruledPitch:0.0}px · BUILT: {slotsNow} slots, footer "
                          + $"{footerH:0.0}, pitch {pitchNow:0.0}px — "
                          + (Mathf.Abs(ruledPitch - pitchNow) < 0.05f && slotsNow == (int)RuledSlots
                             ? "BUILT MATCHES RULED"
                             : "BUILT DOES NOT MATCH RULED — one of them has drifted"));
                Debug.Log($"[T147] live row as BUILT (NEED + progress) = {liveTwoLine:0.0} + "
                          + $"{T24Margin:0.0} margin = {liveTwoLine + T24Margin:0.0} vs {ruledPitch:0.0} — "
                          + (liveTwoLine + T24Margin <= ruledPitch
                             ? $"CLEARS by {ruledPitch - liveTwoLine - T24Margin:0.0}px"
                             : $"SHORT by {liveTwoLine + T24Margin - ruledPitch:0.0}px"));
                Debug.Log($"[T147] canon THREE-line row (compact + NEED + progress) = "
                          + $"{canonThreeLine:0.0} + {T24Margin:0.0} margin = "
                          + $"{canonThreeLine + T24Margin:0.0} vs {ruledPitch:0.0} — "
                          + (canonThreeLine + T24Margin <= ruledPitch
                             ? $"CLEARS by {ruledPitch - canonThreeLine - T24Margin:0.0}px. T24 cut this "
                               + "form for want of room at a 70px slot; at this pitch it fits. REPORTED, "
                               + "not proposed — restoring it is a ruling, not a build decision."
                             : $"SHORT by {canonThreeLine + T24Margin - ruledPitch:0.0}px — T24's cut stands"));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>Spec §4.2's pair check (<c>docs/design/spec-ticket-footer-2026-08-19.md</c>), in
        /// its SECOND form. §4.2, verbatim: "assert `RiskPays` ink + `Pays` ink against the row they
        /// share — or, once they no longer share one, assert each against its own row and assert the
        /// rows do not overlap. Two independent green checks are what let this ship." T147-am built
        /// separate rows, so the first form — one shared row, T144's own check — no longer applies;
        /// this is the second.
        ///
        /// <para>CHECK ONE: each row's enumerated worst case — <c>RISK $13,639</c> on row 1,
        /// <c>PAYS $73,318,376,502</c> on row 2 (the live state's own strings; T144's pool,
        /// <c>PayoutMaximumTests</c>) — fits its OWN 249.0px box. Measured on the real
        /// <c>RiskPays</c>/<c>Pays</c> components via <c>GetPreferredValues</c>, never
        /// <c>float.PositiveInfinity</c> (TMP returns infinities at that width) and never the
        /// <c>MeasureText</c> throwaway (it applies faux-bold to a face already Bold 700).</para>
        ///
        /// <para>CHECK TWO: the two rows do not overlap. Both are built top-left anchored with pivot
        /// (0,1) (<c>BuildTicketColumn</c>'s two <c>MakeText</c> calls), so this reads
        /// <c>anchoredPosition.y</c> and <c>sizeDelta.y</c> directly.</para>
        ///
        /// <para>The settled-state row is REPORTED, never asserted: <c>RETURNED $73,318,376,502</c>
        /// overruns its own row, and that is <c>T133</c>, still open with the Design Director —
        /// separate rows was ruled to fix the PAIR collision (<c>T74-am6</c>), never claimed to fix
        /// this, and spec §2 was corrected at batch 133 to say exactly that.</para></summary>
        [Test]
        public void T147_the_two_footer_rows_each_clear_their_own_box_and_do_not_overlap()
        {
            var go = new GameObject("T147TwoRows");
            try
            {
                var screen = BuildScreen(go);
                TMP_Text riskPays = FindChild<TMP_Text>(screen, "RiskPays");
                TMP_Text pays = FindChild<TMP_Text>(screen, "Pays");
                Assert.IsNotNull(riskPays, "RiskPays not found");
                Assert.IsNotNull(pays, "Pays not found");
                Assert.IsNotNull(riskPays.font, "no font resolved — a measurement in the fallback face is void");
                Assert.IsTrue(riskPays.font.name.Contains("Encode"),
                    $"measured in '{riskPays.font.name}', not Encode Sans — the same mistake T20 made once");

                // CHECK ONE (spec §4.2, second form): each row's own enumerated worst case against its
                // own box. sizeDelta.x is each row's real built width (249.0, footerBoxW in
                // BuildTicketColumn) — read off the component, never recomputed from the constants
                // under test (T20RowFit's own discipline). 100000f, not float.PositiveInfinity: TMP
                // multiplies the width constraint into its layout maths and a value that large returns
                // infinities (T144's own note on this same call).
                float riskBoxW = riskPays.rectTransform.sizeDelta.x;
                float paysBoxW = pays.rectTransform.sizeDelta.x;
                Vector2 riskV = riskPays.GetPreferredValues("RISK $13,639", 100000f, 0f);
                Vector2 paysV = pays.GetPreferredValues("PAYS $73,318,376,502", 100000f, 0f);

                Assert.LessOrEqual(riskV.x, riskBoxW,
                    $"RiskPays (row 1) worst case 'RISK $13,639' measures {riskV.x:0.0}px, which " +
                    $"overruns its own {riskBoxW:0.0}px box by {riskV.x - riskBoxW:0.0}px — row 1 no " +
                    "longer clears its enumerated worst case.");
                Assert.LessOrEqual(paysV.x, paysBoxW,
                    $"Pays (row 2) worst case 'PAYS $73,318,376,502' measures {paysV.x:0.0}px, which " +
                    $"overruns its own {paysBoxW:0.0}px box by {paysV.x - paysBoxW:0.0}px — row 2 no " +
                    "longer clears its enumerated worst case.");

                // CHECK TWO (spec §4.2, second form): the rows do not overlap. Both rows are built
                // top-left anchored with pivot (0,1), and AnchorTopLeft returns -(zone.y + pad) —
                // Unity's canvas y is NEGATIVE downward on this surface — so anchoredPosition.y is
                // already a negated distance from the footer's top. Negate it back here to reason in
                // absolute, downward-positive px, where "top" and "bottom" read the way a person reads
                // a screen instead of by chasing a double negative.
                float riskTop = -riskPays.rectTransform.anchoredPosition.y;
                float riskBottom = riskTop + riskPays.rectTransform.sizeDelta.y;
                float paysTop = -pays.rectTransform.anchoredPosition.y;
                float paysBottom = paysTop + pays.rectTransform.sizeDelta.y;
                float overlap = riskBottom - paysTop;   // > 0 means row 2's top sits above row 1's bottom

                Assert.LessOrEqual(overlap, 0.01f,
                    $"the two footer rows overlap by {overlap:0.0}px — RiskPays runs " +
                    $"{riskTop:0.0}..{riskBottom:0.0}, Pays runs {paysTop:0.0}..{paysBottom:0.0}. The " +
                    "lower row's top must sit at or below the upper row's bottom.");

                // REPORT ONLY, never assert — T144's own standing: the layout call belongs to the
                // Design Director, and a failing assert here would fail the suite for a design reason,
                // not a code defect. The settled-state strings against each row's own 249.0px box.
                Vector2 stakeV = riskPays.GetPreferredValues("STAKE $13,639", 100000f, 0f);
                Vector2 returnedV = pays.GetPreferredValues("RETURNED $73,318,376,502", 100000f, 0f);
                Vector2 paidV = pays.GetPreferredValues("PAID $73,318,376,502", 100000f, 0f);

                Debug.Log($"[T147] row 1 settled 'STAKE $13,639' = {stakeV.x:0.0}w — " +
                          (stakeV.x <= riskBoxW
                              ? $"fits the {riskBoxW:0.0}px row 1 box"
                              : $"OVERRUNS the {riskBoxW:0.0}px row 1 box by {stakeV.x - riskBoxW:0.0}px"));
                Debug.Log($"[T147] row 2 live 'PAYS $73,318,376,502' = {paysV.x:0.0}w — " +
                          (paysV.x <= paysBoxW
                              ? $"fits the {paysBoxW:0.0}px row 2 box"
                              : $"OVERRUNS the {paysBoxW:0.0}px row 2 box by {paysV.x - paysBoxW:0.0}px"));
                // EXPECTED to overrun (~300.9 against 249.0, ~51.9px over, per the spec's own batch-133
                // measurement) — this is T133, still open with the Design Director. Separate rows fixes
                // the PAIR collision (T74-am6) and was never claimed to fix this; spec §2 was corrected
                // at batch 133 to say exactly that. DO NOT "fix" this by asserting — the word is the
                // DD's call, not this suite's.
                Debug.Log($"[T147] row 2 settled 'RETURNED $73,318,376,502' = {returnedV.x:0.0}w — " +
                          (returnedV.x <= paysBoxW
                              ? $"fits the {paysBoxW:0.0}px row 2 box"
                              : $"OVERRUNS the {paysBoxW:0.0}px row 2 box by {returnedV.x - paysBoxW:0.0}px (EXPECTED — T133, open with the DD)"));
                Debug.Log($"[T147] row 2 settled 'PAID $73,318,376,502' = {paidV.x:0.0}w — " +
                          (paidV.x <= paysBoxW
                              ? $"fits the {paysBoxW:0.0}px row 2 box"
                              : $"OVERRUNS the {paysBoxW:0.0}px row 2 box by {paidV.x - paysBoxW:0.0}px"));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>The four measurements the Design Director routed at batches 151 and 153, taken
        /// through the SURFACE'S OWN entry points rather than through a replica of them.
        ///
        /// <para><c>FitToColumn</c> and <c>FitOrFallback</c> are private statics on
        /// <c>TvSweatScreen</c> and are reached by reflection. A reimplementation that drifted from
        /// the real method would produce numbers that look right and mean nothing, and every one of
        /// these questions is about what the surface ACTUALLY does to a string.</para>
        ///
        /// <para>The two slots take DIFFERENT entry points, which is the part that is easy to get
        /// wrong: the compact statement is assigned through <c>FitToColumn</c> directly (`:2985`),
        /// while NEED goes through <c>FitOrFallback</c> (`:3064`), whose own fall-through truncates
        /// the FALLBACK when both rungs miss.</para>
        ///
        /// <para>REPORT-ONLY. The rulings are the DD's.</para></summary>
        [Test]
        public void T157_the_routed_truncation_measurements_for_the_blocked_kinds()
        {
            var go = new GameObject("T157Truncation");
            try
            {
                var screen = BuildScreen(go);
                TMP_Text need = FindChild<TMP_Text>(screen, "LegRowNeed0");
                TMP_Text line = FindChild<TMP_Text>(screen, "LegRowLine0");
                Assert.IsNotNull(need, "LegRowNeed0 not found");
                Assert.IsNotNull(line, "LegRowLine0 not found");
                Assert.IsNotNull(need.font, "no font resolved — a measurement in the fallback face is void");
                Assert.IsTrue(need.font.name.Contains("Encode"),
                    $"measured in '{need.font.name}', not Encode Sans — the same mistake T20 made once");

                MethodInfo fitToColumn = typeof(TvSweatScreen).GetMethod("FitToColumn",
                    BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo fitOrFallback = typeof(TvSweatScreen).GetMethod("FitOrFallback",
                    BindingFlags.NonPublic | BindingFlags.Static);
                Assert.IsNotNull(fitToColumn, "TvSweatScreen.FitToColumn not found by reflection — renamed?");
                Assert.IsNotNull(fitOrFallback, "TvSweatScreen.FitOrFallback not found by reflection — renamed?");

                float needBox = need.rectTransform.sizeDelta.x;
                float lineBox = line.rectTransform.sizeDelta.x;
                Debug.Log($"[T157] boxes :: NEED {needBox:0.0}px · compact {lineBox:0.0}px");

                // ---- (1) WHAT THE MARGIN LINE TRUNCATES TO -------------------------------------
                // Both WinningMargin NEED rungs overrun, so FitOrFallback falls through to
                // FitToColumn(target, FALLBACK) and drops whole words from the end. FitToColumn's
                // dangling-token cleanup matches only " v", " ·" and " —", so nothing strips a
                // trailing preposition. The DD asked whether it stops on "AT".
                foreach ((string primary, string fallback) in new[]
                {
                    ("3+ GOALS APART AT FULL TIME", "3+ GOALS APART AT FT"),
                    ("2 GOALS APART AT FULL TIME", "2 GOALS APART AT FT"),
                })
                {
                    float wp = need.GetPreferredValues(primary, 100000f, 0f).x;
                    float wf = need.GetPreferredValues(fallback, 100000f, 0f).x;
                    var rendered = (string)fitOrFallback.Invoke(null, new object[] { need, primary, fallback });
                    float wr = need.GetPreferredValues(rendered, 100000f, 0f).x;
                    string which = rendered == primary ? "RUNG 1"
                                 : rendered == fallback ? "RUNG 2"
                                 : "TRUNCATED";
                    Debug.Log($"[T157-MARGIN] '{primary}' {wp:0.0} / '{fallback}' {wf:0.0} "
                              + $"-> {which} '{rendered}' {wr:0.0}px vs box {needBox:0.0}px"
                              + (rendered.EndsWith(" AT") ? "  ** ENDS ON THE DANGLING 'AT' **" : ""));
                }

                // ---- (2) DOUBLE CHANCE'S RUNGS -------------------------------------------------
                // Club pool is TvExtentSweep's own ClubNouns, mirrored here because that array is
                // private to the Editor assembly. Uppercased the way the surface uppercases it.
                string[] clubs =
                {
                    "YAMS", "STARTUPS", "BRICKLAYERS", "LONGHAULERS", "MALLARDS", "SPREADSHEETS",
                    "TURNIPS", "MIDDLEMEN", "REGULATORS", "PLUMBERS", "MEATBALLS", "AUDITORS",
                    "FERRETS", "OVERHEADS", "GRAVEDIGGERS", "NOTARIES", "MUSKRATS", "ZAMBONIS",
                    "LOOPHOLES", "REFUNDS",
                };

                int r1 = 0, r2 = 0, trunc = 0;
                foreach (string c in clubs)
                {
                    string primary = $"{c} TO WIN OR DRAW", fallback = $"{c} WIN OR DRAW";
                    var rendered = (string)fitOrFallback.Invoke(null, new object[] { need, primary, fallback });
                    if (rendered == primary) r1++;
                    else if (rendered == fallback) r2++;
                    else { trunc++; Debug.Log($"[T157-DC-NEED] '{primary}' -> TRUNCATED '{rendered}'"); }
                }
                Debug.Log($"[T157-DC-NEED] {clubs.Length} clubs :: rung 1 {r1} · rung 2 {r2} · truncated {trunc}");

                int compactTrunc = 0;
                var lostNoun = new List<string>();
                foreach (string c in clubs)
                {
                    string compact = $"{c} OR DRAW";
                    var rendered = (string)fitToColumn.Invoke(null, new object[] { line, compact });
                    if (rendered != compact)
                    {
                        compactTrunc++;
                        if (!rendered.Contains("DRAW")) lostNoun.Add($"'{compact}' -> '{rendered}'");
                    }
                }
                Debug.Log($"[T157-DC-COMPACT] {clubs.Length} clubs :: {compactTrunc} truncated; "
                          + $"{lostNoun.Count} LOST THE WORD 'DRAW' — the token that says which market this is");
                foreach (string ex in lostNoun) Debug.Log($"[T157-DC-COMPACT]   {ex}");

                // ---- (3) THE SCORER OVERRUN'S SOURCE -------------------------------------------
                // PlayerMultiScorer has NO fallback rung authored, so the fallback argument is null —
                // that is the authored state, not an omission here. The shipped {SURNAME} TO SCORE is
                // measured beside it to separate the surname's contribution from the " 2+" tail's.
                string[] surnames =
                {
                    "LEDGER", "CINDER", "MUFFIN", "PAVEMENT", "COUPON", "WOBBLE",
                    "GASKET", "PYLON", "KETCHUP", "LANYARD", "RACKET", "STAPLER",
                };
                int shippedOver = 0, newOver = 0;
                float worstDelta = 0f;
                foreach (string n in surnames)
                {
                    string shipped = $"{n} TO SCORE", multi = $"{n} TO SCORE 2+";
                    float ws = need.GetPreferredValues(shipped, 100000f, 0f).x;
                    float wm = need.GetPreferredValues(multi, 100000f, 0f).x;
                    if (ws > needBox) shippedOver++;
                    if (wm > needBox) newOver++;
                    worstDelta = Mathf.Max(worstDelta, wm - ws);
                    var rendered = (string)fitOrFallback.Invoke(null, new object[] { need, multi, null });
                    if (rendered != multi)
                        Debug.Log($"[T157-SCORER] '{multi}' {wm:0.0} -> '{rendered}'");
                }
                Debug.Log($"[T157-SCORER] {surnames.Length} surnames :: shipped '{{N}} TO SCORE' over box "
                          + $"{shippedOver} · new '{{N}} TO SCORE 2+' over box {newOver} · widest ' 2+' "
                          + $"tail cost {worstDelta:0.0}px — if shipped is 0 and new is not, the TAIL is "
                          + "the cause and the surname is not");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>`T91`'s two numbers, owed to the Design Director since 2026-08-13 and routed at
        /// batch 153 (`T91-am3`). `T91` needs no ruling — its ruling was made at `T91-am2`, batch 63.
        /// It needs these.
        ///
        /// <para>REPORT-ONLY, and the clock number deliberately reports its own BOUND rather than a
        /// single width: the DD's cell asks for the longest RENDERABLE form and says explicitly that
        /// the box is "the quantity in doubt", so answering with the box would answer the wrong
        /// question.</para></summary>
        [Test]
        public void T91_the_two_numbers_owed_since_batch_63()
        {
            var go = new GameObject("T91Numbers");
            try
            {
                var screen = BuildScreen(go);
                TMP_Text clock = FindChild<TMP_Text>(screen, "Clock");
                Assert.IsNotNull(clock, "Clock not found");
                Assert.IsNotNull(clock.font, "no font resolved — a measurement in the fallback face is void");
                Assert.IsTrue(clock.font.name.Contains("Encode"),
                    $"measured in '{clock.font.name}', not Encode Sans — the same mistake T20 made once");

                // ---- NUMBER 1: the clock's LONGEST RENDERABLE FORM -----------------------------
                // Enumerated from source, not from an observed manifest. Four producers:
                //   TvSweatScreen.cs:2659/:3239  "PRE"
                //   TvSweatScreen.cs:2049        "FT"          (also SweatFlavor.Clock on LegFinal)
                //   TvSweatScreen.cs:2373        "{minute}'"   — SweatFlavor.Minute CAPS AT 89, and
                //                                 says why: "the 90th minute belongs to the final
                //                                 sequence's stoppage time"
                //   TvSweatScreen.cs:2116        "90'+{_stoppageGoalCount}"
                // The first three are bounded. THE FOURTH IS NOT BOUNDED BY ANY FORMATTER: :2115 is
                // a bare ++ on each stoppage-time goal, reset at :2329/:2621/:2699 and capped
                // nowhere. So "longest renderable" has no answer from the format alone — it is
                // whatever the sim can score in stoppage time — and that IS the finding.
                float box = clock.rectTransform.sizeDelta.x;
                foreach (string form in new[] { "PRE", "FT", "89'" })
                {
                    float w = clock.GetPreferredValues(form, 100000f, 0f).x;
                    Debug.Log($"[T91-CLOCK] bounded form '{form}' {w:0.0}px vs box {box:0.0}px — "
                              + (w <= box ? $"fits, {box - w:0.0}px spare" : $"OVERRUNS by {w - box:0.0}px"));
                }
                for (int n = 1; n <= 12; n++)
                {
                    string form = $"90'+{n}";
                    float w = clock.GetPreferredValues(form, 100000f, 0f).x;
                    Debug.Log($"[T91-CLOCK] unbounded form '{form}' {w:0.0}px vs box {box:0.0}px — "
                              + (w <= box ? $"fits, {box - w:0.0}px spare" : $"OVERRUNS by {w - box:0.0}px"));
                }

                // ---- NUMBER 2: element rects and their authored gaps ---------------------------
                // Read off the built components in the CANVAS's local space, never from the
                // constants — the point of the number is to catch the case where the two disagree.
                // NOT `clock.canvas` — that resolves through the Graphic's canvas cache, which is
                // null on a screen built in EditMode with no CanvasUpdateRegistry pass. Found by
                // component instead.
                //
                // A COMMON space is required rather than each element's own anchoredPosition,
                // because THE TWO GROUPS SIT UNDER DIFFERENT PARENTS: the ticket column's spans are
                // children of the canvas root, while the scorebug's are children of its ZoneRoot
                // (T46). Comparing their anchored positions directly would be comparing two
                // coordinate systems and calling the difference a gap.
                Canvas canvasComp = screen.GetComponentInChildren<Canvas>(true);
                Assert.IsNotNull(canvasComp, "no Canvas under the screen — nothing to measure against");
                var canvas = canvasComp.transform as RectTransform;
                foreach (string group in new[] { "row", "scorebug" })
                {
                    string[] names = group == "row"
                        ? new[] { "LegRowLine0", "LegRowPrice0", "LegRowState0" }
                        : new[] { "Leg", "Matchup", "Clock" };
                    float prevRight = float.NaN, prevYLo = float.NaN, prevYHi = float.NaN;
                    string prevName = null;
                    foreach (string nm in names)
                    {
                        TMP_Text t = FindChild<TMP_Text>(screen, nm);
                        if (t == null) { Debug.Log($"[T91-RECT] {group}: {nm} NOT FOUND"); continue; }
                        var corners = new Vector3[4];
                        t.rectTransform.GetWorldCorners(corners);
                        float lo = float.MaxValue, hi = float.MinValue;
                        float ylo = float.MaxValue, yhi = float.MinValue;
                        for (int i = 0; i < 4; i++)
                        {
                            Vector3 lp = canvas.InverseTransformPoint(corners[i]);
                            lo = Mathf.Min(lo, lp.x); hi = Mathf.Max(hi, lp.x);
                            ylo = Mathf.Min(ylo, lp.y); yhi = Mathf.Max(yhi, lp.y);
                        }
                        Debug.Log($"[T91-RECT] {group,-9} {nm,-14} x {lo,8:0.0}..{hi,8:0.0} (w {hi - lo,6:0.0})  "
                                  + $"y {ylo,8:0.0}..{yhi,8:0.0}");
                        if (prevName != null)
                        {
                            // A NEGATIVE x-gap is only a COLLISION if the two also share a y band.
                            // Reported together for that reason: an x-overlap between elements on
                            // different rows is a layout fact, not a defect, and reporting the one
                            // without the other is how a number gets read as an alarm.
                            bool yOverlap = ylo < prevYHi && yhi > prevYLo;
                            float gap = lo - prevRight;
                            Debug.Log($"[T91-GAP]  {group,-9} {prevName} -> {nm} :: x-gap {gap:0.0}px"
                                      + (gap < 0f
                                         ? (yOverlap
                                            ? "  ** OVERLAP, and the y bands INTERSECT **"
                                            : "  (x-overlap only — the y bands are DISJOINT, so they do not collide)")
                                         : ""));
                        }
                        prevRight = hi; prevName = nm; prevYLo = ylo; prevYHi = yhi;
                    }
                }
                Debug.Log("[T91-GAP] authored constants, for comparison: BuildTicketColumn declares "
                          + "chipW 44, priceW 52, gap 6 — the compact row's spans are fixed, never "
                          + "derived from content (§6), so a measured gap that differs from 6 is the "
                          + "finding rather than the tolerance.");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>The footer's BUILT height, read off its two money rows.
        ///
        /// <para>It used to be inferred as <c>riskPays.sizeDelta.y + 8f</c>, which was correct while
        /// the footer held ONE row whose box was <c>footer.height - 8</c>. <c>T147-am</c> split it
        /// into TWO rows of <c>footer.height / 2</c>, and that formula silently began reporting
        /// <b>38.0 for a 60px footer</b> — which then propagated into a negative derived header and
        /// a "ruled" pitch lower than the built one. <b>The suite stayed green throughout:</b>
        /// nothing asserted on these numbers, because they are a report. A stale READER is the same
        /// defect class this lane keeps finding in stale COMMENTS, and it is worth naming as such.</para>
        ///
        /// <para>Read from the rows themselves — top of row 1 to bottom of row 2 — so it survives
        /// any future re-padding. Canvas y is negative downward here (<c>AnchorTopLeft</c> returns
        /// <c>-(zone.y + pad)</c>), hence the negations.</para></summary>
        private static float FooterHeight(TMP_Text riskPays, TMP_Text pays)
        {
            float top = -riskPays.rectTransform.anchoredPosition.y;
            float bottom = -pays.rectTransform.anchoredPosition.y + pays.rectTransform.sizeDelta.y;
            return bottom - top;
        }

        /// <summary>The ticket header's built height. <c>TicketHeader</c> is placed at
        /// <c>AnchorTopLeft(grid.TicketHeader, 8f, 4f)</c> with a box of <c>height - 4</c>, so box
        /// plus that 4px inset recovers it. Read rather than assumed, for the same reason as
        /// <see cref="FooterHeight"/>.</summary>
        private static float HeaderHeight(TMP_Text ticketHeader)
            => ticketHeader.rectTransform.sizeDelta.y + 4f;

        /// <summary>A throwaway text component used only to ask Unity what a string actually
        /// measures. Phase T: TMP, so it measures in the same renderer the surface now uses — a
        /// measurement taken in the OTHER renderer would be exactly the T20 mistake this test's own
        /// assertion warns about, one layer down.</summary>
        private static TMP_Text MeasureText(Transform parent, TMP_FontAsset font, int size, string content)
        {
            var go = new GameObject("Measure", typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.font = font;
            t.fontSize = size;
            t.fontStyle = FontStyles.Bold;
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

                TMP_Text score = FindChild<TMP_Text>(screen, "Matchup"); // the persistent score line
                Assert.IsNotNull(score, "Matchup (the score) not found");
                Assert.AreEqual(CanonScore, score.fontSize, "the score must render at the canon --tv-size-score");

                foreach (string name in new[]
                    { "Clock", "CashOut", "RiskPays", "Flavor", "TicketHeader", "Leg",
                      "LegRowNeed0", "LegRowProgress0", "LegRowLine0" })
                {
                    TMP_Text t = FindChild<TMP_Text>(screen, name);
                    Assert.IsNotNull(t, $"{name} not found — canvas layout changed?");
                    Assert.LessOrEqual(t.fontSize, score.fontSize,
                        $"{name} renders at {t.fontSize}px against the score's {score.fontSize}px. " +
                        "DESIGN.md §5's ratio table makes the score the thing nothing may outgrow.");
                }

                // Spot-check the rungs that carry a named ruling rather than every pair.
                Assert.AreEqual(CanonCashOut, FindChild<TMP_Text>(screen, "CashOut").fontSize,
                    "cash-out sits at .70 of the score and must never reach it (DESIGN.md §5).");
                Assert.AreEqual(CanonClock, FindChild<TMP_Text>(screen, "Clock").fontSize);
                Assert.AreEqual(CanonRisk, FindChild<TMP_Text>(screen, "RiskPays").fontSize,
                    "C8 put risk/pays in the protected set; it sits at the canon --tv-size-risk.");
                Assert.AreEqual(CanonEvent, FindChild<TMP_Text>(screen, "Flavor").fontSize,
                    "the event strip is one line at the canon --tv-size-event.");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        // ---------------------------------------------------------------------------------------
        // T46 — the ticket column owns its width absolutely.
        //
        // DD 2026-08-02: "Stage overdraws ticket column ... scoreline/pitch painted over leg text
        // (struck-through identities, BIFF RACKET TO SCORE cut mid-word). Ticket column owns its
        // width absolutely; stage clips to its region; assert per-frame edges."
        //
        // WHAT THIS INSTRUMENT CANNOT DO, stated up front (C25): it cannot assert containment by
        // reading corners. RectMask2D clips at RENDER time and does not move a Graphic's rect, so
        // GetWorldCorners reports the same overflowing box masked or not. A corner-based
        // "containment" test here would pass identically before and after the fix — a fifth vacuous
        // green gate (C18 §4.2). So it asserts the STRUCTURE that makes containment true, and pairs
        // it with a canary proving the structure is load-bearing rather than decorative.
        // ---------------------------------------------------------------------------------------

        [Test]
        public void T46_right_hand_zone_content_is_owned_and_clipped_by_its_own_zone()
        {
            var go = new GameObject("ZoneOwnership");
            try
            {
                go.SetActive(false);
                var screen = go.AddComponent<TvSweatScreen>();
                screen.theaterEnabled = true; // the stage is one of the three zones under test
                screen.referencePixelsWide = 980;
                InvokePrivate(screen, "Awake");

                var column = FindChild<Image>(screen, "TicketColumnZone");
                Assert.IsNotNull(column, "TicketColumnZone missing");
                float boundary = column.rectTransform.sizeDelta.x; // the column's right edge

                // Every element the ruling names, with the zone that must own AND clip it.
                var owned = new (string element, string zone)[]
                {
                    ("Matchup", "ScoreBugZone"), ("Score", "ScoreBugZone"),
                    ("Leg", "ScoreBugZone"), ("Clock", "ScoreBugZone"),
                    ("Flavor", "EventStripZone"),
                };

                foreach ((string element, string zone) in owned)
                {
                    var g = FindChild<Graphic>(screen, element);
                    Assert.IsNotNull(g, $"{element} not found");
                    // Walked by hand, not via GetComponentInParent: the whole hierarchy is inactive
                    // in this harness (go.SetActive(false) keeps Awake from firing on its own), and
                    // the parameterless overload skips inactive objects — it would return null here
                    // and the assertion would fail for a reason that has nothing to do with T46.
                    RectMask2D mask = NearestMaskAbove(g.transform);
                    Assert.IsNotNull(mask,
                        $"{element} has no clipping ancestor — it can paint into the ticket column");
                    Assert.AreEqual(zone, mask.name,
                        $"{element}'s nearest clip rect is '{mask.name}', not its own zone '{zone}'. "
                        + "The canvas-level glass mask (T25.1) does NOT satisfy T46: its bound is the "
                        + "screen edge, and this overdraw never leaves the screen — it leaves its zone.");
                    Assert.IsTrue(g.transform.IsChildOf(mask.transform),
                        $"{element} is not inside {zone}; a mask only clips its descendants");
                }

                // The stage: the ruling's literal instruction, and the one element with a child that
                // is measurably outside its own rect (NetRipple, up to ~155px past the edge at full
                // scale — on a left-side flash that lands inside the ticket column).
                var stage = screen.GetComponentInChildren<TheaterStage>(true);
                Assert.IsNotNull(stage, "no TheaterStage was built with theaterEnabled = true");
                Assert.IsNotNull(stage.GetComponent<RectMask2D>(),
                    "T46: the stage does not clip to its region. Its own rect is correct, but nothing "
                    + "keeps its children inside it.");

                // Each masked zone must itself begin at or after the column's right edge — a clip
                // rect that straddled the boundary would clip to a region that still overlaps.
                foreach (string zone in new[] { "ScoreBugZone", "EventStripZone" })
                {
                    var z = FindChild<Image>(screen, zone);
                    Assert.GreaterOrEqual(z.rectTransform.anchoredPosition.x, boundary - 0.5f,
                        $"{zone} starts left of the ticket column's right edge ({boundary}px)");
                }

                // CANARY — proof this is not vacuous. An authored fixture at the score's own size
                // overflows its 675px box far enough that its raw rect crosses the boundary. The
                // rect crossing is EXPECTED and is exactly why the mask above must exist; if this
                // ever stops crossing, the structural assertions have stopped being load-bearing and
                // this test is passing for the wrong reason.
                var matchup = FindChild<TMP_Text>(screen, "Matchup");
                if (matchup.font == null)
                {
                    // Scope, stated (C25): with no face loaded there is nothing to measure, so the
                    // canary cannot run. The structural assertions above did run and are unaffected.
                    Debug.Log("[T46] no font on Matchup — canary skipped; structural checks ran.");
                }
                else
                {
                    matchup.text = "TIDEWATER LONGHAULERS 2 — 1 SALTMEN JUNCTION ATHLETIC";
                    float box = matchup.rectTransform.sizeDelta.x;
                    float overflow = matchup.preferredWidth - box;
                    Assert.Greater(overflow, 0f,
                        "the canary fixture no longer overflows its box — T46's mechanism is not "
                        + "reproduced here, so the structural assertions above are no longer known to "
                        + $"be load-bearing. (box {box}px, text {matchup.preferredWidth}px)");
                    // UpperCenter + Overflow spills symmetrically, so half the overflow reaches left
                    // of the rect. Rect left edge = zone origin + anchored x − pivot share of width.
                    RectTransform mrt = matchup.rectTransform;
                    float zoneX = FindChild<Image>(screen, "ScoreBugZone").rectTransform.anchoredPosition.x;
                    float rectLeft = zoneX + mrt.anchoredPosition.x - mrt.sizeDelta.x * mrt.pivot.x;
                    float inkLeft = rectLeft - overflow * 0.5f;
                    Assert.Less(inkLeft, boundary,
                        "the canary no longer reaches into the ticket column, so this test would pass "
                        + "with every mask removed. T46's mechanism must stay reproduced here for the "
                        + $"structural checks to mean anything. (ink left {inkLeft}px, boundary {boundary}px)");
                }
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
