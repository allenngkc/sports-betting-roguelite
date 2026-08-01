using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;
using UnityEngine;
using UnityEngine.UI;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// TV sweat refinement, Parts 2 &amp; 3: the beat-flash re-mapping onto DESIGN.md §4 (gold rationed
    /// to money, loss is darkness, everything else cold white/grey — green and red retired outright),
    /// and the canvas HDR path that lets §3's L4 tier exceed 1.0. These construct a `TvSweatScreen` in
    /// isolation (never entering play mode, `theaterEnabled = false` so `BuildCanvas` never touches
    /// `TheaterStage`/audio) and call the private `Awake`/`BuildCanvas` directly by reflection —
    /// mirroring the existing PlayMode suite's `PressCashOutInteract` pattern for exercising
    /// production methods instead of duplicating their logic.
    /// </summary>
    public class TvSweatScreenPaletteTests
    {
        private static float Luminance(Color c) => (c.r + c.g + c.b) / 3f;

        // Calibrated against the OLD retired literals so a reintroduction of either hue at a similar
        // magnitude is caught, without false-flagging the approved gold (r-dominant but g moderate)
        // or the approved white/grey (no channel dominates by this margin).
        private static bool LooksLikeRetiredRed(Color c) => c.r > 0.7f && c.g < 0.25f && c.b < 0.25f;
        private static bool LooksLikeRetiredGreen(Color c) => c.g > 0.7f && c.r < 0.35f && c.b < 0.6f;
        // T9 (Phase 3B): calibrated against chromeCyan's literal (0.62, 0.86, 0.96) — blue and green
        // both bright, red held back. This is the previous palette's general-chrome cyan; it has no
        // role in DESIGN.md §4 (context is grey). §8's VOID leg state is the ONE place cyan survives,
        // and only the `chromeCyan` field itself is allowed to read this way — see the test below.
        private static bool LooksLikeRetiredCyan(Color c) => c.b > 0.7f && c.g > 0.6f && c.r < 0.75f;

        /// <summary>T15 (Design Director ruling, 2026-07-31): the retired money language survived a
        /// full palette retirement by hiding in a place no palette test looked — embedded as raw hex
        /// inside rich-text markup strings rather than as a serialised <see cref="Color"/> field.
        ///
        /// <para>Every other scan in this file reflects over public <c>Color</c> fields. A string
        /// literal like <c>"&lt;color=#3CE873&gt;"</c> is invisible to all of them, which is exactly
        /// how the slip-strip violation shipped through T8's palette retirement untouched. The
        /// instance is gone — Phase 3C's Layout B rebuild removed the slip strip entirely and moved
        /// risk/pays into the ticket column footer — but <b>the blind spot is what the ruling asked
        /// us to close</b>, and nothing prevents the pattern returning tomorrow.</para>
        ///
        /// <para>So this scans the OWNED RUNTIME SOURCE rather than the object graph. That is an
        /// unusual shape for a test, and deliberate: it is the only way to see a colour that exists
        /// solely as text.</para>
        ///
        /// <para><b>Scope is this worktree's files only.</b> The identical pattern is live in
        /// <c>SportsbookApp.cs</c> (the SureThing surface, a forbidden file here) with the same three
        /// constants. That is routed to the Design Director, not fixed or asserted here — asserting
        /// over another worktree's file would make this suite fail for a reason its owner cannot act
        /// on from inside this repo boundary.</para></summary>
        [Test]
        public void No_retired_money_colour_hides_in_rich_text_markup_in_owned_runtime_source()
        {
            // The retired money language, as it appears in markup: money-good green, money-bad red,
            // and the previous palette's general-chrome cyan. DESIGN.md §4 retires all three — loss
            // is darkness, context is grey, and cyan survives only as §8's VOID leg state.
            string[] retiredHex = { "3CE873", "FF4038", "9EDCF6" };

            string runtimeDir = Path.Combine(
                Directory.GetCurrentDirectory(), "Assets", "SBR", "Runtime");
            Assert.IsTrue(Directory.Exists(runtimeDir),
                $"could not locate the owned runtime source at {runtimeDir} — if the project layout " +
                "moved, fix this path rather than deleting the scan");

            // Only files this worktree owns. SportsbookApp.cs / LaptopOs.cs belong to SureThing and
            // are excluded by name, not by accident — see the summary above.
            string[] notOurs = { "SportsbookApp.cs", "LaptopOs.cs", "LaptopScreen.cs", "LaptopUi.cs" };

            var offenders = new List<string>();
            foreach (string path in Directory.GetFiles(runtimeDir, "*.cs", SearchOption.AllDirectories))
            {
                string file = Path.GetFileName(path);
                if (notOurs.Contains(file)) continue;

                string[] lines = File.ReadAllLines(path);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    string trimmed = line.TrimStart();
                    // A comment naming a retired colour is documentation, not a use of it — this
                    // file's own summaries cite these constants, and flagging those would make the
                    // scan unmaintainable.
                    if (trimmed.StartsWith("//") || trimmed.StartsWith("///") || trimmed.StartsWith("*"))
                        continue;

                    foreach (string hex in retiredHex)
                        if (line.IndexOf(hex, System.StringComparison.OrdinalIgnoreCase) >= 0)
                            offenders.Add($"{file}:{i + 1}  {trimmed}");
                }
            }

            Assert.IsEmpty(offenders,
                "a retired money colour is present as raw hex in runtime source — DESIGN.md §4 retires " +
                "green and red outright and scopes cyan to the VOID leg state. Markup is still palette: " +
                "an approved colour system that a string can bypass is not enforced.\n  " +
                string.Join("\n  ", offenders));
        }

        [Test]
        public void Retired_green_and_red_fields_no_longer_exist_on_the_type()
        {
            Assert.IsNull(typeof(TvSweatScreen).GetField("phosphorGreen"),
                "phosphorGreen must be gone — DESIGN.md §4 retires green outright");
            Assert.IsNull(typeof(TvSweatScreen).GetField("hotRed"),
                "hotRed must be gone — DESIGN.md §4 retires red outright");
        }

        [Test]
        public void No_public_colour_field_reads_as_the_retired_saturated_red_or_green()
        {
            var go = new GameObject("PaletteScan");
            go.SetActive(false); // field defaults only — never let Awake/OnEnable fire here
            try
            {
                var screen = go.AddComponent<TvSweatScreen>();
                var offenders = typeof(TvSweatScreen)
                    .GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.FieldType == typeof(Color))
                    .Select(f => (f.Name, Color: (Color)f.GetValue(screen)))
                    .Where(x => LooksLikeRetiredRed(x.Color) || LooksLikeRetiredGreen(x.Color))
                    .Select(x => x.Name)
                    .ToList();

                Assert.IsEmpty(offenders,
                    $"these public Color fields still read as the retired money-good-green / " +
                    $"money-bad-red language: {string.Join(", ", offenders)}");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void No_public_colour_field_reads_as_retired_general_chrome_cyan_except_the_documented_VOID_field()
        {
            // T9 (Phase 3B): chromeCyan used to be used broadly for leg/clock/records/chrome/slip-strip
            // labels — general chrome duty that cyan has no role for in §4. Every one of those call
            // sites now resolves to flavorColor/contextGrey/structureGrey instead. The single exception
            // is `chromeCyan` itself, which DESIGN.md §8 still assigns to the `VOID` leg state — this
            // scan asserts that field is the ONLY public colour that is still allowed to read as cyan,
            // rather than silently permitting a reintroduction elsewhere under a different name.
            var go = new GameObject("CyanScan");
            go.SetActive(false); // field defaults only — never let Awake/OnEnable fire here
            try
            {
                var screen = go.AddComponent<TvSweatScreen>();
                var offenders = typeof(TvSweatScreen)
                    .GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.FieldType == typeof(Color))
                    .Select(f => (f.Name, Color: (Color)f.GetValue(screen)))
                    .Where(x => LooksLikeRetiredCyan(x.Color))
                    .Select(x => x.Name)
                    .Where(name => name != nameof(TvSweatScreen.chromeCyan))
                    .ToList();

                Assert.IsEmpty(offenders,
                    $"these public Color fields read as the retired general-chrome cyan (DESIGN.md §4 " +
                    $"assigns context to grey; only chromeCyan, scoped to §8's VOID state, may read " +
                    $"this way): {string.Join(", ", offenders)}");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Ordering_gold_below_goldL4_and_deadDark_below_gold_holds()
        {
            var go = new GameObject("OrderingCheck");
            go.SetActive(false); // field defaults only — never let Awake/OnEnable fire here
            try
            {
                var screen = go.AddComponent<TvSweatScreen>();

                // idle < flash < L4 (room-lead-reply.md §1): the quad's idle floor sits at ~0.068
                // (set in GrayboxRoomBuilder, out of this file's reach). A money/won flash must clear
                // it, and the true L4 moment (cash-out accept, payout tally) must clear the ordinary
                // money flash in turn — otherwise a win reads darker than rest, or L4 reads the same
                // as a routine leg win.
                const float quadIdleFloor = 0.068f;
                Assert.Greater(Luminance(screen.gold), quadIdleFloor,
                    "a money/won flash must read brighter than the quad's idle floor");
                Assert.Greater(Luminance(screen.goldL4), Luminance(screen.gold),
                    "the L4 tier (cash-out accept / payout tally) must read brighter than a routine gold flash");

                // Loss is darkness, not merely dim: it must drop BELOW the idle floor, the opposite
                // direction from every money flash, or the "dead" beat stops reading as a dip.
                Assert.Less(Luminance(screen.deadDark), quadIdleFloor,
                    "a loss/dead flash must drop below the idle floor to read as darkness, not a dim flash");
                Assert.Less(Luminance(screen.deadDark), Luminance(screen.gold),
                    "loss must never be as bright as a money beat");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GoldL2_sits_between_structure_and_the_L3_gold_flash()
        {
            // Phase 3C: goldL2 is the one new palette field this phase adds (the ticket column's
            // RISK/PAYS footer — DESIGN.md §7: "sit at the foot in gold at L2"). §3's ladder
            // requires L1 < L2 < L3 < L4; this pins goldL2 into that order against its neighbours
            // rather than trusting the literal alone.
            var go = new GameObject("GoldL2Ordering");
            go.SetActive(false); // field defaults only — never let Awake/OnEnable fire here
            try
            {
                var screen = go.AddComponent<TvSweatScreen>();

                Assert.Greater(Luminance(screen.goldL2), Luminance(screen.structureGrey),
                    "goldL2 (L2) must read brighter than structureGrey (L1)");
                Assert.Less(Luminance(screen.goldL2), Luminance(screen.gold),
                    "goldL2 (L2) must read dimmer than the L3 gold flash — it is a foot-of-column " +
                    "label, never the actionable cash-out amount");
                Assert.Less(Luminance(screen.goldL2), Luminance(screen.goldL4),
                    "goldL2 must never approach L4 — DESIGN.md §3 permits exactly one full-" +
                    "brightness element, and RISK/PAYS is not it");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GoldL4_carries_genuine_HDR_magnitude()
        {
            var go = new GameObject("HdrMagnitude");
            go.SetActive(false); // field defaults only — never let Awake/OnEnable fire here
            try
            {
                var screen = go.AddComponent<TvSweatScreen>();
                Assert.Greater(screen.goldL4.r, 1f,
                    "goldL4 must exceed 1.0 at the source so it has something to hand the HDR-boosted " +
                    "canvas material / the shared bloom volume once it clears the UGUI vertex-colour clamp");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Canvas_black_floor_matches_the_rooms_quad_lift()
        {
            var go = new GameObject("BlackFloor");
            go.SetActive(false); // field defaults only — never let Awake/OnEnable fire here
            try
            {
                var screen = go.AddComponent<TvSweatScreen>();
                var floor = new Color(0.048f, 0.055f, 0.068f);
                const float tol = 1e-4f;

                AssertRgbApprox(floor, screen.screenBg, tol, nameof(screen.screenBg));
                AssertRgbApprox(floor, screen.barBgColor, tol, nameof(screen.barBgColor));
                AssertRgbApprox(floor, screen.pitchBgColor, tol, nameof(screen.pitchBgColor));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void DeadDark_is_the_single_documented_dip_below_the_black_floor()
        {
            // T10 (Phase 3B): the ordering law is `deadDark < idle < gold < goldL4`, and deadDark
            // sitting BELOW the black floor is deliberate — "loss is a dip, not a smaller flash" — and
            // is pinned separately by Ordering_gold_below_goldL4_and_deadDark_below_gold_holds above.
            // This test names the exception explicitly, rather than the floor-scan below silently
            // excluding it, so a future reader sees the dip is intentional and singular.
            var go = new GameObject("DeadDarkException");
            go.SetActive(false); // field defaults only — never let Awake/OnEnable fire here
            try
            {
                var screen = go.AddComponent<TvSweatScreen>();
                var floor = new Color(0.048f, 0.055f, 0.068f);

                Assert.Less(screen.deadDark.r, floor.r, "deadDark.r should sit below the black floor");
                Assert.Less(screen.deadDark.g, floor.g, "deadDark.g should sit below the black floor");
                Assert.Less(screen.deadDark.b, floor.b, "deadDark.b should sit below the black floor");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void No_public_colour_field_sits_below_the_black_floor_except_deadDark()
        {
            var go = new GameObject("FloorFieldScan");
            go.SetActive(false); // field defaults only — never let Awake/OnEnable fire here
            try
            {
                var screen = go.AddComponent<TvSweatScreen>();
                var floor = new Color(0.048f, 0.055f, 0.068f);
                const float tol = 1e-4f;

                var offenders = typeof(TvSweatScreen)
                    .GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => f.FieldType == typeof(Color))
                    .Where(f => f.Name != nameof(TvSweatScreen.deadDark)) // the one documented exception
                    .Select(f => (f.Name, Color: (Color)f.GetValue(screen)))
                    .Where(x => x.Color.r < floor.r - tol || x.Color.g < floor.g - tol || x.Color.b < floor.b - tol)
                    .Select(x => x.Name)
                    .ToList();

                Assert.IsEmpty(offenders,
                    $"these public Color fields sit darker than the agreed black floor (0.048, 0.055, " +
                    $"0.068) on at least one channel, undoing the room's emissive-quad lift: " +
                    $"{string.Join(", ", offenders)}");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void RunOver_emission_rest_values_do_not_undo_the_black_floor_lift()
        {
            // T10 (Phase 3B): TvSweatScreen used to set `_emissRest` from two hardcoded literals in
            // RenderRunOver — `gold * 0.08f` (RunWon) and `new Color(0.008f, 0.010f, 0.018f)` (RunLost)
            // — that bypassed the room-owned `_emissIdle` and, on inspection, both sat under the agreed
            // black floor on at least one channel (RunLost on all three; RunWon on blue alone, since
            // gold's blue component at 8% is only 0.0144). They are now RunWonRest()/RunLostRest(),
            // each clamped component-wise to the floor. This exercises the actual production values via
            // reflection rather than re-deriving the arithmetic here.
            var go = new GameObject("EmissRestFloor");
            go.SetActive(false); // field defaults only — never let Awake/OnEnable fire here
            try
            {
                var screen = go.AddComponent<TvSweatScreen>();
                var floor = new Color(0.048f, 0.055f, 0.068f);
                const float tol = 1e-4f;

                Color won = InvokePrivateFunc<Color>(screen, "RunWonRest");
                Color lost = InvokePrivateFunc<Color>(screen, "RunLostRest");

                Assert.GreaterOrEqual(won.r, floor.r - tol, "RunWonRest.r must not undo the black-floor lift");
                Assert.GreaterOrEqual(won.g, floor.g - tol, "RunWonRest.g must not undo the black-floor lift");
                Assert.GreaterOrEqual(won.b, floor.b - tol,
                    "RunWonRest.b must not undo the black-floor lift (gold's blue channel at 8% used to sit under it)");

                Assert.GreaterOrEqual(lost.r, floor.r - tol, "RunLostRest.r must not undo the black-floor lift");
                Assert.GreaterOrEqual(lost.g, floor.g - tol, "RunLostRest.g must not undo the black-floor lift");
                Assert.GreaterOrEqual(lost.b, floor.b - tol,
                    "RunLostRest.b must not undo the black-floor lift (the old (0.008, 0.010, 0.018) sat roughly 6x darker)");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        private static T InvokePrivateFunc<T>(object target, string method)
        {
            MethodInfo m = target.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(m, $"{target.GetType().Name}.{method} not found by reflection — was it renamed?");
            return (T)m.Invoke(target, null);
        }

        private static void AssertRgbApprox(Color expected, Color actual, float tol, string label)
        {
            Assert.AreEqual(expected.r, actual.r, tol, $"{label}.r below the room's quad-lift floor");
            Assert.AreEqual(expected.g, actual.g, tol, $"{label}.g below the room's quad-lift floor");
            Assert.AreEqual(expected.b, actual.b, tol, $"{label}.b below the room's quad-lift floor");
        }

        [Test]
        public void Hdr_ui_shader_is_present_in_the_build()
        {
            Shader shader = Shader.Find("SBR/TvSweatHdrUI");
            Assert.IsNotNull(shader,
                "SBR/TvSweatHdrUI must be importable — without it the L4 canvas elements silently " +
                "fall back to the LDR-clamped default UI material (TvSweatScreen.MakeHdrMaterial's " +
                "documented, non-throwing fallback)");
        }

        /// <summary>The complete, closed-world list of canvas elements ELIGIBLE to reach L4 — i.e.
        /// which graphics carry the unclamped HDR material and are therefore physically capable of
        /// exceeding 1.0. Adding a name here is a DESIGN decision, not an implementation one.
        ///
        /// <para><b>C3 (Design Director ruling, superseding the Phase 3C rationale below the closed-
        /// world test):</b> eligibility is NOT simultaneity. This list used to hold exactly three
        /// names — CashOut, BigAmount, GoldFlood — on the theory that narrowing WHO could exceed 1.0
        /// was itself what enforced `DESIGN.md` §3's "at most one full-brightness element at any
        /// instant." That over-enforced: it also meant the score at a goal and the ball at a payoff
        /// could never reach the brightness §3/§7 grant them, because they were never even eligible.
        /// The DD ruled the set widened to five (Score and Ball join the original three; the live-leg
        /// pulse stays explicitly OUT), and that §3's one-at-a-time rule is now enforced separately,
        /// by the named <c>_l4Holder</c> / <c>RequestL4</c> / <c>ReleaseL4</c> invariant in
        /// <c>TvSweatScreen.cs</c> — see <see cref="Only_one_eligible_focus_holds_the_L4_token_at_once"/>
        /// and <see cref="Momentary_punch_preempts_sustained_hold_and_the_loser_yields"/> below.</para></summary>
        private static readonly string[] SanctionedL4Elements =
            { "CashOut", "BigAmount", "GoldFlood", "Score", "Ball" };

        // ------------------------------------------------------------------ C3: the one-token
        // invariant. Reflection, because RequestL4/ReleaseL4/_l4Holder are private and should stay
        // that way — the invariant is enforced INSIDE the type, and widening its surface just to
        // test it would create the very bypass the single choke point exists to prevent.

        private static object HdrFocusValue(string name)
        {
            System.Type t = typeof(TvSweatScreen).GetNestedType("HdrFocus", BindingFlags.NonPublic);
            Assert.IsNotNull(t, "HdrFocus enum not found — C3's token model was renamed or removed");
            return System.Enum.Parse(t, name);
        }

        private static bool RequestL4(TvSweatScreen s, string focus, bool momentary)
            => (bool)typeof(TvSweatScreen)
                .GetMethod("RequestL4", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(s, new[] { HdrFocusValue(focus), (object)momentary });

        private static void ReleaseL4(TvSweatScreen s, string focus)
            => typeof(TvSweatScreen)
                .GetMethod("ReleaseL4", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(s, new[] { HdrFocusValue(focus) });

        private static string L4Holder(TvSweatScreen s)
        {
            object v = typeof(TvSweatScreen)
                .GetField("_l4Holder", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(s);
            return v == null ? null : v.ToString();
        }

        /// <summary>Counts how many HDR-eligible materials are actually sitting at the L4 boost.
        /// This reads the MATERIALS, not the holder field — the holder saying "one" while two
        /// materials are lit would be exactly the bug the invariant exists to prevent, and a test
        /// that only read the holder could never see it.</summary>
        private static int MaterialsAtL4(TvSweatScreen s)
        {
            string[] mats = { "_cashOutHdrMat", "_bigAmountHdrMat", "_goldFloodHdrMat", "_scoreHdrMat", "_ballHdrMat" };
            int boostId = Shader.PropertyToID("_HdrBoost");
            int n = 0;
            foreach (string f in mats)
            {
                var m = (Material)typeof(TvSweatScreen)
                    .GetField(f, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(s);
                if (m != null && m.GetFloat(boostId) > 1.5f) n++;
            }
            return n;
        }

        private static TvSweatScreen BuiltScreen(GameObject go)
        {
            var screen = go.AddComponent<TvSweatScreen>();
            screen.theaterEnabled = false;
            InvokePrivate(screen, "Awake");
            return screen;
        }

        /// <summary>C3 (Design Director ruling, 2026-07-31): <b>eligibility is not simultaneity.</b>
        ///
        /// <para>The previous implementation enforced "one full-brightness element" by making only
        /// three graphics capable of exceeding 1.0. That was a ceiling wearing a guarantee's
        /// clothes: it also meant the score at a goal and the ball at a payoff could never reach the
        /// brightness §3 and §7 grant them. Five are now eligible, so simultaneity needs enforcing
        /// for real — this is that test.</para>
        ///
        /// <para>Note it counts lit MATERIALS rather than trusting the holder field. A holder that
        /// says "one" while two materials sit at L4 is precisely the failure worth catching, and it
        /// is invisible to a test that only reads the bookkeeping.</para></summary>
        [Test]
        public void At_most_one_element_holds_the_L4_token_however_many_request_it()
        {
            var go = new GameObject("L4Token");
            go.SetActive(false);
            try
            {
                TvSweatScreen s = BuiltScreen(go);

                Assert.AreEqual(0, MaterialsAtL4(s), "a freshly built canvas must have nothing at L4");

                Assert.IsTrue(RequestL4(s, "CashOut", false), "an uncontested sustained request must succeed");
                Assert.AreEqual(1, MaterialsAtL4(s));

                // Every other eligible focus piles on. Whatever the arbitration decides, the count
                // may never exceed one.
                foreach (string f in new[] { "Payout", "Score", "Ball", "CashOut" })
                {
                    RequestL4(s, f, true);
                    // The invariant is over FOCUSES, not materials. Payout deliberately drives both
                    // BigAmount and GoldFlood — a payout tally and its gold wash are one visual
                    // moment, so they move as a single participant. An earlier version of this test
                    // asserted a flat material count of 1 and failed on exactly that, which is the
                    // eligibility-vs-simultaneity confusion C3 corrected, made one level down:
                    // "how many things are lit" is not "how many things decided to be lit".
                    int expected = L4Holder(s) == "Payout" ? 2 : 1;
                    Assert.AreEqual(expected, MaterialsAtL4(s),
                        $"after {f} requested L4, the lit materials must correspond to exactly ONE " +
                        $"focus (holder={L4Holder(s)}, so {expected} material(s)) — the token is the " +
                        "whole enforcement now that eligibility is wider than one");
                }

                ReleaseL4(s, L4Holder(s));
                Assert.AreEqual(0, MaterialsAtL4(s), "releasing the holder must leave nothing at L4");
            }
            finally { Object.DestroyImmediate(go); }
        }

        /// <summary>C3 rule 4: a momentary punch preempts a sustained state, and the sustained
        /// element yields — in the same call, not on its own next frame. A loser that waits a frame
        /// to notice would put two elements at L4 across the frame boundary, which is the invariant
        /// broken in the one place a per-frame check would not see it.</summary>
        [Test]
        public void A_momentary_punch_preempts_a_sustained_hold_and_the_loser_yields_immediately()
        {
            var go = new GameObject("L4Arbitration");
            go.SetActive(false);
            try
            {
                TvSweatScreen s = BuiltScreen(go);

                RequestL4(s, "CashOut", false); // the cash-out band's sustained gold while actionable
                Assert.AreEqual("CashOut", L4Holder(s));

                Assert.IsTrue(RequestL4(s, "Score", true), "a momentary punch must take the token");
                Assert.AreEqual("Score", L4Holder(s), "the goal's score punch outranks a sustained hold");
                Assert.AreEqual(1, MaterialsAtL4(s),
                    "the preempted holder must drop to L3 in the SAME call — never two lit at once");

                // The reverse must NOT hold: a sustained request cannot evict an existing holder.
                Assert.IsFalse(RequestL4(s, "CashOut", false),
                    "a sustained request must not preempt — precedence is encoded, not call-ordered");
                Assert.AreEqual("Score", L4Holder(s), "the momentary holder keeps the token");
                Assert.AreEqual(1, MaterialsAtL4(s));

                // Releasing a token you do not hold must not clobber whoever does.
                ReleaseL4(s, "CashOut");
                Assert.AreEqual("Score", L4Holder(s),
                    "releasing a focus that is not the holder must be a no-op");
                Assert.AreEqual(1, MaterialsAtL4(s));
            }
            finally { Object.DestroyImmediate(go); }
        }

        // Canonical brightness tiers, from the studio design system
        // (main-2/docs/design/design-system/components/tv/tiers.js). Referenced, not forked —
        // mirrored here as constants because a Unity EditMode test cannot import a JS module, and
        // asserting against invented thresholds would defeat the point of having canon.
        private const float TierL2 = 0.4f;
        private const float TierL3 = 0.7f;

        /// <summary>T16 (Design Director ruling, 2026-07-31), asserted against the design system's
        /// own spec-of-record — `components/tv/TvMomentumTape.prompt.md` — not against a paraphrase
        /// of the ruling line. That spec is stricter than the summary and names three hard rules:
        ///
        /// <list type="bullet">
        /// <item><b>No numerals</b> — "the moment it needs one it has become the banned
        /// win-probability readout."</item>
        /// <item><b>No hue</b> — white and grey only; everything on this surface except gold is
        /// colourless.</item>
        /// <item><b>Never above L2</b> — it must not compete with the score above it or the live
        /// <c>NEED</c> line beside it.</item>
        /// </list>
        ///
        /// <para>The win-probability numeral is OUT permanently (§7's duplication ban — locked odds
        /// make that read the player's job), and the spec names the failure mode precisely: a tape
        /// that acquires a numeral has silently become the thing that was banned.</para></summary>
        [Test]
        public void Momentum_tape_obeys_no_numerals_no_hue_and_the_L2_ceiling()
        {
            // NOT named "...Tape..." on purpose. A previous version called this root "TapeAndProb",
            // and the substring search below matched the ROOT rather than the tape — so the test
            // walked the entire canvas and reported a ticket-column leg row as a tape violation.
            var go = new GameObject("T16Check");
            go.SetActive(false);
            try
            {
                TvSweatScreen s = BuiltScreen(go);

                // Exact name, not a substring. MomentumTape.Build names the object "MomentumTape"
                // and its children "LegTape_n" / "ResolutionCap" / "Beat_n" — a substring match on
                // "Tape" is ambiguous by construction.
                Transform tape = go.GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(t => t.name == "MomentumTape");
                Assert.IsNotNull(tape, "T16 rules the momentum tape IN — it must exist on the canvas");

                foreach (Graphic g in tape.GetComponentsInChildren<Graphic>(true))
                {
                    Color c = g.color;

                    // No hue: white and grey only. "Colourless" on this surface does NOT mean
                    // perfectly neutral — the design system's own cold white, --tv-fact #E7F1F5
                    // (main-2/.../tokens/palette-tv.css), is itself slightly cool with a channel
                    // spread of ~0.055. So the tolerance is set from canon plus headroom, not from
                    // a neutral ideal: a threshold of 0.06 admitted the token by a hair and rejected
                    // anything marginally cooler, which is a false positive waiting to happen.
                    // What this still catches is an actual hue — a green, red, or team colour.
                    const float coldWhiteSpread = 0.055f; // --tv-fact
                    float max = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
                    float min = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
                    Assert.LessOrEqual(max - min, coldWhiteSpread * 2f,
                        $"the tape carries no hue (T16 / TvMomentumTape spec) — '{g.name}' is " +
                        $"({c.r:0.00}, {c.g:0.00}, {c.b:0.00}), a channel spread of {max - min:0.00}");

                    // Never above L2. Compared against the canonical tier, with headroom below L3
                    // so the assertion fails on a real tier promotion rather than on rounding.
                    Assert.Less(Luminance(c), (TierL2 + TierL3) / 2f,
                        $"the tape never exceeds L2 ({TierL2}) — '{g.name}' reads " +
                        $"{Luminance(c):0.00}, competing with the score above it or the NEED line beside it");
                }

                foreach (Text t in tape.GetComponentsInChildren<Text>(true))
                    Assert.IsFalse(t.text != null && t.text.Any(char.IsDigit),
                        $"the tape carries no numerals — '{t.name}' renders \"{t.text}\". The spec is " +
                        "explicit about why: the moment it needs a numeral it has become the banned " +
                        "win-probability readout.");

                Assert.IsNull(typeof(TvSweatScreen).GetField("_tWinPct",
                        BindingFlags.NonPublic | BindingFlags.Instance),
                    "the win-probability numeral is OUT permanently (T16, §7 duplication ban) — " +
                    "its field must be gone, not merely unbuilt");
            }
            finally { Object.DestroyImmediate(go); }
        }

        /// <summary>Phase 3C guard, written BEFORE the Layout B canvas rebuild (PRD §8.1,
        /// `DESIGN.md` §3/§6); widened for C3.
        ///
        /// <para>The sibling test below spot-checks known elements for the HDR material and one
        /// known element without it. That is a whitelist, and a whitelist cannot catch a canvas
        /// rebuild that hands the material to an UNSANCTIONED element — it would simply pass. This
        /// test closes the world: it walks every <see cref="Graphic"/> in the built hierarchy and
        /// asserts the HDR-capable set is exactly <see cref="SanctionedL4Elements"/>, no more and no
        /// less.</para>
        ///
        /// <para><b>What this test does and does not prove (C3).</b> This is an ELIGIBILITY test —
        /// it proves exactly these five graphics are physically capable of exceeding 1.0, and nothing
        /// else is. It does NOT prove, and was never a substitute for proving, that at most one of
        /// them actually sits at L4 at any given instant — five eligible graphics could in principle
        /// all be boosted simultaneously by careless call sites, and this scan would still pass,
        /// because it only inspects which MATERIAL each graphic carries, never the boost each
        /// material's `_HdrBoost` currently holds. `DESIGN.md` §3's one-at-a-time rule is real
        /// simultaneity, and simultaneity is enforced by the one-token invariant tested below, not by
        /// this list's narrowness. A future reader must not re-derive "the ceiling is enforced by only
        /// five names existing" from this test — that reasoning is exactly what C3 ruled wrong.</para>
        ///
        /// <para>If this list widens further without a corresponding DD ruling, or drops a name (a
        /// cash-out band that silently lost its HDR material would leave the one moment the player
        /// can act on unable to reach full brightness), this test fails and names the offender.</para></summary>
        [Test]
        public void Exactly_the_sanctioned_elements_can_reach_L4_and_nothing_else()
        {
            var go = new GameObject("CanvasL4ClosedWorld");
            go.SetActive(false);
            try
            {
                var screen = go.AddComponent<TvSweatScreen>();
                screen.theaterEnabled = false;
                InvokePrivate(screen, "Awake");

                var hdr = new List<string>();
                foreach (Graphic g in go.GetComponentsInChildren<Graphic>(true))
                {
                    Material m = g.material;
                    if (m != null && m.shader != null && m.shader.name == "SBR/TvSweatHdrUI")
                        hdr.Add(g.gameObject.name);
                }
                hdr.Sort();

                var expected = new List<string>(SanctionedL4Elements);
                expected.Sort();

                CollectionAssert.AreEqual(expected, hdr,
                    "the set of canvas elements able to exceed 1.0 must be EXACTLY the sanctioned list. " +
                    "Extra names mean the canvas widened L4 — DESIGN.md §3 permits one full-brightness " +
                    "element at a time and that is enforced here by construction, not by discipline. " +
                    "Missing names mean an element that must reach L4 silently fell back to the clamped " +
                    "default material. Either way this is a design decision, not an implementation one: " +
                    "route it before editing SanctionedL4Elements.\n" +
                    $"expected: [{string.Join(", ", expected)}]\nactual:   [{string.Join(", ", hdr)}]");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void L4_canvas_elements_get_the_hdr_material_and_L3_elements_stay_default()
        {
            var go = new GameObject("CanvasWiring");
            go.SetActive(false); // defer Awake so BuildCanvas runs once, under our control
            try
            {
                var screen = go.AddComponent<TvSweatScreen>();
                screen.theaterEnabled = false; // keep BuildCanvas from touching TheaterStage/audio

                InvokePrivate(screen, "Awake");

                Text cashOut = FindChild<Text>(screen, "CashOut");
                Text bigAmount = FindChild<Text>(screen, "BigAmount");
                Image goldFlood = FindChild<Image>(screen, "GoldFlood");
                Image wonFlood = FindChild<Image>(screen, "WonFlood");
                Text flavor = FindChild<Text>(screen, "Flavor");

                Assert.IsNotNull(cashOut, "CashOut text not found — canvas layout changed?");
                Assert.IsNotNull(bigAmount, "BigAmount text not found — canvas layout changed?");
                Assert.IsNotNull(goldFlood, "GoldFlood image not found — canvas layout changed?");
                Assert.IsNotNull(wonFlood, "WonFlood image not found (renamed from GreenFlood) — canvas layout changed?");
                Assert.IsNotNull(flavor, "Flavor text not found — canvas layout changed?");

                Assert.AreEqual("SBR/TvSweatHdrUI", cashOut.material.shader.name,
                    "the cash-out band must be able to reach L4 (§8.5 Actionable)");
                Assert.AreEqual("SBR/TvSweatHdrUI", bigAmount.material.shader.name,
                    "the big win/cash-out amount must be able to reach L4 (§3: the payoff at its callback)");
                Assert.AreEqual("SBR/TvSweatHdrUI", goldFlood.material.shader.name,
                    "the gold flood must be able to reach L4 for the cash-out/payout beats");

                // Flavor text carries routine (non-L4) beat copy — it must NOT have opted into the
                // HDR material, or every beat would silently compete for the one L4 slot DESIGN.md §3
                // reserves for a single element.
                Assert.AreNotEqual("SBR/TvSweatHdrUI", flavor.material.shader.name,
                    "routine beat text must stay on the default (LDR) UI material — only one L4 element at a time");

                // No trace of the old GameObject name survives the WonLegBeat rename.
                Assert.IsNull(FindChild<Image>(screen, "GreenFlood"), "GreenFlood must be renamed to WonFlood");

                // The renamed flood's construction colour is gold-hued, not the retired green.
                Assert.IsFalse(LooksLikeRetiredGreen(new Color(wonFlood.color.r, wonFlood.color.g, wonFlood.color.b)),
                    "WonFlood's colour must be gold, not the retired green");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

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

        // ---------------------------------------------------------------------------------------
        // 3D — state vocabulary (DESIGN.md §8). Five leg states, six cash-out states.
        // ---------------------------------------------------------------------------------------

        /// <summary>Drives the real UpdateTicketColumn against a real ticket. The private
        /// <c>_ticket</c>/<c>_resolvedThrough</c> fields are set directly because the alternative is
        /// standing up a whole live session for a question that is purely about what a resolved row
        /// renders — and the method under test reads leg state, never how the state was reached.</summary>
        private static void RenderTicketColumn(TvSweatScreen s, Ticket ticket, int resolvedThrough, int liveLegIndex)
        {
            typeof(TvSweatScreen).GetField("_ticket", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(s, ticket);
            typeof(TvSweatScreen).GetField("_resolvedThrough", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(s, resolvedThrough);
            typeof(TvSweatScreen).GetMethod("UpdateTicketColumn", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(s, new object[] { liveLegIndex });
        }

        private static Ticket TwoLegTicket(string runId)
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

        [Test]
        public void Void_is_the_only_leg_state_that_carries_the_struck_through_rule()
        {
            // DESIGN.md §8: "VOID | L2 cyan, struck through on the matrix." Colour alone was
            // carrying that state — the strike did not exist, though the palette field's own comment
            // quoted the spec. The strike is what distinguishes CANCELLED from lost or won, so a row
            // that struck the wrong state would say the opposite of what happened.
            var go = new GameObject("VoidStrike");
            try
            {
                TvSweatScreen s = BuiltScreen(go);
                Ticket ticket = TwoLegTicket("3D-VOID-STRIKE");
                Assert.GreaterOrEqual(ticket.Legs.Count, 2, "this fixture needs two legs");

                // internal setter — the engine voids legs through SweatSession, which this test has no
                // business driving just to reach a rendered state. GetSetMethod(nonPublic: true) is
                // required: PropertyInfo.SetValue alone throws "property set method not found" here.
                MethodInfo setVoided = typeof(Leg).GetProperty("IsVoided").GetSetMethod(nonPublic: true);
                Assert.IsNotNull(setVoided, "Leg.IsVoided has no setter — engine shape changed?");
                setVoided.Invoke(ticket.Legs[0], new object[] { true });

                RenderTicketColumn(s, ticket, resolvedThrough: 2, liveLegIndex: -1);

                Image struck = FindChild<Image>(s, "LegRowStrike0");
                Image unstruck = FindChild<Image>(s, "LegRowStrike1");
                Assert.IsNotNull(struck, "LegRowStrike0 not found — §8's VOID strike is not built");
                Assert.IsNotNull(unstruck, "LegRowStrike1 not found");

                Assert.IsTrue(struck.enabled,
                    "a VOID leg must be struck through (DESIGN.md §8) — cyan alone does not say cancelled");
                Assert.IsFalse(unstruck.enabled,
                    "a non-void resolved leg must NOT be struck: a struck W or L reads as cancelled, " +
                    "which is the one thing the strike must never say");

                Text voidLine = FindChild<Text>(s, "LegRowLine0");
                Assert.IsNotNull(voidLine);
                AssertRgbApprox(s.chromeCyan, voidLine.color, 0.001f, "VOID row");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void The_strike_is_a_fixed_rule_never_measured_from_the_row_text()
        {
            // §6 forbids geometry computed from content. A strike sized to the statement would be
            // exactly that, and would also silently change width every time copy changed.
            var go = new GameObject("StrikeFixed");
            try
            {
                TvSweatScreen s = BuiltScreen(go);
                Image strike = FindChild<Image>(s, "LegRowStrike0");
                Text line = FindChild<Text>(s, "LegRowLine0");
                Assert.IsNotNull(strike, "LegRowStrike0 not found");
                Assert.IsNotNull(line, "LegRowLine0 not found");

                Assert.IsFalse(strike.enabled, "a freshly built row is not struck — VOID is a state, not a default");
                Assert.AreEqual(line.rectTransform.sizeDelta.x, strike.rectTransform.sizeDelta.x, 0.01f,
                    "the strike spans the compact line's fixed width, not its glyphs");

                Vector2 sizeBefore = strike.rectTransform.sizeDelta;
                line.text = "V";
                Assert.AreEqual(sizeBefore, strike.rectTransform.sizeDelta,
                    "the strike resized when the row's text changed — content must never drive geometry");
                line.text = new string('X', 120);
                Assert.AreEqual(sizeBefore, strike.rectTransform.sizeDelta,
                    "the strike resized on long copy — same defect, other direction");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void The_five_leg_states_are_five_distinguishable_treatments()
        {
            // §8's leg table assigns NEXT/LIVE/W/L/VOID five different treatments. A vocabulary whose
            // words look alike is not a vocabulary — this pins that no two collapsed onto one colour,
            // which is the failure mode a palette refactor produces without ever failing a test that
            // checks each colour in isolation.
            var go = new GameObject("FiveLegStates");
            try
            {
                TvSweatScreen s = BuiltScreen(go);
                var byState = new Dictionary<string, Color>
                {
                    { "NEXT (L1 structure)", s.structureGrey },
                    { "LIVE (L3 cold white)", s.flavorColor },
                    { "W (L3 gold)", new Color(s.gold.r, s.gold.g, s.gold.b, 1f) },
                    { "L (L0 dark)", s.deadDark },
                    { "VOID (L2 cyan)", s.chromeCyan },
                };

                foreach (KeyValuePair<string, Color> a in byState)
                    foreach (KeyValuePair<string, Color> b in byState)
                    {
                        if (a.Key == b.Key) continue;
                        float d = Mathf.Abs(a.Value.r - b.Value.r)
                                + Mathf.Abs(a.Value.g - b.Value.g)
                                + Mathf.Abs(a.Value.b - b.Value.b);
                        Assert.Greater(d, 0.05f,
                            $"'{a.Key}' and '{b.Key}' are the same treatment (channel distance {d:F3}). " +
                            "DESIGN.md §8 gives the five leg states five distinct treatments; brightness " +
                            "IS the state here, so two states that look alike are one state.");
                    }
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void The_eight_gate_states_never_contradict_one_another()
        {
            // PRD §5's Phase 3 exit gate, item 3, verbatim: "Open, suspended, unavailable,
            // pending-window, cashed-out, won, lost, and void states do not reuse contradictory
            // colors or labels." Eight states across TWO surfaces — five in the cash-out slot, three
            // leg outcomes. (phase-3-plan.md read this as "eight cash-out states"; the rectangle
            // holds six. Corrected 2026-07-31, with the real source recorded there.)
            //
            // The gate word is CONTRADICTORY, not unique. Suspended and pending-window share one
            // treatment on purpose (DESIGN.md §8: pending window is "As suspended"), so a uniqueness
            // assertion would fail on a pair the design intends. What must never happen is a state
            // that PROMISES input wearing the treatment of one that REFUSES it — DESIGN.md §8:
            // "brightness is a promise about input", the visual half of the TVS-H01 contract.
            var go = new GameObject("EightGateStates");
            try
            {
                TvSweatScreen s = BuiltScreen(go);
                Color live = new Color(s.gold.r, s.gold.g, s.gold.b, 1f);

                // Promises input: open (actionable), cashed-out (the accepted punch/settle).
                // Refuses input: suspended, pending-window, unavailable.
                var promises = new Dictionary<string, Color> { { "open", live }, { "cashed-out", live } };
                var refuses = new Dictionary<string, Color>
                {
                    { "suspended", s.structureGrey },
                    { "pending-window", s.structureGrey },
                    { "unavailable", s.structureGrey },
                };

                foreach (KeyValuePair<string, Color> p in promises)
                    foreach (KeyValuePair<string, Color> r in refuses)
                        Assert.Greater(ChannelDistance(p.Value, r.Value), 0.2f,
                            $"'{p.Key}' and '{r.Key}' wear the same treatment. One accepts the key and " +
                            "the other refuses it; if the slot looks the same in both, the surface has " +
                            "lied about what the press will do (PRD §5 gate item 3, DESIGN.md §8).");

                // The three leg outcomes must not contradict each other either: won is money, lost is
                // darkness, void is cancellation. Any two collapsing means a settled leg reads as the
                // wrong outcome — the most expensive contradiction on the surface.
                var outcomes = new Dictionary<string, Color>
                {
                    { "won", live }, { "lost", s.deadDark }, { "void", s.chromeCyan },
                };
                foreach (KeyValuePair<string, Color> a in outcomes)
                    foreach (KeyValuePair<string, Color> b in outcomes)
                    {
                        if (a.Key == b.Key) continue;
                        Assert.Greater(ChannelDistance(a.Value, b.Value), 0.2f,
                            $"'{a.Key}' and '{b.Key}' share a treatment — a settled leg would read as " +
                            "the wrong outcome (PRD §5 gate item 3).");
                    }

                // And the labels: the two states that share a colour by design must still be
                // separable, which is what the VOID strike exists for on the leg side.
                Assert.IsFalse(FindChild<Text>(s, "CashOut").enabled,
                    "the cash-out slot starts unavailable and quiet — §8.5's 'reserved slot remains " +
                    "visually quiet without reflow'");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        private static float ChannelDistance(Color a, Color b)
            => Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);

        // ---------------------------------------------------------------------------------------
        // 3E — §8.10 held cash-out preview.
        // ---------------------------------------------------------------------------------------

        private static void SetPreview(TvSweatScreen s, bool on)
            => typeof(TvSweatScreen).GetField("_cashOutPreview", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(s, on);

        [Test]
        public void The_preview_is_refused_wherever_acceptance_is_refused()
        {
            // §8.10: "The gate is CanAcceptCashOutNow, exactly as repaired in TVS-H01. If cash-out
            // cannot be accepted right now, it cannot be previewed right now." That single shared
            // gate is what keeps the previewed and accepted amounts the same number — a mid-tween
            // offer is refused by both, so the preview can never quote a price acceptance would not
            // honour. A screen with no live session cannot accept, so it must not preview.
            var go = new GameObject("PreviewGate");
            try
            {
                TvSweatScreen s = BuiltScreen(go);
                object entered = typeof(TvSweatScreen)
                    .GetMethod("EnterCashOutPreview", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(s, null);

                Assert.IsFalse((bool)entered,
                    "the preview entered without an acceptable offer — it would be quoting a price " +
                    "the accept path would refuse (§8.10, TVS-H01)");
                Assert.IsFalse((bool)typeof(TvSweatScreen)
                        .GetField("_cashOutPreview", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(s),
                    "a refused preview must leave no state behind");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void A_previewed_leg_is_struck_and_dimmed_one_level_never_extinguished()
        {
            // 3E: "renders one brightness level down and uses the VOID strike rather than the LOST
            // extinguish, because legs being CANCELLED must not read as legs LOST at the exact
            // moment a player is deciding." The strike says cancelled; L0 would say lost.
            var go = new GameObject("PreviewTreatment");
            try
            {
                TvSweatScreen s = BuiltScreen(go);
                Ticket ticket = TwoLegTicket("3E-PREVIEW-TREATMENT");

                RenderTicketColumn(s, ticket, resolvedThrough: 0, liveLegIndex: 0);
                Color liveInkBefore = FindChild<Text>(s, "LegRowNeed0").color;
                Assert.IsFalse(FindChild<Image>(s, "LegRowStrike0").enabled, "not previewing yet");

                SetPreview(s, true);
                RenderTicketColumn(s, ticket, resolvedThrough: 0, liveLegIndex: 0);

                Assert.IsTrue(FindChild<Image>(s, "LegRowStrike0").enabled,
                    "a remaining live leg must be struck while previewing — cashing out ends it");
                Assert.IsTrue(FindChild<Image>(s, "LegRowStrike1").enabled,
                    "a pending leg is equally ended by cashing out and must be struck too");

                Color liveInkAfter = FindChild<Text>(s, "LegRowNeed0").color;
                Assert.Less(liveInkAfter.a, liveInkBefore.a,
                    "the previewed row must drop one brightness level (L3 to L2)");
                Assert.Greater(liveInkAfter.a, 0f,
                    "the previewed row must NOT go to L0 — that is the LOST extinguish, and a leg " +
                    "being cancelled must never read as a leg lost while the player is deciding");
                AssertRgbApprox(liveInkBefore, liveInkAfter, 0.001f,
                    "a brightness step must not restate the hue — alpha only");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Releasing_the_preview_reverts_completely_with_no_residue()
        {
            // §8.10: "Release is a full revert. No partial state, no lingering strike-throughs, no
            // bank flicker." The implementation earns this by re-rendering from truth rather than
            // restoring a snapshot — this test is what pins that it actually does.
            var go = new GameObject("PreviewRevert");
            try
            {
                TvSweatScreen s = BuiltScreen(go);
                Ticket ticket = TwoLegTicket("3E-PREVIEW-REVERT");

                RenderTicketColumn(s, ticket, resolvedThrough: 0, liveLegIndex: 0);
                Color needBefore = FindChild<Text>(s, "LegRowNeed0").color;
                string needTextBefore = FindChild<Text>(s, "LegRowNeed0").text;
                Color lineBefore = FindChild<Text>(s, "LegRowLine1").color;

                SetPreview(s, true);
                RenderTicketColumn(s, ticket, resolvedThrough: 0, liveLegIndex: 0);
                SetPreview(s, false);
                RenderTicketColumn(s, ticket, resolvedThrough: 0, liveLegIndex: 0);

                Assert.IsFalse(FindChild<Image>(s, "LegRowStrike0").enabled,
                    "a strike survived the release — §8.10's 'no lingering strike-throughs'");
                Assert.IsFalse(FindChild<Image>(s, "LegRowStrike1").enabled,
                    "a pending row's strike survived the release");
                Assert.AreEqual(needBefore, FindChild<Text>(s, "LegRowNeed0").color,
                    "the live row's brightness did not return to L3 after release");
                Assert.AreEqual(needTextBefore, FindChild<Text>(s, "LegRowNeed0").text,
                    "the authored NEED statement changed across a preview round trip");
                Assert.AreEqual(lineBefore, FindChild<Text>(s, "LegRowLine1").color,
                    "the pending row's treatment did not revert");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
