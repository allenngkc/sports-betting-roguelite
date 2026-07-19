using System.Collections.Generic;
using SBR.Engine;
using UnityEngine;
using UnityEngine.UI;

namespace SBR.Game
{
    /// <summary>
    /// The sweat's memory aid: one compact strip per leg. Dots are only added by the TV's
    /// reveal callbacks, so looking away never spoils a beat. Resolved rows collapse to a
    /// money-signal cap; team-colored dots themselves never use reserved money colors.
    /// </summary>
    public sealed class MomentumTape : MonoBehaviour
    {
        private const float SmallDot = 6f;
        private const float MidDot = 9f;
        private const float BigDot = 12f;
        private const float RowHeight = 14f;
        private const float RowGap = 3f;

        private readonly List<Row> _rows = new List<Row>();
        private RectTransform _rect;
        private float _rowWidth;
        private Color _green = new Color(0x3C / 255f, 0xE8 / 255f, 0x73 / 255f, 1f);
        private Color _red = new Color(0xFF / 255f, 0x40 / 255f, 0x38 / 255f, 1f);
        private Color _cyan = new Color(0x9E / 255f, 0xDC / 255f, 0xF6 / 255f, 1f);

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
        public static MomentumTape Build(Transform parent, Vector2 position, Vector2 size)
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
        public void AppendBeat(int legIx, Color beneficiary, int band)
        {
            if (legIx < 0 || legIx >= _rows.Count) return;
            Row row = _rows[legIx];
            if (row.Cap.enabled) return;

            float diameter = band <= 0 ? SmallDot : band == 1 ? MidDot : BigDot;
            var dotGo = new GameObject($"Beat_{row.Dots.Count + 1}", typeof(Image));
            dotGo.transform.SetParent(row.Root, false);
            var dot = dotGo.GetComponent<Image>();
            dot.color = beneficiary;
            dot.raycastTarget = false;
            var dotRt = dot.rectTransform;
            dotRt.anchorMin = dotRt.anchorMax = new Vector2(0f, 0.5f);
            dotRt.pivot = new Vector2(0f, 0.5f);
            dotRt.sizeDelta = new Vector2(diameter, diameter);
            dotRt.anchoredPosition = new Vector2(row.Cursor, 0f);
            row.Cursor += diameter + 3f;
            row.Dots.Add(dot);
        }

        /// <summary>Collapses a resolved leg to its sanctioned money-signal cap.</summary>
        public void ResolveLeg(int legIx, LegGrade grade)
        {
            if (legIx < 0 || legIx >= _rows.Count) return;
            Row row = _rows[legIx];
            for (int i = 0; i < row.Dots.Count; i++) row.Dots[i].enabled = false;

            row.Cap.color = grade == LegGrade.Won ? _green
                : grade == LegGrade.Lost ? _red : _cyan;
            row.Cap.enabled = true;
        }

        public void Show(bool visible)
        {
            if (gameObject.activeSelf != visible) gameObject.SetActive(visible);
        }
    }
}
