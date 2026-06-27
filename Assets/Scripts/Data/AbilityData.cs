using UnityEngine;
using SurvivalMoba.Core;

namespace SurvivalMoba.Data
{
    /// <summary>
    /// Data-driven ability definition shared by basic attacks, the three abilities
    /// and the ultimate. The <see cref="AbilityEffectType"/> tells the runtime
    /// <c>AbilitySystem</c> how to resolve the cast, so new abilities can be added
    /// as assets without new code in most cases.
    ///
    /// Create via Assets > Create > SurvivalMoba > Ability Data.
    /// </summary>
    [CreateAssetMenu(fileName = "AbilityData", menuName = "SurvivalMoba/Ability Data", order = 20)]
    public class AbilityData : ScriptableObject
    {
        [Header("Identity")]
        public string abilityId = "ability_1";
        public string abilityName = "Piercing Beam";
        [TextArea] public string description = "Pierces all enemies in a straight line.";
        public Sprite icon;

        [Header("Targeting")]
        public TargetingType targeting = TargetingType.LineSkillshot;
        public AbilityEffectType effect = AbilityEffectType.PiercingLine;

        [Header("Timing")]
        public float cooldown = 5f;
        [Tooltip("How long an over-time / placed effect lasts.")]
        public float duration = 0f;

        [Header("Damage")]
        [Tooltip("Direct hit damage, or damage-per-tick for over-time effects.")]
        public float damage = 30f;
        [Tooltip("For over-time effects: how many damage ticks per second.")]
        public float ticksPerSecond = 1f;

        [Header("Geometry")]
        [Tooltip("Cast range / line length / dash distance depending on effect.")]
        public float range = 8f;
        [Tooltip("Area radius for placement and meteor abilities, or line half-width.")]
        public float radius = 1f;
        public float projectileSpeed = 14f;

        [Header("Crowd Control")]
        [Range(0f, 1f)] public float slowFraction = 0f;
        public float slowDuration = 0f;

        [Header("Meteor / Multi-hit")]
        [Tooltip("Number of meteors / projectiles spawned by multi-hit abilities.")]
        public int hitCount = 8;

        [Header("Feedback")]
        public GameObject vfxPrefab;
        public AudioClip sfxClip;

        /// <summary>True when the player must drag to aim before the ability fires.</summary>
        public bool RequiresAiming =>
            targeting == TargetingType.LineSkillshot ||
            targeting == TargetingType.ConeSkillshot ||
            targeting == TargetingType.AreaPlacement ||
            targeting == TargetingType.Dash;
    }
}
