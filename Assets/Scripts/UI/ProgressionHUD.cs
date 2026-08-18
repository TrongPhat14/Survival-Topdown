using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ProgressionHUD : MonoBehaviour
{
    [SerializeField] private PlayerProgression progression;
    [SerializeField] private Slider experienceSlider;
    [SerializeField] private TMP_Text levelText;

    private void Awake()
    {
        if (progression == null)
        {
            progression = FindFirstObjectByType<PlayerProgression>();
        }

        if (experienceSlider != null)
        {
            experienceSlider.interactable = false;
            experienceSlider.minValue = 0f;
            experienceSlider.maxValue = 1f;
            experienceSlider.wholeNumbers = false;
        }
    }

    private void OnEnable()
    {
        if (progression == null)
        {
            return;
        }

        progression.ProgressChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (progression != null)
        {
            progression.ProgressChanged -= Refresh;
        }
    }

    private void Refresh()
    {
        if (experienceSlider != null)
        {
            experienceSlider.SetValueWithoutNotify(progression.ExperienceProgress);
        }

        if (levelText != null)
        {
            levelText.SetText("LV\n{0}", progression.CurrentLevel);
        }
    }
}
