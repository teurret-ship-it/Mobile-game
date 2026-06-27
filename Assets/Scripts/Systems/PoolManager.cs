using System.Collections.Generic;
using UnityEngine;

namespace SurvivalMoba.Systems
{
    /// <summary>
    /// Central registry that lazily creates one <see cref="ObjectPool"/> per prefab.
    /// Lets any system spawn a pooled prefab without holding a direct pool reference.
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        [Tooltip("Default number of instances to pre-warm per pool.")]
        [SerializeField] private int defaultPrewarm = 16;

        private readonly Dictionary<GameObject, ObjectPool> _pools = new Dictionary<GameObject, ObjectPool>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private ObjectPool GetOrCreatePool(GameObject prefab, int prewarm)
        {
            if (prefab == null) return null;

            if (!_pools.TryGetValue(prefab, out ObjectPool pool) || pool == null)
            {
                var holder = new GameObject($"Pool_{prefab.name}");
                holder.transform.SetParent(transform, false);
                pool = holder.AddComponent<ObjectPool>();
                pool.Configure(prefab, prewarm);
                _pools[prefab] = pool;
            }

            return pool;
        }

        /// <summary>Spawn a pooled instance of <paramref name="prefab"/>.</summary>
        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, int prewarm = -1)
        {
            ObjectPool pool = GetOrCreatePool(prefab, prewarm < 0 ? defaultPrewarm : prewarm);
            return pool != null ? pool.Spawn(position, rotation) : null;
        }

        /// <summary>Return a pooled instance. Falls back to deactivation if it has no pool.</summary>
        public static void Despawn(GameObject instance)
        {
            if (instance == null) return;
            var marker = instance.GetComponent<PooledInstance>();
            if (marker != null) marker.ReturnToPool();
            else instance.SetActive(false);
        }

        /// <summary>Return a pooled instance after a delay (handy for timed VFX).</summary>
        public void DespawnAfter(GameObject instance, float delay)
        {
            if (instance != null) StartCoroutine(DespawnRoutine(instance, delay));
        }

        private System.Collections.IEnumerator DespawnRoutine(GameObject instance, float delay)
        {
            yield return new WaitForSeconds(delay);
            Despawn(instance);
        }
    }
}
