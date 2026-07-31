using System.IO;
using UnityEditor;
using UnityEngine;

namespace SBR
{
    /// <summary>
    /// R7 — deterministic wear masks: tide-lines, drips, damp blooms and traffic scuff.
    ///
    /// These are NOT surface maps. <see cref="ProceduralSurfaceTextures"/> answers "what is this
    /// material" and tiles everywhere; this file answers "what happened HERE", and every texture
    /// it makes is placed against a specific cause in the room's construction — a radiator, a
    /// cold pane, a walking lane.
    ///
    /// THE ALPHA CHANNEL IS A COVERAGE FIELD, NOT A FINISHED SHAPE. It stores a smooth scalar and
    /// lets the material's `_Cutoff` choose the contour, so the same texture gives a small stain
    /// or a large one by moving one float — no regeneration, no second asset. That is also why
    /// the boundaries read as organic: the field is a gradient plus noise, and any level set
    /// through it is ragged.
    ///
    /// RGB IS DELIBERATELY NEAR-NEUTRAL (a little value break, no hue). Colour comes from the
    /// material's `_BaseColor`, which keeps every wear tint authored in one place. That is a
    /// requirement, not a preference: per the C2 interim ruling the TV's green spill is
    /// temporary and becomes cold white-grey at TV Phase 3, so wear near the display must not
    /// bake in a colour that was chosen to sit against green.
    ///
    /// Same discipline as the surface pipeline: seeded, stable across rebuilds, cached by asset
    /// path so a rebuild reuses rather than regenerates.
    /// </summary>
    public static class ProceduralWearTextures
    {
        private const string TexFolder = "Assets/SBR/Environment/Textures";

        public enum WearKind
        {
            /// <summary>Dirt tide-line along a floor/wall junction. Tiles horizontally.</summary>
            EdgeGrime,
            /// <summary>Drips running down from a leak or a cold pane. Tiles horizontally.</summary>
            Streak,
            /// <summary>Soft damp patch. Clamped both axes; used with a transparent material.</summary>
            Bloom,
            /// <summary>Worn traffic patch for the floor. Tiles along U.</summary>
            Scuff,
        }

        public static Texture2D GetOrCreate(WearKind kind, int res, int seed, float shape = 1f)
        {
            if (!AssetDatabase.IsValidFolder(TexFolder))
                AssetDatabase.CreateFolder("Assets/SBR/Environment", "Textures");

            string path = $"{TexFolder}/Wear_{kind}_{res}_{seed}_{shape:0.00}.png";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null)
                return existing;

            Color[] px = kind switch
            {
                WearKind.EdgeGrime => BuildEdgeGrime(res, seed, shape),
                WearKind.Streak    => BuildStreak(res, seed, shape),
                WearKind.Bloom     => BuildBloom(res, seed, shape),
                _                  => BuildScuff(res, seed, shape),
            };

            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.SetPixels(px);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            // Bloom is a lone patch and must not repeat; the rest tile along the run they follow.
            imp.wrapModeU = kind == WearKind.Bloom ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;
            imp.wrapModeV = TextureWrapMode.Clamp;   // every kind fades out vertically by design
            imp.filterMode = FilterMode.Bilinear;
            imp.sRGBTexture = true;
            imp.alphaSource = TextureImporterAlphaSource.FromInput;
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled = true;
            // Uncompressed: DXT5 quantises alpha in 4x4 blocks, and since _Cutoff slices a level
            // set through that channel the artefact would show up directly as a blocky stain
            // edge. These are small textures; the memory is not worth the ugliness.
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // ------------------------------------------------------------------- fields

        /// <summary>
        /// Dirt climbing a wall from the floor. Real tide-lines are not level - they are a height
        /// gradient broken by noise, which is exactly what this stores.
        /// <paramref name="shape"/> scales how far up the wall the dirt reaches.
        /// </summary>
        private static Color[] BuildEdgeGrime(int res, int seed, float shape)
        {
            var px = new Color[res * res];
            float reach = Mathf.Clamp01(0.55f * shape);

            for (int y = 0; y < res; y++)
            {
                float v = y / (float)(res - 1);              // 0 at the floor
                float baseCov = 1f - Smooth01(v / reach);
                for (int x = 0; x < res; x++)
                {
                    float u = x / (float)res;
                    // Two scales: a slow wander that makes the line meander along the wall, and
                    // a fine break-up so the contour is not a smooth curve either.
                    float wander = Fbm(u * 3f, v * 3f, 3, 3, seed, 3);
                    float fine   = Fbm(u * 17f, v * 17f, 17, 17, seed + 91, 3);
                    float cov = baseCov + (wander - 0.5f) * 0.55f + (fine - 0.5f) * 0.22f;
                    px[y * res + x] = Neutral(fine, Mathf.Clamp01(cov));
                }
            }
            return px;
        }

