using UnityEngine;
using UnityEngine.UI;
using SurvivalMoba.Core;
using SurvivalMoba.Data;

namespace SurvivalMoba.UI
{
    /// <summary>
    /// A single selectable upgrade card shown on the level-up screen. Bound to an
    /// <see cref="UpgradeData"/> and reports the player's choice through a callback.
    /// </summary>
    public class UpgradeCard : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Image background;
        [SerializeField] private Text nameText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Button button;

        [Header("Rarity Colours")]
        [SerializeField] private Color common = new Color(0.7f, 0.7f, 0.7f);
        [SerializeField] private Color rare = new Color(0.3f, 0.6f, 1f);
        [SerializeField] private Color epic = new Color(0.7f, 0.3f, 1f);
        [SerializeField] private Color legendary = new Color(1f, 0.7f, 0.2f);

        private UpgradeData _data;
        private System.Action<UpgradeData> _onChosen;

        private void Awake()
        {
            if (button == null) button = GetComponent<Button>();
            if (button != null) button.onClick.AddListener(HandleClick);
        }

        public void Bind(UpgradeData data, System.Action<UpgradeData> onChosen)
        {
            _data = data;
            _onChosen = onChosen;

            if (data == null) return;

            if (nameText != null) nameText.text = data.upgradeName;
            if (descriptionText != null) descriptionText.text = data.description;
            if (icon != null) { icon.sprite = data.icon; icon.enabled = data.icon != null; }
            if (background != null) background.color = RarityColor(data.rarity);
        }

        private Color RarityColor(Rarity r)
        {
            switch (r)
            {
                case Rarity.Rare: return rare;
                case Rarity.Epic: return epic;
                case Rarity.Legendary: return legendary;
                default: return common;
            }
        }

        private void HandleClick()
        {
            _onChosen?.Invoke(_data);
        }
    }
}
