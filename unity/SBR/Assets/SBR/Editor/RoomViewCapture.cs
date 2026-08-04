using System;
using System.IO;
using SBR.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

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
