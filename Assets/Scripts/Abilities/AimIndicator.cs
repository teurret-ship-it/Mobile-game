using UnityEngine;
using SurvivalMoba.Core;
using SurvivalMoba.Data;

namespace SurvivalMoba.Abilities
{
    /// <summary>
    /// Visual skill-shot aiming reticle. Owns two child visuals — a line/arrow for
    /// directional skillshots/dashes and a circle for area placement — and shows the
    /// appropriate one while the player drags an ability button.
    ///
    /// Both child transforms should be authored 1 unit long/wide so they scale cleanly.
    /// </summary>
    public class AimIndicator : MonoBehaviour
    {
        [SerializeField] private Transform lineVisual;   // scales along local Z to show range
        [SerializeField] private Transform circleVisual; // scales on X/Z to show radius

        private void Awake() => Hide();

        public void Hide()
        {
            if (lineVisual != null) lineVisual.gameObject.SetActive(false);
            if (circleVisual != null) circleVisual.gameObject.SetActive(false);
        }

        /// <summary>
        /// Show the reticle for <paramref name="ability"/> from <paramref name="origin"/>
        /// aimed along <paramref name="worldDir"/> (already flattened to X/Z).
        /// </summary>
        public void Show(AbilityData ability, Vector3 origin, Vector3 worldDir)
        {
            if (ability == null) return;

            switch (ability.targeting)
            {
                case TargetingType.LineSkillshot:
                case TargetingType.ConeSkillshot:
                case TargetingType.Dash:
                    ShowLine(origin, worldDir, ability.range);
                    break;
                case TargetingType.AreaPlacement:
                    ShowCircle(origin + worldDir * ability.range, ability.radius);
                    break;
                default:
                    Hide();
                    break;
            }
        }

        private void ShowLine(Vector3 origin, Vector3 dir, float length)
        {
            if (circleVisual != null) circleVisual.gameObject.SetActive(false);
            if (lineVisual == null) return;

            lineVisual.gameObject.SetActive(true);
            lineVisual.position = origin;
            if (dir.sqrMagnitude > 0.0001f)
                lineVisual.rotation = Quaternion.LookRotation(dir, Vector3.up);

            Vector3 s = lineVisual.localScale;
            lineVisual.localScale = new Vector3(s.x, s.y, length);
        }

        private void ShowCircle(Vector3 center, float radius)
        {
            if (lineVisual != null) lineVisual.gameObject.SetActive(false);
            if (circleVisual == null) return;

            circleVisual.gameObject.SetActive(true);
            circleVisual.position = center;
            Vector3 s = circleVisual.localScale;
            circleVisual.localScale = new Vector3(radius * 2f, s.y, radius * 2f);
        }
    }
}
