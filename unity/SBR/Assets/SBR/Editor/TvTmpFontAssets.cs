using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using Object = UnityEngine.Object;

namespace SBR.EditorTools
{
    /// <summary>
    /// Generates the TV surface's TMP font assets from the Encode Sans variable TTF committed under
    /// <c>Resources/Tv/Fonts</c> with its OFL licence. Phase T's step T-1.
    ///
    /// <para>Built as a script for the reason <see cref="SureThingTmpFontAssets"/> gives and does not
    /// need repeating: a hand-clicked font asset carries its sampling size, padding, render mode and
    /// atlas dimensions in somebody's click history, and that is the unreproducible artifact C34
    /// rules against, one layer below the frames it rules on.</para>
    ///
    /// <para><b>THE TRAP, AND WHAT IT ACTUALLY WAS HERE.</b> Faces are resolved by STYLE NAME, never
    /// by face index. On the laptop that rule was learned from Archivo, whose faceIndex 0 reports
    /// SemiBold. <b>Encode Sans is worse.</b> Its axis defaults are <c>wght=100, wdth=75</c> and its
    /// <c>OS/2 usWeightClass</c> is <c>100</c>; nameID 1 reads <i>"Encode Sans Condensed Thin"</i>.
    /// So the default face is not merely the wrong weight — it is <b>Condensed Thin</b>, wrong on
    /// BOTH axes, and it is what Unity's legacy <c>Font</c> renders because that path takes the
    /// default instance. Measured from the font's own <c>fvar</c> and <c>name</c> tables by
    /// <c>tools/ttf_faces.py</c>, which is committed beside this so the claim is re-runnable rather
    /// than a sentence in a comment.</para>
    ///
    /// <para>Encode Sans exposes <b>45 named instances</b> (9 weights x 5 widths) and Regular 400 at
    /// normal width sits at instance <b>21</b>. The laptop generator's probe stops at 12, so reusing
    /// it unchanged would have found nothing and — by its own guard — refused rather than shipped the
    /// default. Correct behaviour, wrong ceiling. This one derives the ceiling from the family's
    /// actual instance count instead of a number that happened to be enough once.</para>
    ///
    /// <para>There are no italic instances at all: the axes are weight and width only. Any
    /// <c>FontStyle.Italic</c> on this surface is synthesised, which is filed as <b>T77</b>.</para>
    /// </summary>
    public static class TvTmpFontAssets
    {
        // ---- atlas parameters, the whole reason this is a script ---------------------------------

        /// <summary>Sampling size the SDF is rendered at, derived rather than copied from the laptop.
        ///
        /// <para>This surface is read at its own scale: the production canvas is 980x550 and the
        /// capture frame is 2176x1223, so canvas px reach the frame at <b>2.2204x</b> (the same
        /// constant <c>tools/ladder_read.py</c> derives and the ladder work has used throughout).
        /// The largest type that actually RENDERS is the attract line at 46 canvas px = <b>102 frame
        /// px</b>; the score, this surface's declared largest element, is 36 = 80 frame px.
        /// <c>_tBigAmount</c>'s 96 is excluded on purpose — it renders nothing (flagged at its own
        /// declaration) and sizing an atlas for a corpse is how atlases get to be 4096.</para>
        ///
        /// <para>128 clears the largest rendering size with headroom and is a power of two. The
        /// laptop's 90 would sit BELOW this surface's largest glyph at frame scale, which is the
        /// specific reason this constant is not inherited.</para></summary>
        public const int SamplingPointSize = 128;

        /// <summary>SDF spread in atlas pixels, held at the laptop's 10% of sampling size. This
        /// surface's type is flat fills of two inks (owning doc §3.1) — no outline, no soft edge —
        /// so the spread only has to bound what the shader could ever push.</summary>
        public const int AtlasPadding = 13;

        /// <summary>2048, not the laptop's 1024. At 128pt a padded glyph box is ~154px, so a 1024
        /// page holds ~36 glyphs and a Latin set would spill across several Dynamic pages. Stated
        /// because it is a consequence of the sampling size above, not an independent taste.</summary>
        public const int AtlasWidth = 2048;
        public const int AtlasHeight = 2048;

