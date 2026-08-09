using System.IO;
using UnityEditor;
using UnityEngine;

namespace SBR.EditorTools
{
    /// <summary>
    /// Imports TMP's essential resources — the TMP_Settings asset, the default font asset, and the
    /// SDF shaders and materials — which this project has never had. `com.unity.ugui` carries the
    /// TextMeshPro *package*, but the project-side resources are a separate import, and without them
    /// TMP components resolve no settings and render nothing.
    ///
    /// **Scripted rather than clicked, for the same reason the font assets are** (see
    /// <see cref="SureThingTmpFontAssets"/>): the editor window is a scarce studio-wide resource and
    /// a click-through cannot be re-run, reviewed or handed to anyone else. This runs headlessly
    /// under -executeMethod.
    ///
    /// **It verifies rather than assumes.** An import that silently did nothing and a successful one
    /// are indistinguishable from a batchmode log tail, which is C29's exact shape — so this checks
    /// for the settings asset afterwards and exits non-zero when it is absent. A bootstrap that
    /// reports success on an empty import would cost the whole window and be discovered later.
    /// </summary>
    public static class SureThingTmpBootstrap
    {
        /// <summary>Unity resolves the virtual `Packages/` path to wherever the package actually
        /// lives, so this survives the PackageCache hash changing — which it does on every package
        /// update, and which is why the literal cache path must never be hardcoded.</summary>
        private const string EssentialsPackage =
            "Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage";

        /// <summary>Where TMP puts its settings once the essentials land. Presence of this asset is
        /// the gate: it is what TMP_Settings.instance resolves and what every TMP component needs.</summary>
        private const string SettingsAsset = "Assets/TextMesh Pro/Resources/TMP Settings.asset";

        private static double _deadline;

        /// <summary>Imports the essentials and **stays alive until the import actually completes.**
        ///
        /// The first cut of this ran under `-quit` and called ImportPackage, which is asynchronous:
        /// the editor exited before the import ran, the step reported ok, and nothing landed. The
        /// next step then died inside TMP itself on a null TMP_Settings. **The step that did nothing
        /// reported success — C29's exact shape, in the script written to prevent it.**
        ///
        /// So: no `-quit` for this invocation (the launcher omits it), and the process exits from the
        /// import callbacks instead. The watchdog covers the case where no callback ever fires, which
        /// would otherwise hang the editor for the whole window with another seat queued behind.</summary>
        [MenuItem("SBR/SureThing/Import TMP essential resources")]
        public static void ImportEssentials()
        {
            if (AssetDatabase.LoadAssetAtPath<Object>(SettingsAsset) != null)
            {
                Debug.Log($"[SureThingTmpBootstrap] already present: {SettingsAsset} — nothing to do");
                EditorApplication.Exit(0);
                return;
            }

            string full = Path.GetFullPath(EssentialsPackage);
            if (!File.Exists(full))
            {
                Debug.LogError($"[SureThingTmpBootstrap] essentials package not found at {full}. " +
                               "com.unity.ugui may have moved it; do not hardcode a PackageCache path.");
                EditorApplication.Exit(2);
                return;
            }

            AssetDatabase.importPackageCompleted += OnCompleted;
            AssetDatabase.importPackageFailed += OnFailed;
            AssetDatabase.importPackageCancelled += OnCancelled;

            Debug.Log($"[SureThingTmpBootstrap] importing {full}");
            AssetDatabase.ImportPackage(full, interactive: false);

            _deadline = EditorApplication.timeSinceStartup + 300.0;
            EditorApplication.update += Watchdog;
        }

        private static void OnCompleted(string packageName)
        {
            Debug.Log($"[SureThingTmpBootstrap] import completed: {packageName}");
            AssetDatabase.Refresh();
            EditorApplication.Exit(0);
        }

        private static void OnFailed(string packageName, string error)
        {
            Debug.LogError($"[SureThingTmpBootstrap] import FAILED: {packageName} — {error}");
            EditorApplication.Exit(2);
        }

        private static void OnCancelled(string packageName)
        {
            Debug.LogError($"[SureThingTmpBootstrap] import CANCELLED: {packageName}");
            EditorApplication.Exit(2);
        }

        private static void Watchdog()
        {
            if (EditorApplication.timeSinceStartup < _deadline) return;
            Debug.LogError("[SureThingTmpBootstrap] import produced no callback within 300s — " +
                           "exiting rather than holding the editor with another seat queued.");
            EditorApplication.Exit(2);
        }

