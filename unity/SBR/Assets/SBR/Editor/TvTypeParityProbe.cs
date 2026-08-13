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

        /// <summary>Are this surface's faces TABULAR BY CONSTRUCTION? Owning doc §4 makes tabular
        /// figures mandatory — "scores, clocks, money and counts all change in place; non-tabular
        /// figures make the whole surface twitch on every tick" — and that cannot be satisfied by
        /// turning a feature on. TMP's OTL_FeatureTag declares only kern, liga, mark and mkmk, so
        /// there is no `tnum` to enable; the laptop established this at S29 and closed it by putting
        /// figures on a face whose digits are equal-advance to begin with.
        ///
        /// <para>So the question is a measurement, and this is it: ten of each digit, per face, per
        /// size the surface actually uses for numbers. A tabular face returns one width for all ten.
        /// The spread is the jitter, in canvas px, that a ten-digit figure would show as its digits
        /// change — which is the defect §4 names, stated in the unit it is seen in.</para></summary>
        [MenuItem("SBR/TV/Probe digit advances (tabular check)")]
        public static void ProbeDigits()
        {
            var faces = new (string label, string path, int[] sizes)[]
            {
                ("REGULAR   (score, clock)", "Tv/Fonts/EncodeSans SDF", new[] { 36, 28 }),
                ("REG BOLD  (score, clock)", "Tv/Fonts/EncodeSans Bold SDF", new[] { 36, 28 }),
                ("CONDENSED (money, count)", "Tv/Fonts/EncodeSansCondensed SDF", new[] { 29, 24, 19 }),
                ("COND BOLD (money, count)", "Tv/Fonts/EncodeSansCondensed Bold SDF", new[] { 29, 24, 19 }),
            };

            var canvasGo = new GameObject("DigitProbeCanvas", typeof(Canvas));
            try
            {
                foreach ((string label, string path, int[] sizes) in faces)
                {
                    TMP_FontAsset f = Resources.Load<TMP_FontAsset>(path);
                    if (f == null) { Debug.Log($"[TvDigitProbe] {label}: MISSING at {path}"); continue; }
                    foreach (int size in sizes)
                    {
                        float min = float.MaxValue, max = float.MinValue;
                        string widest = "", narrowest = "";
                        for (char d = '0'; d <= '9'; d++)
                        {
                            float w = TmpWidth(canvasGo.transform, f, size, new string(d, 10));
                            if (w < min) { min = w; narrowest = d.ToString(); }
                            if (w > max) { max = w; widest = d.ToString(); }
                        }
                        float spread = max - min;
                        Debug.Log($"[TvDigitProbe] {label} {size,3}pt  ten-digit width {min:0.00}-{max:0.00}px  " +
                                  $"SPREAD {spread:0.0000}px  " +
                                  $"{(spread < 0.01f ? "TABULAR by construction" : $"PROPORTIONAL (widest '{widest}', narrowest '{narrowest}')")}");
                    }
                }
            }
            finally
            {
                Object.DestroyImmediate(canvasGo);
            }
        }

        /// <summary>T82: what does `tnum` actually substitute, in the font being built? Prints the
        /// digit map <see cref="TvTabularFigures"/> resolves, so the substitution is a printed fact
        /// rather than a claim, and so the reader is exercised by something other than the generator.
        ///
        /// <para>The map is the half of T82 that is solved. What it cannot yet do is REACH the shipped
        /// asset — see the finding recorded with this probe's commit.</para></summary>
        [MenuItem("SBR/TV/Probe tnum substitution")]
        public static void ProbeTnum()
        {
            foreach (string face in new[] { "EncodeSans", "EncodeSansCondensed" })
            {
                string path = $"Assets/SBR/Resources/Tv/Fonts/{face}.ttf";
                var map = TvTabularFigures.ReadTnumMap(path, out string note);
                Debug.Log($"[TvTnumProbe] {face}: {note}");
                if (map.Count == 0) continue;

                // The digits are what the mandate is about; the other ~97 substitutions are the rest
                // of the figure set (fractions, superiors) and are printed only as a count.
                var f = Resources.Load<TMP_FontAsset>($"Tv/Fonts/{face} SDF");
                string digits = f == null ? "(asset not loaded — showing map size only)" : "";
                if (f != null)
                {
                    var parts = new System.Collections.Generic.List<string>();
                    f.TryAddCharacters("0123456789");
                    for (uint u = '0'; u <= '9'; u++)
                        if (f.characterLookupTable.TryGetValue(u, out TMP_Character ch))
                            parts.Add($"{(char)u}:{ch.glyphIndex}->{(map.TryGetValue(ch.glyphIndex, out uint t) ? t.ToString() : "NONE")}");
                    digits = string.Join(" ", parts);
                }
                Debug.Log($"[TvTnumProbe]   digits {digits}");
            }
        }

        /// <summary>T85's second re-measure: does the money control collide? The pair caught the
        /// CASH OUT figure overprinting HOLD E, and the ruled order is to re-measure at tracking 0
        /// before anything is widened or shrunk.
        ///
        /// <para>Builds the real screen and measures the two slots that share §6.1's one fixed
        /// rectangle, rather than reasoning from the layout constants — the figure and the status
        /// word are anchored from opposite edges of the same box, so whether they meet is a fact
        /// about rendered widths, not about the box.</para></summary>
        [MenuItem("SBR/TV/Probe money control fit")]
        public static void ProbeMoneyControl()
        {
            var go = new GameObject("MoneyProbe");
            go.SetActive(false);
            try
            {
                var screen = go.AddComponent<SBR.Game.TvSweatScreen>();
                screen.theaterEnabled = false;
                typeof(SBR.Game.TvSweatScreen)
                    .GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(screen, null);

                TMP_Text figure = FindChild<TMP_Text>(screen.transform, "CashOut");
                TMP_Text status = FindChild<TMP_Text>(screen.transform, "CashOutStatus");
                if (figure == null || status == null)
                {
                    Debug.Log($"[TvMoneyProbe] slots not found (figure {(figure == null ? "MISSING" : "ok")}, " +
                              $"status {(status == null ? "MISSING" : "ok")})");
                    return;
                }

                float slot = figure.rectTransform.rect.width;
                // The six states §6.1 names, in the copy each actually renders.
                foreach (string money in new[] { "CASH OUT $183", "CASH OUT $1,240", "CASHED OUT $183", "MARKET SUSPENDED" })
                {
                    foreach (string word in new[] { "HOLD E", "UPDATING" })
                    {
                        float fw = figure.GetPreferredValues(money, 0f, 0f).x;
                        float sw = status.GetPreferredValues(word, 0f, 0f).x;
                        float slack = slot - (fw + sw);
                        Debug.Log($"[TvMoneyProbe] slot {slot:0.0}px  figure '{money}' {fw:0.0}px + " +
                                  $"status '{word}' {sw:0.0}px = {fw + sw:0.0}px  " +
                                  $"slack {slack:0.0}px  {(slack < 0f ? "COLLIDES" : "clears")}");
                    }
                }
                Debug.Log($"[TvMoneyProbe] tracking in force: figure {figure.characterSpacing:0.##}, " +
                          $"status {status.characterSpacing:0.##} (TMP hundredths of an em)");

                // T85's other re-measure, in the same run so both defects are read off one build.
                // The gate reports fits/misses; T74 needs the MAGNITUDE to choose between widening a
                // span, shrinking a ruled size, and re-authoring.
                TMP_Text need = FindChild<TMP_Text>(screen.transform, "LegRowNeed0");
                if (need != null)
                {
                    float col = need.rectTransform.rect.width;
                    foreach (string s in new[] { "ONE TEAM SCORELESS", "ONE TEAM BLANKED", "LANYARD TO SCORE",
                                                 "BOTH TEAMS SCORE", "MIDDLEMEN ML", "NOT YET" })
                    {
                        float w = need.GetPreferredValues(s, 0f, 0f).x;
                        Debug.Log($"[TvNeedProbe] NEED col {col:0.0}px  '{s}' {w:0.0}px  " +
                                  $"over by {w - col:0.0}px  {(w <= col ? "fits" : "OVERRUNS")}");
                    }
                    Debug.Log($"[TvNeedProbe] tracking in force: {need.characterSpacing:0.##} " +
                              $"(hundredths of an em), size {need.fontSize:0.#}");
                }
            }
            finally { Object.DestroyImmediate(go); }
        }

        private static T FindChild<T>(Transform root, string name) where T : Component
        {
            foreach (T t in root.GetComponentsInChildren<T>(true))
                if (t.gameObject.name == name) return t;
            return null;
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
