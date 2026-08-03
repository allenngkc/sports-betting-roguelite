using System.IO;
using UnityEditor;
using UnityEngine;

namespace SBR
{
    /// <summary>
    /// Phase 4 of the room visual pass (direction B, Vice Grip): non-functional surface dressing.
    ///
    /// ARCHITECTURE. GrayboxRoomBuilder owns every functional transform, collider, camera, screen
    /// and interaction. This file owns only things you look at, and everything it creates is
    /// COLLIDER-FREE by design - walkable clearance and the interaction rays must be exactly what
    /// the graybox established. It builds into a fresh `RoomArtGenerated` root each build rather
    /// than into the `RoomArtRoot` prefab instance, so generated dressing and any future
    /// hand-authored prefab content stay cleanly separated and neither clobbers the other.
    ///
    /// Everything here is deterministic - the project's rule is that nothing in the scene is ever
    /// hand-authored (design/05), and this pass keeps that true for art as well as geometry.
    /// </summary>
    public static class RoomArtDressing
    {
        private const string TexFolder = "Assets/SBR/Environment/Textures";
        internal const string GeneratedRootName = "RoomArtGenerated";

        /// <summary>
        /// R7.0. Everything under this root is excluded from the Adaptive Probe Volume bake by
        /// <c>GrayboxRoomBuilder.MarkStaticForGI</c>. Wear is thin quads sitting 2-3mm off a
        /// surface: as GI geometry they are actively harmful, occluding at probe scale and able
        /// to invalidate or leak the probes behind the wall they are decorating. Dirt must not
        /// change how the room is lit - it has no business in the bake at all.
        /// </summary>
        internal const string WearRootName = "Dressing_Wear";

        /// <summary>R7 wear seed. Fixed so every rebuild produces byte-identical masks.</summary>
        private const int WearSeed = 20260731;

        // Room interior, mirrored from the builder.
        private const float HalfW = 1.3f;
        private const float HalfL = 2.0f;
        private const float Height = 2.3f;

        public static void Build()
        {
            var root = new GameObject(GeneratedRootName).transform;
            root.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            Material trim = GrayboxRoomBuilder.Mat("ArtTrimMetal",
                new Color(0.115f, 0.112f, 0.098f), smoothness: 0.30f);
            Material paper = GrayboxRoomBuilder.Mat("ArtPaper",
                new Color(0.52f, 0.50f, 0.44f), smoothness: 0.05f);
            Material grime = GrayboxRoomBuilder.Mat("ArtGrime",
                new Color(0.085f, 0.080f, 0.068f), smoothness: 0.12f);

            // R19(b): restored to the spec's cool #22252A (B>G>R). The built value had drifted
            // hue-flipped WARM (R>G>B), which undercuts the reason this palette exists: the
            // institution's metal being colder than the room is the contrast the whole story
            // runs on. GUARD: if this reads too dark under the approved one-tube rig, that is a
            // lighting finding under R12, NOT a licence to lighten the albedo back up.
            Material conduit = GrayboxRoomBuilder.Mat("ArtConduit",
                new Color(0.0160f, 0.0185f, 0.0232f), smoothness: 0.34f);

            // Painted steel for the display enclosure - same wear language as the bunk frames,
            // a touch lighter than the conduit so the housing reads as a separate installed
            // object rather than dissolving into the pipe runs behind it.
            // R19(b): restored to the spec's cool #3A3F42 (B>G>R), same hue-flip fix and same
            // R12 guard as ArtConduit above - do not lighten this to compensate for how it reads
            // under the rig; that is a lighting finding, not an albedo problem.
            //
            // R20 (Design Director, 2026-08-01): "the TV housing's chipped paint" is unbuilt
            // required work, so this moves from a flat Mat to a SurfaceMat carrying
            // ChippedPaint. TINT UNCHANGED from R19(b) - new maps, not a re-colour.
            // smoothMin/smoothMax straddle the old flat 0.28: intact paint (0.34) is glossier
            // than the dull exposed metal (0.08) in the chips.
            Material steel = GrayboxRoomBuilder.SurfaceMat("ArtHousingSteel",
                new Color(0.0423f, 0.0497f, 0.0545f),
                ProceduralSurfaceTextures.SurfaceKind.ChippedPaint, 512,
                contrast: 1.90f, tiling: 3.0f,
                normalStrength: 8.0f, aoStrength: 1.0f,
                smoothMin: 0.08f, smoothMax: 0.34f);
            Material stencilMat = GrayboxRoomBuilder.Mat("ArtStencil", Color.white,
                smoothness: 0.10f, baseMap: GetOrCreateStencil("RM-4B 217-9C", 256, 64));
            Material indicator = GrayboxRoomBuilder.Mat("ArtIndicator",
                new Color(0.35f, 0.06f, 0.04f),
                emission: new Color(0.85f, 0.14f, 0.08f), smoothness: 0.45f);

            // R7 Tier 1 wear. Colour lives here, never in the textures - see
            // ProceduralWearTextures. Cutoffs are the shape control: the alpha channel stores a
            // coverage field and _Cutoff picks which contour through it becomes the stain edge.
            Material wearGrime = GrayboxRoomBuilder.Mat("ArtWearGrime",
                new Color(0.048f, 0.045f, 0.038f), doubleSided: true, smoothness: 0.07f,
                baseMap: ProceduralWearTextures.GetOrCreate(
                    ProceduralWearTextures.WearKind.EdgeGrime, 512, WearSeed, 1.0f),
                alphaClip: 0.46f);

            // Real rust, not a grade compensation - the radiator is a water fixture and this is
            // the one place in the room where a warm oxide colour is physically motivated.
            Material wearRust = GrayboxRoomBuilder.Mat("ArtWearRust",
                new Color(0.150f, 0.070f, 0.032f), doubleSided: true, smoothness: 0.11f,
                baseMap: ProceduralWearTextures.GetOrCreate(
                    ProceduralWearTextures.WearKind.Streak, 512, WearSeed + 5, 1.0f),
                alphaClip: 0.44f);

            Material wearCondensation = GrayboxRoomBuilder.Mat("ArtWearCondensation",
                new Color(0.058f, 0.060f, 0.064f), doubleSided: true, smoothness: 0.22f,
                baseMap: ProceduralWearTextures.GetOrCreate(
                    ProceduralWearTextures.WearKind.Streak, 512, WearSeed + 23, 0.7f),
                alphaClip: 0.52f);

            // The only transparent wear in Tier 1. Soft falloff is the whole point of a damp
            // patch, and transparency costs depth-write, so this stays a single quad.
            // MULTIPLY: these are stains, so the colour here is a DARKENING FACTOR, not a
            // surface albedo. 0.52 means "take the wall down to about half where the damp is
            // thickest"; the texture's alpha decides where that applies and fades it to nothing
            // at the quad edge. Authored as a neutral factor so it survives the C2 correction.
            Material wearDamp = GrayboxRoomBuilder.Mat("ArtWearDamp",
                new Color(0.52f, 0.54f, 0.58f), doubleSided: true, smoothness: 0.18f,
                baseMap: ProceduralWearTextures.GetOrCreate(
                    ProceduralWearTextures.WearKind.Bloom, 256, WearSeed + 11, 1.0f),
                multiply: true);

            // Slightly SMOOTHER than the floor it sits on: traffic polishes, it does not only
            // dirty. Getting that inversion right is what separates wear from more noise.
            Material wearScuff = GrayboxRoomBuilder.Mat("ArtWearScuff",
                new Color(0.052f, 0.049f, 0.042f), doubleSided: true, smoothness: 0.26f,
                baseMap: ProceduralWearTextures.GetOrCreate(
                    ProceduralWearTextures.WearKind.Scuff, 256, WearSeed + 31, 1.0f),
                alphaClip: 0.50f);

            BuildConduit(root, conduit, trim);
            BuildDisplayHousing(root, steel, stencilMat, indicator);
            BuildWindowSurround(root, trim);
            BuildRadiator(root, trim);
            BuildClutter(root, paper, grime, trim);
            BuildWear(root, wearGrime, wearRust, wearCondensation, wearDamp, wearScuff);

            Debug.Log($"[RoomArtDressing] built {root.childCount} dressing groups (collider-free)");
        }

