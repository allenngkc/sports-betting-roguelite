using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace SBR.Game
{
    /// <summary>
    /// M5's face-up bookie thread: a code-built world-space canvas, bottom anchored like texting,
    /// with stamped rounds from the model rather than the live run. The visual-only buzz drives
    /// emission and a tiny light.
    ///
    /// The design/08 authority claim that stood here is deleted with R39's values. design/08 is T3,
    /// the deprecated anti-reference, dead since 2026-07-24 — a live comment in shipped code citing
    /// it as a licence is C7's shape inside source rather than in docs.
    ///
    /// OWNERSHIP, because this file straddles the line: the room owns the OBJECT and therefore its
    /// EMISSION, which reaches the room as light (R28, and S63's split one surface over). Nobody
    /// owns the CONTENT — R28-am keeps the live BookieFeed and forbids anything being authored onto
    /// the screen. So the emission block below is ruled and built; the content colours further down
    /// are neither, and R39 does not touch them.
    /// </summary>
    public sealed class PhoneScreen : MonoBehaviour
    {
        [Header("Wiring (set by GrayboxRoomBuilder)")]
        public BookieFeed feed;
        public Renderer screenRenderer;
        public Light buzzLight;

        [Header("Layout")]
        public Vector2 screenWorldSize = new Vector2(0.065f, 0.135f);
        [Tooltip("Metres the canvas floats above the face-up screen.")]
        public float canvasOffset = 0.0015f;
        public int referencePixelsWide = 420;

        [Header("Screen emission")]
        // R39 (DD 2026-08-07, batch 14) — STRUCK, same family as S63 and for the same reason.
        //
        // Was:  idle   (0.020, 0.030, 0.060)  chroma 14.5  hue 278.9deg  — and ALWAYS ON
        //       unread (0.055, 0.105, 0.180)  chroma 18.0  hue 264.5deg  — live in the b13 frame
        //       buzz   (0.30,  0.50,  0.90 )  chroma 31.9  hue 271.4deg
        //
        // All three blue-dominant, in the quadrant §1.1 names as its own failure mode, on an object
        // the room owns (R28) — and the rest state carried 2.7x the chroma of the laptop's granted
        // rest state. None of it was ever audited: no instrument in this studio reads an emission
        // value and judges it, so the only region that samples this phone reads it on the
        // screens-DARK set, with its emission switched off by construction.
        //
        // The phone is HIS (§6), so it joins the laptop's granted family rather than taking a
        // colour of its own — ONE authored chromaticity for both of his screens, amplitude per
        // state. Writing the ladder as multiples of one base instead of three hand-picked triples
        // is the lid's lesson: "one family" then holds BY CONSTRUCTION rather than by my matching
        // three chromaticities and asserting they agree.
        //
        // THE AMPLITUDE LADDER IS PRESERVED, so this is a hue change and not a value change —
        // R35's caution, which the drab-green swatch had to answer:
        //
        //   idle    x1    L* 21.09   (was 20.06,  +1.03)
        //   unread  x3    L* 37.50   (was 37.80,  -0.30)
        //   buzz    x15   L* 75.48   (was 75.22,  +0.26)
        //
        // chroma falls 14.5 -> 5.4, 18.0 -> 7.8, 31.9 -> 13.3; hue is 83.3deg at all three ends.
        //
        // EXACT VALUES ARE NOT RULED. The DD ruled the direction and held the values for the
        // instrument, exactly as S63's were held — "I am not setting three values blind". Unlike
        // the lid, these are observable, so the frame is obtainable. Treat these as the proposal.
        //
        // THE BUZZ ITSELF IS KEPT: colours struck, event kept. buzzDuration and the wave below are
        // untouched. A 0.55s flash is not R37's continuous pulse and the two were explicitly not
        // flattened together — what was ruled is the blue at chroma 31.9 driving a real Light.
        public static readonly Color RestEmission = LaptopScreen.GrantedLidEmission;

        [ColorUsage(false, true)] public Color idleEmission = Amp(1f);
        [ColorUsage(false, true)] public Color unreadEmission = Amp(3f);
        [ColorUsage(false, true)] public Color buzzEmission = Amp(15f);
        public float buzzDuration = 0.55f;
        public float lightIntensity = 0.65f;

        /// <summary>
        /// One base chromaticity, scaled. Written out rather than using Color * float because that
        /// operator scales alpha too, and a serialized a=15 on an emission colour is a puzzle
        /// waiting for whoever reads the scene next.
        /// </summary>
        private static Color Amp(float k)
            => new Color(RestEmission.r * k, RestEmission.g * k, RestEmission.b * k);

        // CONTENT colours — drawn on the canvas, not emitted into the room. Deliberately NOT
        // touched by R39, which ruled the emission above and nothing else.
        //
        // The "(design/08)" authority that headed this block is deleted for the reason given in the
        // class summary, but deleting a false citation is not the same as ruling the values under
        // it, and these are not mine to rule: chromeCyan (0.62, 0.86, 0.96) is T9, a RETIRED hue,
        // still printed on the BOOKIE label below. Its replacement is a design call and the
        // surface's content authority is exactly what R28/R28-am leave unassigned — the room owns
        // the object, nobody owns the content, and nothing may be authored onto the screen.
        // Flagged, not fixed. It is also invisible to T30's scan, which matches named retired
        // constants verbatim and would only catch this one because it IS the verbatim constant.
        [Header("Palette — CONTENT, unruled (see note)")]
        public Color screenBg = new Color(0.018f, 0.022f, 0.030f, 0.98f);
        public Color bubble = new Color(0.18f, 0.19f, 0.21f, 0.98f);
        public Color textColor = new Color(0.93f, 0.92f, 0.88f, 1f);
        public Color chromeCyan = new Color(0.62f, 0.86f, 0.96f, 1f);

        private Canvas _canvas;
        private RectTransform _threadRoot;
        private Text _badgeText;
        private GameObject _badge;
        private MaterialPropertyBlock _emissionBlock;
        private long _renderedRevision = -1;
        private long _seenArrivalSequence;
        private float _buzzStartedAt = -1f;
        private string _renderedText = "";

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        public string RenderedText => _renderedText;
        public int RenderedMessageCount { get; private set; }

        private void Awake()
        {
            _emissionBlock = new MaterialPropertyBlock();
            BuildCanvas();
        }

        private void Update()
        {
            if (_canvas.worldCamera == null)
                _canvas.worldCamera = Camera.main;

            if (feed != null)
            {
                if (feed.Revision != _renderedRevision)
                {
                    _renderedRevision = feed.Revision;
                    RebuildThread();
                }

                if (feed.ArrivalSequence > _seenArrivalSequence)
                {
                    _seenArrivalSequence = feed.ArrivalSequence;
                    _buzzStartedAt = Time.unscaledTime;
                }
            }

            UpdateBuzz();
        }

        private void OnDisable()
        {
            if (screenRenderer != null && _emissionBlock != null)
            {
                _emissionBlock.Clear();
                screenRenderer.SetPropertyBlock(_emissionBlock);
            }
            if (buzzLight != null)
            {
                buzzLight.intensity = 0f;
                buzzLight.enabled = false;
            }
        }

        private void RebuildThread()
        {
            for (int i = _threadRoot.childCount - 1; i >= 0; i--)
                Destroy(_threadRoot.GetChild(i).gameObject);

            var sb = new StringBuilder();
            foreach (BookieMessage message in feed.Messages)
            {
                if (sb.Length > 0) sb.Append("\n\n");
                sb.Append("ROUND-").Append(message.Round).Append("  ·  BOOKIE\n")
                  .Append(message.Text);
            }

            _renderedText = sb.ToString();
            float y = 0f;
            for (int i = feed.Messages.Count - 1; i >= 0; i--)
            {
                BookieMessage message = feed.Messages[i];
                const float bubbleHeight = 116f;
                RectTransform messageBubble = MakePanel(_threadRoot, $"Message{i}",
                    new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, y),
                    new Vector2(_threadRoot.rect.width, bubbleHeight), bubble);
                MakeText(messageBubble, "Copy", new Vector2(0f, 0f), new Vector2(0f, 0f),
                    new Vector2(16f, 12f), new Vector2(messageBubble.rect.width - 32f, bubbleHeight - 24f),
                    24, TextAnchor.LowerLeft, textColor,
                    $"ROUND-{message.Round}  ·  BOOKIE\n{message.Text}");
                y += bubbleHeight + 12f;
            }

            RenderedMessageCount = feed.Messages.Count;
            _badge.SetActive(feed.UnreadCount > 0);
            _badgeText.text = feed.UnreadCount > 99 ? "99+" : feed.UnreadCount.ToString();
        }

        private void UpdateBuzz()
        {
            if (screenRenderer == null)
                return;

            bool unread = feed != null && feed.UnreadCount > 0;
            Color emission = unread ? unreadEmission : idleEmission;
            float elapsed = Time.unscaledTime - _buzzStartedAt;
            bool buzzing = _buzzStartedAt >= 0f && elapsed < Mathf.Max(0.05f, buzzDuration);
            float wave = 0f;
            if (buzzing)
            {
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.05f, buzzDuration));
                wave = Mathf.Sin(t * Mathf.PI);
                emission = Color.Lerp(emission, buzzEmission, wave);
            }

            _emissionBlock.SetColor(EmissionColorId, emission);
            screenRenderer.SetPropertyBlock(_emissionBlock);

            if (buzzLight != null)
            {
                buzzLight.enabled = buzzing;
                buzzLight.intensity = buzzing ? lightIntensity * wave : 0f;
            }
        }

        private void BuildCanvas()
        {
            int width = referencePixelsWide;
            int height = Mathf.RoundToInt(width * screenWorldSize.y / screenWorldSize.x);

            var canvasGo = new GameObject("PhoneCanvas", typeof(Canvas), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            RectTransform canvasRt = _canvas.GetComponent<RectTransform>();
            canvasRt.sizeDelta = new Vector2(width, height);

            if (screenRenderer != null)
            {
                Transform screen = screenRenderer.transform;
                Vector3 outwardNormal = -screen.forward; // Quad visible face is -Z; here that is up.
                // As on TvSweatScreen/LaptopScreen, canvas +Z points INTO the display. Flipping this
                // mirrors world-space UGUI even when the double-sided material still looks healthy.
                canvasGo.transform.SetPositionAndRotation(
                    screen.position + outwardNormal * canvasOffset,
                    Quaternion.LookRotation(-outwardNormal, screen.up));
            }
            canvasGo.transform.localScale = Vector3.one * (screenWorldSize.x / width);

            Image backing = MakeStretchImage(canvasGo.transform, "Backing", screenBg);
            backing.raycastTarget = false;

            RectTransform header = MakePanel(canvasRt, "Header", new Vector2(0f, 1f), new Vector2(0f, 1f),
                Vector2.zero, new Vector2(width, 74f), new Color(0f, 0f, 0f, 0.35f));
            MakeText(header, "Title", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(20f, 0f), new Vector2(260f, 50f), 27, TextAnchor.MiddleLeft,
                chromeCyan, "BOOKIE");

            _badge = MakePanel(header, "UnreadBadge", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-16f, 0f), new Vector2(76f, 44f), new Color(0.10f, 0.24f, 0.30f, 1f)).gameObject;
            _badgeText = MakeText((RectTransform)_badge.transform, "Count", new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(76f, 44f), 23,
                TextAnchor.MiddleCenter, textColor, "");

            _threadRoot = MakePanel(canvasRt, "Thread", new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(14f, 16f), new Vector2(width - 28f, height - 104f),
                new Color(0f, 0f, 0f, 0f));
            _threadRoot.gameObject.AddComponent<RectMask2D>();
        }

        private Text MakeText(RectTransform parent, string name, Vector2 anchor, Vector2 pivot,
            Vector2 position, Vector2 size, int fontSize, TextAnchor alignment, Color color, string content)
        {
            var go = new GameObject(name, typeof(Text));
            go.transform.SetParent(parent, false);
            var value = go.GetComponent<Text>();
            try { value.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
            catch { Debug.LogWarning("[PhoneScreen] built-in font not found; text will not render."); }
            value.fontSize = fontSize;
            value.alignment = alignment;
            value.color = color;
            value.text = content;
            value.raycastTarget = false;
            value.horizontalOverflow = HorizontalWrapMode.Wrap;
            value.verticalOverflow = VerticalWrapMode.Overflow;
            RectTransform rt = value.rectTransform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return value;
        }

        private static RectTransform MakePanel(RectTransform parent, string name, Vector2 anchor,
            Vector2 pivot, Vector2 position, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            RectTransform rt = image.rectTransform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return rt;
        }

        private static Image MakeStretchImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            RectTransform rt = image.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return image;
        }
    }
}
