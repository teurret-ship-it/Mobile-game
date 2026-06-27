using UnityEngine;
using SurvivalMoba.Data;

namespace SurvivalMoba.Abilities
{
    /// <summary>
    /// Runtime wrapper around an <see cref="AbilityData"/> asset that tracks the
    /// live cooldown for one ability slot.
    /// </summary>
    public class AbilityRuntime
    {
        public AbilityData Data { get; }
        public int Slot { get; }

        private float _cooldownRemaining;
        private float _lastFullCooldown;

        public AbilityRuntime(AbilityData data, int slot)
        {
            Data = data;
            Slot = slot;
            _lastFullCooldown = data != null ? data.cooldown : 1f;
        }

        public bool IsReady => _cooldownRemaining <= 0f;

        /// <summary>0..1 fraction of cooldown remaining (1 = just used, 0 = ready).</summary>
        public float CooldownFraction =>
            _lastFullCooldown > 0f ? Mathf.Clamp01(_cooldownRemaining / _lastFullCooldown) : 0f;

        public float CooldownRemaining => Mathf.Max(0f, _cooldownRemaining);

        public void Tick(float deltaTime)
        {
            if (_cooldownRemaining > 0f) _cooldownRemaining -= deltaTime;
        }

        /// <summary>Start the cooldown using a multiplier (e.g. from cooldown reduction).</summary>
        public void StartCooldown(float multiplier)
        {
            _lastFullCooldown = (Data != null ? Data.cooldown : 1f) * Mathf.Max(0.05f, multiplier);
            _cooldownRemaining = _lastFullCooldown;
        }
    }
}
