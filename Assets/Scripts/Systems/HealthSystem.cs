using System;
using UnityEngine;
using SurvivalMoba.Core;

namespace SurvivalMoba.Systems
{
    /// <summary>
    /// Reusable health container for the player and all enemies. Implements
    /// <see cref="IDamageable"/> so projectiles and abilities can damage it
    /// without knowing the concrete owner.
    /// </summary>
    public class HealthSystem : MonoBehaviour, IDamageable
    {
        [SerializeField] private Faction faction = Faction.Enemy;
        [SerializeField] private float maxHealth = 20f;

        private float _current;
        private float _slowFraction;
        private float _slowUntil;

        /// <summary>Fired with (current, max) whenever health changes.</summary>
        public event Action<float, float> HealthChanged;

        /// <summary>Fired once when health reaches zero.</summary>
        public event Action Died;

        public Faction Faction => faction;
        public bool IsAlive => _current > 0f;
        public Vector3 Position => transform.position;
        public float Current => _current;
        public float Max => maxHealth;
        public float Normalized => maxHealth > 0f ? _current / maxHealth : 0f;

        /// <summary>Current movement multiplier from active slows (1 = no slow).</summary>
        public float MoveSpeedMultiplier =>
            Time.time < _slowUntil ? Mathf.Clamp01(1f - _slowFraction) : 1f;

        private void Awake()
        {
            _current = maxHealth;
        }

        /// <summary>Reset to full health, optionally with a new maximum (used on pool reuse).</summary>
        public void ResetHealth(float? newMax = null)
        {
            if (newMax.HasValue) maxHealth = newMax.Value;
            _current = maxHealth;
            _slowUntil = 0f;
            _slowFraction = 0f;
            HealthChanged?.Invoke(_current, maxHealth);
        }

        public void SetFaction(Faction f) => faction = f;

        public void SetMaxHealth(float value, bool refill)
        {
            maxHealth = Mathf.Max(1f, value);
            if (refill) _current = maxHealth;
            else _current = Mathf.Min(_current, maxHealth);
            HealthChanged?.Invoke(_current, maxHealth);
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive || amount <= 0f) return;

            _current = Mathf.Max(0f, _current - amount);
            HealthChanged?.Invoke(_current, maxHealth);

            // Floating damage numbers (enemies only, and only if the service is present).
            if (faction == Faction.Enemy && UI.DamageNumberService.Instance != null)
                UI.DamageNumberService.Instance.Show(transform.position, amount);

            if (_current <= 0f) Died?.Invoke();
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f) return;
            _current = Mathf.Min(maxHealth, _current + amount);
            HealthChanged?.Invoke(_current, maxHealth);
        }

        public void ApplySlow(float slowFraction, float duration)
        {
            // Keep the strongest currently-active slow.
            if (slowFraction >= _slowFraction || Time.time >= _slowUntil)
            {
                _slowFraction = Mathf.Clamp01(slowFraction);
            }
            _slowUntil = Mathf.Max(_slowUntil, Time.time + duration);
        }
    }
}
