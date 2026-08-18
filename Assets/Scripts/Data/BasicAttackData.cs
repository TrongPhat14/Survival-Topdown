using UnityEngine;

[CreateAssetMenu(fileName = "BasicAttackData", menuName = "Survival/Basic Attack Data")]
public class BasicAttackData : ScriptableObject
{
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private GameObject impactEffectPrefab;

    [Header("Shot")]
    [SerializeField, Min(0.05f)] private float fireInterval = 0.5f;
    [SerializeField, Min(1)] private int projectileCount = 3;
    [SerializeField, Range(0f, 90f)] private float spreadAngle = 30f;
    [SerializeField, Min(0f)] private float baseDamage = 10f;
    [SerializeField, Min(0.5f)] private float attackRange = 8f;

    [Header("Charges")]
    [SerializeField, Min(1)] private int maxCharges = 3;
    [SerializeField, Min(0.1f)] private float chargeRecoveryDuration = 3f;

    public Projectile ProjectilePrefab => projectilePrefab;
    public GameObject ImpactEffectPrefab => impactEffectPrefab;
    public float FireInterval => fireInterval;
    public int ProjectileCount => projectileCount;
    public float SpreadAngle => spreadAngle;
    public float BaseDamage => baseDamage;
    public float AttackRange => attackRange;
    public int MaxCharges => maxCharges;
    public float ChargeRecoveryDuration => chargeRecoveryDuration;
}
