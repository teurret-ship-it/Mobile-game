using UnityEngine;
using SurvivalMoba.Systems;

namespace SurvivalMoba.UI
{
    /// <summary>
    /// Pooled world-space damage number that drifts upward and fades, then returns to
    /// the pool. Prefab should carry a TextMesh (or 3D Text) component.
    /// </summary>
    [RequireComponent(typeof(TextMesh))]
    public class FloatingDamageNumber : MonoBehaviour, IPooledObject
    {
        [SerializeField] private float lifetime = 0.8f;
        [SerializeField] private float riseSpeed = 1.5f;

        private TextMesh _text;
        private float _spawnTime;
        private Color _baseColor = Color.white;
        private Camera _cam;

        private void Awake()
        {
            _text = GetComponent<TextMesh>();
            _baseColor = _text.color;
            _cam = Camera.main;
        }

        public void Show(float amount)
        {
            _text.text = Mathf.RoundToInt(amount).ToString();
            _spawnTime = Time.time;
            _text.color = _baseColor;
        }

        public void OnSpawnFromPool() { }
        public void OnReturnToPool() { }

        private void Update()
        {
            float t = (Time.time - _spawnTime) / lifetime;
            if (t >= 1f)
            {
                PoolManager.Despawn(gameObject);
                return;
            }

            transform.position += Vector3.up * (riseSpeed * Time.deltaTime);

            // Billboard toward the camera so the number stays readable in the iso view.
            if (_cam == null) _cam = Camera.main;
            if (_cam != null) transform.forward = _cam.transform.forward;

            Color c = _baseColor;
            c.a = 1f - t;
            _text.color = c;
        }
    }
}
