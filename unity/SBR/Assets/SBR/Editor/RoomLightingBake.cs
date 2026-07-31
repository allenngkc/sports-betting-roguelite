using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace SBR
{
    /// <summary>
    /// PHASE B - bakes the room's Adaptive Probe Volume.
    ///
    /// MUST RUN AFTER <see cref="GrayboxRoomBuilder.Build"/>, never before and never instead.
    /// The builder deletes and recreates Room.unity from scratch on every run, which discards
    /// the ProbeVolumePerSceneData component the bake writes into the scene. Build then bake
    /// is the only valid order; a bake alone against an unbuilt scene bakes nothing, and a
    /// build alone leaves the room lit exactly as it was before this phase.
    ///
    /// This bake is PROBES ONLY - no lightmaps. That is not a shortcut: every mesh in the room
    /// is generated at runtime and carries no UV2, so lightmapping would need an unwrap pass
    /// per mesh. URP's APV path is volumetric and needs no UVs, and the core package's own
    /// bake driver disables lightmaps for this bake type ("Additional only").
    ///
    /// Run headless - note NO -quit and NO -nographics. The bake is driven by
    /// EditorApplication.update so the editor must keep ticking (this exits itself when done),
    /// and dilation runs compute shaders so a graphics device is required:
    ///
    ///   Unity.exe -batchmode -projectPath (project) -executeMethod SBR.RoomLightingBake.Bake
    /// </summary>
    public static class RoomLightingBake
    {
        private const string ScenePath = "Assets/Scenes/Room.unity";
        private const string BakeFolder = "Assets/Scenes/Room";
        private const string BakingSetPath = "Assets/Scenes/Room/RoomBakingSet.asset";

        /// <summary>
        /// Probe spacing in metres at the finest subdivision. The interior is only 2.6 x 4.0m,
        /// so APV's 1.0m default would place roughly three probes across the room's width and
        /// the bounce would be a single flat value - the exact failure this phase exists to
        /// fix. 0.25m resolves the couch corner, the bunk undersides and the window pool
        /// separately from each other, which is where the interesting bounce lives.
        /// </summary>
        private const float ProbeSpacing = 0.25f;

        /// <summary>Wall-clock guard so a stalled bake cannot hang a batch run forever.</summary>
        private const double TimeoutSeconds = 900.0;

        private static double _deadline;

        [MenuItem("SBR/Bake Room Indirect Light")]
        public static void BakeInteractive()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            Prepare();
            if (!AdaptiveProbeVolumes.BakeAsync())
                throw new InvalidOperationException("APV bake refused to start (already baking?)");
        }

        public static void Bake()
        {
            Prepare();

            if (!AdaptiveProbeVolumes.BakeAsync())
            {
                Debug.LogError("[RoomLightingBake] APV bake refused to start");
                EditorApplication.Exit(1);
                return;
            }

            _deadline = EditorApplication.timeSinceStartup + TimeoutSeconds;
            EditorApplication.update += PollUntilDone;
        }

        private static void PollUntilDone()
        {
            if (AdaptiveProbeVolumes.isRunning)
            {
                if (EditorApplication.timeSinceStartup < _deadline)
                    return;

                EditorApplication.update -= PollUntilDone;
                AdaptiveProbeVolumes.Cancel();
                Debug.LogError($"[RoomLightingBake] bake exceeded {TimeoutSeconds}s - cancelled");
                EditorApplication.Exit(2);
                return;
            }

            EditorApplication.update -= PollUntilDone;

            int code = 0;
            try
            {
                Finish();
            }
            catch (Exception e)
            {
                Debug.LogError($"[RoomLightingBake] finish failed: {e}");
                code = 1;
            }

            EditorApplication.Exit(code);
        }

        /// <summary>
        /// Opens the room and puts it in a baking set. A scene that belongs to no set has
        /// nowhere to store baked probe data, so the bake silently produces nothing - which is
        /// the failure mode to watch for if this ever reports success with a black room.
        /// </summary>
        private static void Prepare()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            if (UnityEngine.Object.FindObjectsByType<ProbeVolume>(FindObjectsInactive.Include).Length == 0)
            {
                throw new InvalidOperationException(
                    "no ProbeVolume in the scene - run SBR.GrayboxRoomBuilder.Build first");
            }

            WarmRenderPipeline();

            ProbeVolumeBakingSet set = GetOrCreateBakingSet();

            // The builder recreates the scene asset every run, so re-establish the mapping from
            // scratch rather than trusting whatever GUIDs the set is carrying. TryAddScene is a
            // no-op if the scene already belongs to some set, including a stale one.
            string sceneGuid = AssetDatabase.AssetPathToGUID(ScenePath);
            foreach (string stale in set.sceneGUIDs.Where(g => g != sceneGuid).ToArray())
                set.RemoveScene(stale);
            if (!set.sceneGUIDs.Contains(sceneGuid))
                set.TryAddScene(sceneGuid);

            set.minDistanceBetweenProbes = ProbeSpacing;
            set.simplificationLevels = 3;
            EditorUtility.SetDirty(set);

            ProbeReferenceVolume.instance.SetActiveBakingSet(set);
            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"[RoomLightingBake] baking '{scene.name}' into {BakingSetPath} " +
                      $"at {ProbeSpacing:0.00}m probe spacing");
        }

        /// <summary>
        /// Renders one throwaway frame so the probe system exists before we ask it to bake.
        ///
        /// This is not a warm-up nicety, it is the difference between baking and silently
        /// doing nothing. APV's InitializeBake returns false unless
        /// ProbeReferenceVolume.instance is both initialized and enabled by the SRP, and the
        /// only thing that ever sets either is UniversalRenderPipeline's constructor plus its
        /// render loop (UniversalRenderPipeline.cs:409 and :922). Unity constructs the pipeline
        /// lazily on the first render, and a plain -executeMethod batch run never renders - so
        /// without this the bake refuses to start and reports nothing but a false return value.
        ///
        /// A private throwaway camera rather than the scene's PlayerCamera: it does not matter
        /// what the frame contains, only that a frame happened, and this way the bake cannot be
        /// affected by whatever state the real camera is in.
        /// </summary>
        private static void WarmRenderPipeline()
        {
            var camGo = new GameObject("~ApvBakeWarmup");
            var cam = camGo.AddComponent<Camera>();
            var rt = new RenderTexture(64, 64, 16, RenderTextureFormat.ARGB32);

            try
            {
                cam.targetTexture = rt;
                cam.Render();
            }
            finally
            {
                cam.targetTexture = null;
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
                UnityEngine.Object.DestroyImmediate(camGo);
            }

            Debug.Log("[RoomLightingBake] probe system initialized=" +
                      ProbeReferenceVolume.instance.isInitialized);
        }

        private static ProbeVolumeBakingSet GetOrCreateBakingSet()
        {
            var existing = AssetDatabase.LoadAssetAtPath<ProbeVolumeBakingSet>(BakingSetPath);
            if (existing != null)
                return existing;

            if (!AssetDatabase.IsValidFolder(BakeFolder))
                AssetDatabase.CreateFolder("Assets/Scenes", "Room");

            var set = ScriptableObject.CreateInstance<ProbeVolumeBakingSet>();
            set.name = "RoomBakingSet";

            // SetDefaults is internal to the core runtime assembly. It is not optional - it
            // initialises the baking process settings, registers the default lighting scenario
            // and sets chunkSizeInBricks, and a set without it bakes into an invalid layout.
            // There is no public equivalent, so this is reflection by necessity rather than
            // preference. If it ever disappears, the bake fails loudly here instead of quietly
            // producing an empty set.
            MethodInfo setDefaults = typeof(ProbeVolumeBakingSet)
                .GetMethod("SetDefaults", BindingFlags.Instance | BindingFlags.NonPublic);
            if (setDefaults == null)
            {
                throw new InvalidOperationException(
                    "ProbeVolumeBakingSet.SetDefaults not found - the core package API changed");
            }
            setDefaults.Invoke(set, null);

            AssetDatabase.CreateAsset(set, BakingSetPath);
            return set;
        }

        private static void Finish()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            var set = AssetDatabase.LoadAssetAtPath<ProbeVolumeBakingSet>(BakingSetPath);

            // The bake writes a ProbeVolumePerSceneData component into the scene; without
            // saving, the whole bake is discarded when the editor exits.
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException($"failed to save {ScenePath} after baking");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            int perScene = UnityEngine.Object
                .FindObjectsByType<ProbeVolumePerSceneData>(FindObjectsInactive.Include).Length;

            Debug.Log($"[RoomLightingBake] done: perSceneData={perScene}, " +
                      $"scenesInSet={set.sceneGUIDs.Count}, spacing={ProbeSpacing:0.00}m");
        }
    }
}
