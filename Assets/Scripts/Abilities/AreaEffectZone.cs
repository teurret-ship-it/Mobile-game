using System.Collections.Generic;
using UnityEngine;
using SurvivalMoba.Core;
using SurvivalMoba.Enemies;
using SurvivalMoba.Systems;

namespace SurvivalMoba.Abilities
{
    /// <summary>
    /// Pooled persistent ground effect (e.g. Frost Zone). For its lifetime it
    /// periodically damages and optionally slows every enemy inside its radius,
    /// then returns itself to the pool.
    /// </summary>
    public class AreaEffectZone : MonoBehaviour, IPooledObject
    {
        [SerializeField] private Transform visual;

        private float _radius;
        private float _damagePerTick;
        private float _tickInterval;
        private float _slowFraction;
        private float _slowDuration;
        private float _expireAt;
        private float _nextTick;

        private readonly List<EnemyController> _hits = new List<EnemyController>();

        /// <summary>
        /// Configure and start the zone. <paramref name="ticksPerSecond"/> controls how
        /// often damage is applied; total damage = damagePerTick * duration * ticksPerSecond.
        /// </summary>
        public void Configure(float radius, float damagePerTick, float ticksPerSecond,
            float duration, float slowFraction, float slowDuration)
        {
            _radius = radius;
            _damagePerTick = damagePerTick;
            _tickInterval = ticksPerSecond > 0f ? 1f / ticksPerSecond : 1f;
            _slowFraction = slowFraction;
            _slowDuration = slowDuration;

            _expireAt = Time.time + duration;
            _nextTick = Time.time; // first tick immediately

            if (visual != null)
            {
                // Scale a unit-diameter visual to match the radius.
                visual.localScale = new Vector3(_radius * 2f, visual.localScale.y, _radius * 2f);
            }
        }

        public void OnSpawnFromPool() { }
        public void OnReturnToPool() { }

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

            if (Time.time >= _expireAt)
            {
                PoolManager.Despawn(gameObject);
                return;
            }

            if (Time.time >= _nextTick)
            {
                _nextTick += _tickInterval;
                ApplyTick();
            }
        }

        private void ApplyTick()
        {
            EnemyRegistry.QueryRadius(transform.position, _radius, _hits);
            for (int i = 0; i < _hits.Count; i++)
            {
                EnemyController e = _hits[i];
                if (e == null || !e.IsAlive) continue;

                HealthSystem hp = e.GetComponent<HealthSystem>();
                if (hp == null) continue;

                hp.TakeDamage(_damagePerTick);
                if (_slowFraction > 0f) hp.ApplySlow(_slowFraction, _slowDuration);
            }
        }
    }
}
