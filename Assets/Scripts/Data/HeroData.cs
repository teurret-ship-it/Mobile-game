using UnityEngine;

namespace SurvivalMoba.Data
{
    /// <summary>
    /// Definition of a playable hero: base stats, basic attack and the four abilities.
    /// Create via Assets > Create > SurvivalMoba > Hero Data.
    /// </summary>
    [CreateAssetMenu(fileName = "HeroData", menuName = "SurvivalMoba/Hero Data", order = 0)]
    public class HeroData : ScriptableObject
    {
        [Header("Identity")]
        public string heroName = "Arcane Hunter";
        [TextArea] public string role = "Ranged survival mage";

        [Header("Base Stats")]
        public float maxHealth = 100f;
        public float moveSpeed = 5f;
        [Tooltip("Multiplier applied to basic-attack cooldown. Higher = faster.")]
        public float attackSpeed = 1f;
        public float baseDamage = 10f;
        public float basicAttackRange = 6f;

        [Header("Basic Attack")]
        public AbilityData basicAttack;

        [Header("Abilities")]
        public AbilityData ability1;
        public AbilityData ability2;
        public AbilityData ability3;
        public AbilityData ultimate;
    }
}
