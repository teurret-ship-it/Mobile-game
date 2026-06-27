using UnityEngine;

namespace SurvivalMoba.Player
{
    /// <summary>
    /// Smoothly follows the hero while keeping the fixed isometric angle.
    /// Place the camera at the desired iso angle in the editor (e.g. rotation 45,45,0,
    /// orthographic) and assign the target; this script only translates, never rotates.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;

        [Tooltip("Offset from the target, in world space. Captured from the start position if zero.")]
        [SerializeField] private Vector3 offset = Vector3.zero;

        [Tooltip("Follow smoothing time. 0 = snap instantly.")]
        [SerializeField] private float smoothTime = 0.15f;

        private Vector3 _velocity;

        private void Start()
        {
            if (target == null && PlayerCharacter.Instance != null)
                target = PlayerCharacter.Instance.transform;

            // If no explicit offset was set, derive it from the current placement so the
            // designer's framing in the editor is preserved.
            if (offset == Vector3.zero && target != null)
                offset = transform.position - target.position;
        }

        public void SetTarget(Transform t) => target = t;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = target.position + offset;
            transform.position = smoothTime <= 0f
                ? desired
                : Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);
        }
    }
}
