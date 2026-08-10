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
        // T30: THE THRESHOLD PREDICATES ARE RETIRED. They approximated the retired constants and an
        // approximation is always wrong at some boundary — LooksLikeRetiredRed required g < 0.25f
        // while #FF4038's green channel is 0x40/255 = 0.25098, so it missed, by 0.00098, the exact
        // colour its own comment said it was "calibrated against". Three shipped guards asserted
        // less than they read as asserting, and nobody could see it from the code.
        //
        // The named constants are the law, so match them VERBATIM. A colour either is one of the
        // retired values or it is not; there is no boundary to be wrong at.
        private static Color FromHex(string hex) => new Color(
            System.Convert.ToInt32(hex.Substring(0, 2), 16) / 255f,
            System.Convert.ToInt32(hex.Substring(2, 2), 16) / 255f,
            System.Convert.ToInt32(hex.Substring(4, 2), 16) / 255f, 1f);

        /// <summary>True when <paramref name="c"/> IS one of the retired money constants. Epsilon is
        /// a float-representation tolerance only — not a similarity band.</summary>
        private static bool IsRetiredMoneyConstant(Color c)
        {
            foreach (string hex in new[] { RetiredGreenHex, RetiredRedHex })
            {
                Color r = FromHex(hex);
                if (Mathf.Abs(c.r - r.r) < 0.002f && Mathf.Abs(c.g - r.g) < 0.002f
                    && Mathf.Abs(c.b - r.b) < 0.002f) return true;
            }
            return false;
        }

        private static bool LooksLikeRetiredRed(Color c) => IsRetiredMoneyConstant(c);
        private static bool LooksLikeRetiredGreen(Color c) => IsRetiredMoneyConstant(c);
        // T9 (Phase 3B): calibrated against chromeCyan's literal (0.62, 0.86, 0.96) — blue and green
        // both bright, red held back. This is the previous palette's general-chrome cyan; it has no
        // role in DESIGN.md §4 (context is grey). §8's VOID leg state is the ONE place cyan survives,
        // and only the `chromeCyan` field itself is allowed to read this way — see the test below.
        private static bool LooksLikeRetiredCyan(Color c) => c.b > 0.7f && c.g > 0.6f && c.r < 0.75f;

        // The retired money language as literal constants. These were inline in the markup scan
        // below; they are hoisted here UNCHANGED — same three values, same order — because more than
        // one scan now needs to name them, and two scans each carrying a private copy of "the
        // retired colours" are two lists that eventually disagree. A palette law with two
        // definitions is the same class of blind spot TV-S3 was raised to close.
        private const string RetiredGreenHex = "3CE873"; // money-good green — DESIGN.md §4 retires it outright
        private const string RetiredRedHex = "FF4038";   // money-bad red — retired outright alongside green
        private const string RetiredCyanHex = "9EDCF6";  // the previous palette's general-chrome cyan

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
            string[] retiredHex = { RetiredGreenHex, RetiredRedHex, RetiredCyanHex };

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

        // ---------------------------------------------------------------------------------------
        // TV-S3 — the colour-field scan, widened from one type's public surface to the whole TV
        // surface. Everything above this line reads TvSweatScreen's PUBLIC fields or reads source
        // text. The helpers below exist so a single test can look everywhere else a Color can hide.
        // ---------------------------------------------------------------------------------------

        private const BindingFlags EveryDeclaredField =
            BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        /// <summary>TV-S3 (Design Director ruling, 2026-08-01): <b>the scan that kept missing this
        /// class is the finding, not the pixel.</b>
        ///
        /// <para>Three guards in this file already claim to enforce "no retired money colour", and
        /// all three were looking in the wrong place at once. <see cref="Retired_green_and_red_fields_no_longer_exist_on_the_type"/>
        /// and <see cref="No_public_colour_field_reads_as_the_retired_saturated_red_or_green"/>
        /// reflect over <c>TvSweatScreen</c>'s PUBLIC <see cref="Color"/> fields;
        /// <see cref="No_retired_money_colour_hides_in_rich_text_markup_in_owned_runtime_source"/>
        /// scans source for hex inside markup. The actual violation was three PRIVATE
        /// <see cref="Color"/> fields — <c>_green</c> #3CE873, <c>_red</c> #FF4038, <c>_cyan</c>
        /// #9EDCF6 — on <c>MomentumTape</c>, a DIFFERENT owned type on the same surface. Wrong
        /// visibility, wrong type, not markup: invisible to all three by construction, not by
        /// accident. Each guard passed while the retired palette was live on screen.</para>
        ///
        /// <para><b>How scope is decided.</b> Namespace alone cannot decide it: every owned runtime
        /// type is in <c>SBR.Game</c>, including <c>LaptopOs</c>, <c>PhoneScreen</c> and
        /// <c>SportsbookApp</c>, which are other surfaces with their own legitimate palettes and are
        /// not owned by this worktree (<c>LaptopOs.MoneyGood</c> is a live, sanctioned green over
        /// there). A hardcoded list of TV types cannot decide it either — a list is precisely the
        /// blind spot this test exists to close, and the next type added to the surface would not be
        /// on it. So the scope is DISCOVERED and the rule is reachability: <b>a type is on the TV
        /// surface if <see cref="TvSweatScreen"/> can reach it through declared fields</b>,
        /// transitively, staying inside <c>SBR.Game</c> in the owned runtime assembly. That picks up
        /// <c>MomentumTape</c>, <c>TheaterStage</c>, <c>TvLight</c>, the audio/choreography
        /// collaborators and every nested type, and it picks up anything wired in tomorrow with no
        /// edit here. The other surfaces fall out on their own: the TV holds no reference to any of
        /// them, which is the same fact that makes them a different surface.</para>
        ///
        /// <para><b>What is hardcoded, and why.</b> Exactly one thing: the four names in
        /// <c>mustBeFound</c>. That is not the scope — it is the anti-regression on the DISCOVERY.
        /// If someone stops holding <c>MomentumTape</c> in a field and builds it some other way, the
        /// reachability walk silently stops covering it and this test would quietly pass over the
        /// very type TV-S3 was raised about. The floor makes that shrinkage loud instead.</para>
        ///
        /// <para><b>What "retired" means here is not re-invented.</b> This reuses this file's own
        /// <see cref="LooksLikeRetiredGreen"/> / <see cref="LooksLikeRetiredRed"/> helpers unchanged
        /// — a second, divergent definition of the money law would be the same defect wearing a new
        /// hat. It then asks a second, strictly narrower question against the same file's own
        /// retired constants: is this field one of them VERBATIM? Both questions are needed, and the
        /// reason is uncomfortable: <c>LooksLikeRetiredRed</c> requires <c>g &lt; 0.25</c>, and
        /// #FF4038's green channel is 0x40/255 = 0.25098 — the heuristic misses the retired red it
        /// was calibrated against, by 0.00098. The identity check catches what the hair's-breadth
        /// costs us. Tightening the shared threshold instead would change what three shipped guards
        /// assert, which is a palette ruling and not this test's call to make — routed, not
        /// unilaterally fixed.</para>
        ///
        /// <para><b>HDR is not a violation.</b> <c>gold</c>, <c>goldL4</c> and <c>goldL2</c> are
        /// sanctioned money colours and the first two carry components above 1.0 on purpose
        /// (DESIGN.md §3's L4 tier). The heuristics already pass them — gold's green channel is far
        /// too high to read as red — and the identity check refuses to quantise anything above 1.0,
        /// because every retired constant is an LDR value and an HDR colour therefore cannot BE
        /// one.</para></summary>
        [Test]
        public void No_colour_field_anywhere_on_the_TV_surface_reads_as_the_retired_money_language()
        {
            var scratch = new List<Object>();
            var offenders = new List<string>();
            var unreadable = new List<string>();
            try
            {
                List<System.Type> surface = TvSurfaceTypes();

                // The floor on the discovery rule — see the summary. These are the types the audit
                // named; if the walk stops reaching one of them, the walk broke, not the palette.
                string[] mustBeFound = { "TvSweatScreen", "MomentumTape", "TheaterStage", "TvLight" };
                foreach (string required in mustBeFound)
                    Assert.IsTrue(surface.Any(candidate => candidate.Name == required),
                        $"the TV surface walk no longer reaches {required}. That is a failure of this " +
                        "SCAN, not a pass: reachability from TvSweatScreen is how the scope is decided, " +
                        "so a type that drops out of the walk drops out of every colour law this test " +
                        "enforces — silently, which is exactly how TV-S3's violation shipped.");

                foreach (System.Type t in surface)
                {
                    // Compiler-generated types are not palette. A coroutine's state machine
                    // (TvSweatScreen+<AnimateCashOut>d__NNN) hoists captured locals into fields, so a
                    // Color local inside an iterator surfaces here as an unconstructible "colour
                    // field". That is a local the SOURCE scan can see and this one cannot, and
                    // treating it as the scan going blind would keep this test permanently red for
                    // a shape it was never about.
                    if (t.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false)
                        || t.Name.IndexOf('<') >= 0)
                        continue;

                    List<FieldInfo> colourFields = t.GetFields(EveryDeclaredField)
                        .Where(field => field.FieldType == typeof(Color) || field.FieldType == typeof(Color32))
                        .ToList();
                    if (colourFields.Count == 0) continue;

                    // Static fields read off the type; instance fields need an object to read the
                    // declared defaults from. Only pay for an instance when something needs one.
                    object instance = null;
                    if (colourFields.Any(field => !field.IsStatic))
                    {
                        instance = DefaultsOnlyInstance(t, scratch);
                        if (instance == null)
                        {
                            unreadable.Add($"{t.FullName} ({colourFields.Count(field => !field.IsStatic)} instance " +
                                           "colour field(s)) — could not be constructed for a defaults read");
                            continue;
                        }
                    }

                    foreach (FieldInfo f in colourFields)
                    {
                        object raw = f.GetValue(f.IsStatic ? null : instance);
                        if (raw == null) continue;
                        Color c = f.FieldType == typeof(Color) ? (Color)raw : (Color)(Color32)raw;

                        string verdict = RetiredMoneyVerdict(c);
                        if (verdict != null)
                            offenders.Add($"{t.Name}.{f.Name} [{Visibility(f)} {f.FieldType.Name}] = " +
                                          $"({c.r:0.###}, {c.g:0.###}, {c.b:0.###}) — {verdict}");
                    }
                }

                // A type the scan could not read is not a type that passed. Reporting it as a
                // failure is the whole lesson of TV-S3: a sweep that quietly skips what it cannot
                // reach is a sweep that reports clean while the violation is on screen.
                Assert.IsEmpty(unreadable,
                    "these TV-surface types declare instance Color fields this scan could not read, " +
                    "because it could not build a defaults-only instance of them. Treat that as the " +
                    "scan going blind on the surface it exists to cover — teach DefaultsOnlyInstance " +
                    "how to build them rather than letting them fall out of the sweep:\n  " +
                    string.Join("\n  ", unreadable));

                Assert.IsEmpty(offenders,
                    "a retired money colour is live as a Color field on the TV surface. DESIGN.md §4 " +
                    "retires money-good green and money-bad red OUTRIGHT: gold is the only money " +
                    "colour and loss is darkness, so a field still carrying either hue is the old " +
                    "palette still speaking, through a name the palette retirement never looked at.\n" +
                    "Read the VISIBILITY and the OWNING TYPE below before assuming this is " +
                    "TvSweatScreen's problem. This scan exists because the three guards above it only " +
                    "ever read TvSweatScreen's public fields, while the real violation was three " +
                    "private fields on MomentumTape — the same surface, a different type, and no " +
                    "guard pointed at it.\n  " + string.Join("\n  ", offenders));
            }
            finally
            {
                foreach (Object o in scratch)
                    if (o != null) Object.DestroyImmediate(o);
            }
        }

        /// <summary>Discovers the TV surface instead of listing it: every <c>SBR.Game</c> type in the
        /// owned runtime assembly that <see cref="TvSweatScreen"/> can reach through declared fields,
        /// transitively, plus the nested types of everything reached. See the calling test's summary
        /// for why reachability is the scope rule and a name list is not.</summary>
        private static List<System.Type> TvSurfaceTypes()
        {
            Assembly owned = typeof(TvSweatScreen).Assembly;
            var found = new HashSet<System.Type>();
            var pending = new Queue<System.Type>();
            pending.Enqueue(typeof(TvSweatScreen));

            while (pending.Count > 0)
            {
                System.Type t = pending.Dequeue();
                if (t == null || t.Assembly != owned || t.Namespace != "SBR.Game") continue;
                if (!found.Add(t)) continue;

                // Nested types are part of the same surface and carry the same palette law — and
                // they are the single easiest place for a colour to sit unobserved.
                foreach (System.Type nested in t.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
                    pending.Enqueue(nested);

                foreach (FieldInfo f in t.GetFields(EveryDeclaredField))
                    foreach (System.Type reached in Reachable(f.FieldType))
                        pending.Enqueue(reached);
            }
            return found.ToList();
        }

        /// <summary>Arrays and generics hide their element types behind a wrapper the walk must not
        /// stop at: a <c>List&lt;Row&gt;</c> field reaches <c>Row</c>, and a <c>Dot[]</c> reaches
        /// <c>Dot</c>. Missing that would put whole types out of scope for no reason a reader could
        /// ever guess from the failure.</summary>
        private static IEnumerable<System.Type> Reachable(System.Type t)
        {
            if (t == null) yield break;
            if (t.IsArray)
            {
                foreach (System.Type element in Reachable(t.GetElementType())) yield return element;
                yield break;
            }
            if (t.IsGenericType)
                foreach (System.Type arg in t.GetGenericArguments())
                    foreach (System.Type element in Reachable(arg)) yield return element;
            yield return t;
        }

        /// <summary>Builds an instance carrying only DECLARED FIELD DEFAULTS — never a live one.
        /// Components go onto an inactive <see cref="GameObject"/> so <c>Awake</c>/<c>OnEnable</c>
        /// never fire, matching the pattern every other scan in this file uses. Returns null when the
        /// type cannot be built, and the caller reports that as a failure rather than a skip.</summary>
        private static object DefaultsOnlyInstance(System.Type t, List<Object> scratch)
        {
            if (t.IsAbstract || t.IsInterface || t.ContainsGenericParameters) return null;

            if (typeof(Component).IsAssignableFrom(t))
            {
                var go = new GameObject("TvS3_" + t.Name);
                go.SetActive(false); // field defaults only — never let Awake/OnEnable fire here
                scratch.Add(go);
                return go.AddComponent(t);
            }

            if (typeof(ScriptableObject).IsAssignableFrom(t))
            {
                ScriptableObject so = ScriptableObject.CreateInstance(t);
                scratch.Add(so);
                return so;
            }

            try { return System.Activator.CreateInstance(t, nonPublic: true); }
            catch { return null; }
        }

        /// <summary>Two questions against one vocabulary: does this colour READ as a retired money
        /// hue (this file's own heuristics, reused unchanged), and failing that, IS it one of this
        /// file's own retired constants verbatim? The second exists because #FF4038 misses
        /// <see cref="LooksLikeRetiredRed"/>'s green bound by 0.00098 — see the calling test's
        /// summary. Returns null when the colour is clean, otherwise the reason, phrased for someone
        /// reading a red test they did not write.</summary>
        private static string RetiredMoneyVerdict(Color c)
        {
            if (LooksLikeRetiredGreen(c)) return "reads as the retired money-good green (DESIGN.md §4 retires green outright)";
            if (LooksLikeRetiredRed(c)) return "reads as the retired money-bad red (DESIGN.md §4: loss is darkness, never red)";

            // HDR is sanctioned money, not a violation: DESIGN.md §3's L4 tier puts gold above 1.0 on
            // purpose. Every retired constant is an LDR value, so a colour with a component over 1.0
            // cannot BE one — and quantising it into range would manufacture a match that is not there.
            if (c.r > 1f || c.g > 1f || c.b > 1f) return null;

            string hex = Hex24(c);
            if (hex == RetiredGreenHex) return $"IS the retired money-good green #{RetiredGreenHex}, verbatim";
            if (hex == RetiredRedHex) return $"IS the retired money-bad red #{RetiredRedHex}, verbatim";
            return null;
        }

        private static string Hex24(Color c) =>
            $"{Mathf.RoundToInt(Mathf.Clamp01(c.r) * 255f):X2}" +
            $"{Mathf.RoundToInt(Mathf.Clamp01(c.g) * 255f):X2}" +
            $"{Mathf.RoundToInt(Mathf.Clamp01(c.b) * 255f):X2}";

        /// <summary>Names the visibility in the failure text on purpose. "private" in that line is
        /// the TV-S3 finding restated: the guard that was missing is the one that looks past
        /// <c>public</c>.</summary>
        private static string Visibility(FieldInfo f)
        {
            string access = f.IsPublic ? "public"
                : f.IsPrivate ? "private"
                : f.IsAssembly ? "internal"
                : "protected";
            return f.IsStatic ? access + " static" : access;
        }

        [Test]
        public void No_public_colour_field_reads_as_retired_general_chrome_cyan_except_the_documented_VOID_field()
        {
            // T9 (Phase 3B): chromeCyan used to be used broadly for leg/clock/records/chrome/slip-strip
            // labels — general chrome duty that cyan has no role for in §4. Every one of those call
            // sites now resolves to flavorColor/contextGrey/structureGrey instead. The single exception
            // is the VOID leg state, which DESIGN.md §8 still assigns a cyan — this scan asserts those
            // fields are the ONLY public colours allowed to read as cyan, rather than silently
            // permitting a reintroduction elsewhere under a different name.
            //
            // TV-20 added a SECOND exempt field. Canon's actual VOID token is `--tv-void` #7FB2C4
            // (`palette-tv.css:25`); `chromeCyan` #9EDBF5 was standing in for it and is markedly
            // brighter. `tvVoid` now carries the state and `chromeCyan` is kept only because its name
            // is serialized in `Room.unity`, a §11 forbidden file this worktree cannot rename.
            //
            // The exemption is by ROLE, not by one hardcoded name: both fields that legitimately
            // carry §8's VOID state are named here together, so the next colour added for that role
            // is an explicit decision rather than a test that quietly went red.
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
                    .Where(name => name != nameof(TvSweatScreen.chromeCyan)
                                && name != nameof(TvSweatScreen.tvVoid))
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
                    .Where(f => f.Name != nameof(TvSweatScreen.deadDark))  // documented exception 1
                    // Documented exception 2 — TV-03, and a genuine canon-vs-DESIGN.md conflict
                    // routed to the DD rather than silently resolved either way.
                    //
                    // Canon names `--tv-gold-ink` #0A0C10 = (0.039, 0.047, 0.063), which is darker
                    // than the floor on all three channels. It is exempt because it is not a PANEL
                    // colour: it is type punched out of a solid L4 gold field, so it is a hole in a
                    // lit shape, not a dark region of the panel. The floor exists so nothing undoes
                    // the room's emissive-quad lift — ink surrounded by the brightest element on
                    // the surface does not.
                    //
                    // Stated as a conflict, not a preference: C14 says 1:1 unless physically
                    // impossible, canon gives an exact hex, and DESIGN.md's floor forbids it. If the
                    // DD rules the floor wins, raise the token here and record the deviation.
                    .Where(f => f.Name != nameof(TvSweatScreen.goldInk))
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
        /// <para><b>Widened to six for T63 (DD batch 13, "ungated and proceeds as built" batch 14).</b>
        /// `CashOutField` joins. This gate caught the change and said "route it before editing" — so
        /// here is the routing, at the edit, rather than in a note somewhere else.
        ///
        /// <para><b>Why the ruling cannot be met without it.</b> T63 requires the actionable band to
        /// render above the quiet scoreline. In C33's ruled unit the scoreline's cold white is 0.942
        /// Rec.709. Rec.709 weights green at 0.7152, and gold is a low-blue, sub-maximal-green
        /// colour — so within the 0..1 range that the clamped default material allows, <b>no gold
        /// can out-rank cold white at all</b>: reaching 0.942 requires G ~ 1.0, which is lemon, not
        /// gold. The requirement is unsatisfiable on the default material. The field must have the
        /// HDR path or T63 cannot be built.</para>
        ///
        /// <para><b>Why this is not the widening this gate exists to stop.</b> The band is a
        /// compound element — the slot is "three things, not one: an actionable FIELD, the money
        /// figure, and a status word." `CashOut` and `CashOutField` are two graphics of ONE
        /// sanctioned occupant, and they share a single material INSTANCE, so they cannot be
        /// independently boosted: one token moves both or neither. That is asserted directly below,
        /// which is what keeps the sixth name from being a real sixth occupant. The occupant count
        /// is unchanged at five.</para>
        ///
        /// <para>This is also the reason the previous wiring was wrong: the material sat on the
        /// figure alone, so `RequestL4(HdrFocus.CashOut)` boosted a number and left the field it
        /// sits on at rest — measured, field 0.696 against figure 0.827 in the same zone.</para></summary>
        private static readonly string[] SanctionedL4Elements =
            // "GoldFlood" REMOVED batch 27. This gate asks that a change here be routed before it is
            // made; T40's enforcement IS that routing — the element is struck, so a name for it here
            // would assert an eligibility that has nowhere to land. Dropping a name is the other
            // thing this gate watches for, and the reason it watches is "an element that must reach
            // L4 silently fell back to the clamped default material". That is not this: there is no
            // element.
            { "CashOut", "CashOutField", "BigAmount", "Score", "Ball" };

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
        /// <summary>The production constant, read rather than guessed. T49 moved the L4 boost from
        /// 1.8 to 1.4 and this helper's old <c>&gt; 1.5f</c> threshold — calibrated to 1.8 — silently
        /// stopped detecting L4 at all, failing both one-token tests while the production code was
        /// correct. That is T30's lesson exactly: an approximation is always wrong at some boundary,
        /// and a ruling eventually walks the value past it. Comparing to the real constant cannot
        /// go stale, whatever the DD rules next.</summary>
        private static float ConstBoost(string name) => (float)typeof(TvSweatScreen)
            .GetField(name, BindingFlags.NonPublic | BindingFlags.Static).GetRawConstantValue();

        /// <summary>How many GRAPHICS one focus lights. The invariant is over FOCUSES, not
        /// materials — a visual moment can legitimately be more than one graphic, and C3 corrected
        /// exactly this confusion once already ("how many things are lit" is not "how many things
        /// decided to be lit").
        ///
        /// <para>`CashOut` drives three since batch 19: the slot's figure, the slot's field (T63 —
        /// the inversion needs both halves), and the gold flood. The flood joined when T68-am moved
        /// both payoff figures into the slot: the tally and its wash are still one visual moment,
        /// they simply belong to this focus now rather than to `Payout`. `Payout` keeps its own
        /// mapping although nothing currently requests it — see the note on `_tBigAmount`.</para></summary>
        private static int MaterialsDrivenBy(string focus)
        {
            // Both counts dropped by one at batch 27, for the same reason: the gold flood rode each
            // of these focuses at some point and is now struck. CashOut is the slot's figure and its
            // field; Payout is BigAmount alone.
            switch (focus)
            {
                case "CashOut": return 2;
                default: return 1;
            }
        }

        private static int MaterialsAtL4(TvSweatScreen s)
        {
            // `_cashOutFieldHdrMat` ADDED batch 19, and its absence was a real blind spot: T63 gave
            // the actionable FIELD its own material and this counter never saw it, so from T63 until
            // now the one-token instrument could not observe the element that batch 16's blocker was
            // actually about. A counter with a hard-coded list silently stops covering whatever is
            // added next — the same shape as C33-am2 and C35, one level down.
            string[] mats = { "_cashOutHdrMat", "_cashOutFieldHdrMat", "_bigAmountHdrMat",
                              "_scoreHdrMat", "_ballHdrMat" };
            int boostId = Shader.PropertyToID("_HdrBoost");
            float l4 = ConstBoost("HdrBoostL4");
            int n = 0;
            foreach (string f in mats)
            {
                var m = (Material)typeof(TvSweatScreen)
                    .GetField(f, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(s);
                // Verbatim match on the constant, not a band. Epsilon is float representation only.
                if (m != null && Mathf.Abs(m.GetFloat(boostId) - l4) < 0.0005f) n++;
            }
            return n;
        }

        /// <summary>True when the cash-out HDR material actually built. <c>MakeHdrMaterial</c>
        /// returns null when <c>Shader.Find("SBR/TvSweatHdrUI")</c> misses, and the L4 release is
        /// guarded on it — so a test that asserts on the token without checking this asserts
        /// nothing (C18: a check states what it cannot see).</summary>
        private static void SetPrivateBool(TvSweatScreen s, string field, bool value)
        {
            FieldInfo f = typeof(TvSweatScreen).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(f, $"TvSweatScreen.{field} not found by reflection — was it renamed?");
            f.SetValue(s, value);
        }

        private static bool HasHdrMaterial(TvSweatScreen s)
            => (Material)typeof(TvSweatScreen)
                .GetField("_cashOutHdrMat", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(s) != null;

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
                Assert.AreEqual(MaterialsDrivenBy("CashOut"), MaterialsAtL4(s));

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
                    int expected = MaterialsDrivenBy(L4Holder(s));
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

                // T63: the list went from five names to six, and this is what stops that being a
                // sixth OCCUPANT. CashOut and CashOutField are two graphics of one compound element
                // and must share a single material INSTANCE, so one token moves both or neither and
                // they can never be independently at L4. Without this, the widening above would be
                // exactly the thing the gate exists to catch.
                Graphic figure = null, fieldG = null;
                foreach (Graphic g in go.GetComponentsInChildren<Graphic>(true))
                {
                    if (g.gameObject.name == "CashOut") figure = g;
                    else if (g.gameObject.name == "CashOutField") fieldG = g;
                }
                Assert.IsNotNull(figure, "CashOut figure not found — canvas layout changed?");
                Assert.IsNotNull(fieldG, "CashOutField not found — canvas layout changed?");
                // They must be SEPARATE instances. Sharing one between a Text and an Image does not
                // survive uGUI batching — measured: the field rendered nothing across all 8 frames
                // of a capture that had shown it solid. What makes them one occupant is not one
                // material, it is one TOKEN: ApplyBoost's CashOut case drives both, asserted below.
                Assert.AreNotSame(figure.material, fieldG.material,
                    "T63: the band's figure and field must have SEPARATE material instances. One "
                    + "shared instance makes the Image sample the font atlas and render nothing.");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        // RENAMED batch 14, and the rename IS the fix. The old name —
        // `L4_canvas_elements_get_the_hdr_material_and_L3_elements_stay_default` — claimed the L4/L3
        // assignment was correct. It never checked that. It checks which elements carry the HDR
        // MATERIAL, i.e. which are eligible to exceed 1.0, and it was green throughout the period
        // when the event strip sat at the L4 tier VALUE (alpha 1.0, measured 0.858 Rec.709 against a
        // 0.866 scoreline) without the material — a state this test asserts as proof of the opposite.
        //
        // Recorded studio-wide as a blind-spot class: A NAME CLAIMING MORE THAN ITS TEST. The gate
        // read green, the name read like a guarantee, and nobody had to lie for the two to diverge.
        //
        // Gate V1 on the owning document — one L4 token, verified by a per-frame ladder scan in
        // Rec.709 composited luma — SUPERSEDES this check. This is now a wiring test and says so.
        [Test]
        public void Hdr_material_is_wired_only_to_elements_eligible_to_exceed_1_0()
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
                Text flavor = FindChild<Text>(screen, "Flavor");

                Assert.IsNotNull(cashOut, "CashOut text not found — canvas layout changed?");
                Assert.IsNotNull(bigAmount, "BigAmount text not found — canvas layout changed?");
                Assert.IsNotNull(flavor, "Flavor text not found — canvas layout changed?");

                Assert.AreEqual("SBR/TvSweatHdrUI", cashOut.material.shader.name,
                    "the cash-out band must be able to reach L4 (§8.5 Actionable)");
                Assert.AreEqual("SBR/TvSweatHdrUI", bigAmount.material.shader.name,
                    "the big win/cash-out amount must be able to reach L4 (§3: the payoff at its callback)");

                // Flavor text must not have opted into the HDR material, so it can never EXCEED 1.0.
                //
                // What this assertion does NOT establish, corrected batch 14: that the strip is
                // below L4. Lacking the material caps it at 1.0; it does not put it under 1.0, and
                // for the whole life of this test the strip sat AT alpha 1.0 — the L4 tier value —
                // while this line passed. "Only one L4 element at a time" was never in evidence
                // here. The tier is now enforced at TvSweatScreen.SetEventStrip and measured by V1.
                Assert.AreNotEqual("SBR/TvSweatHdrUI", flavor.material.shader.name,
                    "routine beat text must stay on the default (LDR) UI material, so it can never "
                    + "exceed 1.0. This does not by itself place it below L4 — V1 does that.");

                // T40 ENFORCED (batch 27). These three assertions used to require the two floods to
                // EXIST and to be HDR-capable. They are INVERTED rather than deleted, per T17's
                // precedent — the check that guarded a thing's colour now guards its absence, so a
                // full-screen wash cannot come back quietly the way `_wonFlood` did after T40 ruled
                // it deleted in batch 5 and it stayed in the tree for four weeks.
                //
                // Any full-screen `MakeStretchImage(root, …)` created after the zones washes every
                // fact on the surface, which is what the frame showed at flood peak. `DimOverlay` is
                // the one such element that stays, by name and by ruling — a dim is not a wash.
                foreach (string wash in new[] { "GoldFlood", "WonFlood", "GreenFlood" })
                    Assert.IsNull(FindChild<Image>(screen, wash),
                        $"T40: `{wash}` is a full-screen gold wash and is struck — deleted, not "
                        + "z-ordered and not dimmed (C10). At its peak every fact on the surface "
                        + "renders gold, so the money signal means nothing on the one beat it "
                        + "matters most. If a payoff needs punctuation, §6.1's brief L4 punch is "
                        + "measured already providing it.");

                Assert.IsNotNull(FindChild<Image>(screen, "DimOverlay"),
                    "DimOverlay is NOT struck — a dim is not a wash and T40 does not reach it. "
                    + "Asserted so the flood removal cannot quietly take a third element with it.");
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
                // TV-20: the VOID token is canon's `--tv-void` #7FB2C4 now, not chromeCyan #9EDBF5 —
                // the old value was markedly brighter and lighter than the token it stood in for.
                AssertRgbApprox(s.tvVoid, voidLine.color, 0.001f, "VOID row");
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
                // TV-14 changed what `Line` IS: it was the whole compact row, and is now just the
                // statement span, with price and state chip beside it. The strike still spans the
                // whole ROW, which is the correct referent — DESIGN.md §8 strikes the LEG ("struck
                // through on the matrix"), not the words. So it must be WIDER than the statement,
                // and it must still be fixed rather than measured from glyphs, which is what the
                // invariance checks below actually prove.
                Assert.Greater(strike.rectTransform.sizeDelta.x, line.rectTransform.sizeDelta.x,
                    "the strike must span the whole row, not just the statement span — a rule that " +
                    "stopped at the end of the words would strike the fact and leave the price and " +
                    "the state chip unstruck, which says the leg is only partly void");

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

        // ---------------------------------------------------------------------------------------
        // T62 — the ledger moves, and EVERY surface mirroring it repaints on that frame.
        //
        // DD 2026-08-05, found on this slice's own T58 proof frames: the live leg's progress line
        // printed the pre-goal score for a whole beat while the scoreline above printed the goal.
        // Same revealed value, same frame, two readings — correcting 51 match-minutes later.
        //
        // INSTRUMENT SCOPE, stated because it is weaker than it looks (C25): this is a SOURCE scan,
        // not a rendered assertion. It pins that the ledger-advance site repaints both mirrors; it
        // cannot prove what the two elements actually displayed. The rendered pin needs a PlayMode
        // goal and is owed at the next editor slot, alongside T63's measurement. A source scan is the
        // established idiom here (T30's retired-hue scan works the same way) and it catches exactly
        // the regression that occurred: half a repaint at a ledger advance.
        // ---------------------------------------------------------------------------------------

        [Test]
        public void T62_advancing_the_ledger_repaints_every_mirror_of_it()
        {
            string path = Path.Combine(Application.dataPath, "SBR", "Runtime", "TvSweatScreen.cs");
            Assert.IsTrue(File.Exists(path), $"TvSweatScreen.cs not found at {path}");
            string[] lines = File.ReadAllLines(path);

            var advances = new List<int>();
            for (int i = 0; i < lines.Length; i++)
                if (lines[i].Contains("_ledger.CompleteGoal(")) advances.Add(i);

            Assert.IsNotEmpty(advances,
                "no _ledger.CompleteGoal call found — this scan just stopped covering anything. "
                + "If the ledger advance was renamed, re-point this test at the new name rather than "
                + "deleting it (C29's shape: a check that inspects nothing is not a pass).");

            foreach (int i in advances)
            {
                // The repaint must follow within the same short block. Deliberately narrow: the whole
                // defect was that the column's repaint lived a beat away, in another method.
                bool repainted = false;
                for (int j = i; j < System.Math.Min(i + 12, lines.Length); j++)
                    if (lines[j].Contains("RepaintRevealedScore(")) { repainted = true; break; }

                Assert.IsTrue(repainted,
                    $"T62: TvSweatScreen.cs:{i + 1} advances the revealed score with "
                    + "_ledger.CompleteGoal but does not call RepaintRevealedScore within the same "
                    + "block. Repainting only the scorebug there is the original defect: the live "
                    + "leg row reads the SAME _ledger.Picked/Opponent and would print the pre-goal "
                    + "score until the next beat's RenderEvent.");
            }
        }

        [Test]
        public void T62_the_repaint_helper_drives_both_mirrors()
        {
            // Guards the other half: the helper must actually touch BOTH surfaces. A helper that
            // quietly lost its column call would satisfy the scan above and restore the defect.
            string path = Path.Combine(Application.dataPath, "SBR", "Runtime", "TvSweatScreen.cs");
            string[] lines = File.ReadAllLines(path);
            int start = System.Array.FindIndex(lines, l => l.Contains("private void RepaintRevealedScore("));
            Assert.Greater(start, -1, "RepaintRevealedScore not found — was it renamed?");

            string body = string.Join("\n", lines.Skip(start).Take(8));
            Assert.IsTrue(body.Contains("UpdateScorebug("),
                "RepaintRevealedScore must repaint the scorebug");
            Assert.IsTrue(body.Contains("UpdateTicketColumn("),
                "RepaintRevealedScore must repaint the ticket column — that omission IS T62");
        }

        // ---------------------------------------------------------------------------------------
        // T58 — the goal flash is a BRIGHTNESS event, never a hue event.
        //
        // DD 2026-08-04: the flash overlay was gold, measured 56-58° at up to 67% saturation on the
        // scoreline's peak pixel against 204-205° at ~5% at rest. Gold is rationed to money and a
        // goal is not money. It also re-created T41's defect in a second channel — the scoreline read
        // 0.72 at the flash while the actionable cash-out band read 0.62.
        // ---------------------------------------------------------------------------------------

        [Test]
        public void T58_the_goal_flash_carries_no_hue_of_its_own()
        {
            var go = new GameObject("tv");
            try
            {
                TvSweatScreen s = BuiltScreen(go);
                Text flash = FindChild<Text>(s, "Score");
                Text matchup = FindChild<Text>(s, "Matchup");
                Assert.IsNotNull(flash, "the Score punch overlay is missing");
                Assert.IsNotNull(matchup, "the persistent Matchup scoreline is missing");

                Assert.AreEqual(matchup.color, flash.color,
                    "T58: the punch must be the SAME cold white as the scoreline it overlays. Any "
                    + "other colour makes the flash a hue event; identical colour makes it a pure "
                    + "brightness event by construction, because superimposing a value on itself and "
                    + "boosting can only brighten.");

                // And it is specifically not gold — stated separately so the failure names the defect
                // rather than just reporting two colours that differ.
                Assert.AreNotEqual(new Color(s.gold.r, s.gold.g, s.gold.b, 1f), flash.color,
                    "T58: the goal flash is gold. Gold is rationed to money — won legs, payout "
                    + "figures, the cash-out band. A goal is the event that may PRODUCE money, which "
                    + "is exactly the distinction the rationing rule exists to hold.");

                // The rest position must genuinely be cold: blue-ish channel not below red, which is
                // what separates §4's cold white from any warm cast. Not a similarity band — a
                // direction check on the axis the ruling measured (hue 204-205° at rest vs 56-58°).
                Assert.GreaterOrEqual(flash.color.b, flash.color.r,
                    "T58: the flash reads warm (red channel above blue), which is the gold direction");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        // ---------------------------------------------------------------------------------------
        // T59 — the slot's state IS the input's state.
        //
        // DD 2026-08-04, answering the question T43 routed up rather than guessing: "a player who
        // presses E during suspension receives a cash-out they were just told was unavailable, at a
        // price the display is not showing."
        // ---------------------------------------------------------------------------------------

        [Test]
        public void T59_a_suspended_slot_refuses_the_key()
        {
            var go = new GameObject("tv");
            try
            {
                TvSweatScreen s = BuiltScreen(go);
                MethodInfo gate = typeof(TvSweatScreen).GetMethod(
                    "CanAcceptCashOutNow", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsNotNull(gate, "CanAcceptCashOutNow not found — was it renamed?");

                // Drive the presentation into the suspended slate, then ask the INPUT gate. Before
                // T59 these were separate values and this returned true on the pending-loss path,
                // where _marketSuspended is false because ResolveBeat never suspends.
                InvokePrivate(s, "ShowMarketSuspended");
                Assert.IsFalse((bool)gate.Invoke(s, null),
                    "T59: the slot reads MARKET SUSPENDED while the accept gate still says yes. "
                    + "Display state and input state must be one value.");

                // TVS-H01's contract: the stand-suppression predicate reads the same gate, so the two
                // cannot drift apart. Asserted here because T59 is exactly the kind of change that
                // would break it silently.
                MethodInfo live = typeof(TvSweatScreen).GetMethod(
                    "CashOutLive", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsNotNull(live, "CashOutLive not found — was it renamed?");
                Assert.AreEqual(gate.Invoke(s, null), live.Invoke(s, null),
                    "TVS-H01: CashOutLive and CanAcceptCashOutNow must agree exactly");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        // ---------------------------------------------------------------------------------------
        // T42 — team hues are muted and confined to the pitch dots.
        //
        // DD 2026-08-02: "team names at luminance 0.87-0.92, full chroma; hues must be muted,
        // brightness-secondary, confined to pitch dots." The scorebug half landed with T32.1
        // (4293baa, "the scoreline goes cold"); this is the half that survived it — the dots.
        //
        // Canon mirrored as constants with its path cited, per the handoff's §4A rule: a C# test
        // cannot import a CSS token, and inventing a "muted enough" threshold is exactly the T30
        // mistake (an approximation is always wrong at some boundary). A colour either IS the canon
        // value or it is not.
        // ---------------------------------------------------------------------------------------

        // main-2/docs/design/design-system/tokens/palette-tv.css:22-23
        private const string CanonTeamAHex = "5C7BA8"; // --tv-team-a, muted blue
        private const string CanonTeamBHex = "B2739E"; // --tv-team-b, muted pink

        [Test]
        public void T42_the_only_team_hues_are_canons_two_muted_ones()
        {
            var go = new GameObject("tv");
            try
            {
                TvSweatScreen s = BuiltScreen(go);

                Assert.AreEqual(FromHex(CanonTeamAHex), s.teamHueA,
                    "teamHueA must be --tv-team-a (#5C7BA8) verbatim");
                Assert.AreEqual(FromHex(CanonTeamBHex), s.teamHueB,
                    "teamHueB must be --tv-team-b (#B2739E) verbatim");

                // The five saturated pool entries the surface used to draw dots with. Two of them
                // (orange, violet) are not in the TV palette at all. None may reappear as a TV
                // colour field. TheaterPalette itself is deliberately NOT asserted on: the laptop
                // (SportsbookApp.cs, another worktree) still draws from that pool, and this suite
                // has no authority over its palette.
                string[] retiredPool = { "3D7BFF", "E84DD0", "FF8A2B", "9B5CF6" };
                foreach (FieldInfo f in typeof(TvSweatScreen)
                    .GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (f.FieldType != typeof(Color)) continue;
                    var c = (Color)f.GetValue(s);
                    foreach (string hex in retiredPool)
                        Assert.AreNotEqual(FromHex(hex), c,
                            $"TvSweatScreen.{f.Name} carries retired saturated team hue #{hex}; "
                            + "T42 confines team hue to the two muted pitch-dot tokens");
                }
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        // ---------------------------------------------------------------------------------------
        // T43 — MARKET SUSPENDED is L1 unlit slate from its FIRST frame.
        //
        // DD 2026-08-02: "suspended renders on solid gold before dimming a frame later; dim state
        // exists and works — transition ordering bug."
        //
        // These tests are deliberately shaped to catch ORDERING, which is why neither of them lets
        // Update() run before asserting. The old build was correct one frame later, so any test that
        // ticked a frame first would have passed against the defect and certified the lie. The
        // instrument has to read the surface on the transition frame itself, because that is the
        // frame the Design Director photographed.
        // ---------------------------------------------------------------------------------------

        [Test]
        public void T43_suspending_dims_the_whole_slot_on_the_same_frame_as_the_label()
        {
            var go = new GameObject("tv");
            try
            {
                TvSweatScreen s = BuiltScreen(go);
                Text cashOut = FindChild<Text>(s, "CashOut");
                Image field = FindChild<Image>(s, "CashOutField");
                Text status = FindChild<Text>(s, "CashOutStatus");
                Assert.IsNotNull(field, "CashOutField missing — TV-03's actionable field is the thing under test");
                Assert.IsNotNull(status, "CashOutStatus missing — TV-04 split the status word out of the figure");

                // The live, actionable slot: gold figure, lit field, HOLD E, holding the L4 token.
                // Set by hand rather than by driving a session — the transition is what is under
                // test, and it must clean up whatever state preceded it.
                cashOut.enabled = true;
                cashOut.color = s.gold;
                cashOut.text = "CASH OUT $183";
                field.enabled = true;
                status.enabled = true;
                status.text = "HOLD E";
                // C25 — this instrument's scope, stated with it: the L4 leg of the assertion below
                // is only meaningful when the HDR material actually built. MakeHdrMaterial returns
                // null if Shader.Find misses, and ApplyCashOutSlotState's release is guarded on the
                // material, so without it there is no token to release and the check would assert
                // nothing. It is skipped explicitly rather than silently passing.
                bool hdrBuilt = HasHdrMaterial(s);
                if (hdrBuilt)
                {
                    RequestL4(s, "CashOut", false);
                    Assert.AreEqual("CashOut", L4Holder(s), "test setup failed to seat the L4 token");
                }

                InvokePrivate(s, "ShowMarketSuspended"); // the transition, and NOT a frame more

                Assert.AreEqual("MARKET SUSPENDED", cashOut.text, "the label did not change");
                Assert.AreEqual(s.structureGrey, cashOut.color,
                    "§8.5: suspended is L1 unlit slate — structureGrey, on the label's own frame");
                Assert.IsFalse(field.enabled,
                    "T43: the gold field survived the suspension. This is the defect the DD measured — "
                    + "MARKET SUSPENDED rendered on solid gold because the field was only re-derived "
                    + "in Update(), one frame later.");
                Assert.IsFalse(status.enabled,
                    "TV-12/13: MARKET SUSPENDED owns the slot exclusively — HOLD E must not sit beside "
                    + "it promising input the accept gate refuses");
                if (hdrBuilt)
                    Assert.AreNotEqual("CashOut", L4Holder(s),
                        "C3/§8.5: a suspended slot must not hold the surface's only L4 token — "
                        + "brightness is a promise about input");
                else
                    Debug.Log("[T43] HDR material absent (Shader.Find missed) — the L4 leg is "
                        + "unmeasurable in this run. The field and status legs above did run.");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void T43_a_tweening_price_never_lights_the_field_or_takes_the_L4_token()
        {
            // Regression guard for a defect T43's own fix introduced, found in diff review before it
            // ran. Moving the slot's derivation out of Update means RenderCashOut reaches it — and
            // one RenderCashOut runs synchronously inside StartCoroutine, BEFORE the handle lands in
            // _cashOutAnimation. CanAcceptCashOutNow reads that handle, so mid-tween it answered
            // "acceptable" for exactly one frame and lit the gold field at L4 during a price update.
            //
            // TVS-H02 is the same quirk one element over, and _cashOutTweening is the flag written
            // for it. This test drives the flag directly rather than a real coroutine: the coroutine
            // is what makes the window hard to observe, and the invariant does not depend on it —
            // while a tween is in flight the field is dark, whatever the handle currently says.
            var go = new GameObject("tv");
            try
            {
                TvSweatScreen s = BuiltScreen(go);
                Text cashOut = FindChild<Text>(s, "CashOut");
                Image field = FindChild<Image>(s, "CashOutField");

                cashOut.enabled = true;
                field.enabled = true;
                SetPrivateBool(s, "_cashOutTweening", true);

                InvokePrivate(s, "ApplyCashOutSlotState");

                Assert.IsFalse(field.enabled,
                    "§8.5: the gold field is a promise the key works right now, and it does not while "
                    + "the price is settling — CanAcceptCashOutNow refuses a mid-tween accept. Reading "
                    + "the Coroutine handle instead of _cashOutTweening reopens TVS-H02 here.");
                Assert.AreNotEqual("CashOut", L4Holder(s),
                    "C3: a settling price must not hold the surface's only L4 token");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void T43_the_gold_taunt_cannot_repaint_a_suspended_slot()
        {
            var go = new GameObject("tv");
            try
            {
                TvSweatScreen s = BuiltScreen(go);
                Text cashOut = FindChild<Text>(s, "CashOut");
                Image field = FindChild<Image>(s, "CashOutField");

                // §8.7's pending-loss window renders the suspended slate while the MARKET is still
                // open — ResolveBeat never calls SuspendMarket, so _marketSuspended stays false and
                // this is a genuine reachable state, not a contrived one. The per-frame taunt was
                // gated on _marketSuspended alone, so it repainted the words MARKET SUSPENDED in
                // full-brightness gold for the entire window. Left unfixed, the slot's own animator
                // undoes the slate every frame and the first test above would still pass.
                InvokePrivate(s, "ShowMarketSuspended");
                Assert.AreEqual(s.structureGrey, cashOut.color, "precondition: the slate was not applied");

                InvokePrivate(s, "AnimateCashOutTaunt");

                Assert.AreEqual(s.structureGrey, cashOut.color,
                    "T43: the per-frame gold taunt repainted a suspended slot. The market is not "
                    + "suspended in a pending-loss window, so a _marketSuspended-only guard does not "
                    + "hold — the slot's own presentation state is what gates it.");
                Assert.AreEqual("MARKET SUSPENDED", cashOut.text, "the taunt changed the label");
                Assert.IsFalse(field.enabled, "the taunt re-lit the field under a suspended label");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        // ------------------------------------------------------------------ T64 / T65

        // ------------------------------------------------------------------ batch 16
        //
        // SCOPE, stated once for the four below (C25): three are SOURCE scans and one is colour
        // arithmetic. They pin the wiring and the values, not the pixels. The rendered pin for T68
        // is a seated capture of the actionable band, and it is owed at the next capture — the
        // defect T68 names was invisible to every suite this surface has precisely because no
        // instrument compared an element to its own ink.

        [Test]
        public void T68am_the_accepted_figure_renders_in_the_slot_with_the_inversion()
        {
            // §6.1's accepted state, built. The figure used to sit on a canvas-centre element over a
            // SINE-PULSING flood: gold-on-flood runs 12.47:1 at alpha 0 down to 1.71:1 at the 0.55
            // peak, and dark ink inverts that to 1.08:1 for most of the beat. Neither static ink is
            // right because the ground MOVES. The slot gives it a stable field.
            var go = new GameObject("T68amAccepted");
            go.SetActive(false);
            try
            {
                var screen = go.AddComponent<TvSweatScreen>();
                screen.theaterEnabled = false;
                InvokePrivate(screen, "Awake");

                Text figure = FindChild<Text>(screen, "CashOut");
                Text status = FindChild<Text>(screen, "CashOutStatus");
                Image field = FindChild<Image>(screen, "CashOutField");
                Assert.IsNotNull(figure); Assert.IsNotNull(status); Assert.IsNotNull(field);

                typeof(TvSweatScreen).GetMethod("ShowCashOutAccepted",
                    BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(screen, new object[] { "CASHED OUT $199" });

                Assert.IsTrue(field.enabled,
                    "T68-am: the accepted state lights the slot's field — that is the stable ground "
                    + "the figure is legible against, and the reason the flood does not have to be.");
                Assert.AreEqual(screen.goldInk, figure.color,
                    "T68-am: the figure is punched out of the field, the same inversion T68 built "
                    + "and measured at 7.95:1 (9.68:1 computed at this state's L3).");
                Assert.IsFalse(status.enabled,
                    "T68-am: no status word. The OFFER is over — T43's 'nothing of the offer "
                    + "outlives the accept' is about the price and the HOLD E instruction, not the "
                    + "slot rectangle, which §6.1 gives six states.");
                Assert.AreEqual("CASHED OUT $199", figure.text);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void T71_both_payoff_moments_take_one_treatment()
        {
            // The reason to rule them together is the reason T68 exists: two payoff moments drifting
            // apart in treatment is the class of drift that produced a money control with an
            // unreadable label. One state got measured, its sibling did not, and they drifted.
            string src = File.ReadAllText(Path.Combine(Application.dataPath, "SBR", "Runtime", "TvSweatScreen.cs"));

            foreach (string beat in new[] { "IEnumerator CashOutFloodBeat", "IEnumerator WinBeat" })
            {
                int at = src.IndexOf(beat, System.StringComparison.Ordinal);
                Assert.Greater(at, -1, $"{beat} not found — re-point this scan rather than deleting it.");
                string body = string.Join("\n",
                    src.Substring(at, System.Math.Min(2000, src.Length - at))
                       .Split('\n').Where(l => !l.TrimStart().StartsWith("//")));

                Assert.IsTrue(body.Contains("ShowCashOutAccepted("),
                    $"T71: {beat} must render its figure in the slot. Splitting the two payoff "
                    + "moments re-creates exactly the divergence T68 was.");
                Assert.IsFalse(body.Contains("_tBigAmount.text = $\"") || body.Contains("_tBigAmount.text = \"+$0\""),
                    $"T71: {beat} still writes a money figure to the canvas-centre element, which "
                    + "puts it back on the pulsing flood.");
            }
        }

        [Test]
        public void C35_the_accept_beat_owns_its_token_so_the_punch_can_settle()
        {
            // C34[C35]: an element and its ground must not move together. The corollary here is that
            // the per-frame derivation must NOT re-assert the token while accepted — it would either
            // cancel §6.1's brief punch on the next Update or hold it for the whole beat, and V8's
            // new clause asks whether the ground is static across the beat. It is only static if one
            // owner drives it.
            // Searched over the WHOLE source, not a fixed window from the method head. The first
            // version used a 4000-char window and the method's comments had grown past it — the
            // second time a fixed scan window has silently stopped covering its target (batch 16
            // used 500 and hit the same wall). The literal below appears in code and nowhere else.
            string src = File.ReadAllText(Path.Combine(Application.dataPath, "SBR", "Runtime", "TvSweatScreen.cs"));
            Assert.Greater(src.IndexOf("private void ApplyCashOutSlotState", System.StringComparison.Ordinal), -1,
                "ApplyCashOutSlotState not found — re-point this scan rather than deleting it.");

            Assert.IsTrue(src.Contains("_cashOutHdrMat != null && !accepted"),
                "C35: while accepted, the BEAT owns the L4 token. If this derivation re-asserts it "
                + "every frame, the brief punch either never settles or never happens.");
            Assert.IsTrue(src.Contains("IEnumerator PunchThenSettle"),
                "C35/§6.1: the punch-then-L3 arc is its own coroutine, run detached so it cannot "
                + "re-pace the beat it decorates.");
        }

        [Test]
        public void G1_the_two_at_budget_forms_fit_their_measured_columns()
        {
            // The DD authored these against ~13.6px/char at 28px and ~7.3px at 15px — planning
            // figures extrapolated from two strings — and said explicitly: FitToColumn is the
            // authority, not the character counts; measure the two that sit at the budget line and
            // take the authored fallback if either misses. This is that measurement.
            var go = new GameObject("G1Budget");
            go.SetActive(false);
            try
            {
                var screen = go.AddComponent<TvSweatScreen>();
                screen.theaterEnabled = false;
                InvokePrivate(screen, "Awake");

                Text need = FindChild<Text>(screen, "LegRowNeed0");
                Text line = FindChild<Text>(screen, "LegRowLine0");
                Assert.IsNotNull(need, "LegRowNeed0 not found — row builder changed?");
                Assert.IsNotNull(line, "LegRowLine0 not found — row builder changed?");

                MethodInfo fits = typeof(TvSweatScreen).GetMethod("Fits",
                    BindingFlags.NonPublic | BindingFlags.Static);
                Assert.IsNotNull(fits, "Fits not found by reflection — was it renamed?");
                bool Fit(Text t, string s) => (bool)fits.Invoke(null, new object[] { t, s });

                // Reported either way, because a miss here is not a failure — it is the signal to
                // ship the authored fallback, and the DD pre-committed both.
                bool scoreless = Fit(need, "ONE TEAM SCORELESS");
                bool corners = Fit(line, "UNDER 10.5 CORNERS");
                // Reported, not merely asserted: the DD asked WHICH of the two lands, because the
                // answer decides whether the fallback ships. A gate that only says "one of them
                // worked" answers a question nobody asked.
                TestContext.WriteLine($"G1 MEASURED  NEED col {need.rectTransform.rect.width:0.0}px: "
                    + $"'ONE TEAM SCORELESS' {(scoreless ? "FITS" : "MISSES -> ONE TEAM BLANKED")}");
                TestContext.WriteLine($"G1 MEASURED  compact col {line.rectTransform.rect.width:0.0}px: "
                    + $"'UNDER 10.5 CORNERS' {(corners ? "FITS" : "MISSES -> UNDER 10.5 CNRS")}");
                foreach (string s in new[] { "MIDDLEMEN ML", "LANYARD TO SCORE", "BOTH TEAMS SCORE", "NOT YET" })
                    TestContext.WriteLine($"G1 MEASURED  '{s}': NEED {(Fit(need, s) ? "fits" : "MISSES")}, "
                        + $"compact {(Fit(line, s) ? "fits" : "MISSES")}");
                Assert.IsTrue(scoreless || Fit(need, "ONE TEAM BLANKED"),
                    "G1: neither `ONE TEAM SCORELESS` nor its authored fallback `ONE TEAM BLANKED` "
                    + "fits the 249px NEED column. Both forms are constants — if both miss, the "
                    + "budget itself is wrong and the deck needs a third line, not a truncation.");
                Assert.IsTrue(corners || Fit(line, "UNDER 10.5 CNRS"),
                    "G1: neither `UNDER 10.5 CORNERS` nor its last-resort `UNDER 10.5 CNRS` fits the "
                    + "143px compact column.");
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void G1_the_compact_statement_carries_identity_and_never_the_fixture()
        {
            // The compact form states WHICH BET THIS IS. The fixture is dropped entirely — the
            // scorebug carries who is playing whom and the BACKED marker carries the side, which is
            // what makes 143px workable. A re-introduced `v {other}` is the T69 defect returning.
            string src = File.ReadAllText(Path.Combine(Application.dataPath, "SBR", "Runtime", "TvSweatScreen.cs"));
            int at = src.IndexOf("private string LegStatement(", System.StringComparison.Ordinal);
            Assert.Greater(at, -1, "LegStatement not found — re-point this scan rather than deleting it.");
            int end = src.IndexOf("private static string FitOrFallback", System.StringComparison.Ordinal);
            Assert.Greater(end, at, "LegStatement's end marker not found.");
            string body = src.Substring(at, end - at);

            Assert.IsFalse(body.Contains("ML · v") || body.Contains(" v {"),
                "G1: the compact statement must not name the fixture. The scorebug already carries "
                + "it; restating it here is what overran the 143px column in the first place.");
            foreach (string form in new[] { "ML\"", "GOALS\"", "BTTS YES", "BTTS NO", "CORNERS\"", "CARDS\"", "ANYTIME" })
                Assert.IsTrue(body.Contains(form), $"G1: the authored compact form `{form}` is missing.");
        }

        [Test]
        public void G1_the_scorer_pair_names_its_player_exactly_once()
        {
            // The pair-defect the DD found while authoring the top of it: NEED said
            // `LANYARD TO SCORE` and the progress line said `WAITING FOR LANYARD` — T69's
            // "a fact named twice" reproduced vertically instead of horizontally.
            string src = File.ReadAllText(Path.Combine(Application.dataPath, "SBR", "Runtime", "SweatActiveLegModel.cs"));
            int at = src.IndexOf("DescribeAnytimeScorer(ActiveLegInput", System.StringComparison.Ordinal);
            Assert.Greater(at, -1, "DescribeAnytimeScorer not found — re-point this scan.");
            // CODE ONLY. The first version of this scan matched the retired string inside the very
            // comment that records why it was retired — a scan that fails on its own documentation.
            string body = string.Join("\n",
                src.Substring(at, System.Math.Min(1400, src.Length - at))
                   .Split('\n')
                   .Where(l => !l.TrimStart().StartsWith("//")));

            Assert.IsFalse(body.Contains("WAITING FOR"),
                "G1: the scorer progress line must be `NOT YET` / `SCORED`. `WAITING FOR {SURNAME}` "
                + "names the player a second time, three lines under the NEED that already named him.");
            Assert.IsTrue(body.Contains("\"NOT YET\"") && body.Contains("\"SCORED\""),
                "G1: the scorer progress line is `NOT YET` (unscored) and `SCORED` (resolved).");
            // and NEED carries the surname, not the full name
            Assert.IsTrue(body.Contains("Surname(l.BackedPlayerName)"),
                "G1: NEED names the player by surname — the convention the progress line already used.");
        }

        [Test]
        public void T68_the_slot_ink_has_one_authority_and_the_taunt_yields_to_it()
        {
            // The defect was not the value, it was that the value had five authors. The field
            // inverted and the type did not, because they were set in different places.
            string path = Path.Combine(Application.dataPath, "SBR", "Runtime", "TvSweatScreen.cs");
            string[] lines = File.ReadAllLines(path);

            var sites = new List<int>();
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].TrimStart().StartsWith("//")) continue;
                if (lines[i].Contains("_tCashOut.color") && lines[i].Contains("=")) sites.Add(i);
            }
            Assert.AreEqual(2, sites.Count,
                "T68: the cash-out amount's ink must be written in exactly two places — the one "
                + "derivation in ApplyCashOutSlotState, and the per-frame taunt that yields to it. "
                + $"Found {sites.Count}. A third author is how the field and the type came apart.");

            bool taunt = false;
            foreach (int i in sites)
            {
                // the taunt's assignment must sit under a guard naming the lit field
                for (int j = i; j > System.Math.Max(0, i - 10); j--)
                    if (lines[j].Contains("!_cashOutFieldLit")) { taunt = true; break; }
            }
            Assert.IsTrue(taunt,
                "T68: the per-frame taunt repaints the amount gold. On a lit (inverted) field that "
                + "undoes the punch-out on the next Update — the same repaint T43 caught once "
                + "already. It must be gated on !_cashOutFieldLit.");
        }

        [Test]
        public void T68_the_punched_ink_clears_a_legibility_threshold_against_its_own_field()
        {
            // C33-am2's companion gate: a dominance instrument is silent on whether the dominant
            // element can be READ. This is the check that would have caught T68 two batches ago.
            //
            // SCOPE: this is a WCAG-style ratio on linear relative luminance of the AUTHORED
            // colours. It deliberately does not try to reproduce the ruling's 15.3:1, which was
            // measured on a rendered frame through the grade — the convention behind that exact
            // figure is not derivable from the batch text, and inventing one to match it would be
            // the instrument laundering a number it cannot compute.
            var go = new GameObject("T68Contrast");
            go.SetActive(false);
            try
            {
                var s = go.AddComponent<TvSweatScreen>();
                float field = Contrast(s.gold, s.goldInk);
                Assert.Greater(field, 4.5f,
                    $"T68: the actionable field against its punched ink is {field:F2}:1. The label "
                    + "is the confirm-gesture copy T22/T36 ruled — if it does not clear the field, "
                    + "the money control has no readable instruction on it.");

                // and the defect itself, as a regression pin: light ink on the lit field
                float broken = Contrast(s.gold, new Color(s.gold.r, s.gold.g, s.gold.b, 1f));
                Assert.Less(broken, 1.1f,
                    "sanity: gold ink on a gold field is the ~1:1 case T68 measured. If this "
                    + "assertion starts failing, this test's colour model has drifted, not the build.");
            }
            finally { Object.DestroyImmediate(go); }
        }

        /// <summary>WCAG contrast ratio on linear relative luminance. Clamped because this
        /// surface's palette is HDR and authored values legitimately exceed 1.0.</summary>
        private static float Contrast(Color a, Color b)
        {
            float la = RelLum(a), lb = RelLum(b);
            float hi = Mathf.Max(la, lb), lo = Mathf.Min(la, lb);
            return (hi + 0.05f) / (lo + 0.05f);
        }

        private static float RelLum(Color c)
        {
            float L(float v)
            {
                v = Mathf.Clamp01(v);
                return v <= 0.04045f ? v / 12.92f : Mathf.Pow((v + 0.055f) / 1.055f, 2.4f);
            }
            return 0.2126f * L(c.r) + 0.7152f * L(c.g) + 0.0722f * L(c.b);
        }

        [Test]
        public void T69_the_row_statement_is_re_authored_against_its_column()
        {
            // The engine's DisplayLabel prints the backed team twice on a Moneyline and overruns
            // the column. It is read and re-authored on this surface, never changed at the source.
            string path = Path.Combine(Application.dataPath, "SBR", "Runtime", "TvSweatScreen.cs");
            string src = File.ReadAllText(path);

            Assert.IsTrue(src.Contains("FitToColumn(_legRow[i].Line, LegStatement(leg))"),
                "T69: the ticket row must take its statement through LegStatement (which names the "
                + "backed side once) and FitToColumn (which truncates on a word boundary against "
                + "the measured column). Assigning leg.DisplayLabel raw is the defect.");

            int at = src.IndexOf("private string LegStatement(", System.StringComparison.Ordinal);
            Assert.Greater(at, -1, "LegStatement not found — re-point this scan rather than deleting it.");
            string body = src.Substring(at, System.Math.Min(2200, src.Length - at));
            // G1 superseded T69's interim form: the compact statement states IDENTITY, and the
            // fixture is dropped entirely because the scorebug already carries it. `{CLUB} ML`.
            Assert.IsTrue(body.Contains("{club} ML"),
                "T69/G1: the Moneyline compact form is `{CLUB} ML` — the identity, named once, with "
                + "no fixture. The `ML · v {OTHER}` form was the interim step, not the destination.");
        }

        [Test]
        public void T67_the_strip_text_starts_past_the_measured_bloom_reach()
        {
            string path = Path.Combine(Application.dataPath, "SBR", "Runtime", "TvSweatScreen.cs");
            string src = File.ReadAllText(path);
            Assert.IsTrue(src.Contains("const float StripBloomInset = 40f;"),
                "T67: the strip's text zone begins 40px past the band boundary (canvas x 305-980). "
                + "Measured reach was +0.181 mean over the first 20px and 0.000 from x=365. This is "
                + "the structural answer to a CENTRED line, which only clears the halo while short.");
            Assert.IsTrue(src.Contains("new Rect(StripBloomInset, 0f,"),
                "T67: the inset must move the TEXT rect. Insetting the zone ground instead would "
                + "shrink the strip's panel, which is not what was ruled.");
        }

        [Test]
        public void The_event_strip_has_exactly_one_painting_point()
        {
            // Batch 14: "the event strip goes L2 ... one rule." Enforcing that as one rule rather
            // than as fourteen correct call sites is the whole point — TV-S1 already tiered this
            // element once and missed eleven sites, and a convention each new beat has to remember
            // is what let that happen. `_tFlavor.color` may be assigned in SetEventStrip and nowhere
            // else; a new beat physically cannot choose its own tier.
            string path = Path.Combine(Application.dataPath, "SBR", "Runtime", "TvSweatScreen.cs");
            string[] lines = File.ReadAllLines(path);
            int helper = System.Array.FindIndex(lines, l => l.Contains("private void SetEventStrip("));
            Assert.Greater(helper, -1,
                "SetEventStrip not found — re-point this scan rather than deleting it (C29's shape).");

            int assignments = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].TrimStart().StartsWith("//")) continue;
                if (!lines[i].Contains("_tFlavor.color")) continue;
                assignments++;
                Assert.IsTrue(i > helper && i < helper + 6,
                    $"TvSweatScreen.cs:{i + 1} paints the event strip outside SetEventStrip. The "
                    + "strip has one tier (L2) and one painting point; hue stays the caller's, the "
                    + "tier does not.");
            }
            Assert.AreEqual(1, assignments,
                "expected exactly one _tFlavor.color assignment (inside SetEventStrip) — if the "
                + "helper was restructured, re-point this scan rather than loosening it.");

            // The double-tier trap: the helper applies L2 itself, so a call site that hands it
            // already-tiered ink lands at 0.4 x 0.4 = 0.16. Three sites carried AtTier(..., TierL2)
            // before batch 14 and had to be unwound to raw ink; this is what stops one coming back.
            int calls = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].TrimStart().StartsWith("//")) continue;
                if (!lines[i].Contains("SetEventStrip(")) continue;
                if (i == helper) continue;              // the declaration itself
                calls++;
                Assert.IsFalse(lines[i].Contains("AtTier("),
                    $"TvSweatScreen.cs:{i + 1} passes pre-tiered ink to SetEventStrip, which "
                    + "applies L2 again — the strip lands at 0.16, not 0.40. Pass the raw ink.");
            }
            Assert.Greater(calls, 0, "no SetEventStrip call sites found — this scan stopped covering anything.");
        }

        [Test]
        public void The_event_strip_is_painted_at_L2()
        {
            // The other half: the single painting point must actually apply L2. A helper that
            // routed every site and then painted at full alpha would satisfy the scan above and
            // restore the exact defect — which is the shape C29 names, one level in.
            var go = new GameObject("EventStripTier");
            go.SetActive(false);
            try
            {
                var screen = go.AddComponent<TvSweatScreen>();
                screen.theaterEnabled = false;
                InvokePrivate(screen, "Awake");

                Text flavor = FindChild<Text>(screen, "Flavor");
                Assert.IsNotNull(flavor, "Flavor text not found — canvas layout changed?");

                MethodInfo set = typeof(TvSweatScreen).GetMethod("SetEventStrip",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsNotNull(set, "SetEventStrip not found by reflection — was it renamed?");

                set.Invoke(screen, new object[] { screen.flavorColor });
                Assert.AreEqual(screen.flavorColor.a * 0.4f, flavor.color.a, 1e-4f,
                    "the event strip must paint at L2 (alpha x 0.40). At raw alpha it measured "
                    + "0.858 Rec.709 against a 0.866 scoreline — not separated from the score at "
                    + "all, on a surface whose first law is that brightness is the semantic channel.");

                // Hue is the caller's and must survive the tier untouched.
                Assert.AreEqual(screen.flavorColor.r, flavor.color.r, 1e-4f, "the tier changed the ink");
                Assert.AreEqual(screen.flavorColor.g, flavor.color.g, 1e-4f, "the tier changed the ink");
                Assert.AreEqual(screen.flavorColor.b, flavor.color.b, 1e-4f, "the tier changed the ink");

                // The double-tier trap is a CALL-SITE defect and this test cannot see it — calling
                // the helper directly bypasses the call sites entirely. It is checked by the source
                // scan above instead. Saying so here rather than adding an assertion that looks
                // like coverage and is not: that is the blind-spot class batch 14 recorded off this
                // very file, and repeating it one test down would be a poor way to honour it.
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void T63_the_actionable_field_is_the_element_that_carries_L4()
        {
            // The defect this pins is structural and would have failed before the fix: the HDR
            // material was on the money FIGURE only, so RequestL4(HdrFocus.CashOut) boosted a
            // number and left the gold field it sits on at rest — a granted token that changed
            // nothing where the eye actually reads the band. Measured in Rec.709 (C33's unit) on
            // the current set: field 0.696, figure 0.827, quiet scoreline 0.866, ball 0.902.
            var go = new GameObject("T63Field");
            go.SetActive(false); // defer Awake so BuildCanvas runs once, under our control
            try
            {
                var screen = go.AddComponent<TvSweatScreen>();
                screen.theaterEnabled = false;
                InvokePrivate(screen, "Awake");

                Image field = FindChild<Image>(screen, "CashOutField");
                Text figure = FindChild<Text>(screen, "CashOut");
                Assert.IsNotNull(field, "CashOutField not found — canvas layout changed?");
                Assert.IsNotNull(figure, "CashOut figure not found — canvas layout changed?");

                Assert.AreEqual("SBR/TvSweatHdrUI", field.material.shader.name,
                    "T63: the actionable field carries no HDR material, so it cannot be boosted to "
                    + "L4 at all. The field IS the actionable state — boosting only the figure "
                    + "moves the number and leaves the band at rest.");
                Assert.AreNotSame(figure.material, field.material,
                    "T63: separate instances, not one shared — a Text and an Image sharing one "
                    + "material instance makes the Image render nothing under uGUI batching.");

                // What makes them ONE occupant is one token driving both. Pinned at the source,
                // because an EditMode test cannot see a material's boost reach a rendered pixel.
                string src = File.ReadAllText(Path.Combine(Application.dataPath, "SBR", "Runtime", "TvSweatScreen.cs"));
                int caseAt = src.IndexOf("case HdrFocus.CashOut:", System.StringComparison.Ordinal);
                Assert.Greater(caseAt, -1, "ApplyBoost's CashOut case not found — was it renamed?");
                int breakAt = src.IndexOf("break;", caseAt, System.StringComparison.Ordinal);
                Assert.Greater(breakAt, caseAt, "ApplyBoost's CashOut case has no break; — parse failed.");
                string body = src.Substring(caseAt, breakAt - caseAt);
                Assert.IsTrue(body.Contains("_cashOutHdrMat") && body.Contains("_cashOutFieldHdrMat"),
                    "T63: ApplyBoost's CashOut case must drive BOTH the figure's and the field's "
                    + "material. Driving one is the original defect: the token is granted and the "
                    + "band does not change where the eye reads it.");

                // The field and the figure carry the SAME ink. `goldL4` was tried and reverted: a
                // vertex colour is packed to Color32, so goldL4 clamps to (255,255,74) — hue 60 deg
                // lemon — and a full-width field that bright blooms the whole panel (measured: band,
                // event strip and risk/pays all read hue 60.0 at ~61% sat). The band's brightness
                // must come from the boost, not from a hotter authored colour.
                Assert.AreEqual(figure.color.r, field.color.r, 1e-4f, "T63: field and figure differ in ink");
                Assert.AreEqual(figure.color.g, field.color.g, 1e-4f, "T63: field and figure differ in ink");
                Assert.AreEqual(figure.color.b, field.color.b, 1e-4f, "T63: field and figure differ in ink");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void C33_the_L4_gold_outranks_the_L3_gold_in_the_ruled_unit()
        {
            // The canary under T63's fix. Calling a colour "the L4 gold" is worth nothing unless it
            // is actually brighter, and brightness is now a ruled unit: Rec.709 luma on
            // display-encoded values (C33). RGB-average — the superseded unit — under-reports
            // saturated warm colour against neutral, which is exactly where this surface keeps its
            // money, and that mis-ranking is what produced a reported 0.21 gap where the real one
            // was 0.047. A future palette edit that dims goldL4 must fail here, in this unit.
            var go = new GameObject("C33Unit");
            go.SetActive(false); // only the authored defaults are under test — no canvas needed
            try
            {
                var screen = go.AddComponent<TvSweatScreen>();
                float l3 = Rec709(screen.gold);
                float l4 = Rec709(screen.goldL4);
                Assert.Greater(l4, l3,
                    $"C33/T63: goldL4 Rec.709 luma {l4:F3} does not outrank gold's {l3:F3}. The "
                    + "surface's only sustained L4 element is painted in goldL4, so if it is not "
                    + "the brighter value the ladder is inverted at its top tier.");
                Assert.Greater(l4 - l3, 0.01f,
                    $"C33/T63: goldL4 and gold differ by {l4 - l3:F3} Rec.709, which is under this "
                    + "instrument's ~0.01 resolution (8-bit display-encoded, one code value is "
                    + "~0.004). A tier separation that cannot be resolved is not a tier.");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>C33's ruled unit: Rec.709 luma on display-encoded values. Quoted with every
        /// ladder number, studio-wide. Mirrors tools/ladder_read.py's `rec709`.</summary>
        private static float Rec709(Color c) => 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;

        [Test]
        public void T64_no_surface_of_this_TV_carries_an_idle_flicker()
        {
            // Behavioural, not a source scan: two Updates with no Flash in between must leave the
            // wired Light at EXACTLY the same intensity. A flicker of any amplitude fails this, and
            // it fails without needing to know how the flicker was spelled.
            var go = new GameObject("tvlight-flickertest");
            try
            {
                Light point = go.AddComponent<Light>();
                TvLight light = go.AddComponent<TvLight>();
                light.pointLight = point;

                typeof(TvLight).GetMethod("Awake",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .Invoke(light, null);
                var update = typeof(TvLight).GetMethod("Update",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                update.Invoke(light, null);
                float first = point.intensity;
                for (int i = 0; i < 8; i++)
                {
                    update.Invoke(light, null);
                    Assert.AreEqual(first, point.intensity,
                        "T64: the idle room spill moved between frames with nothing driving it. "
                        + "A display that works does not flicker, this surface has exactly one "
                        + "pulse kind and it is LIVE, and an effect with no fire condition is "
                        + "continuous involuntary motion. Removed, not zeroed — if a flicker dial "
                        + "came back, delete it rather than setting it to 0.");
                }
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void T64_the_flicker_dials_are_gone_not_zeroed()
        {
            // The companion to the behavioural test, and it guards the thing that test cannot see:
            // a dial at 0.0 passes the behavioural check while a stale scene can still carry a
            // non-zero serialized override (batch 13 recorded exactly that trap from the room lane).
            // A field that does not exist cannot be overridden.
            foreach (string field in new[] { "idleEmissionFlicker" })
                Assert.IsNull(typeof(TvSweatScreen).GetField(field),
                    $"T64: TvSweatScreen.{field} is back. It must be REMOVED, not zeroed — a "
                    + "serialized scene value survives a changed default.");
            foreach (string field in new[] { "flickerAmp", "flickerHz" })
                Assert.IsNull(typeof(TvLight).GetField(field),
                    $"T64: TvLight.{field} is back. Same rule, and this is the channel that "
                    + "reaches the player as ROOM light rather than as panel pixels.");
        }

        [Test]
        public void T65_a_leg_win_never_re_tints_the_room()
        {
            // The named defect: WonLegBeat fired tvLight.Flash(gold, 3.0f), taking the room to
            // hue 40.7deg at 71.1% saturation and roughly double the luma. A leg win is not a
            // payoff — there are three or four per ticket.
            string path = Path.Combine(Application.dataPath, "SBR", "Runtime", "TvSweatScreen.cs");
            string[] lines = File.ReadAllLines(path);
            int start = System.Array.FindIndex(lines, l => l.Contains("IEnumerator WonLegBeat("));
            Assert.Greater(start, -1,
                "WonLegBeat not found — re-point this scan rather than deleting it (C29's shape: "
                + "a check that inspects nothing is not a pass).");

            for (int i = start; i < lines.Length; i++)
            {
                string l = lines[i];
                if (i > start && l.Contains("IEnumerator ")) break;   // next beat, stop
                if (l.TrimStart().StartsWith("//")) continue;          // the record of the fix is prose
                Assert.IsFalse(l.Contains("tvLight") || l.Contains("EmissionFlash("),
                    $"T65: TvSweatScreen.cs:{i + 1} re-tints a room-facing channel from inside "
                    + "WonLegBeat. The room re-tint fires on SETTLEMENT, never on a leg, and it "
                    + "goes through RoomSettlementGlow() so no site chooses its own colour. The "
                    + "leg's win is carried where the ration already carries it: the row goes gold.");
            }
        }

        [Test]
        public void T65_the_room_light_is_never_handed_the_surfaces_money_gold()
        {
            // The rule, not the site. Gold is rationed to the PANEL; the room light is a separate
            // instrument bound to the room's palette. Any tvLight call that names a colour inline
            // is how the ration leaks back out, whichever beat it sits in.
            string path = Path.Combine(Application.dataPath, "SBR", "Runtime", "TvSweatScreen.cs");
            string[] lines = File.ReadAllLines(path);
            int seen = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string l = lines[i];
                if (l.TrimStart().StartsWith("//")) continue;
                if (!l.Contains("tvLight?.Flash(") && !l.Contains("tvLight.Flash(")) continue;
                seen++;
                Assert.IsFalse(l.Contains("gold") || l.Contains("new Color("),
                    $"T65: TvSweatScreen.cs:{i + 1} flashes the ROOM light with a colour named at "
                    + "the call site. Route it through RoomSettlementGlow(), which carries the one "
                    + "palette-bound value. Gold does not leave the panel.");
            }
            Assert.Greater(seen, 0, "no tvLight Flash call found — this scan stopped covering anything.");
        }

        [Test]
        public void T65_the_room_re_tint_sits_inside_the_rooms_own_warm_band()
        {
            // The value, pinned where it is decided. The room's warm key is ~92deg and the laptop
            // lid's sanctioned contribution 85.1-85.3deg; a saturated 40deg amber is a new hue, not
            // a warming. This asserts the LIGHT's hue, which is the thing this file controls.
            //
            // SCOPE (C25): the ruling bounds the room's measured CAST, not the light's own hue, and
            // the cast also depends on amplitude — it runs monotonically from ~130deg at zero to
            // ~45.5deg as amplitude rises, crossing the band once over roughly [0.78, 1.06]. This
            // test cannot see that. Gate V6 on rendered frames is what settles it
            // (tools/v6_room_region.py); this only stops the value drifting back toward gold.
            var go = new GameObject("tv-retint-value");
            go.SetActive(false); // only the authored defaults are under test — no canvas needed
            try
            {
                TvSweatScreen s = go.AddComponent<TvSweatScreen>();
                Color.RGBToHSV(s.roomSettlementWarm, out float h, out float sat, out _);
                float deg = h * 360f;
                Assert.GreaterOrEqual(deg, 85f, $"T65: room re-tint hue {deg:F1}deg is below the room's band.");
                Assert.LessOrEqual(deg, 92f, $"T65: room re-tint hue {deg:F1}deg is above the room's band.");

                Color.RGBToHSV(s.gold, out float gh, out _, out _);
                Assert.AreNotEqual(Mathf.Round(gh * 360f), Mathf.Round(deg),
                    "T65: the room re-tint is back on the money hue. Gold stays on the panel.");
                Assert.Less(sat, 0.45f,
                    $"T65: room re-tint saturation {sat * 100f:F0}% — the defect measured 71% and "
                    + "the room's own resting saturation is ~40%. A re-tint is a warming, not a "
                    + "colour event.");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
