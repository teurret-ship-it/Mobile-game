using UnityEngine;
using SurvivalMoba.Core;

namespace SurvivalMoba.Data
{
    /// <summary>
    /// A level-up upgrade choice. Create via
    /// Assets > Create > SurvivalMoba > Upgrade Data.
    /// </summary>
    [CreateAssetMenu(fileName = "UpgradeData", menuName = "SurvivalMoba/Upgrade Data", order = 40)]
    public class UpgradeData : ScriptableObject
    {
        [Header("Identity")]
        public string upgradeId = "power_surge";
        public string upgradeName = "Power Surge";
        [TextArea] public string description = "+10% ability damage";
        public Sprite icon;
        public Rarity rarity = Rarity.Common;

        [Header("Effect")]
        public UpgradeEffectType effectType = UpgradeEffectType.AbilityDamage;

        [Tooltip("Magnitude of the effect. Percentages are expressed as fractions, e.g. 0.10 for +10%.")]
        public float value = 0.10f;

        [Header("Stacking")]
        [Tooltip("How many times this upgrade can be picked. 0 = unlimited.")]
        public int maxStacks = 5;
    }
}
