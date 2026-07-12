using System.Collections;
using UnityEngine;

namespace SBR.Game
{
    /// <summary>
    /// The desk-screen engagement (M4/M5 grill decision: E-zoom + cursor, no sit). Interact while idle
    /// glides the camera to the focus anchor (framing the lid) and zooms to focusFov, then frees the
    /// cursor so the world-space laptop UI takes clicks. Interact again — or holding Move — glides
    /// back to the standing eye position, KEEPING the current look direction (playtest #3's stand-up
    /// lesson) and restoring the standing FOV. High-frequency surface, so the ceremony stays light:
    /// no seat, no look clamp, just the glide the couch already tuned.
    /// </summary>
    public sealed class DeskFocus : Interactable
    {
        /// <summary>The focus that owns the camera, including both glide windows.</summary>
        public static DeskFocus Active { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Active = null; // safety for disabled domain reload
        }

        [Header("Wiring (set by GrayboxRoomBuilder)")]
        [Tooltip("Focused camera pose: in front of the lid, looking at its center.")]
        public Transform focusAnchor;

        [Tooltip("Per-instance interaction copy: laptop and phone must not share a hard-coded prompt.")]
        public string prompt = "Use laptop";

        [Header("Dials")]
        public float transitionDuration = 0.35f;
        [Tooltip("Focused FOV - frames the lid with a little desk around it.")]
        public float focusFov = 30f;
        [Tooltip("Seconds of held Move input that backs the player out.")]
        public float backOutMoveHold = 0.5f;
        [Tooltip("Move magnitude below this does not count as intent to leave.")]
        public float moveDeadzone = 0.4f;

        private enum State { Idle, FocusingIn, Focused, FocusingOut }

        private State _state = State.Idle;
        private FirstPersonController _controller;
        private Camera _lens;
        private Vector3 _preFocusPosition;
        private float _standingFov;
        private float _moveHeldTime;

        public override string Prompt => _state == State.Idle ? prompt : "Back";

        public override void OnInteract(PlayerInteractor player)
        {
            if (_state == State.Idle)
            {
                // Claim before the first glide frame. M5's second focus made the old completion-time
                // claim raceable: both surfaces could start moving the same camera.
                if (Active != null && Active != this)
                    return;

                FirstPersonController controller = player != null
                    ? player.GetComponentInParent<FirstPersonController>()
                    : null;
                if (controller == null)
                    controller = FindAnyObjectByType<FirstPersonController>();
                if (controller == null || controller.cameraTransform == null || focusAnchor == null)
                    return;

                _controller = controller;
                Active = this;
                StartCoroutine(FocusIn());
            }
            else if (_state == State.Focused)
            {
                StartCoroutine(FocusOut());
            }
            // Presses during a transition are ignored.
        }

        private void Update()
        {
            if (_state != State.Focused || _controller == null)
                return;

            if (_controller.MoveInput.magnitude > moveDeadzone)
            {
                _moveHeldTime += Time.deltaTime;
                if (_moveHeldTime >= backOutMoveHold)
                    StartCoroutine(FocusOut());
            }
            else
            {
                _moveHeldTime = 0f;
            }
        }

        private IEnumerator FocusIn()
        {
            _state = State.FocusingIn;
            Transform cam = _controller.cameraTransform;
            _preFocusPosition = cam.position;
            _lens = cam.GetComponent<Camera>();
            if (_lens != null) _standingFov = _lens.fieldOfView;

            _controller.BeginExternalCameraControl();
            yield return CameraGlide.Go(cam, focusAnchor.position, focusAnchor.rotation,
                transitionDuration, _lens, focusFov);

            _controller.SetCursorFree(true); // the laptop UI takes clicks now
            _moveHeldTime = 0f;
            _state = State.Focused;
        }

        private IEnumerator FocusOut()
        {
            _state = State.FocusingOut;
            _controller.SetCursorFree(false); // relock for the glide home
            Transform cam = _controller.cameraTransform;

            // Travel back to the standing eye position only; the view keeps looking at the desk.
            Quaternion keptView = cam.rotation;
            yield return CameraGlide.Go(cam, _preFocusPosition, keptView,
                transitionDuration, _lens, _lens != null ? _standingFov : 0f);

            // Split the kept view into body yaw + camera pitch, then hand control back. ExitSeated
            // is the controller's generic "resume Normal from an external pose" resync.
            _controller.transform.rotation = Quaternion.Euler(0f, keptView.eulerAngles.y, 0f);
            cam.rotation = keptView;
            _controller.ExitSeated();
            _state = State.Idle;
            if (Active == this)
                Active = null;
        }

        private void OnDisable()
        {
            bool ownsCamera = Active == this;
            StopAllCoroutines();

            // Unwind the external pose before releasing the shared claim. Otherwise the other desk
            // surface can acquire a controller still stranded mid-glide (M5 ownership race audit).
            if (ownsCamera && _controller != null && _controller.cameraTransform != null)
            {
                Transform cam = _controller.cameraTransform;
                Quaternion keptView = cam.rotation;
                cam.SetPositionAndRotation(_preFocusPosition, keptView);
                if (_lens != null)
                    _lens.fieldOfView = _standingFov;
                _controller.SetCursorFree(false);
                _controller.transform.rotation = Quaternion.Euler(0f, keptView.eulerAngles.y, 0f);
                cam.rotation = keptView;
                _controller.ExitSeated();
            }

            _state = State.Idle;
            _moveHeldTime = 0f;
            if (ownsCamera)
                Active = null;
        }
    }
}
