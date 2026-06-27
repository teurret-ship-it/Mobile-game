using System.Collections.Generic;
using UnityEngine;

namespace SurvivalMoba.Systems
{
    /// <summary>
    /// Lightweight component-based object pool for a single prefab.
    /// Avoids Instantiate/Destroy churn during gameplay (enemies, projectiles, orbs, VFX).
    ///
    /// Pooled prefabs may implement <see cref="IPooledObject"/> to receive spawn/despawn callbacks.
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        [Tooltip("Prefab that this pool produces.")]
        [SerializeField] private GameObject prefab;

        [Tooltip("How many instances to pre-warm on Awake.")]
        [SerializeField] private int initialSize = 16;

        [Tooltip("If true the pool grows when empty, otherwise null is returned.")]
        [SerializeField] private bool expandable = true;

        private readonly Queue<GameObject> _available = new Queue<GameObject>();
        private Transform _root;

        public GameObject Prefab => prefab;

        private void Awake()
        {
            // Keep instances tidy under a child transform.
            _root = new GameObject($"{name}_Instances").transform;
            _root.SetParent(transform, false);

            for (int i = 0; i < initialSize; i++)
            {
                _available.Enqueue(CreateInstance());
            }
        }

        /// <summary>Configure the pool from code (used by pools created at runtime).</summary>
        public void Configure(GameObject pooledPrefab, int prewarm = 16, bool canExpand = true)
        {
            prefab = pooledPrefab;
            initialSize = prewarm;
            expandable = canExpand;
        }

        private GameObject CreateInstance()
        {
            GameObject go = Instantiate(prefab, _root);
            go.SetActive(false);

            // Tag the instance with its owning pool so it can return itself.
            PooledInstance marker = go.GetComponent<PooledInstance>();
            if (marker == null) marker = go.AddComponent<PooledInstance>();
            marker.OwningPool = this;

            return go;
        }

        /// <summary>Take an instance from the pool and activate it at the given position/rotation.</summary>
        public GameObject Spawn(Vector3 position, Quaternion rotation)
        {
            GameObject go = null;

            // Skip over any instances that were destroyed externally.
            while (_available.Count > 0 && go == null)
            {
                go = _available.Dequeue();
            }

            if (go == null)
            {
                if (!expandable) return null;
                go = CreateInstance();
            }

            go.transform.SetPositionAndRotation(position, rotation);
            go.SetActive(true);

            var pooled = go.GetComponent<IPooledObject>();
            pooled?.OnSpawnFromPool();

            return go;
        }

        /// <summary>Return an instance to the pool and deactivate it.</summary>
        public void Return(GameObject go)
        {
            if (go == null) return;

            var pooled = go.GetComponent<IPooledObject>();
            pooled?.OnReturnToPool();

            go.SetActive(false);
            go.transform.SetParent(_root, false);
            _available.Enqueue(go);
        }
    }

    /// <summary>
    /// Optional callbacks for pooled objects so they can reset their state on reuse.
    /// </summary>
    public interface IPooledObject
    {
        void OnSpawnFromPool();
        void OnReturnToPool();
    }

    /// <summary>
    /// Auto-added marker that remembers which pool an instance belongs to, so any
    /// script holding the GameObject can return it without a separate lookup.
    /// </summary>
    public class PooledInstance : MonoBehaviour
    {
        public ObjectPool OwningPool;

        /// <summary>Convenience: return this object to its pool.</summary>
        public void ReturnToPool()
        {
            if (OwningPool != null) OwningPool.Return(gameObject);
            else gameObject.SetActive(false);
        }
    }
}
