using UnityEngine;
using UnityEngine.InputSystem;

namespace SBR.Game
{
    /// <summary>
    /// Raycasts from the camera center against the Interactable layer, tracks the
    /// nearest Interactable under the crosshair (hover enter/exit + prompt), and
    /// fires it on the Interact action (E / gamepad South).
    /// Interact is polled with WasPressedThisFrame so the template asset's Hold
    /// interaction on the action does not delay a simple press.
    /// While seated with nothing hovered, an Interact press stands the player up
    /// (routed to SitSpot.Active) so "press E again to stand" works even when the
    /// couch is out of view.
    /// </summary>
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [Header("Wiring (set by GrayboxRoomBuilder)")]
        public InputActionAsset actions;
        public Transform rayOrigin; // the player camera
        public LayerMask interactableMask;

        [Header("Dials")]
        [Tooltip("Raycast reach in meters. Builder sets 2.6 so the TV is reachable from the couch (2.18m).")]
        public float range = 2.2f;

        public Interactable Hovered { get; private set; }

        /// <summary>Prompt for the HUD: hovered target first, else the active seat's Stand up.</summary>
        public string ActivePrompt
        {
            get
            {
                if (Hovered != null)
                    return Hovered.Prompt;
                if (SitSpot.Active != null)
                    return SitSpot.Active.Prompt;
                return null;
            }
        }

        private InputAction _interact;

        private void OnEnable()
        {
            if (actions != null)
            {
                InputActionMap map = actions.FindActionMap("Player", throwIfNotFound: false);
                if (map != null)
                {
                    _interact = map.FindAction("Interact");
                    map.Enable();
                }
            }
        }

        private void OnDisable()
        {
            if (Hovered != null)
            {
                Hovered.OnHoverExit();
                Hovered = null;
            }
        }

        private void Update()
        {
            UpdateHover();

            if (_interact != null && _interact.WasPressedThisFrame())
            {
                if (Hovered != null)
                    Hovered.OnInteract(this);
                else if (SitSpot.Active != null)
                    SitSpot.Active.OnInteract(this);
            }
        }

        private void UpdateHover()
        {
            Interactable next = null;
            if (rayOrigin != null &&
                Physics.Raycast(rayOrigin.position, rayOrigin.forward, out RaycastHit hit,
                                range, interactableMask, QueryTriggerInteraction.Collide))
            {
                next = hit.collider.GetComponentInParent<Interactable>();
            }

            if (next == Hovered)
                return;

            if (Hovered != null)
                Hovered.OnHoverExit();
            Hovered = next;
            if (Hovered != null)
                Hovered.OnHoverEnter();
        }
    }
}
