using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(WeaponController))]
[RequireComponent(typeof(PlayerSkills))]
[RequireComponent(typeof(Health))]
public class PlayerProgression : MonoBehaviour
{
    [SerializeField] private ProgressionData data;

    private WeaponController weapon;
    private PlayerSkills skills;
    private Health health;
    private EnemyFactory enemyFactory;
    private float baseArmor;
    private int totalExperience;
    private int currentLevel = 1;
    private bool isFactorySubscribed;

    public event Action ProgressChanged;
    public event Action<int> LevelChanged;

    public int TotalExperience => totalExperience;
    public int CurrentLevel => currentLevel;
    public float DamageMultiplier =>
        data != null ? data.GetDamageMultiplier(CurrentLevel) : 0f;

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
        baseArmor = health.Armor;
        ApplyCombatStats();
    }

    private void OnEnable()
    {
        TrySubscribeToFactory();
    }

    private void Start()
    {
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
            ApplyLevelUp(gainedLevels);
            SoundManager.Play(SoundId.LevelUp);
            LevelChanged?.Invoke(currentLevel);
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

    private void ApplyLevelUp(int gainedLevels)
    {
        health?.IncreaseHealth(
            data.MaxHealthIncrease * gainedLevels,
            data.CurrentHealthIncrease * gainedLevels);
        ApplyCombatStats();
    }

    private void ApplyCombatStats()
    {
        float multiplier = DamageMultiplier;
        weapon?.SetDamageMultiplier(multiplier);
        skills?.SetDamageMultiplier(multiplier);
        health?.SetArmor(baseArmor + data.GetArmorBonus(CurrentLevel));
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
