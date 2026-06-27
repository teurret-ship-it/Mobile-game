using UnityEngine;
using SurvivalMoba.Core;
using SurvivalMoba.Data;
using SurvivalMoba.Systems;

namespace SurvivalMoba.Player
{
    /// <summary>
    /// Central hero component. Builds the runtime <see cref="PlayerStats"/> from a
    /// <see cref="HeroData"/> asset, owns the <see cref="HealthSystem"/> and acts as
    /// the access point other systems use to reach the hero (stats, position, death).
    /// </summary>
    [RequireComponent(typeof(HealthSystem))]
    public class PlayerCharacter : MonoBehaviour
    {
        public static PlayerCharacter Instance { get; private set; }

        [SerializeField] private HeroData heroData;

        private HealthSystem _health;

        public PlayerStats Stats { get; private set; }
        public HeroData Hero => heroData;
        public HealthSystem Health => _health;
        public Vector3 Position => transform.position;

        private void Awake()
        {
            Instance = this;

            _health = GetComponent<HealthSystem>();
            _health.SetFaction(Faction.Player);

            Stats = new PlayerStats(heroData);
            _health.SetMaxHealth(Stats.MaxHealth, refill: true);

            // Keep max health in sync when Vitality-style upgrades are taken.
            Stats.StatsChanged += OnStatsChanged;
            _health.Died += OnDied;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (Stats != null) Stats.StatsChanged -= OnStatsChanged;
            if (_health != null) _health.Died -= OnDied;
        }

        private void Start()
        {
            if (GameManager.Instance != null) GameManager.Instance.SetPlayer(transform);
        }

        private void OnStatsChanged()
        {
            // Preserve current health when raising the maximum.
            _health.SetMaxHealth(Stats.MaxHealth, refill: false);
        }

        private void OnDied()
        {
            if (GameManager.Instance != null) GameManager.Instance.OnPlayerDied();
        }
    }
}
