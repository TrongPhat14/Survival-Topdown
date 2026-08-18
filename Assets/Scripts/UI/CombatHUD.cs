using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class CombatHUD : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private GameObject attackCooldownRoot;
    [SerializeField] private Slider[] chargeSliders;

    [Header("Bomb")]
    [SerializeField] private Image bombCooldownFill;
    [SerializeField] private TMP_Text bombCooldownText;

    [Header("Dash")]
    [SerializeField] private Image dashCooldownFill;
    [SerializeField] private TMP_Text dashCooldownText;

    private WeaponController weapon;
    private PlayerSkills skills;

    private void Awake()
    {
        weapon = FindFirstObjectByType<WeaponController>();
        skills = FindFirstObjectByType<PlayerSkills>();

        if (attackCooldownRoot != null)
        {
            attackCooldownRoot.SetActive(false);
        }

        if (chargeSliders == null)
        {
            return;
        }

        foreach (Slider chargeSlider in chargeSliders)
        {
            if (chargeSlider != null)
            {
                chargeSlider.interactable = false;
            }
        }
    }

    private void Update()
    {
        UpdateCharges();

        if (skills == null)
        {
            return;
        }

        UpdateCooldown(
            bombCooldownFill,
            bombCooldownText,
            skills.BombCooldownRemaining,
            skills.BombCooldown);

        UpdateCooldown(
            dashCooldownFill,
            dashCooldownText,
            skills.DashCooldownRemaining,
            skills.DashCooldown);
    }

    private void UpdateCharges()
    {
        if (weapon == null || chargeSliders == null)
        {
            return;
        }

        int currentCharges = weapon.CurrentCharges;
        float recoveryProgress = weapon.ChargeRecoveryProgress;

        for (int i = 0; i < chargeSliders.Length; i++)
        {
            Slider chargeSlider = chargeSliders[i];
            if (chargeSlider == null)
            {
                continue;
            }

            float value = 0f;

            if (i < currentCharges)
            {
                value = 1f;
            }
            else if (i == currentCharges && currentCharges < weapon.MaxCharges)
            {
                value = recoveryProgress;
            }

            chargeSlider.SetValueWithoutNotify(value);
        }
    }

    private static void UpdateCooldown(
        Image fill,
        TMP_Text label,
        float remaining,
        float duration)
    {
        bool isCoolingDown = remaining > 0f;

        if (fill != null)
        {
            fill.fillAmount = isCoolingDown && duration > 0f
                ? Mathf.Clamp01(remaining / duration)
                : 0f;
        }

        if (label != null)
        {
            label.text = isCoolingDown
                ? Mathf.CeilToInt(remaining).ToString()
                : string.Empty;
        }
    }
}
