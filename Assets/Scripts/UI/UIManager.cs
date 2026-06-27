using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SurvivalMoba.Core;
using SurvivalMoba.Data;
using SurvivalMoba.Enemies;
using SurvivalMoba.Player;
using SurvivalMoba.Systems;
using SurvivalMoba.XP;

namespace SurvivalMoba.UI
{
    /// <summary>
    /// Wires all in-game HUD elements to the systems that drive them: health/XP bars,
    /// level, match timer, boss bar, the level-up choice panel and the end-of-match
    /// victory/defeat screens. Keeps gameplay scripts free of direct UI references.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] private Image healthFill;
        [SerializeField] private Text healthText;
        [SerializeField] private Image xpFill;
        [SerializeField] private Text levelText;
        [SerializeField] private Text timerText;

        [Header("Boss")]
        [SerializeField] private GameObject bossBarRoot;
        [SerializeField] private Image bossFill;

        [Header("Level Up")]
        [SerializeField] private GameObject levelUpPanel;
        [SerializeField] private List<UpgradeCard> upgradeCards = new List<UpgradeCard>();

        [Header("End Screens")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject defeatPanel;
        [SerializeField] private GameObject pausePanel;

        [Header("System References")]
        [SerializeField] private LevelUpManager levelUpManager;
        [SerializeField] private WaveManager waveManager;

        private HealthSystem _bossHealth;

        private void Start()
        {
            // Health
            if (PlayerCharacter.Instance != null)
            {
                PlayerCharacter.Instance.Health.HealthChanged += OnHealthChanged;
                OnHealthChanged(PlayerCharacter.Instance.Health.Current, PlayerCharacter.Instance.Health.Max);
            }

            // XP / level
            if (XPSystem.Instance != null)
            {
                XPSystem.Instance.XPChanged += OnXPChanged;
                XPSystem.Instance.LeveledUp += OnLevelChanged;
                OnXPChanged(XPSystem.Instance.CurrentXP, XPSystem.Instance.RequiredXP);
                OnLevelChanged(XPSystem.Instance.Level);
            }

            // Game state / timer
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TimerTick += OnTimer;
                GameManager.Instance.Victory += OnVictory;
                GameManager.Instance.Defeat += OnDefeat;
                OnTimer(0f);
            }

            // Level-up flow
            if (levelUpManager == null) levelUpManager = FindObjectOfType<LevelUpManager>();
            if (levelUpManager != null)
            {
                levelUpManager.ChoicesReady += OnChoicesReady;
                levelUpManager.Closed += HideLevelUp;
            }

            // Boss bar
            if (waveManager == null) waveManager = FindObjectOfType<WaveManager>();
            if (waveManager != null) waveManager.BossSpawned += OnBossSpawned;

            HideLevelUp();
            if (bossBarRoot != null) bossBarRoot.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (PlayerCharacter.Instance != null)
                PlayerCharacter.Instance.Health.HealthChanged -= OnHealthChanged;
            if (XPSystem.Instance != null)
            {
                XPSystem.Instance.XPChanged -= OnXPChanged;
                XPSystem.Instance.LeveledUp -= OnLevelChanged;
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TimerTick -= OnTimer;
                GameManager.Instance.Victory -= OnVictory;
                GameManager.Instance.Defeat -= OnDefeat;
            }
            if (levelUpManager != null)
            {
                levelUpManager.ChoicesReady -= OnChoicesReady;
                levelUpManager.Closed -= HideLevelUp;
            }
            if (waveManager != null) waveManager.BossSpawned -= OnBossSpawned;
            if (_bossHealth != null) _bossHealth.HealthChanged -= OnBossHealthChanged;
        }

        private void Update()
        {
            // Boss bar follows live boss health; hide it when the boss is gone.
            if (bossBarRoot != null && bossBarRoot.activeSelf && (_bossHealth == null || !_bossHealth.IsAlive))
                bossBarRoot.SetActive(false);
        }

        // --- HUD callbacks ---

        private void OnHealthChanged(float current, float max)
        {
            if (healthFill != null) healthFill.fillAmount = max > 0f ? current / max : 0f;
            if (healthText != null) healthText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
        }

        private void OnXPChanged(int current, int required)
        {
            if (xpFill != null) xpFill.fillAmount = required > 0f ? (float)current / required : 0f;
        }

        private void OnLevelChanged(int level)
        {
            if (levelText != null) levelText.text = $"Lv {level}";
        }

        private void OnTimer(float elapsed)
        {
            if (timerText == null) return;
            float remaining = GameManager.Instance != null ? GameManager.Instance.TimeRemaining : 0f;
            int m = Mathf.FloorToInt(remaining / 60f);
            int s = Mathf.FloorToInt(remaining % 60f);
            timerText.text = $"{m:00}:{s:00}";
        }

        // --- Boss ---

        private void OnBossSpawned(HealthSystem bossHealth)
        {
            _bossHealth = bossHealth;
            if (_bossHealth != null) _bossHealth.HealthChanged += OnBossHealthChanged;
            if (bossBarRoot != null) bossBarRoot.SetActive(true);
            OnBossHealthChanged(bossHealth.Current, bossHealth.Max);
        }

        private void OnBossHealthChanged(float current, float max)
        {
            if (bossFill != null) bossFill.fillAmount = max > 0f ? current / max : 0f;
        }

        // --- Level up ---

        private void OnChoicesReady(List<UpgradeData> choices)
        {
            if (levelUpPanel != null) levelUpPanel.SetActive(true);

            for (int i = 0; i < upgradeCards.Count; i++)
            {
                bool has = i < choices.Count;
                upgradeCards[i].gameObject.SetActive(has);
                if (has) upgradeCards[i].Bind(choices[i], OnUpgradeChosen);
            }
        }

        private void OnUpgradeChosen(UpgradeData upgrade)
        {
            if (levelUpManager != null) levelUpManager.ChooseUpgrade(upgrade);
        }

        private void HideLevelUp()
        {
            if (levelUpPanel != null) levelUpPanel.SetActive(false);
        }

        // --- End screens & buttons (hook these to UI buttons) ---

        private void OnVictory() { if (victoryPanel != null) victoryPanel.SetActive(true); }
        private void OnDefeat() { if (defeatPanel != null) defeatPanel.SetActive(true); }

        /// <summary>Pause button hook.</summary>
        public void OnPausePressed()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.TogglePause();
            if (pausePanel != null) pausePanel.SetActive(GameManager.Instance.State == GameState.Paused);
        }

        /// <summary>Resume button hook.</summary>
        public void OnResumePressed()
        {
            if (GameManager.Instance != null) GameManager.Instance.Resume();
            if (pausePanel != null) pausePanel.SetActive(false);
        }
    }
}
