using UnityEngine;
using SurvivalMoba.Core;
using SurvivalMoba.Data;
using SurvivalMoba.Systems;

namespace SurvivalMoba.Enemies
{
    /// <summary>
    /// Spawns the currently-active enemy type in a ring around the player (outside
    /// the visible area), throttled by spawn rate and a max-alive cap. The
    /// <see cref="WaveManager"/> reconfigures it over the course of a match.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Ring")]
        [SerializeField] private float minRadius = 12f;
        [SerializeField] private float maxRadius = 18f;

        [Header("Drops")]
        [Tooltip("XP orb prefab injected into spawned enemies.")]
        [SerializeField] private GameObject xpOrbPrefab;

        [Header("Global Cap")]
        [Tooltip("Hard cap on alive enemies regardless of wave settings (device tuning).")]
        [SerializeField] private int hardMaxAlive = 120;

        private EnemyData _currentEnemy;
        private float _spawnRate = 1f;
        private int _maxAlive = 50;
        private float _accumulator;
        private bool _spawningEnabled = true;

        public void SetWave(EnemyData enemy, float spawnRate, int maxAlive)
        {
            _currentEnemy = enemy;
            _spawnRate = Mathf.Max(0f, spawnRate);
            _maxAlive = maxAlive;
            _accumulator = 0f;
        }

        public void SetSpawningEnabled(bool enabled) => _spawningEnabled = enabled;

        private void Update()
        {
            if (!_spawningEnabled || _currentEnemy == null || _spawnRate <= 0f) return;
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

            int cap = Mathf.Min(_maxAlive, hardMaxAlive);
            if (EnemyRegistry.Count >= cap) return;

            _accumulator += _spawnRate * Time.deltaTime;
            while (_accumulator >= 1f && EnemyRegistry.Count < cap)
            {
                _accumulator -= 1f;
                SpawnOne(_currentEnemy);
            }
        }

        /// <summary>Spawn a single enemy (also used by the boss spawn path).</summary>
        public EnemyController SpawnOne(EnemyData enemy)
        {
            if (enemy == null || enemy.prefab == null || PoolManager.Instance == null) return null;

            Transform player = GameManager.Instance != null ? GameManager.Instance.Player : null;
            Vector3 center = player != null ? player.position : transform.position;

            float angle = Random.value * Mathf.PI * 2f;
            float dist = Random.Range(minRadius, maxRadius);
            Vector3 pos = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * dist;
            pos.y = 0f;

            GameObject go = PoolManager.Instance.Spawn(enemy.prefab, pos, Quaternion.identity);
            EnemyController ctrl = go != null ? go.GetComponent<EnemyController>() : null;
            if (ctrl != null) ctrl.Configure(enemy, xpOrbPrefab);
            return ctrl;
        }
    }
}
