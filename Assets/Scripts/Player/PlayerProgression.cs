using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(ActorMotor))]
[RequireComponent(typeof(WeaponController))]
[RequireComponent(typeof(PlayerSkills))]
[RequireComponent(typeof(Health))]
public class PlayerProgression : MonoBehaviour
{
    [SerializeField] private ProgressionData data;

    private WeaponController weapon;
    private PlayerSkills skills;
    private Health health;
    private ActorMotor motor;
    private EnemyFactory enemyFactory;
    private float baseArmor;
    private float damageMultiplier;
    private float armorBonus;
    private int totalExperience;
    private int currentLevel = 1;
    private bool isFactorySubscribed;
    private readonly Dictionary<PlayerUpgradeType, int> upgradeLevels = new Dictionary<PlayerUpgradeType, int>();

    public event Action ProgressChanged;
    public event Action<int> LevelChanged;
    public event Action<int> LevelsGained;
    public event Action<PlayerUpgradeData, int> UpgradeApplied;

    public int TotalExperience => totalExperience;
    public int CurrentLevel => currentLevel;
    public float DamageMultiplier => damageMultiplier;

    public int ExperienceInCurrentLevel => data != null
        ? totalExperience % data.ExperiencePerLevel
        : 0;

    public int ExperienceToNextLevel => data != null
        ? data.ExperiencePerLevel
        : 1;

    public float ExperienceProgress =>
        Mathf.Clamp01((float)ExperienceInCurrentLevel / ExperienceToNextLevel);

    private void Awake()
    {
        weapon = GetComponent<WeaponController>();
        skills = GetComponent<PlayerSkills>();
        health = GetComponent<Health>();
        motor = GetComponent<ActorMotor>();
        baseArmor = health.Armor;
        ApplyCombatStats();
    }

    private void OnEnable()
    {
        TrySubscribeToFactory();
    }

    private void Start()
    {
        EnsureUpgradeController();
        TrySubscribeToFactory();
        ProgressChanged?.Invoke();
    }

    private void OnDisable()
    {
        UnsubscribeFromFactory();
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0 || data == null)
        {
            return;
        }

        totalExperience += amount;
        int previousLevel = currentLevel;
        currentLevel = data.GetLevel(totalExperience);

        if (currentLevel != previousLevel)
        {
            int gainedLevels = currentLevel - previousLevel;
            SoundManager.Play(SoundId.LevelUp);
            LevelChanged?.Invoke(currentLevel);
            LevelsGained?.Invoke(gainedLevels);
        }

