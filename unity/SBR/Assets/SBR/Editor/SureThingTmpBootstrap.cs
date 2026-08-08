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

        [MenuItem("SBR/SureThing/Import TMP essential resources")]
        public static void ImportEssentials()
        {
            if (AssetDatabase.LoadAssetAtPath<Object>(SettingsAsset) != null)
            {
                Debug.Log($"[SureThingTmpBootstrap] already present: {SettingsAsset} — nothing to do");
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

            Debug.Log($"[SureThingTmpBootstrap] importing {full}");
            AssetDatabase.ImportPackage(full, interactive: false);
            AssetDatabase.Refresh();
            // Deliberately NOT asserting the settings asset in this same invocation. ImportPackage
            // completes across a domain reload, so a check here would read the pre-import state and
            // report a false failure. Verify() runs as its own Unity invocation — which is also why
            // the launcher sequences two runs rather than one.
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
