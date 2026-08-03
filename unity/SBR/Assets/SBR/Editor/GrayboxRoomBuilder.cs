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
    ///
    /// ALWAYS FOLLOW A BUILD WITH <see cref="RoomLightingBake"/>. Rebuilding the scene from
    /// scratch discards the baked Adaptive Probe Volume data along with it, and the room then
    /// renders with direct light only - no bounce, no colour bleed, and flat walls. It still
    /// looks like a room, which is what makes this easy to miss.
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
            MarkStaticForGI();

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
            Debug.LogWarning("[GrayboxRoomBuilder] indirect light is now UNBAKED - run " +
                             "SBR/Bake Room Indirect Light (SBR.RoomLightingBake.Bake) or the " +
                             "room renders with direct light only");
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
            // R20 (Design Director, 2026-08-01): "the battered metal desk" is unbuilt
            // required work - it currently shares Prop with the stool and mini-fridge, so
            // giving it a wear surface would batter them too. Own material, see below.
            public Material Desk;
            public Material Couch;
            public Material Bezel;
            public Material LaptopShell;
            public Material PhoneShell;
            public Material BunkFrame;
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
                Wall = SurfaceMat("WallDark", new Color(0.255f, 0.245f, 0.210f),
                    ProceduralSurfaceTextures.SurfaceKind.Plaster, 1024,
                    contrast: 2.10f, tiling: 0.75f,
                    normalStrength: 7.0f, aoStrength: 0.8f,
                    smoothMin: 0.06f, smoothMax: 0.22f),
                // 4b: lifted and warmed so the floor belongs to the same room as the walls -
                // it was reading as a cold blue-grey slab under warm plaster.
                // Worn but INTACT, not the derelict full-coverage flaking of the concept.
                Floor = SurfaceMat("FloorWorn", new Color(0.185f, 0.166f, 0.134f),
                    ProceduralSurfaceTextures.SurfaceKind.WornFloor, 1024,
                    contrast: 2.00f, tiling: 0.60f,
                    normalStrength: 6.0f, aoStrength: 0.9f,
                    smoothMin: 0.14f, smoothMax: 0.42f),
                // sits between wall and floor; takes the tube's uplight and the stain story
                // Lowest relief of the four - water stains are discolouration, not topography.
                Ceiling = SurfaceMat("CeilingStained", new Color(0.208f, 0.198f, 0.166f),
                    ProceduralSurfaceTextures.SurfaceKind.CeilingStain, 1024,
                    contrast: 1.80f, tiling: 0.50f,
                    normalStrength: 3.5f, aoStrength: 0.5f,
                    smoothMin: 0.05f, smoothMax: 0.15f),
                Prop = Mat("PropGray", new Color(0.180f, 0.178f, 0.160f), smoothness: 0.35f),
                // R20: the desk is a NAMED gap in the ruling ("a battered metal desk"), not a
                // generic prop, so it gets its own SurfaceMat rather than staying on Prop -
                // Prop is also the stool and the mini-fridge, and neither was named. Tint is
                // UNCHANGED from Prop's own value: the desk's hue was not ruled, only its
                // surface was, so this is the same colour the desk already had.
                // res/tiling/relief per R20's spec: a desktop is a big, close, mostly-flat
                // panel, so tiling stays low (1.4) so a dent reads as one broad event rather
                // than a repeating stamp at 3m viewing distance.
                Desk = SurfaceMat("DeskBattered", new Color(0.180f, 0.178f, 0.160f),
                    ProceduralSurfaceTextures.SurfaceKind.BatteredMetal, 512,
                    contrast: 1.60f, tiling: 1.4f,
                    normalStrength: 5.0f, aoStrength: 0.7f,
                    smoothMin: 0.16f, smoothMax: 0.44f),
                // R19(c): the palette's Drab green #3A4230 was instantiated nowhere in the built
                // room. Applied to both bunks' structural frames only - BunkSlab/BunkPostFront/
                // BunkPostBack (was Prop) and Bunk2Slab/Bunk2PostFront/Bunk2PostBack (was the
                // Bunk2Dark material, now retired - see BuildDeskCluster). The bedding half of
                // this ruling (bunk 2's mattress fabric) lives in RoomArtDressing on
                // ArtBunk2Shadow. The couch is a separate "couch fabric" in the design doc and is
                // deliberately untouched here - it carries a design-verified relief result.
                BunkFrame = Mat("BunkFrameGreen", new Color(0.0423f, 0.0545f, 0.0296f),
                    smoothness: 0.30f),
                // 4b: lifted so the weave actually reads - the couch was dark enough that its
                // texture was invisible, which wasted the one fabric map in the room.
                // Highest relief: the weave is the whole point, and at 17cm tiling it is the one
                // surface the player gets close enough to read thread by thread.
                Couch = SurfaceMat("CouchGray", new Color(0.172f, 0.158f, 0.132f),
                    ProceduralSurfaceTextures.SurfaceKind.FabricWeave, 512,
                    contrast: 1.50f, tiling: 6.0f,
                    normalStrength: 10.0f, aoStrength: 1.2f,
                    smoothMin: 0.03f, smoothMax: 0.11f),
                Bezel = Mat("BezelBlack", new Color(0.045f, 0.045f, 0.040f), smoothness: 0.25f),
                // R19(a), the highest violation in the slice: TVBody, LaptopBase and PhoneBody
                // all pointed at Bezel, so the institution's hardware and the occupant's own read
                // as materially identical - the doc's whole story is the contrast between them.
                // Split so the laptop differs from the TV housing on at least two of the three
                // required channels (value, hue temperature, finish) and reads lighter, warmer
                // and lower-cost: 2.17x lighter than the housing by luminance (0.1052 vs 0.0485),
                // warm where the housing is cool (R-B +0.0270 vs -0.0122), and glossier (0.42 vs
                // 0.28) - three channels satisfied where the ruling required two. TVBody keeps
                // Bezel unchanged. The phone "is his too and sits near the laptop, not on it", so
                // it gets its own close-but-distinct shell rather than sharing LaptopShell.
                LaptopShell = Mat("LaptopShell", new Color(0.115f, 0.104f, 0.088f),
                    smoothness: 0.42f),
                PhoneShell = Mat("PhoneShell", new Color(0.092f, 0.085f, 0.076f),
                    smoothness: 0.50f),
                // Unified-grade spec §2: lift the panel's black floor so nothing in frame is
                // darker than the screen's own off state. Pure black on a panel in a dim, dusty
                // room is physically impossible and is the clearest "this was composited" tell.
                // Also retuned green -> cold white-grey per the 2026-07-27 three-source note:
                // the display is a quiet, mostly colourless screen, and the room's cool light
                // comes from the window, not from here.
                //
                // TvSweatScreen reads this material's emission as its idle baseline (_emissIdle)
                // and eases every beat back to it, so this value IS the TV's rest state.
                //
                // Deliberately NOT pushed above 1.0. The grade spec's >1.0 requirement is for the
                // BRIGHT tiers (canvas L4, beat flashes) - blowing the idle quad past 1.0 would
                // both white out the panel at rest and invert the beat flashes, which currently
                // sit at 0.02-0.25 in TvSweatScreen.cs. Ordering must stay idle < flash.
                TvScreen = Mat("ScreenTV", new Color(0.012f, 0.014f, 0.018f),
                    emission: new Color(0.048f, 0.055f, 0.068f), doubleSided: true),
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

        private const int TexSeed = 20260725;

        /// <summary>
        /// A full PBR surface: albedo + normal + metallic/gloss + occlusion, all derived from one
        /// deterministic field so they describe the same surface.
        ///
        /// Before this every room surface was flat-shaded off a single albedo map, so plaster
        /// damage, floor scuffs and fabric weave existed as tone variation only - they vanished
        /// under raking light instead of catching it, which is most of why the room read as
        /// painted-on rather than built.
        ///
        /// smoothMin/smoothMax are the rough (grimy) and clean ends of the surface; the mask map
        /// interpolates between them off the same height field, so wear is rougher than the
        /// material around it rather than the whole plane sharing one gloss value.
        /// </summary>
        // internal so RoomArtDressing can build its own SurfaceMat materials (R20: the TV
        // housing's ChippedPaint) through the same deterministic albedo+normal+mask+AO path
        // rather than duplicating its body - see Mat() below for the same reasoning.
        internal static Material SurfaceMat(string assetName, Color tint,
                                           ProceduralSurfaceTextures.SurfaceKind kind, int res,
                                           float contrast, float tiling,
                                           float normalStrength, float aoStrength,
                                           float smoothMin, float smoothMax) =>
            Mat(assetName, tint,
                smoothness: smoothMax,
                baseMap: ProceduralSurfaceTextures.GetOrCreate(kind, res, TexSeed, contrast),
                tiling: tiling,
                normalMap: ProceduralSurfaceTextures.GetOrCreateNormal(
                    kind, res, TexSeed, contrast, normalStrength),
                maskMap: ProceduralSurfaceTextures.GetOrCreateMask(
                    kind, res, TexSeed, contrast, smoothMin, smoothMax),
                occlusionMap: ProceduralSurfaceTextures.GetOrCreateOcclusion(
                    kind, res, TexSeed, contrast, aoStrength));

        // internal so RoomArtDressing can author its own dressing materials through the same
        // deterministic path rather than duplicating the URP/Lit setup.
        internal static Material Mat(string name, Color baseColor,
                                    Color? emission = null, bool doubleSided = false,
                                    float smoothness = 0.15f,
                                    Texture2D baseMap = null, Texture2D emissionMap = null,
                                    float tiling = 1f,
                                    Texture2D normalMap = null, float normalScale = 1f,
                                    Texture2D maskMap = null, Texture2D occlusionMap = null,
                                    float alphaClip = -1f, bool transparent = false,
                                    bool multiply = false)
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
                // MUST be an AnyEmissive flag, not None. URP's MaterialPostprocessor recomputes
                // the _EMISSION keyword from exactly this field on every material import:
                //   BaseShaderGUI.cs:946  shouldEmissionBeEnabled = flags & AnyEmissive
                //   BaseShaderGUI.cs:953  CoreUtils.SetKeyword(material, _EMISSION, ...)
                // With None the postprocessor stripped the keyword EnableKeyword had just set,
                // silently killing emission on the TV, laptop, phone, window and indicator lamp
                // the next time anyone opened the editor. RealtimeEmissive keeps the keyword
                // alive through import and still bakes nothing - this project has no lightmaps.
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
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

            // Surface relief. Unlike _EMISSION above, these keywords are derived by URP's
            // postprocessor from whether the texture is actually assigned, so they survive
            // import correctly - setting them here just makes the pre-import state honest.
            if (normalMap != null)
            {
                mat.SetTexture("_BumpMap", normalMap);
                mat.SetTextureScale("_BumpMap", new Vector2(tiling, tiling));
                mat.SetFloat("_BumpScale", normalScale);
                mat.EnableKeyword("_NORMALMAP");
            }
            if (maskMap != null)
            {
                mat.SetTexture("_MetallicGlossMap", maskMap);
                mat.SetTextureScale("_MetallicGlossMap", new Vector2(tiling, tiling));
                mat.SetFloat("_SmoothnessTextureChannel", 0f); // smoothness from metallic alpha
                mat.EnableKeyword("_METALLICSPECGLOSSMAP");
            }
            if (occlusionMap != null)
            {
                mat.SetTexture("_OcclusionMap", occlusionMap);
                mat.SetTextureScale("_OcclusionMap", new Vector2(tiling, tiling));
                mat.SetFloat("_OcclusionStrength", 1f);
                mat.EnableKeyword("_OCCLUSIONMAP");
            }

            // R7 wear surfaces. As with _EMISSION above, URP recomputes these keywords from the
            // float properties on every import, so the floats are the durable state and the
            // keywords here only make the pre-import material honest.
            //
            // Alpha clip is the default for wear because it writes depth: the decal then sorts
            // correctly against everything and still receives the renderer's SSAO. The texture
            // stores a coverage FIELD rather than a finished shape, so _Cutoff picks the contour
            // - a lower cutoff spreads the stain, a higher one shrinks it, with no new texture.
            if (alphaClip >= 0f)
            {
                mat.SetFloat("_AlphaClip", 1f);
                mat.SetFloat("_Cutoff", alphaClip);
                mat.EnableKeyword("_ALPHATEST_ON");
                mat.SetOverrideTag("RenderType", "TransparentCutout");
                mat.renderQueue = (int)RenderQueue.AlphaTest;
            }

            // Transparent is reserved for damp blooms, where a soft falloff IS the effect. It
            // costs depth-write, so these get no SSAO and must stay few.
            //
            // MULTIPLY, NOT ALPHA BLEND, for anything that is meant to be a STAIN. Alpha blending
            // a Lit surface does not darken what is behind it - it REPLACES it with a second lit
            // surface. The ceiling soot proved this the expensive way: albedo 0.044, but with no
            // occlusion map and no normal map it was a flatter, less-occluded surface than the
            // ceiling underneath, so the "soot" rendered 5-20 luminance levels BRIGHTER than the
            // ceiling it was dirtying, as a uniform film bounded by the quad's own outline.
            // Multiply makes the decal a filter over the existing surface, which is what dirt
            // physically is, and URP's _ALPHAMODULATE_ON makes alpha 0 mean "leave it alone" -
            // so the quad border becomes invisible for free instead of needing to be hidden.
            if (transparent || multiply)
            {
                mat.SetFloat("_Surface", 1f);
                mat.SetFloat("_ZWrite", 0f);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.renderQueue = (int)RenderQueue.Transparent;

                if (multiply)
                {
                    // URP's BaseShaderGUI.BlendMode is Alpha=0, Premultiply=1, Additive=2,
                    // Multiply=3 - NOT the order you would guess. Setting 2 here gave One/One,
                    // i.e. additive, which brightens exactly what a stain is meant to darken.
                    // The postprocessor derives _SrcBlend/_DstBlend and _ALPHAMODULATE_ON from
                    // this field on import, so this number is the one that actually matters.
                    mat.SetFloat("_Blend", 3f); // BlendMode.Multiply
                    mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.DstColor);
                    mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
                    mat.EnableKeyword("_ALPHAMODULATE_ON");
                }
                else
                {
                    mat.SetFloat("_Blend", 0f);
                    mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetFloat("_DstBlend",
                                 (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.DisableKeyword("_ALPHAMODULATE_ON");
                }
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
            // R19(c): frame is BunkFrame (drab green #3A4230), not Prop - see BuildMaterials.
            Box("BunkSlab", root.transform,
                new Vector3(-0.9f, 1.54f, 0.3f), new Vector3(0.8f, 0.08f, 1.9f), mats.BunkFrame);
            Box("BunkPostFront", root.transform,
                new Vector3(-0.53f, 0.77f, -0.62f), new Vector3(0.06f, 1.54f, 0.06f),
                mats.BunkFrame);
            Box("BunkPostBack", root.transform,
                new Vector3(-0.53f, 0.77f, 1.22f), new Vector3(0.06f, 1.54f, 0.06f),
                mats.BunkFrame);

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
            // R20: DeskTop and all four legs are the desk's own SurfaceMat (Desk, not
            // Prop) - see BuildMaterials. Only these five; the stool and mini-fridge below
            // stay on Prop deliberately, the ruling did not name them.
            Box("DeskTop", deskRoot, new Vector3(1.05f, 0.73f, 1.45f),
                new Vector3(0.5f, 0.04f, 1.1f), mats.Desk);
            Box("DeskLegA", deskRoot, new Vector3(0.855f, 0.355f, 0.955f),
                new Vector3(0.05f, 0.71f, 0.05f), mats.Desk);
            Box("DeskLegB", deskRoot, new Vector3(1.245f, 0.355f, 0.955f),
                new Vector3(0.05f, 0.71f, 0.05f), mats.Desk);
            Box("DeskLegC", deskRoot, new Vector3(0.855f, 0.355f, 1.945f),
                new Vector3(0.05f, 0.71f, 0.05f), mats.Desk);
            Box("DeskLegD", deskRoot, new Vector3(1.245f, 0.355f, 1.945f),
                new Vector3(0.05f, 0.71f, 0.05f), mats.Desk);

            // Second bunk frame over the desk (2026-07-27 layout brief). The room now reads as
            // built for two occupants; from the standing camera the window sits between this
            // bunk and the one over the couch, which is the approved composition.
            //
            // Deliberately NOT lit - see the dressing pass. Allen's note on concept C: this bunk
            // being darker than the rest of the room implies another occupant and reads as
            // faintly unsettling. It should be legible as occupied, never legible as empty.
            //
            // Slab stops at the far wall (z 0.5..2.0) rather than mirroring the couch bunk's
            // length, and the posts clear the stool's footprint in z.
            // R19(c): frame is BunkFrame (drab green #3A4230, shared with bunk 1's frame - see
            // BuildMaterials), not the old Bunk2Dark. Bunk2Dark's own darker-than-Prop albedo
            // existed only to keep this slab from becoming the brightest object in frame even
            // when TvLight reached the corner; BunkFrame is itself dark enough to hold that job,
            // so Bunk2Dark's creation is retired rather than left as an orphan - nothing else in
            // this file references it after this change.
            //
            // FLAGGED, NOT FIXED (R19(c)): drab green's luminance (0.0501) is brighter than the
            // Bunk2Dark it replaces (0.0389). Bunk 2's mattress is a ratified test at 43.9 +/-1
            // measured luminance - see RoomArtDressing.BuildClutter (ArtBunk2Shadow) for the full
            // note. This is a known, deliberate, measured risk; do not compensate here either.
            Box("Bunk2Slab", deskRoot, new Vector3(0.9f, 1.54f, 1.25f),
                new Vector3(0.8f, 0.08f, 1.5f), mats.BunkFrame);
            Box("Bunk2PostFront", deskRoot, new Vector3(0.53f, 0.77f, 0.57f),
                new Vector3(0.06f, 1.54f, 0.06f), mats.BunkFrame);
            Box("Bunk2PostBack", deskRoot, new Vector3(0.53f, 0.77f, 1.93f),
                new Vector3(0.06f, 1.54f, 0.06f), mats.BunkFrame);

            // Stays put: at 0.45m tall it passes under the 1.50m slab with clearance, and its
            // z-span (1.275..1.625) misses both bunk posts.
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

            // R19(a): LaptopShell, not Bezel - see BuildMaterials for the channel breakdown.
            GameObject lapBase = Box("LaptopBase", root.transform,
                new Vector3(1.08f, 0.76f, 1.62f), new Vector3(0.22f, 0.02f, 0.32f),
                mats.LaptopShell);

            // Lid: 0.32 x 0.22 quad hinged on the wall-side edge of the base, tilted 20
            // degrees back toward the wall (+X), screen facing the room (-X and up).
            const float lidTiltDeg = 20f;
            Vector3 hinge = new Vector3(1.19f, 0.77f, 1.62f);
            Vector3 widthDir = Vector3.forward; // hinge axis runs along Z
            Vector3 lidUp = Quaternion.AngleAxis(-lidTiltDeg, Vector3.forward) * Vector3.up;
            Vector3 lidNormal = Vector3.Cross(widthDir, lidUp); // -X and slightly up
            Vector3 lidCenter = hinge + lidUp * 0.11f;
            // R16: keeps its MeshCollider - LaptopScreen is on the Interactable layer.
            GameObject lid = Quad("LaptopScreen", root.transform, lidCenter,
                new Vector2(0.32f, 0.22f), lidNormal, lidUp, mats.LaptopScreen,
                keepCollider: true);

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

            // R19(a): PhoneShell, not Bezel - the phone "is his too" but sits near the laptop,
            // not on it, so it gets its own close-but-distinct shell (see BuildMaterials).
            GameObject body = Box("PhoneBody", root.transform,
                new Vector3(1.0f, 0.754f, 1.15f), new Vector3(0.075f, 0.008f, 0.15f),
                mats.PhoneShell);
            // R16: keeps its MeshCollider - PhoneScreen is on the Interactable layer.
            GameObject screen = Quad("PhoneScreen", root.transform,
                new Vector3(1.0f, 0.759f, 1.15f), new Vector2(0.065f, 0.135f),
                Vector3.up, Vector3.forward, mats.PhoneScreen, keepCollider: true);

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
            // Pulled forward to z = -0.05 (was 0.85). The second bunk now occupies z 0.5..2.0 at
            // slab height 1.58, and the tube was hanging almost directly above it - lighting its
            // top face and making it the brightest object in the room. Moving the fixture toward
            // the door end leaves that bunk in shadow per the brief, and gives a cleaner division
            // of labour between the sources: the tube owns the aisle and the couch side, the desk
            // lamp owns the desk.
            // REVERTED to x 0.85. Nudging this to 1.02 to graze the right wall put it directly
            // over the second bunk (x 0.5..1.3) and lit its mattress, which breaks the ratified
            // "occupied, never empty" treatment. A ratified requirement outranks a surface effect.
            var tubeGo = new GameObject("FluorescentKey");
            tubeGo.transform.position = new Vector3(0.85f, 2.05f, -0.05f);
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

            // WINDOW - short throw, not ambient fill (2026-07-27 lighting note).
            // The previous brief asked for "more blue" and the result tinted the entire room
            // blue-teal, which Allen rejected: "too blue, keep it natural". The correction is
            // about REACH, not intensity. So: brighter and more saturated at the source, but
            // range cut from 3.0 to 1.7 so the blue pools on the sill, the floor and wall right
            // around the window and the near edge of the closest bunk - and dies well before the
            // far wall, the display, or the room's own olive surfaces.
            var windowGo = new GameObject("WindowGlowLight");
            windowGo.transform.position = new Vector3(0f, 1.42f, 1.80f);
            var glow = windowGo.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.intensity = 2.4f;
            glow.range = 1.70f;
            glow.color = new Color(0.40f, 0.56f, 0.92f);
            glow.shadows = LightShadows.None;

            // DESK LAMP - the fourth source, from concept render C (Allen approved 2026-07-27).
            // Gives the desk its own warm pool and justifies why anyone would work there. Kept
            // tight and low so it reads as a task light rather than a second room light, and
            // aimed down at the laptop so it never spills onto the second bunk above it.
            var lampGo = new GameObject("DeskLampLight");
            lampGo.transform.position = new Vector3(1.12f, 1.28f, 1.72f);
            lampGo.transform.rotation =
                Quaternion.LookRotation(new Vector3(-0.28f, -0.95f, -0.14f).normalized, Vector3.up);
            var lamp = lampGo.AddComponent<Light>();
            lamp.type = LightType.Spot;
            lamp.spotAngle = 64f;
            lamp.innerSpotAngle = 22f;
            lamp.intensity = 2.6f;
            lamp.range = 1.5f;
            lamp.color = new Color(1.00f, 0.82f, 0.52f);
            lamp.shadows = LightShadows.None;

            // Gradient ambient does surface separation a single flat value cannot: the ceiling
            // catches the tube, the floor stays dirty and warm, the walls sit between. This is
            // the other half of the fix for the undifferentiated-box problem.
            // 4b: dropped ~38%. These were tuned in Phase 2 when the room was flat, untextured
            // and had no dressing - at that point ambient was doing the work of making anything
            // read at all. With surface maps, conduit and a lit window carrying the frame, the
            // same values over-lift everything and kill the falloff.
            // ATMOSPHERIC HAZE - grade spec item 3, the second half of the "single highest-
            // leverage change". Puts air between the camera and the screen so the TV's emission
            // has something to travel through instead of stopping dead at the bezel. In a room
            // this filthy the haze is justified in-fiction. Tinted to the room's own olive so it
            // does not read as a blue-grey wash. Lives in RenderSettings, not the volume: URP
            // has no fog VolumeComponent.
            // GRAZING WALL WASH - the fix for surface relief, and it is geometric, not cosmetic.
            //
            // Lambertian sensitivity to a normal perturbation scales with sin(theta), where theta
            // is the light's incidence angle off the surface normal. Light arriving perpendicular
            // to a surface reveals NO relief; light travelling nearly parallel reveals the most.
            // The ceiling was the proof all along: it carries the weakest normal map in the room
            // (channel sd ~9, vs the couch fabric's ~80) yet reads the strongest, purely because
            // the tube hangs 0.25m beneath it and rakes across it at theta ~= 90 degrees.
            //
            // So these hug their wall and aim down its face, putting theta near 90 where the maps
            // finally have light that varies across them. No map values changed to achieve this.
            // A right-wall graze was tried here and REMOVED. The geometry was sound - for a wall
            // and a light d off it, a point h below sees N.L = d / sqrt(d^2 + h^2), so d=0.26
            // gives theta ~72deg: nearly all the relief sensitivity of a true graze with ~6x the
            // brightness of hugging the wall at 45mm. But that offset necessarily puts the light
            // out into the room, and the only wall it could graze here has the second bunk in
            // front of it, so it lit the mattress and broke the ratified dark-bunk treatment.
            //
            // Recorded rather than deleted because the reasoning is reusable: this room cannot
            // graze its right wall without relighting that bunk. If relief on plaster matters
            // later, the lever is a wall the bunks do not occupy, or baked/APV indirect light -
            // not a stronger normal map. See docs/room-visual-pass/PHASE_A_FINDINGS.md.

            // Couch side, deliberately much dimmer. The fabric weave is the room's strongest
            // normal map and was contributing nothing because that corner sits in shadow. Just
            // enough raking light to let the weave read, without undoing the dark left half.
            var couchGrazeGo = new GameObject("CouchGraze");
            couchGrazeGo.transform.position = new Vector3(-1.04f, 1.44f, 0.35f);
            couchGrazeGo.transform.rotation =
                Quaternion.LookRotation(new Vector3(0.11f, -0.99f, 0f).normalized, Vector3.up);
            var couchGraze = couchGrazeGo.AddComponent<Light>();
            couchGraze.type = LightType.Spot;
            couchGraze.spotAngle = 110f;
            couchGraze.innerSpotAngle = 30f;
            // R10 FALLBACK, raised 0.32 -> 1.60. Bounce was tried first per the approved route
            // and measured as structurally unable to raise relief (see the note below the probe
            // block). This light is Mixed, so its DIRECT contribution stays realtime - which is
            // the whole point: direct light at a grazing angle is the only thing that produces
            // the across-surface gradient a normal map needs. It already sits at y = 1.44, under
            // BunkSlab's underside at 1.50, so it cannot reach either bunk - the constraint four
            // earlier attempts at this corner were reverted for.
            couchGraze.intensity = 1.60f;
            couchGraze.range = 2.2f;
            couchGraze.color = new Color(0.70f, 0.74f, 0.80f);
            couchGraze.shadows = LightShadows.None;

            // A BAKED-ONLY BOUNCE LIGHT WAS TRIED HERE TWICE AND REMOVED. Do not re-add one.
            //
            // R10 asked for directional variation on the couch rather than another visible lamp,
            // so a Light with lightmapBakeType = Baked looked ideal: it contributes only through
            // the probe field and Unity strips it from the realtime list, so the player never
            // sees a new source. It measured the exact opposite of the goal, twice.
            //
            //   wide, close  (100deg, 0.45m off the couch): couch mean +2.50%, relief 0.93x
            //   narrow, back ( 50deg, 1.15m off the couch): couch mean +2.28%, relief 0.94x
            //
            // Brighter AND flatter, and narrowing the source barely moved it - so this is not a
            // tuning miss, it is structural. Relief% is a GRADIENT DIVIDED BY A MEAN. Probe
            // lighting is spherical harmonics, which is smooth at the scale of a cushion, so it
            // raises the mean without raising the gradient - it can only ever move the
            // denominator. Any probe-mediated addition lowers relief on a surface that is
            // already lit, which is also why raising the bunk slab's underside albedo to bounce
            // more light down was not tried: same mechanism, same outcome.
            //
            // This does NOT contradict R6, where probes raised wall relief 6.3x. There the walls
            // went from flat ambient to bounce that varied across metres of surface. Here the
            // couch is already lit and the addition is smooth across the whole cushion.
            //
            // Conclusion, and it sharpens R12: only light arriving at a grazing angle as DIRECT
            // light can raise relief. Bounce fills shadow; it does not reveal surface.

            // REFLECTION PROBE - without this the room has no environment specular at all.
            // No skybox is assigned and there was no probe, so URP's reflection lookup returned
            // near-black: every surface had a broad rough specular lobe with nothing to reflect
            // into it. That is why the first two PBR passes looked flat - the normal maps were
            // correct, but a normal map can only modulate light that VARIES with the normal, and
            // ambient-only lighting does not. Box projection so the room's own walls reflect at
            // the right parallax in a space this small.
            var probeGo = new GameObject("RoomReflectionProbe");
            probeGo.transform.position = new Vector3(0f, 1.15f, 0f);
            var probe = probeGo.AddComponent<ReflectionProbe>();
            probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
            probe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.OnAwake;
            probe.timeSlicingMode = UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.NoTimeSlicing;
            probe.size = new Vector3(2.8f, 2.4f, 4.2f);
            probe.boxProjection = true;
            probe.resolution = 128;
            probe.clearFlags = UnityEngine.Rendering.ReflectionProbeClearFlags.SolidColor;
            probe.backgroundColor = new Color(0.020f, 0.020f, 0.026f);
            probe.nearClipPlane = 0.05f;
            probe.farClipPlane = 12f;
            probe.intensity = 1f;

            // ---------------------------------------------------------- indirect light
            // PHASE B. Everything above is direct-only. A URP realtime light bounces
            // nothing: a surface it does not hit directly receives the flat ambient value
            // and nothing else, which is why this room reads as one lamp in a black box and
            // why relief only appears where the tube happens to rake (PHASE_A_FINDINGS.md).
            //
            // Mixed keeps every pool exactly as tuned - direct light and shadows still render
            // in realtime, so the window's short throw and the desk lamp's tight cone are
            // untouched - and adds their INDIRECT contribution to the Adaptive Probe Volume
            // below. The two runtime-driven lights stay Realtime: TvLight belongs to the TV
            // sweat slice and changes colour every frame, and PhoneBuzzLight is a flash. A
            // baked bounce from either would be a lie the moment they change.
            foreach (Light l in new[] { moon, tube, bounce, glow, lamp, couchGraze })
                l.lightmapBakeType = LightmapBakeType.Mixed;

            // Sized to the interior plus a small margin so the boundary probes sit inside the
            // walls rather than on them. fillEmptySpaces places probes through the open air the
            // player walks in, not only in the shell of geometry - without it the middle of the
            // room interpolates between wall probes and the bounce loses its directionality.
            var apvGo = new GameObject("AdaptiveProbeVolume");
            apvGo.transform.position = new Vector3(0f, Height * 0.5f, 0f);
            var apv = apvGo.AddComponent<ProbeVolume>();
            apv.mode = ProbeVolume.Mode.Local;
            apv.size = new Vector3(HalfW * 2f + 0.4f, Height + 0.4f, HalfL * 2f + 0.4f);
            apv.fillEmptySpaces = true;
            apv.overrideRendererFilters = false;

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.085f;
            RenderSettings.fogColor = new Color(0.082f, 0.079f, 0.061f);

            // R9 - ambient down 35% (DD-approved band 30-40%). MEASURED EFFECT: NONE.
            //
            // These values are kept because they are approved and within band, but do not expect
            // them to do anything, and do not re-derive R9 from first principles later. A/B at
            // 100% vs 65% with everything else identical moved every region by 0.00-0.02% and
            // 0.10% of pixels by more than one luminance level, which is film grain:
            //
            //   ceiling plaster 29.12 -> 29.12    couch corner  35.15 -> 35.14
            //   under bunk 2    49.36 -> 49.35    whole frame   37.75 -> 37.76
            //
            // WHY: R6 already removed the flat-ambient problem as a side effect. Every static
            // renderer is receiveGI = LightProbes, so it samples the Adaptive Probe Volume and
            // not the ambient probe. RenderSettings ambient now reaches the room ONLY as
            // environment input to the bake - and this room is a sealed box, so that environment
            // is fully occluded and contributes essentially nothing.
            //
            // The premise behind R9 - "flat fill is diluting the directional bounce" - was true
            // before R6 and false after it. What fills the shadows now IS R6's bounce, which is
            // directional and is the thing worth keeping. If deeper shadows are ever wanted, the
            // levers are the grade or individual light intensities; turning down indirect
            // (m_IndirectOutputScale) would just undo R6.
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.0585f, 0.0566f, 0.0397f);
            RenderSettings.ambientEquatorColor = new Color(0.0371f, 0.0371f, 0.0312f);
            RenderSettings.ambientGroundColor = new Color(0.0234f, 0.0208f, 0.0163f);
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

            // UNIFIED GRADE (tv-sweat/docs/tv-sweat-refinement/unified-grade-spec.md).
            // The room is painterly, the TV is a vector-crisp LED matrix on black; separately
            // both are fine, together they read as two assets in one frame. The fix is to put
            // them through one camera - one global volume, TV inside the pass, never exempt.

            // LIFT THE BLACK FLOOR - the spec's "single highest-leverage change".
            //
            // Spec names Shadows/Midtones/Highlights for this. That component CANNOT do it:
            // its shadow term is a MULTIPLIER, and any multiplier times pure black is still
            // pure black. Raising a true black floor needs an ADDITIVE offset, which lives in
            // LiftGammaGain.lift. Using SMH here would have looked like the fix was applied
            // while the panel's #000000 stayed exactly #000000.
            //
            // T48 - NEUTRAL BLACK POINT AT THE SAME VALUE. Was (0.99, 1.00, 1.03, 0.0075),
            // tinted cool to land on the spec's #0a0c10 target. That target was one number doing
            // two jobs: naming a luminance floor, and accidentally naming a hue inherited from
            // the TV substrate. A panel may be cool; plaster may not (law 1.1).
            //
            // Measured consequence of the old value: the room's own surfaces read hue 268-273deg
            // at chroma 6.6-9.5 with every screen dark and TvLight disabled, while the same
            // frames grade-bypassed read chroma 0.55-1.66 and neutral. The grade was multiplying
            // chroma 4x to 14x. Evidence is the R23/R26 conformance pair.
            //
            // WHY THE FIX IS EXACTLY THIS AND COSTS NO BRIGHTNESS. URP's PrepareLiftGammaGain
            // takes GammaToLinear(xyz) * 0.15, SUBTRACTS that triplet's own luminance, then adds
            // w to every channel. So w is the VALUE and xyz is purely the HUE - the xyz triplet
            // contributes nothing achromatic by construction. Neutralising it is therefore free:
            //
            //   applied lift, before:  R 0.004103  G 0.007493  B 0.017572   blue 4.28x red
            //   applied lift, after:   R 0.007500  G 0.007500  B 0.007500   blue 1.00x red
            //   lift luminance:        0.007500 -> 0.007500, delta 0.00000000
            //
            // The black floor - the thing the spec's "single highest-leverage change" is for -
            // is bit-identical. Only the cast is gone.
            //
            // CALIBRATION NOTE, still true and still the trap: URP scales lift far harder than
            // its raw value implies. A w of 0.055 - which reads like "raise black to ~5%" - came
            // back as a flat mid-grey panel around 38%, milky and washed out, and failed the
            // spec's own §5 ladder checks. Measured ratio is roughly 7x, so w is an order of
            // magnitude smaller than it looks. Do not "correct" it upward.
            var lift = GetOrAddVolumeComponent<LiftGammaGain>(profile);
            lift.lift.overrideState = true;
            lift.lift.value = new Vector4(1.00f, 1.00f, 1.00f, 0.0075f);

            var bloom = GetOrAddVolumeComponent<Bloom>(profile);
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 0.90f;   // spec: only real light sources bloom
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 0.70f;
            bloom.scatter.overrideState = true;
            bloom.scatter.value = 0.70f;

            // Shared lens. Kept under the spec's 0.08 because it attacks small type first and
            // the TV's metadata row is already the tightest thing in frame (spec §5:
            // legibility outranks integration).
            var aberration = GetOrAddVolumeComponent<ChromaticAberration>(profile);
            aberration.intensity.overrideState = true;
            aberration.intensity.value = 0.065f;

            var grade = GetOrAddVolumeComponent<ColorAdjustments>(profile);
            grade.postExposure.overrideState = true;
            grade.postExposure.value = 0.35f;
            grade.contrast.overrideState = true;
            grade.contrast.value = 16f;
            // Was +4. Grade spec item 8 wants slight DEsaturation of the room, and the lifted
            // floor plus fog already soften it - pushing saturation on top fights both.
            grade.saturation.overrideState = true;
            grade.saturation.value = 0f;

            // Compression, cheaply. The room should feel like it is closing in at the edges.
            var vignette = GetOrAddVolumeComponent<Vignette>(profile);
            vignette.intensity.overrideState = true;
            vignette.intensity.value = 0.30f;
            vignette.smoothness.overrideState = true;
            vignette.smoothness.value = 0.40f;

            // Grime that costs nothing and reads at every camera distance.
            var grain = GetOrAddVolumeComponent<FilmGrain>(profile);
            grain.type.overrideState = true;
            grain.type.value = FilmGrainLookup.Medium1;
            grain.intensity.overrideState = true;
            // Spec calls grain "the strongest single unifier" - one grain over the room and the
            // TV together makes them share a sensor, which is most of why they stop reading as
            // two separate images.
            grain.intensity.value = 0.20f;
            grain.response.overrideState = true;
            grain.response.value = 0.70f;

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
        /// <summary>
        /// PHASE B. Marks the room as bakeable geometry for the Adaptive Probe Volume.
        ///
        /// Two flags, and both are load-bearing:
        ///
        /// ContributeGI puts the renderer into the GI scene. Without it the baker sees an
        /// empty box - probes get the flat ambient value and nothing bounces off anything,
        /// which is the exact state this phase exists to fix.
        ///
        /// receiveGI = LightProbes says "sample the probe volume, do not lightmap me". That
        /// matters here beyond preference: every mesh in this room is generated at runtime by
        /// ChamferedBoxMesh / ConduitMesh and carries NO UV2. Lightmapping would need an
        /// unwrap pass per mesh; APV is volumetric and needs no UVs at all. This is why the
        /// bake is probes-only and why nothing here required a UV change.
        ///
        /// The Player is skipped deliberately - it moves, so it must sample probes as a
        /// dynamic object rather than be baked into them. Canvas-based objects (the HUD, and
        /// the TV / laptop / phone screens) have no MeshRenderer and are skipped for free.
        /// </summary>
        private static void MarkStaticForGI()
        {
            // R7.0. Wear decals are deliberately kept OUT of the bake - see
            // RoomArtDressing.WearRootName for why thin quads make bad GI geometry.
            GameObject wearGo = GameObject.Find(
                $"{RoomArtDressing.GeneratedRootName}/{RoomArtDressing.WearRootName}");
            Transform wear = wearGo != null ? wearGo.transform : null;

            int marked = 0, skipped = 0;
            foreach (GameObject root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root.name == "Player")
                    continue;

                foreach (MeshRenderer mr in root.GetComponentsInChildren<MeshRenderer>(true))
                {
                    if (wear != null && mr.transform.IsChildOf(wear))
                    {
                        skipped++;
                        continue;
                    }

                    GameObjectUtility.SetStaticEditorFlags(
                        mr.gameObject,
                        GameObjectUtility.GetStaticEditorFlags(mr.gameObject) |
                        StaticEditorFlags.ContributeGI);
                    mr.receiveGI = ReceiveGI.LightProbes;
                    marked++;
                }
            }

            Debug.Log($"[GrayboxRoomBuilder] marked {marked} renderers ContributeGI + LightProbes, " +
                      $"skipped {skipped} wear decals (excluded from the bake by design)");
        }

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

        // T57: the design doc requires "Meshes are built at true world size with localScale 1" -
        // GameObject.CreatePrimitive(PrimitiveType.Quad) plus a size-shaped localScale was
        // exactly the anti-pattern the rule names, so this now builds a real quad mesh via
        // ChamferedBoxMesh.GetOrCreateQuad(size, uvRepeats) and leaves localScale at 1, same as
        // Box() above. uvRepeats is Vector2.one, NOT a default to retune: it reproduces the
        // primitive's 0..1 UVs exactly, and WindowPane's night-city texture is explicitly
        // non-tiling and expects to map 0..1 across the pane exactly once - any other repeat
        // value corrupts it.
        //
        // GetOrCreateQuad's normal sits on local -Z with Unity's own front-face winding,
        // deliberately matching CreatePrimitive(Quad)'s convention, so the LookRotation(-facing,
        // up) below and every call site's facing/up pair are unchanged by this swap.
        //
        // R16: GameObject.CreatePrimitive(PrimitiveType.Quad) used to silently bring its own
        // MeshCollider, so every screen built here was quietly adding an undocumented solid
        // collider. With the primitive gone nothing supplies that collider for free, so
        // keepCollider now adds a MeshCollider explicitly, sharing the same generated mesh.
        // TVScreen and WindowPane are redundant with the wall/body colliders already sitting
        // directly behind them and stay collider-free (keepCollider defaults false, same
        // collider-free contract as RoomArtDressing.ArtQuad). LaptopScreen and PhoneScreen pass
        // keepCollider: true - they sit on the Interactable layer and interaction raycasting is
        // not re-plumbed to satisfy a collider count. True inventory is unchanged: 29 = 27
        // BoxColliders (24 solid, 3 triggers) + 2 named interaction MeshColliders (LaptopScreen,
        // PhoneScreen).
        private static GameObject Quad(string name, Transform parent, Vector3 center,
                                       Vector2 size, Vector3 facing, Vector3 up, Material mat,
                                       bool keepCollider = false)
        {
            var go = new GameObject(name);
            if (parent != null)
                go.transform.SetParent(parent, true);
            go.transform.position = center;
            // Unity's Quad primitive renders on its local -Z side; aim local -Z along 'facing'.
            // Screen materials are Cull Off, so a flipped face still renders either way.
            go.transform.rotation = Quaternion.LookRotation(-facing.normalized, up);
            go.transform.localScale = Vector3.one; // mesh carries true world size, not the transform

            Mesh mesh = ChamferedBoxMesh.GetOrCreateQuad(size, Vector2.one);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            if (keepCollider)
                go.AddComponent<MeshCollider>().sharedMesh = mesh;

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
