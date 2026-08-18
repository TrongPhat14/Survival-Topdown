using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class WaveHUD : MonoBehaviour
{
    [SerializeField] private Slider waveProgress;
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text aliveText;
    [SerializeField, Min(0f)] private float fillSpeed = 1f;

    private float targetProgress;

    private void Awake()
    {
        if (waveProgress == null)
        {
            return;
        }

        waveProgress.interactable = false;
        waveProgress.minValue = 0f;
        waveProgress.maxValue = 1f;
        waveProgress.wholeNumbers = false;
        targetProgress = waveProgress.value;
    }

    private void Update()
    {
        if (waveProgress == null || Mathf.Approximately(waveProgress.value, targetProgress))
        {
            return;
        }

        float value = Mathf.MoveTowards(
            waveProgress.value,
            targetProgress,
            fillSpeed * Time.unscaledDeltaTime);

        waveProgress.SetValueWithoutNotify(value);
    }

    public void SetWave(int currentWave, int totalWaves)
    {
        totalWaves = Mathf.Max(1, totalWaves);
        currentWave = Mathf.Clamp(currentWave, 0, totalWaves);
        targetProgress = (float)currentWave / totalWaves;

        if (waveText != null)
        {
            waveText.SetText("Wave {0} / {1}", currentWave, totalWaves);
        }
    }

    public void SetAliveCount(int aliveCount)
    {
        if (aliveText != null)
        {
            aliveText.SetText("{0}", Mathf.Max(0, aliveCount));
        }
    }

    public void SetState(int currentWave, int totalWaves, int aliveCount)
    {
        SetWave(currentWave, totalWaves);
        SetAliveCount(aliveCount);
    }
}