        /// <summary>SDFAA for the same reason the laptop uses it, more so: this surface IS the seated
        /// view, read across a room at 2.18m through the URP grade, and it is the surface whose
        /// contrast ladder is measured on frames.</summary>
        public const GlyphRenderMode RenderMode = GlyphRenderMode.SDFAA;

        /// <summary>Probe ceiling for named instances. Encode Sans ships 45 (9 weights x 5 widths);
        /// 64 clears that with room for the family to grow a width or a weight without this
        /// silently stopping short — which is precisely the failure the laptop's 12 would have been
        /// here. Costs probe time on a miss only, and a miss refuses rather than falls back.</summary>
        private const int MaxNamedInstanceProbe = 64;

        private const string FontDir = "Assets/SBR/Resources/Tv/Fonts";
        /// <summary>T82's wiring, and it is one constant by design.
        ///
        /// <para><c>EncodeSans-Tabular.ttf</c> is derived from the committed <c>EncodeSans.ttf</c> by
        /// <c>tools/tnum_font.py</c>, which rewrites ONLY the cmap so the ten digit codepoints
        /// address the glyphs the `tnum` feature substitutes. Nothing else in the file moves — glyph
        /// ids, metrics and variation data are byte-identical — so the 45 named instances this
        /// generator selects by style name are the same instances they always were.</para>
        ///
        /// <para>The substitution is resolved before TMP sees the font because TMP cannot resolve it
        /// after: <c>OTL_FeatureTag</c> declares only kern, liga, mark and mkmk. Patching the asset
        /// instead was tried and cannot work — a Dynamic font asset serializes no character or glyph
        /// table, so a build-time remap is discarded on save.</para></summary>
        private const string SourceFace = "EncodeSans-Tabular";

        /// <summary>TMP maps a weight to its table by <c>fontWeight / 100</c>, so Bold (700) is 7.
        /// The laptop wires SemiBold at 6 for its one wordmark; this surface's kit components are the
        /// 700s (<c>SureThingTmpFontAssets:128</c> says so explicitly and defers them to Phase T).</summary>
        private const int BoldWeightIndex = 7;

        [MenuItem("SBR/TV/Generate TMP font assets")]
        public static void Generate()
        {
            // Four faces, two voices x two weights. Canon splits this surface by WIDTH — condensed
            // carries the dense numeric and long-string slots, regular the rest — and the inventory
            // found 7 regular-face and 4 condensed-face call sites already asking for Bold.
            //
            // All four are generated even though which ones the call sites end up using is a DD
            // matter (T73, T75). Generating a face does not commit the surface to it; it makes the
            // ruled options concrete enough to be shot on frames, which is what the DD rules from.
            TMP_FontAsset roman = GenerateOne("Regular", "EncodeSans SDF");
            TMP_FontAsset romanBold = GenerateOne("Bold", "EncodeSans Bold SDF");
            TMP_FontAsset cond = GenerateOne("Condensed Regular", "EncodeSansCondensed SDF");
            TMP_FontAsset condBold = GenerateOne("Condensed Bold", "EncodeSansCondensed Bold SDF");

            // Both condensed faces come from the VARIABLE EncodeSans.ttf, not from the static
            // EncodeSansCondensed.ttf sitting beside it. The static file is Regular 400 only —
            // upstream ships no variable build of it — so it cannot produce a condensed Bold at all.
            // The variable family carries the whole condensed column, Bold included. Filed as T73:
            // it changes that gap from "we need a new font file" to "the weight is already committed".
            // The static file is left in place because legacy `Font` still loads it until T-3 lands.

            WireBold(roman, romanBold);
            WireBold(cond, condBold);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[TvTmpFontAssets] done at {SamplingPointSize}pt padding {AtlasPadding} " +
                      $"{RenderMode} {AtlasWidth}x{AtlasHeight} · shader arm " +
                      $"[{SureThingTmpFontAssets.ShaderArmTag}] '{SureThingTmpFontAssets.ShaderName}'");

            // Debug.LogError does not move batchmode's exit code, so without this a generation that
            // produced nothing exits 0 and the window reads as spent well. That is C29's shape one
            // layer out from the test runner — the same defect tmp-phase-l-bootstrap.ps1 was written
            // against, and it would land here on the step that feeds every frame downstream.
            int built = (roman != null ? 1 : 0) + (romanBold != null ? 1 : 0)
                        + (cond != null ? 1 : 0) + (condBold != null ? 1 : 0);
            if (built < 4)
            {
                Debug.LogError($"[TvTmpFontAssets] only {built} of 4 faces were built. Failing the step.");
                if (Application.isBatchMode) EditorApplication.Exit(2);
            }
        }

