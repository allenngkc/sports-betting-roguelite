using UnityEngine;
using UnityEngine.InputSystem;

namespace SBR.Game
{
    /// <summary>
    /// M2 first-person controller for the graybox room. Small-room tuning: walk only
    /// (no run, no jump), gravity as a stick-to-floor force. Look modes:
    ///   Normal   - yaw on the body, pitch on the camera (clamped +/-85).
    ///   External - a SitSpot transition owns the camera; this script leaves it alone.
    ///   Seated   - look offsets around a fixed seat orientation, clamped per seat.
    /// All wiring fields are public and set by GrayboxRoomBuilder (code-first scene rule).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonController : MonoBehaviour
    {
        public enum LookMode { Normal, External, Seated }

        [Header("Wiring (set by GrayboxRoomBuilder)")]
        public InputActionAsset actions;
        public Transform cameraTransform;

        [Header("Move dials")]
        [Tooltip("Walk speed in m/s. Small room - keep it slow.")]
        public float walkSpeed = 2.2f;
        [Tooltip("Downward acceleration in m/s^2 while airborne.")]
        public float gravity = 15f;
        [Tooltip("Constant downward speed while grounded so the capsule hugs steps/floor.")]
        public float groundedStickSpeed = 2f;

        [Header("Look dials")]
        [Tooltip("Degrees of rotation per mouse-delta unit.")]
        public float lookSensitivity = 0.12f;
        [Tooltip("Pitch clamp in degrees, symmetric.")]
        public float pitchLimit = 85f;

        public LookMode Mode { get; private set; } = LookMode.Normal;
        public bool MovementLocked { get; private set; }
        public Vector2 MoveInput { get; private set; }

        private CharacterController _cc;
        private InputAction _move;
        private InputAction _look;
        private float _yaw;
        private float _pitch;
        private float _verticalVelocity;

        // Seated-mode state.
        private float _seatYaw;
        private float _seatPitch;
        private float _seatYawOffset;
        private float _seatPitchOffset;
        private float _seatYawLimit = 60f;
        private float _seatPitchLimit = 40f;
        private float _seatLookScale = 1f;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _yaw = transform.eulerAngles.y;
        }

        private void OnEnable()
        {
            if (actions != null)
            {
                InputActionMap map = actions.FindActionMap("Player", throwIfNotFound: false);
                if (map != null)
                {
                    _move = map.FindAction("Move");
                    _look = map.FindAction("Look");
                    map.Enable();
                }
            }
            SetCursorLocked(true);
        }

        private void OnDisable()
        {
            SetCursorLocked(false);
        }

        private void Update()
        {
            HandleSystemKeys();

            MoveInput = _move != null ? _move.ReadValue<Vector2>() : Vector2.zero;

            if (Cursor.lockState == CursorLockMode.Locked)
                ApplyLook(_look != null ? _look.ReadValue<Vector2>() : Vector2.zero);

            if (!MovementLocked)
                ApplyMove();
        }

        private void HandleSystemKeys()
        {
            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.escapeKey.wasPressedThisFrame)
                    SetCursorLocked(false);
                if (!Application.isEditor && kb.qKey.wasPressedThisFrame)
                    Application.Quit();
            }

            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame &&
                Cursor.lockState != CursorLockMode.Locked)
            {
                SetCursorLocked(true);
            }
        }

        private void ApplyLook(Vector2 delta)
        {
            float dx = delta.x * lookSensitivity;
            float dy = delta.y * lookSensitivity;

            switch (Mode)
            {
                case LookMode.Normal:
                    _yaw += dx;
                    _pitch = Mathf.Clamp(_pitch - dy, -pitchLimit, pitchLimit);
                    transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
                    if (cameraTransform != null)
                        cameraTransform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
                    break;

                case LookMode.Seated:
                    // Scaled so a zoomed seat keeps a constant screen-space look speed - at a
                    // ~4x zoom, unscaled deltas slam the tight clamp in a single flick.
                    dx *= _seatLookScale;
                    dy *= _seatLookScale;
                    _seatYawOffset = Mathf.Clamp(_seatYawOffset + dx, -_seatYawLimit, _seatYawLimit);
                    _seatPitchOffset = Mathf.Clamp(_seatPitchOffset - dy, -_seatPitchLimit, _seatPitchLimit);
                    if (cameraTransform != null)
                        cameraTransform.rotation = Quaternion.Euler(
                            _seatPitch + _seatPitchOffset, _seatYaw + _seatYawOffset, 0f);
                    break;

                case LookMode.External:
                    break; // a transition owns the camera right now
            }
        }

        private void ApplyMove()
        {
            Vector3 planar = transform.right * MoveInput.x + transform.forward * MoveInput.y;
            if (planar.sqrMagnitude > 1f)
                planar.Normalize();

            if (_cc.isGrounded)
                _verticalVelocity = -groundedStickSpeed;
            else
                _verticalVelocity -= gravity * Time.deltaTime;

            Vector3 velocity = planar * walkSpeed + Vector3.up * _verticalVelocity;
            _cc.Move(velocity * Time.deltaTime);
        }

        /// <summary>Movement off, look frozen; the caller animates the camera.</summary>
        public void BeginExternalCameraControl()
        {
            Mode = LookMode.External;
            MovementLocked = true;
        }

        /// <summary>Look becomes offsets around the seat orientation, clamped. lookScale scales
        /// mouse deltas while seated (pass seatedFov/standingFov so a zoomed seat keeps a constant
        /// screen-space look speed; 1 = unscaled).</summary>
        public void EnterSeated(Quaternion seatRotation, float yawLimit, float pitchLimit,
                                float lookScale = 1f)
        {
            Vector3 e = seatRotation.eulerAngles;
            _seatYaw = e.y;
            _seatPitch = NormalizeAngle(e.x);
            _seatYawOffset = 0f;
            _seatPitchOffset = 0f;
            _seatYawLimit = yawLimit;
            _seatPitchLimit = pitchLimit;
            _seatLookScale = Mathf.Clamp(lookScale, 0.05f, 1f);
            Mode = LookMode.Seated;
            MovementLocked = true;
        }

        /// <summary>Back to normal control. Call after the camera is restored to its rig pose.</summary>
        public void ExitSeated()
        {
            Mode = LookMode.Normal;
            MovementLocked = false;
            // Resync from the restored transforms so there is no snap on the next look frame.
            _yaw = transform.eulerAngles.y;
            _pitch = cameraTransform != null
                ? Mathf.Clamp(NormalizeAngle(cameraTransform.localEulerAngles.x), -pitchLimit, pitchLimit)
                : 0f;
            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
        }

        public void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private static float NormalizeAngle(float degrees)
        {
            return degrees > 180f ? degrees - 360f : degrees;
        }
    }
}
