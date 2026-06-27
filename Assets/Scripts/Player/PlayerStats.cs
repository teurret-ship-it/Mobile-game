using System;
using SurvivalMoba.Core;
using SurvivalMoba.Data;

namespace SurvivalMoba.Player
{
    /// <summary>
    /// Mutable runtime stat block for the hero. Initialised from <see cref="HeroData"/>
    /// and then modified by upgrades during a run. Other systems read the computed
    /// properties so they always see the up-to-date, upgrade-adjusted value.
    /// </summary>
    public class PlayerStats
    {
        private readonly HeroData _hero;

        // --- Upgrade accumulators (additive fractions unless noted) ---
        public float AbilityDamageBonus { get; private set; }       // +0.10 == +10%
        public float AttackSpeedBonus { get; private set; }
        public float MoveSpeedBonus { get; private set; }
        public float FlatMaxHealthBonus { get; private set; }
        public float CooldownReduction { get; private set; }        // global, 0..~0.6
        public float DashCooldownReduction { get; private set; }
        public float UltimateCooldownReduction { get; private set; }
        public int ExtraProjectiles { get; private set; }
        public int ExtraPierce { get; private set; }
        public float FrostRadiusBonus { get; private set; }

        /// <summary>Raised whenever an upgrade changes the stat block.</summary>
        public event Action StatsChanged;

        public PlayerStats(HeroData hero)
        {
            _hero = hero;
        }

        public HeroData Hero => _hero;

        // --- Computed effective stats ---
        public float MaxHealth => _hero.maxHealth + FlatMaxHealthBonus;
        public float MoveSpeed => _hero.moveSpeed * (1f + MoveSpeedBonus);

        /// <summary>Effective basic-attack cooldown after attack speed and CDR.</summary>
        public float BasicAttackCooldown
        {
            get
            {
                float attackSpeed = _hero.attackSpeed * (1f + AttackSpeedBonus);
                float baseCd = _hero.basicAttack != null ? _hero.basicAttack.cooldown : 0.8f;
                return baseCd / UnityEngine.Mathf.Max(0.01f, attackSpeed);
            }
        }

        public float BasicAttackDamage =>
            (_hero.basicAttack != null ? _hero.basicAttack.damage : _hero.baseDamage) * (1f + AbilityDamageBonus);

        public float BasicAttackRange => _hero.basicAttackRange;

        /// <summary>Damage multiplier applied to all ability damage.</summary>
        public float AbilityDamageMultiplier => 1f + AbilityDamageBonus;

        /// <summary>Total cooldown multiplier for a generic ability (lower is faster).</summary>
        public float CooldownMultiplier => UnityEngine.Mathf.Clamp(1f - CooldownReduction, 0.2f, 1f);

        /// <summary>Total basic-attack projectile count (1 base + extras).</summary>
        public int BasicProjectileCount => 1 + ExtraProjectiles;

        /// <summary>Total enemies a basic projectile can pierce (0 base + extras).</summary>
        public int BasicPierceCount => ExtraPierce;

        /// <summary>
        /// Apply a single upgrade pick to the stat block and notify listeners.
        /// </summary>
        public void ApplyUpgrade(UpgradeData upgrade)
        {
            if (upgrade == null) return;

            switch (upgrade.effectType)
            {
                case UpgradeEffectType.AbilityDamage: AbilityDamageBonus += upgrade.value; break;
                case UpgradeEffectType.AttackSpeed: AttackSpeedBonus += upgrade.value; break;
                case UpgradeEffectType.MoveSpeed: MoveSpeedBonus += upgrade.value; break;
                case UpgradeEffectType.MaxHealth: FlatMaxHealthBonus += upgrade.value; break;
                case UpgradeEffectType.CooldownReduction: CooldownReduction += upgrade.value; break;
                case UpgradeEffectType.ExtraProjectile: ExtraProjectiles += UnityEngine.Mathf.RoundToInt(upgrade.value); break;
                case UpgradeEffectType.ProjectilePierce: ExtraPierce += UnityEngine.Mathf.RoundToInt(upgrade.value); break;
                case UpgradeEffectType.FrostRadius: FrostRadiusBonus += upgrade.value; break;
                case UpgradeEffectType.DashCooldown: DashCooldownReduction += upgrade.value; break;
                case UpgradeEffectType.UltimateCooldown: UltimateCooldownReduction += upgrade.value; break;
            }

            StatsChanged?.Invoke();
        }

        /// <summary>Returns the cooldown multiplier for a specific ability slot.</summary>
        public float GetAbilityCooldownMultiplier(int slotIndex)
        {
            // slotIndex: 0..2 = abilities, 3 = ultimate. Slot 2 is the dash in the MVP hero.
            float reduction = CooldownReduction;
            if (slotIndex == 2) reduction += DashCooldownReduction;
            if (slotIndex == 3) reduction += UltimateCooldownReduction;
            return UnityEngine.Mathf.Clamp(1f - reduction, 0.2f, 1f);
        }
    }
}
