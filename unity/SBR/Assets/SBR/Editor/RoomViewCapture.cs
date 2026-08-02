using System;
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
        private const string OutDirKey = "SBR.RoomViewCapture.OutDir";
        private const int Width = 2560;
        private const int Height = 1440;
        private const int WarmupFrames = 8;

        private static int _frames;

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

            // The seated rig the ruling names, unchanged from the gate pose.
            var seatedEye = new Vector3(-0.950f, 1.150f, 0.300f);
            var tvCenter = new Vector3(1.232f, 1.100f, 0.300f);
            Shoot(cam, outDir, "conformance-seated-screens-dark.png",
                  seatedEye, Quaternion.LookRotation(tvCenter - seatedEye, Vector3.up), 17f);

            // The wide frame that actually carries wall, floor and bunk in one shot.
            var widePos = new Vector3(0.300f, 1.640f, -1.400f);
            var wideRot = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            Shoot(cam, outDir, "conformance-room-screens-dark.png", widePos, wideRot, 68f);

            // DIAGNOSTIC, not canonical. R23 is explicit that the grade IS the room, so the
            // graded frame above is the one the law is judged on. But if that frame ever reads
            // cool, the very next question is whether the cast comes from the room's LIGHT or
            // from the grade's own blue shadow lift, and answering it later costs a whole
            // editor lease. One extra ungraded frame answers it for free, and R18 requires this
            // measurement to be repeated whenever MoonDirectional's colour or intensity moves.
            var volGo = GameObject.Find("RoomPostFx");
            var vol = volGo != null ? volGo.GetComponent<Volume>() : null;
            if (vol != null)
            {
                vol.enabled = false;
                Shoot(cam, outDir, "diagnostic-room-screens-dark-UNGRADED.png", widePos, wideRot, 68f);
                vol.enabled = true;
            }
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

        private static void Capture(string outDir)
        {
            Camera cam = FindPlayerCamera();

            // Stop the controller writing to the transform between our pose and the render.
            var controller = UnityEngine.Object.FindAnyObjectByType<FirstPersonController>();
            if (controller != null)
                controller.enabled = false;

            Shoot(cam, outDir, "standing-overview.png",
                new Vector3(0.300f, 1.640f, -1.400f),
                Quaternion.LookRotation(Vector3.forward, Vector3.up),
                68f);

            // Matches the builder: LookRotation(tvScreenCenter - seatedEye, up).
            var seatedEye = new Vector3(-0.950f, 1.150f, 0.300f);
            var tvCenter = new Vector3(1.232f, 1.100f, 0.300f);
            Shoot(cam, outDir, "seated-tv-couch.png",
                seatedEye,
                Quaternion.LookRotation(tvCenter - seatedEye, Vector3.up),
                17f);

            // Normal to the tilted laptop lid, 0.52m out along its outward normal.
            Shoot(cam, outDir, "focused-laptop-desk.png",
                new Vector3(0.738982f, 1.051217f, 1.620000f),
                Quaternion.LookRotation(new Vector3(0.939693f, -0.342020f, 0f),
                                        new Vector3(0.342020f, 0.939693f, 0f)),
                30f);
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
