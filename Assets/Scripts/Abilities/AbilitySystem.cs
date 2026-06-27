using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalMoba.Core;
using SurvivalMoba.Combat;
using SurvivalMoba.Data;
using SurvivalMoba.Enemies;
using SurvivalMoba.Player;
using SurvivalMoba.Systems;

namespace SurvivalMoba.Abilities
{
    /// <summary>
    /// Owns the hero's four ability slots (3 abilities + ultimate), the aiming flow
    /// and the resolution of each <see cref="AbilityEffectType"/>. UI ability buttons
    /// drive it via <see cref="OnButtonDown"/>/<see cref="OnButtonDrag"/>/<see cref="OnButtonUp"/>.
    ///
    /// Convention: each <see cref="AbilityData.vfxPrefab"/> doubles as the spawned object —
    /// a <see cref="Projectile"/> for projectile/line abilities, an <see cref="AreaEffectZone"/>
    /// for area-over-time abilities, and an impact VFX for the meteor ultimate.
    /// </summary>
    public class AbilitySystem : MonoBehaviour
    {
        [SerializeField] private PlayerCharacter character;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Transform castOrigin;
        [SerializeField] private AimIndicator aimIndicator;

        private readonly AbilityRuntime[] _slots = new AbilityRuntime[4];

        private int _activeSlot = -1;
        private bool _hasAim;
        private Vector3 _aimDir = Vector3.forward;

        private Vector3 _camForward = Vector3.forward;
        private Vector3 _camRight = Vector3.right;

        private readonly List<EnemyController> _scratch = new List<EnemyController>();

        public AbilityRuntime GetRuntime(int slot) =>
            (slot >= 0 && slot < _slots.Length) ? _slots[slot] : null;

        private void Awake()
        {
            if (character == null) character = GetComponentInParent<PlayerCharacter>();
            if (castOrigin == null) castOrigin = transform;
            if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;

            CacheCameraBasis();
        }

        private void Start()
        {
            HeroData hero = character != null ? character.Hero : null;
            if (hero != null)
            {
                _slots[0] = new AbilityRuntime(hero.ability1, 0);
                _slots[1] = new AbilityRuntime(hero.ability2, 1);
                _slots[2] = new AbilityRuntime(hero.ability3, 2);
                _slots[3] = new AbilityRuntime(hero.ultimate, 3);
            }
        }

        private void CacheCameraBasis()
        {
            if (cameraTransform == null) return;
            _camForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            if (_camForward.sqrMagnitude < 0.001f) _camForward = Vector3.forward;
            _camRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
        }

        private void Update()
        {
            for (int i = 0; i < _slots.Length; i++)
                _slots[i]?.Tick(Time.deltaTime);
        }

        // --- Input from ability buttons ---

        public void OnButtonDown(int slot)
        {
            AbilityRuntime rt = GetRuntime(slot);
            if (rt == null || rt.Data == null || !rt.IsReady) return;
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

            _activeSlot = slot;
            _hasAim = false;
            _aimDir = AutoAimDirection(rt.Data);

            if (rt.Data.RequiresAiming && aimIndicator != null)
                aimIndicator.Show(rt.Data, castOrigin.position, _aimDir);
        }

        public void OnButtonDrag(int slot, Vector2 screenDelta)
        {
            if (_activeSlot != slot) return;
            AbilityRuntime rt = GetRuntime(slot);
            if (rt == null || rt.Data == null || !rt.Data.RequiresAiming) return;

            // Ignore tiny drags so a quick tap still counts as auto-aim.
            if (screenDelta.sqrMagnitude < 100f) return;

            _aimDir = ScreenToWorldDir(screenDelta);
            _hasAim = true;
            if (aimIndicator != null) aimIndicator.Show(rt.Data, castOrigin.position, _aimDir);
        }

        public void OnButtonUp(int slot)
        {
            if (_activeSlot != slot) return;

            AbilityRuntime rt = GetRuntime(slot);
            if (aimIndicator != null) aimIndicator.Hide();
            _activeSlot = -1;

            if (rt == null || rt.Data == null || !rt.IsReady) return;

            // If the player never dragged, fall back to auto-aim.
            Vector3 dir = _hasAim ? _aimDir : AutoAimDirection(rt.Data);
            Cast(rt, dir);
        }

        // --- Aiming helpers ---

        private Vector3 ScreenToWorldDir(Vector2 screenDelta)
        {
            Vector3 world = _camRight * screenDelta.x + _camForward * screenDelta.y;
            world.y = 0f;
            return world.sqrMagnitude > 0.0001f ? world.normalized : _aimDir;
        }

        private Vector3 AutoAimDirection(AbilityData data)
        {
            EnemyController nearest = EnemyRegistry.FindNearest(castOrigin.position, Mathf.Max(data.range, 8f));
            if (nearest != null)
            {
                Vector3 d = nearest.transform.position - castOrigin.position;
                d.y = 0f;
                if (d.sqrMagnitude > 0.0001f) return d.normalized;
            }
            Vector3 fwd = character != null ? character.transform.forward : transform.forward;
            fwd.y = 0f;
            return fwd.sqrMagnitude > 0.0001f ? fwd.normalized : Vector3.forward;
        }

        // --- Casting / effect resolution ---

