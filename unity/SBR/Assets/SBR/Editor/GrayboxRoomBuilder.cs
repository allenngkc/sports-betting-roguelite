using System;
using System.Linq;
using SBR.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace SBR
{
    /// <summary>
    /// M2 graybox room generator (design/08 room spec). Builds Assets/Scenes/Room.unity
    /// from scratch - idempotent: deletes and rebuilds the scene and its materials on
    /// every run, so nothing in the scene is ever hand-authored (design/05 rule).
    ///
    /// Room frame: interior 2.6m wide (X) x 4.0m long (Z) x 2.3m high (Y), centered on
    /// the origin; floor top at y=0; the door end is the -Z short wall. Couch sits along
    /// the LEFT long wall (-X) facing +X; the TV hangs on the RIGHT long wall (+X).
    ///
    /// Run headless:
    ///   Unity.exe -batchmode -quit -projectPath (project) -executeMethod SBR.GrayboxRoomBuilder.Build
    /// </summary>
    public static class GrayboxRoomBuilder
    {
        private const string ScenePath = "Assets/Scenes/Room.unity";
        private const string MaterialsFolder = "Assets/SBR/Materials";
        private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";
        private const string InteractableLayerName = "Interactable";
        // First user slot we try; if occupied by another name we take the next free one.
        private const int PreferredLayerIndex = 6;

        // Interior half-extents.
        private const float HalfW = 1.3f;  // X
        private const float HalfL = 2.0f;  // Z
        private const float Height = 2.3f; // Y
        private const float WallT = 0.1f;

        [MenuItem("SBR/Build Graybox Room")]
        public static void Build()
        {
            if (!Application.isBatchMode &&
                !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (inputActions == null)
                throw new InvalidOperationException($"input actions asset missing at {InputActionsPath}");

            int layer = EnsureInteractableLayer();
            Materials mats = BuildMaterials();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
                AssetDatabase.DeleteAsset(ScenePath);

            BuildShell(mats);
            BuildCouch(mats, layer);
            BuildTv(mats, inputActions);
            BuildWindow(mats);
            BuildDeskCluster(mats, layer);
            var interactor = BuildPlayer(inputActions, layer);
            BuildHud(interactor);
            BuildLighting();

            if (!System.IO.Directory.Exists("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException($"failed to save scene to {ScenePath}");

            RegisterInBuildSettings();
            AssetDatabase.SaveAssets();

            int interactables = UnityEngine.Object
                .FindObjectsByType<Interactable>().Length;
            Debug.Log($"[GrayboxRoomBuilder] built {ScenePath}: layer '{InteractableLayerName}'={layer}, " +
                      $"interactables={interactables} (expect 3: couch, laptop, phone - the TV is the live " +
                      "sweat surface in M3, no longer interactable)");
        }

        // ------------------------------------------------------------------ layer

        private static int EnsureInteractableLayer()
        {
            int existing = LayerMask.NameToLayer(InteractableLayerName);
            if (existing != -1)
                return existing;

            var tagManagerAsset = AssetDatabase
                .LoadAllAssetsAtPath("ProjectSettings/TagManager.asset").FirstOrDefault();
            if (tagManagerAsset == null)
                throw new InvalidOperationException("could not load ProjectSettings/TagManager.asset");

            var serialized = new SerializedObject(tagManagerAsset);
            SerializedProperty layers = serialized.FindProperty("layers");

            int target = -1;
            for (int i = PreferredLayerIndex; i < layers.arraySize; i++)
            {
                if (string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue))
                {
                    target = i;
                    break;
                }
            }
            if (target == -1)
                throw new InvalidOperationException("no free layer slot for 'Interactable'");

            layers.GetArrayElementAtIndex(target).stringValue = InteractableLayerName;
            serialized.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            Debug.Log($"[GrayboxRoomBuilder] created layer '{InteractableLayerName}' at index {target}");
            return target;
        }

        // -------------------------------------------------------------- materials

        private struct Materials
        {
            public Material Wall;
            public Material Prop;
            public Material Couch;
            public Material Bezel;
            public Material TvScreen;
            public Material LaptopScreen;
            public Material PhoneScreen;
            public Material WindowGlow;
        }

        private static Materials BuildMaterials()
        {
            if (!AssetDatabase.IsValidFolder("Assets/SBR"))
                AssetDatabase.CreateFolder("Assets", "SBR");
            if (!AssetDatabase.IsValidFolder(MaterialsFolder))
                AssetDatabase.CreateFolder("Assets/SBR", "Materials");

            return new Materials
            {
                // near-black #1A1A1E so screens can glow against it
                Wall = Mat("WallDark", new Color32(0x1A, 0x1A, 0x1E, 0xFF)),
                Prop = Mat("PropGray", new Color(0.18f, 0.18f, 0.20f)),
                Couch = Mat("CouchGray", new Color(0.20f, 0.19f, 0.17f)),
                Bezel = Mat("BezelBlack", new Color(0.05f, 0.05f, 0.06f)),
                TvScreen = Mat("ScreenTV", new Color(0.01f, 0.02f, 0.015f),
                    emission: new Color(0.010f, 0.045f, 0.020f), doubleSided: true),
                LaptopScreen = Mat("ScreenLaptop", new Color(0.01f, 0.02f, 0.015f),
                    emission: new Color(0.025f, 0.055f, 0.035f), doubleSided: true),
                PhoneScreen = Mat("ScreenPhone", new Color(0.01f, 0.015f, 0.02f),
                    emission: new Color(0.020f, 0.030f, 0.060f), doubleSided: true),
                WindowGlow = Mat("WindowGlow", new Color(0.01f, 0.02f, 0.05f),
                    emission: new Color(0.040f, 0.080f, 0.220f), doubleSided: true),
            };
        }

        private static Material Mat(string name, Color baseColor,
                                    Color? emission = null, bool doubleSided = false)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                throw new InvalidOperationException("URP Lit shader not found - is URP active?");

            string path = $"{MaterialsFolder}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.shader = shader;
            }

            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Smoothness", 0.15f);
            if (emission.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                mat.SetColor("_EmissionColor", emission.Value);
            }
            else
            {
                mat.DisableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.black);
            }
            if (doubleSided)
                mat.SetFloat("_Cull", (float)CullMode.Off); // facing mistakes never blank a screen
            EditorUtility.SetDirty(mat);
            return mat;
        }

        // ------------------------------------------------------------------ shell

        private static void BuildShell(Materials mats)
        {
            Transform root = new GameObject("RoomShell").transform;
            float wallY = Height * 0.5f;             // 1.15
            float span = 2f * HalfW + 2f * WallT;    // 2.8 - short walls overlap the long-wall ends

            Box("Floor", root, new Vector3(0f, -WallT * 0.5f, 0f),
                new Vector3(span, WallT, 2f * HalfL + 2f * WallT), mats.Wall);
            Box("Ceiling", root, new Vector3(0f, Height + WallT * 0.5f, 0f),
                new Vector3(span, WallT, 2f * HalfL + 2f * WallT), mats.Wall);
            Box("WallLeft", root, new Vector3(-(HalfW + WallT * 0.5f), wallY, 0f),
                new Vector3(WallT, Height, 2f * HalfL), mats.Wall);
            Box("WallRight", root, new Vector3(HalfW + WallT * 0.5f, wallY, 0f),
                new Vector3(WallT, Height, 2f * HalfL), mats.Wall);
            Box("WallFar", root, new Vector3(0f, wallY, HalfL + WallT * 0.5f),
                new Vector3(span, Height, WallT), mats.Wall);
            Box("WallNearDoor", root, new Vector3(0f, wallY, -(HalfL + WallT * 0.5f)),
                new Vector3(span, Height, WallT), mats.Wall);
        }

        // ------------------------------------------------------------------ couch

        private static void BuildCouch(Materials mats, int layer)
        {
            var root = new GameObject("Couch");

            // Lower bunk volume = the couch: seat 0.42 high, 0.7 deep, 1.8 long, facing +X.
            GameObject seat = Box("CouchSeat", root.transform,
                new Vector3(-0.95f, 0.21f, 0.3f), new Vector3(0.7f, 0.42f, 1.8f), mats.Couch);
            GameObject back = Box("CouchBackrest", root.transform,
                new Vector3(-1.225f, 0.62f, 0.3f), new Vector3(0.15f, 0.4f, 1.8f), mats.Couch);
            // Upper bunk slab, underside at 1.5m.
            Box("BunkSlab", root.transform,
                new Vector3(-0.9f, 1.54f, 0.3f), new Vector3(0.8f, 0.08f, 1.9f), mats.Prop);
            Box("BunkPostFront", root.transform,
                new Vector3(-0.53f, 0.77f, -0.62f), new Vector3(0.06f, 1.54f, 0.06f), mats.Prop);
            Box("BunkPostBack", root.transform,
                new Vector3(-0.53f, 0.77f, 1.22f), new Vector3(0.06f, 1.54f, 0.06f), mats.Prop);

            // Forgiving hover volume over the whole seat (trigger: no physics blocking).
            var hoverVolume = root.AddComponent<BoxCollider>();
            hoverVolume.center = new Vector3(-0.95f, 0.6f, 0.3f);
            hoverVolume.size = new Vector3(0.8f, 1.2f, 1.8f);
            hoverVolume.isTrigger = true;

            var sit = root.AddComponent<SitSpot>();
            var anchor = new GameObject("SeatAnchor").transform;
            anchor.SetParent(root.transform, false);
            // Seated eye ~1.15m, base rotation aimed at the TV screen's CENTER (keep in sync with
            // BuildTv), not just +X - at the seated zoom FOV even the 5cm eye-vs-center offset shows.
            Vector3 seatedEye = new Vector3(-0.95f, 1.15f, 0.3f);
            Vector3 tvScreenCenter = new Vector3(1.232f, 1.1f, 0.3f);
            anchor.SetPositionAndRotation(seatedEye,
                Quaternion.LookRotation(tvScreenCenter - seatedEye, Vector3.up));
            sit.seatAnchor = anchor;
            // Playtest #4: seated view should hold "just the TV". At 2.18m the 0.98x0.55 screen
            // subtends ~25x14 degrees; 17 degrees vertical FOV fills ~85% of the view with a slim
            // frame of room so the TvLight reaction shot still reads at the edges.
            sit.seatedFov = 17f;
            sit.highlightRenderers = new[]
            {
                seat.GetComponent<Renderer>(),
                back.GetComponent<Renderer>(),
            };

            SetLayerRecursive(root, layer);
        }

        // --------------------------------------------------------------------- tv

        private static void BuildTv(Materials mats, InputActionAsset inputActions)
        {
            var root = new GameObject("TV");

            // Wall-mounted on the right long wall, center at seated eye height (~1.1m).
            Box("TVBody", root.transform,
                new Vector3(1.265f, 1.1f, 0.3f), new Vector3(0.06f, 0.65f, 1.1f), mats.Bezel);
            GameObject screen = Quad("TVScreen", root.transform,
                new Vector3(1.232f, 1.1f, 0.3f), new Vector2(0.98f, 0.55f),
                Vector3.left, Vector3.up, mats.TvScreen);

            // M3: the TV is the live sweat surface, no longer interactable (the ScreenStub is gone and the
            // TV stays on the default layer). TvSweatScreen hangs a world-space canvas on the screen inset
            // in front of this emissive quad and steps the real engine's SweatSession while seated.
            var tv = root.AddComponent<TvSweatScreen>();
            tv.emissiveScreen = screen.GetComponent<Renderer>();
            tv.actions = inputActions;
            tv.screenWorldSize = new Vector2(0.98f, 0.55f);

            // TvLight: the room is the reaction shot (design/08). A point light just off the TV, driven by
            // the screen state (phosphor idle, green flare, red wash, gold pulse).
            var lightGo = new GameObject("TvLight");
            lightGo.transform.position = new Vector3(1.05f, 1.15f, 0.3f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 3.2f;
            light.intensity = 0.5f;
            light.color = new Color(0.35f, 1f, 0.5f);
            light.shadows = LightShadows.None;
            var tvLight = lightGo.AddComponent<TvLight>();
            tvLight.pointLight = light;
            tv.tvLight = tvLight;

            // DemoRunDriver: the endless demo channel that feeds the screen real tickets (M4 replaces it).
            var driverGo = new GameObject("DemoRunDriver");
            var driver = driverGo.AddComponent<DemoRunDriver>();
            driver.screen = tv;
            tv.driver = driver;
        }

        // ----------------------------------------------------------------- window

        private static void BuildWindow(Materials mats)
        {
            Transform root = new GameObject("Window").transform;
            // Night outside: emissive dark-blue quad on the far short wall. Mood only in M2.
            Quad("WindowPane", root, new Vector3(0f, 1.4f, HalfL - 0.01f),
                new Vector2(1.2f, 1.0f), Vector3.back, Vector3.up, mats.WindowGlow);
        }

        // ----------------------------------------------------- desk / laptop / phone

        private static void BuildDeskCluster(Materials mats, int layer)
        {
            Transform deskRoot = new GameObject("Desk").transform;

            // Desk 1.1 long x 0.75 high x 0.5 deep against the far end of the right wall.
            Box("DeskTop", deskRoot, new Vector3(1.05f, 0.73f, 1.45f),
                new Vector3(0.5f, 0.04f, 1.1f), mats.Prop);
            Box("DeskLegA", deskRoot, new Vector3(0.855f, 0.355f, 0.955f),
                new Vector3(0.05f, 0.71f, 0.05f), mats.Prop);
            Box("DeskLegB", deskRoot, new Vector3(1.245f, 0.355f, 0.955f),
                new Vector3(0.05f, 0.71f, 0.05f), mats.Prop);
            Box("DeskLegC", deskRoot, new Vector3(0.855f, 0.355f, 1.945f),
                new Vector3(0.05f, 0.71f, 0.05f), mats.Prop);
            Box("DeskLegD", deskRoot, new Vector3(1.245f, 0.355f, 1.945f),
                new Vector3(0.05f, 0.71f, 0.05f), mats.Prop);

            Box("Stool", null, new Vector3(0.55f, 0.225f, 1.45f),
                new Vector3(0.35f, 0.45f, 0.35f), mats.Prop);
            // Door-end left corner, ~1m left of the player spawn (playtest #3: the old spot
            // by the desk collided with the stool).
            Box("MiniFridge", null, new Vector3(-0.95f, 0.425f, -1.65f),
                new Vector3(0.5f, 0.85f, 0.5f), mats.Prop);

            BuildLaptop(mats, layer);
            BuildPhone(mats, layer);
        }

        private static void BuildLaptop(Materials mats, int layer)
        {
            var root = new GameObject("Laptop");
            root.transform.position = new Vector3(1.15f, 0.85f, 1.62f);

            GameObject lapBase = Box("LaptopBase", root.transform,
                new Vector3(1.08f, 0.76f, 1.62f), new Vector3(0.22f, 0.02f, 0.32f), mats.Bezel);

            // Lid: 0.32 x 0.22 quad hinged on the wall-side edge of the base, tilted 20
            // degrees back toward the wall (+X), screen facing the room (-X and up).
            const float lidTiltDeg = 20f;
            Vector3 hinge = new Vector3(1.19f, 0.77f, 1.62f);
            Vector3 widthDir = Vector3.forward; // hinge axis runs along Z
            Vector3 lidUp = Quaternion.AngleAxis(-lidTiltDeg, Vector3.forward) * Vector3.up;
            Vector3 lidNormal = Vector3.Cross(widthDir, lidUp); // -X and slightly up
            Vector3 lidCenter = hinge + lidUp * 0.11f;
            GameObject lid = Quad("LaptopScreen", root.transform, lidCenter,
                new Vector2(0.32f, 0.22f), lidNormal, lidUp, mats.LaptopScreen);

            // Generous interaction volume - the laptop itself is too small to raycast comfortably.
            var grabVolume = root.AddComponent<BoxCollider>();
            grabVolume.center = Vector3.zero;
            grabVolume.size = new Vector3(0.45f, 0.4f, 0.5f);
            grabVolume.isTrigger = true;

            var stub = root.AddComponent<ScreenStub>();
            stub.prompt = "Use laptop";
            stub.screenRenderer = lid.GetComponent<Renderer>();
            stub.idleEmission = new Color(0.025f, 0.055f, 0.035f);
            stub.pulseEmission = new Color(0.12f, 1.1f, 0.35f);
            stub.highlightRenderers = new[] { lapBase.GetComponent<Renderer>() };

            SetLayerRecursive(root, layer);
        }

        private static void BuildPhone(Materials mats, int layer)
        {
            var root = new GameObject("Phone");
            root.transform.position = new Vector3(1.0f, 0.80f, 1.15f);

            GameObject body = Box("PhoneBody", root.transform,
                new Vector3(1.0f, 0.754f, 1.15f), new Vector3(0.075f, 0.008f, 0.15f), mats.Bezel);
            GameObject screen = Quad("PhoneScreen", root.transform,
                new Vector3(1.0f, 0.759f, 1.15f), new Vector2(0.065f, 0.135f),
                Vector3.up, Vector3.forward, mats.PhoneScreen);

            var grabVolume = root.AddComponent<BoxCollider>();
            grabVolume.center = Vector3.zero;
            grabVolume.size = new Vector3(0.22f, 0.15f, 0.3f);
            grabVolume.isTrigger = true;

            var stub = root.AddComponent<ScreenStub>();
            stub.prompt = "Check phone";
            stub.screenRenderer = screen.GetComponent<Renderer>();
            stub.idleEmission = new Color(0.020f, 0.030f, 0.060f);
            stub.pulseEmission = new Color(0.30f, 0.50f, 0.90f); // dim cyan/white chrome, not money-green
            stub.highlightRenderers = new[] { body.GetComponent<Renderer>() };

            SetLayerRecursive(root, layer);
        }

        // ----------------------------------------------------------------- player

        private static PlayerInteractor BuildPlayer(InputActionAsset inputActions, int layer)
        {
            var player = new GameObject("Player");
            player.transform.position = new Vector3(0.3f, 0.02f, -1.4f); // by the door end, facing +Z

            var cc = player.AddComponent<CharacterController>();
            cc.radius = 0.3f;
            cc.height = 1.7f;
            cc.center = new Vector3(0f, 0.85f, 0f);
            cc.stepOffset = 0.2f;
            cc.skinWidth = 0.02f;

            var camGo = new GameObject("PlayerCamera");
            camGo.transform.SetParent(player.transform, false);
            camGo.transform.localPosition = new Vector3(0f, 1.62f, 0f); // standing eye height
            var cam = camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
            cam.nearClipPlane = 0.05f; // compact room - default 0.3 clips walls
            cam.fieldOfView = 68f;
            camGo.AddComponent<AudioListener>();

            var controller = player.AddComponent<FirstPersonController>();
            controller.actions = inputActions;
            controller.cameraTransform = camGo.transform;

            var interactor = player.AddComponent<PlayerInteractor>();
            interactor.actions = inputActions;
            interactor.rayOrigin = camGo.transform;
            interactor.interactableMask = 1 << layer;
            interactor.range = 2.6f; // couch-to-TV is 2.18m; spec's 2.2 misses edge-of-screen aims

            return interactor;
        }

        private static void BuildHud(PlayerInteractor interactor)
        {
            var hudGo = new GameObject("InteractionHud");
            var hud = hudGo.AddComponent<InteractionHud>();
            hud.interactor = interactor;
        }

        // --------------------------------------------------------------- lighting

        private static void BuildLighting()
        {
            var moonGo = new GameObject("MoonDirectional");
            moonGo.transform.rotation = Quaternion.Euler(50f, 200f, 0f); // rakes in from the window end
            var moon = moonGo.AddComponent<Light>();
            moon.type = LightType.Directional;
            moon.intensity = 0.25f;
            moon.color = new Color(0.60f, 0.68f, 0.90f);
            moon.shadows = LightShadows.Soft;
            moon.shadowStrength = 0.9f;

            var windowGo = new GameObject("WindowGlowLight");
            windowGo.transform.position = new Vector3(0f, 1.5f, 1.6f);
            var glow = windowGo.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.intensity = 0.7f;
            glow.range = 3.5f;
            glow.color = new Color(0.50f, 0.62f, 1.00f);

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.050f, 0.055f, 0.075f);
        }

        // ---------------------------------------------------------------- helpers

        private static void RegisterInBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static GameObject Box(string name, Transform parent, Vector3 center,
                                      Vector3 size, Material mat, int layer = 0)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            if (parent != null)
                go.transform.SetParent(parent, true);
            go.transform.position = center;
            go.transform.localScale = size;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            go.layer = layer;
            return go;
        }

        private static GameObject Quad(string name, Transform parent, Vector3 center,
                                       Vector2 size, Vector3 facing, Vector3 up, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
            if (parent != null)
                go.transform.SetParent(parent, true);
            go.transform.position = center;
            // Unity's Quad primitive renders on its local -Z side; aim local -Z along 'facing'.
            // Screen materials are Cull Off, so a flipped face still renders either way.
            go.transform.rotation = Quaternion.LookRotation(-facing.normalized, up);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        private static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
                SetLayerRecursive(child.gameObject, layer);
        }
    }
}
