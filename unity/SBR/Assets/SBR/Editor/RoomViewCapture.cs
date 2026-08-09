using System;
using System.Collections.Generic;
using System.IO;
using SBR.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;   // Volume - the R23 diagnostic disables the grade for one frame

namespace SBR
{
    /// <summary>
    /// DISPOSABLE capture harness for the room visual pass - delete this file once the pass
    /// is signed off. Reproduces the exact three gate views recorded in
    /// artifacts/room-visual-pass/baseline/room-recon.md so before/after comparisons use
    /// identical camera poses rather than hand-placed scene cameras.
    ///
    /// Renders the live PlayerCamera in real Play Mode after a short warm-up so the TV and
    /// laptop world-space UI have initialised, exactly as the baseline captures did.
    ///
    /// Run (note: NO -quit and NO -nographics - the harness exits the editor itself, and
    /// post-processing needs a graphics device):
    ///   Unity.exe -batchmode -projectPath (project)
    ///             -executeMethod SBR.RoomViewCapture.CaptureAll -outDir (absolute path)
    /// </summary>
    public static class RoomViewCapture
    {
        private const string ScenePath = "Assets/Scenes/Room.unity";
        private const string ArmedKey = "SBR.RoomViewCapture.Armed";
        private const string GlowCueArmedKey = "SBR.RoomViewCapture.GlowCueArmed";
        private const string BlurArmedKey = "SBR.RoomViewCapture.BlurArmed";
        private const string OutDirKey = "SBR.RoomViewCapture.OutDir";
        private const int Width = 2560;
        private const int Height = 1440;
        private const int WarmupFrames = 8;

        private static int _frames;
        private static int _glowFrames;
        private static int _blurFrames;

        /// <summary>
        /// Edit-mode capture. No Play Mode, so no domain reload - this runs to completion in a
        /// single -executeMethod call and works reliably in batch, unlike CaptureAll below.
        /// Lights, emissive screen materials and TvLight all exist in edit mode, so this is
        /// valid evidence for lighting and material work. What it does NOT show is live TV and
        /// laptop UI content, which only the runtime scripts populate - use CaptureAll for
        /// readability evidence.
        ///
        ///   Unity.exe -batchmode -quit -projectPath (project)
        ///             -executeMethod SBR.RoomViewCapture.CaptureEditMode -outDir (path)
        /// </summary>
        public static void CaptureEditMode()
        {
            string outDir = OutDirFromArgs();
            Directory.CreateDirectory(outDir);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Capture(outDir);
        }

        /// <summary>
        /// R23 - the screens-dark conformance set, the instrument for law §1.1.
        ///
        /// §1.1 says a blue-tinted room is the explicit failure mode, and it has been reporting
        /// ITSELF as failing because it was being judged on gameplay captures - frames in which
        /// three emissive screens and a green TV light are pouring colour into the room. That
        /// cannot separate "the room is cool" from "the screens are cool", so the law was
        /// unfalsifiable on its own evidence.
        ///
        /// This set removes the screens from the measurement entirely: emission forced black on
        /// all three panels, and the two screen-driven lights disabled. What remains is the
        /// room's own cast under its own rig at its own grade - the grade included deliberately,
        /// because the grade IS the room and not a layer over it.
        ///
        /// EDIT MODE ON PURPOSE. The Play Mode harness exists to show live screen content, which
        /// is precisely what this set must not contain, so the domain-reload dance buys nothing
        /// here and costs reliability in batch.
        ///
        /// TWO frames, and the second is not padding. The ruling names the seated rig AND
        /// requires wall, floor and bunk regions to be reported; a 17-degree close-up on a dark
        /// panel cannot contain three surfaces. Both requirements are only satisfiable together.
        ///
        ///   Unity.exe -batchmode -quit -projectPath (project)
        ///             -executeMethod SBR.RoomViewCapture.CaptureConformance -outDir (path)
        /// </summary>
        public static void CaptureConformance()
        {
            string outDir = OutDirFromArgs();
            Directory.CreateDirectory(outDir);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            int darkened = DarkenScreens();
            Debug.Log($"[RoomViewCapture] R23 conformance: {darkened} screen emitters silenced");

            Camera cam = FindPlayerCamera();
            var controller = UnityEngine.Object.FindAnyObjectByType<FirstPersonController>();
            if (controller != null)
                controller.enabled = false;

            // R26 - the set is captured TWICE, graded then grade-bypassed, same rig, same
            // framing, same regions, same instrument. Only the grade differs between the two
            // passes, which is what makes the pair an isolation rather than two pictures: any
            // difference between them is attributable to the grade and to nothing else.
            //
            // The graded pass stays canonical - R23 is explicit that the grade IS the room and
            // not a layer over it, so law 1.1 is judged on the graded frames. The bypassed pass
            // exists to answer the question the graded frames cannot: if the room reads cool,
            // is that the room's LIGHT or the room's GRADE?
            var volGo = GameObject.Find("RoomPostFx");
            var vol = volGo != null ? volGo.GetComponent<Volume>() : null;
            if (vol == null)
                throw new InvalidOperationException(
                    "RoomPostFx volume not found - the grade-bypassed pass cannot be isolated, " +
                    "and a set missing half its pair would silently look complete");

            ShootConformanceSet(cam, outDir, "");
            vol.enabled = false;
            ShootConformanceSet(cam, outDir, "-UNGRADED");
            vol.enabled = true;
        }

        /// <summary>
        /// Both poses of the conformance set, suffixed so the graded and grade-bypassed passes
        /// land side by side under identical names. Identical framing between passes is the
        /// whole point - if the poses drifted, the pair would no longer isolate anything.
        ///
        /// The seated rig is the one the ruling names. The wide frame is the one that can
        /// actually carry wall, floor and bunk in a single shot, which the ruling also requires;
        /// a 17-degree close-up on a dark panel cannot hold three surfaces, so both are needed
        /// to satisfy both halves of the same sentence. The region instrument measures the wide
        /// frame, because that is where the ruled regions live.
        /// </summary>
        private static void ShootConformanceSet(Camera cam, string outDir, string suffix)
        {
            var seatedEye = new Vector3(-0.950f, 1.150f, 0.300f);
            var tvCenter = new Vector3(1.232f, 1.100f, 0.300f);
            Shoot(cam, outDir, $"conformance-seated-screens-dark{suffix}.png",
                  seatedEye, Quaternion.LookRotation(tvCenter - seatedEye, Vector3.up), 17f);

            Shoot(cam, outDir, $"conformance-room-screens-dark{suffix}.png",
                  new Vector3(0.300f, 1.640f, -1.400f),
                  Quaternion.LookRotation(Vector3.forward, Vector3.up), 68f);
        }