        /// <summary>
        /// Drips from a leak or condensation. Each runs a different distance, because they start
        /// at different times and dry at different rates - uniform-length streaks read as a comb.
        /// </summary>
        private static Color[] BuildStreak(int res, int seed, float shape)
        {
            var px = new Color[res * res];
            var rng = new System.Random(seed);
            int count = Mathf.Max(4, Mathf.RoundToInt(11 * shape));

            var cx = new float[count];
            var halfW = new float[count];
            var len = new float[count];
            var gain = new float[count];
            for (int i = 0; i < count; i++)
            {
                cx[i] = (float)rng.NextDouble();
                halfW[i] = 0.004f + (float)rng.NextDouble() * 0.017f;
                len[i] = 0.25f + (float)rng.NextDouble() * 0.70f;
                gain[i] = 0.55f + (float)rng.NextDouble() * 0.45f;
            }

            for (int y = 0; y < res; y++)
            {
                float v = y / (float)(res - 1);
                float down = 1f - v;                          // 0 at the top, where drips start
                for (int x = 0; x < res; x++)
                {
                    float u = x / (float)res;
                    float best = 0f;
                    for (int i = 0; i < count; i++)
                    {
                        // Wrap the horizontal distance so streaks tile across the seam.
                        float dx = Mathf.Abs(u - cx[i]);
                        dx = Mathf.Min(dx, 1f - dx);
                        if (dx > halfW[i] * 3f) continue;

                        float across = 1f - Smooth01(dx / halfW[i]);
                        // A drip is strongest where it starts and thins as it runs out of water.
                        float along = 1f - Smooth01(down / len[i]);
                        best = Mathf.Max(best, across * along * gain[i]);
                    }
                    float grain = Fbm(u * 23f, v * 9f, 23, 9, seed + 17, 2);
                    float cov = best * (0.75f + grain * 0.5f);
                    px[y * res + x] = Neutral(grain, Mathf.Clamp01(cov));
                }
            }
            return px;
        }

        /// <summary>
        /// A damp patch. Smooth alpha rather than a field, because this one is used with a
        /// transparent material where the softness IS the effect and nothing thresholds it.
        /// </summary>
        private static Color[] BuildBloom(int res, int seed, float shape)
        {
            var px = new Color[res * res];
            float radius = Mathf.Clamp(0.42f * shape, 0.12f, 0.49f);

            for (int y = 0; y < res; y++)
            {
                float v = y / (float)(res - 1);
                for (int x = 0; x < res; x++)
                {
                    float u = x / (float)(res - 1);
                    float d = Mathf.Sqrt((u - 0.5f) * (u - 0.5f) + (v - 0.5f) * (v - 0.5f));
                    // Perturb the radius so the edge is a lobed blotch, not a circle.
                    float lobe = Fbm(u * 2.5f, v * 2.5f, 5, 5, seed + 43, 3);
                    float r = radius * (0.7f + lobe * 0.6f);
                    float cov = 1f - Smooth01(d / Mathf.Max(r, 0.02f));
                    // Damp is darkest at the centre and never fully opaque at the rim.
                    px[y * res + x] = Neutral(lobe, Mathf.Clamp01(cov * cov * 0.85f));
                }
            }
            return px;
        }

        /// <summary>
        /// Traffic wear: irregular patches where feet have polished or dirtied a lane. Fades at
        /// both V edges so the path has soft sides rather than a cut-out strip.
        /// </summary>
        private static Color[] BuildScuff(int res, int seed, float shape)
        {
            var px = new Color[res * res];

            for (int y = 0; y < res; y++)
            {
                float v = y / (float)(res - 1);
                // Soft shoulders: full strength down the middle of the lane, gone at the kerbs.
                float lane = 1f - Smooth01(Mathf.Abs(v - 0.5f) / 0.42f);
                for (int x = 0; x < res; x++)
                {
                    float u = x / (float)res;
                    float broad = Fbm(u * 4f, v * 4f, 4, 4, seed + 7, 3);
                    float fine  = Fbm(u * 15f, v * 15f, 15, 15, seed + 61, 2);
                    float cov = lane * (broad * 0.75f + fine * 0.45f) * shape;
                    px[y * res + x] = Neutral(fine, Mathf.Clamp01(cov));
                }
            }
            return px;
        }

        // ------------------------------------------------------------------ helpers

        /// <summary>
        /// RGB carries a small value break only - no hue. See the class summary: wear colour is
        /// the material's job so it stays correctable when the TV green becomes white-grey.
        /// </summary>
        private static Color Neutral(float variation, float coverage)
        {
            float g = 0.78f + variation * 0.22f;
            return new Color(g, g, g, coverage);
        }

        private static float Smooth01(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * (3f - 2f * t);
        }

        /// <summary>
        /// Value noise on a wrapping lattice, so any texture built from it tiles exactly. The
        /// period is in lattice cells and must match the frequency the caller samples at.
        /// </summary>
        private static float ValueNoise(float x, float y, int periodX, int periodY, int seed)
        {
            int xi = Mathf.FloorToInt(x), yi = Mathf.FloorToInt(y);
            float xf = x - xi, yf = y - yi;

            float a = Hash(Wrap(xi, periodX), Wrap(yi, periodY), seed);
            float b = Hash(Wrap(xi + 1, periodX), Wrap(yi, periodY), seed);
            float c = Hash(Wrap(xi, periodX), Wrap(yi + 1, periodY), seed);
            float d = Hash(Wrap(xi + 1, periodX), Wrap(yi + 1, periodY), seed);

            float u = Smooth01(xf), v = Smooth01(yf);
            return Mathf.Lerp(Mathf.Lerp(a, b, u), Mathf.Lerp(c, d, u), v);
        }

        private static float Fbm(float x, float y, int periodX, int periodY, int seed, int octaves)
        {
            float sum = 0f, amp = 0.5f, norm = 0f;
            int px = periodX, py = periodY;
            for (int o = 0; o < octaves; o++)
            {
                sum += ValueNoise(x, y, px, py, seed + o * 131) * amp;
                norm += amp;
                x *= 2f; y *= 2f; px *= 2; py *= 2; amp *= 0.5f;
            }
            return sum / norm;
        }

        private static int Wrap(int v, int period) => ((v % period) + period) % period;

        private static float Hash(int x, int y, int seed)
        {
            int h = x * 374761393 + y * 668265263 + seed * 1274126177;
            h = (h ^ (h >> 13)) * 1274126177;
            return ((h ^ (h >> 16)) & 0x7fffffff) / (float)0x7fffffff;
        }
    }
}
