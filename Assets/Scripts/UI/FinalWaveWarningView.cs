using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class FinalWaveWarningView : MonoBehaviour
{
    [SerializeField] private WaveController waveController;
    [SerializeField] private Graphic[] warningIcons;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float fadeInDuration = 0.2f;
    [SerializeField, Min(0f)] private float displayDuration = 1f;
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.5f;
    [SerializeField, Min(0.05f)] private float blinkInterval = 0.15f;
    [SerializeField, Range(0f, 1f)] private float blinkMinimumAlpha = 0.35f;

    private CanvasGroup canvasGroup;
    private Color[] iconColors;
    private Coroutine displayRoutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        CacheIconColors();

        if (waveController == null)
        {
            waveController = FindFirstObjectByType<WaveController>();
        }
    }

    private void OnEnable()
    {
        if (waveController != null)
        {
            waveController.WaveStarted += HandleWaveStarted;
        }
    }

    private void OnDisable()
    {
        if (waveController != null)
        {
            waveController.WaveStarted -= HandleWaveStarted;
        }
    }

    private void HandleWaveStarted(int waveNumber)
    {
        if (waveNumber != waveController.TotalWaves)
        {
            return;
        }

        if (displayRoutine != null)
        {
            StopCoroutine(displayRoutine);
        }

        displayRoutine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        SetIconAlpha(1f);
        yield return Fade(0f, 1f, fadeInDuration);

        float elapsed = 0f;
        while (elapsed < displayDuration)
        {
            elapsed += Time.deltaTime;
            float blink = Mathf.Lerp(
                blinkMinimumAlpha,
                1f,
                Mathf.PingPong(elapsed / blinkInterval, 1f));
            SetIconAlpha(blink);
            yield return null;
        }

        SetIconAlpha(1f);
        yield return Fade(1f, 0f, fadeOutDuration);
        displayRoutine = null;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            canvasGroup.alpha = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    private void CacheIconColors()
    {
        if (warningIcons == null)
        {
            iconColors = System.Array.Empty<Color>();
            return;
        }

        iconColors = new Color[warningIcons.Length];
        for (int i = 0; i < warningIcons.Length; i++)
        {
            iconColors[i] = warningIcons[i] != null
                ? warningIcons[i].color
                : Color.white;
        }
    }

    private void SetIconAlpha(float alpha)
    {
        if (warningIcons == null || iconColors == null)
        {
            return;
        }

        for (int i = 0; i < warningIcons.Length; i++)
        {
            if (warningIcons[i] == null)
            {
                continue;
            }

            Color color = iconColors[i];
            color.a *= alpha;
            warningIcons[i].color = color;
        }
    }
}
