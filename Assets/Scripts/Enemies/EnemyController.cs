using UnityEngine;
using SurvivalMoba.Core;
using SurvivalMoba.Data;
using SurvivalMoba.Player;
using SurvivalMoba.Systems;
using SurvivalMoba.XP;

namespace SurvivalMoba.Enemies
{
    /// <summary>
    /// Drives a single enemy: chase the player, deal contact damage on a cooldown,
    /// and on death drop an XP orb and return to the pool. Stats come from an
    /// <see cref="EnemyData"/> asset assigned at spawn time.
    /// </summary>
    [RequireComponent(typeof(HealthSystem))]
    public class EnemyController : MonoBehaviour, IPooledObject
    {
        [SerializeField] private EnemyData data;
        [SerializeField] private GameObject xpOrbPrefab;
        [SerializeField] private GameObject deathVfx;

        private HealthSystem _health;
        private Transform _player;
        private HealthSystem _playerHealth;
        private float _attackTimer;
        private bool _dead;

        public bool IsAlive => !_dead && _health != null && _health.IsAlive;
        public EnemyType Type => data != null ? data.enemyType : EnemyType.Grunt;
        public EnemyData Data => data;

        private void Awake()
        {
            _health = GetComponent<HealthSystem>();
            _health.SetFaction(Faction.Enemy);
            _health.Died += OnDied;
        }

        private void OnDestroy()
        {
            if (_health != null) _health.Died -= OnDied;
        }

        /// <summary>Assign data before/at spawn (e.g. from the spawner).</summary>
        public void Configure(EnemyData enemyData, GameObject orbPrefab)
        {
            data = enemyData;
            if (orbPrefab != null) xpOrbPrefab = orbPrefab;
        }

        public void OnSpawnFromPool()
        {
            _dead = false;
            _attackTimer = 0f;

            if (data != null) _health.ResetHealth(data.maxHealth);
            else _health.ResetHealth();

            CachePlayer();
            EnemyRegistry.Register(this);
        }

        public void OnReturnToPool()
        {
            EnemyRegistry.Unregister(this);
        }

        private void CachePlayer()
        {
            if (PlayerCharacter.Instance != null)
            {
                _player = PlayerCharacter.Instance.transform;
                _playerHealth = PlayerCharacter.Instance.Health;
            }
        }

        private void Update()
        {
            if (_dead || data == null) return;
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;
            if (_player == null) { CachePlayer(); if (_player == null) return; }

            Vector3 toPlayer = _player.position - transform.position;
            toPlayer.y = 0f;
            float dist = toPlayer.magnitude;

            // Move toward the player, honouring any active slow.
            if (dist > data.attackRange)
            {
                Vector3 dir = toPlayer / Mathf.Max(dist, 0.0001f);
                float speed = data.moveSpeed * _health.MoveSpeedMultiplier;
                transform.position += dir * (speed * Time.deltaTime);
                transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            }

            // Contact damage on a cooldown when in range.
            if (_attackTimer > 0f) _attackTimer -= Time.deltaTime;
            if (dist <= data.attackRange && _attackTimer <= 0f && _playerHealth != null)
            {
                _playerHealth.TakeDamage(data.contactDamage);
                _attackTimer = data.attackCooldown;
            }
        }

        private void OnDied()
        {
            if (_dead) return;
            _dead = true;

            DropXp();
            SpawnDeathVfx();

            bool wasBoss = data != null && data.IsBoss;
            EnemyRegistry.Unregister(this);
            PoolManager.Despawn(gameObject);

            if (wasBoss && GameManager.Instance != null)
                GameManager.Instance.TriggerVictory();
        }

        private void DropXp()
        {
            if (xpOrbPrefab == null || PoolManager.Instance == null || data == null) return;
            GameObject go = PoolManager.Instance.Spawn(xpOrbPrefab, transform.position, Quaternion.identity);
            XPOrb orb = go != null ? go.GetComponent<XPOrb>() : null;
            if (orb != null) orb.SetValue(data.xpReward);
        }

        private void SpawnDeathVfx()
        {
            if (deathVfx == null || PoolManager.Instance == null) return;
            GameObject fx = PoolManager.Instance.Spawn(deathVfx, transform.position, Quaternion.identity);
            if (fx != null) PoolManager.Instance.DespawnAfter(fx, 1.5f);
        }
    }
}
