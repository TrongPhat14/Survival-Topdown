using Unity.Cinemachine;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CinemachineBasicMultiChannelPerlin))]
public class CameraShake : MonoBehaviour
{
    [Header("Player Hit Enemy")]
    [SerializeField, Min(0f)] private float playerHitDuration = 0.1f;
    [SerializeField, Min(0f)] private float playerHitAmplitude = 0.35f;

    [Header("Bomb Explosion")]
    [SerializeField, Min(0f)] private float bombDuration = 0.32f;
    [SerializeField, Min(0f)] private float bombAmplitude = 1.2f;

    [Header("Player Damaged")]
    [SerializeField, Min(0f)] private float playerDamagedDuration = 0.18f;
    [SerializeField, Min(0f)] private float playerDamagedAmplitude = 0.7f;

    private static CameraShake instance;

    private CinemachineBasicMultiChannelPerlin noise;
    private float remainingTime;
    private float totalDuration;
    private float peakAmplitude;

    private void Awake()
    {
        noise = GetComponent<CinemachineBasicMultiChannelPerlin>();
        noise.AmplitudeGain = 0f;
    }

    private void OnEnable()
    {
        instance = this;
    }

    private void OnDisable()
    {
        ResetShake();

        if (instance == this)
        {
            instance = null;
        }
    }

    private void Update()
    {
        if (remainingTime <= 0f)
        {
            return;
        }

        remainingTime = Mathf.Max(
            0f,
            remainingTime - Time.unscaledDeltaTime);

        float normalizedTime = totalDuration > 0f
            ? remainingTime / totalDuration
            : 0f;
        noise.AmplitudeGain = peakAmplitude * Mathf.SmoothStep(
            0f,
            1f,
            normalizedTime);

        if (remainingTime <= 0f)
        {
            ResetShake();
        }
    }

    public static void PlayPlayerHitEnemy()
    {
        instance?.StartShake(
            instance.playerHitDuration,
            instance.playerHitAmplitude);
    }

    public static void PlayBombExplosion()
    {
        instance?.StartShake(
            instance.bombDuration,
            instance.bombAmplitude);
    }

    public static void PlayPlayerDamaged()
    {
        instance?.StartShake(
            instance.playerDamagedDuration,
            instance.playerDamagedAmplitude);
    }

    private void StartShake(float duration, float amplitude)
    {
        if (duration <= 0f || amplitude <= 0f || noise.NoiseProfile == null)
        {
            return;
        }

        if (remainingTime <= 0f)
        {
            totalDuration = duration;
            remainingTime = duration;
            peakAmplitude = amplitude;
        }
        else
        {
            totalDuration = Mathf.Max(remainingTime, duration);
            remainingTime = totalDuration;
            peakAmplitude = Mathf.Max(peakAmplitude, amplitude);
        }

        noise.AmplitudeGain = peakAmplitude;
    }

    private void ResetShake()
    {
        remainingTime = 0f;
        totalDuration = 0f;
        peakAmplitude = 0f;

        if (noise != null)
        {
            noise.AmplitudeGain = 0f;
        }
    }
}