        /// <summary>
        /// Forces every screen emitter to contribute nothing, without touching a single asset.
        ///
        /// A MaterialPropertyBlock overrides the emission colour per RENDERER, so the shared
        /// material assets on disk are untouched and the next ordinary build is unaffected. The
        /// alternative - editing the materials - would silently corrupt the room for every other
        /// capture, and the emission flag it would disturb is the one that already broke this
        /// project once (see Mat() in GrayboxRoomBuilder).
        ///
        /// The two screen-driven lights go too. TvLight is green and PhoneBuzzLight is a flash;
        /// both are screen colour arriving by another route, and R23's whole point is that no
        /// screen's colour enters the room's measured cast.
        /// </summary>
        private static int DarkenScreens()
        {
            int n = 0;
            var block = new MaterialPropertyBlock();

            foreach (MeshRenderer mr in UnityEngine.Object
                         .FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include))
            {
                if (mr.name != "TVScreen" && mr.name != "LaptopScreen" && mr.name != "PhoneScreen")
                    continue;
                mr.GetPropertyBlock(block);
                block.SetColor("_EmissionColor", Color.black);
                block.SetColor("_BaseColor", new Color(0.010f, 0.010f, 0.012f, 1f));
                mr.SetPropertyBlock(block);
                n++;
            }

            foreach (Light l in UnityEngine.Object
                         .FindObjectsByType<Light>(FindObjectsInactive.Include))
            {
                if (l.name == "TvLight" || l.name == "PhoneBuzzLight")
                {
                    l.enabled = false;
                    n++;
                }
            }
            return n;
        }

        /// <summary>
        /// R7/R8 - the wear A/B pair. Captures the three review poses twice, once with the
        /// generated wear root live and once with it disabled, into wear-on/ and wear-off/.
        ///
        /// THIS IS THE ONLY INSTRUMENT THAT CAN ANSWER "does the wear read". Box statistics on a
        /// still frame cannot: a decal is small and sits against busy geometry, so a box drawn
        /// round it measures the pipe fitting, the window sill or the slab edge next to it. That
        /// was tried and it reported two of four wear pieces as reading when brightened crops
        /// show nothing at any of them. A per-pixel diff of two otherwise identical renders has
        /// no such ambiguity: whatever differs IS the wear, because nothing else changed.
        ///
        /// R7's original verdict came from exactly this measurement - 1.92% of pixels changed
        /// against a 1.69% baseline, "very nearly invisible" - so re-running it is also the only
        /// way to compare against the number that parked the inventory.
        ///
        /// EDIT MODE, and that matters here more than elsewhere: the diff is only meaningful if
        /// everything except the wear is identical between the two passes. Play Mode would put
        /// live screen content in both frames with no guarantee it matches, and any difference
        /// there would be counted as wear.
        ///
        ///   Unity.exe -batchmode -quit -projectPath (project)
        ///             -executeMethod SBR.RoomViewCapture.CaptureWearAB -outDir (path)
        /// </summary>
        public static void CaptureWearAB()
        {
            string outDir = OutDirFromArgs();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            var wearRoot = GameObject.Find("Dressing_Wear");
            if (wearRoot == null)
                throw new InvalidOperationException(
                    "Dressing_Wear not found - the A/B cannot isolate the wear, and a pair whose " +
                    "two halves are identical would read as 'the wear changes nothing'");

            Camera cam = FindPlayerCamera();
            var controller = UnityEngine.Object.FindAnyObjectByType<FirstPersonController>();
            if (controller != null)
                controller.enabled = false;

            string on = Path.Combine(outDir, "wear-on");
            string off = Path.Combine(outDir, "wear-off");
            Directory.CreateDirectory(on);
            Directory.CreateDirectory(off);

            // WARM THE PIPELINE BEFORE THE FIRST SAVED FRAME. Without this the first Edit Mode
            // render of a session comes back with an unresolved TV panel (magenta) and a
            // materially different frame, so an A/B whose two halves are the first and second
            // renders attributes the whole warm-up delta to whatever was toggled between them.
            // Measured before the fix: 91.76% of pixels "changed", reproducible across runs -
            // the determinism control passed precisely because the same broken ordering repeats.
            // Caught by looking at the frames: the TV cannot change colour because a floor decal
            // was disabled.
            WarmRender(cam);

            wearRoot.SetActive(true);
            Capture(on);
            wearRoot.SetActive(false);
            Capture(off);
            wearRoot.SetActive(true);   // leave the scene as we found it

            Debug.Log($"[RoomViewCapture] wear A/B written to {on} and {off}");
        }

