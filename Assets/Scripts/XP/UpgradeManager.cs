using System.Collections.Generic;
using UnityEngine;
using SurvivalMoba.Data;
using SurvivalMoba.Player;

namespace SurvivalMoba.XP
{
    /// <summary>
    /// Holds the pool of available upgrades, rolls the 3 choices shown on level-up,
    /// tracks stack counts and applies the chosen upgrade to the player's stats.
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        [Tooltip("All upgrades that can appear during a run.")]
        [SerializeField] private List<UpgradeData> upgradePool = new List<UpgradeData>();

        [Tooltip("How many choices to present per level-up.")]
        [SerializeField] private int choicesPerLevel = 3;

        private readonly Dictionary<UpgradeData, int> _stacks = new Dictionary<UpgradeData, int>();
        private readonly List<UpgradeData> _scratch = new List<UpgradeData>();

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Returns up to <see cref="choicesPerLevel"/> distinct, still-available upgrades.</summary>
        public List<UpgradeData> RollChoices()
        {
            _scratch.Clear();
            foreach (UpgradeData u in upgradePool)
            {
                if (u == null) continue;
                if (u.maxStacks > 0 && GetStacks(u) >= u.maxStacks) continue;
                _scratch.Add(u);
            }

            // Fisher-Yates shuffle, then take the first N.
            for (int i = _scratch.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (_scratch[i], _scratch[j]) = (_scratch[j], _scratch[i]);
            }

            int take = Mathf.Min(choicesPerLevel, _scratch.Count);
            return _scratch.GetRange(0, take);
        }

        public int GetStacks(UpgradeData upgrade)
        {
            return _stacks.TryGetValue(upgrade, out int n) ? n : 0;
        }

        /// <summary>Apply the player's chosen upgrade.</summary>
        public void Apply(UpgradeData upgrade)
        {
            if (upgrade == null) return;

            _stacks[upgrade] = GetStacks(upgrade) + 1;

            PlayerCharacter player = PlayerCharacter.Instance;
            if (player != null && player.Stats != null)
            {
                player.Stats.ApplyUpgrade(upgrade);
            }
        }
    }
}
