using System;
using UnityEngine;
using SurvivalMoba.Enemies;

namespace SurvivalMoba.Core
{
    /// <summary>
    /// Owns the high-level match flow: timer, pause/resume, level-up freeze and
    /// the victory/defeat conditions. Other systems read <see cref="State"/> and
    /// subscribe to its events rather than polling.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Match")]
        [Tooltip("Total survival time in seconds for the match.")]
        [SerializeField] private float matchDuration = 300f;

        [Header("References")]
        [SerializeField] private Transform player;

        private float _elapsed;
        private GameState _state = GameState.Loading;

        public event Action<GameState> StateChanged;
        public event Action<float> TimerTick;        // elapsed seconds
        public event Action Victory;
        public event Action Defeat;

        public GameState State => _state;
        public Transform Player => player;
        public float Elapsed => _elapsed;
        public float MatchDuration => matchDuration;
        public float TimeRemaining => Mathf.Max(0f, matchDuration - _elapsed);
        public bool IsPlaying => _state == GameState.Playing;

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
            EnemyRegistry.Clear();
        }

        private void Start()
        {
            StartMatch();
        }

        public void SetPlayer(Transform t) => player = t;

        public void SetMatchDuration(float seconds) => matchDuration = seconds;

        public void StartMatch()
        {
            _elapsed = 0f;
            Time.timeScale = 1f;
            SetState(GameState.Playing);
        }

        private void Update()
        {
            if (_state != GameState.Playing) return;

            _elapsed += Time.deltaTime;
            TimerTick?.Invoke(_elapsed);

            if (_elapsed >= matchDuration)
            {
                TriggerVictory();
            }
        }

        private void SetState(GameState next)
        {
            if (_state == next) return;
            _state = next;
            StateChanged?.Invoke(_state);
        }

        /// <summary>Freeze gameplay (level-up screen, settings, etc.).</summary>
        public void Pause(GameState pauseState = GameState.Paused)
        {
            if (_state == GameState.Victory || _state == GameState.Defeat) return;
            Time.timeScale = 0f;
            SetState(pauseState);
        }

        /// <summary>Resume gameplay after a pause or level-up.</summary>
        public void Resume()
        {
            if (_state == GameState.Victory || _state == GameState.Defeat) return;
            Time.timeScale = 1f;
            SetState(GameState.Playing);
        }

        public void TogglePause()
        {
            if (_state == GameState.Playing) Pause();
            else if (_state == GameState.Paused) Resume();
        }

        /// <summary>Called by the player's health system when the hero dies.</summary>
        public void OnPlayerDied()
        {
            if (_state == GameState.Victory || _state == GameState.Defeat) return;
            Time.timeScale = 0f;
            SetState(GameState.Defeat);
            Defeat?.Invoke();
        }

        /// <summary>Called when the boss is defeated, or when the timer runs out.</summary>
        public void TriggerVictory()
        {
            if (_state == GameState.Victory || _state == GameState.Defeat) return;
            Time.timeScale = 0f;
            SetState(GameState.Victory);
            Victory?.Invoke();
        }
    }
}
