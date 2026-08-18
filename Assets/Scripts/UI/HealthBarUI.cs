using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private bool faceCamera;

    private Camera targetCamera;

    private void Awake()
    {
        if (slider == null)
        {
            slider = GetComponentInChildren<Slider>(true);
        }

        if (health == null)
        {
            health = GetComponentInParent<Health>();
        }

        if (health == null)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                health = player.GetComponent<Health>();
            }
        }

        if (faceCamera)
        {
            targetCamera = Camera.main;
        }
    }

    private void OnEnable()
    {
        if (health == null)
        {
            return;
        }

        health.HealthChanged += Refresh;
        Refresh(health.CurrentHealth, health.MaxHealth);
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.HealthChanged -= Refresh;
        }
    }

    private void LateUpdate()
    {
        if (!faceCamera)
        {
            return;
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            return;
        }

        Vector3 forward = transform.position - targetCamera.transform.position;
        if (forward.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(forward, targetCamera.transform.up);
        }
    }

    private void Refresh(float currentHealth, float maxHealth)
    {
        if (slider != null)
        {
            slider.SetValueWithoutNotify(maxHealth > 0f ? currentHealth / maxHealth : 0f);
        }

        if (valueText != null)
        {
            valueText.SetText("{0:0} / {1:0}", currentHealth, maxHealth);
        }
    }
}
