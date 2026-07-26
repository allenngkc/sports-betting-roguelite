using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Random = System.Random; // disambiguate from UnityEngine.Random, which we must not use

namespace SBR
{
    /// <summary>
    /// Deterministic, seamlessly tileable greyscale-ish variation maps for URP/Lit _BaseMap.
    /// URP multiplies _BaseColor by the base map, so everything here is centred near white
    /// with darker detail, modulating a tint rather than replacing it.
    ///
    /// TILING APPROACH: every pattern is built either from
    ///   (a) lattice value noise hashed AFTER wrapping coordinates modulo an integer cell
    ///       count ("period"), which makes it mathematically exact that f(x) == f(x + period), or
    ///   (b) toroidal (wrap-around) distance to placed features (patches/blotches/scuffs), so a
    ///       feature straddling an edge reappears correctly on the opposite edge, or
    ///   (c) plain sine/cosine over a normalized [0,1) coordinate times an integer count, which
    ///       is periodic in that coordinate by construction.
    /// No mirroring/blending hacks are needed because every ingredient is periodic already.
    /// </summary>
    public static class ProceduralSurfaceTextures
    {
        public enum SurfaceKind { Plaster, WornFloor, CeilingStain, FabricWeave }

        private const string ParentFolder = "Assets/SBR/Environment";
        private const string FolderPath = ParentFolder + "/Textures";
        private const int PermMask = 4095; // permutation table is 4096 entries long

        /// <summary>
        /// Expands luminance away from the map's OWN mean rather than a fixed 0.5 pivot. That
        /// matters: these maps sit high (0.7-1.0) so pivoting on 0.5 would just brighten them.
        /// Pivoting on the actual mean preserves average surface brightness while pushing dirt,
        /// wear and stains apart, which is what makes grime read as story at 3m instead of as
        /// gentle noise.
        /// </summary>
        private static void ApplyContrast(Color[] px, float contrast)
        {
            float mean = 0f;
            for (int i = 0; i < px.Length; i++) mean += px[i].g;
            mean /= px.Length;

            for (int i = 0; i < px.Length; i++)
            {
                px[i] = new Color(
                    Mathf.Clamp01(mean + (px[i].r - mean) * contrast),
                    Mathf.Clamp01(mean + (px[i].g - mean) * contrast),
                    Mathf.Clamp01(mean + (px[i].b - mean) * contrast));
            }
        }

        public static Texture2D GetOrCreate(SurfaceKind kind, int resolution, int seed,
                                            float contrast = 1f)
        {
            string assetPath = Mathf.Approximately(contrast, 1f)
                ? $"{FolderPath}/{kind}_{resolution}_{seed}.png"
                : $"{FolderPath}/{kind}_{resolution}_{seed}_c{Mathf.RoundToInt(contrast * 100f)}.png";

            Texture2D existing = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (existing != null)
                return existing;

            if (!AssetDatabase.IsValidFolder(FolderPath))
                AssetDatabase.CreateFolder(ParentFolder, "Textures");

            var rng = new Random(seed);
            int[] perm = BuildPermutation(rng);

            Color[] pixels = kind switch
            {
                SurfaceKind.Plaster => GeneratePlaster(resolution, rng, perm),
                SurfaceKind.WornFloor => GenerateWornFloor(resolution, rng, perm),
                SurfaceKind.CeilingStain => GenerateCeilingStain(resolution, rng, perm),
                SurfaceKind.FabricWeave => GenerateFabricWeave(resolution, rng, perm),
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };

            if (!Mathf.Approximately(contrast, 1f))
                ApplyContrast(pixels, contrast);

            var tex = new Texture2D(resolution, resolution, TextureFormat.RGB24, false);
            tex.SetPixels(pixels);
            tex.Apply();
            byte[] png = tex.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(tex);

            File.WriteAllBytes(assetPath, png);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = true;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        }

        // Periodic value noise: a lattice point's hash depends only on its coordinates
        // modulo "period", so ValueNoise(x + period, y) == ValueNoise(x, y) exactly, and
        // FractalNoise sums octaves whose cell counts are integers, so every octave (and
        // the sum) tiles across the [0,1) uv domain.
        private static int[] BuildPermutation(Random rng)
        {
            int[] p = new int[256];
            for (int i = 0; i < 256; i++) p[i] = i;
            for (int i = 255; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (p[i], p[j]) = (p[j], p[i]);
            }
            int[] table = new int[PermMask + 1];
            for (int i = 0; i < table.Length; i++) table[i] = p[i & 255];
            return table;
        }

        private static float Fade(float t) => t * t * t * (t * (t * 6f - 15f) + 10f);

        private static float HashLattice(int[] perm, int x, int y, int period)
        {
            int xi = ((x % period) + period) % period;
            int yi = ((y % period) + period) % period;
            int idx = perm[(xi + perm[yi & PermMask]) & PermMask];
            return idx / 255f;
        }

