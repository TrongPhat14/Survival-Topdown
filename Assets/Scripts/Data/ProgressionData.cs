using UnityEngine;

[CreateAssetMenu(fileName = "PlayerProgression", menuName = "Survival/Player Progression Data")]
public class ProgressionData : ScriptableObject
{
    [Header("Experience")]
    [SerializeField, Min(1)] private int experiencePerLevel = 100;

    [Header("Per Level")]
    [SerializeField, Min(0f)] private float maxHealthIncrease = 40f;
    [SerializeField, Min(0f)] private float currentHealthIncrease = 40f;
    [SerializeField, Min(0f)] private float armorIncrease = 2f;
    [SerializeField, Min(0f)] private float damageMultiplierIncrease = 0.1f;

    public int ExperiencePerLevel => Mathf.Max(1, experiencePerLevel);
    public float MaxHealthIncrease => Mathf.Max(0f, maxHealthIncrease);
    public float CurrentHealthIncrease => Mathf.Max(0f, currentHealthIncrease);
    public float ArmorIncrease => Mathf.Max(0f, armorIncrease);

    public int GetLevel(int totalExperience)
    {
        return Mathf.Max(0, totalExperience) / ExperiencePerLevel + 1;
    }

    public float GetDamageMultiplier(int level)
    {
        return Mathf.Max(0, level - 1) * damageMultiplierIncrease;
    }

    public float GetArmorBonus(int level)
    {
        return Mathf.Max(0, level - 1) * ArmorIncrease;
    }
}
