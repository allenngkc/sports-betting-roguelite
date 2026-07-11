using System;
using System.Collections;
using UnityEngine;

namespace SBR.Game
{
    /// <summary>
    /// The couch seat. Interact while idle: locks movement (look stays live), eases the
    /// camera (smoothstep, 0.35s) to the seat anchor, then hands look control back to the
    /// controller in Seated mode (yaw/pitch clamped around the seat forward, which faces
    /// the TV). Interact again - or holding Move for more than standUpMoveHold seconds -
    /// eases the camera back to the standing eye position while KEEPING the current look
    /// direction (playtest #3: swinging the view back to the pre-sit pose felt wrong).
    /// The player capsule itself never moves; only the camera travels.
    /// </summary>
    public sealed class SitSpot : Interactable
    {
        /// <summary>The seat currently occupied, if any. M2 has exactly one.</summary>
        public static SitSpot Active { get; private set; }

        /// <summary>Raised when the player sits (true) or stands (false). M3's TvSweatScreen listens so
        /// the sweat steps only while seated (design/04): sitting starts/resumes, standing pauses.</summary>
        public static event Action<bool> SeatedChanged;

        /// <summary>M3 interact-suppression hook: while this returns true (a live cash-out offer is
        /// showing), an Interact press must NOT stand the player up - E is the cash-out accept, and
        /// standing is hold-move during a live sweat. Null = never suppressed. TvSweatScreen owns it.</summary>
        public static Func<bool> InteractStandSuppressed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            // Safety for disabled domain reload: clear statics before scene objects wire themselves up.
            Active = null;
            SeatedChanged = null;
            InteractStandSuppressed = null;
        }

        [Header("Wiring (set by GrayboxRoomBuilder)")]
        [Tooltip("Seated eye pose: position + base rotation facing the TV.")]
        public Transform seatAnchor;

        [Header("Dials")]
        public float transitionDuration = 0.35f;
        [Tooltip("Seated camera FOV - the sit zooms in so the TV nearly fills the view " +
                 "(playtest #4: at the standing FOV the TV read too small). 0 = leave FOV alone.")]
        public float seatedFov = 17f;
        [Tooltip("Seated look-around clamps, degrees around the seat forward. Tight on purpose " +
                 "(playtest #5): a glance that keeps the TV in view, not a room survey.")]
        public float seatedYawLimit = 12f;
        public float seatedPitchLimit = 8f;
        [Tooltip("Seconds of held Move input that stands the player back up.")]
        public float standUpMoveHold = 0.5f;
        [Tooltip("Move magnitude below this does not count as intent to stand.")]
        public float moveDeadzone = 0.4f;

        private enum State { Idle, SittingDown, Seated, StandingUp }

        private State _state = State.Idle;
        private FirstPersonController _controller;
        private Vector3 _preSitPosition;
        private float _moveHeldTime;
        private Camera _lens;
        private float _standingFov;

        public override string Prompt => _state == State.Idle ? "Sit" : "Stand up";

        public override void OnInteract(PlayerInteractor player)
        {
            if (_state == State.Idle)
            {
                FirstPersonController controller = player != null
                    ? player.GetComponentInParent<FirstPersonController>()
                    : null;
                if (controller == null)
                    controller = FindAnyObjectByType<FirstPersonController>();
                if (controller == null || controller.cameraTransform == null || seatAnchor == null)
                    return;

                _controller = controller;
                StartCoroutine(SitDown());
            }
            else if (_state == State.Seated)
            {
                // While a live cash-out offer is showing, E belongs to the sweat (accept), not standing;
                // the player stands with hold-move instead (design/04, M3 decision 6).
                if (InteractStandSuppressed != null && InteractStandSuppressed())
                    return;
                StartCoroutine(StandUp());
            }
            // Presses during a transition are ignored.
        }

        private void Update()
        {
            if (_state != State.Seated || _controller == null)
                return;

            if (_controller.MoveInput.magnitude > moveDeadzone)
            {
                _moveHeldTime += Time.deltaTime;
                if (_moveHeldTime >= standUpMoveHold)
                    StartCoroutine(StandUp());
            }
            else
            {
                _moveHeldTime = 0f;
            }
        }

        private IEnumerator SitDown()
        {
            _state = State.SittingDown;
            Transform cam = _controller.cameraTransform;
            _preSitPosition = cam.position;
            _lens = seatedFov > 0f ? cam.GetComponent<Camera>() : null;
            if (_lens != null) _standingFov = _lens.fieldOfView;

            _controller.BeginExternalCameraControl();
            yield return LerpCamera(cam, seatAnchor.position, seatAnchor.rotation, seatedFov);

            // Look scale = zoom ratio, so the mouse covers screen space at the standing speed.
            float lookScale = _lens != null && _standingFov > 0f ? seatedFov / _standingFov : 1f;
            _controller.EnterSeated(seatAnchor.rotation, seatedYawLimit, seatedPitchLimit, lookScale);
            _moveHeldTime = 0f;
            _state = State.Seated;
            Active = this;
            SeatedChanged?.Invoke(true); // the sweat starts/resumes
        }

        private IEnumerator StandUp()
        {
            _state = State.StandingUp;
            Active = null;
            SeatedChanged?.Invoke(false); // you looked away - the sweat pauses, the offer stays frozen
            Transform cam = _controller.cameraTransform;

            _controller.BeginExternalCameraControl();
            // Travel back to the standing eye position only; the view stays where the player
            // was looking (e.g. still on the TV), so standing never swings the camera around.
            Quaternion keptView = cam.rotation;
            yield return LerpCamera(cam, _preSitPosition, keptView, _lens != null ? _standingFov : 0f);

            // Split the kept view into body yaw + camera pitch so ExitSeated's resync from the
            // transforms is snap-free.
            _controller.transform.rotation = Quaternion.Euler(0f, keptView.eulerAngles.y, 0f);
            cam.rotation = keptView;
            _controller.ExitSeated();
            _state = State.Idle;
        }

        private IEnumerator LerpCamera(Transform cam, Vector3 toPosition, Quaternion toRotation,
                                       float toFov = 0f)
        {
            Vector3 fromPosition = cam.position;
            Quaternion fromRotation = cam.rotation;
            float fromFov = _lens != null ? _lens.fieldOfView : 0f;
            bool zoom = _lens != null && toFov > 0f;
            float duration = Mathf.Max(0.01f, transitionDuration);
            float t = 0f;

            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / duration);
                float eased = t * t * (3f - 2f * t); // smoothstep
                cam.SetPositionAndRotation(
                    Vector3.Lerp(fromPosition, toPosition, eased),
                    Quaternion.Slerp(fromRotation, toRotation, eased));
                if (zoom) _lens.fieldOfView = Mathf.Lerp(fromFov, toFov, eased);
                yield return null;
            }
        }
    }
}
