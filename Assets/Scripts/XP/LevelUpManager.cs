using System;
using System.Collections.Generic;
using UnityEngine;
using SurvivalMoba.Core;
using SurvivalMoba.Data;

namespace SurvivalMoba.XP
{
    /// <summary>
    /// Bridges XP level-ups to the upgrade-choice UI. On level up it freezes the
    /// game and asks the UI to present rolled choices; when the player picks one it
    /// applies the upgrade and either presents the next queued level-up or resumes.
    /// </summary>
    public class LevelUpManager : MonoBehaviour
    {
        [SerializeField] private XPSystem xpSystem;
        [SerializeField] private UpgradeManager upgradeManager;

        private int _pendingLevelUps;
        private bool _choosing;

        /// <summary>Raised when choices are ready to display (UI listens to this).</summary>
        public event Action<List<UpgradeData>> ChoicesReady;

        /// <summary>Raised when the level-up flow ends and gameplay resumes.</summary>
        public event Action Closed;

        private void Awake()
        {
            if (xpSystem == null) xpSystem = FindObjectOfType<XPSystem>();
            if (upgradeManager == null) upgradeManager = FindObjectOfType<UpgradeManager>();
        }

        private void OnEnable()
        {
            if (xpSystem != null) xpSystem.LeveledUp += OnLeveledUp;
        }

        private void OnDisable()
        {
            if (xpSystem != null) xpSystem.LeveledUp -= OnLeveledUp;
        }

        private void OnLeveledUp(int newLevel)
        {
            _pendingLevelUps++;
            if (!_choosing) PresentNext();
        }

        private void PresentNext()
        {
            if (_pendingLevelUps <= 0)
            {
                _choosing = false;
                if (GameManager.Instance != null) GameManager.Instance.Resume();
                Closed?.Invoke();
                return;
            }

            _pendingLevelUps--;
            _choosing = true;

            if (GameManager.Instance != null) GameManager.Instance.Pause(GameState.LevelUp);

            List<UpgradeData> choices = upgradeManager != null
                ? upgradeManager.RollChoices()
                : new List<UpgradeData>();

            if (choices.Count == 0)
            {
                // Nothing left to offer; skip straight through.
                PresentNext();
                return;
            }

            ChoicesReady?.Invoke(choices);
        }

        /// <summary>Called by the UI when the player taps an upgrade card.</summary>
        public void ChooseUpgrade(UpgradeData upgrade)
        {
            if (!_choosing) return;
            if (upgradeManager != null) upgradeManager.Apply(upgrade);
            PresentNext();
        }
    }
}