        /// <summary>
        /// R35 - the lid-glow A/B, so the ruled colour can be judged on a frame.
        ///
        /// The strike landed without a frame because none could show it: in Play Mode the lid's
        /// emission sits BEHIND the SureThing canvas, and what reaches the room is dominated by the
        /// desk lamp's warm pool. Both problems disappear in Edit Mode - the canvas is built in
        /// Awake, so it does not exist here, and the lid quad renders its own emission unobstructed.
        /// That is why this is Edit Mode and not merely convenient: it is the only state in which
        /// the thing being ruled is visible at all.
        ///
        /// Three states, because "rule the exact value" needs more than a before/after:
        ///   emission-struck  the retired violet, for comparison only
        ///   emission-attention  the proposal's bright end - the cue the player must catch
        ///   emission-idle       the proposal's rest end - what the room lives with
        ///
        /// The struck value is hard-coded HERE and nowhere else. It is the A/B's comparand, not a
        /// live option, and it must not be copied back into LaptopScreen.
        ///
        /// Diff with:  python tools/wear_ab_diff.py &lt;dir&gt;/emission-struck &lt;dir&gt;/emission-attention
        ///
        ///   Unity.exe -batchmode -quit -projectPath (project)
        ///             -executeMethod SBR.RoomViewCapture.CaptureEmissionAB -outDir (path)
        /// </summary>
        public static void CaptureEmissionAB()
        {
            string outDir = OutDirFromArgs();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            var book = UnityEngine.Object.FindAnyObjectByType<SBR.Game.LaptopScreen>();
            if (book == null || book.lidRenderer == null)
                throw new InvalidOperationException(
                    "LaptopScreen or its lidRenderer is missing - the A/B would capture three " +
                    "identical sets and read as 'the colour makes no difference'");

            Camera cam = FindPlayerCamera();
            var controller = UnityEngine.Object.FindAnyObjectByType<FirstPersonController>();
            if (controller != null)
                controller.enabled = false;

            WarmRender(cam);   // never let the first saved frame be the pipeline's first

            // (label, emission) - struck value quoted from batch 11's ruling, for comparison only.
            var states = new (string label, Color value)[]
            {
                ("emission-struck",    new Color(0.28f,  0.10f,  0.55f)),
                // The superseded 4.07x warm build, carried so the ruled ~3x ceiling can be judged
                // against the thing it replaced rather than in the abstract. Like the struck
                // violet, it is quoted HERE only and is not a live option.
                ("emission-prev-4x",   new Color(0.155f, 0.130f, 0.098f)),
                // The 3.00x attention end, quoted here since S63-am2 struck the cue and removed
                // the field. Retained so the four-arm A/B the colour grant rests on stays
                // reproducible; like the two values above it, a comparand and not a live option.
                ("emission-attention", new Color(0.114f, 0.096f, 0.072f)),
                ("emission-idle",      book.idleEmission),
            };

            var block = new MaterialPropertyBlock();
            int emissionId = Shader.PropertyToID("_EmissionColor");

            foreach (var (label, value) in states)
            {
                book.lidRenderer.GetPropertyBlock(block);
                block.SetColor(emissionId, value);
                book.lidRenderer.SetPropertyBlock(block);

                string dir = Path.Combine(outDir, label);
                Directory.CreateDirectory(dir);
                Capture(dir);
                Debug.Log($"[RoomViewCapture] {label} = {value.r:F3},{value.g:F3},{value.b:F3} -> {dir}");
            }

            // Leave the lid at its shipped rest value rather than whichever state ran last.
            book.lidRenderer.GetPropertyBlock(block);
            block.SetColor(emissionId, book.idleEmission);
            book.lidRenderer.SetPropertyBlock(block);
        }

        /// <summary>
        /// Does a change to the lid's emission reach a frame the player actually occupies?
        ///
        /// Built for S63-am2 (DD 2026-08-07, batch 13), which suspended the laptop's attention
        /// cue pending exactly one Play Mode frame with wantsYou &amp;&amp; !engaged true from a pose
        /// containing the laptop, disposition pre-committed both ways. IT ANSWERED THAT AND THE
        /// CUE WAS STRUCK - seated OUT OF FRUSTUM, focused IDENTICAL at 0.00%, standing 233 px
        /// above JND out of 3.69M (a one-pixel rim line), room cast 0.000. Evidence retained at
        /// artifacts/room-visual-pass/s63am2-glow-cue.
        ///
        /// KEPT, GENERALISED, because the fact it established outlives the cue: at runtime the
        /// lid is behind an opaque canvas and is absent from the seated frame altogether, so ANY
        /// future proposal to treat that surface has to clear the same bar. The two arms are now
        /// quoted constants rather than live fields - there is no longer a cue state to read.
        ///
        /// PLAY MODE IS THE POINT, not a formality. The lid's emission is the whole subject, and
        /// in Edit Mode nothing covers it. At runtime BuildSkeleton() puts the SureThing
        /// world-space canvas canvasOffset (4mm) in front of the lid at exactly the lid's own
        /// world size, so the surface being ruled sits behind an opaque quad. Every emission A/B
        /// this lane has shot was Edit Mode, and therefore measured the lid uncovered - a state
        /// the player is never in.
        ///
        /// A PAIR, NOT A SINGLE FRAME. "Does it read" is a difference question and one frame
        /// cannot answer it. Arm A is the cue firing; arm B is the same frame at idle.
        ///
        /// THE STATE IS REACHED, NOT FORCED. Run.Phase defaults to Betting and DeskFocus.Active
        /// is null until the player zooms the desk, so on the batch-13 run wantsYou &amp;&amp; !engaged
        /// was already true and Glow() wrote the attention value unaided - logged as proof rather
        /// than asserted. That branch is gone now, but the pose and the occlusion it measured are
        /// properties of the room, not of the cue.
        ///
        /// BOTH EMITTERS ARE FROZEN. LaptopScreen is disabled so nothing overwrites arm B, which
        /// also freezes the canvas content so the arms cannot differ by an OS tick. PhoneScreen
        /// is disabled for a second reason: it drives its own emission up to (0.30,0.50,0.90) on
        /// a 0.55s buzz with a real Light, from the desk beside the laptop. Left running it could
        /// manufacture a difference between the arms or mask one. Its emission at capture is
        /// logged rather than assumed - on the batch-13 run it sat at its blue unread value.
        ///
        ///   Unity.exe -batchmode -projectPath (project)
        ///             -executeMethod SBR.RoomViewCapture.CaptureLidEmissionInPlay -outDir (path)
        ///
        /// SCOPE, AND IT MATTERS (C25). What this produces is an internally-valid PAIR and
        /// nothing more. The two arms are shot back to back with everything but the lid frozen,
        /// so a difference between them IS the lid. They are NOT comparable to the ratified
        /// CaptureAll sets: measured on the batch-13 run, cue-off differs from the batch-13
        /// CaptureAll frame across 95.4% of pixels because this method warms and poses the camera
        /// differently. R9-A's mattress reads 44.10 on that set and 38.08 on this one -- and
        /// 38.08 on BOTH arms, which is how the pair stays trustworthy while the cross-set
        /// comparison does not. DO NOT judge a ratified figure on frames from this method; that
        /// is the batch-9 mattress defect (a ratified number quoted without its capture) exactly.
        ///
        /// NO -quit: the harness exits the editor itself, as CaptureAll does.
        /// Diff with:  python tools/wear_ab_diff.py (dir)/cue-on (dir)/cue-off
        /// </summary>
        public static void CaptureLidEmissionInPlay()
        {
            string outDir = OutDirFromArgs();
            Directory.CreateDirectory(outDir);

            // SessionState survives the domain reload that entering Play Mode triggers.
            SessionState.SetString(OutDirKey, outDir);
            SessionState.SetBool(GlowCueArmedKey, true);

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }

