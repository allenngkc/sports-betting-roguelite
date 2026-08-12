using System.Collections.Generic;
using SBR.Engine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SBR.Game
{
    /// <summary>
    /// The sweat's memory aid: one compact strip per leg. Dots are only added by the TV's
    /// reveal callbacks, so looking away never spoils a beat. Resolved rows collapse to a
    /// single cap.
    /// <para>
    /// T16 (design register, 2026-07-31) rules this component "no numerals, no hue, never
    /// above L2". There is consequently no colour in this file at all: every mark is white or
    /// grey, and the tier ladder carries every distinction the retired green / red / cyan used
    /// to carry. Size (the delta band) and brightness (the tier) are the only channels left,
    /// and neither one can be mistaken for the banned win-probability readout.
    /// </para>
    /// </summary>
    public sealed class MomentumTape : MonoBehaviour
    {
        private const float SmallDot = 6f;
        private const float MidDot = 9f;
        private const float BigDot = 12f;
        private const float RowHeight = 14f;
        private const float RowGap = 3f;

        // The MOMENTUM label: canon's eyebrow scale (--tv-size-eyebrow 15, tokens/typography.css),
        // mirrored here as a named constant with its source cited because a C# const cannot import
        // a CSS custom property (handoff §4A).
        private const int LabelSize = 15;
        private const float LabelWidth = 96f;
        private const float LabelGap = 10f;

        private TMP_Text _label;

        private readonly List<Row> _rows = new List<Row>();
        private RectTransform _rect;
        private float _rowWidth;

        // ── The tier ladder ─────────────────────────────────────────────────────────────────
        // Mirrored from main-2/docs/design/design-system/components/tv/tiers.js, which reads
        // verbatim `{ L4: 1, L3: 0.7, L2: 0.4, L1: 0.15, L0: 0 }` under the law "brightness is
        // the primary semantic channel, hue is secondary". T16 caps THIS component at L2, so L4
        // and L3 are deliberately not named below: a constant this file is forbidden to use is a
        // constant a later edit will eventually reach for. L0 is not named either — a fully
        // transparent mark is indistinguishable from no mark, and every mark drawn here has to
        // keep reading as "something happened on this leg".
        private const float TierL2 = 0.4f;  // the ceiling: the current sample, and a landed leg
        private const float TierL1 = 0.15f; // the dormant tier: history, and legs already settled

        // ── The colourless ramp ─────────────────────────────────────────────────────────────
        // T16's spec of record (main-2/docs/design/design-system/components/tv/
        // TvMomentumTape.prompt.md): "no hue (white and grey only — everything on this surface
        // except gold is colourless)". These mirror the TV palette's three colourless roles
        // (main-2/docs/design/design-system/tokens/palette-tv.css) flattened to true neutrals —
        // even those tokens carry a faint cool cast (--tv-context is #7A878F, not a grey) and
        // T16 leaves no room for it. Value only: the tier above supplies every alpha.
        private const float NeutralFact = 1f;        // --tv-fact #E7F1F5, de-tinted to plain white
        private const float NeutralContext = 0.52f;  // --tv-context #7A878F → mean channel 133/255
        private const float NeutralStructure = 0.33f; // --tv-structure #4A555C → mean channel 84/255

        // The only colour constructor in this file. r == g == b by construction, so "no hue" is a
        // property of the code and not a convention a later edit can quietly break.
        private static Color Neutral(float value, float tier) => new Color(value, value, value, tier);

        // Dots. The spec splits the strip in two — "label and current sample at L2, history at
        // L1" — mirroring TvMomentumTape.jsx:45-46 (`i === last ? tier("L2") : tier("L1")`, over
        // --tv-fact for the current sample and --tv-context for history).
        private static readonly Color CurrentSample = Neutral(NeutralFact, TierL2);
        private static readonly Color HistorySample = Neutral(NeutralContext, TierL1);

        // Caps. Until T16 these were three saturated fields on this file's lines 24-26 —
        // _green #3CE873 (W), _red #FF4038 (L), _cyan #9EDCF6 (VOID) — all at alpha 1f, which
        // broke the no-hue rule and the L2 ceiling at once. The ramp carries the three grades
        // now, and §4/§8's standing rule survives the translation intact: "loss is darkness,
        // never red", so the lost cap is the dimmest mark this component can draw.
        private static readonly Color CapWon = Neutral(NeutralFact, TierL2);        // a landed leg is fact
        private static readonly Color CapLost = Neutral(NeutralStructure, TierL1);  // darkness, not red
        private static readonly Color CapVoided = Neutral(NeutralContext, TierL1);  // never a result: context

        private sealed class Row
        {
            public readonly RectTransform Root;
            public readonly Image Cap;
            public readonly List<Image> Dots = new List<Image>();
            public float Cursor;

            public Row(RectTransform root, Image cap)
            {
                Root = root;
                Cap = cap;
            }
        }

        /// <summary>Builds a code-only UGUI tape in the supplied canvas hierarchy.</summary>
        /// <summary>Builds the tape. <paramref name="labelFont"/> is canon's regular face
        /// (`--font-tv`) for the MOMENTUM label; the tape holds no font policy of its own, so the
        /// caller that already resolves the surface's two faces passes the right one in.</summary>
        public static MomentumTape Build(Transform parent, Vector2 position, Vector2 size,
            TMP_FontAsset labelFont = null)
        {
            var go = new GameObject("MomentumTape", typeof(RectTransform), typeof(MomentumTape));
            go.transform.SetParent(parent, false);
            var tape = go.GetComponent<MomentumTape>();
            tape._rect = go.GetComponent<RectTransform>();
            tape._rect.anchorMin = tape._rect.anchorMax = new Vector2(0.5f, 0.5f);
            tape._rect.pivot = new Vector2(0.5f, 0.5f);
            tape._rect.sizeDelta = size;
            tape._rect.anchoredPosition = position;
            tape._rowWidth = Mathf.Max(1f, size.x);

            // The MOMENTUM label (TvMomentumTape.jsx:25-28). It did not exist — this component had
            // no Text at all — so the DD's tier correction landed on an element that was not built.
            //
            // L2, per that correction: labels live at L2, and L1 is the dormant/structure tier. The
            // bars keep their own tiers (current sample L2, history L1 as structure), so the label
            // reads at the same weight as the live sample rather than sinking to history.
            //
            // Regular face and --tv-context, not condensed: canon marks the tape's own chrome
            // regular (TvMomentumTape.jsx:23) and only the dense numeric slots condensed.
            if (labelFont != null)
            {
                var labelGo = new GameObject("MomentumLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelGo.transform.SetParent(tape.transform, false);
                var label = labelGo.GetComponent<TextMeshProUGUI>();
                label.font = labelFont;
                label.fontSize = LabelSize;
                label.text = "MOMENTUM";
                label.color = Neutral(NeutralContext, TierL2);
                label.alignment = TextAlignmentOptions.Left;
                label.raycastTarget = false;
                label.enableWordWrapping = false; // was HorizontalWrapMode.Overflow: never wrap
                var lrt = label.rectTransform;
                lrt.anchorMin = lrt.anchorMax = new Vector2(0f, 0.5f);
                lrt.pivot = new Vector2(1f, 0.5f);           // sits to the LEFT of the bars
                lrt.sizeDelta = new Vector2(LabelWidth, RowHeight + RowGap);
                lrt.anchoredPosition = new Vector2(-LabelGap, 0f);
                tape._label = label;
            }

            tape.Show(false);
            return tape;
        }

        /// <summary>Rebuilds the compact rows for a new ticket.</summary>
        public void ResetForTicket(int legCount)
        {
            for (int i = 0; i < _rows.Count; i++)
                Destroy(_rows[i].Root.gameObject);
            _rows.Clear();

            int count = Mathf.Max(0, legCount);
            float totalHeight = count * RowHeight + Mathf.Max(0, count - 1) * RowGap;
            for (int i = 0; i < count; i++)
            {
                var rowGo = new GameObject($"LegTape_{i + 1}", typeof(RectTransform));
                rowGo.transform.SetParent(transform, false);
                var rowRt = rowGo.GetComponent<RectTransform>();
                rowRt.anchorMin = rowRt.anchorMax = new Vector2(0f, 0.5f);
                rowRt.pivot = new Vector2(0f, 0.5f);
                rowRt.sizeDelta = new Vector2(_rowWidth, RowHeight);
                rowRt.anchoredPosition = new Vector2(0f,
                    totalHeight * 0.5f - RowHeight * 0.5f - i * (RowHeight + RowGap));

                var capGo = new GameObject("ResolutionCap", typeof(Image));
                capGo.transform.SetParent(rowGo.transform, false);
                var cap = capGo.GetComponent<Image>();
                cap.raycastTarget = false;
                cap.enabled = false;
                var capRt = cap.rectTransform;
                capRt.anchorMin = capRt.anchorMax = new Vector2(0f, 0.5f);
                capRt.pivot = new Vector2(0f, 0.5f);
                capRt.anchoredPosition = Vector2.zero;
                capRt.sizeDelta = new Vector2(_rowWidth, 4f);

                _rows.Add(new Row(rowRt, cap));
            }
        }

        /// <summary>Appends one revealed non-final beat to a leg's live strip.</summary>
        /// <param name="beneficiary">
        /// Accepted and deliberately ignored. T16 forbids hue on this surface, so a dot cannot
        /// take the team colour (or any colour) — it used to, on this file's old line 104, and
        /// that was the violation. The parameter survives only because removing it would change
        /// the public API, which is out of scope for a colour-and-tier fix; callers may pass
        /// anything. The strip differentiates by size (the delta band) and tier, nothing else.
        /// </param>
        public void AppendBeat(int legIx, Color beneficiary, int band)
        {
            if (legIx < 0 || legIx >= _rows.Count) return;
            Row row = _rows[legIx];
            if (row.Cap.enabled) return;

            // Everything already on the strip has just become history, so it drops to L1 before
            // the new sample lands at L2. That leaves exactly one L2 dot per row, which is what
            // "current sample at L2, history at L1" means for a strip built one dot at a time.
            for (int i = 0; i < row.Dots.Count; i++) row.Dots[i].color = HistorySample;

            float diameter = band <= 0 ? SmallDot : band == 1 ? MidDot : BigDot;
            var dotGo = new GameObject($"Beat_{row.Dots.Count + 1}", typeof(Image));
            dotGo.transform.SetParent(row.Root, false);
            var dot = dotGo.GetComponent<Image>();
            dot.color = CurrentSample; // was `beneficiary`, the team hue — banned by T16
            dot.raycastTarget = false;
            var dotRt = dot.rectTransform;
            dotRt.anchorMin = dotRt.anchorMax = new Vector2(0f, 0.5f);
            dotRt.pivot = new Vector2(0f, 0.5f);
            dotRt.sizeDelta = new Vector2(diameter, diameter);
            dotRt.anchoredPosition = new Vector2(row.Cursor, 0f);
            row.Cursor += diameter + 3f;
            row.Dots.Add(dot);
        }

        /// <summary>
        /// Collapses a resolved leg to its single cap. The grade reads off the tier ladder, never
        /// off a hue: T16 retired the green / red / cyan money signals on this surface, and a
        /// resolved leg is by definition no longer the live thing, so no cap passes L2.
        /// </summary>
        public void ResolveLeg(int legIx, LegGrade grade)
        {
            if (legIx < 0 || legIx >= _rows.Count) return;
            Row row = _rows[legIx];
            for (int i = 0; i < row.Dots.Count; i++) row.Dots[i].enabled = false;

            row.Cap.color = grade == LegGrade.Won ? CapWon
                : grade == LegGrade.Lost ? CapLost : CapVoided;
            row.Cap.enabled = true;
        }

        public void Show(bool visible)
        {
            if (gameObject.activeSelf != visible) gameObject.SetActive(visible);
        }
    }
}
