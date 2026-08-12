using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SBR.EditorTools
{
    /// <summary>
    /// Measures the SAME strings through both renderers and prints the ratio, so "the migration
    /// preserved rendered size" is a number rather than an expectation.
    ///
    /// <para>Phase T's step T-3 moves exactly one variable — the renderer — and T74's ruling is that
    /// the migration preserves rendered size. Neither claim is checkable by reading the diff: UGUI
    /// rasterises a <c>Font</c> at a pixel size, TMP scales glyph metrics stored at its asset's
    /// sampling size, and whether those land on the same em is a measurement.</para>
    ///
    /// <para>It exists because the G1 copy-fit test failed the moment the surface moved to TMP.
    /// That failure has two very different causes and they need separating before anything is
    /// changed: either TMP measures wider than UGUI for the same face and size (a scale factor,
    /// <c>TvSweatScreen.TypeScale</c>, is owed), or the FACE changed underneath the migration —
    /// legacy <c>Font</c> took whatever instance Unity picked from the variable TTF, and the TMP
    /// asset is the explicitly-resolved Regular 400 / wdth 100. The second would mean T-3 moved two
    /// variables, which C43 forbids, and the fix is a different one entirely.</para>
    ///
    /// <para>Reads BOTH the legacy Font and the TMP asset from the same Resources folder, on the
    /// same strings, at the same sizes. The ratio column is the answer.</para>
    /// </summary>
    public static class TvTypeParityProbe
    {
        // The strings that actually decide this: G1's at-budget pair is what failed, and the rest
        // span the surface's real size range so a per-size drift shows up as a trend rather than a
        // single number that could be a rounding artefact.
        private static readonly (string text, int size)[] Cases =
        {
            ("ONE TEAM SCORELESS", 28),   // G1 at-budget primary, NEED size
            ("ONE TEAM BLANKED", 28),     // G1 authored fallback
            ("CASH OUT $183", 29),        // the cash-out figure
            ("MARKET SUSPENDED", 15),     // the string T20's face error clipped
            ("ZAMBONIS 0 — REGULATORS 1", 36), // the score line, widest routine content
            ("PAYS", 15),
            ("$1,234", 24),
        };

        [MenuItem("SBR/TV/Probe type parity (UGUI vs TMP)")]
        public static void Probe()
        {
            Font legacy = Resources.Load<Font>("Tv/Fonts/EncodeSans");
            TMP_FontAsset tmp = Resources.Load<TMP_FontAsset>("Tv/Fonts/EncodeSans SDF");
            Font legacyCond = Resources.Load<Font>("Tv/Fonts/EncodeSansCondensed");
            TMP_FontAsset tmpCond = Resources.Load<TMP_FontAsset>("Tv/Fonts/EncodeSansCondensed SDF");

            Debug.Log($"[TvTypeParityProbe] legacy regular: {(legacy == null ? "NULL" : legacy.name)} · " +
                      $"tmp regular: {(tmp == null ? "NULL" : tmp.name)} " +
                      $"(face '{(tmp == null ? "-" : tmp.faceInfo.styleName)}') · " +
                      $"legacy cond: {(legacyCond == null ? "NULL" : legacyCond.name)} · " +
                      $"tmp cond: {(tmpCond == null ? "NULL" : tmpCond.name)} " +
                      $"(face '{(tmpCond == null ? "-" : tmpCond.faceInfo.styleName)}')");

            var canvasGo = new GameObject("ParityCanvas", typeof(Canvas));
            try
            {
                Transform parent = canvasGo.transform;
                Report("REGULAR", parent, legacy, tmp);
                Report("CONDENSED", parent, legacyCond, tmpCond);
            }
            finally
            {
                Object.DestroyImmediate(canvasGo);
            }
        }

        private static void Report(string label, Transform parent, Font legacy, TMP_FontAsset tmp)
        {
            if (legacy == null || tmp == null)
            {
                Debug.Log($"[TvTypeParityProbe] {label}: a face is missing, skipped");
                return;
            }

            Debug.Log($"[TvTypeParityProbe] --- {label} --- (uguiPx, tmpPx, tmp/ugui)");
            foreach ((string text, int size) in Cases)
            {
                float u = UguiWidth(parent, legacy, size, text);
                float t = TmpWidth(parent, tmp, size, text);
                float ratio = u > 0f ? t / u : float.NaN;
                Debug.Log($"[TvTypeParityProbe] {label} {size,3}pt  ugui {u,8:0.00}  tmp {t,8:0.00}  " +
                          $"ratio {ratio,6:0.0000}  '{text}'");
            }
        }

        private static float UguiWidth(Transform parent, Font font, int size, string content)
        {
            var go = new GameObject("u", typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = font;
            t.fontSize = size;
            t.text = content;
            TextGenerationSettings s = t.GetGenerationSettings(Vector2.zero);
            float w = t.cachedTextGeneratorForLayout.GetPreferredWidth(content, s) / t.pixelsPerUnit;
            Object.DestroyImmediate(go);
            return w;
        }

        private static float TmpWidth(Transform parent, TMP_FontAsset font, int size, string content)
        {
            var go = new GameObject("t", typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.font = font;
            t.fontSize = size;
            t.enableWordWrapping = false;
            t.text = content;
            float w = t.GetPreferredValues(content, 0f, 0f).x;
            Object.DestroyImmediate(go);
            return w;
        }
    }
}