        /// <summary>Wires the bold face into the base face's weight table so
        /// <c>fontWeight = FontWeight.Bold</c> resolves to a real 700 rather than falling back to the
        /// base face and rendering Regular — a weight tier that silently does nothing, which is the
        /// exact shape of defect both surfaces keep paying for.
        ///
        /// <para>Mutated through the getter's array because <c>fontWeightTable</c>'s SETTER is
        /// <c>internal</c>; the getter hands back the serialized backing array and
        /// <c>TMP_FontWeightPair</c> is a struct, so indexing it writes the stored element. That is
        /// the laptop's finding, reused rather than rediscovered.</para></summary>
        private static void WireBold(TMP_FontAsset baseFace, TMP_FontAsset boldFace)
        {
            if (baseFace == null || boldFace == null) return;
            baseFace.fontWeightTable[BoldWeightIndex].regularTypeface = boldFace;
            EditorUtility.SetDirty(baseFace);
            Debug.Log($"[TvTmpFontAssets] wired '{boldFace.name}' into '{baseFace.name}' " +
                      $"weight table at index {BoldWeightIndex} (700)");
        }

        /// <summary>Finds the face index whose style name matches, so nothing here depends on a magic
        /// number. FreeType exposes a variable font's named instances as <c>(n &lt;&lt; 16)</c>; index
        /// 0 is the default instance, which for Encode Sans is Condensed Thin.
        ///
        /// <para>On a miss this logs every style name it DID find. The laptop's version returned -1
        /// and left the caller to guess whether the name was wrong, the ceiling was too low, or the
        /// font had changed — three very different repairs behind one silence.</para></summary>
        private static int ResolveFaceIndex(string path, string styleName)
        {
            var seen = new System.Collections.Generic.List<string>();
            for (int i = 0; i <= MaxNamedInstanceProbe; i++)
            {
                int faceIndex = i == 0 ? 0 : i << 16;
                TMP_FontAsset probe = null;
                try
                {
                    probe = TMP_FontAsset.CreateFontAsset(path, faceIndex, SamplingPointSize,
                        AtlasPadding, GlyphRenderMode.SDFAA, 256, 256);
                }
                catch { continue; }
                if (probe == null) continue;
                string style = probe.faceInfo.styleName;
                Object.DestroyImmediate(probe);
                seen.Add($"[{i}] {style}");
                if (style == styleName) return faceIndex;
            }
            Debug.LogError($"[TvTmpFontAssets] no '{styleName}' instance in {path}. " +
                           $"Probed {MaxNamedInstanceProbe + 1} indices and found: {string.Join(", ", seen)}");
            return -1;
        }

        private static TMP_FontAsset GenerateOne(string styleName, string assetName)
        {
            string ttfPath = $"{FontDir}/{SourceFace}.ttf";
            if (AssetDatabase.LoadAssetAtPath<Font>(ttfPath) == null)
            {
                Debug.LogError($"[TvTmpFontAssets] source face not found: {ttfPath}");
                return null;
            }

            int faceIndex = ResolveFaceIndex(ttfPath, styleName);
            if (faceIndex < 0)
            {
                // Loud, and it must be: falling back to the default face is how this surface came to
                // render its roman voice in Condensed Thin without anyone choosing it.
                Debug.LogError($"[TvTmpFontAssets] refusing to fall back to the default face for " +
                               $"'{styleName}' — the default here is Condensed Thin 100/75.");
                return null;
            }

            TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(ttfPath, faceIndex, SamplingPointSize,
                AtlasPadding, RenderMode, AtlasWidth, AtlasHeight);
            if (asset == null)
            {
                Debug.LogError($"[TvTmpFontAssets] CreateFontAsset returned null for '{styleName}'");
                return null;
            }
            asset.name = assetName;

            string outPath = $"{FontDir}/{assetName}.asset";
            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(outPath) != null)
                AssetDatabase.DeleteAsset(outPath);
            AssetDatabase.CreateAsset(asset, outPath);

