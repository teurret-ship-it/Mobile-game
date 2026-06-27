using System;
using System.Collections.Generic;
using UnityEngine;
using SurvivalMoba.Core;
using SurvivalMoba.Data;
using SurvivalMoba.Systems;

namespace SurvivalMoba.Enemies
{
    /// <summary>
    /// Walks the <see cref="WaveData"/> timeline against the match clock, reconfiguring
    /// the <see cref="EnemySpawner"/> as phases begin and spawning the boss on its wave.
    /// Exposes the boss's health so the UI can show a boss bar.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [SerializeField] private WaveData waveData;
        [SerializeField] private EnemySpawner spawner;

        private readonly List<WaveEntry> _ordered = new List<WaveEntry>();
        private int _nextIndex;
        private bool _bossSpawned;

        /// <summary>Raised when the boss is spawned, passing its HealthSystem for the UI bar.</summary>
        public event Action<HealthSystem> BossSpawned;

        private void Awake()
        {
            if (spawner == null) spawner = FindObjectOfType<EnemySpawner>();

            if (waveData != null)
            {
                _ordered.AddRange(waveData.entries);
                _ordered.Sort((a, b) => a.time.CompareTo(b.time));

                if (GameManager.Instance != null)
                    GameManager.Instance.SetMatchDuration(waveData.matchDuration);
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

            float elapsed = GameManager.Instance.Elapsed;
            while (_nextIndex < _ordered.Count && elapsed >= _ordered[_nextIndex].time)
            {
                ActivateEntry(_ordered[_nextIndex]);
                _nextIndex++;
            }
        }

        private void ActivateEntry(WaveEntry entry)
        {
            if (entry == null || spawner == null) return;

            if (entry.isBossWave)
            {
                SpawnBoss(entry.enemy);
            }
            else
            {
                spawner.SetWave(entry.enemy, entry.spawnRate, entry.maxAlive);
            }
        }

        private void SpawnBoss(EnemyData bossData)
        {
            if (_bossSpawned || bossData == null) return;
            _bossSpawned = true;

            EnemyController boss = spawner.SpawnOne(bossData);
            if (boss != null)
            {
                HealthSystem hp = boss.GetComponent<HealthSystem>();
                if (hp != null) BossSpawned?.Invoke(hp);
            }
        }
    }
}
