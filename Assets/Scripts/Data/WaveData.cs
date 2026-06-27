using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalMoba.Data
{
    /// <summary>
    /// A single timed entry in the wave timeline. When the match clock reaches
    /// <see cref="time"/>, the spawner switches to these settings.
    /// </summary>
    [Serializable]
    public class WaveEntry
    {
        [Tooltip("Match time (seconds) at which this configuration becomes active.")]
        public float time;

        [Tooltip("Enemy type to spawn during this phase.")]
        public EnemyData enemy;

        [Tooltip("Enemies spawned per second.")]
        public float spawnRate = 1f;

        [Tooltip("Maximum simultaneously alive enemies for this phase.")]
        public int maxAlive = 50;

        [Tooltip("If set, spawn this single enemy once (used for the boss) and skip rate spawning.")]
        public bool isBossWave = false;
    }

    /// <summary>
    /// The full ordered timeline for a match. Create via
    /// Assets > Create > SurvivalMoba > Wave Data.
    /// </summary>
    [CreateAssetMenu(fileName = "WaveData", menuName = "SurvivalMoba/Wave Data", order = 30)]
    public class WaveData : ScriptableObject
    {
        [Tooltip("Total match length in seconds. Survive this long to win (if no boss kill).")]
        public float matchDuration = 300f;

        [Tooltip("Wave entries, ordered by time ascending.")]
        public List<WaveEntry> entries = new List<WaveEntry>();
    }
}
