using System;
using UnityEngine;

[DisallowMultipleComponent]
public class WeaponController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private BasicAttackData attackData;

    [Header("Projectile Pool")]
    [SerializeField, Min(1)] private int initialPoolSize = 24;
    [SerializeField, Min(1)] private int maxPoolSize = 64;

    public event Action Fired;
    public event Action<int> ChargesChanged;

    public float AttackRange => attackData != null ? attackData.AttackRange : 0f;
    public int CurrentCharges => currentCharges;
    public int MaxCharges => attackData != null ? attackData.MaxCharges : 0;
    public float ChargeRecoveryProgress =>
        attackData == null || currentCharges >= attackData.MaxCharges
            ? 0f
            : Mathf.Clamp01(chargeRecoveryTimer / attackData.ChargeRecoveryDuration);
    public float DamageMultiplier => damageMultiplier;

    private float nextFireTime;
    private float chargeRecoveryTimer;
    private float damageMultiplier;
    private int currentCharges;
    private GameInput gameInput;
    private ProjectileFactory projectileFactory;
    private VfxPool vfxPool;

    private void Awake()
    {
        if (attackData == null)
        {
            Debug.LogError("BasicAttackData is not assigned.", this);
            enabled = false;
            return;
        }

        currentCharges = attackData.MaxCharges;
        ResolveProjectileFactory();
        ResolveImpactVfxPool();
    }

    private void Update()
    {
        RecoverCharges();
    }

    private void OnEnable()
    {
        gameInput = GameInput.Instance;

        if (gameInput != null)
        {
            gameInput.AttackPressed += HandleAttackPressed;
        }
    }

    private void OnDisable()
    {
        if (gameInput != null)
        {
            gameInput.AttackPressed -= HandleAttackPressed;
            gameInput = null;
        }
    }

    private void HandleAttackPressed()
    {
        SoundManager.Play(SoundId.ClickButton);
        TryFire();
    }

    public bool TryFire()
    {
        if (muzzle == null ||
            projectileFactory == null ||
            currentCharges <= 0 ||
            Time.time < nextFireTime)
        {
            return false;
        }

        nextFireTime = Time.time + attackData.FireInterval;
        currentCharges--;
        ChargesChanged?.Invoke(currentCharges);

        float shotDamage = CalculateDamage(attackData.BaseDamage);

        for (int i = 0; i < attackData.ProjectileCount; i++)
        {
            Quaternion shotRotation = GetShotRotation(i);

            projectileFactory.Spawn(
                attackData.ProjectilePrefab,
                transform.root,
                muzzle.position,
                shotRotation,
                shotRotation * Vector3.forward,
                shotDamage,
                vfxPool,
                attackData.ImpactEffectPrefab);

            Debug.DrawRay(muzzle.position, shotRotation * Vector3.forward * 2f, Color.cyan, 0.25f);
        }

        Fired?.Invoke();
        return true;
    }

    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = Mathf.Max(0f, multiplier);
    }

    public float CalculateDamage(float baseDamage)
    {
        return DamageCalculator.CalculatePlayerDamage(baseDamage, damageMultiplier);
    }

    private void RecoverCharges()
    {
        if (attackData == null || currentCharges >= attackData.MaxCharges)
        {
            chargeRecoveryTimer = 0f;
            return;
        }

        chargeRecoveryTimer += Time.deltaTime;

        while (chargeRecoveryTimer >= attackData.ChargeRecoveryDuration &&
               currentCharges < attackData.MaxCharges)
        {
            chargeRecoveryTimer -= attackData.ChargeRecoveryDuration;
            currentCharges++;
            ChargesChanged?.Invoke(currentCharges);
        }

        if (currentCharges >= attackData.MaxCharges)
        {
            chargeRecoveryTimer = 0f;
        }
    }

    private void ResolveProjectileFactory()
    {
        projectileFactory = ProjectileFactory.FindOrCreate();
        if (!projectileFactory.Register(
                attackData.ProjectilePrefab,
                initialPoolSize,
                maxPoolSize))
        {
            projectileFactory = null;
        }
    }

    private void ResolveImpactVfxPool()
    {
        if (attackData.ImpactEffectPrefab == null)
        {
            return;
        }

        vfxPool = VfxPool.FindOrCreate();
        if (!vfxPool.Register(attackData.ImpactEffectPrefab, 8, 32))
        {
            vfxPool = null;
        }
    }

    private Quaternion GetShotRotation(int shotIndex)
    {
        if (attackData.ProjectileCount <= 1)
        {
            return muzzle.rotation;
        }

        float t = shotIndex / (attackData.ProjectileCount - 1f);
        float yawOffset = Mathf.Lerp(
            -attackData.SpreadAngle * 0.5f,
            attackData.SpreadAngle * 0.5f,
            t);
        return Quaternion.AngleAxis(yawOffset, Vector3.up) * muzzle.rotation;
    }

    private void OnDrawGizmosSelected()
    {
        if (muzzle == null)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(muzzle.position, 0.06f);
        Gizmos.DrawRay(muzzle.position, muzzle.forward * 0.8f);
    }
}
