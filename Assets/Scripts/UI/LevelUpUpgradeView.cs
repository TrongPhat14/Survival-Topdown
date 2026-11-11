using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LevelUpUpgradeView : MonoBehaviour
{
    private const int ChoiceCount = 3;

    [SerializeField] private ChoiceCard[] cards = new ChoiceCard[ChoiceCount];

    public event Action<PlayerUpgradeData> ChoiceSelected;

    public bool IsConfigured
    {
        get
        {
            if (cards == null || cards.Length != ChoiceCount)
            {
                return false;
            }

            foreach (ChoiceCard card in cards)
            {
                if (card == null || !card.IsConfigured)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public void Show(
        IReadOnlyList<PlayerUpgradeData> upgrades,
        PlayerProgression progression)
    {
        if (!IsConfigured || upgrades == null || progression == null)
        {
            Debug.LogError(
                "LevelUpUpgradeView references are not configured.",
                this);
            return;
        }

        for (int i = 0; i < cards.Length; i++)
        {
            bool hasChoice = i < upgrades.Count;
            cards[i].SetVisible(hasChoice);

            if (!hasChoice)
            {
                continue;
            }

            PlayerUpgradeData upgrade = upgrades[i];
            int currentLevel = progression.GetUpgradeLevel(upgrade.UpgradeType);
            cards[i].Bind(
                upgrade,
                currentLevel,
                progression.GetUpgradePreview(upgrade),
                () => ChoiceSelected?.Invoke(upgrade));
        }

        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    [Serializable]
    private sealed class ChoiceCard
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Button button;
        [SerializeField] private Image accent;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text rankText;
        [SerializeField] private TMP_Text descriptionText;

        private UnityAction currentClick;

        public bool IsConfigured =>
            root != null &&
            button != null &&
            accent != null &&
            nameText != null &&
            rankText != null &&
            descriptionText != null;

        public void SetVisible(bool visible)
        {
            root.SetActive(visible);
        }

        public void Bind(
            PlayerUpgradeData upgrade,
            int currentLevel,
            string preview,
            UnityAction onClick)
        {
            if (currentClick != null)
            {
                button.onClick.RemoveListener(currentClick);
            }

            currentClick = onClick;
            button.onClick.AddListener(currentClick);
            accent.color = upgrade.AccentColor;
            nameText.text = upgrade.DisplayName.ToUpperInvariant();
            rankText.SetText(
                "LV {0} > {1}",
                currentLevel,
                currentLevel + 1);
            descriptionText.text = string.IsNullOrWhiteSpace(preview)
                ? upgrade.Description
                : preview;
        }
    }
}
