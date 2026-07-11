using UnityEngine;
using UnityEngine.UI;

namespace SBR.Game
{
    /// <summary>
    /// The single overlay HUD: a centered 4px crosshair dot and a prompt label under it
    /// ("E - Sit"). The whole canvas hierarchy is built in code at Awake - no prefab, no
    /// serialized UI in the scene (design/05 code-first rule). GrayboxRoomBuilder places
    /// one of these in the scene and wires the interactor reference.
    /// </summary>
    public sealed class InteractionHud : MonoBehaviour
    {
        [Header("Wiring (set by GrayboxRoomBuilder)")]
        public PlayerInteractor interactor;

        [Header("Dials")]
        public string interactKeyLabel = "E";
        public int promptFontSize = 20;
        public Color crosshairIdleColor = new Color(1f, 1f, 1f, 0.55f);
        public Color crosshairHoverColor = new Color(0.55f, 1f, 0.65f, 0.95f);
        public Color promptColor = new Color(0.90f, 0.93f, 0.95f, 0.95f);

        private Image _crosshair;
        private Text _promptLabel;

        private void Awake()
        {
            BuildCanvas();
        }

        private void Update()
        {
            string prompt = interactor != null ? interactor.ActivePrompt : null;
            if (_promptLabel != null)
                _promptLabel.text = string.IsNullOrEmpty(prompt)
                    ? string.Empty
                    : $"{interactKeyLabel} - {prompt}";
            if (_crosshair != null)
                _crosshair.color = interactor != null && interactor.Hovered != null
                    ? crosshairHoverColor
                    : crosshairIdleColor;
        }

        private void BuildCanvas()
        {
            var canvasGo = new GameObject("HudCanvas", typeof(Canvas), typeof(CanvasScaler));
            canvasGo.transform.SetParent(transform, false);

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            // Crosshair: a sprite-less Image renders as a solid tinted rect.
            var crosshairGo = new GameObject("Crosshair", typeof(Image));
            crosshairGo.transform.SetParent(canvasGo.transform, false);
            _crosshair = crosshairGo.GetComponent<Image>();
            _crosshair.color = crosshairIdleColor;
            _crosshair.raycastTarget = false;
            RectTransform crosshairRect = _crosshair.rectTransform;
            crosshairRect.anchorMin = crosshairRect.anchorMax = new Vector2(0.5f, 0.5f);
            crosshairRect.sizeDelta = new Vector2(4f, 4f);
            crosshairRect.anchoredPosition = Vector2.zero;

            var labelGo = new GameObject("Prompt", typeof(Text));
            labelGo.transform.SetParent(canvasGo.transform, false);
            _promptLabel = labelGo.GetComponent<Text>();
            Font font = LoadBuiltinFont();
            if (font != null)
                _promptLabel.font = font;
            _promptLabel.fontSize = promptFontSize;
            _promptLabel.alignment = TextAnchor.UpperCenter;
            _promptLabel.color = promptColor;
            _promptLabel.raycastTarget = false;
            _promptLabel.text = string.Empty;
            RectTransform labelRect = _promptLabel.rectTransform;
            labelRect.anchorMin = labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.sizeDelta = new Vector2(600f, 60f);
            labelRect.anchoredPosition = new Vector2(0f, -46f);
        }

        private static Font LoadBuiltinFont()
        {
            // Unity 6 ships LegacyRuntime.ttf as the built-in UGUI font (Arial was removed).
            try
            {
                return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch
            {
                Debug.LogWarning("[InteractionHud] built-in font not found; prompt text will not render.");
                return null;
            }
        }
    }
}
