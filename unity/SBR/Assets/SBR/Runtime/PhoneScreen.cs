using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace SBR.Game
{
    /// <summary>
    /// M5's face-up bookie thread: a code-built world-space canvas, bottom anchored like texting,
    /// with stamped rounds from the model rather than the live run. Neutral bubbles keep design/08's
    /// palette law intact; cyan is chrome, while the visual-only buzz pulses emission and a tiny light.
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

        [Header("Visual buzz")]
        [ColorUsage(false, true)] public Color idleEmission = new Color(0.020f, 0.030f, 0.060f);
        [ColorUsage(false, true)] public Color unreadEmission = new Color(0.055f, 0.105f, 0.180f);
        [ColorUsage(false, true)] public Color buzzEmission = new Color(0.30f, 0.50f, 0.90f);
        public float buzzDuration = 0.55f;
        public float lightIntensity = 0.65f;

        [Header("Palette (design/08)")]
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