        // ----------------------------------------------------------------- conduit

        /// <summary>
        /// The signature detail of the approved concept: surface-mounted conduit stapled along
        /// the ceiling and right wall, feeding the TV and the fluorescent, with slack cable
        /// sagging where nobody bothered to clip it. This is the single element that does most
        /// of the "someone has bodged this room" storytelling, so it gets real routing rather
        /// than a decorative squiggle - every run starts at a plausible source and ends at
        /// something that draws power.
        ///
        /// Runs sit 0.02m off their surface (inner faces: right wall x = 1.3, ceiling y = 2.3).
        /// </summary>
        private static void BuildConduit(Transform parent, Material conduit, Material clamp)
        {
            var g = new GameObject("Dressing_Conduit").transform;
            g.SetParent(parent, false);

            const float wx = 1.28f;   // just off the right wall
            const float cy = 2.27f;   // just under the ceiling

            // Main run: in from the far wall along the ceiling, then down the wall to the TV.
            var main = new[]
            {
                new Vector3(wx, cy, 1.92f),
                new Vector3(wx, cy, 0.30f),
                new Vector3(wx, 1.52f, 0.30f),
            };
            AddTube(g, "ConduitMain", "main", main, 0.016f, 8, conduit);
            ClampsAlong(g, clamp, main[0], main[1], 6, wx, true);
            ClampsAlong(g, clamp, main[1], main[2], 2, wx, true);

            // Branch across the ceiling to the fluorescent fixture at (0.85, 2.05, -0.05).
            //
            // This run used to stop at z = 0.86, which is where the tube USED to hang. Phase 6
            // pulled the fixture forward to z = -0.05 to keep the second bunk in shadow and the
            // conduit that supplies it was never moved with it, so the feed has been terminating
            // at bare ceiling ~0.9m short of the thing it powers. The whole point of routing this
            // network properly was that every run starts at a plausible source and ends at
            // something that draws power; this one ended at nothing. Caught while adding the
            // R7 soot halo, which marks where the tube actually is and would have made the
            // disconnect obvious.
            var feed = new[]
            {
                new Vector3(wx, cy, 0.95f),
                new Vector3(0.98f, cy, 0.95f),
                new Vector3(0.98f, cy, -0.05f),
                new Vector3(0.88f, cy, -0.05f),
                new Vector3(0.88f, 2.11f, -0.05f),
            };
            AddTube(g, "ConduitFixtureFeed", "feed", feed, 0.012f, 8, conduit);

            // A second, older ceiling line heading off toward the far wall - two runs reads as
            // accumulated bodging, one run reads as design.
            var second = new[]
            {
                new Vector3(wx, cy, 1.58f),
                new Vector3(0.12f, cy, 1.58f),
                new Vector3(0.12f, cy, 1.94f),
            };
            AddTube(g, "ConduitSecondary", "second", second, 0.012f, 8, conduit);

            // Junction boxes where runs change ownership.
            ArtBox(g, "JunctionCeiling", new Vector3(wx, cy - 0.005f, 0.95f),
                new Vector3(0.085f, 0.055f, 0.105f), clamp);
            ArtBox(g, "JunctionWall", new Vector3(wx - 0.005f, 1.62f, 0.30f),
                new Vector3(0.055f, 0.10f, 0.085f), clamp);

            // Slack cable: TV power dropping to a floor socket, and the desk's own tangle.
            AddCable(g, "CableTvDrop", "tvdrop", new[]
            {
                new Vector3(1.26f, 0.80f, 0.32f),
                new Vector3(1.26f, 0.09f, 0.72f),
                new Vector3(1.23f, 0.06f, 1.12f),
            }, 0.009f, 6, 0.085f, 10, conduit);

            AddCable(g, "CableDesk", "desk", new[]
            {
                new Vector3(1.22f, 0.71f, 1.74f),
                new Vector3(1.24f, 0.10f, 1.44f),
            }, 0.008f, 6, 0.11f, 10, conduit);
        }

