using System.Collections;
using UnityEngine;

namespace SBR.Game
{
    /// <summary>
    /// The couch seat. Interact while idle: locks movement (look stays live), eases the
    /// camera (smoothstep, 0.35s) to the seat anchor, then hands look control back to the
    /// controller in Seated mode (yaw/pitch clamped around the seat forward, which faces
    /// the TV). Interact again - or holding Move for more than standUpMoveHold seconds -
    /// eases the camera back to the exact pre-sit pose and restores normal control.
    /// The player capsule itself never moves; only the camera travels.
    /// </summary>
    public sealed class SitSpot : Interactable
    {
        /// <summary>The seat currently occupied, if any. M2 has exactly one.</summary>
        public static SitSpot Active { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Active = null; // safety for disabled domain reload
        }

        [Header("Wiring (set by GrayboxRoomBuilder)")]
        [Tooltip("Seated eye pose: position + base rotation facing the TV.")]
        public Transform seatAnchor;

        [Header("Dials")]
        public float transitionDuration = 0.35f;
        public float seatedYawLimit = 60f;
        public float seatedPitchLimit = 40f;
        [Tooltip("Seconds of held Move input that stands the player back up.")]
        public float standUpMoveHold = 0.5f;
        [Tooltip("Move magnitude below this does not count as intent to stand.")]
        public float moveDeadzone = 0.4f;

        private enum State { Idle, SittingDown, Seated, StandingUp }

        private State _state = State.Idle;
        private FirstPersonController _controller;
        private Vector3 _preSitPosition;
        private Quaternion _preSitRotation;
        private float _moveHeldTime;

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
            _preSitRotation = cam.rotation;

            _controller.BeginExternalCameraControl();
            yield return LerpCamera(cam, seatAnchor.position, seatAnchor.rotation);

            _controller.EnterSeated(seatAnchor.rotation, seatedYawLimit, seatedPitchLimit);
            _moveHeldTime = 0f;
            _state = State.Seated;
            Active = this;
        }

        private IEnumerator StandUp()
        {
            _state = State.StandingUp;
            Active = null;
            Transform cam = _controller.cameraTransform;

            _controller.BeginExternalCameraControl();
            yield return LerpCamera(cam, _preSitPosition, _preSitRotation);

            _controller.ExitSeated();
            _state = State.Idle;
        }

        private IEnumerator LerpCamera(Transform cam, Vector3 toPosition, Quaternion toRotation)
        {
            Vector3 fromPosition = cam.position;
            Quaternion fromRotation = cam.rotation;
            float duration = Mathf.Max(0.01f, transitionDuration);
            float t = 0f;

            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / duration);
                float eased = t * t * (3f - 2f * t); // smoothstep
                cam.SetPositionAndRotation(
                    Vector3.Lerp(fromPosition, toPosition, eased),
                    Quaternion.Slerp(fromRotation, toRotation, eased));
                yield return null;
            }
        }
    }
}
