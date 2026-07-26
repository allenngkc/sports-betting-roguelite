using System;
using System.Linq;
using SBR.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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
        private const string EnvironmentFolder = "Assets/SBR/Environment";
        private const string EnvPrefabsFolder = "Assets/SBR/Environment/Prefabs";
        private const string EnvPostFxFolder = "Assets/SBR/Environment/PostFx";
        private const string ArtRootPrefabPath = "Assets/SBR/Environment/Prefabs/RoomArtRoot.prefab";
        private const string RoomVolumeProfilePath = "Assets/SBR/Environment/PostFx/RoomVolume.asset";
        private const string ArtRootName = "RoomArtRoot";
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
            RunDirector director = BuildTv(mats, inputActions);
            BuildWindow(mats);
            BuildDeskCluster(mats, layer, director);
            var interactor = BuildPlayer(inputActions, layer);
            BuildHud(interactor);
            BuildEventSystem();
            BuildLighting();
            BuildPostFx();
            EnsureArtRootPrefab();
            InstantiateArtRoot();
            RoomArtDressing.Build();

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
            public Material Floor;
            public Material Ceiling;
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
            EnsureEnvironmentFolders();

            // Vice Grip palette (room-visual-pass, direction B). Asset names are historical -
            // the struct field is the role. The old single near-black #1A1A1E shared by floor,
            // ceiling and all four walls is why the graybox read as an undifferentiated box:
            // with no value or hue separation between surfaces, no amount of light helps.
            // Albedo now sits in the mid-dark range so the fluorescent key has something to
            // land on, and smoothness varies per surface to do material work for free.
            return new Materials
            {
                // Phase 4: surface maps. Tiling is repeats-per-metre thanks to the world-scale
                // UVs on the chamfered meshes, so these numbers are real physical sizes: plaster
                // mottling every 1.3m, floor wear every 1.7m, ceiling blotches every 2m, and a
                // fabric weave every 17cm. Base colours stay as the tint the maps modulate.
                // 4b: wall base reverted to its pre-map value - compensating for the map's
                // darkening made the walls the brightest thing in frame, which flattened the
                // whole room. The maps are meant to darken; that is the point of them.
                // 4c: Allen playtested and called the grime "slightly too soft". Contrast raised
                // rather than adding geometry - ApplyContrast pivots on each map's own mean, so
                // pushing it darkens the dirt without darkening the surface overall. Highlights
                // clip slightly at these values, which is wanted: clean stays uniformly clean and
                // dirty gets properly dirty, instead of everything sitting in a soft mid-band.
                Wall = Mat("WallDark", new Color(0.255f, 0.245f, 0.210f), smoothness: 0.08f,
                    baseMap: Tex(ProceduralSurfaceTextures.SurfaceKind.Plaster, 1024, 2.10f),
                    tiling: 0.75f),
                // 4b: lifted and warmed so the floor belongs to the same room as the walls -
                // it was reading as a cold blue-grey slab under warm plaster.
                // Worn but INTACT, not the derelict full-coverage flaking of the concept.
                Floor = Mat("FloorWorn", new Color(0.185f, 0.166f, 0.134f), smoothness: 0.22f,
                    baseMap: Tex(ProceduralSurfaceTextures.SurfaceKind.WornFloor, 1024, 2.00f),
                    tiling: 0.60f),
                // sits between wall and floor; takes the tube's uplight and the stain story
                Ceiling = Mat("CeilingStained", new Color(0.208f, 0.198f, 0.166f), smoothness: 0.06f,
                    baseMap: Tex(ProceduralSurfaceTextures.SurfaceKind.CeilingStain, 1024, 1.80f),
                    tiling: 0.50f),
                Prop = Mat("PropGray", new Color(0.180f, 0.178f, 0.160f), smoothness: 0.35f),
                // 4b: lifted so the weave actually reads - the couch was dark enough that its
                // texture was invisible, which wasted the one fabric map in the room.
                Couch = Mat("CouchGray", new Color(0.172f, 0.158f, 0.132f), smoothness: 0.04f,
                    baseMap: Tex(ProceduralSurfaceTextures.SurfaceKind.FabricWeave, 512, 1.50f),
                    tiling: 6.0f),
                Bezel = Mat("BezelBlack", new Color(0.045f, 0.045f, 0.040f), smoothness: 0.25f),
                TvScreen = Mat("ScreenTV", new Color(0.01f, 0.02f, 0.015f),
                    emission: new Color(0.010f, 0.045f, 0.020f), doubleSided: true),
                LaptopScreen = Mat("ScreenLaptop", new Color(0.01f, 0.02f, 0.015f),
                    emission: new Color(0.025f, 0.055f, 0.035f), doubleSided: true),
                PhoneScreen = Mat("ScreenPhone", new Color(0.01f, 0.015f, 0.02f),
                    emission: new Color(0.020f, 0.030f, 0.060f), doubleSided: true),
                // The pane is no longer a flat blue rectangle: it carries a generated night-city
                // view as both base and emission map, so the window reads as somewhere the
                // player is not. Base colour goes white so the texture is not tinted away, and
                // the quad's UVs are 0..1 across the pane so tiling stays at 1.
                WindowGlow = Mat("WindowGlow", Color.white,
                    emission: new Color(0.80f, 0.80f, 0.86f), doubleSided: true,
                    baseMap: RoomArtDressing.GetOrCreateNightCity(512, 512, 20260725),
                    emissionMap: RoomArtDressing.GetOrCreateNightCity(512, 512, 20260725)),
            };
        }

        private static Texture2D Tex(ProceduralSurfaceTextures.SurfaceKind kind, int res,
                                     float contrast = 1f) =>
            ProceduralSurfaceTextures.GetOrCreate(kind, res, 20260725, contrast);

        // internal so RoomArtDressing can author its own dressing materials through the same
        // deterministic path rather than duplicating the URP/Lit setup.
        internal static Material Mat(string name, Color baseColor,
                                    Color? emission = null, bool doubleSided = false,
                                    float smoothness = 0.15f,
                                    Texture2D baseMap = null, Texture2D emissionMap = null,
                                    float tiling = 1f)
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
            mat.SetFloat("_Smoothness", smoothness);
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
            // Box meshes carry world-scale UVs (1 unit = 1 UV), so tiling is literally "repeats
            // per metre" and texel density stays uniform across every surface in the room.
            if (baseMap != null)
            {
                mat.SetTexture("_BaseMap", baseMap);
                mat.SetTextureScale("_BaseMap", new Vector2(tiling, tiling));
            }
            if (emissionMap != null)
            {
                mat.SetTexture("_EmissionMap", emissionMap);
                mat.SetTextureScale("_EmissionMap", new Vector2(tiling, tiling));
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
                new Vector3(span, WallT, 2f * HalfL + 2f * WallT), mats.Floor);
            Box("Ceiling", root, new Vector3(0f, Height + WallT * 0.5f, 0f),
                new Vector3(span, WallT, 2f * HalfL + 2f * WallT), mats.Ceiling);
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

        private static RunDirector BuildTv(Materials mats, InputActionAsset inputActions)
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

            // RunDirector (M4): the single owner of the engine Run. The TV walks its sweats; the
            // laptop drives betting/shop/new-run through it.
            var directorGo = new GameObject("RunDirector");
            var director = directorGo.AddComponent<RunDirector>();
            tv.director = director;

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

            return director;
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

        private static void BuildDeskCluster(Materials mats, int layer, RunDirector director)
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

            BuildLaptop(mats, layer, director);
            BuildPhone(mats, layer, director);
        }

        private static void BuildLaptop(Materials mats, int layer, RunDirector director)
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

            // The laptop IS the book. DeskFocus glides the camera to the lid and frees the cursor;
            // LaptopScreen hosts the code-built OS and its SureThing app on a world-space canvas.
            var focus = root.AddComponent<DeskFocus>();
            focus.highlightRenderers = new[] { lapBase.GetComponent<Renderer>() };

            // Focus anchor: in front of the lid along its outward normal, looking at its center.
            // The Quad primitive's visible face is local -Z, so outward = -lid.forward.
            Transform lidT = lid.transform;
            Vector3 outward = -lidT.forward;
            var anchor = new GameObject("FocusAnchor").transform;
            anchor.SetParent(root.transform, false);
            anchor.SetPositionAndRotation(
                lidT.position + outward * LaptopFocusDistance,
                Quaternion.LookRotation(-outward, lidT.up));
            focus.focusAnchor = anchor;

            var book = root.AddComponent<LaptopScreen>();
            book.director = director;
            book.tv = UnityEngine.Object.FindAnyObjectByType<TvSweatScreen>();
            book.lidRenderer = lid.GetComponent<Renderer>();

            SetLayerRecursive(root, layer);
        }

        // Frames the 0.32x0.22 lid at ~80% of the 30-degree focus FOV.
        private const float LaptopFocusDistance = 0.52f;

        private static void BuildPhone(Materials mats, int layer, RunDirector director)
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

            // M5: the phone is the bookie's voice. It gets its own hardened DeskFocus rather than a
            // stub, with a top-down pose whose up is the quad's screen-up (+Z, away from the player).
            var focus = root.AddComponent<DeskFocus>();
            focus.prompt = "Check phone";
            focus.focusFov = 30f;
            focus.highlightRenderers = new[] { body.GetComponent<Renderer>() };

            Transform screenT = screen.transform;
            var anchor = new GameObject("FocusAnchor").transform;
            anchor.SetParent(root.transform, false);
            anchor.SetPositionAndRotation(
                screenT.position + Vector3.up * 0.30f,
                Quaternion.LookRotation(Vector3.down, screenT.up));
            focus.focusAnchor = anchor;

            var feed = root.AddComponent<BookieFeed>();
            feed.director = director;
            feed.phoneFocus = focus;

            // A tiny cyan/white blink is chrome, never money-green (design/08 palette law).
            var lightGo = new GameObject("PhoneBuzzLight");
            lightGo.transform.SetParent(root.transform, false);
            lightGo.transform.position = screenT.position + Vector3.up * 0.035f;
            var buzzLight = lightGo.AddComponent<Light>();
            buzzLight.type = LightType.Point;
            buzzLight.range = 0.55f;
            buzzLight.intensity = 0f;
            buzzLight.color = new Color(0.55f, 0.82f, 1.0f);
            buzzLight.shadows = LightShadows.None;
            buzzLight.enabled = false;

            var phone = root.AddComponent<PhoneScreen>();
            phone.feed = feed;
            phone.screenRenderer = screen.GetComponent<Renderer>();
            phone.buzzLight = buzzLight;

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

            // URP ships post-processing wired on PC_Renderer but every camera defaults to
            // ignoring it, which is why the graybox had no tonemapping, bloom or grade at all.
            // Without this the Vice Grip lighting has nowhere to land.
            var camData = cam.GetUniversalAdditionalCameraData();
            camData.renderPostProcessing = true;
            camData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            camData.antialiasingQuality = AntialiasingQuality.High;

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

        private static void BuildEventSystem()
        {
            // The laptop's UGUI buttons need an event system; the input-system UI module reads the
            // same devices the Player map uses (its own default UI actions).
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<InputSystemUIInputModule>();
        }

        // --------------------------------------------------------------- lighting

        private static void BuildLighting()
        {
            // Vice Grip rig: the failing fluorescent over the desk is the KEY, not the moon.
            // The story is interior and electrical - a fixture nobody is coming to replace.
            // TvLight (built in BuildTv, driven by TvLight.cs) is not ours to touch; the tube
            // is deliberately yellow-leaning so it separates from that light's pure green
            // rather than compounding into one flat green room.

            // Reduced to a shape-defining rim so silhouettes read against the far wall.
            var moonGo = new GameObject("MoonDirectional");
            moonGo.transform.rotation = Quaternion.Euler(50f, 200f, 0f); // rakes in from the window end
            var moon = moonGo.AddComponent<Light>();
            moon.type = LightType.Directional;
            moon.intensity = 0.14f; // 4b: less global lift, let the tube own the room
            moon.color = new Color(0.55f, 0.62f, 0.85f);
            moon.shadows = LightShadows.Soft;
            moon.shadowStrength = 0.75f;

            // The tube: high on the right wall over the desk, throwing down and across the
            // room. The one shadow-caster - hard contact shadows are the direction's signature.
            // Tuning pass 1: at 3.2 / 118deg the tube read as a faint patch on the ceiling and
            // TvLight's green owned the room. A wide cone spreads energy too thin in a 2.6m
            // box, and sitting at z=1.05 it lit only the far end - the standing camera is at
            // z=-1.4 looking down the whole 4m. Narrower, stronger, pulled toward the middle.
            var tubeGo = new GameObject("FluorescentKey");
            tubeGo.transform.position = new Vector3(0.95f, 2.06f, 0.85f);
            tubeGo.transform.rotation =
                Quaternion.LookRotation(new Vector3(-0.52f, -0.82f, -0.24f).normalized, Vector3.up);
            var tube = tubeGo.AddComponent<Light>();
            tube.type = LightType.Spot;
            // 4b: 96deg spread the energy so evenly the room read as uniformly lit rather than
            // pooled. Narrower and brighter gives a defined pool with fast falloff, which is
            // the compressed, oppressive read the direction is built on.
            tube.spotAngle = 78f;
            tube.innerSpotAngle = 30f;
            tube.intensity = 11.0f;
            tube.range = 7f;
            tube.color = new Color(0.92f, 0.86f, 0.42f);
            tube.shadows = LightShadows.Soft;
            tube.shadowStrength = 0.85f;

            // Unshadowed spill so the wall and ceiling immediately behind the fixture do not
            // fall dead - a bare spot cone leaves its own mounting wall black.
            var bounceGo = new GameObject("FluorescentBounce");
            bounceGo.transform.position = new Vector3(0.85f, 1.95f, 1.20f);
            var bounce = bounceGo.AddComponent<Light>();
            bounce.type = LightType.Point;
            bounce.intensity = 0.90f;
            bounce.range = 3.4f;
            bounce.color = new Color(0.85f, 0.80f, 0.45f);
            bounce.shadows = LightShadows.None;

            // The window is a lit pane onto a night city, not a blue floodlight. Dim enough
            // that the tube keeps the room, bright enough that the pane never reads boarded.
            var windowGo = new GameObject("WindowGlowLight");
            windowGo.transform.position = new Vector3(0f, 1.45f, 1.72f);
            var glow = windowGo.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.intensity = 0.65f;
            glow.range = 3.0f;
            glow.color = new Color(0.46f, 0.54f, 0.72f);
            glow.shadows = LightShadows.None;

            // Gradient ambient does surface separation a single flat value cannot: the ceiling
            // catches the tube, the floor stays dirty and warm, the walls sit between. This is
            // the other half of the fix for the undifferentiated-box problem.
            // 4b: dropped ~38%. These were tuned in Phase 2 when the room was flat, untextured
            // and had no dressing - at that point ambient was doing the work of making anything
            // read at all. With surface maps, conduit and a lit window carrying the frame, the
            // same values over-lift everything and kill the falloff.
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.090f, 0.087f, 0.061f);
            RenderSettings.ambientEquatorColor = new Color(0.057f, 0.057f, 0.048f);
            RenderSettings.ambientGroundColor = new Color(0.036f, 0.032f, 0.025f);
        }

        // ------------------------------------------------------------- post-effects

        /// <summary>
        /// The room's global post-processing volume. URP wires post-process data on
        /// PC_Renderer but nothing in this project ever enabled it, so the graybox rendered
        /// with no tonemapping, no bloom and no grade - raw linear output straight to screen.
        /// The profile is a persistent asset under Environment/ so later hand-tuning survives,
        /// but the builder still asserts every value it cares about on each rebuild.
        /// </summary>
        private static void BuildPostFx()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(RoomVolumeProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, RoomVolumeProfilePath);
            }

            // Neutral, not ACES: ACES desaturates the fluorescent's sick yellow-green into
            // something closer to white, which is the one colour this direction cannot lose.
            var tone = GetOrAddVolumeComponent<Tonemapping>(profile);
            tone.mode.overrideState = true;
            tone.mode.value = TonemappingMode.Neutral;

            var bloom = GetOrAddVolumeComponent<Bloom>(profile);
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 0.75f;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 0.90f;
            bloom.scatter.overrideState = true;
            bloom.scatter.value = 0.62f;

            var grade = GetOrAddVolumeComponent<ColorAdjustments>(profile);
            grade.postExposure.overrideState = true;
            grade.postExposure.value = 0.35f;
            grade.contrast.overrideState = true;
            grade.contrast.value = 16f;
            grade.saturation.overrideState = true;
            grade.saturation.value = 4f;

            // Compression, cheaply. The room should feel like it is closing in at the edges.
            var vignette = GetOrAddVolumeComponent<Vignette>(profile);
            vignette.intensity.overrideState = true;
            vignette.intensity.value = 0.28f;
            vignette.smoothness.overrideState = true;
            vignette.smoothness.value = 0.50f;

            // Grime that costs nothing and reads at every camera distance.
            var grain = GetOrAddVolumeComponent<FilmGrain>(profile);
            grain.type.overrideState = true;
            grain.type.value = FilmGrainLookup.Medium1;
            grain.intensity.overrideState = true;
            grain.intensity.value = 0.22f;
            grain.response.overrideState = true;
            grain.response.value = 0.75f;

            EditorUtility.SetDirty(profile);

            var volGo = new GameObject("RoomPostFx");
            var volume = volGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;
            volume.sharedProfile = profile;
        }

        private static T GetOrAddVolumeComponent<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (profile.TryGet(out T existing))
                return existing;

            T added = profile.Add<T>();
            added.name = typeof(T).Name;
            AssetDatabase.AddObjectToAsset(added, profile);
            return added;
        }

        // ---------------------------------------------------------------- art root

        /// <summary>
        /// The persistent authoring surface. Build() recreates Room.unity from nothing every
        /// run, so any dressing authored directly into the scene is destroyed. RoomArtRoot is
        /// a prefab instead: the builder owns functional transforms, colliders, cameras and
        /// screens; this prefab owns collider-free visual dressing and survives every rebuild.
        /// Created empty on first run - Phase 4 fills the groups.
        /// </summary>
        private static void EnsureArtRootPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(ArtRootPrefabPath) != null)
                return;

            var temp = new GameObject(ArtRootName);
            foreach (string group in new[] { "Walls", "Ceiling", "Floor", "Furniture", "Desk" })
            {
                var child = new GameObject($"Dressing_{group}");
                child.transform.SetParent(temp.transform, false);
            }

            PrefabUtility.SaveAsPrefabAsset(temp, ArtRootPrefabPath);
            UnityEngine.Object.DestroyImmediate(temp);
            Debug.Log($"[GrayboxRoomBuilder] created empty art root prefab at {ArtRootPrefabPath}");
        }

        private static void InstantiateArtRoot()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArtRootPrefabPath);
            if (prefab == null)
                throw new InvalidOperationException($"art root prefab missing at {ArtRootPrefabPath}");

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = ArtRootName;
            instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            instance.transform.localScale = Vector3.one;

            // Build() always starts from an empty scene so a duplicate should be impossible,
            // but this is the PRD's source-of-truth gate and it is cheap to assert rather than
            // trust: rebuilding twice must leave exactly one complete art root.
            int roots = UnityEngine.Object
                .FindObjectsByType<Transform>(FindObjectsInactive.Include)
                .Count(t => t.parent == null && t.name == ArtRootName);
            if (roots != 1)
                throw new InvalidOperationException($"expected exactly one {ArtRootName}, found {roots}");
        }

        // ---------------------------------------------------------------- helpers

        private static void EnsureEnvironmentFolders()
        {
            if (!AssetDatabase.IsValidFolder(EnvironmentFolder))
                AssetDatabase.CreateFolder("Assets/SBR", "Environment");
            if (!AssetDatabase.IsValidFolder(EnvPrefabsFolder))
                AssetDatabase.CreateFolder(EnvironmentFolder, "Prefabs");
            if (!AssetDatabase.IsValidFolder(EnvPostFxFolder))
                AssetDatabase.CreateFolder(EnvironmentFolder, "PostFx");
        }

        private static void RegisterInBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        /// <summary>
        /// Phase 3: every box is now a chamfered mesh built at true world size rather than a
        /// scaled primitive cube. Sharp 90-degree edges catch no light, which is why the graybox
        /// read as flat; a narrow bevel face picks up a highlight and gives every form thickness.
        ///
        /// COLLISION IS DELIBERATELY UNCHANGED. The old primitive carried a unit BoxCollider
        /// scaled by the transform; this carries an explicit BoxCollider of exactly the same
        /// world dimensions with localScale left at 1. Walkable clearance, the interaction rays
        /// and the CharacterController all behave identically to the graybox.
        /// </summary>
        private static GameObject Box(string name, Transform parent, Vector3 center,
                                      Vector3 size, Material mat, int layer = 0,
                                      float bevel = -1f)
        {
            var go = new GameObject(name);
            if (parent != null)
                go.transform.SetParent(parent, true);
            go.transform.position = center;
            go.transform.localScale = Vector3.one; // the mesh carries the size, not the transform

            if (bevel < 0f)
                bevel = ChamferedBoxMesh.DefaultBevel(size);

            go.AddComponent<MeshFilter>().sharedMesh = ChamferedBoxMesh.GetOrCreate(size, bevel);
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;

            var col = go.AddComponent<BoxCollider>();
            col.size = size;
            col.center = Vector3.zero;

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
