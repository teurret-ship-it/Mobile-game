using UnityEngine;
using SurvivalMoba.Systems;

namespace SurvivalMoba.UI
{
    /// <summary>
    /// Optional global service that spawns floating damage numbers. If present in the
    /// scene, <see cref="HealthSystem"/> routes damage through it; if absent, damage
    /// numbers are simply skipped. Toggle with the settings "damage numbers" option.
    /// </summary>
    public class DamageNumberService : MonoBehaviour
    {
        public static DamageNumberService Instance { get; private set; }

        [SerializeField] private GameObject numberPrefab;
        [SerializeField] private bool enabledByDefault = true;

        public bool ShowNumbers { get; set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ShowNumbers = enabledByDefault;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Show(Vector3 worldPos, float amount)
        {
            if (!ShowNumbers || numberPrefab == null || PoolManager.Instance == null) return;

            GameObject go = PoolManager.Instance.Spawn(numberPrefab, worldPos + Vector3.up, Quaternion.identity);
            FloatingDamageNumber num = go != null ? go.GetComponent<FloatingDamageNumber>() : null;
            if (num != null) num.Show(amount);
        }
    }
}