            // The atlas texture and the material are SUB-ASSETS and must be added explicitly.
            // CreateFontAsset builds them in memory; only the Font Asset Creator window persists
            // them, so a scripted asset saved without this step reloads with m_AtlasTextures
            // unassigned — a font asset that exists, passes every null check, and renders nothing.
            if (asset.atlasTextures == null || asset.atlasTextures.Length == 0 || asset.atlasTextures[0] == null)
            {
                Debug.LogError($"[TvTmpFontAssets] {assetName} produced no atlas texture — " +
                               "the asset would render nothing. Not shipping it silently.");
                return null;
            }

            Texture2D atlas = asset.atlasTextures[0];
            atlas.name = assetName + " Atlas";
            AssetDatabase.AddObjectToAsset(atlas, asset);

            // The shader arm and _Sharpness are deliberately NOT re-declared here. They are the
            // laptop generator's C13 A/B, and that comparison is project-wide: two surfaces on
            // different arms would make any cross-surface read of the same defect unreadable, which
            // is exactly how T49's bloom comparison had to be re-run. One authority, referenced.
            Shader shader = Shader.Find(SureThingTmpFontAssets.ShaderName);
            if (shader == null)
            {
                Debug.LogError($"[TvTmpFontAssets] shader '{SureThingTmpFontAssets.ShaderName}' not " +
                               "found — essential resources are missing or incomplete.");
                return null;
            }
            var material = new Material(shader)
            {
                name = $"{assetName} Atlas Material " +
                       $"[{SureThingTmpFontAssets.ShaderArmTag} s{SureThingTmpFontAssets.SharpnessTag}]"
            };
            material.SetTexture(ShaderUtilities.ID_MainTex, atlas);

            // The CONFIGURED atlas size, never the texture's current size. A Dynamic atlas serialises
            // at 1x1 and only grows at runtime, so reading atlas.width here shipped _TextureWidth = 1
            // on the laptop and every glyph rendered soft. That is C13's first-capture finding.
            material.SetFloat(ShaderUtilities.ID_TextureWidth, AtlasWidth);
            material.SetFloat(ShaderUtilities.ID_TextureHeight, AtlasHeight);
            material.SetFloat(ShaderUtilities.ID_GradientScale, AtlasPadding + 1);
            material.SetFloat(ShaderUtilities.ID_WeightNormal, asset.normalStyle);
            material.SetFloat(ShaderUtilities.ID_WeightBold, asset.boldStyle);
            material.SetFloat(SureThingTmpFontAssets.SharpnessId, SureThingTmpFontAssets.Sharpness);
            asset.material = material;
            AssetDatabase.AddObjectToAsset(material, asset);

