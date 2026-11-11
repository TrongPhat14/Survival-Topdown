using UnityEngine;

public enum PlayerUpgradeType
{
    Damage,
    MaxHealth,
    Armor,
    MoveSpeed,
    AttackRange,
    ChargeRecovery,
    BombRadius,
    SkillCooldown
}

[CreateAssetMenu(fileName = "PlayerUpgrade", menuName = "Survival/Player Upgrade")]
public class PlayerUpgradeData : ScriptableObject
{
    [SerializeField] private PlayerUpgradeType upgradeType;
    [SerializeField] private string displayName = "Upgrade";
    [SerializeField, TextArea] private string description = "Improve a player stat.";
    [SerializeField, Min(1)] private int maxLevel = 5;
    [SerializeField, Min(0f)] private float valuePerLevel = 0.1f;
    [SerializeField] private Color accentColor = Color.cyan;

    public PlayerUpgradeType UpgradeType => upgradeType;
    public string DisplayName => displayName;
    public string Description => description;
    public int MaxLevel => Mathf.Max(1, maxLevel);
    public float ValuePerLevel => Mathf.Max(0f, valuePerLevel);
    public Color AccentColor => accentColor;
}
