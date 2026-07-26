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
        private const string GeneratedRootName = "RoomArtGenerated";

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

            Material conduit = GrayboxRoomBuilder.Mat("ArtConduit",
                new Color(0.038f, 0.036f, 0.032f), smoothness: 0.34f);

            BuildConduit(root, conduit, trim);
            BuildWindowSurround(root, trim);
            BuildClutter(root, paper, grime, trim);

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

            // Branch across the ceiling to the fluorescent fixture at (0.95, 2.06, 0.85).
            var feed = new[]
            {
                new Vector3(wx, cy, 0.95f),
                new Vector3(0.98f, cy, 0.95f),
                new Vector3(0.98f, cy, 0.86f),
                new Vector3(0.98f, 2.11f, 0.86f),
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

            ArtBox(g, "FrameTop", new Vector3(cx, cy + halfY + t * 0.5f, z),
                new Vector3(halfX * 2f + t * 2f, t, d), trim);
            ArtBox(g, "FrameLeft", new Vector3(cx - halfX - t * 0.5f, cy, z),
                new Vector3(t, halfY * 2f, d), trim);
            ArtBox(g, "FrameRight", new Vector3(cx + halfX + t * 0.5f, cy, z),
                new Vector3(t, halfY * 2f, d), trim);
            // The sill is deeper and sits proud - it is what sells the wall as having thickness.
            ArtBox(g, "Sill", new Vector3(cx, cy - halfY - t * 0.5f, z - 0.035f),
                new Vector3(halfX * 2f + t * 2f, t * 1.3f, d + 0.07f), trim);
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

            // Bunk: one thin folded blanket on the slab (slab top is y = 1.58).
            ArtBox(g, "Blanket", new Vector3(-0.86f, 1.605f, 0.72f),
                new Vector3(0.66f, 0.05f, 0.62f), grime);
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