        private void Cast(AbilityRuntime rt, Vector3 dir)
        {
            AbilityData data = rt.Data;
            float dmgMul = character != null && character.Stats != null
                ? character.Stats.AbilityDamageMultiplier : 1f;

            // Face the cast direction.
            if (character != null && dir.sqrMagnitude > 0.0001f)
                character.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

            switch (data.effect)
            {
                case AbilityEffectType.Projectile:
                    CastProjectile(data, dir, dmgMul, pierce: 0);
                    break;
                case AbilityEffectType.PiercingLine:
                    CastProjectile(data, dir, dmgMul, pierce: 999);
                    break;
                case AbilityEffectType.AreaOverTime:
                    CastAreaZone(data, dir, dmgMul);
                    break;
                case AbilityEffectType.Dash:
                    CastDash(data, dir, dmgMul);
                    break;
                case AbilityEffectType.MeteorStorm:
                    StartCoroutine(CastMeteorStorm(data, dmgMul));
                    break;
                case AbilityEffectType.SelfBuff:
                    // Reserved for future heroes; no-op for the MVP hero.
                    break;
            }

            PlaySfx(data);
            float cdMul = character != null && character.Stats != null
                ? character.Stats.GetAbilityCooldownMultiplier(rt.Slot) : 1f;
            rt.StartCooldown(cdMul);
        }

        private void CastProjectile(AbilityData data, Vector3 dir, float dmgMul, int pierce)
        {
            if (data.vfxPrefab == null || PoolManager.Instance == null) return;
            GameObject go = PoolManager.Instance.Spawn(
                data.vfxPrefab, castOrigin.position, Quaternion.LookRotation(dir, Vector3.up));
            Projectile proj = go != null ? go.GetComponent<Projectile>() : null;
            if (proj != null)
            {
                proj.Launch(dir, data.projectileSpeed, data.damage * dmgMul, Faction.Player,
                    pierce, data.slowFraction, data.slowDuration);
            }
        }

        private void CastAreaZone(AbilityData data, Vector3 dir, float dmgMul)
        {
            if (data.vfxPrefab == null || PoolManager.Instance == null) return;

            // Placement abilities drop at range along the aim; centred ones drop on the player.
            Vector3 pos = data.targeting == TargetingType.AreaPlacement
                ? castOrigin.position + dir * data.range
                : castOrigin.position;
            pos.y = 0f;

            float radius = data.radius;
            if (character != null && character.Stats != null)
                radius *= 1f + character.Stats.FrostRadiusBonus;

            GameObject go = PoolManager.Instance.Spawn(data.vfxPrefab, pos, Quaternion.identity);
            AreaEffectZone zone = go != null ? go.GetComponent<AreaEffectZone>() : null;
            if (zone != null)
            {
                zone.Configure(radius, data.damage * dmgMul, data.ticksPerSecond,
                    data.duration, data.slowFraction, data.slowDuration);
            }
        }

        private void CastDash(AbilityData data, Vector3 dir, float dmgMul)
        {
            if (character == null) return;

            Vector3 start = character.transform.position;
            Vector3 end = start + dir * data.range;
            end.y = start.y;

            // Damage enemies along the dash corridor.
            EnemyRegistry.QueryRadius(Vector3.Lerp(start, end, 0.5f), data.range, _scratch);
            float halfWidth = Mathf.Max(0.75f, data.radius);
            for (int i = 0; i < _scratch.Count; i++)
            {
                EnemyController e = _scratch[i];
                if (e == null || !e.IsAlive) continue;

                Vector3 toEnemy = e.transform.position - start;
                float along = Vector3.Dot(toEnemy, dir);
                if (along < 0f || along > data.range) continue;
                Vector3 perp = toEnemy - dir * along;
                if (perp.magnitude > halfWidth) continue;

                HealthSystem hp = e.GetComponent<HealthSystem>();
                if (hp != null) hp.TakeDamage(data.damage * dmgMul);
            }

            // Teleport-dash to the end point (CharacterController-safe assignment).
            CharacterController cc = character.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                character.transform.position = end;
                cc.enabled = true;
            }
            else
            {
                character.transform.position = end;
            }

            SpawnTimedVfx(data.vfxPrefab, end, 1f);
        }

        private IEnumerator CastMeteorStorm(AbilityData data, float dmgMul)
        {
            int count = Mathf.Max(1, data.hitCount);
            float interval = data.duration > 0f ? data.duration / count : 0.2f;
            float impactRadius = Mathf.Max(1f, data.radius);

            for (int i = 0; i < count; i++)
            {
                if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) yield break;
                if (character == null) yield break;

                // Random impact point in a ring around the player.
                Vector2 r = Random.insideUnitCircle * data.range;
                Vector3 impact = character.transform.position + new Vector3(r.x, 0f, r.y);

                SpawnTimedVfx(data.vfxPrefab, impact, 1.2f);

                EnemyRegistry.QueryRadius(impact, impactRadius, _scratch);
                for (int j = 0; j < _scratch.Count; j++)
                {
                    EnemyController e = _scratch[j];
                    if (e == null || !e.IsAlive) continue;
                    HealthSystem hp = e.GetComponent<HealthSystem>();
                    if (hp != null) hp.TakeDamage(data.damage * dmgMul);
                }

                yield return new WaitForSeconds(interval);
            }
        }

        private void SpawnTimedVfx(GameObject prefab, Vector3 pos, float life)
        {
            if (prefab == null || PoolManager.Instance == null) return;
            GameObject fx = PoolManager.Instance.Spawn(prefab, pos, Quaternion.identity);
            if (fx != null) PoolManager.Instance.DespawnAfter(fx, life);
        }

        private void PlaySfx(AbilityData data)
        {
            if (data.sfxClip != null)
                AudioSource.PlayClipAtPoint(data.sfxClip, castOrigin.position, 0.8f);
        }
    }
}