        [InitializeOnLoadMethod]
        private static void RehookGlowCue()
        {
            if (SessionState.GetBool(GlowCueArmedKey, false))
                EditorApplication.update += OnGlowCueUpdate;
        }

        private static void OnGlowCueUpdate()
        {
            if (!EditorApplication.isPlaying)
                return;

            _glowFrames++;
            if (_glowFrames < WarmupFrames)
                return;

            EditorApplication.update -= OnGlowCueUpdate;
            SessionState.SetBool(GlowCueArmedKey, false);

            int code = 0;
            try
            {
                GlowCueRun(SessionState.GetString(OutDirKey, string.Empty));
            }
            catch (Exception e)
            {
                Debug.LogError($"[GlowCue] capture failed: {e}");
                code = 1;
            }

            EditorApplication.Exit(code);
        }

        private static void GlowCueRun(string outDir)
        {
            if (string.IsNullOrEmpty(outDir))
                throw new InvalidOperationException("output directory lost across domain reload");

            var book = UnityEngine.Object.FindAnyObjectByType<SBR.Game.LaptopScreen>();
            if (book == null || book.lidRenderer == null)
                throw new InvalidOperationException(
                    "LaptopScreen or its lidRenderer is missing - both arms would be identical " +
                    "and the pair would read as 'the cue makes no difference'");

            // The probe. Quoted, not read from a field: this is the struck 3.00x attention end,
            // kept so the run that struck it stays reproducible bit for bit.
            var probe = new Color(0.114f, 0.096f, 0.072f);

            int emissionId = Shader.PropertyToID("_EmissionColor");
            var block = new MaterialPropertyBlock();
            book.lidRenderer.GetPropertyBlock(block);
            Color live = block.GetColor(emissionId);

            string phase = book.director != null && book.director.Run != null
                ? book.director.Run.Phase.ToString()
                : "<no run>";
            string focus = SBR.Game.DeskFocus.Active == null
                ? "null (NOT engaged)"
                : SBR.Game.DeskFocus.Active.name;

            Debug.Log($"[GlowCue] phase={phase}  DeskFocus.Active={focus}");
            Debug.Log($"[GlowCue] lid emission live on the renderer = {Fmt(live)}");
            Debug.Log($"[GlowCue]   idleEmission (granted) = {Fmt(book.idleEmission)}");
            Debug.Log($"[GlowCue]   probe (struck 3.00x)   = {Fmt(probe)}");

            // Proves the shipped path actually put the granted colour on the renderer, rather
            // than the field merely holding it. That distinction is not academic here: a public
            // field's default does not touch an already-serialized component, and the first
            // strike was never built for exactly that reason - the A/B then captured the value
            // it was supposed to be replacing, and only the picture caught it.
            if (!Same(live, book.idleEmission))
            {
                throw new InvalidOperationException(
                    $"the lid is NOT carrying the granted colour at runtime: renderer has " +
                    $"{Fmt(live)}, idleEmission is {Fmt(book.idleEmission)}. Either " +
                    "ApplyLidEmission did not run or something else owns this property block; " +
                    "nothing was written.");
            }

            // R39-am (DD 2026-08-08, batch 15): the phone gets the same treatment, and for the
            // same reason the lid did. Its emitter sits behind a world-space canvas 1.5mm out
            // with an opaque backing, so a Play-Mode region reads the canvas -- my own filed
            // reading was that mistake, and R39's "unlike the lid, these are observable" line
            // was struck on it. Edit Mode already gave the authored-value conformance; what is
            // unknown is whether ANY of it reaches the player.
            //
            // Disposition pre-committed: if the phone is as unobservable at runtime as the lid,
            // the granted colours stand -- they govern Edit-Mode captures, the material and every
            // bake-adjacent path -- and no cue, state or gameplay signal is ever built on the
            // phone's glow.
            var phone = UnityEngine.Object.FindAnyObjectByType<SBR.Game.PhoneScreen>();
            Renderer phoneRenderer = null;
            Color phoneLive = Color.black;
            if (phone != null)
            {
                if (phone.screenRenderer != null)
                {
                    phoneRenderer = phone.screenRenderer;
                    var phoneBlock = new MaterialPropertyBlock();
                    phoneRenderer.GetPropertyBlock(phoneBlock);
                    phoneLive = phoneBlock.GetColor(emissionId);
                    Debug.Log($"[GlowCue] phone emission live on the renderer = {Fmt(phoneLive)}");
                }
                phone.enabled = false;
            }

            // Glow() must not overwrite arm B, and the canvas must not tick between the arms.
            book.enabled = false;

            Camera cam = FindPlayerCamera();
            var controller = UnityEngine.Object.FindAnyObjectByType<FirstPersonController>();
            if (controller != null)
                controller.enabled = false;

            WarmRender(cam);

            // Controls, on the model the Edit-Mode set had to learn the hard way: control-a
            // opens, control-z closes after every arm has been written and restored. a == b
            // shows the pipeline settled; a == z shows the room ended as it began. Two sets
            // were thrown away before this discipline existed, each failing in a way the other
            // control could not have seen.
            foreach (string ctl in new[] { "control-a", "control-b" })
            {
                WarmRender(cam);
                string cd = Path.Combine(outDir, ctl);
                Directory.CreateDirectory(cd);
                Capture(cd);
            }

            ShootArm(cam, book.lidRenderer, outDir, "cue-on", probe, emissionId);
            ShootArm(cam, book.lidRenderer, outDir, "cue-off", book.idleEmission, emissionId);

            // R39-am's pair. Same shape, one object over.
            if (phoneRenderer != null)
            {
                // Save the ACTUAL block and put it back, exactly as the Edit-Mode set had to
                // learn. ShootArm writes an emission-only block, so "restoring" by writing the
                // value back leaves the renderer subtly unlike how it started -- and on the
                // batch-15 run that showed up as control-a != control-z, confined to this
                // object's own 44x21 px box while the rest of the room was bit-identical. The
                // arms were unaffected (phone-on == phone-restored), but a control that fails
                // for a known harmless reason is a control everyone learns to ignore.
                bool phoneHadBlock = phoneRenderer.HasPropertyBlock();
                var phoneOriginal = new MaterialPropertyBlock();
                if (phoneHadBlock) phoneRenderer.GetPropertyBlock(phoneOriginal);

                ShootArm(cam, phoneRenderer, outDir, "phone-on", phoneLive, emissionId);
                ShootArm(cam, phoneRenderer, outDir, "phone-off", Color.black, emissionId);
                ShootArm(cam, phoneRenderer, outDir, "phone-restored", phoneLive, emissionId);

                if (phoneHadBlock) phoneRenderer.SetPropertyBlock(phoneOriginal);
                else phoneRenderer.SetPropertyBlock(null);
            }
            else
            {
                Debug.LogWarning("[GlowCue] no phone renderer -- R39-am's pair was NOT shot, and " +
                                 "an absent pair must not read as an unobservable one");
            }

            WarmRender(cam);
            string closeDir = Path.Combine(outDir, "control-z");
            Directory.CreateDirectory(closeDir);
            Capture(closeDir);

            // AFTER the arms, deliberately. This walks the camera through all three poses, and
            // anything the pipeline settles over time would then be settling BETWEEN the halves
            // of the A/B rather than only before it. Ran before the arms on the batch-13 shot;
            // the pair stayed internally valid (mattress 38.08 in both, identical) but the set
            // came out 95% different from the ratified CaptureAll set - see the scope note above.
            LogLidFraming(cam, book.lidRenderer);
        }