        private static void AddTube(Transform parent, string name, string key, Vector3[] pts,
                                    float radius, int sides, Material mat)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            go.AddComponent<MeshFilter>().sharedMesh =
                ConduitMesh.GetOrCreateTube(key, pts, radius, sides);
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        }

        private static void AddCable(Transform parent, string name, string key, Vector3[] pts,
                                     float radius, int sides, float sag, int seg, Material mat)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            go.AddComponent<MeshFilter>().sharedMesh =
                ConduitMesh.GetOrCreateSaggingCable(key, pts, radius, sides, sag, seg);
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        }

        /// <summary>Evenly spaced fixing clamps, which is what makes a run read as stapled up
        /// by hand rather than floating.</summary>
        private static void ClampsAlong(Transform parent, Material mat, Vector3 a, Vector3 b,
                                        int count, float wallX, bool onWall)
        {
            for (int i = 0; i < count; i++)
            {
                float t = (i + 0.5f) / count;
                Vector3 p = Vector3.Lerp(a, b, t);
                Vector3 size = onWall
                    ? new Vector3(0.030f, 0.042f, 0.042f)
                    : new Vector3(0.042f, 0.030f, 0.042f);
                ArtBox(parent, $"Clamp_{i}", p, size, mat);
            }
        }

        // --------------------------------------------------------- display housing

        /// <summary>
        /// The display is not a television the occupant bought - it is bolted equipment an
        /// institution installed, in the same riveted painted-steel language as the bunk frames.
        /// Previous concepts kept reading as "a nice TV pasted onto a bad wall"; making the
        /// enclosure part of the room's own construction is what seats it.
        ///
        /// The glass is recessed by standing the FRAME proud of it rather than by moving the
        /// screen. The screen quad is functional geometry the TV slice composes against - its
        /// world position and size feed TvSweatScreen's canvas - so it is not ours to move.
        /// The housing front face sits ~42mm in front of the glass, which reads as a recess.
        /// </summary>
        private static void BuildDisplayHousing(Transform parent, Material steel, Material stencil,
                                                Material indicator)
        {
            var g = new GameObject("Dressing_DisplayHousing").transform;
            g.SetParent(parent, false);

            // Screen: centre (1.232, 1.1, 0.3), 0.98 along Z by 0.55 along Y, facing -X.
            const float cy = 1.1f, cz = 0.3f;
            const float halfZ = 0.49f, halfY = 0.275f;
            const float t = 0.085f;    // frame bar thickness
            const float fx = 1.245f;   // frame centre in X -> spans 1.19..1.30 (wall face)
            const float fd = 0.11f;    // frame depth
            const float face = 1.19f;  // front face of the housing

            ArtBox(g, "HousingTop", new Vector3(fx, cy + halfY + t * 0.5f, cz),
                new Vector3(fd, t, halfZ * 2f + t * 2f), steel);
            ArtBox(g, "HousingBottom", new Vector3(fx, cy - halfY - t * 0.5f, cz),
                new Vector3(fd, t, halfZ * 2f + t * 2f), steel);
            ArtBox(g, "HousingNear", new Vector3(fx, cy, cz - halfZ - t * 0.5f),
                new Vector3(fd, halfY * 2f, t), steel);
            ArtBox(g, "HousingFar", new Vector3(fx, cy, cz + halfZ + t * 0.5f),
                new Vector3(fd, halfY * 2f, t), steel);

            // Rivets around the bezel - the single detail that says "bolted in", so they are
            // spaced like real fixings rather than scattered.
            const float rx = face - 0.008f;
            for (int i = 0; i < 9; i++)
            {
                float z = cz - halfZ - t * 0.5f + i * ((halfZ * 2f + t) / 8f);
                Rivet(g, $"RivetT_{i}", new Vector3(rx, cy + halfY + t * 0.5f, z), steel);
                Rivet(g, $"RivetB_{i}", new Vector3(rx, cy - halfY - t * 0.5f, z), steel);
            }
            for (int i = 1; i < 4; i++)
            {
                float y = cy - halfY + i * (halfY * 2f / 4f);
                Rivet(g, $"RivetN_{i}", new Vector3(rx, y, cz - halfZ - t * 0.5f), steel);
                Rivet(g, $"RivetF_{i}", new Vector3(rx, y, cz + halfZ + t * 0.5f), steel);
            }

            // Stencilled equipment code on the bottom bar. Deliberately a different string from
            // the concept render's - the brief asked for that STYLE of marking, not that string.
            ArtQuad(g, "StencilCode", new Vector3(face - 0.004f, cy - halfY - t * 0.52f, cz + 0.20f),
                new Vector2(0.30f, 0.052f), Vector3.left, Vector3.up, stencil);

            // One small physical indicator lamp beside the panel. Emissive only, no Light
            // component - the room's lighting brief specifies its sources and this is not one.
            ArtBox(g, "IndicatorBezel", new Vector3(face + 0.012f, cy - halfY - t * 0.5f, cz - 0.38f),
                new Vector3(0.030f, 0.040f, 0.040f), steel);
            ArtBox(g, "IndicatorLamp", new Vector3(face - 0.006f, cy - halfY - t * 0.5f, cz - 0.38f),
                new Vector3(0.008f, 0.020f, 0.020f), indicator);

            // Conduit gland where the wall run enters the housing, so the feed is continuous
            // with the room's existing pipe runs rather than stopping short of them.
            ArtBox(g, "HousingGland", new Vector3(1.275f, cy + halfY + t + 0.055f, cz),
                new Vector3(0.075f, 0.075f, 0.075f), steel);
        }

        private static void Rivet(Transform parent, string name, Vector3 p, Material m) =>
            ArtBox(parent, name, p, new Vector3(0.016f, 0.024f, 0.024f), m);

        /// <summary>
        /// Stencil plate texture. Real glyphs from a compact 3x5 bitmap font rather than a blank
        /// plate - at couch distance a blank rectangle reads as a missing decal, and the marking
        /// is the detail that sells the panel as inventoried equipment.
        /// </summary>
        public static Texture2D GetOrCreateStencil(string code, int width, int height)
        {
            if (!AssetDatabase.IsValidFolder(TexFolder))
                AssetDatabase.CreateFolder("Assets/SBR/Environment", "Textures");

            string safe = code.Replace(" ", "_");
            string path = $"{TexFolder}/Stencil_{safe}.png";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null)
                return existing;

            var px = new Color[width * height];
            var plate = new Color(0.085f, 0.082f, 0.072f);
            var paint = new Color(0.62f, 0.60f, 0.52f);
            for (int i = 0; i < px.Length; i++) px[i] = plate;

            const int gw = 3, gh = 5, gap = 1;
            int cell = Mathf.Max(1, Mathf.Min(width / (code.Length * (gw + gap)), height / (gh + 2)));
            int originX = (width - code.Length * (gw + gap) * cell) / 2;
            int originY = (height - gh * cell) / 2;

            for (int c = 0; c < code.Length; c++)
            {
                string[] rows = Glyph(code[c]);
                if (rows == null) continue;
                for (int r = 0; r < gh; r++)
                for (int col = 0; col < gw; col++)
                {
                    if (rows[r][col] != '#') continue;
                    // Row 0 is the TOP of the glyph; texture Y runs upward.
                    int bx = originX + c * (gw + gap) * cell + col * cell;
                    int by = originY + (gh - 1 - r) * cell;
                    for (int dx = 0; dx < cell; dx++)
                    for (int dy = 0; dy < cell; dy++)
                    {
                        int x = bx + dx, y = by + dy;
                        if (x < 0 || x >= width || y < 0 || y >= height) continue;
                        px[y * width + x] = paint;
                    }
                }
            }

            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.SetPixels(px);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = true;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static string[] Glyph(char c) => c switch
        {
            '0' => new[] { "###", "#.#", "#.#", "#.#", "###" },
            '1' => new[] { ".#.", "##.", ".#.", ".#.", "###" },
            '2' => new[] { "###", "..#", "###", "#..", "###" },
            '3' => new[] { "###", "..#", "###", "..#", "###" },
            '4' => new[] { "#.#", "#.#", "###", "..#", "..#" },
            '5' => new[] { "###", "#..", "###", "..#", "###" },
            '7' => new[] { "###", "..#", "..#", "..#", "..#" },
            '9' => new[] { "###", "#.#", "###", "..#", "###" },
            'B' => new[] { "##.", "#.#", "##.", "#.#", "##." },
            'C' => new[] { "###", "#..", "#..", "#..", "###" },
            'M' => new[] { "#.#", "###", "###", "#.#", "#.#" },
            'R' => new[] { "##.", "#.#", "##.", "#.#", "#.#" },
            '-' => new[] { "...", "...", "###", "...", "..." },
            _ => null,   // space and anything unmapped render as bare plate
        };

        /// <summary>
        /// Collider-free quad, for decals whose UVs must run 0..1 across the face.
        ///
        /// T57: built from ChamferedBoxMesh.GetOrCreateQuad(size, Vector2.one) at localScale 1,
        /// not a scaled primitive - see GrayboxRoomBuilder.Quad for the full ruling. Vector2.one
        /// is required, not a default to retune: it reproduces the primitive's 0..1 UVs, which is
        /// what StencilCode's decal expects. With no primitive there is nothing to destroy a
        /// collider from, so this simply never adds one - "dressing adds zero colliders" stays
        /// true by construction, not by cleanup.
        /// </summary>
        private static GameObject ArtQuad(Transform parent, string name, Vector3 center,
                                          Vector2 size, Vector3 facing, Vector3 up, Material mat)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = center;
            go.transform.rotation = Quaternion.LookRotation(-facing.normalized, up);
            go.transform.localScale = Vector3.one; // mesh carries true world size, not the transform
            go.AddComponent<MeshFilter>().sharedMesh =
                ChamferedBoxMesh.GetOrCreateQuad(size, Vector2.one);
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        // ------------------------------------------------------------------ window

        /// <summary>
        /// A recessed frame and a proud sill around the builder's window pane. In the approved
        /// concept the window is the room's only connection to outside and the single most
        /// valuable element in frame; as a bare quad flush on the wall it read as a boarded panel.
        /// The builder's pane stays authoritative - this only surrounds it.
        /// </summary>
        private static void BuildWindowSurround(Transform parent, Material trim)
        {
            var g = new GameObject("Dressing_Window").transform;
            g.SetParent(parent, false);

            const float z = 1.955f;      // just proud of the far wall's inner face at z = 2.0
            const float cx = 0f, cy = 1.4f;
            const float halfX = 0.60f, halfY = 0.50f;   // pane is 1.2 x 1.0
            const float t = 0.075f;      // frame bar thickness
            const float d = 0.09f;       // how far the frame stands off the wall

            // Chamfered industrial reveal (concept render C, approved 2026-07-27): two concentric
            // rings at different depths rather than one flat frame. The outer ring sits proud on
            // the wall, the inner ring steps back toward the glass, and the step between them
            // reads as an angled reveal - a bunker window punched through a thick wall, not a
            // picture frame stuck on a flat surface.
            const float od = 0.055f;                 // outer ring, proud of the wall
            const float id_ = 0.125f;                // inner ring, stepped back toward the glass
            const float ot = 0.105f, it = 0.055f;    // ring bar thicknesses

            // Outer ring - the proud edge.
            ArtBox(g, "RevealOuterTop", new Vector3(cx, cy + halfY + ot * 0.5f, z - 0.02f),
                new Vector3(halfX * 2f + ot * 2f, ot, od), trim);
            ArtBox(g, "RevealOuterLeft", new Vector3(cx - halfX - ot * 0.5f, cy, z - 0.02f),
                new Vector3(ot, halfY * 2f + ot * 2f, od), trim);
            ArtBox(g, "RevealOuterRight", new Vector3(cx + halfX + ot * 0.5f, cy, z - 0.02f),
                new Vector3(ot, halfY * 2f + ot * 2f, od), trim);

            // Inner ring - stepped back, narrower. The offset between the two is the chamfer.
            ArtBox(g, "RevealInnerTop", new Vector3(cx, cy + halfY + it * 0.5f, z + 0.03f),
                new Vector3(halfX * 2f + it * 2f, it, id_), trim);
            ArtBox(g, "RevealInnerLeft", new Vector3(cx - halfX - it * 0.5f, cy, z + 0.03f),
                new Vector3(it, halfY * 2f, id_), trim);
            ArtBox(g, "RevealInnerRight", new Vector3(cx + halfX + it * 0.5f, cy, z + 0.03f),
                new Vector3(it, halfY * 2f, id_), trim);

            // Heavy sill - deeper and proud. It is what sells the wall as having real thickness,
            // and it is the surface the window's short-throw blue actually pools on.
            ArtBox(g, "Sill", new Vector3(cx, cy - halfY - ot * 0.55f, z - 0.055f),
                new Vector3(halfX * 2f + ot * 2f, ot * 1.2f, od + 0.13f), trim);
        }

        /// <summary>
        /// Cast-iron radiator under the window (concept render C). Cheap, and it does real work:
        /// it gives the window wall an object at floor level to catch the short-throw blue, so
        /// the pool has something to fall across instead of dying on bare floor.
        /// </summary>
        private static void BuildRadiator(Transform parent, Material trim)
        {
            var g = new GameObject("Dressing_Radiator").transform;
            g.SetParent(parent, false);

            ArtBox(g, "RadiatorBody", new Vector3(0f, 0.34f, 1.88f),
                new Vector3(0.78f, 0.52f, 0.11f), trim);
            // Fins, read as a ribbed silhouette rather than modelled individually.
            for (int i = 0; i < 7; i++)
            {
                float x = -0.30f + i * 0.10f;
                ArtBox(g, $"RadiatorFin_{i}", new Vector3(x, 0.34f, 1.845f),
                    new Vector3(0.045f, 0.46f, 0.055f), trim);
            }
            ArtBox(g, "RadiatorFeedPipe", new Vector3(0.36f, 0.16f, 1.86f),
                new Vector3(0.045f, 0.30f, 0.045f), trim);
        }

        // ----------------------------------------------------------------- clutter

        /// <summary>
        /// Restrained per the approved gate: a few heavy pieces in small clusters, not a trash
        /// mountain. Every item sits clear of the central walking lane and off the interaction
        /// surfaces of the laptop and phone.
        /// </summary>
        private static void BuildClutter(Transform parent, Material paper, Material grime, Material trim)
        {
            var g = new GameObject("Dressing_Clutter").transform;
            g.SetParent(parent, false);

            // Desk top sits at y = 0.75 (0.73 centre + 0.02 half-thickness), spanning
            // x 0.80..1.30, z 0.90..2.00. The laptop occupies ~z 1.46..1.78, phone ~z 1.08..1.23.
            ArtBox(g, "PaperStack", new Vector3(0.94f, 0.767f, 1.94f),
                new Vector3(0.20f, 0.034f, 0.27f), paper);
            ArtBox(g, "PaperStackTop", new Vector3(0.97f, 0.788f, 1.92f),
                new Vector3(0.18f, 0.008f, 0.24f), paper);
            ArtBox(g, "Ashtray", new Vector3(1.19f, 0.762f, 1.03f),
                new Vector3(0.115f, 0.024f, 0.115f), grime);
            ArtBox(g, "DeadCan", new Vector3(1.22f, 0.783f, 1.31f),
                new Vector3(0.062f, 0.066f, 0.062f), trim);

            // Floor cluster, tucked under the desk against the right wall - outside the aisle.
            ArtBox(g, "FloorBox", new Vector3(1.16f, 0.055f, 1.72f),
                new Vector3(0.24f, 0.11f, 0.30f), paper);

            // Bunk 1: one thin folded blanket on the slab (slab top is y = 1.58).
            ArtBox(g, "Blanket", new Vector3(-0.86f, 1.605f, 0.72f),
                new Vector3(0.66f, 0.05f, 0.62f), grime);

            // Bunk 2, over the desk. Dressed as though someone uses it - mattress, rumpled
            // bedding, a pillow - but it sits outside every light cone in the room and stays
            // in shadow. Allen's note on concept C: it should be legible as OCCUPIED, never
            // legible as EMPTY. That ambiguity is the point, so nothing here is lit to confirm.
            // R19(c): mattress fabric colour is the palette's Drab green #3A4230, matching the
            // bunk frames (GrayboxRoomBuilder.BunkFrame) rather than its own bespoke near-black.
            // The couch is a separate "couch fabric" in the design doc and is untouched by this
            // ruling - it carries a design-verified relief result that must not be disturbed.
            //
            // FLAGGED, NOT FIXED: drab green's luminance (0.0501) is brighter than the material
            // it replaces here (0.058/0.055/0.047) and brighter than the Bunk2Dark frame colour
            // it also replaces elsewhere (0.0389). Bunk 2's mattress is a ratified test at
            // 43.9 +/-1 measured luminance, and this change may lift it. That is a known,
            // deliberate, measured risk - the lead will measure and escalate if it breaks. Do
            // NOT compensate, darken, or "fix" it here.
            Material bunkShadow = GrayboxRoomBuilder.Mat("ArtBunk2Shadow",
                new Color(0.0423f, 0.0545f, 0.0296f), smoothness: 0.03f);

            ArtBox(g, "Bunk2Mattress", new Vector3(0.92f, 1.615f, 1.28f),
                new Vector3(0.70f, 0.07f, 1.34f), bunkShadow);
            ArtBox(g, "Bunk2Bedding", new Vector3(0.95f, 1.665f, 1.52f),
                new Vector3(0.62f, 0.05f, 0.78f), bunkShadow);
            // The pillow is the one thing allowed to catch a little light - a single pale shape
            // is what makes the bunk read as SLEPT IN rather than as an empty shelf. Everything
            // else up there stays in shadow.
            ArtBox(g, "Bunk2Pillow", new Vector3(0.90f, 1.678f, 0.78f),
                new Vector3(0.46f, 0.09f, 0.26f), grime);

            // Desk lamp housing for the fourth light source - a clamp-on task lamp. The light
            // itself is built in GrayboxRoomBuilder.BuildLighting (DeskLampLight).
            ArtBox(g, "DeskLampStem", new Vector3(1.245f, 1.10f, 1.80f),
                new Vector3(0.030f, 0.62f, 0.030f), trim);
            ArtBox(g, "DeskLampArm", new Vector3(1.185f, 1.395f, 1.755f),
                new Vector3(0.155f, 0.028f, 0.115f), trim);
            ArtBox(g, "DeskLampShade", new Vector3(1.115f, 1.335f, 1.720f),
                new Vector3(0.145f, 0.090f, 0.145f), trim);
        }

        // ------------------------------------------------------------------- R7 wear

        /// <summary>
        /// R7 Tier 1. Localised wear, placed against causes the room's own construction supplies.
        ///
        /// R6 created the need for this. Before indirect light the skirting line, the corners and
        /// the space under the bunks were near-black, so their uniformity was invisible. Bounce
        /// now reaches all of them, and the remaining tell is no longer "this room is flat" but
        /// "this room is evenly dirty" - every surface map tiles at world scale, so a wall is
        /// identically grimy at the ceiling and at the floor. Real dirt has causes. This room
        /// supplies four: a water fixture, a cold pane, one walking lane, and gravity.
        ///
        /// Everything here is collider-free and lives under <see cref="WearRootName"/>, which
        /// GrayboxRoomBuilder excludes from the probe bake.
        /// </summary>
        private static void BuildWear(Transform parent, Material grime, Material rust,
                                      Material condensation, Material damp, Material scuff)
        {
            var g = new GameObject(WearRootName).transform;
            g.SetParent(parent, false);

            const float off = 0.003f;    // 3mm off the surface: true world size, so this is literal
            const float skirt = 0.45f;   // the coverage field fades out well inside this
            const float tile = 1.2f;     // metres per texture repeat along a run

            // R7.1 SKIRTING GRIME. The longest continuous line in the room and the most reliable
            // dirt gradient there is - everything that settles ends up at the bottom of a wall.
            // Runs the full length of all four walls including behind the couch and the radiator,
            // because dirt does not stop where the furniture starts.
            float y = skirt * 0.5f;
            WearQuad(g, "SkirtLeft", new Vector3(-HalfW + off, y, 0f), new Vector2(HalfL * 2f, skirt),
                     Vector3.right, Vector3.up, grime, HalfL * 2f / tile);
            WearQuad(g, "SkirtRight", new Vector3(HalfW - off, y, 0f), new Vector2(HalfL * 2f, skirt),
                     Vector3.left, Vector3.up, grime, HalfL * 2f / tile);
            WearQuad(g, "SkirtFar", new Vector3(0f, y, HalfL - off), new Vector2(HalfW * 2f, skirt),
                     Vector3.back, Vector3.up, grime, HalfW * 2f / tile);
            WearQuad(g, "SkirtDoor", new Vector3(0f, y, -HalfL + off), new Vector2(HalfW * 2f, skirt),
                     Vector3.forward, Vector3.up, grime, HalfW * 2f / tile);

            // R7.2 RADIATOR RUST. Down the front face of the radiator body, which sits at
            // z = 1.88 with depth 0.11, so its front plane is z = 1.825. Best-motivated wear in
            // the room: the cause and the effect are in the same frame.
            WearQuad(g, "RadiatorRust", new Vector3(0.06f, 0.30f, 1.825f - off),
                     new Vector2(0.56f, 0.44f), Vector3.back, Vector3.up, rust, 1f);

            // Rising damp on the wall the radiator is bolted to. Sits in front of the skirting
            // and behind the condensation run, so the three read as one damp story rather than
            // three unrelated stains.
            WearQuad(g, "RadiatorDamp", new Vector3(0f, 0.66f, HalfL - off * 1.8f),
                     new Vector2(1.05f, 0.72f), Vector3.back, Vector3.up, damp, 1f);

            // R7.3 WINDOW CONDENSATION. The pane at (0, 1.4, 1.99) is the coldest surface in the
            // room and sits directly above a heat source; water condenses on it and runs down the
            // wall toward the radiator.
            WearQuad(g, "WindowRun", new Vector3(0f, 1.02f, HalfL - off),
                     new Vector2(0.95f, 0.44f), Vector3.back, Vector3.up, condensation, 1f);

            // R7.4 FLOOR TRAFFIC. The walkable lane is bounded by the couch front face at
            // x = -0.60 and the desk legs at x = 0.855, so the track sits just off centre. This
            // is the one piece of wear that implies a person without showing one.
            WearQuad(g, "TrafficPath", new Vector3(0.13f, off, 0f), new Vector2(1.15f, 3.70f),
                     Vector3.up, Vector3.forward, scuff, 1f);

            // Where the stool at (0.55, 0.225, 1.45) gets dragged in and out from the desk.
            WearQuad(g, "StoolScuff", new Vector3(0.55f, off * 1.5f, 1.30f),
                     new Vector2(0.58f, 0.52f), Vector3.up, Vector3.forward, scuff, 1f);

            // R7.5 CEILING SOOT. FluorescentKey (GrayboxRoomBuilder) sits at (0.85, 2.05, -0.05)
            // and is deliberately a failing tube - one that has earned its own key light runs hot
            // and dirty, and heat plus soot from it settle on the ceiling directly above. The
            // ceiling is the room's most visible surface, so this is the highest-value placement
            // in the tier: nothing else in Tier 1 touched it at all.
            // NO CEILING SOOT. Dropped by the Design Director 2026-07-31: the ceiling already
            // reads, and it is the room's best-reading surface because the tube rakes it at
            // theta ~= 87deg (standing law R12 - surface detail is gated by lighting, not
            // texture authoring). A stain there would be spent on the one surface that does not
            // need help. The rendering contradiction that held the quad back is therefore moot
            // and is not worth carrying: see docs/6-memo/2026-07-31-room-design-review-package.md
            // section 5.1 if it ever needs to be re-opened.

            // R7.6 CONDUIT DRIP. The main run clings to the right wall (inner face x = 1.3) on
            // its way down to the TV; moisture that beads on the surface-mounted pipe has nowhere
            // to go but down the wall it is stapled to, so the wall stains along the run rather
            // than the pipe itself.
            WearQuad(g, "ConduitDrip", new Vector3(HalfW - off, 1.55f, 0.55f),
                     new Vector2(0.70f, 0.90f), Vector3.left, Vector3.up, condensation, 1f);
        }

        /// <summary>
        /// A flat decal quad with NO collider, oriented like the builder's screens: local -Z is
        /// aimed along <paramref name="facing"/>, so the surface normal ends up facing the room.
        /// Double-sided materials mean a facing mistake can never blank a decal.
        /// </summary>
        private static GameObject WearQuad(Transform parent, string name, Vector3 center,
                                           Vector2 size, Vector3 facing, Vector3 up,
                                           Material mat, float uRepeats)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = center;
            go.transform.rotation = Quaternion.LookRotation(-facing.normalized, up);
            go.transform.localScale = Vector3.one;

            // V always maps exactly once: every wear field is a gradient down its height, and
            // repeating it vertically would stack tide-lines up the wall.
            go.AddComponent<MeshFilter>().sharedMesh =
                ChamferedBoxMesh.GetOrCreateQuad(size, new Vector2(Mathf.Max(uRepeats, 0.05f), 1f));
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;

            // DIRT DOES NOT CAST SHADOWS. A MeshRenderer defaults to shadowCastingMode.On, so
            // every decal was throwing a hard-edged shadow of its own quad. The ceiling soot was
            // the obvious victim - a 1.3m plane 4mm under the ceiling with the fluorescent below
            // it projected a crisp RECTANGLE onto the brightest surface in the room - but the
            // skirting was doing the same thing to the walls and the traffic path to the floor.
            // The stain is meant to be a mark ON a surface, not an object floating in front of it.
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = true;   // it must still sit inside the room's lighting
            return go;
        }

        // ------------------------------------------------------------------ helper

        /// <summary>
        /// A chamfered box with NO collider. This is the whole contract of this file: dressing is
        /// visual only and must never alter the room's physical shape.
        /// </summary>
        private static GameObject ArtBox(Transform parent, string name, Vector3 center,
                                         Vector3 size, Material mat)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = center;
            go.transform.localScale = Vector3.one;
            go.AddComponent<MeshFilter>().sharedMesh =
                ChamferedBoxMesh.GetOrCreate(size, ChamferedBoxMesh.DefaultBevel(size));
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        // ------------------------------------------------------------- night city

        /// <summary>
        /// The view through the window. Generated rather than painted: a dark skyline of stacked
        /// blocks with a scatter of lit windows, mostly warm amber with a few cold ones. Assigned
        /// to the builder's WindowGlow material as both base and emission map, so the pane stops
        /// being a flat blue rectangle and becomes somewhere the player is not.
        ///
        /// Non-tiling by design - it maps 0..1 across the window quad exactly once.
        /// </summary>
        public static Texture2D GetOrCreateNightCity(int width, int height, int seed)
        {
            if (!AssetDatabase.IsValidFolder(TexFolder))
                AssetDatabase.CreateFolder("Assets/SBR/Environment", "Textures");

            string path = $"{TexFolder}/NightCity_{width}x{height}_{seed}.png";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null)
                return existing;

            var rng = new System.Random(seed);
            var px = new Color[width * height];

            // Sky: near-black at the top, faintly warmer toward the horizon from city glow.
            for (int y = 0; y < height; y++)
            {
                float t = y / (float)(height - 1);           // 0 bottom, 1 top
                Color sky = Color.Lerp(new Color(0.055f, 0.062f, 0.088f),
                                       new Color(0.012f, 0.014f, 0.028f), t);
                for (int x = 0; x < width; x++)
                    px[y * width + x] = sky;
            }

            // Skyline: overlapping blocks of varying width and height, drawn back to front.
            int cursor = -8;
            while (cursor < width)
            {
                int bw = 14 + rng.Next(30);
                int bh = Mathf.RoundToInt(height * (0.18f + (float)rng.NextDouble() * 0.42f));
                float depth = 0.55f + (float)rng.NextDouble() * 0.45f;   // nearer = darker
                var body = new Color(0.020f * depth, 0.023f * depth, 0.034f * depth);

                for (int x = cursor; x < cursor + bw && x < width; x++)
                {
                    if (x < 0) continue;
                    for (int y = 0; y < bh && y < height; y++)
                        px[y * width + x] = body;
                }

                // Lit windows on a regular grid, most dark, a scatter alight.
                const int cw = 3, ch = 4, gap = 3;
                for (int wy = 6; wy < bh - ch - 2; wy += ch + gap)
                for (int wx = cursor + 4; wx < cursor + bw - cw - 2; wx += cw + gap)
                {
                    if (rng.NextDouble() > 0.30) continue;
                    // Mostly warm sodium; a few cold fluorescent offices.
                    Color lit = rng.NextDouble() < 0.82
                        ? new Color(0.95f, 0.66f, 0.28f)
                        : new Color(0.62f, 0.78f, 0.92f);
                    lit *= 0.55f + (float)rng.NextDouble() * 0.45f;

                    for (int x = wx; x < wx + cw; x++)
                    for (int y = wy; y < wy + ch; y++)
                    {
                        if (x < 0 || x >= width || y < 0 || y >= height) continue;
                        px[y * width + x] = lit;
                    }
                }

                cursor += bw + rng.Next(3);
            }

            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.SetPixels(px);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.wrapMode = TextureWrapMode.Clamp;   // must not tile across the pane
            importer.filterMode = FilterMode.Bilinear;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = true;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
