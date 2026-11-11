using UnityEngine;

[CreateAssetMenu(fileName = "PlayerProgression", menuName = "Survival/Player Progression Data")]
public class ProgressionData : ScriptableObject
{
    [Header("Experience")]
    [SerializeField, Min(1)] private int experiencePerLevel = 100;

    public int ExperiencePerLevel => Mathf.Max(1, experiencePerLevel);

    public int GetLevel(int totalExperience)
    {
        return Mathf.Max(0, totalExperience) / ExperiencePerLevel + 1;
    }

}
