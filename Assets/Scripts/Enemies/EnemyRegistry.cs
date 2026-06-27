using System.Collections.Generic;
using UnityEngine;

namespace SurvivalMoba.Enemies
{
    /// <summary>
    /// Static registry of all live enemies. Enemies add/remove themselves on
    /// spawn/despawn so combat code can query "nearest enemy" cheaply without
    /// scanning the whole scene with FindObjectsOfType.
    /// </summary>
    public static class EnemyRegistry
    {
        private static readonly List<EnemyController> Active = new List<EnemyController>();

        public static IReadOnlyList<EnemyController> ActiveEnemies => Active;
        public static int Count => Active.Count;

        public static void Register(EnemyController enemy)
        {
            if (enemy != null && !Active.Contains(enemy)) Active.Add(enemy);
        }

        public static void Unregister(EnemyController enemy)
        {
            Active.Remove(enemy);
        }

        public static void Clear() => Active.Clear();

        /// <summary>
        /// Find the closest living enemy to <paramref name="position"/> within
        /// <paramref name="maxRange"/>. Returns null if none qualify.
        /// </summary>
        public static EnemyController FindNearest(Vector3 position, float maxRange)
        {
            EnemyController best = null;
            float bestSqr = maxRange * maxRange;

            for (int i = 0; i < Active.Count; i++)
            {
                EnemyController e = Active[i];
                if (e == null || !e.IsAlive) continue;

                float sqr = (e.transform.position - position).sqrMagnitude;
                if (sqr <= bestSqr)
                {
                    bestSqr = sqr;
                    best = e;
                }
            }

            return best;
        }

        /// <summary>Collect all living enemies within <paramref name="radius"/> of a point.</summary>
        public static void QueryRadius(Vector3 center, float radius, List<EnemyController> results)
        {
            results.Clear();
            float sqr = radius * radius;
            for (int i = 0; i < Active.Count; i++)
            {
                EnemyController e = Active[i];
                if (e == null || !e.IsAlive) continue;
                if ((e.transform.position - center).sqrMagnitude <= sqr) results.Add(e);
            }
        }
    }
}