            EditorUtility.SetDirty(asset);
            Debug.Log($"[TvTmpFontAssets] {SourceFace} '{styleName}' (faceIndex {faceIndex}) -> {outPath} " +
                      $"· style '{asset.faceInfo.styleName}' · pointSize {asset.faceInfo.pointSize} " +
                      $"· material mirrors {material.GetFloat(ShaderUtilities.ID_TextureWidth)}x" +
                      $"{material.GetFloat(ShaderUtilities.ID_TextureHeight)} " +
                      $"(texture is currently {atlas.width}x{atlas.height} — Dynamic, grows at runtime)");
            return asset;
        }

        /// <summary>T82: the unit is the ASSET, and the inventory NAMES its members. Which slots ride
        /// which asset is otherwise recoverable only by reading 22 call sites, and an inventory that
        /// does not say what it covers is a claim rather than a list (C18).
        ///
        /// <para>It is also the argument that settled RiskPays: tabular lives on the asset, and
        /// RiskPays shares condensed Bold 700 with CashOut, which ticks — so it takes tabular because
        /// its asset-mate needs it, not on its own merits.</para></summary>
        private static string Members(string asset) => asset switch
        {
            "EncodeSans SDF" =>
                "TicketHeader, LegRowState, Leg, Clock, CashOutStatus, TakeoverSub, Subtitle, Chrome, " +
                "Consolation, and the seven on synthesised bold: Attract, TakeoverTitle, " +
                "BigAmount (renders nothing, T79), Matchup, Score, Flavor, InterventionPrompt",
            "EncodeSans Bold SDF" =>
                "NO SLOT TODAY — built and wired into the regular face's weight table at 700, but T73 " +
                "names only condensed sites. An unused member is a fact about the inventory, not an " +
                "omission from it",
            "EncodeSansCondensed SDF" => "LegRowPrice, LegRowProgress",
            "EncodeSansCondensed Bold SDF" => "LegRowLine, LegRowNeed, RiskPays, CashOut",
            _ => "(unlisted)",
        };

        /// <summary>Asserts the pin rather than describing it (C34.1: "an unasserted pin is a
        /// comment"). Reads back what each generated asset actually carries, so a wrong instance is
        /// caught here and not four batches later on a frame nobody re-measured.</summary>
        [MenuItem("SBR/TV/Verify TMP font assets")]
        public static void Verify()
        {
            (string asset, string expectStyle)[] want =
            {
                ("EncodeSans SDF", "Regular"),
                ("EncodeSans Bold SDF", "Bold"),
                ("EncodeSansCondensed SDF", "Condensed Regular"),
                ("EncodeSansCondensed Bold SDF", "Condensed Bold"),
            };

            bool ok = true;
            foreach ((string name, string expect) in want)
            {
                var a = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>($"{FontDir}/{name}.asset");
                if (a == null) { Debug.LogError($"[TvTmpFontAssets] MISSING: {name}"); ok = false; continue; }

                string style = a.faceInfo.styleName;
                bool styleOk = style == expect;
                bool atlasOk = a.atlasTextures != null && a.atlasTextures.Length > 0 && a.atlasTextures[0] != null;
                bool matOk = a.material != null
                             && Mathf.Approximately(a.material.GetFloat(ShaderUtilities.ID_TextureWidth), AtlasWidth)
                             && Mathf.Approximately(a.material.GetFloat(ShaderUtilities.ID_TextureHeight), AtlasHeight);

                // T82, asserted where the property actually lives. Counting tabular digits in the
                // asset's characterLookupTable would always read 0 of 0 — a Dynamic font asset
                // serializes no character table, which is the finding that sent T82 to a derived font
                // in the first place. The tabular property is in the SOURCE FONT's cmap, so what an
                // asset must prove is which font it was built from.
                //
                // Structural on purpose: no canvas, no rasterisation, nothing to crash. The rendered
                // confirmation the ruling asks for is the digit probe, run separately.
                // The path-based CreateFontAsset overload this generator uses records its source in
                // `m_SourceFontFilePath` and leaves BOTH `sourceFontFile` and `m_SourceFontFileGUID`
                // empty. Two assertions were written against those before the asset file was read;
                // both reported NONE against correctly generated assets, which is a verify that fails
                // an artefact for a property of the verifier. Read the field the asset actually
                // writes — through SerializedObject, public Editor API.
                string wantPath = $"{FontDir}/{SourceFace}.ttf";
                string gotPath = new SerializedObject(a).FindProperty("m_SourceFontFilePath")?.stringValue;
                bool tabOk = gotPath == wantPath;
                string src = string.IsNullOrEmpty(gotPath) ? "NONE" : gotPath;

                if (!styleOk || !atlasOk || !matOk || !tabOk) ok = false;
                Debug.Log($"[TvTmpFontAssets] {name}: style '{style}' expected '{expect}' " +
                          $"{(styleOk ? "OK" : "WRONG INSTANCE")} · atlas {(atlasOk ? "OK" : "MISSING")} " +
                          $"· material mirror {(matOk ? "OK" : "WRONG")} · source '{src}' " +
                          $"{(tabOk ? "OK (tabular)" : $"NOT THE TABULAR FONT — expected '{SourceFace}'")}");
                Debug.Log($"[TvTmpFontAssets]   members: {Members(name)}");
            }
            Debug.Log(ok
                ? "[TvTmpFontAssets] VERIFY PASS — every face is the named instance it claims."
                : "[TvTmpFontAssets] VERIFY FAIL — see errors above. Do not shoot a set from this build.");

            // Exits 3 on failure, matching tmp-phase-l-bootstrap.ps1's contract. A verify that only
            // logs is not a gate: an import that silently did nothing and a successful one look
            // identical in a batchmode log tail.
            if (!ok && Application.isBatchMode) EditorApplication.Exit(3);
        }
    }
}
