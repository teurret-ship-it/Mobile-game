using UnityEngine;
using UnityEngine.EventSystems;

namespace SurvivalMoba.InputControl
{
    /// <summary>
    /// Virtual on-screen joystick for movement. Attach to a UI Image (the background)
    /// that has a child "handle" Image. Exposes a normalised <see cref="Direction"/>
    /// (X = screen-right, Y = screen-up) that the player controller maps to world space.
    ///
    /// Works as a fixed joystick by default; enable <see cref="dynamicOrigin"/> to let
    /// the joystick recenter under the first touch within its rect.
    /// </summary>
    public class MobileJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;

        [Tooltip("Max handle travel as a fraction of the background radius.")]
        [SerializeField, Range(0.5f, 2f)] private float handleRange = 1f;

        [Tooltip("Inputs below this magnitude are treated as zero (dead zone).")]
        [SerializeField, Range(0f, 0.4f)] private float deadZone = 0.1f;

        [Tooltip("Recenter the joystick under the touch position when pressed.")]
        [SerializeField] private bool dynamicOrigin = false;

        private Canvas _canvas;
        private Camera _cam;
        private Vector2 _input = Vector2.zero;

        /// <summary>Normalised joystick vector. Magnitude 0..1.</summary>
        public Vector2 Direction => _input;
        public bool IsPressed => _input.sqrMagnitude > 0f;

        private void Awake()
        {
            if (background == null) background = GetComponent<RectTransform>();
            if (handle == null && transform.childCount > 0)
                handle = transform.GetChild(0) as RectTransform;

            _canvas = GetComponentInParent<Canvas>();
            if (_canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                _cam = _canvas.worldCamera;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (dynamicOrigin)
            {
                background.position = eventData.position;
            }
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 radius = background.sizeDelta * 0.5f;
            // Convert the touch into joystick-local space, normalised by the background radius.
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, _cam, out Vector2 local);

            Vector2 normalized = new Vector2(
                radius.x != 0f ? local.x / radius.x : 0f,
                radius.y != 0f ? local.y / radius.y : 0f);

            // Clamp to the unit circle.
            if (normalized.magnitude > 1f) normalized.Normalize();

            // Apply dead zone.
            _input = normalized.magnitude < deadZone ? Vector2.zero : normalized;

            if (handle != null)
            {
                handle.anchoredPosition = _input * radius * handleRange;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _input = Vector2.zero;
            if (handle != null) handle.anchoredPosition = Vector2.zero;
        }
    }
}
