using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerProgression))]
public class LevelUpUpgradeController : MonoBehaviour
{
    private const int ChoiceCount = 3;

    [SerializeField] private LevelUpUpgradeView view;

    private readonly List<PlayerUpgradeData> availableUpgrades = new List<PlayerUpgradeData>();
    private readonly List<PlayerUpgradeData> choices = new List<PlayerUpgradeData>(ChoiceCount);
    private PlayerProgression progression;
    private int pendingChoices;
    private float previousTimeScale = 1f;
    private bool restoreGameplayInput;
    private bool selectionOpen;
    private bool gameStateCaptured;

    private void Awake()
    {
        progression = GetComponent<PlayerProgression>();
        availableUpgrades.AddRange(
            Resources.LoadAll<PlayerUpgradeData>("Upgrades"));

        if (availableUpgrades.Count == 0)
        {
            Debug.LogError(
                "No PlayerUpgradeData assets found in Resources/Upgrades.",
                this);
            enabled = false;
            return;
        }

        if (view == null)
        {
            view = FindFirstObjectByType<LevelUpUpgradeView>(
                FindObjectsInactive.Include);
        }

        if (view == null || !view.IsConfigured)
        {
            Debug.LogError(
                "LevelUpUpgradeView is missing or not configured in the scene hierarchy.",
                this);
            enabled = false;
            return;
        }

        view.ChoiceSelected += HandleChoiceSelected;
        view.Hide();
    }

    private void OnEnable()
    {
        if (progression != null)
        {
            progression.LevelsGained += HandleLevelsGained;
        }
    }

    private void OnDisable()
    {
        if (progression != null)
        {
            progression.LevelsGained -= HandleLevelsGained;
        }

        RestoreGameState();
    }

    private void OnDestroy()
    {
        if (view != null)
        {
            view.ChoiceSelected -= HandleChoiceSelected;
        }

        RestoreGameState();
    }

    private void HandleLevelsGained(int count)
    {
        pendingChoices += Mathf.Max(0, count);

        if (!selectionOpen)
        {
            ShowNextSelection();
        }
    }

    private void ShowNextSelection()
    {
        while (pendingChoices > 0)
        {
            BuildChoices();

            if (choices.Count == 0)
            {
                pendingChoices--;
                continue;
            }

            if (!gameStateCaptured)
            {
                PauseGame();
            }

            selectionOpen = true;
            view.Show(choices, progression);
            return;
        }

        RestoreGameState();
    }

    private void BuildChoices()
    {
        choices.Clear();
        List<PlayerUpgradeData> eligible = new(availableUpgrades.Count);

        foreach (PlayerUpgradeData upgrade in availableUpgrades)
        {
            if (progression.CanApplyUpgrade(upgrade))
            {
                eligible.Add(upgrade);
            }
        }

        int count = Mathf.Min(ChoiceCount, eligible.Count);
        for (int i = 0; i < count; i++)
        {
            int selectedIndex = Random.Range(0, eligible.Count);
            choices.Add(eligible[selectedIndex]);
            eligible.RemoveAt(selectedIndex);
        }
    }

    private void HandleChoiceSelected(PlayerUpgradeData upgrade)
    {
        if (!selectionOpen || !progression.ApplyUpgrade(upgrade))
        {
            return;
        }

        pendingChoices = Mathf.Max(0, pendingChoices - 1);
        selectionOpen = false;
        view.Hide();
        ShowNextSelection();
    }

    private void PauseGame()
    {
        if (gameStateCaptured)
        {
            return;
        }

        gameStateCaptured = true;
        previousTimeScale = Time.timeScale;
        GameInput gameInput = GameInput.Instance;
        restoreGameplayInput = gameInput != null && gameInput.enabled;
        gameInput?.DisableGameplayInput();
        Time.timeScale = 0f;
    }

    private void RestoreGameState()
    {
        if (!gameStateCaptured)
        {
            return;
        }

        view?.Hide();
        selectionOpen = false;
        Time.timeScale = previousTimeScale;

        if (restoreGameplayInput)
        {
            GameInput.Instance?.EnableGameplayInput();
        }

        restoreGameplayInput = false;
        gameStateCaptured = false;
    }
}
