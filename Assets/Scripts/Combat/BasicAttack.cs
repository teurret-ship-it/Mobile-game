using UnityEngine;
using SurvivalMoba.Core;
using SurvivalMoba.Data;
using SurvivalMoba.Enemies;
using SurvivalMoba.Player;
using SurvivalMoba.Systems;

namespace SurvivalMoba.Combat
{
    /// <summary>
    /// Auto-targeting basic attack. On a ready cooldown it finds the nearest enemy
    /// in range, faces it, and fires one or more projectiles (extra projectiles fan
    /// out slightly). Pierce and projectile count come from the player's upgrades.
    /// </summary>
    public class BasicAttack : MonoBehaviour
    {
        [SerializeField] private PlayerCharacter character;
        [SerializeField] private Transform muzzle;

        [Tooltip("Projectile prefab fired by the basic attack. Needs a Projectile component.")]
        [SerializeField] private GameObject projectilePrefab;

        [Tooltip("Angle (degrees) between extra fanned projectiles.")]
        [SerializeField] private float spreadAngle = 12f;

        [Tooltip("Auto-fire while there is a target in range.")]
        [SerializeField] private bool autoFire = true;

        private float _cooldownTimer;

        private void Awake()
        {
            if (character == null) character = GetComponentInParent<PlayerCharacter>();
            if (muzzle == null) muzzle = transform;
        }

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

            if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;
            if (autoFire) TryAttack();
        }

        /// <summary>Public entry point so a manual attack button can also trigger it.</summary>
        public void TryAttack()
        {
            if (_cooldownTimer > 0f) return;

            PlayerStats stats = character != null ? character.Stats : null;
            if (stats == null) return;

            EnemyController target = EnemyRegistry.FindNearest(transform.position, stats.BasicAttackRange);

            Vector3 dir = target != null
                ? (target.transform.position - muzzle.position)
                : transform.forward;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;
            dir.Normalize();

            // Only auto-fire when something is actually in range.
            if (autoFire && target == null) return;

            if (target != null)
            {
                transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            }

            FireSpread(dir, stats);
            _cooldownTimer = stats.BasicAttackCooldown;
        }

        private void FireSpread(Vector3 baseDir, PlayerStats stats)
        {
            int count = Mathf.Max(1, stats.BasicProjectileCount);
            float damage = stats.BasicAttackDamage;
            int pierce = stats.BasicPierceCount;
            float speed = character.Hero.basicAttack != null ? character.Hero.basicAttack.projectileSpeed : 12f;

            // Center the fan around the aim direction.
            float startAngle = -spreadAngle * (count - 1) * 0.5f;
            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + spreadAngle * i;
                Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * baseDir;
                SpawnProjectile(dir, speed, damage, pierce);
            }
        }

        private void SpawnProjectile(Vector3 dir, float speed, float damage, int pierce)
        {
            if (projectilePrefab == null || PoolManager.Instance == null) return;

            GameObject go = PoolManager.Instance.Spawn(
                projectilePrefab, muzzle.position, Quaternion.LookRotation(dir, Vector3.up));
            if (go == null) return;

            Projectile proj = go.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Launch(dir, speed, damage, Faction.Player, pierce);
            }
        }
    }
}
