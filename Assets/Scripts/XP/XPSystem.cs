using System;
using UnityEngine;

namespace SurvivalMoba.XP
{
    /// <summary>
    /// Tracks the player's XP and level. Uses a simple escalating XP curve and
    /// raises events that the UI and the level-up flow listen to.
    /// </summary>
    public class XPSystem : MonoBehaviour
    {
        public static XPSystem Instance { get; private set; }

        [Header("XP Curve")]
        [Tooltip("XP needed for level 1 -> 2.")]
        [SerializeField] private int baseRequirement = 20;

        [Tooltip("Multiplier applied to the requirement each level.")]
        [SerializeField] private float growth = 1.6f;

        private int _level = 1;
        private int _currentXP;
        private int _requiredXP;

        /// <summary>(currentXP, requiredXP) within the current level.</summary>
        public event Action<int, int> XPChanged;

        /// <summary>Fired with the new level when the player levels up.</summary>
        public event Action<int> LeveledUp;

        public int Level => _level;
        public int CurrentXP => _currentXP;
        public int RequiredXP => _requiredXP;

        private void Awake()
        {
            Instance = this;
            _requiredXP = baseRequirement;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            XPChanged?.Invoke(_currentXP, _requiredXP);
        }

        public void AddXP(int amount)
        {
            if (amount <= 0) return;
            _currentXP += amount;

            // Handle multiple level-ups from a single large gain.
            while (_currentXP >= _requiredXP)
            {
                _currentXP -= _requiredXP;
                _level++;
                _requiredXP = Mathf.RoundToInt(_requiredXP * growth);
                LeveledUp?.Invoke(_level);
            }

            XPChanged?.Invoke(_currentXP, _requiredXP);
        }
    }
}
