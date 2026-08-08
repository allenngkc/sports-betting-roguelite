using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace SBR.EditorTools
{
    /// <summary>
    /// Reports what this TMP build can actually address for S29 (tabular figures) and S20
    /// (weight 600), before either is built.
    ///
    /// **It exists because C15's premise may not hold.** C15 says the migration makes tracking,
    /// tabular figures and weight 600 reachable. Tracking did become reachable and is spent. But
    /// `OTL_FeatureTag` in this package declares exactly four members — kern, liga, mark, mkmk — and
    /// TMP's layout only ever tests those four, so there is no `tnum` to enable. That is a source
    /// read, and a source read is what this project rules against; this measures instead.
    ///
    /// Two questions, two measurements:
    ///
    /// 1. **Do the faces already deliver uniform digits?** S29 was signed on exactly that claim
    ///    ("Archivo Narrow digits uniform, 456/1000 em, spread 0"), which makes tabular figures
    ///    unnecessary rather than unavailable. If the spread is zero, S29 needs no feature and no
    ///    build — it needs an assertion.
    /// 2. **Is a weight-600 face reachable?** S20 ruled no weight tier without TMP named instances.
    ///    Archivo ships as a variable TTF, and FreeType addresses a variable font's named instances
    ///    through the upper 16 bits of the face index. Whether Unity's font engine honours that
    ///    convention is not documented here, so this tries it and reports what comes back.
    ///
    /// Read-only: it writes no asset and changes nothing.
    /// </summary>
    public static class SureThingTypeFeatureProbe
    {
        private const string FontDir = "Assets/SBR/Resources/SureThing/Fonts";

        [MenuItem("SBR/SureThing/Probe type features (S29, S20)")]
        public static void Probe()
        {
            var report = new StringBuilder("\n[TypeProbe] ================ S29 / S20 ================\n");

            report.Append("\n-- S29: digit advances in the shipped assets --\n");
            foreach (string face in new[] { "Archivo SDF", "ArchivoNarrow SDF" })
            {
                var fa = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>($"{FontDir}/{face}.asset");
                if (fa == null) { report.Append($"  {face}: MISSING\n"); continue; }
                fa.TryAddCharacters("0123456789", out _);

                float min = float.MaxValue, max = float.MinValue;
                var each = new StringBuilder();
                for (char c = '0'; c <= '9'; c++)
                {
                    if (!fa.characterLookupTable.TryGetValue(c, out TMP_Character ch) || ch.glyph == null)
                    { each.Append($"{c}=? "); continue; }
                    float adv = ch.glyph.metrics.horizontalAdvance;
                    each.Append($"{c}={adv:0.##} ");
                    if (adv < min) min = adv;
                    if (adv > max) max = adv;
                }
                // Spread is the whole question. Zero means the face is already tabular by
                // construction and S29's signature rests on a fact rather than on a hope.
                report.Append($"  {face}: spread {(max - min):0.####} (min {min:0.##} max {max:0.##})\n");
                report.Append($"    {each}\n");
                report.Append($"    pointSize {fa.faceInfo.pointSize} · scale {fa.faceInfo.scale} · " +
                              $"family '{fa.faceInfo.familyName}' style '{fa.faceInfo.styleName}'\n");
            }

            report.Append("\n-- S20: is a weight-600 named instance reachable? --\n");
            foreach (string ttf in new[] { "Archivo", "ArchivoNarrow" })
            {
                string path = $"{FontDir}/{ttf}.ttf";
                // faceIndex 0 is the default instance. FreeType exposes a variable font's named
                // instances as (instanceIndex << 16), so 65536 is the first, 131072 the second, and
                // so on. Probing a handful is cheaper than assuming either answer.
                foreach (int faceIndex in new[] { 0, 1, 65536, 131072, 196608, 262144, 327680, 393216 })
                {
                    TMP_FontAsset probe = null;
                    try
                    {
                        probe = TMP_FontAsset.CreateFontAsset(path, faceIndex, 90, 9,
                            GlyphRenderMode.SDFAA, 256, 256);
                    }
                    catch (System.Exception e)
                    {
                        report.Append($"  {ttf} faceIndex {faceIndex}: threw {e.GetType().Name}\n");
                        continue;
                    }
                    if (probe == null) { report.Append($"  {ttf} faceIndex {faceIndex}: null\n"); continue; }
                    report.Append($"  {ttf} faceIndex {faceIndex}: family '{probe.faceInfo.familyName}' " +
                                  $"style '{probe.faceInfo.styleName}'\n");
                    Object.DestroyImmediate(probe);
                }
            }

            report.Append("\n[TypeProbe] ============================================\n");
            Debug.Log(report.ToString());
        }
    }
}
