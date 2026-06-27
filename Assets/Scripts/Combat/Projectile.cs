using UnityEngine;
using SurvivalMoba.Core;
using SurvivalMoba.Systems;

namespace SurvivalMoba.Combat
{
    /// <summary>
    /// Pooled, straight-flying projectile used by the basic attack and projectile
    /// abilities. Travels along its forward vector, damages targets of the opposing
    /// faction, supports pierce, and returns itself to the pool on hit or timeout.
    ///
    /// Requires a trigger Collider on the prefab. Targets must expose an
    /// <see cref="IDamageable"/> on themselves or a parent.
    /// </summary>
    public class Projectile : MonoBehaviour, IPooledObject
    {
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private GameObject hitVfx;

        private float _speed;
        private float _damage;
        private Faction _ownerFaction;
        private int _pierceRemaining;
        private float _slowFraction;
        private float _slowDuration;
        private float _despawnAt;

        /// <summary>
        /// Configure a freshly spawned projectile.
        /// </summary>
        public void Launch(Vector3 direction, float speed, float damage, Faction ownerFaction,
            int pierce = 0, float slowFraction = 0f, float slowDuration = 0f)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

            _speed = speed;
            _damage = damage;
            _ownerFaction = ownerFaction;
            _pierceRemaining = pierce;
            _slowFraction = slowFraction;
            _slowDuration = slowDuration;
            _despawnAt = Time.time + lifetime;
        }

        public void OnSpawnFromPool() { }
        public void OnReturnToPool() { }

        private void Update()
        {
            transform.position += transform.forward * (_speed * Time.deltaTime);
            if (Time.time >= _despawnAt) Despawn();
        }

        private void OnTriggerEnter(Collider other)
        {
            // Resolve the damageable on the collider or a parent (rigidbody root).
            IDamageable target = other.GetComponentInParent<IDamageable>();
            if (target == null || target.Faction == _ownerFaction || !target.IsAlive) return;

            target.TakeDamage(_damage);
            if (_slowFraction > 0f) target.ApplySlow(_slowFraction, _slowDuration);

            SpawnHitVfx();

            if (_pierceRemaining <= 0)
            {
                Despawn();
            }
            else
            {
                _pierceRemaining--;
            }
        }

        private void SpawnHitVfx()
        {
            if (hitVfx == null) return;
            if (PoolManager.Instance != null)
            {
                GameObject fx = PoolManager.Instance.Spawn(hitVfx, transform.position, Quaternion.identity);
                if (fx != null) PoolManager.Instance.DespawnAfter(fx, 1f);
            }
        }

        private void Despawn() => PoolManager.Despawn(gameObject);
    }
}
