using UnityEngine;
using SurvivalMoba.Core;
using SurvivalMoba.Player;
using SurvivalMoba.Systems;

namespace SurvivalMoba.XP
{
    /// <summary>
    /// Pooled XP pickup dropped by dead enemies. When the player comes within the
    /// attract radius it homes in, and on contact it adds its value and despawns.
    /// </summary>
    public class XPOrb : MonoBehaviour, IPooledObject
    {
        [SerializeField] private float attractRadius = 3f;
        [SerializeField] private float collectRadius = 0.6f;
        [SerializeField] private float homingSpeed = 9f;

        private int _value = 1;
        private Transform _player;

        public void SetValue(int value) => _value = Mathf.Max(1, value);

        public void OnSpawnFromPool()
        {
            _player = PlayerCharacter.Instance != null ? PlayerCharacter.Instance.transform : null;
        }

        public void OnReturnToPool() { }

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;
            if (_player == null)
            {
                _player = PlayerCharacter.Instance != null ? PlayerCharacter.Instance.transform : null;
                if (_player == null) return;
            }

            Vector3 toPlayer = _player.position - transform.position;
            toPlayer.y = 0f;
            float dist = toPlayer.magnitude;

            if (dist <= collectRadius)
            {
                Collect();
                return;
            }

            // Homing pickup magnet.
            if (dist <= attractRadius)
            {
                Vector3 dir = toPlayer / Mathf.Max(dist, 0.0001f);
                transform.position += dir * (homingSpeed * Time.deltaTime);
            }
        }

        private void Collect()
        {
            if (XPSystem.Instance != null) XPSystem.Instance.AddXP(_value);
            PoolManager.Despawn(gameObject);
        }
    }
}