        private static void ShootArm(Camera cam, Renderer lid, string outDir,
                                     string label, Color emission, int emissionId)
        {
            var block = new MaterialPropertyBlock();
            lid.GetPropertyBlock(block);
            block.SetColor(emissionId, emission);
            lid.SetPropertyBlock(block);

            string dir = Path.Combine(outDir, label);
            Directory.CreateDirectory(dir);
            Capture(dir);
            Debug.Log($"[GlowCue] {label} = {Fmt(emission)} -> {dir}");
        }

        /// <summary>
        /// Where the lid actually lands in each ratified pose, reported rather than asserted.
        /// "A pose that contains the laptop" is a claim about the frustum, and this lane has
        /// already shipped one finding built on frustum coverage that turned out to say nothing
        /// about visibility (R7-F, ruled informational) - so this reports position only, and the
        /// image pair is what answers whether the cue reads.
        /// </summary>
        private static void LogLidFraming(Camera cam, Renderer lid)
        {
            float prevAspect = cam.aspect;
            cam.aspect = (float)Width / Height;
            try
            {
                Vector3 centre = lid.bounds.center;
                foreach (Pose p in RatifiedPoses())
                {
                    cam.transform.SetPositionAndRotation(p.Eye, p.Rot);
                    cam.fieldOfView = p.Fov;
                    Vector3 v = cam.WorldToViewportPoint(centre);
                    bool inFrame = v.z > 0f && v.x >= 0f && v.x <= 1f && v.y >= 0f && v.y <= 1f;
                    Debug.Log($"[GlowCue] lid centre in {p.File}: " +
                              $"viewport ({v.x:F3}, {v.y:F3}) depth {v.z:F3}m -> " +
                              $"{(inFrame ? "IN FRUSTUM" : "OUT OF FRUSTUM")}");
                }
            }
            finally
            {
                cam.aspect = prevAspect;
            }
        }

        private static string Fmt(Color c) => $"{c.r:F4},{c.g:F4},{c.b:F4}";

        private static bool Same(Color a, Color b)
            => Mathf.Abs(a.r - b.r) < 1e-4f
            && Mathf.Abs(a.g - b.g) < 1e-4f
            && Mathf.Abs(a.b - b.b) < 1e-4f;

