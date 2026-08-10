using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace SBR.EditorTools
{
    /// <summary>
    /// Generates the TMP font assets S11 named — "one LoadFont + two TMP assets" — from the Archivo
    /// and Archivo Narrow variable TTFs that ship under Resources/SureThing/Fonts with their OFL
    /// licences, plus the one weight-600 instance S20 reserves.
    ///
    /// **This exists instead of the Font Asset Creator window on purpose.** A hand-clicked font asset
    /// carries its sampling size, padding, render mode and atlas dimensions in somebody's click
    /// history; nobody can re-derive it. That is the unreproducible artifact C34 rules against, one
    /// layer below the frames it rules on. Every parameter below is a named constant, so the assets
    /// can be deleted and rebuilt identically and a change to any of them is a diff.
    ///
    /// **Faces are resolved by STYLE NAME, never by face index.** These are variable fonts, and the
    /// default face is not what anyone would assume: Archivo.ttf's faceIndex 0 reports SemiBold, and
    /// its Regular sits at a named-instance index. The first cut of this generator took the default
    /// and so shipped the surface's roman voice at weight 600 for the whole migration, unchosen —
    /// which S11 had done before it, in UGUI, for the same reason. A magic index would also rot the
    /// first time the font is updated and the instances reorder. Ruled Regular 400 by Allen,
    /// 2026-08-08.
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

        // ---- C13 shader A/B, in the drawer -------------------------------------------------------
        //
        // **Flip this one constant and re-run the bootstrap; nothing else moves.** It exists because
        // the material-mirror fix may turn out to be a real defect that was not the CAUSE of the
        // desk-pose softness, and this is the next suspect rather than a fresh start.
        //
        // TMP's own font-asset creation path builds its material from `ShaderRef_MobileSDF`
        // (`TMP_FontAsset.cs:638`). This generator has always used the full `TextMeshPro/Distance
        // Field` instead — my choice, made for quality on a world-space canvas read at an angle, and
        // very possibly the better one. But it IS a divergence from TMP's convention that I
        // introduced, it lives in the same three materials the softness lives in, and the two
        // shaders do not antialias identically: the full shader declares 12 ScaleRatio references
        // against the mobile shader's 10.
        //
        // **Arm A** (`TextMeshPro/Distance Field`) is what ships today.
        // **Arm B** (`TextMeshPro/Mobile/Distance Field`) is TMP's own default.
        //
        // The arm is stamped into each material's NAME so a frame can always be traced back to the
        // build that produced it — an A/B whose arms are indistinguishable in the artifact is how
        // T49's bloom comparison ended up unreadable and had to be re-run.
        private const bool UseMobileSdfShader = false;

        private const string ShaderArmA = "TextMeshPro/Distance Field";
        private const string ShaderArmB = "TextMeshPro/Mobile/Distance Field";

        /// <summary>The shader this build uses, and the short tag that records it in every material
        /// name and in the bootstrap's log line.</summary>
        internal static string ShaderName => UseMobileSdfShader ? ShaderArmB : ShaderArmA;
        internal static string ShaderArmTag => UseMobileSdfShader ? "MobileSDF" : "DF";

        // ---- C13: the edge ramp -------------------------------------------------------------------
        //
        // `TMP_SDF.shader:186` — `scale *= abs(input.texcoord0.w) * _GradientScale * (_Sharpness + 1)`.
        // `texcoord0.w` carries the per-glyph scale, so `scale` grows with the glyph and **the SDF's
        // edge ramp is constant in SCREEN space at every size**. That is not a malfunction; it is the
        // mechanism that produces the exact signature room measured — a fixed ~1.6px ramp that does
        // not scale with the glyph.
        //
        // **`_Sharpness` is the only term in that expression this generator never set.** It fell to
        // the shader's default of 0, which is the WIDEST ramp the shader permits; the range is -1..1
        // and at +1 the multiplier doubles, halving the ramp uniformly at every glyph size. Uniform
        // is the point: it is the only shape that can move a fixed screen-space ramp.
        //
        // Measured at 1:1 on the flat captures at 0: 1.14-2.04px, mean ~1.5px. **The ramp is present
        // without any magnification**, which is why this is measurable here rather than only at the
        // desk pose.
        //
        // TMP ships 0 by default, so this is not a defect introduced here — it is a default that may
        // simply be wrong for a surface read at an angle through a graded room. **The value is a
        // design call (Allen's eye, then the DD); this constant only makes the arms producible.**
        public const float Sharpness = 0f;

        /// <summary>Stamped into every material name and the Verify line. Invariant-formatted so the
        /// tag cannot come out as "s0,5" on a comma-decimal machine and quietly split the set.</summary>
        internal static string SharpnessTag =>
            Sharpness.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>`_Sharpness` has no cached id in ShaderUtilities, so it is resolved by name once
        /// rather than per material.</summary>
        internal static readonly int SharpnessId = Shader.PropertyToID("_Sharpness");

        private const string FontDir = "Assets/SBR/Resources/SureThing/Fonts";

        /// <summary>TMP maps a weight to its table by `fontWeight / 100`, so SemiBold (600) is 6.
        /// Read from TMP_Text rather than assumed.</summary>
        private const int SemiBoldWeightIndex = 6;

        [MenuItem("SBR/SureThing/Generate TMP font assets")]
        public static void Generate()
        {
            // The two voices, both at Regular 400. S14's two-voice split is a WIDTH split — roman for
            // running text and labels, condensed for figures, prices, names — and it was never
            // supposed to be a weight split as well.
            TMP_FontAsset roman = GenerateOne("Archivo", "Regular", "Archivo SDF");
            GenerateOne("ArchivoNarrow", "Regular", "ArchivoNarrow SDF");

            // S20's entire scope on this surface is ONE element: OsRail.jsx puts the rail's identity
            // mark at fontWeight 600 and every other laptop component in the kit is 400 (LockAction,
            // MarginHeader, Masthead all state it explicitly). The 700s in the kit are TV components
            // and belong to Phase T. So exactly one extra face is generated — the roman SemiBold —
            // rather than a symmetrical pair the surface has no ruled use for.
            TMP_FontAsset romanSemiBold = GenerateOne("Archivo", "SemiBold", "Archivo SemiBold SDF");

            if (roman != null && romanSemiBold != null)
            {
                // Wiring it into the weight table is what makes `fontWeight = FontWeight.SemiBold`
                // resolve. Without this the property sets cleanly, falls back to the base face, and
                // renders Regular — a weight tier that silently does nothing, which is the exact
                // shape of defect this surface keeps paying for.
                // Mutated through the getter's array rather than assigned back: fontWeightTable's
                // SETTER is `internal`, so `roman.fontWeightTable = table` does not compile outside
                // the TMP assembly. The getter hands back the serialized backing array itself and
                // TMP_FontWeightPair is a struct, so indexing it gives direct write access to the
                // stored element. Caught from the package source rather than in the editor window,
                // which was queued behind another seat at the time.
                roman.fontWeightTable[SemiBoldWeightIndex].regularTypeface = romanSemiBold;
                EditorUtility.SetDirty(roman);
                Debug.Log($"[SureThingTmpFontAssets] wired '{romanSemiBold.name}' into " +
                          $"'{roman.name}' weight table at index {SemiBoldWeightIndex} (600)");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SureThingTmpFontAssets] done at {SamplingPointSize}pt padding {AtlasPadding} " +
                      $"{RenderMode} {AtlasWidth}x{AtlasHeight} · shader arm [{ShaderArmTag}] '{ShaderName}'");
        }

        /// <summary>Finds the face index whose style name matches, so nothing here depends on a
        /// magic number. FreeType exposes a variable font's named instances as (n &lt;&lt; 16); index 0 is
        /// the default instance, which for Archivo is NOT Regular.</summary>
        private static int ResolveFaceIndex(string path, string styleName)
        {
            for (int i = 0; i <= 12; i++)
            {
                int faceIndex = i == 0 ? 0 : i << 16;
                TMP_FontAsset probe = null;
                try
                {
                    // The same parameters the type probe already ran successfully against these two
                    // faces. The asset is read for its style name and destroyed, so smaller values
                    // would be cheaper — but "cheaper and untested" is not worth a queued window.
                    probe = TMP_FontAsset.CreateFontAsset(path, faceIndex, SamplingPointSize,
                        AtlasPadding, GlyphRenderMode.SDFAA, 256, 256);
                }
                catch { continue; }
                if (probe == null) continue;
                string style = probe.faceInfo.styleName;
                Object.DestroyImmediate(probe);
                if (style == styleName) return faceIndex;
            }
            return -1;
        }

        private static TMP_FontAsset GenerateOne(string face, string styleName, string assetName)
        {
            string ttfPath = $"{FontDir}/{face}.ttf";
            if (AssetDatabase.LoadAssetAtPath<Font>(ttfPath) == null)
            {
                Debug.LogError($"[SureThingTmpFontAssets] source face not found: {ttfPath}");
                return null;
            }

            int faceIndex = ResolveFaceIndex(ttfPath, styleName);
            if (faceIndex < 0)
            {
                // Loud, and it must be: falling back to the default face is precisely how the roman
                // voice ended up at SemiBold without anyone choosing it.
                Debug.LogError($"[SureThingTmpFontAssets] {face} has no '{styleName}' named instance — " +
                               "refusing to fall back to the default face.");
                return null;
            }

            TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(ttfPath, faceIndex, SamplingPointSize,
                AtlasPadding, RenderMode, AtlasWidth, AtlasHeight);
            if (asset == null)
            {
                Debug.LogError($"[SureThingTmpFontAssets] CreateFontAsset returned null for {face} {styleName}");
                return null;
            }
            asset.name = assetName;

            string outPath = $"{FontDir}/{assetName}.asset";
            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(outPath) != null)
                AssetDatabase.DeleteAsset(outPath);
            AssetDatabase.CreateAsset(asset, outPath);

            // **The atlas texture and the material are SUB-ASSETS and must be added explicitly.**
            // CreateFontAsset builds them in memory; only the Font Asset Creator window persists
            // them, so a scripted asset saved without this step reloads with m_AtlasTextures
            // unassigned — a font asset that exists, passes every null check, and renders nothing.
            if (asset.atlasTextures == null || asset.atlasTextures.Length == 0 || asset.atlasTextures[0] == null)
            {
                Debug.LogError($"[SureThingTmpFontAssets] {assetName} produced no atlas texture — " +
                               "the asset would render nothing. Not shipping it silently.");
                return null;
            }

            Texture2D atlas = asset.atlasTextures[0];
            atlas.name = assetName + " Atlas";
            AssetDatabase.AddObjectToAsset(atlas, asset);

            Shader shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                Debug.LogError($"[SureThingTmpFontAssets] shader '{ShaderName}' not found — essential " +
                               "resources are missing or incomplete.");
                return null;
            }
            var material = new Material(shader)
            {
                name = $"{assetName} Atlas Material [{ShaderArmTag} s{SharpnessTag}]"
            };
            material.SetTexture(ShaderUtilities.ID_MainTex, atlas);

            // **The CONFIGURED atlas size, never the texture's current size.** This read
            // `atlas.width`/`atlas.height`, and a Dynamic atlas serialises at 1x1 and only grows to
            // its configured ceiling at runtime — so the material shipped `_TextureWidth = 1`. The
            // SDF shader derives its antialiasing gradient from these two fields, so at 1 it computed
            // the wrong screen-space sample rate and every glyph on the surface rendered soft. That
            // is C13's first-capture finding, and it is why the room read the laptop as blurry at
            // the desk pose while the grade and the atlas both measured clean.
            //
            // I logged "atlas 1x1" at generation and wrote it down as expected Dynamic behaviour,
            // which it is — the mistake was mirroring the texture's state into a field that must
            // describe its CONFIGURATION. **A named constant does not protect a value that is copied
            // from somewhere else**; the constant was right the whole time.
            material.SetFloat(ShaderUtilities.ID_TextureWidth, AtlasWidth);
            material.SetFloat(ShaderUtilities.ID_TextureHeight, AtlasHeight);
            material.SetFloat(ShaderUtilities.ID_GradientScale, AtlasPadding + 1);
            material.SetFloat(ShaderUtilities.ID_WeightNormal, asset.normalStyle);
            material.SetFloat(ShaderUtilities.ID_WeightBold, asset.boldStyle);
            // Set EXPLICITLY even at 0, so every arm — including the baseline — comes out of the same
            // code path. An arm that is "whatever the shader defaults to" is not a measured arm.
            material.SetFloat(SharpnessId, Sharpness);
            asset.material = material;
            AssetDatabase.AddObjectToAsset(material, asset);

            EditorUtility.SetDirty(asset);
            Debug.Log($"[SureThingTmpFontAssets] {face} '{styleName}' (faceIndex {faceIndex}) -> {outPath} " +
                      $"· style '{asset.faceInfo.styleName}' · material mirrors " +
                      $"{material.GetFloat(ShaderUtilities.ID_TextureWidth)}x" +
                      $"{material.GetFloat(ShaderUtilities.ID_TextureHeight)} " +
                      $"(texture is currently {atlas.width}x{atlas.height} — Dynamic, grows at runtime)");
            return asset;
        }
    }
}
