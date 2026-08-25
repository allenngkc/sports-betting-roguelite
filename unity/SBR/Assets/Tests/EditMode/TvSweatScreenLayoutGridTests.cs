using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SBR.Engine;
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
                need.text = FxNeedWidest;
                progress.text = FxProgressWidest;
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
                Vector2 riskV = riskPays.GetPreferredValues(FxRiskWorst, Unconstrained, 0f);
                Vector2 stakeV = riskPays.GetPreferredValues(FxStakeWorst, Unconstrained, 0f);
                Vector2 paysV = riskPays.GetPreferredValues(FxPaysWorst, Unconstrained, 0f);
                Vector2 returnedV = riskPays.GetPreferredValues(FxReturnedWorst, Unconstrained, 0f);
                Vector2 paidV = riskPays.GetPreferredValues(FxPaidWorst, Unconstrained, 0f);

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
                need.text = FxNeedWidest;
                progress.text = FxProgressWidest;
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
                Vector2 compactV = compact.GetPreferredValues(FxCompactProbe, 100000f, 0f);
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
                Vector2 needV = need.GetPreferredValues(FxNeedWidest, 100000f, 0f);
                Vector2 progV = progress.GetPreferredValues(FxProgressWidest, 100000f, 0f);

                Debug.Log($"[T147] column {columnH:0.0} = header {headerH:0.0} + {slotsNow} x pitch "
                          + $"{pitchNow:0.0} + footer {footerH:0.0}");
                // HEIGHT PROBE ONLY. A TMP line box is a function of face and size, never of the
                // string, so any string gives the compact line's true height — but this one is T90-am's
                // NEED worst case, NOT the compact line's, and its width here is therefore not an
                // extent verdict. The compact statement's own extent belongs to the T84 sweep.
                Debug.Log($"[T147] compact  line box {compactV.y:0.0}h at the compact size (height probe; "
                          + $"width {compactV.x:0.0} is NOT this slot's worst case — see the T84 sweep)");
                Debug.Log($"[T147] NEED     '{FxNeedWidest}' {needV.x:0.0}w x {needV.y:0.0}h "
                          + $"against box {need.rectTransform.sizeDelta.x:0.0}w (T90's band)");
                Debug.Log($"[T147] progress '{FxProgressWidest}' {progV.x:0.0}w x {progV.y:0.0}h "
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
                Vector2 riskV = riskPays.GetPreferredValues(FxRiskWorst, 100000f, 0f);
                Vector2 paysV = pays.GetPreferredValues(FxPaysWorst, 100000f, 0f);

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
                Vector2 stakeV = riskPays.GetPreferredValues(FxStakeWorst, 100000f, 0f);
                Vector2 returnedV = pays.GetPreferredValues(FxReturnedWorst, 100000f, 0f);
                Vector2 paidV = pays.GetPreferredValues(FxPaidWorst, 100000f, 0f);

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
                    (FxMargin3Primary, FxMargin3Fallback),
                    (FxMargin2Primary, FxMargin2Fallback),
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

        /// <summary>§4 of `spec-terse-copy-2026-08-20.md`: every rung the DD authored for the four
        /// blocked kinds, measured over the closed pools, in the box it renders in, through the
        /// SURFACE'S OWN selector.
        ///
        /// <para>Three questions, and the third is the one that matters most. (1) How many of the 20
        /// clubs / 12 surnames each rung clears. (2) Whether the rung's truncation is REACHABLE at
        /// all — a rung that fits for every member never truncates, and §4 says aiming for that is
        /// the point. (3) `T156`'s collision test: the rung AND every truncation of it, checked
        /// against what OTHER markets on this surface can produce. `{CLUB} TO WIN OR DRAW`
        /// truncating to `{CLUB} WIN` is why `DoubleChance` was re-authored — a row stating a
        /// requirement the player does not have — so a collision found here is a finding, never
        /// something to author around.</para>
        ///
        /// <para>`FitToColumn` and `FitOrFallback` are reached by REFLECTION. A replica that drifted
        /// would answer a question nobody asked.</para></summary>
        [Test]
        public void TerseCopy_the_rungs_measured_over_the_closed_pools()
        {
            var go = new GameObject("TerseCopyRungs");
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
                Assert.IsNotNull(fitToColumn, "TvSweatScreen.FitToColumn not found by reflection — renamed?");

                // The pool, read the way T158 reads it — so the collision check compares against what
                // the surface can ACTUALLY produce rather than against a list kept by hand here.
                System.Type sweep = System.Type.GetType("SBR.EditorTools.TvExtentSweep, SBR.Game.Editor");
                Assert.IsNotNull(sweep, "could not load TvExtentSweep — the collision check would be blind");
                FieldInfo casesField = sweep.GetField("Cases", BindingFlags.NonPublic | BindingFlags.Static);
                Assert.IsNotNull(casesField, "TvExtentSweep.Cases not found by reflection");
                var pool = new Dictionary<string, HashSet<string>>();
                foreach (object row in (System.Array)casesField.GetValue(null))
                {
                    System.Type rt = row.GetType();
                    var sl = (string)rt.GetField("Item1").GetValue(row);
                    var st = (string[])rt.GetField("Item3").GetValue(row);
                    if (sl == null || st == null) continue;
                    if (!pool.TryGetValue(sl, out HashSet<string> set)) pool[sl] = set = new HashSet<string>();
                    foreach (string x in st) set.Add(x);
                }

                string[] clubs =
                {
                    "YAMS", "STARTUPS", "BRICKLAYERS", "LONGHAULERS", "MALLARDS", "SPREADSHEETS",
                    "TURNIPS", "MIDDLEMEN", "REGULATORS", "PLUMBERS", "MEATBALLS", "AUDITORS",
                    "FERRETS", "OVERHEADS", "GRAVEDIGGERS", "NOTARIES", "MUSKRATS", "ZAMBONIS",
                    "LOOPHOLES", "REFUNDS",
                };
                string[] surnames =
                {
                    "LEDGER", "CINDER", "MUFFIN", "PAVEMENT", "COUPON", "WOBBLE",
                    "GASKET", "PYLON", "KETCHUP", "LANYARD", "RACKET", "STAPLER",
                };

                // kind · slot · the rung's format · the pool it generates over · the kind's OWN forms
                // (a truncation landing on one of these is the LADDER WORKING, not a collision)
                var rungs = new List<(string Kind, TMP_Text Slot, string SlotName, string Fmt, string[] Pool, string[] Own)>
                {
                    ("DoubleChance NEED r1", need, "LegRowNeed0", "{0} UNBEATEN AT FULL TIME", clubs,
                        new[] { "{0} UNBEATEN AT FULL TIME", "{0} UNBEATEN" }),
                    ("DoubleChance NEED r2", need, "LegRowNeed0", "{0} UNBEATEN", clubs,
                        new[] { "{0} UNBEATEN AT FULL TIME", "{0} UNBEATEN" }),
                    ("DoubleChance compact", line, "LegRowLine0", "{0} UNBEATEN", clubs,
                        new[] { "{0} UNBEATEN" }),
                    ("Handicap NEED r3 (+)", need, "LegRowNeed0", "{0} +1.5", clubs,
                        new[] { "{0} +1.5", "{0} WITHIN 1", "{0} WITHIN 1 GOAL" }),
                    ("Handicap NEED r3 (-)", need, "LegRowNeed0", "{0} -1.5", clubs,
                        new[] { "{0} -1.5", "{0} BY 2+", "{0} TO WIN BY 2+" }),
                    ("PlayerMultiScorer NEED r2", need, "LegRowNeed0", "{0} 2+", surnames,
                        new[] { "{0} 2+", "{0} TO SCORE 2+" }),
                    ("Handicap compact (+, shipped)", line, "LegRowLine0", "{0} +1.5", clubs,
                        new[] { "{0} +1.5" }),
                    ("Handicap compact (-, shipped)", line, "LegRowLine0", "{0} -1.5", clubs,
                        new[] { "{0} -1.5" }),
                    ("PlayerMultiScorer compact (shipped)", line, "LegRowLine0", "{0} 2+", surnames,
                        new[] { "{0} 2+" }),
                };

                foreach (var r in rungs)
                {
                    float box = r.Slot.rectTransform.sizeDelta.x;
                    int clears = 0;
                    var misses = new List<string>();
                    var collisions = new List<string>();
                    float widest = 0f; string widestS = "";
                    var ownConcrete = new HashSet<string>();
                    foreach (string m in r.Pool)
                        foreach (string f in r.Own) ownConcrete.Add(string.Format(f, m));

                    foreach (string m in r.Pool)
                    {
                        string str = string.Format(r.Fmt, m);
                        float w = r.Slot.GetPreferredValues(str, 100000f, 0f).x;
                        if (w > widest) { widest = w; widestS = str; }
                        if (w <= box) { clears++; continue; }
                        misses.Add($"{m} ({w:0.0})");
                        var cut = (string)fitToColumn.Invoke(null, new object[] { r.Slot, str });
                        // T156: does the truncation land on a string ANOTHER market can produce?
                        if (pool.TryGetValue(r.SlotName, out HashSet<string> set)
                            && set.Contains(cut) && !ownConcrete.Contains(cut))
                            collisions.Add($"'{str}' -> '{cut}' COLLIDES with another market's string");
                        else
                            collisions.Add($"'{str}' -> '{cut}'");
                    }

                    Debug.Log($"[TERSE] {r.Kind,-36} box {box:0.0}  widest '{widestS}' {widest:0.0}  "
                              + $"clears {clears}/{r.Pool.Length}"
                              + (clears == r.Pool.Length
                                 ? "  — TRUNCATION UNREACHABLE, the outcome §4 aims for"
                                 : $"  — misses: {string.Join(", ", misses)}"));
                    foreach (string c in collisions) Debug.Log($"[TERSE-CUT] {r.Kind,-36} {c}");
                }

                // THE LADDER AS THE SURFACE ACTUALLY RUNS IT. Every rung above was truncated in
                // ISOLATION, which is the right way to price a rung and the WRONG way to read what
                // renders: FitOrFallback tries rung 1, then rung 2, and only truncates the one it
                // ends on. Reporting rung 1's isolated truncation as a rendering would invent a
                // string the ladder prevents — `YAMS UNBEATEN AT` is exactly that shape.
                MethodInfo fitOrFallback = typeof(TvSweatScreen).GetMethod("FitOrFallback",
                    BindingFlags.NonPublic | BindingFlags.Static);
                Assert.IsNotNull(fitOrFallback, "TvSweatScreen.FitOrFallback not found by reflection");
                int r1 = 0, r2 = 0, fellToFloor = 0;
                var lost = new List<string>();
                foreach (string c in clubs)
                {
                    string p1 = $"{c} UNBEATEN AT FULL TIME", p2 = $"{c} UNBEATEN";
                    var rendered = (string)fitOrFallback.Invoke(null, new object[] { need, p1, p2 });
                    if (rendered == p1) r1++;
                    else if (rendered == p2) r2++;
                    else { fellToFloor++; if (!rendered.Contains("UNBEATEN")) lost.Add($"{c} -> '{rendered}'"); }
                }
                Debug.Log($"[TERSE-LADDER] DoubleChance NEED, as the surface runs it :: rung 1 {r1}/20 · "
                          + $"rung 2 {r2}/20 · truncated {fellToFloor}/20; of those, {lost.Count} lost the word "
                          + "UNBEATEN entirely and render as the club alone");
                foreach (string x in lost) Debug.Log($"[TERSE-LADDER]   {x}");

                // The COMPACT slot has NO ladder (T155's build order is unbuilt), so its form
                // truncates directly — this is what renders today, not a hypothetical.
                int cLost = 0;
                foreach (string c in clubs)
                {
                    var rendered = (string)fitToColumn.Invoke(null, new object[] { line, $"{c} UNBEATEN" });
                    if (!rendered.Contains("UNBEATEN")) cLost++;
                }
                Debug.Log($"[TERSE-LADDER] DoubleChance COMPACT, no ladder to fall to :: {cLost}/20 render "
                          + "as the club alone, with no word naming the market");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>THE SHAPE `T165` WAS RULED FOR, WHICH NO TEST HAD EVER RENDERED.
        ///
        /// <para>`T165`/`T165-am` moved the counter's referent from the LEG to the FIXTURE, and its
        /// word from `LEG n/m` to `MATCH n/m`. The reason is `T140` arm A: a telling is a
        /// (ticket, FIXTURE) and two legs can ride ONE match, so a counter reading `LEG 2/3` beside a
        /// column showing THREE rows on TWO tellings prints a leg total the ticket column
        /// contradicts.</para>
        ///
        /// <para><b>Every fixture this suite builds is an ORDINARY ticket</b> — one leg per match, so
        /// <c>FixtureCount == Legs.Count</c> and <c>evt.FixtureIndex</c> never diverges from
        /// <c>evt.LegIndex</c>. On that shape `MATCH n/m` renders the exact digits `LEG n/m` did, and
        /// the ruling's whole subject goes unobserved: the referent could regress to the leg and every
        /// pin in this file would stay green. This builds the interleaved ticket instead.</para>
        ///
        /// <para><b>⚠ THE <c>_session</c> REFLECTION IS LOAD-BEARING, NOT SETUP.</b>
        /// <c>FixtureTotal()</c> falls back to <c>_ticket.Legs.Count</c> when <c>_session</c> is null,
        /// and the shared <see cref="RenderPregameFor"/> sets ONLY <c>_ticket</c> — so a test written
        /// on that helper would render `MATCH 1/3` off the FALLBACK, pass, and prove nothing whatever
        /// about the fixture referent. That is the vacuous-gate failure this lane has been bitten by
        /// repeatedly, which is why <see cref="RenderPregameWithSessionFor"/> exists rather than an
        /// extra argument on the shared helper.</para>
        ///
        /// <para><b>Assertion 5 is what makes this a gate rather than a restatement.</b> Asserting the
        /// counter equals `MATCH 1/{FixtureCount}` passes under BOTH referents on an ordinary ticket,
        /// so on its own it only re-types the format string. Asserting it is NOT
        /// `MATCH 1/{Legs.Count}` is the half that FAILS if the referent regresses to the leg — and it
        /// can only fail here, on a ticket where those two totals are different numbers.</para></summary>
        [Test]
        public void T165_the_counter_counts_TELLINGS_on_a_same_match_ticket()
        {
            (Ticket ticket, SweatSession session) = FindSameMatchCounterTicket(
                "T165-SAMEMATCH-A", "T165-SAMEMATCH-B", "T165-SAMEMATCH-C", "T165-SAMEMATCH-D");
            Assert.IsNotNull(ticket,
                "no legal interleaved [A, B, A] ticket collapsed to fewer tellings than legs on these "
                + "seeds. The shape T165 was ruled for is then unreachable from this pool and this "
                + "gate would be vacuous — widen the seeds rather than relax the assertions");

            var go = new GameObject("T165SameMatchCounter");
            try
            {
                TvSweatScreen s = BuildScreen(go);

                // 1. ANTI-VACUITY, BEFORE ANYTHING IS RENDERED. If the ticket does not carry FEWER
                //    tellings than legs, the two referents COINCIDE on it and no assertion below can
                //    tell them apart — the test would be measuring a distinction it cannot see.
                Assert.Less(session.FixtureCount, ticket.Legs.Count,
                    $"FixtureCount {session.FixtureCount} is not below Legs.Count {ticket.Legs.Count} — "
                    + "this ticket has one telling per leg, so the FIXTURE referent and the retired LEG "
                    + "referent print the same digits and this gate cannot distinguish them");

                // 2. And that it is interleaved in the shape claimed: legs 0 and 2 on ONE matchup,
                //    leg 1 on another. Reference identity, not index equality — the fixture grouping
                //    the session builds is over the Matchup objects themselves.
                Assert.AreSame(ticket.Legs[0].Matchup, ticket.Legs[2].Matchup,
                    "legs 0 and 2 are not on the same matchup — the ticket is not the [A, B, A] shape "
                    + "this gate claims to have found");
                Assert.AreNotSame(ticket.Legs[0].Matchup, ticket.Legs[1].Matchup,
                    "leg 1 shares matchup A — the ticket collapsed to ONE telling, not the two-telling "
                    + "interleave the counter has to count");

                // 3. The counter written by the REAL RenderPregame, with the REAL locked session
                //    behind it (see the helper: this is the load-bearing part, not plumbing).
                RenderPregameWithSessionFor(s, ticket, session);
                TMP_Text leg = FindChild<TMP_Text>(s, "Leg");
                Assert.IsNotNull(leg, "Leg not found");
                string rendered = leg.text;

                // Logged HERE, ahead of the verdicts, rather than after them: a Debug.Log that sits
                // below a failing assert never runs, and the point of this line is that the evidence
                // survives a FAILURE, not that it decorates a pass. TV-FINALFIX's own precedent.
                Debug.Log($"[T165-SAMEMATCH] legs {ticket.Legs.Count} · fixtures {session.FixtureCount} "
                          + $"· counter rendered '{rendered}'");

                // 4. THE BINARY: the counter's denominator is the TELLING count.
                Assert.AreEqual($"MATCH 1/{session.FixtureCount}", rendered,
                    $"the counter rendered '{rendered}' on a ticket of {ticket.Legs.Count} legs across "
                    + $"{session.FixtureCount} tellings. T165's referent is the FIXTURE: the session is "
                    + "the authority on what a match is, because FixtureCount is the same grouping the "
                    + "joint price uses");

                // 5. THE DISCRIMINATOR. Assertion 4 alone would pass under EITHER referent on an
                //    ordinary ticket, where the two totals are the same number; this is the one that
                //    fails if the referent regresses from the fixture back to the leg, because on
                //    THIS ticket `MATCH 1/3` is what the retired LEG referent would have printed.
                Assert.AreNotEqual($"MATCH 1/{ticket.Legs.Count}", rendered,
                    $"the counter printed the LEG total ({ticket.Legs.Count}) on a ticket with only "
                    + $"{session.FixtureCount} tellings — the referent has regressed to the leg, which "
                    + "is precisely the `LEG 2/3 beside three rows` defect T165 was ruled to close. "
                    + "This assertion, not the equality above, is what catches it");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>THE POOL↔CODE EDGE, which `T158` STRUCTURALLY CANNOT SEE — and the sixth phantom
        /// it was pre-positioned to become.
        ///
        /// <para>The counter's form lives in THREE places: the code's format string
        /// (<c>TvSweatScreen.RenderPregame</c> / <c>RenderEvent</c>), the `T84` pool
        /// (<c>TvExtentSweep.Cases</c>), and this file's measured-fixture table. **`T158` asserts the
        /// third against the second.** Update those two, miss the code, and it stays GREEN while
        /// measuring a string the surface can never render — which is exactly how the fifth phantom
        /// (<c>TICKET n OF m</c>, the header's format, pooled but never emitted) survived.</para>
        ///
        /// <para>So this drives the real render and asserts what the code ACTUALLY EMITS is in the
        /// pool for its slot. It closes the one edge of the triangle nothing else checks.</para>
        ///
        /// <para>Like `T158`, a lookup that fails must FAIL rather than skip: a guard that passes
        /// while blind is the defect class this exists to close.</para></summary>
        [Test]
        public void T165_the_counter_the_code_emits_is_in_the_pool()
        {
            var go = new GameObject("T165PoolVsCode");
            try
            {
                TvSweatScreen s = BuildScreen(go);
                Ticket ticket = CounterTicket("T165-POOL-VS-CODE");
                RenderPregameFor(s, ticket);

                TMP_Text leg = FindChild<TMP_Text>(s, "Leg");
                Assert.IsNotNull(leg, "Leg not found");
                string emitted = leg.text;
                Assert.IsNotEmpty(emitted,
                    "the counter rendered EMPTY on a live ticket — nothing to check, and a pin that "
                    + "checks nothing must fail rather than pass");

                System.Type sweep = System.Type.GetType("SBR.EditorTools.TvExtentSweep, SBR.Game.Editor");
                Assert.IsNotNull(sweep,
                    "could not load TvExtentSweep — the pin cannot see the pool and must fail rather "
                    + "than pass");
                FieldInfo casesField = sweep.GetField("Cases", BindingFlags.NonPublic | BindingFlags.Static);
                Assert.IsNotNull(casesField, "TvExtentSweep.Cases not found by reflection — renamed?");
                var cases = (System.Array)casesField.GetValue(null);
                Assert.IsNotNull(cases, "TvExtentSweep.Cases read as null");

                var pooled = new HashSet<string>();
                foreach (object row in cases)
                {
                    System.Type rt = row.GetType();
                    var slot = (string)rt.GetField("Item1").GetValue(row);
                    var strings = (string[])rt.GetField("Item3").GetValue(row);
                    if (slot != "Leg" || strings == null) continue;
                    foreach (string str in strings) pooled.Add(str);
                }
                Assert.Greater(pooled.Count, 0, "the 'Leg' slot has NO POOL AT ALL — nothing to check");

                Debug.Log($"[T165-POOL] code emitted '{emitted}'; pool holds "
                          + string.Join(", ", pooled.OrderBy(x => x).Select(x => $"'{x}'")));

                Assert.IsTrue(pooled.Contains(emitted),
                    $"the counter emits '{emitted}', which is NOT in the T84 pool for 'Leg'. Either "
                    + "the code's format changed without the pool, or the pool changed without the "
                    + "code — and T158 cannot see either, because it only compares the measured "
                    + "fixture against the pool. Pooled: "
                    + string.Join(", ", pooled.OrderBy(x => x).Select(x => $"'{x}'")));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>Drives the real <c>RenderPregame</c> so the counter is produced by the CODE, not
        /// by a fixture string. Reflected because the seam is private — and reflected seams are
        /// invisible to the compiler, so a rename here throws at run time rather than failing the
        /// build (this lane broke two suites that way in one day).</summary>
        /// <summary>A locked two-leg ticket. Local to this file rather than shared with the
        /// palette suite's own builder: a test fixture reached across class boundaries is a
        /// dependency between suites, and these two want to move independently.</summary>
        private static Ticket CounterTicket(string runId)
        {
            var run = new Run(runId, new RunConfig());
            Ticket t = run.PlaceTicket(new[]
            {
                new Pick(0, MarketSelection.Moneyline(Side.Home)),
                new Pick(1, MarketSelection.Moneyline(Side.Home)),
            }, 10);
            run.LockRound();
            return t;
        }

        private static void RenderPregameFor(TvSweatScreen s, Ticket ticket)
        {
            typeof(TvSweatScreen).GetField("_ticket", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(s, ticket);
            MethodInfo render = typeof(TvSweatScreen).GetMethod("RenderPregame",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(render, "TvSweatScreen.RenderPregame not found by reflection — renamed?");
            render.Invoke(s, System.Array.Empty<object>());
        }

        /// <summary>The interleaved <c>[matchA, matchB, matchA]</c> ticket the `T165` counter gate
        /// needs — THREE legs, TWO tellings — together with its locked session, searched off the
        /// board.
        ///
        /// <para>The pair is the NESTED GOAL PAIR: over the higher line ENTAILS over the lower, which
        /// is pure set containment, so no board or pricing change can refuse it on correlation
        /// grounds. The seed/matchup search is still needed, because the board decides which markets
        /// it OFFERS at all.</para>
        ///
        /// <para>Deliberately does NOT drive the sweat, unlike <c>TvSweatFinalFixtureGateTests.Find</c>:
        /// the counter under test is written by <c>RenderPregame</c>, before any beat, so surviving a
        /// telling is not a precondition here. Returns a null tuple when no seed offers a legal one —
        /// the caller must FAIL on that rather than skip, since a gate that quietly finds no subject
        /// is a gate that quietly checks nothing.</para></summary>
        private static (Ticket Ticket, SweatSession Session) FindSameMatchCounterTicket(params string[] seeds)
        {
            foreach (string seed in seeds)
            {
                int matchups = new Run(seed, new RunConfig()).CurrentSlate.Matchups.Count;
                for (int a = 0; a < matchups; a++)
                    for (int b = 0; b < matchups; b++)
                    {
                        if (a == b) continue;
                        var run = new Run(seed, new RunConfig());
                        RunConfig cfg = run.Config;
                        var picks = new[]
                        {
                            new Pick(a, MarketSelection.TotalGoals(cfg.GoalLines[1], true)),
                            new Pick(b, MarketSelection.Moneyline(Side.Home)),
                            new Pick(a, MarketSelection.TotalGoals(cfg.GoalLines[0], true)),
                        };
                        // RefusalFor FIRST. PlaceTicket THROWS on a refused set, and a search that
                        // throws its way across the board is a search that stops at the first
                        // matchup pair the engine happens to dislike.
                        if (run.RefusalFor(picks) != null) continue;

                        Ticket t = run.PlaceTicket(picks, 10);
                        run.LockRound();   // the session does not exist until the round is locked
                        if (t.Legs.Count != 3) continue;
                        if (!ReferenceEquals(t.Legs[0].Matchup, t.Legs[2].Matchup)) continue;
                        if (ReferenceEquals(t.Legs[0].Matchup, t.Legs[1].Matchup)) continue;

                        SweatSession session = run.Sweats[0];
                        // The SHAPE, not the intent. A ticket whose fixtures did not actually
                        // collapse is an ordinary ticket wearing a same-match ticket's picks, and it
                        // renders identical digits under either referent — exactly the candidate
                        // that would hand this gate a green with nothing behind it.
                        if (session.FixtureCount >= t.Legs.Count) continue;
                        return (t, session);
                    }
            }

            return (null, null);
        }

        /// <summary>`T165`'s render seam: <c>_ticket</c> AND <c>_session</c>, then the real
        /// <c>RenderPregame</c>.
        ///
        /// <para>A SECOND helper rather than a widened <see cref="RenderPregameFor"/>, on purpose.
        /// Other tests in this file depend on that one's ticket-only shape — and the difference
        /// between the two is precisely the thing under test: with <c>_session</c> null,
        /// <c>FixtureTotal()</c> falls back to <c>_ticket.Legs.Count</c>, so the counter answers with
        /// the LEG total while looking exactly right.</para></summary>
        private static void RenderPregameWithSessionFor(TvSweatScreen s, Ticket ticket, SweatSession session)
        {
            FieldInfo ticketField = typeof(TvSweatScreen).GetField("_ticket",
                BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo sessionField = typeof(TvSweatScreen).GetField("_session",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(ticketField, "TvSweatScreen._ticket not found by reflection — renamed?");
            Assert.IsNotNull(sessionField,
                "TvSweatScreen._session not found by reflection — renamed? A silent miss here would "
                + "leave FixtureTotal() on its leg-count fallback and render the gate vacuous, which "
                + "is the one outcome it exists to prevent — so it fails loudly instead");
            ticketField.SetValue(s, ticket);
            sessionField.SetValue(s, session);

            MethodInfo render = typeof(TvSweatScreen).GetMethod("RenderPregame",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(render, "TvSweatScreen.RenderPregame not found by reflection — renamed?");
            render.Invoke(s, System.Array.Empty<object>());
        }

        /// <summary>`T165` STEP 3's MEASUREMENT, REPORT-ONLY — what the counter may say once its
        /// referent is the FIXTURE rather than the leg.
        ///
        /// <para>`LEG n/m` counting fixtures is FALSE on a same-match ticket: four legs, three
        /// tellings, a counter reading `2/3` beside a column showing four rows. So the word has to
        /// move, and `T165` leaves the form to TV with no width asserted — *"only measurement
        /// decides."* This supplies the numbers that decision needs and rules nothing.</para>
        ///
        /// <para>DELIBERATELY NOT PART OF THE `T158` FIXTURE TABLE. That pin asserts every MEASURED
        /// fixture is in the `T84` pool for its slot, and these candidates are by definition NOT in
        /// the pool — the surface cannot render them yet. Adding them there to make this measurable
        /// would manufacture exactly the phantom the pin exists to catch. A candidate is priced
        /// here; only the form that WINS earns a pool entry, in the same diff that teaches the code
        /// to emit it.</para>
        ///
        /// <para>The ink math is copied from `T91`'s own block below rather than re-derived, so the
        /// numbers are commensurable with the ones `T91-cl` ruled on.</para></summary>
        [Test]
        public void T165_price_the_fixture_counter_candidates_against_the_ticket_header()
        {
            var go = new GameObject("T165CounterForms");
            try
            {
                var screen = BuildScreen(go);
                TMP_Text leg = FindChild<TMP_Text>(screen, "Leg");
                TMP_Text header = FindChild<TMP_Text>(screen, "TicketHeader");
                Assert.IsNotNull(leg, "Leg not found — T91-cl moved it into BuildTicketColumn");
                Assert.IsNotNull(header, "TicketHeader not found — Leg's neighbour since T91-cl");
                Assert.IsNotNull(leg.font, "no font resolved — a measurement in the fallback face is void");
                Assert.IsTrue(leg.font.name.Contains("Encode"),
                    $"measured in '{leg.font.name}', not Encode Sans — the same mistake T20 made once");

                Canvas canvasComp = screen.GetComponentInChildren<Canvas>(true);
                Assert.IsNotNull(canvasComp, "no Canvas under the screen — nothing to measure against");
                var canvas = canvasComp.transform as RectTransform;

                (float lo, float hi, float yLo, float yHi) Ink(TMP_Text t, string form)
                {
                    var cs = new Vector3[4];
                    t.rectTransform.GetWorldCorners(cs);
                    float bl = float.MaxValue, br = float.MinValue, y0 = float.MaxValue, y1 = float.MinValue;
                    for (int i = 0; i < 4; i++)
                    {
                        Vector3 lp = canvas.InverseTransformPoint(cs[i]);
                        bl = Mathf.Min(bl, lp.x); br = Mathf.Max(br, lp.x);
                        y0 = Mathf.Min(y0, lp.y); y1 = Mathf.Max(y1, lp.y);
                    }
                    float w = t.GetPreferredValues(form, 100000f, 0f).x;
                    bool centred = t.alignment == TextAlignmentOptions.Top
                                || t.alignment == TextAlignmentOptions.Center
                                || t.alignment == TextAlignmentOptions.Bottom;
                    bool right = t.alignment == TextAlignmentOptions.TopRight
                              || t.alignment == TextAlignmentOptions.Right
                              || t.alignment == TextAlignmentOptions.BottomRight;
                    float lo = centred ? (bl + br) * 0.5f - w * 0.5f : right ? br - w : bl;
                    return (lo, lo + w, y0, y1);
                }

                const float InkFloor = 2f; // T90-am's floor, generalised by T91-cl to any y-sharing pair
                (float hLo, float hHi, float hY0, float hY1) = Ink(header, "TICKET 2/2");
                Debug.Log($"[T165-INK] neighbour TicketHeader align={header.alignment} widest "
                          + $"'TICKET 2/2' INK x {hLo:0.0}..{hHi:0.0}  y {hY0:0.0}..{hY1:0.0}");

                // MaxLegs is 4 and FixtureCount <= Legs.Count, so n/m never exceeds 4/4; the digits
                // are tabular (T82's atlas working), so 4/4 measures equal to 1/1 and is the widest
                // form of every candidate. `LEG 4/4` is the incumbent, priced for comparison.
                // `LEG 4/4` is GONE from this set, not merely deprioritised: T165-am retired it and
                // the pool no longer holds it, so measuring it here would be measuring a string the
                // surface can no longer emit — a phantom, and the exact thing T158 exists to catch.
                // Its 66.9px is preserved in route-t165-counter-form-2026-08-24.md for the record.
                foreach (string form in new[]
                {
                    "MATCH 4/4", "GAME 4/4", "FIXTURE 4/4", "TELLING 4/4",
                })
                {
                    (float lo, float hi, float y0, float y1) = Ink(leg, form);
                    bool yMeet = y0 < hY1 && y1 > hY0;
                    string left = hLo <= lo ? "TicketHeader" : "Leg";
                    float clearance = left == "TicketHeader" ? lo - hHi : hLo - hi;
                    float width = hi - lo;
                    Debug.Log($"[T165-FORM] {form,-12} ink {width,6:0.0}px  x {lo:0.0}..{hi:0.0}  "
                              + $"clearance to TicketHeader {clearance,6:0.0}px  "
                              + $"y {(yMeet ? "SHARED" : "disjoint")}  "
                              + (!yMeet ? "floor N/A (different rows)"
                                 : clearance >= InkFloor ? "FITS" : $"** FAILS the {InkFloor:0.0}px floor **"));
                }

                Debug.Log("[T165-FORM] report only — no form is ruled here. The word goes to the DD "
                          + "on these numbers; the winner earns its T84 pool entry in the build diff.");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>THE PENDING WINDOW'S COMPOSITION, REPORT-ONLY — what `T143`/`S85` cost the
        /// intervention zone.
        ///
        /// <para><b>The zone does not resize to content</b> (§6's grid), and its own build note
        /// records the last overrun being ROUTED rather than absorbed: title + three options measured
        /// 110.0px in a 90.0px zone. So the composition is priced BEFORE any copy is authored — this
        /// lane's own law, and the reason `T165`'s word went to the DD on numbers rather than on
        /// arithmetic.</para>
        ///
        /// <para><b>What is new and why it costs height.</b> `T143` says the window NAMES EVERY DEAD
        /// LEG (<c>PendingDeadLegIndices</c>), and `S85`'s general rule says the surface states
        /// <c>NoSingleCallSaves</c> BEFORE the offer — the flag gates the STATEMENT, not the offer,
        /// since §7c rules saves stay LEGAL. Both are new ROWS in a zone already at 82.5 of 90.</para>
        ///
        /// <para><b>This rules nothing.</b> It reports rows, height and overrun for each candidate
        /// shape; the height and the copy are the DD's.</para></summary>
        [Test]
        public void T143_S85_price_the_pending_window_composition()
        {
            var go = new GameObject("PendingWindowComposition");
            try
            {
                var screen = BuildScreen(go);
                TMP_Text prompt = FindChild<TMP_Text>(screen, "InterventionPrompt");
                Assert.IsNotNull(prompt, "InterventionPrompt is not built on this screen");
                Assert.IsNotNull(prompt.font, "no font resolved — a measurement in the fallback face is void");
                Assert.IsTrue(prompt.font.name.Contains("Encode"),
                    $"measured in '{prompt.font.name}', not Encode Sans — the mistake T20 made once");

                float zoneW = prompt.rectTransform.sizeDelta.x;
                float zoneH = prompt.rectTransform.sizeDelta.y;
                Debug.Log($"[PENDZONE] InterventionPrompt zone {zoneW:0.0} x {zoneH:0.0}");

                // The shipped rows, verbatim from PendingWindowBeat.
                const string optM = "HOLD M MULLIGAN (ONE MULLIGAN SLIP)";
                const string optR = "HOLD R SEND TO REVIEW (ONE REF'S WHISTLE)";
                const string optN = "N LET IT DIE";

                // CANDIDATE ROWS, not authored copy — the SHAPES the two rulings require, at the
                // longest plausible content so the measurement prices the worst case rather than the
                // happy one (C46). Real leg names come from MarketSheet and are uppercased there.
                const string deadOne = "DULUTH AUDITORS +1.5 IS DEAD";
                const string deadTwo = "DULUTH AUDITORS +1.5 AND BRICKLAYERS OVER 2.5 ARE DEAD";
                const string noSave = "NO SINGLE CALL SAVES THIS TICKET";

                foreach ((string label, string[] rows) in new[]
                {
                    ("shipped worst case (both consumables)", new[] { optM, optR, optN }),
                    ("+ one dead leg named", new[] { deadOne, optM, optR, optN }),
                    ("+ two dead legs named", new[] { deadTwo, optM, optR, optN }),
                    ("+ two dead + no-single-call-saves", new[] { deadTwo, noSave, optM, optR, optN }),
                    ("no-save case only (S85 minimum)", new[] { noSave, optM, optR, optN }),

                    // OPTION 1 — A ROW YIELDS. The two spending rows appear only when the run OWNS
                    // that consumable, so the row count is 1 + canM + canR. Priced at every ownership
                    // combination because C46 forbids leaning on the common case — the zone's own
                    // build note makes that explicit for the shipped composition.
                    ("opt1: one consumable + dead leg", new[] { deadOne, optR, optN }),
                    ("opt1: no consumables + dead leg", new[] { deadOne, optN }),
                    ("opt1: one consumable + dead + no-save", new[] { deadOne, noSave, optR, optN }),

                    // OPTION 3 — THE COPY SHARES AN EXISTING ROW rather than taking a new one.
                    ("opt3: dead leg joins the decline row", new[] { optM, optR, deadOne + "   ·   " + optN }),
                    ("opt3: two legs join the decline row", new[] { optM, optR, deadTwo + "   ·   " + optN }),
                    ("opt3: no-save joins the decline row", new[] { optM, optR, noSave + "   ·   " + optN }),
                })
                {
                    string composed = string.Join("\n", rows);
                    Vector2 pref = prompt.GetPreferredValues(composed, zoneW, 0f);
                    float widest = 0f;
                    string widestRow = string.Empty;
                    foreach (string r in rows)
                    {
                        float w = prompt.GetPreferredValues(r, 100000f, 0f).x;
                        if (w > widest) { widest = w; widestRow = r; }
                    }
                    bool fitsH = pref.y <= zoneH;
                    bool fitsW = widest <= zoneW;
                    Debug.Log($"[PENDZONE] {label,-38} rows={rows.Length} "
                        + $"h={pref.y,6:0.0} vs {zoneH:0.0} {(fitsH ? "FITS" : $"OVER by {pref.y - zoneH:0.0}")}"
                        + $"  widest row {widest,6:0.0} vs {zoneW:0.0} {(fitsW ? "fits" : "OVERRUNS")}"
                        + $"  '{widestRow}'");
                }

                Debug.Log("[PENDZONE] report only — no copy is authored here and no height is ruled. "
                    + "The zone does not resize to content (§6), so which row yields is the DD's call, "
                    + "reported with the zone's dimensions as the standing condition requires.");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>THE TEAM-TOTAL NEED FALLBACK, MEASURED — <c>docs/design/measurement-ask-team-total-fallback-2026-08-25.md</c>.
        ///
        /// <para>Report-only. It rules nothing; §4 of the ask pre-committed what each result means
        /// BEFORE the number existed, which is why this file authors no reading.</para>
        ///
        /// <para><b>Through the REAL fit path, not <c>GetPreferredValues</c>.</b> The chain the ask
        /// names is <c>DescribeActiveLeg</c>'s <c>default:</c> → <c>LegStatement</c>'s
        /// <c>default:</c> → <c>SheetName</c> → the sheet's own row name, then
        /// <c>FitOrFallback(t, primary, "")</c> — whose empty fallback means it lands on
        /// <c>FitToColumn</c>, which drops whole words FROM THE END. Both private members are
        /// reached by reflection so the measurement walks the shipped code rather than a copy of
        /// it.</para>
        ///
        /// <para><b>What is reported per case</b> (ask §2): the input string and its width, the
        /// string <c>FitToColumn</c> returns and ITS width, and two explicit flags — whether the
        /// DISTINCTIVE final word survived, and whether a single over-wide word came back whole
        /// (<c>T46</c>'s containment backstop being reached by shipped copy).</para></summary>
        [Test]
        public void T156_measure_the_team_total_NEED_fallback_through_the_real_fit_path()
        {
            var go = new GameObject("TeamTotalFallback");
            try
            {
                var screen = BuildScreen(go);
                TMP_Text need = FindChild<TMP_Text>(screen, "LegRowNeed0");
                Assert.IsNotNull(need, "LegRowNeed0 is not built — the NEED span is the ask's subject");
                Assert.IsNotNull(need.font, "no font resolved — a measurement in the fallback face is void");
                Assert.IsTrue(need.font.name.Contains("Encode"),
                    $"measured in '{need.font.name}', not Encode Sans — the mistake T20 made once");

                float box = need.rectTransform.rect.width;
                Debug.Log($"[TT-FIT] NEED box {box:0.0}px · commit {CommitAtMeasurement()} · T168-am BUILT: false "
                    + "(no reference to T168 anywhere under Assets/**; the club token is still the FULL name)");

                MethodInfo legStatement = typeof(TvSweatScreen).GetMethod(
                    "LegStatement", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo fitToColumn = typeof(TvSweatScreen).GetMethod(
                    "FitToColumn", BindingFlags.NonPublic | BindingFlags.Static);
                Assert.IsNotNull(legStatement, "TvSweatScreen.LegStatement not found — renamed? The "
                    + "measurement must fail rather than silently measure a string the surface never emits.");
                Assert.IsNotNull(fitToColumn, "TvSweatScreen.FitToColumn not found — renamed?");

                // Find the clubs the ask asks for, off a real board rather than constructed.
                // ⚠ KEYED ON THE TEAM, NOT THE MATCHUP. The first version bucketed the MATCHUP, so a
                // matchup whose two teams had different name lengths landed in BOTH buckets and cases
                // 1 and 3 measured the same club — three distinct rows reported as four. Caught by
                // reading the output rather than by the run, which passed.
                var run = new Run("TT-FALLBACK", new RunConfig());
                (Matchup M, Side S) oneWord = (null, Side.Home), twoWord = (null, Side.Home);
                foreach (Matchup m in run.CurrentSlate.Matchups)
                    foreach (Side side in new[] { Side.Home, Side.Away })
                    {
                        Team t = side == Side.Home ? m.Home : m.Away;
                        int words = t.Name.Split(' ').Length;
                        if (words == 2 && oneWord.M == null) oneWord = (m, side);   // "City Noun"
                        if (words > 2 && twoWord.M == null) twoWord = (m, side);    // "Two Word Noun"
                    }
                Assert.IsNotNull(oneWord.M, "no one-word-city club on this slate — cases 1/2 unreachable");

                void Measure(string label, (Matchup M, Side S) pick, MarketKind kind,
                    System.Func<MarketSelection, bool> want)
                {
                    Matchup m = pick.M;
                    MarketSelection sel = default;
                    bool found = false;
                    // The team-total offers carry their team in a NAMED field; match on it so the row
                    // measured is the CLUB this case is about rather than whichever side came first.
                    foreach (MarketOffer o in m.Markets)
                        if (o.Selection.Kind == kind && o.Selection.Team == pick.S && want(o.Selection))
                        { sel = o.Selection; found = true; break; }
                    if (!found) { Debug.Log($"[TT-FIT] {label,-34} NOT OFFERED on this matchup — case unreachable"); return; }

                    var leg = new Leg(m, sel, 2.00);
                    var input = (string)legStatement.Invoke(screen, new object[] { leg });
                    var fitted = (string)fitToColumn.Invoke(null, new object[] { need, input });
                    float wIn = need.GetPreferredValues(input, 100000f, 0f).x;
                    float wOut = need.GetPreferredValues(fitted, 100000f, 0f).x;

                    // THE TWO FLAGS THE ASK REQUIRES, both stated rather than left to the reader.
                    string[] inWords = input.Split(' ');
                    string distinctive = inWords[inWords.Length - 1];
                    bool distinctiveSurvived = fitted.EndsWith(distinctive);
                    bool singleWordWhole = !fitted.Contains(" ") && wOut > box;

                    Debug.Log($"[TT-FIT] {label,-34} in '{input}' {wIn,7:0.0}px  ->  OUT '{fitted}' {wOut,7:0.0}px "
                        + $"vs box {box:0.0}  · distinctive '{distinctive}' {(distinctiveSurvived ? "SURVIVES" : "** LOST **")}"
                        + $"{(singleWordWhole ? "  · ** T46 BACKSTOP REACHED: one over-wide word returned whole **" : "")}");
                }

                Measure("1 TeamTotalGoals 1.5 (1-word city)", oneWord, MarketKind.TeamTotalGoals,
                    s => System.Math.Abs(s.Line - 1.5) < 0.01);
                Measure("2 TeamTotalCards 1.5 (SAME club)", oneWord, MarketKind.TeamTotalCards,
                    s => System.Math.Abs(s.Line - 1.5) < 0.01);
                if (twoWord.M != null)
                    Measure("3 TeamTotalGoals 1.5 (2-word city)", twoWord, MarketKind.TeamTotalGoals,
                        s => System.Math.Abs(s.Line - 1.5) < 0.01);
                else
                    Debug.Log("[TT-FIT] 3 TeamTotalGoals 1.5 (2-word city)  NO two-word-city club on this "
                        + "slate — the rare case is unreachable here and is reported as such, not skipped silently");
                Measure("4 TeamTotalCorners 4.5 (control)", oneWord, MarketKind.TeamTotalCorners,
                    s => System.Math.Abs(s.Line - 4.5) < 0.01);

                Debug.Log("[TT-FIT] report only — §4 of the ask pre-committed the reading; this file authors none.");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>`C58-am2`: a routed width is meaningless without its build state, so the commit
        /// travels with the number. Read from the repo rather than hard-coded, so it cannot go stale.</summary>
        private static string CommitAtMeasurement()
        {
            try
            {
                string dir = System.IO.Path.GetDirectoryName(Application.dataPath);
                for (int i = 0; i < 8 && !string.IsNullOrEmpty(dir); i++)
                {
                    string dotGit = System.IO.Path.Combine(dir, ".git");

                    // ⚠ IN A WORKTREE `.git` IS A FILE, NOT A DIRECTORY — it holds `gitdir: <path>`.
                    // The first version only ever looked for `<dir>/.git/HEAD`, so it found nothing
                    // and reported the commit unreadable. This lane WORKS IN A WORKTREE, so the file
                    // form is the normal case here, not the exotic one.
                    string gitDir = null;
                    if (System.IO.Directory.Exists(dotGit)) gitDir = dotGit;
                    else if (System.IO.File.Exists(dotGit))
                    {
                        string line = System.IO.File.ReadAllText(dotGit).Trim();
                        const string marker = "gitdir:";
                        if (line.StartsWith(marker)) gitDir = line.Substring(marker.Length).Trim();
                    }

                    if (gitDir != null)
                    {
                        string head = System.IO.Path.Combine(gitDir, "HEAD");
                        if (!System.IO.File.Exists(head)) return "(HEAD missing — state it by hand)";
                        string h = System.IO.File.ReadAllText(head).Trim();
                        if (!h.StartsWith("ref:")) return h.Length >= 7 ? h.Substring(0, 7) : h;

                        string rel = h.Substring(4).Trim();
                        string refPath = System.IO.Path.Combine(gitDir, rel);
                        if (System.IO.File.Exists(refPath))
                            return System.IO.File.ReadAllText(refPath).Trim().Substring(0, 7);

                        // A packed ref, or a worktree whose HEAD points into the COMMON dir.
                        string common = System.IO.Path.Combine(gitDir, "commondir");
                        if (System.IO.File.Exists(common))
                        {
                            string cd = System.IO.File.ReadAllText(common).Trim();
                            string root = System.IO.Path.GetFullPath(System.IO.Path.Combine(gitDir, cd));
                            string p2 = System.IO.Path.Combine(root, rel);
                            if (System.IO.File.Exists(p2))
                                return System.IO.File.ReadAllText(p2).Trim().Substring(0, 7);
                        }
                        return "(ref unresolved — state it by hand)";
                    }
                    dir = System.IO.Path.GetDirectoryName(dir);
                }
            }
            catch (System.Exception e) { return $"(commit unreadable: {e.GetType().Name} — state it by hand)"; }
            return "(commit unreadable — state it by hand)";
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
                // ---- INK CLEARANCE, which is what "clearance" actually means -----------------
                // The rects above are BOXES. A box overlap is not a collision: T95 is this lane's
                // own case of two elements sharing a rect and reading fine, and of two CENTRED
                // elements with different boxes reading as a doubled line. What decides it is where
                // the INK sits inside each box, and that depends on the element's ALIGNMENT.
                //
                // Measured, not derived: each element's widest form is measured on the element
                // itself, and its ink extents are placed from its own alignment.
                Debug.Log("[T91-INK] ink extents, placed from each element's own alignment");
                var inkLo = new Dictionary<string, float>();
                var inkHi = new Dictionary<string, float>();
                var yLo = new Dictionary<string, float>();
                var yHi = new Dictionary<string, float>();
                foreach ((string nm, string widest) in new[]
                {
                    // T91-cl (batch 158): TicketHeader joins this set because Leg's neighbour
                    // changed — see the retired/added pairs below. Widest form read off
                    // TvExtentSweep.cs's own pool for this slot ("TICKET 2/2" / "TICKET 2 OF
                    // 2"), not invented here; both are 13 characters and the digit is tabular
                    // (T82's atlas working), so they measure equal and either is the widest.
                    ("TicketHeader", "TICKET 2/2"),
                    // T165-am (batch 178): `MATCH n/m`, not `LEG n/m` — the referent is the FIXTURE.
                    // This string, TvExtentSweep's pool for this slot, and the code's format are ONE
                    // fact in three places; T158 compares this against the pool but CANNOT see the
                    // code, which is what T165_the_counter_the_code_emits_is_in_the_pool closes.
                    ("Leg", "MATCH 4/4"),
                    ("Matchup", "BRICKLAYERS 0 \u2014 MIDDLEMEN 0"),
                    ("Clock", "90'+9"),
                })
                {
                    TMP_Text t = FindChild<TMP_Text>(screen, nm);
                    if (t == null) { Debug.Log($"[T91-INK] {nm} NOT FOUND"); continue; }
                    var cs = new Vector3[4];
                    t.rectTransform.GetWorldCorners(cs);
                    float bl = float.MaxValue, br = float.MinValue, by0 = float.MaxValue, by1 = float.MinValue;
                    for (int i = 0; i < 4; i++)
                    {
                        Vector3 lp = canvas.InverseTransformPoint(cs[i]);
                        bl = Mathf.Min(bl, lp.x); br = Mathf.Max(br, lp.x);
                        by0 = Mathf.Min(by0, lp.y); by1 = Mathf.Max(by1, lp.y);
                    }
                    float w = t.GetPreferredValues(widest, 100000f, 0f).x;
                    bool centred = t.alignment == TextAlignmentOptions.Top
                                || t.alignment == TextAlignmentOptions.Center
                                || t.alignment == TextAlignmentOptions.Bottom;
                    bool right = t.alignment == TextAlignmentOptions.TopRight
                              || t.alignment == TextAlignmentOptions.Right
                              || t.alignment == TextAlignmentOptions.BottomRight;
                    float lo = centred ? (bl + br) * 0.5f - w * 0.5f : right ? br - w : bl;
                    float hi = lo + w;
                    inkLo[nm] = lo; inkHi[nm] = hi; yLo[nm] = by0; yHi[nm] = by1;
                    Debug.Log($"[T91-INK] {nm,-9} align={t.alignment} widest '{widest}' {w:0.0}px  "
                              + $"box x {bl:0.0}..{br:0.0}  INK x {lo:0.0}..{hi:0.0}  y {by0:0.0}..{by1:0.0}");
                }

                // ("Leg", "Matchup") is RETIRED here, NOT deleted silently: this exact pair is the
                // whole reason this change exists. T91-cl measured it COLLIDING by 41.7px, Leg's
                // ink sharing Matchup's y-band, and that finding is why LEG n/m no longer lives in
                // this band at all (see BuildTicketColumn). It was checked, it failed, and the fix
                // was to move Leg rather than to widen anything. Leg's neighbour now is
                // TicketHeader, added below.
                const float InkFloor = 2f; // T90-am's floor; T91-am2 put it on both sides of the
                                            // column edge; T91-cl generalised it to ANY pair whose
                                            // ink shares a y-band, which is what is asserted below.
                foreach ((string a, string b) in new[] { ("TicketHeader", "Leg"), ("Matchup", "Clock") })
                {
                    if (!inkLo.ContainsKey(a) || !inkLo.ContainsKey(b)) continue;
                    // Left-to-right order is not assumed: the pair is ordered by measured position.
                    string left = inkLo[a] <= inkLo[b] ? a : b;
                    string rightN = left == a ? b : a;
                    float clearance = inkLo[rightN] - inkHi[left];
                    bool yMeet = yLo[a] < yHi[b] && yHi[a] > yLo[b];
                    Debug.Log($"[T91-INK] {left} -> {rightN} :: ink clearance {clearance:0.0}px, "
                              + $"y bands {(yMeet ? "INTERSECT" : "disjoint")} — "
                              + (clearance >= 0f
                                 ? "no ink collision"
                                 : yMeet
                                   ? $"** INK COLLIDES by {-clearance:0.0}px AT THE WIDEST FORMS **"
                                   : "x-overlap only, different rows"));
                    // T91-cl: the 2px ink floor is a property of ANY TWO ELEMENTS WHOSE INK SHARES
                    // A y-BAND, not of one particular seam, so it is asserted on every pair this
                    // block measures rather than only logged. A pre-existing violation is a
                    // finding, not a reason to soften this assertion.
                    Assert.GreaterOrEqual(clearance, InkFloor,
                        $"[T91-cl] ink floor violated between {left} and {rightN}: {left} ink " +
                        $"{inkLo[left]:0.0}..{inkHi[left]:0.0}, {rightN} ink " +
                        $"{inkLo[rightN]:0.0}..{inkHi[rightN]:0.0}, measured clearance " +
                        $"{clearance:0.0}px (floor {InkFloor:0.0}px)");
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

        // ---- THE MEASURED-FIXTURE TABLE, and the pin that guards it (T158) ------------------
        //
        // This lane shipped THREE fixtures measuring strings the product cannot emit — `MARCUS VALE
        // TO SCORE`, retired by T69, and the `LIVE • ...` progress form, which is canon in the
        // design-system components and was never implemented. All three were green throughout, because a fixture
        // string is not asserted against anything. These constants are the single source, and
        // T158 asserts every one of them against the T84 pool for its slot.
        internal const string FxNeedWidest = "ONE TEAM BLANKED";
        internal const string FxProgressWidest = "CLEAN-SHEET PATH LIVE";
        // WAS `LANYARD TO SCORE` UNTIL T158 CAUGHT IT — a NEED-shaped string measured against the
        // COMPACT slot, which renders the scorer arm as `{SURNAME} ANYTIME`. A fourth phantom, found
        // by the pin on its first run and in this seat's own instrument. Height is
        // string-independent so no routed number moves; the width in that log line was already
        // labelled as not being the slot's worst case.
        internal const string FxCompactProbe = "PAVEMENT ANYTIME";
        internal const string FxRiskWorst = "RISK $13,639";
        internal const string FxStakeWorst = "STAKE $13,639";
        internal const string FxPaysWorst = "PAYS $73,318,376,502";
        internal const string FxReturnedWorst = "RETURNED $73,318,376,502";
        internal const string FxPaidWorst = "PAID $73,318,376,502";
        internal const string FxMargin3Primary = "3+ GOALS APART AT FULL TIME";
        internal const string FxMargin3Fallback = "3+ GOALS APART AT FT";
        internal const string FxMargin2Primary = "2 GOALS APART AT FULL TIME";
        internal const string FxMargin2Fallback = "2 GOALS APART AT FT";

        /// <summary>Every string a layout test measures AS EVIDENCE, paired with the slot it is
        /// measured against. <see cref="T158_every_measured_fixture_string_is_in_the_T84_pool_for_its_slot"/>
        /// asserts each one is renderable in that slot.
        ///
        /// <para><b>Deliberately NOT covered: stress content.</b>
        /// <c>Zone_rects_are_unchanged_when_text_content_changes_dramatically</c> assigns absurd
        /// strings ON PURPOSE — its subject is that the rects do not move whatever the content, so an
        /// unrenderable string is the point rather than a defect. Same for the punch-overlay test's
        /// long matchup line. Those are exempt BY NAME here rather than by silence, so a later reader
        /// can see they were considered.</para></summary>
        internal static readonly (string Slot, string Text)[] MeasuredFixtures =
        {
            ("LegRowNeed0", FxNeedWidest),
            ("LegRowProgress0", FxProgressWidest),
            ("LegRowLine0", FxCompactProbe),
            ("RiskPays", FxRiskWorst),
            ("RiskPays", FxStakeWorst),
            ("Pays", FxPaysWorst),
            ("Pays", FxReturnedWorst),
            ("LegRowNeed0", FxMargin3Primary),
            ("LegRowNeed0", FxMargin3Fallback),
            ("LegRowNeed0", FxMargin2Primary),
            ("LegRowNeed0", FxMargin2Fallback),
        };

        /// <summary>Strings measured DELIBERATELY BEFORE THEY EXIST — candidates, not fixtures.
        ///
        /// <para>`T84`'s candidate instrument exists for exactly this and says so: <i>"the sweep's
        /// pools may hold only strings the code can already emit, and A CANDIDATE IS BY DEFINITION
        /// ONE IT CANNOT."</i> Measure-before-you-author is a precondition on this lane, so a rule
        /// that every measured string must be in the pool would forbid the practice it depends on.
        ///
        /// <para><b>The assertion is INVERTED for these, not waived.</b> A candidate must be ABSENT
        /// from the pool. Once it is adopted the code can emit it, it enters the pool, and this
        /// table is then wrong — so the pin fails and the entry moves to
        /// <see cref="MeasuredFixtures"/>. Neither table can rot silently.</para></summary>
        internal static readonly (string Slot, string Text, string Why)[] MeasuredCandidates =
        {
            ("Pays", FxPaidWorst,
             "T133's rung-2 candidate. Measured at 235.8px against the 249.0 box (13.2px spare, more "
             + "headroom than the incumbent PAYS), but NOT adopted: batch 108 rejected it for "
             + "colliding at the root with PAY $60, and that copy call is still open with the DD. "
             + "The surface cannot emit it, and that is correct."),

            // T165's three REJECTED words, registered so they cannot rot silently. All three were
            // priced against the real face and all three FIT — GAME 84.6px, TELLING 108.2px,
            // FIXTURE 109.4px, against 149.7px of available ink — so width rejected none of them.
            // T165-am ruled MATCH on vocabulary: MATCH is already shipped copy on this surface
            // (`THE MATCH ENDS LEVEL`; the scoreline slot is `Matchup`), GAME appears in no shipped
            // copy at all, and FIXTURE/TELLING are engine words the player has never seen.
            ("Leg", "GAME 4/4",
             "T165 candidate, priced at 84.6px ink / 69.1px clearance — FITS. Not adopted: GAME "
             + "appears in NO shipped copy, so it would be a second word for a concept the surface "
             + "already names (T94's family). T165-am ruled MATCH."),
            ("Leg", "FIXTURE 4/4",
             "T165 candidate, priced at 109.4px ink / 44.3px clearance — FITS. Not adopted: engine "
             + "vocabulary. T165-am ruled MATCH."),
            ("Leg", "TELLING 4/4",
             "T165 candidate, priced at 108.2px ink / 45.6px clearance — FITS. Not adopted: "
             + "`telling` is the session contract's word and the player has never seen it. "
             + "T165-am ruled MATCH."),
        };

        /// <summary>EVERY STRING A LAYOUT TEST MEASURES MUST BE RENDERABLE IN THE SLOT IT IS MEASURED
        /// AGAINST. Ordered by Allen after this lane shipped three phantom fixtures in one file.
        ///
        /// <para>The T84 pool is the enumerated set of strings each slot can actually produce. A
        /// measured string missing from it means either the fixture is a phantom or the pool is
        /// incomplete — and both are findings, which is why this asserts rather than logs.</para>
        ///
        /// <para><b>The pool lives in another assembly and is reached by reflection.</b>
        /// <c>SBR.Tests.EditMode</c> does not reference <c>SBR.Game.Editor</c> and deliberately still
        /// does not — a guard is not worth a build-graph change. <b>A lookup that fails must FAIL THE
        /// PIN, never skip it:</b> a guard that passes while blind is exactly the defect class this
        /// pin exists to close, and this file has three examples of it.</para></summary>
        [Test]
        public void T158_every_measured_fixture_string_is_in_the_T84_pool_for_its_slot()
        {
            System.Type sweep = System.Type.GetType("SBR.EditorTools.TvExtentSweep, SBR.Game.Editor");
            Assert.IsNotNull(sweep,
                "could not load SBR.EditorTools.TvExtentSweep from SBR.Game.Editor — the pin cannot "
                + "see the pool it is meant to check, and a pin that cannot see its subject must fail "
                + "rather than pass.");
            FieldInfo casesField = sweep.GetField("Cases",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(casesField,
                "TvExtentSweep.Cases not found by reflection — renamed? The pin fails rather than "
                + "silently checking nothing.");
            var cases = (System.Array)casesField.GetValue(null);
            Assert.IsNotNull(cases, "TvExtentSweep.Cases read as null");
            Assert.Greater(cases.Length, 0, "TvExtentSweep.Cases is EMPTY — nothing to check against");

            // The pool is an array of value tuples (slot, source, strings). Read the fields by name
            // so a reordering of the tuple cannot silently repoint this at the wrong member.
            var pool = new Dictionary<string, HashSet<string>>();
            foreach (object row in cases)
            {
                System.Type rt = row.GetType();
                var slot = (string)rt.GetField("Item1").GetValue(row);
                var strings = (string[])rt.GetField("Item3").GetValue(row);
                if (slot == null || strings == null) continue;
                if (!pool.TryGetValue(slot, out HashSet<string> set))
                    pool[slot] = set = new HashSet<string>();
                foreach (string str in strings) set.Add(str);
            }
            Debug.Log($"[T158] pool read: {pool.Count} slots, "
                      + $"{pool.Values.Sum(v => v.Count)} enumerated strings");

            var missing = new List<string>();
            foreach ((string slot, string text) in MeasuredFixtures)
            {
                if (!pool.TryGetValue(slot, out HashSet<string> set))
                {
                    missing.Add($"slot '{slot}' has NO POOL AT ALL (measuring '{text}')");
                    continue;
                }
                if (set.Contains(text)) continue;
                // Name a near neighbour so the report says what the slot CAN render, not merely
                // what it cannot.
                string nearest = set.OrderBy(x => Mathf.Abs(x.Length - text.Length))
                                    .FirstOrDefault() ?? "(pool empty)";
                missing.Add($"'{text}' is not renderable in '{slot}' "
                            + $"({set.Count} strings pooled there; e.g. '{nearest}')");
            }

            // Candidates: the assertion inverts. A candidate found IN the pool has been adopted,
            // and the finding is that this table is out of date rather than that anything is broken.
            foreach ((string slot, string text, string why) in MeasuredCandidates)
            {
                bool inPool = pool.TryGetValue(slot, out HashSet<string> cset) && cset.Contains(text);
                Debug.Log($"[T158] candidate '{text}' in '{slot}' :: "
                          + (inPool ? "NOW IN THE POOL — adopted" : "absent, as a candidate should be")
                          + $" · {why}");
                Assert.IsFalse(inPool,
                    $"'{text}' is listed as a CANDIDATE for '{slot}' but the pool now contains it, so "
                    + "the surface can emit it and it is no longer a candidate. Move it to "
                    + "MeasuredFixtures rather than leaving this table stale.");
            }

            foreach (string m in missing) Debug.Log($"[T158] PHANTOM {m}");
            Assert.IsEmpty(missing,
                "a layout test measures a string its slot cannot render. Either the fixture is a "
                + "phantom or the T84 pool is incomplete; BOTH are findings and neither is fixed by "
                + "narrowing this assertion.\n  " + string.Join("\n  ", missing));
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
                    // `Leg` LEFT THIS LIST AT T91-cl, and this is an inventory correction rather
                    // than a relaxation: it moved out of the scorebug into the ticket column header,
                    // so it is no longer right-hand zone content and this test is about right-hand
                    // zone content. Its new sibling `TicketHeader` has never been in this list
                    // either, because the ticket column takes no ZoneRoot of its own — `ZoneRoot` is
                    // called for the scorebug and the event strip ONLY, and the column's children
                    // clip to the canvas-level glass mask. The rule still binds every element that
                    // remains in the zone; the zone simply has one fewer member.
                    ("Clock", "ScoreBugZone"),
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