        ProgressChanged?.Invoke();
    }

    private void HandleEnemyDefeated(EnemyController enemy)
    {
        if (enemy != null)
        {
            AddExperience(enemy.ExperienceReward);
        }
    }

    public int GetUpgradeLevel(PlayerUpgradeType upgradeType)
    {
        return upgradeLevels.TryGetValue(upgradeType, out int level)
            ? level
            : 0;
    }

    public bool CanApplyUpgrade(PlayerUpgradeData upgrade)
    {
        return upgrade != null &&
               GetUpgradeLevel(upgrade.UpgradeType) < upgrade.MaxLevel;
    }

    public string GetUpgradePreview(PlayerUpgradeData upgrade)
    {
        if (upgrade == null)
        {
            return string.Empty;
        }

        int level = GetUpgradeLevel(upgrade.UpgradeType);
        float value = upgrade.ValuePerLevel;

        switch (upgrade.UpgradeType)
        {
            case PlayerUpgradeType.Damage:
                return $"Total damage bonus: {FormatPercent(level * value)} -> " +
                       $"{FormatPercent((level + 1) * value)}";
            case PlayerUpgradeType.MaxHealth:
                return $"Max Health: {FormatNumber(health.MaxHealth)} -> " +
                       $"{FormatNumber(health.MaxHealth + value)}\n" +
                       $"Restore {FormatNumber(value)} Health";
            case PlayerUpgradeType.Armor:
                return $"Armor: {FormatNumber(health.Armor)} -> " +
                       $"{FormatNumber(health.Armor + value)}";
            case PlayerUpgradeType.MoveSpeed:
                return GetMultiplierPreview("Move Speed", motor.MoveSpeed, level, value);
            case PlayerUpgradeType.AttackRange:
                return GetMultiplierPreview("Attack Range", weapon.AttackRange, level, value);
            case PlayerUpgradeType.ChargeRecovery:
                return GetRecoveryPreview(level, value);
            case PlayerUpgradeType.BombRadius:
                return GetMultiplierPreview("Bomb Radius", skills.BombRadius, level, value);
            case PlayerUpgradeType.SkillCooldown:
                return $"Total cooldown reduction: {FormatPercent(level * value)} -> " +
                       $"{FormatPercent((level + 1) * value)}";
            default:
                return upgrade.Description;
        }
    }

    public bool ApplyUpgrade(PlayerUpgradeData upgrade)
    {
        if (!CanApplyUpgrade(upgrade))
        {
            return false;
        }

        PlayerUpgradeType type = upgrade.UpgradeType;
        float value = upgrade.ValuePerLevel;
        int nextLevel = GetUpgradeLevel(type) + 1;
        upgradeLevels[type] = nextLevel;

        switch (type)
        {
            case PlayerUpgradeType.Damage:
                damageMultiplier += value;
                ApplyCombatStats();
                break;
            case PlayerUpgradeType.MaxHealth:
                health.IncreaseHealth(value, value);
                break;
            case PlayerUpgradeType.Armor:
                armorBonus += value;
                ApplyCombatStats();
                break;
            case PlayerUpgradeType.MoveSpeed:
                motor.AddMoveSpeedMultiplier(value);
                break;
            case PlayerUpgradeType.AttackRange:
                weapon.AddAttackRangeMultiplier(value);
                break;
            case PlayerUpgradeType.ChargeRecovery:
                weapon.AddChargeRecoveryReduction(value);
                break;
            case PlayerUpgradeType.BombRadius:
                skills.AddBombRadiusMultiplier(value);
                break;
            case PlayerUpgradeType.SkillCooldown:
                skills.AddSkillCooldownReduction(value);
                break;
            default:
                return false;
        }

        UpgradeApplied?.Invoke(upgrade, nextLevel);
        return true;
    }

    private void ApplyCombatStats()
    {
        float multiplier = DamageMultiplier;
        weapon?.SetDamageMultiplier(multiplier);
        skills?.SetDamageMultiplier(multiplier);
        health?.SetArmor(baseArmor + armorBonus);
    }

    private static string GetMultiplierPreview(
        string label,
        float currentValue,
        int level,
        float increasePerLevel)
    {
        float currentMultiplier = 1f + level * increasePerLevel;
        float nextMultiplier = 1f + (level + 1) * increasePerLevel;
        float nextValue = currentMultiplier <= 0f
            ? currentValue
            : currentValue * nextMultiplier / currentMultiplier;
        return $"{label}: {FormatNumber(currentValue)} -> {FormatNumber(nextValue)}";
    }

    private string GetRecoveryPreview(int level, float reductionPerLevel)
    {
        float currentReduction = level * reductionPerLevel;
        float nextReduction = (level + 1) * reductionPerLevel;
        float currentDuration = weapon.ChargeRecoveryDuration;
        float baseDuration = 1f - currentReduction <= 0f
            ? currentDuration
            : currentDuration / (1f - currentReduction);
        float nextDuration = baseDuration * (1f - nextReduction);
        return $"Charge Recovery: {FormatNumber(currentDuration)}s -> " +
               $"{FormatNumber(nextDuration)}s";
    }

    private static string FormatNumber(float value)
    {
        return value.ToString("0.##");
    }

    private static string FormatPercent(float value)
    {
        return $"+{value * 100f:0}%";
    }

    private void EnsureUpgradeController()
    {
        if (GetComponent<LevelUpUpgradeController>() == null)
        {
            gameObject.AddComponent<LevelUpUpgradeController>();
        }
    }

    private void TrySubscribeToFactory()
    {
        if (isFactorySubscribed)
        {
            return;
        }

        if (enemyFactory == null)
        {
            enemyFactory = FindFirstObjectByType<EnemyFactory>();
        }

        if (enemyFactory == null)
        {
            return;
        }

        enemyFactory.EnemyDefeated += HandleEnemyDefeated;
        isFactorySubscribed = true;
    }

    private void UnsubscribeFromFactory()
    {
        if (!isFactorySubscribed || enemyFactory == null)
        {
            return;
        }

        enemyFactory.EnemyDefeated -= HandleEnemyDefeated;
        isFactorySubscribed = false;
    }
}