        /// <summary>
        /// EMIT Part B — the per-emitter A/B set the emission instrument measures.
        ///
        /// EDIT MODE ON PURPOSE, and it is the whole reason this works. At runtime both
        /// screens carry a world-space canvas over the emissive quad (4mm on the lid, 1.5mm on
        /// the phone) with an opaque backing, which is what made the lid's cue unphotographable
        /// and what made my own dark-vs-lit phone reading wrong. In Edit Mode no canvas exists,
        /// so the emitter is the surface you are looking at.
        ///
        /// TWO FRAMES PER EMITTER: the shipped value, and the same frame with THAT emitter alone
        /// forced black. The pair does two jobs at once — it isolates one emitter's contribution
        /// from every other light in the room, and it PROVES the measured region is reading that
        /// emitter at all. If the two frames are identical in a region, the region does not see
        /// the emitter and no verdict about it is supportable there. That is the check I did not
        /// have when I reported a canvas as an emission reading.
        ///
        /// EMITTERS ARE DISCOVERED, NOT LISTED. Scanning for a non-black _EmissionColor means a
        /// new emissive surface added later turns up here on its own; the analyser cross-checks
        /// what was captured against its registry and fails if they disagree, so neither side can
        /// quietly fall behind the room. A hardcoded list on both sides would just be two lists.
        ///
        ///   Unity.exe -batchmode -quit -projectPath (project)
        ///             -executeMethod SBR.RoomViewCapture.CaptureEmissionSet -outDir (path)
        /// </summary>
        public static void CaptureEmissionSet()
        {
            string outDir = OutDirFromArgs();
            Directory.CreateDirectory(outDir);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            int emissionId = Shader.PropertyToID("_EmissionColor");
            var found = new List<(string mat, Color value, List<Renderer> rends)>();

            foreach (Renderer r in UnityEngine.Object.FindObjectsByType<Renderer>(
                         FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Material m = r.sharedMaterial;
                if (m == null || !m.HasProperty(emissionId)) continue;
                Color e = m.GetColor(emissionId);
                if (Mathf.Max(e.r, Mathf.Max(e.g, e.b)) <= 0.0001f) continue;

                var hit = found.Find(f => f.mat == m.name);
                if (hit.rends != null) hit.rends.Add(r);
                else found.Add((m.name, e, new List<Renderer> { r }));
            }

            if (found.Count == 0)
                throw new InvalidOperationException(
                    "no emissive surfaces found — the room has at least five, so this is the " +
                    "harness failing, not the room being clean (C29: a run that measured " +
                    "nothing is not a pass)");

            Camera cam = FindPlayerCamera();
            var controller = UnityEngine.Object.FindAnyObjectByType<FirstPersonController>();
            if (controller != null) controller.enabled = false;
            WarmRender(cam);

            var manifest = new List<string>();
            manifest.Add("# EMIT Part B capture manifest");
            manifest.Add($"scene={ScenePath}");
            manifest.Add($"width={Width} height={Height}");
            manifest.Add($"poses={string.Join(",", Array.ConvertAll(RatifiedPoses(), p => p.File))}");
            manifest.Add("mode=EditMode (no world-space canvas exists; emitters are unoccluded)");
            manifest.Add($"emitters={found.Count}");

            // DETERMINISM CONTROL, and the first run of this harness is why it exists.
            //
            // The shared reference frame was shot straight after the warm-up while every
            // off-frame came later in the sequence, and something in the pipeline was still
            // settling: the reference landed at mean luma 71.385 against ~51.27 for every
            // off-frame. A 20-luma global gap that no small emitter can produce, and it made
            // all five emitters report ~70% footprint at +25 dY'. Nothing about the room was
            // wrong; the instrument was measuring its own warm-up.
            //
            // Two captures of an UNCHANGED state settle it. If they are not bit-identical the
            // pipeline is still moving and no difference in this set means anything, so the
            // analyser refuses to measure rather than reporting drift as light. This is R8's
            // first-render artefact in a new costume, and that one also passed a determinism
            // control twice before the picture caught it -- so the control is compared by the
            // analyser, not asserted here.
            foreach (string ctl in new[] { "control-a", "control-b" })
            {
                WarmRender(cam);
                string ctlDir = Path.Combine(outDir, ctl);
                Directory.CreateDirectory(ctlDir);
                Capture(ctlDir);
            }
            manifest.Add("control=control-a,control-b (identical state; must be bit-identical)");

            // The full-value reference frame, every emitter as shipped.
            WarmRender(cam);
            string allDir = Path.Combine(outDir, "all-emitters-on");
            Directory.CreateDirectory(allDir);
            Capture(allDir);

            foreach (var (mat, value, rends) in found)
            {
                // SAVE THE ACTUAL STATE, then put it back byte for byte.
                //
                // The previous version "restored" by writing the SHARED MATERIAL's emission into
                // a property block, which is not a restore -- it is a reset to a value the
                // renderer may never have had. Any renderer carrying its own block came back
                // different, so each capture left the room altered for every capture after it,
                // and off-ScreenLaptop ended up 20 luma dark across 72% of a pose the laptop is
                // not even in frustum for.
                //
                // A FRESH block per renderer, too. GetPropertyBlock does NOT clear its
                // destination, so a reused block silently carries the previous renderer's
                // properties into the next one.
                var originals = new List<(Renderer r, bool had, MaterialPropertyBlock blk)>();
                foreach (Renderer r in rends)
                {
                    bool had = r.HasPropertyBlock();
                    var orig = new MaterialPropertyBlock();
                    if (had) r.GetPropertyBlock(orig);
                    originals.Add((r, had, orig));

                    var blackout = new MaterialPropertyBlock();
                    if (had) r.GetPropertyBlock(blackout);
                    blackout.SetColor(emissionId, Color.black);
                    r.SetPropertyBlock(blackout);
                }

                // Warm before every capture, so each frame is taken at the same settled
                // state as the reference rather than somewhere along the warm-up curve.
                WarmRender(cam);
                string dir = Path.Combine(outDir, $"off-{mat}");
                Directory.CreateDirectory(dir);
                Capture(dir);

                foreach (var (r, had, orig) in originals)
                {
                    if (had) r.SetPropertyBlock(orig);
                    else r.SetPropertyBlock(null);   // no block originally: clear, do not invent one
                }

                manifest.Add($"emitter={mat} value={value.r:F4},{value.g:F4},{value.b:F4} " +
                             $"renderers={rends.Count} off_dir=off-{mat}");
                Debug.Log($"[EMIT] {mat} = {value.r:F4},{value.g:F4},{value.b:F4} " +
                          $"on {rends.Count} renderer(s) -> {dir}");
            }

            // CLOSING CONTROL, and it is the one that matters. control-a/b sit before the
            // measurements and only ever proved the warm-up had settled -- they bracket the
            // wrong interval. This one is shot AFTER every emitter has been blacked out and
            // put back, so control-a == control-z means the room ended exactly as it began
            // and no capture mutated the scene for the captures after it. That is the failure
            // the first two runs had, and neither control could see it.
            WarmRender(cam);
            string closeDir = Path.Combine(outDir, "control-z");
            Directory.CreateDirectory(closeDir);
            Capture(closeDir);
            manifest.Add("control_close=control-z (after all restores; must equal control-a)");

            File.WriteAllLines(Path.Combine(outDir, "manifest.txt"), manifest);
            Debug.Log($"[EMIT] {found.Count} emitters captured -> {outDir}");
        }

        /// <summary>
        /// Allen's blur finding — is it the GRADE or the canvas RESOLUTION?
        ///
        /// He walked the post-C13-merge tree and reported the SureThing UI "VERY BLURRY", whole
        /// surface uniformly soft, text legible but fuzzy. Three of the four quiet-failure checks
        /// pass on his screenshot (two voices present, U+2212 rendering, rail reads NOTEBOOK), so
        /// the TMP assets travelled and it is not running on a fallback face. Two candidates
        /// survive and they route to different owners, which is why this must be measured rather
        /// than argued:
        ///
        ///   GRADE       FilmGrain 0.2, ChromaticAberration 0.065 (visibly fringing the blue box
        ///               in his frame), Bloom threshold 0.9 / scatter 0.7 with highQualityFiltering
        ///               OFF. Screens sit inside the unified grade and are not exempt. The grade
        ///               was tuned against the OLD laptop surface; the new one is authored to a
        ///               13px fact floor with 1-2px rules. -> the DD, under C20.
        ///   RESOLUTION  the canvas is authored 1024 wide (S2's locked artboard) and his frame
        ///               shows it ~1310 across, so ~1.28x magnification. Harmless for SDF text,
        ///               real upsampling blur for sprite content. -> room's canvas number.
        ///
        /// PLAY MODE, AND THAT IS NOT OPTIONAL. The SureThing canvas is built in Awake, so in Edit
        /// Mode there is no UI on the lid at all — the entire subject of this test is absent. Every
        /// emission set this lane has shot was Edit Mode for the opposite reason.
        ///
        /// Same pose, same frame, grade on then off. If the ungraded frame is sharp the grade is
        /// the cause; if both are soft it is resolution; if both are sharp the blur is neither and
        /// lives in something only Allen's own display path shows.
        ///
        ///   Unity.exe -batchmode -projectPath (project)
        ///             -executeMethod SBR.RoomViewCapture.CaptureLaptopBlurAB -outDir (path)
        ///
        /// NO -quit: exits itself, as CaptureAll does.
        /// </summary>
        public static void CaptureLaptopBlurAB()
        {
            string outDir = OutDirFromArgs();
            Directory.CreateDirectory(outDir);

            // FORCE THE FONT ASSETS TO REIMPORT, in this session, before anything renders.
            //
            // SureThing's generator fix corrected _TextureWidth/_TextureHeight from 1x1 to the
            // atlas's real 1024x1024 on all three faces. The .asset files in this tree are
            // verifiably correct — and the first verification shoot measured NO change at all
            // (1.00x on the UI panel, 1.01x on the glyph strip, crops indistinguishable).
            //
            // Two readings survive that and they need separating: either the corrected material
            // never reached the render because a -batchmode run reused a stale Library import, or
            // the mismatch was never what softens these glyphs. A serialized material change with
            // no reimport is exactly the shape that produces a false negative, and this lane threw
            // one away an hour ago by shooting a tree whose fix had not travelled.
            //
            // So the reimport happens HERE rather than as a separate launch: same session, same
            // editor, guaranteed to precede the first frame. If the glyphs sharpen now, it was a
            // stale import; if they do not, the mismatch is exonerated as the cause and stays
            // correct-as-authored.
            foreach (string face in new[]
                     {
                         "Assets/SBR/Resources/SureThing/Fonts/Archivo SDF.asset",
                         "Assets/SBR/Resources/SureThing/Fonts/Archivo SemiBold SDF.asset",
                         "Assets/SBR/Resources/SureThing/Fonts/ArchivoNarrow SDF.asset",
                     })
            {
                AssetDatabase.ImportAsset(face, ImportAssetOptions.ForceUpdate);
                Debug.Log($"[BlurAB] force-reimported {face}");
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            SessionState.SetString(OutDirKey, outDir);
            SessionState.SetBool(BlurArmedKey, true);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }

        [InitializeOnLoadMethod]
        private static void RehookBlur()
        {
            if (SessionState.GetBool(BlurArmedKey, false))
                EditorApplication.update += OnBlurUpdate;
        }

        private static void OnBlurUpdate()
        {
            if (!EditorApplication.isPlaying)
                return;

            _blurFrames++;
            if (_blurFrames < WarmupFrames)
                return;

            EditorApplication.update -= OnBlurUpdate;
            SessionState.SetBool(BlurArmedKey, false);

            int code = 0;
            try
            {
                BlurRun(SessionState.GetString(OutDirKey, string.Empty));
            }
            catch (Exception e)
            {
                Debug.LogError($"[BlurAB] capture failed: {e}");
                code = 1;
            }

            EditorApplication.Exit(code);
        }

        private static void BlurRun(string outDir)
        {
            if (string.IsNullOrEmpty(outDir))
                throw new InvalidOperationException("output directory lost across domain reload");

            // The UI must actually be there, or a soft frame and an EMPTY frame look alike in a
            // sharpness metric. Assert the canvas exists before either arm is shot.
            var book = UnityEngine.Object.FindAnyObjectByType<SBR.Game.LaptopScreen>();
            if (book == null)
                throw new InvalidOperationException("no LaptopScreen - nothing to measure");
            var canvas = book.GetComponentInChildren<Canvas>(true);
            if (canvas == null)
                throw new InvalidOperationException(
                    "the laptop canvas does not exist in this Play session, so both arms would be " +
                    "blank and would score identically 'sharp'. Not a blur measurement.");
            int graphics = canvas.GetComponentsInChildren<UnityEngine.UI.Graphic>(true).Length;
            Debug.Log($"[BlurAB] canvas present with {graphics} Graphic(s) - the subject is in frame");
            if (graphics == 0)
                throw new InvalidOperationException("canvas draws nothing; no blur to measure");

            Camera cam = FindPlayerCamera();
            var controller = UnityEngine.Object.FindAnyObjectByType<FirstPersonController>();
            if (controller != null) controller.enabled = false;

            GameObject volGo = GameObject.Find("RoomPostFx");
            var vol = volGo != null ? volGo.GetComponent<Volume>() : null;
            if (vol == null)
                throw new InvalidOperationException(
                    "RoomPostFx volume not found - the grade cannot be bypassed, so this run " +
                    "cannot separate grade from resolution and must not pretend to");

            // CONTROLS, and this pass is why they are here. The first blur A/B had none: it shot
            // graded then ungraded straight through, so its 1.44x / 1.14x figures rested on the
            // assumption that two captures of an unchanged main-camera state agree. A routed
            // finding says that assumption is unestablished — C34 reproducibility was only ever
            // demonstrated for the flat halves, and the MAIN-CAMERA halves measurably are not
            // reproducible. Until this pair comes back identical, every number I have reported off
            // a room-camera frame is illustration rather than measurement, including my own.
            //
            // So the control runs FIRST and the analyser is told to believe nothing without it.
            foreach (string ctl in new[] { "control-a", "control-b" })
            {
                WarmRender(cam);
                string cd = Path.Combine(outDir, ctl);
                Directory.CreateDirectory(cd);
                Capture(cd);
            }
            Debug.Log("[BlurAB] control pair shot (main camera, unchanged state)");

            WarmRender(cam);
            string gradedDir = Path.Combine(outDir, "graded");
            Directory.CreateDirectory(gradedDir);
            Capture(gradedDir);
            Debug.Log("[BlurAB] graded arm shot");

            vol.enabled = false;
            WarmRender(cam);
            string plainDir = Path.Combine(outDir, "ungraded");
            Directory.CreateDirectory(plainDir);
            Capture(plainDir);
            vol.enabled = true;
            Debug.Log("[BlurAB] ungraded arm shot; volume restored");
        }

        public static void CaptureAll()
        {
            string outDir = OutDirFromArgs();
            Directory.CreateDirectory(outDir);

            // SessionState survives the domain reload that entering Play Mode triggers;
            // plain statics do not, which is why the re-hook below exists.
            SessionState.SetString(OutDirKey, outDir);
            SessionState.SetBool(ArmedKey, true);

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }

        [InitializeOnLoadMethod]
        private static void Rehook()
        {
            if (SessionState.GetBool(ArmedKey, false))
                EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            if (!EditorApplication.isPlaying)
                return;

            _frames++;
            if (_frames < WarmupFrames)
                return;

            EditorApplication.update -= OnUpdate;
            SessionState.SetBool(ArmedKey, false);

            int code = 0;
            try
            {
                string outDir = SessionState.GetString(OutDirKey, string.Empty);
                if (string.IsNullOrEmpty(outDir))
                    throw new InvalidOperationException("output directory lost across domain reload");
                Capture(outDir);
            }
            catch (Exception e)
            {
                Debug.LogError($"[RoomViewCapture] capture failed: {e}");
                code = 1;
            }

            EditorApplication.Exit(code);
        }

        /// <summary>One ratified review pose: the eye, the look, the field of view.</summary>
        private readonly struct Pose
        {
            public readonly string File;
            public readonly Vector3 Eye;
            public readonly Quaternion Rot;
            public readonly float Fov;

            public Pose(string file, Vector3 eye, Quaternion rot, float fov)
            {
                File = file; Eye = eye; Rot = rot; Fov = fov;
            }
        }

        /// <summary>
        /// The three ratified review poses, in one place. They were duplicated between Capture()
        /// and every diagnostic that wanted to report where something lands in frame, which is a
        /// standing invitation for a measurement to be quoted against a pose that has since moved
        /// (C25's failure mode - a number without the rig it was taken on).
        /// </summary>
        private static Pose[] RatifiedPoses()
        {
            // Matches the builder: LookRotation(tvScreenCenter - seatedEye, up).
            var seatedEye = new Vector3(-0.950f, 1.150f, 0.300f);
            var tvCenter = new Vector3(1.232f, 1.100f, 0.300f);

            return new[]
            {
                new Pose("standing-overview.png",
                    new Vector3(0.300f, 1.640f, -1.400f),
                    Quaternion.LookRotation(Vector3.forward, Vector3.up),
                    68f),

                new Pose("seated-tv-couch.png",
                    seatedEye,
                    Quaternion.LookRotation(tvCenter - seatedEye, Vector3.up),
                    17f),

                // Normal to the tilted laptop lid, 0.52m out along its outward normal.
                new Pose("focused-laptop-desk.png",
                    new Vector3(0.738982f, 1.051217f, 1.620000f),
                    Quaternion.LookRotation(new Vector3(0.939693f, -0.342020f, 0f),
                                            new Vector3(0.342020f, 0.939693f, 0f)),
                    30f),
            };
        }

        private static void Capture(string outDir)
        {
            Camera cam = FindPlayerCamera();

            // Stop the controller writing to the transform between our pose and the render.
            var controller = UnityEngine.Object.FindAnyObjectByType<FirstPersonController>();
            if (controller != null)
                controller.enabled = false;

            foreach (Pose p in RatifiedPoses())
                Shoot(cam, outDir, p.File, p.Eye, p.Rot, p.Fov);
        }

        /// <summary>
        /// Renders and discards a few frames so the first SAVED frame is not the pipeline's
        /// first. Cheap insurance that costs a few milliseconds and, unwarmed, silently
        /// invalidated a whole A/B measurement (see CaptureWearAB).
        /// </summary>
        private static void WarmRender(Camera cam)
        {
            var rt = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 1 };
            RenderTexture prev = cam.targetTexture;
            try
            {
                cam.targetTexture = rt;
                for (int i = 0; i < 3; i++)
                    cam.Render();
            }
            finally
            {
                cam.targetTexture = prev;
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
            }
        }

        private static void Shoot(Camera cam, string dir, string file,
                                  Vector3 pos, Quaternion rot, float fov)
        {
            cam.transform.SetPositionAndRotation(pos, rot);
            cam.fieldOfView = fov;

            var rt = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 1
            };

            RenderTexture prevTarget = cam.targetTexture;
            RenderTexture prevActive = RenderTexture.active;
            Texture2D tex = null;

            try
            {
                cam.targetTexture = rt;
                cam.Render();

                RenderTexture.active = rt;
                tex = new Texture2D(Width, Height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0f, 0f, Width, Height), 0, 0);
                tex.Apply();

                string path = Path.Combine(dir, file);
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Debug.Log($"[RoomViewCapture] wrote {path}");
            }
            finally
            {
                cam.targetTexture = prevTarget;
                RenderTexture.active = prevActive;
                if (tex != null)
                    UnityEngine.Object.DestroyImmediate(tex);
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
            }
        }

        private static Camera FindPlayerCamera()
        {
            foreach (Camera c in UnityEngine.Object
                         .FindObjectsByType<Camera>(FindObjectsInactive.Include))
            {
                if (c.name == "PlayerCamera")
                    return c;
            }
            throw new InvalidOperationException("PlayerCamera not found in the open scene");
        }

        private static string OutDirFromArgs()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-outDir")
                    return args[i + 1];
            }
            throw new InvalidOperationException("missing -outDir (absolute path) argument");
        }
    }
}
