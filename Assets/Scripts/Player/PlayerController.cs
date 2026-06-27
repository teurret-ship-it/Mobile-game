using UnityEngine;
using SurvivalMoba.Core;
using SurvivalMoba.InputControl;
using SurvivalMoba.Systems;

namespace SurvivalMoba.Player
{
    /// <summary>
    /// Moves the hero on the X/Z plane from joystick input, converted to be
    /// camera-relative so "up" on the stick is always "away" on the isometric floor.
    /// Uses a CharacterController if present, otherwise moves the transform directly.
    /// </summary>
    [RequireComponent(typeof(PlayerCharacter))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private MobileJoystick joystick;
        [SerializeField] private Transform cameraTransform;

        [Tooltip("How quickly the hero rotates to face the movement direction.")]
        [SerializeField] private float turnSpeed = 720f;

        [Tooltip("Allow keyboard WASD in the editor for quick testing.")]
        [SerializeField] private bool editorKeyboard = true;

        private PlayerCharacter _character;
        private HealthSystem _health;
        private CharacterController _cc;

        // Camera-relative basis vectors projected onto the floor plane.
        private Vector3 _camForward = Vector3.forward;
        private Vector3 _camRight = Vector3.right;

        private void Awake()
        {
            _character = GetComponent<PlayerCharacter>();
            _health = GetComponent<HealthSystem>();
            _cc = GetComponent<CharacterController>();

            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;

            CacheCameraBasis();
        }

        public void SetJoystick(MobileJoystick j) => joystick = j;

        private void CacheCameraBasis()
        {
            if (cameraTransform == null) return;

            // Flatten the camera's forward/right onto the X/Z plane.
            _camForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            if (_camForward.sqrMagnitude < 0.001f) _camForward = Vector3.forward;
            _camRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
        }

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

            Vector2 input = ReadInput();
            if (input.sqrMagnitude > 1f) input.Normalize();

            // Map screen-space stick input to camera-relative world direction.
            Vector3 worldDir = _camRight * input.x + _camForward * input.y;

            float speed = _character.Stats != null ? _character.Stats.MoveSpeed : 5f;
            speed *= _health.MoveSpeedMultiplier; // apply active slows

            Vector3 motion = worldDir * speed;

            if (_cc != null && _cc.enabled)
            {
                // Apply a little gravity so the controller stays grounded.
                motion.y = -9.81f;
                _cc.Move(motion * Time.deltaTime);
            }
            else
            {
                transform.position += new Vector3(motion.x, 0f, motion.z) * Time.deltaTime;
            }

            if (worldDir.sqrMagnitude > 0.001f)
            {
                Quaternion target = Quaternion.LookRotation(worldDir, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, target, turnSpeed * Time.deltaTime);
            }
        }

        private Vector2 ReadInput()
        {
            Vector2 input = joystick != null ? joystick.Direction : Vector2.zero;

#if UNITY_EDITOR
            if (editorKeyboard && input == Vector2.zero)
            {
                input = new Vector2(
                    UnityEngine.Input.GetAxisRaw("Horizontal"),
                    UnityEngine.Input.GetAxisRaw("Vertical"));
            }
#endif
            return input;
        }
    }
}
