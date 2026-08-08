using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace SBR.EditorTools
{
    /// <summary>
    /// Generates the two TMP font assets S11 named — "one LoadFont + two TMP assets" — from the
    /// Archivo and Archivo Narrow TTFs that already ship under Resources/SureThing/Fonts with their
    /// OFL licences.
    ///
    /// **This exists instead of the Font Asset Creator window on purpose.** A hand-clicked font asset
    /// carries its sampling size, padding, render mode and atlas dimensions in somebody's click
    /// history; nobody can re-derive it, and re-generating it later is a guess. That is the
    /// unreproducible artifact C34 rules against, one layer below the frames it rules on. Every
    /// parameter below is a named constant, so the assets can be deleted and rebuilt identically by
    /// anyone, and a change to any of them is a diff.
    ///
    /// Run it from **SBR ▸ SureThing ▸ Generate TMP font assets**, or headlessly via
    /// <see cref="Generate"/> with -executeMethod.
    /// </summary>
    public static class SureThingTmpFontAssets
    {
        // ---- atlas parameters, the whole reason this is a script ---------------------------------

        /// <summary>Sampling size the SDF is rendered at. The largest type on this surface is the
        /// 31px wax payout figure; 90 gives it roughly 3x supersampling, which is TMP's own
        /// recommendation for UI faces and leaves headroom if the fact floor ever rises.</summary>
        public const int SamplingPointSize = 90;

        /// <summary>SDF spread, in atlas pixels. 9 is 10% of the sampling size — TMP's default ratio.
        /// It bounds how far an outline or a soft edge could ever push, and this surface uses
        /// neither: type here is flat fills of two inks (owning doc §3.1).</summary>
        public const int AtlasPadding = 9;

        public const int AtlasWidth = 1024;
        public const int AtlasHeight = 1024;

        /// <summary>SDFAA, not bitmap: the laptop canvas is world-space, read at an angle, inside the
        /// room's URP grade. A bitmap atlas would alias the moment the camera moves off-axis, and
        /// "read at an angle" is S2's first sentence about this surface.</summary>
        public const GlyphRenderMode RenderMode = GlyphRenderMode.SDFAA;

        /// <summary>**Dynamic, and this is the one parameter I would revisit with the assets in
        /// hand.** Static bakes a fixed character set and is the more reproducible choice, which is
        /// the direction C34 pulls. Dynamic is chosen anyway because a glyph this surface prints and
        /// the atlas does not carry renders as NOTHING — silently, with every test green, which is
        /// the exact S2/C18 failure this project keeps paying for. The surface prints generated team
        /// names, U+2212 MINUS (S30, mandated), and the middot separator, so the character set is not
        /// something I can enumerate honestly from here.
        ///
        /// The trade is real and it is recorded rather than hidden: dynamic atlas *packing* varies
        /// with the order glyphs are first requested. **Packing is not appearance** — each glyph's
        /// SDF is rendered identically either way — so the composited frame is unaffected, and C34's
        /// subject is the frame. If a later measurement ever shows otherwise, this is the constant to
        /// change first.</summary>
        public const AtlasPopulationMode PopulationMode = AtlasPopulationMode.Dynamic;

        private const string FontDir = "Assets/SBR/Resources/SureThing/Fonts";

        /// <summary>The two faces, and the names LaptopScreen.LoadFont resolves. Roman for running
        /// text and labels, condensed for figures, prices, names, masthead and action labels — the
        /// two-voice split S14 specified and S11 chose the superfamily to serve.</summary>
        public static readonly string[] Faces = { "Archivo", "ArchivoNarrow" };

        [MenuItem("SBR/SureThing/Generate TMP font assets")]
        public static void Generate()
        {
            foreach (string face in Faces) GenerateOne(face);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SureThingTmpFontAssets] generated {Faces.Length} assets at {SamplingPointSize}pt " +
                      $"padding {AtlasPadding} {RenderMode} {AtlasWidth}x{AtlasHeight} {PopulationMode}");
        }

        private static void GenerateOne(string face)
        {
            string ttfPath = $"{FontDir}/{face}.ttf";
            var source = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
            if (source == null)
            {
                // Loud, not silent: a missing face here would otherwise surface as a null font asset
                // and a screen that renders no type at all, which is the S2 defect with extra steps.
                Debug.LogError($"[SureThingTmpFontAssets] source face not found: {ttfPath}");
                return;
            }

            TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(
                source, SamplingPointSize, AtlasPadding, RenderMode,
                AtlasWidth, AtlasHeight, PopulationMode, enableMultiAtlasSupport: true);
            if (asset == null)
            {
                Debug.LogError($"[SureThingTmpFontAssets] CreateFontAsset returned null for {face}");
                return;
            }

            asset.name = face + " SDF";

            // Regenerating replaces the asset in place so the GUID survives. A fresh GUID would break
            // every serialized reference to it, which on this surface means the scene's LaptopScreen
            // — and a scene that loses its font reference renders nothing while compiling clean.
            string outPath = $"{FontDir}/{face} SDF.asset";
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(outPath);
            if (existing != null) EditorUtility.CopySerialized(asset, existing);
            else AssetDatabase.CreateAsset(asset, outPath);

            EditorUtility.SetDirty(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(outPath));
            Debug.Log($"[SureThingTmpFontAssets] {face} -> {outPath}");
        }
    }
}