        private static float ValueNoise(int[] perm, float fx, float fy, int period)
        {
            int x0 = Mathf.FloorToInt(fx);
            int y0 = Mathf.FloorToInt(fy);
            float u = Fade(fx - x0);
            float v = Fade(fy - y0);
            float n00 = HashLattice(perm, x0, y0, period);
            float n10 = HashLattice(perm, x0 + 1, y0, period);
            float n01 = HashLattice(perm, x0, y0 + 1, period);
            float n11 = HashLattice(perm, x0 + 1, y0 + 1, period);
            return Mathf.Lerp(Mathf.Lerp(n00, n10, u), Mathf.Lerp(n01, n11, u), v);
        }

        /// u,v are normalized [0,1) texture coordinates. baseCells is the noise cell
        /// count at octave 0; each further octave doubles it (still an integer, so
        /// still exactly periodic). Result is roughly in [0,1].
        private static float FractalNoise(int[] perm, float u, float v, int baseCells, int octaves, float persistence)
        {
            float sum = 0f, amp = 1f, max = 0f;
            int cells = Mathf.Max(1, baseCells);
            for (int o = 0; o < octaves; o++)
            {
                sum += ValueNoise(perm, u * cells, v * cells, cells) * amp;
                max += amp;
                amp *= persistence;
                cells *= 2;
            }
            return sum / max;
        }

        // Shortest wrap-around delta/distance on a resolution x resolution torus.
        private static float WrappedDelta(float d, int period)
        {
            float ad = Mathf.Abs(d);
            return Mathf.Min(ad, period - ad);
        }

        private static float ToroidalDistance(float x0, float y0, float x1, float y1, int resolution)
        {
            float dx = WrappedDelta(x0 - x1, resolution);
            float dy = WrappedDelta(y0 - y1, resolution);
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        private static Color Gray(float value)
        {
            value = Mathf.Clamp01(value);
            return new Color(value, value, value, 1f);
        }

        // Dirty painted plaster: soft mottling + a few darker patched rectangles + fine
        // grain. Luminance ~0.80-1.0, patches to ~0.65.
        private static Color[] GeneratePlaster(int res, Random rng, int[] perm)
        {
            var pixels = new Color[res * res];
            int patchCount = 3 + rng.Next(4);
            var patches = new (float cx, float cy, float w, float h, float dark)[patchCount];
            for (int i = 0; i < patchCount; i++)
            {
                patches[i] = (
                    (float)rng.NextDouble() * res,
                    (float)rng.NextDouble() * res,
                    res * (0.12f + (float)rng.NextDouble() * 0.15f),
                    res * (0.08f + (float)rng.NextDouble() * 0.12f),
                    0.10f + (float)rng.NextDouble() * 0.12f);
            }

            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float u = x / (float)res;
                    float v = y / (float)res;
                    float mottle = FractalNoise(perm, u, v, 4, 3, 0.5f);
                    float value = 1f - mottle * 0.20f;
                    float grain = FractalNoise(perm, u, v, Mathf.Max(8, res / 4), 2, 0.5f);
                    value -= (grain - 0.5f) * 0.05f;

                    foreach (var p in patches)
                    {
                        float dx = WrappedDelta(x - p.cx, res) / p.w;
                        float dy = WrappedDelta(y - p.cy, res) / p.h;
                        float d = Mathf.Max(dx, dy);
                        value -= p.dark * (1f - Mathf.SmoothStep(0f, 1f, d));
                    }

                    pixels[y * res + x] = Gray(value);
                }
            }
            return pixels;
        }

