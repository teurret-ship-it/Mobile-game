using UnityEngine;
using SurvivalMoba.Core;

namespace SurvivalMoba.Data
{
    /// <summary>
    /// Data-driven definition of an enemy. Create assets via
    /// Assets > Create > SurvivalMoba > Enemy Data.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData", menuName = "SurvivalMoba/Enemy Data", order = 10)]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        public string enemyName = "Grunt";
        public EnemyType enemyType = EnemyType.Grunt;

        [Header("Stats")]
        public float maxHealth = 20f;
        public float contactDamage = 5f;
        public float moveSpeed = 2f;

        [Tooltip("Distance at which the enemy can deal contact damage.")]
        public float attackRange = 1.2f;

        [Tooltip("Seconds between contact-damage ticks.")]
        public float attackCooldown = 1f;

        [Header("Rewards")]
        public int xpReward = 1;

        [Header("Visuals")]
        [Tooltip("Prefab spawned for this enemy. Must contain an EnemyController.")]
        public GameObject prefab;

        public bool IsBoss => enemyType == EnemyType.Boss;
    }
}