        /// <summary>Gates generation on the thing generation needs. Split out from
        /// <see cref="Verify"/> because the whole failure above was a step running before its
        /// precondition was checked: TMP_FontAsset.CreateFontAsset dereferences TMP_Settings, so
        /// without the essentials it throws inside TMP rather than failing in our code.</summary>
        [MenuItem("SBR/SureThing/Verify TMP essential resources only")]
        public static void VerifyEssentials()
        {
            bool settings = AssetDatabase.LoadAssetAtPath<Object>(SettingsAsset) != null;
            Debug.Log($"[SureThingTmpBootstrap] TMP settings: {(settings ? "PRESENT" : "MISSING")}");
            if (!settings)
            {
                Debug.LogError($"[SureThingTmpBootstrap] {SettingsAsset} absent — the import did not " +
                               "land. Generation would die inside TMP on a null TMP_Settings.");
                EditorApplication.Exit(3);
            }
        }

        /// <summary>Second invocation: the essentials are either there or the window is wasted, and
        /// this is the difference between finding that out now and finding it out after the
        /// migration has been built on top of it.</summary>
        [MenuItem("SBR/SureThing/Verify TMP essential resources")]
        public static void Verify()
        {
            bool settings = AssetDatabase.LoadAssetAtPath<Object>(SettingsAsset) != null;
            var roman = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(
                "Assets/SBR/Resources/SureThing/Fonts/Archivo SDF.asset");
            var cond = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(
                "Assets/SBR/Resources/SureThing/Fonts/ArchivoNarrow SDF.asset");

            Debug.Log($"[SureThingTmpBootstrap] TMP settings: {(settings ? "PRESENT" : "MISSING")} · " +
                      $"Archivo SDF: {(roman != null ? "PRESENT" : "MISSING")} · " +
                      $"ArchivoNarrow SDF: {(cond != null ? "PRESENT" : "MISSING")}");

            if (!settings || roman == null || cond == null)
            {
                Debug.LogError("[SureThingTmpBootstrap] Phase L cannot proceed — see the line above.");
                EditorApplication.Exit(3);
                return;
            }

            // **Presence is not usability, and this check was originally missing.** The first
            // generated pair existed, was distinct, and reported PRESENT — while throwing
            // "m_AtlasTextures of TMP_FontAsset has not been assigned", because the atlas texture and
            // material are sub-assets the script had not persisted. A font asset in that state passes
            // every null check and renders nothing.
            //
            // That is a gate whose name described a stronger proposition than the one it tested —
            // C18 §4.2's newest row (T66), reproduced here within a day of it being written down.
            foreach (var pair in new[] { ("Archivo", roman), ("ArchivoNarrow", cond) })
            {
                var (label, fa) = pair;
                bool hasAtlas = fa.atlasTextures != null && fa.atlasTextures.Length > 0
                                && fa.atlasTextures[0] != null;
                bool hasMaterial = fa.material != null;
                Debug.Log($"[SureThingTmpBootstrap] {label}: atlas {(hasAtlas ? "OK" : "UNASSIGNED")} · " +
                          $"material {(hasMaterial ? "OK" : "UNASSIGNED")} · " +
                          $"pointSize {fa.faceInfo.pointSize} · scale {fa.faceInfo.scale}");
                if (!hasAtlas || !hasMaterial)
                {
                    Debug.LogError($"[SureThingTmpBootstrap] {label} would render NOTHING — refusing " +
                                   "to report this bootstrap as usable.");
                    EditorApplication.Exit(3);
                    return;
                }

                // **C13's first-capture defect, gated.** The SDF shader derives its antialiasing
                // gradient from the material's own copy of the atlas dimensions. The generator used
                // to mirror the TEXTURE's size, and a Dynamic atlas is 1x1 until runtime — so the
                // material shipped `_TextureWidth = 1` and every glyph rendered soft, while the
                // asset, the atlas and the constants all measured correct.
                //
                // This is the third derived-value defect on this surface after the unassigned atlas
                // and the SemiBold default face, and the pattern is the same each time: **presence
                // was checked, agreement was not.** So this checks agreement.
                float mirroredW = fa.material.GetFloat(TMPro.ShaderUtilities.ID_TextureWidth);
                float mirroredH = fa.material.GetFloat(TMPro.ShaderUtilities.ID_TextureHeight);
                bool mirrorOk = Mathf.Approximately(mirroredW, SureThingTmpFontAssets.AtlasWidth)
                                && Mathf.Approximately(mirroredH, SureThingTmpFontAssets.AtlasHeight);
                // The shader arm is reported on the same line as the mirror, because if the C13 hunt
                // reopens these frames get compared across arms and **a capture whose arm is not
                // recorded is not in the comparison** (T49's bloom A/B had to be re-run for exactly
                // this). The material's own name carries it too, so the artifact is self-describing
                // even without the log.
                Debug.Log($"[SureThingTmpBootstrap] {label}: material mirrors {mirroredW}x{mirroredH} " +
                          $"against configured {SureThingTmpFontAssets.AtlasWidth}x" +
                          $"{SureThingTmpFontAssets.AtlasHeight} — {(mirrorOk ? "AGREE" : "DISAGREE")} · " +
                          $"shader '{fa.material.shader.name}' arm [{SureThingTmpFontAssets.ShaderArmTag}]");
                if (fa.material.shader.name != SureThingTmpFontAssets.ShaderName)
                {
                    // Assets left over from the other arm would silently mix the comparison.
                    Debug.LogError($"[SureThingTmpBootstrap] {label} was built with " +
                                   $"'{fa.material.shader.name}' but this tree is set to " +
                                   $"'{SureThingTmpFontAssets.ShaderName}' — regenerate before shooting.");
                    EditorApplication.Exit(3);
                    return;
                }
                if (!mirrorOk)
                {
                    Debug.LogError($"[SureThingTmpBootstrap] {label}'s material describes an atlas it " +
                                   "does not have. Glyphs will render soft and nothing else will " +
                                   "look wrong — refusing to report this bootstrap as usable.");
                    EditorApplication.Exit(3);
                    return;
                }

                // **The gate that would have caught the whole thing.** Archivo.ttf's default face is
                // SemiBold, so a generator that took faceIndex 0 shipped the roman voice at weight
                // 600 for an entire migration and nothing said a word — every earlier check here
                // asked whether the asset EXISTED. Allen ruled Regular 400 on 2026-08-08; this is
                // what makes that stick rather than depending on the generator staying correct.
                if (fa.faceInfo.styleName != "Regular")
                {
                    Debug.LogError($"[SureThingTmpBootstrap] {label} is style " +
                                   $"'{fa.faceInfo.styleName}', not 'Regular'. The base voices are " +
                                   "Regular 400; a weight tier is a named instance in the weight " +
                                   "table, never the default face.");
                    EditorApplication.Exit(3);
                    return;
                }
            }

            // S20: the one weight tier the kit asks for on this surface (OsRail.jsx:17). Checked as
            // WIRED, not merely present — TMP resolves fontWeightTable[600 / 100], and an unwired
            // entry falls back to the base face and renders Regular. That is a weight tier that sets
            // cleanly, throws nothing, and does nothing.
            var semi = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(
                "Assets/SBR/Resources/SureThing/Fonts/Archivo SemiBold SDF.asset");
            TMPro.TMP_FontWeightPair[] weights = roman.fontWeightTable;
            bool wired = semi != null && weights != null && weights.Length > 6
                         && weights[6].regularTypeface == semi;
            Debug.Log($"[SureThingTmpBootstrap] weight 600: asset " +
                      $"{(semi != null ? $"PRESENT (style '{semi.faceInfo.styleName}')" : "MISSING")} · " +
                      $"table index 6 {(wired ? "WIRED" : "NOT WIRED")}");
            if (!wired || semi.faceInfo.styleName != "SemiBold")
            {
                Debug.LogError("[SureThingTmpBootstrap] S20's weight tier is not usable — it would " +
                               "render Regular and look like it worked.");
                EditorApplication.Exit(3);
            }

            // S11's ruling is "one LoadFont + two TMP assets". Two faces that resolved to the same
            // asset would satisfy every null check and silently collapse the surface's two-voice type
            // split — the defect LaptopScreen's own _fontCond comment already claimed once and was
            // wrong about. Cheap to rule out, expensive to discover on a frame.
            if (ReferenceEquals(roman, cond))
            {
                Debug.LogError("[SureThingTmpBootstrap] both faces resolved to ONE asset — the " +
                               "two-voice split (S14/S11) would be silently collapsed.");
                EditorApplication.Exit(3);
            }
        }
    }
}
