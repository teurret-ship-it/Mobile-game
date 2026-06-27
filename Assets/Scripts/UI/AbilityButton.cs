using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SurvivalMoba.Abilities;

namespace SurvivalMoba.UI
{
    /// <summary>
    /// One on-screen ability button. Forwards press/drag/release to the
    /// <see cref="AbilitySystem"/> for the configured slot and drives a radial
    /// cooldown overlay (a filled Image) so the player can see readiness.
    /// </summary>
    public class AbilityButton : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Tooltip("0..2 = abilities, 3 = ultimate. Use -1 for the basic attack handled separately.")]
        [SerializeField] private int slot = 0;

        [SerializeField] private AbilitySystem abilitySystem;

        [Header("UI")]
        [SerializeField] private Image iconImage;
        [Tooltip("Image with Type=Filled used as the cooldown sweep overlay.")]
        [SerializeField] private Image cooldownOverlay;
        [SerializeField] private Text cooldownText;

        private Vector2 _downPos;

        private void Awake()
        {
            if (abilitySystem == null) abilitySystem = FindObjectOfType<AbilitySystem>();
        }

        private void Update()
        {
            AbilityRuntime rt = abilitySystem != null ? abilitySystem.GetRuntime(slot) : null;
            if (rt == null) return;

            // Lazily assign the icon from the ability data.
            if (iconImage != null && iconImage.sprite == null && rt.Data != null && rt.Data.icon != null)
                iconImage.sprite = rt.Data.icon;

            float frac = rt.CooldownFraction;
            if (cooldownOverlay != null)
            {
                cooldownOverlay.enabled = frac > 0f;
                cooldownOverlay.fillAmount = frac;
            }
            if (cooldownText != null)
            {
                bool show = frac > 0f;
                cooldownText.enabled = show;
                if (show) cooldownText.text = Mathf.CeilToInt(rt.CooldownRemaining).ToString();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _downPos = eventData.position;
            if (abilitySystem != null) abilitySystem.OnButtonDown(slot);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (abilitySystem != null)
                abilitySystem.OnButtonDrag(slot, eventData.position - _downPos);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (abilitySystem != null) abilitySystem.OnButtonUp(slot);
        }
    }
}