        // Worn (but intact) hard floor: one broad directional wear band, scattered
        // scuffs/speckles, faint plank/tile seams. Luminance ~0.70-1.0.
        private static Color[] GenerateWornFloor(int res, Random rng, int[] perm)
        {
            var pixels = new Color[res * res];
            bool wearAlongV = rng.Next(2) == 0;
            float wearCenter = (float)rng.NextDouble();
            int plankCount = 6 + rng.Next(5);
            int scuffCount = Mathf.Max(4, res / 6);
            var scuffs = new (float x, float y, float r, float dark)[scuffCount];
            for (int i = 0; i < scuffCount; i++)
            {
                scuffs[i] = (
                    (float)rng.NextDouble() * res,
                    (float)rng.NextDouble() * res,
                    1f + (float)rng.NextDouble() * (res * 0.015f),
                    0.05f + (float)rng.NextDouble() * 0.10f);
            }

            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float u = x / (float)res;
                    float v = y / (float)res;

                    // Single toroidal cosine lobe -> one seamless bright "traffic path".
                    float coord = wearAlongV ? v : u;
                    float wear = 0.5f * (1f + Mathf.Cos(2f * Mathf.PI * (coord - wearCenter)));
                    float value = 0.82f + wear * 0.14f;
                    float variation = FractalNoise(perm, u, v, 5, 3, 0.5f);
                    value += (variation - 0.5f) * 0.10f;

                    // Plank/tile seams: integer cell count over normalized uv keeps
                    // the grid phase-consistent across the wrap boundary.
                    float fu = (u * plankCount) % 1f;
                    float fv = (v * plankCount) % 1f;
                    float seamDist = Mathf.Min(Mathf.Min(fu, 1f - fu), Mathf.Min(fv, 1f - fv));
                    const float seamWidth = 0.04f;
                    if (seamDist < seamWidth)
                        value -= 0.06f * (1f - seamDist / seamWidth);

                    pixels[y * res + x] = Gray(value);
                }
            }

            // Scuffs/speckles as a toroidal-distance pass on top of the base floor.
            foreach (var s in scuffs)
            {
                int pad = Mathf.CeilToInt(s.r) + 1;
                int minX = Mathf.FloorToInt(s.x) - pad, maxX = Mathf.CeilToInt(s.x) + pad;
                int minY = Mathf.FloorToInt(s.y) - pad, maxY = Mathf.CeilToInt(s.y) + pad;
                for (int yy = minY; yy <= maxY; yy++)
                {
                    for (int xx = minX; xx <= maxX; xx++)
                    {
                        float d = ToroidalDistance(xx, yy, s.x, s.y, res) / s.r;
                        if (d >= 1f) continue;
                        int wx = ((xx % res) + res) % res;
                        int wy = ((yy % res) + res) % res;
                        int idx = wy * res + wx;
                        float falloff = 1f - Mathf.SmoothStep(0f, 1f, d);
                        pixels[idx] = Gray(pixels[idx].r - s.dark * falloff);
                    }
                }
            }

            return pixels;
        }

        // Water-stained ceiling: a few large soft irregular blotches with a darker
        // concentric ring, otherwise clean. Luminance ~0.85-1.0, centres ~0.6.
        private static Color[] GenerateCeilingStain(int res, Random rng, int[] perm)
        {
            var pixels = new Color[res * res];
            int blotchCount = 2 + rng.Next(3);
            var blotches = new (float cx, float cy, float radius)[blotchCount];
            for (int i = 0; i < blotchCount; i++)
            {
                blotches[i] = (
                    (float)rng.NextDouble() * res,
                    (float)rng.NextDouble() * res,
                    res * (0.15f + (float)rng.NextDouble() * 0.18f));
            }

            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float u = x / (float)res;
                    float v = y / (float)res;
                    float grain = FractalNoise(perm, u, v, Mathf.Max(4, res / 16), 2, 0.5f);
                    float value = 0.95f + (grain - 0.5f) * 0.08f;

                    // Shared low-frequency warp so blotches read as irregular, not circular.
                    float warp = (FractalNoise(perm, u, v, 3, 2, 0.5f) - 0.5f) * res * 0.06f;

                    foreach (var b in blotches)
                    {
                        float dist = ToroidalDistance(x, y, b.cx, b.cy, res) + warp;
                        float r = dist / b.radius;
                        // Flat-ish body (darkest at centre) plus a distinct ring near
                        // the outer edge -> a visible local-minimum "water ring".
                        float body = 1f - Mathf.SmoothStep(0f, 1f, r);
                        float bodyDark = body * 0.30f;
                        float ring = Mathf.Exp(-Mathf.Pow((r - 0.9f) * 8f, 2f)) * 0.15f;
                        value -= bodyDark + ring;
                    }

                    pixels[y * res + x] = Gray(value);
                }
            }
            return pixels;
        }

        // Coarse woven upholstery: high-frequency warp/weft cross-hatch with slight
        // jitter so it doesn't read as digital. Luminance ~0.75-1.0.
        private static Color[] GenerateFabricWeave(int res, Random rng, int[] perm)
        {
            var pixels = new Color[res * res];
            // Even thread count keeps the over/under checkerboard parity consistent
            // across the wrap boundary (an odd count would flip it -> visible seam).
            int threadCount = Mathf.Max(8, res / 12);
            if (threadCount % 2 != 0) threadCount++;

            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float u = x / (float)res;
                    float v = y / (float)res;
                    float jitterU = (FractalNoise(perm, u, v, 6, 2, 0.5f) - 0.5f) * 0.15f;
                    float jitterV = (FractalNoise(perm, v, u, 6, 2, 0.5f) - 0.5f) * 0.15f;
                    float warpPhase = (u + jitterU) * threadCount;
                    float weftPhase = (v + jitterV) * threadCount;
                    float warpProfile = 0.5f + 0.5f * Mathf.Cos(2f * Mathf.PI * warpPhase);
                    float weftProfile = 0.5f + 0.5f * Mathf.Cos(2f * Mathf.PI * weftPhase);
                    bool warpOnTop = ((Mathf.FloorToInt(warpPhase) + Mathf.FloorToInt(weftPhase)) & 1) == 0;
                    float thread = warpOnTop ? warpProfile : weftProfile;
                    float value = 0.80f + thread * 0.20f;
                    float fibre = FractalNoise(perm, u, v, Mathf.Max(4, res / 8), 2, 0.5f);
                    value += (fibre - 0.5f) * 0.06f;

                    pixels[y * res + x] = Gray(value);
                }
            }
            return pixels;
        }
    }
}
