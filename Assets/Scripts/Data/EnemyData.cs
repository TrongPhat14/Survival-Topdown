using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Survival/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [SerializeField] private string displayName = "Enemy";

    [Header("Stats")]
    [SerializeField, Min(1f)] private float maxHealth = 100f;
    [SerializeField, Min(0f)] private float moveSpeed = 1.5f;
    [SerializeField, Min(0.05f)] private float repathInterval = 0.2f;

    [Header("Attack")]
    [SerializeField, Min(0.1f)] private float attackRange = 2.2f;
    [SerializeField, Min(0.1f)] private float attackDuration = 2f;
    [SerializeField, Min(0f)] private float damageDelay = 0.8f;
    [SerializeField, Min(0f)] private float attackDamage = 10f;

    [Header("Death")]
    [SerializeField, Min(0f)] private float destroyDelay = 1.2f;

    [Header("Reward")]
    [SerializeField, Min(0)] private int experienceReward = 20;

    public string DisplayName => displayName;
    public float MaxHealth => maxHealth;
    public float MoveSpeed => moveSpeed;
    public float RepathInterval => repathInterval;
    public float AttackRange => attackRange;
    public float AttackDuration => attackDuration;
    public float DamageDelay => damageDelay;
    public float AttackDamage => attackDamage;
    public float DestroyDelay => destroyDelay;
    public int ExperienceReward => experienceReward;
}
