using UnityEngine;

namespace SBR.Game
{
    /// <summary>
    /// Base class for anything the crosshair can target. Default hover feedback is a
    /// dependency-free base-color tint applied through a MaterialPropertyBlock (no
    /// material instancing, fully reversible by clearing the block).
    /// GrayboxRoomBuilder wires highlightRenderers explicitly so screen quads (which
    /// ScreenStub animates with its own property block) are never fought over; if the
    /// array is left empty, all child renderers are used.
    /// </summary>
    public abstract class Interactable : MonoBehaviour
    {
        [Header("Hover highlight")]
        [Tooltip("Renderers tinted on hover. Empty = all child renderers.")]
        public Renderer[] highlightRenderers;
        public Color hoverTint = new Color(0.32f, 0.34f, 0.40f, 1f);

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int LegacyColorId = Shader.PropertyToID("_Color");

        private MaterialPropertyBlock _mpb;

        public abstract string Prompt { get; }

        public abstract void OnInteract(PlayerInteractor player);

        protected virtual void Awake()
        {
            if (highlightRenderers == null || highlightRenderers.Length == 0)
                highlightRenderers = GetComponentsInChildren<Renderer>();
            _mpb = new MaterialPropertyBlock();
        }

        public virtual void OnHoverEnter()
        {
            _mpb.Clear();
            _mpb.SetColor(BaseColorId, hoverTint);
            _mpb.SetColor(LegacyColorId, hoverTint);
            ApplyBlock();
        }

        public virtual void OnHoverExit()
        {
            _mpb.Clear(); // empty block restores the material's own values
            ApplyBlock();
        }

        private void ApplyBlock()
        {
            for (int i = 0; i < highlightRenderers.Length; i++)
            {
                if (highlightRenderers[i] != null)
                    highlightRenderers[i].SetPropertyBlock(_mpb);
            }
        }
    }
}
