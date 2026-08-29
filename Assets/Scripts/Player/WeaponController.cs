using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerTargeting))]
public class WeaponController : MonoBehaviour
{
    private const int FixedMaxCharges = 3;

    [Header("References")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private BasicAttackData attackData;
    [SerializeField] private AttackRangeIndicator rangeIndicatorPrefab;

    [Header("Projectile Pool")]
    [SerializeField, Min(1)] private int initialPoolSize = 24;
    [SerializeField, Min(1)] private int maxPoolSize = 64;

    public event Action Fired;
    public event Action<int> ChargesChanged;

    public float AttackRange => attackData != null ? attackData.AttackRange : 0f;
    public int CurrentCharges => currentCharges;
    public int MaxCharges => attackData != null ? FixedMaxCharges : 0;
    public float ChargeRecoveryProgress =>
        attackData == null || currentCharges >= FixedMaxCharges
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
    private PlayerTargeting targeting;
    private PlayerController playerController;
    private AttackRangeIndicator attackRangeIndicator;

    private void Awake()
    {
        if (attackData == null)
        {
            Debug.LogError("BasicAttackData is not assigned.", this);
            enabled = false;
            return;
        }

        targeting = GetComponent<PlayerTargeting>();
        playerController = GetComponent<PlayerController>();
        currentCharges = FixedMaxCharges;
        attackRangeIndicator = AttackRangeIndicator.CreateSkillIndicator(
            rangeIndicatorPrefab,
            transform,
            AttackRange);
        attackRangeIndicator?.Hide();
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
            gameInput.AttackStarted += ShowAttackRange;
            gameInput.AttackPressed += HandleAttackPressed;
            gameInput.AttackReleased += HideAttackRange;
        }
    }

    private void OnDisable()
    {
        if (gameInput != null)
        {
            gameInput.AttackStarted -= ShowAttackRange;
            gameInput.AttackPressed -= HandleAttackPressed;
            gameInput.AttackReleased -= HideAttackRange;
            gameInput = null;
        }

        HideAttackRange();
    }

    private void OnDestroy()
    {
        if (attackRangeIndicator != null)
        {
            attackRangeIndicator.Dispose();
        }

        attackRangeIndicator = null;
    }

    private void HandleAttackPressed()
    {
        SoundManager.Play(SoundId.ClickButton);
        TryFire();
    }

    private void ShowAttackRange()
    {
        attackRangeIndicator?.Show(AttackRange);
    }

    private void HideAttackRange()
    {
        attackRangeIndicator?.Hide();
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
        Quaternion aimRotation = GetAimRotation(
            out Vector3 aimDirection,
            out Vector3 spawnPosition);

        if (aimDirection.sqrMagnitude > 0.01f)
        {
            playerController?.RequestAttackFacing(aimDirection);
        }

        for (int i = 0; i < attackData.ProjectileCount; i++)
        {
            Quaternion shotRotation = GetShotRotation(i, aimRotation);

            projectileFactory.Spawn(
                attackData.ProjectilePrefab,
                transform.root,
                spawnPosition,
                shotRotation,
                shotRotation * Vector3.forward,
                shotDamage,
                vfxPool,
                attackData.ImpactEffectPrefab);

            Debug.DrawRay(
                spawnPosition,
                shotRotation * Vector3.forward * 2f,
                Color.cyan,
                0.25f);
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
        if (attackData == null || currentCharges >= FixedMaxCharges)
        {
            chargeRecoveryTimer = 0f;
            return;
        }

        chargeRecoveryTimer += Time.deltaTime;

        while (chargeRecoveryTimer >= attackData.ChargeRecoveryDuration &&
               currentCharges < FixedMaxCharges)
        {
            chargeRecoveryTimer -= attackData.ChargeRecoveryDuration;
            currentCharges++;
            ChargesChanged?.Invoke(currentCharges);
        }

        if (currentCharges >= FixedMaxCharges)
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

    private Quaternion GetAimRotation(
        out Vector3 aimDirection,
        out Vector3 spawnPosition)
    {
        if (targeting != null &&
            targeting.TryGetAimDirection(
                transform.position,
                out Vector3 facingDirection))
        {
            Quaternion facingRotation = Quaternion.LookRotation(
                facingDirection,
                Vector3.up);
            spawnPosition = GetAimAlignedMuzzlePosition(facingRotation);
            aimDirection = facingDirection;

            Transform currentTarget = targeting.CurrentTarget;
            if (currentTarget != null)
            {
                Vector3 directionFromMuzzle =
                    currentTarget.position - spawnPosition;
                directionFromMuzzle.y = 0f;

                if (directionFromMuzzle.sqrMagnitude > 0.001f)
                {
                    aimDirection = directionFromMuzzle.normalized;
                }
            }

            return Quaternion.LookRotation(aimDirection, Vector3.up);
        }

        aimDirection = Vector3.zero;
        spawnPosition = muzzle.position;
        return muzzle.rotation;
    }

    private Vector3 GetAimAlignedMuzzlePosition(Quaternion aimRotation)
    {
        Vector3 currentMuzzleOffset = muzzle.position - transform.position;
        Quaternion rotationDelta =
            aimRotation * Quaternion.Inverse(transform.rotation);

        return transform.position + rotationDelta * currentMuzzleOffset;
    }

    private Quaternion GetShotRotation(int shotIndex, Quaternion aimRotation)
    {
        if (attackData.ProjectileCount <= 1)
        {
            return aimRotation;
        }

        float t = shotIndex / (attackData.ProjectileCount - 1f);
        float yawOffset = Mathf.Lerp(
            -attackData.SpreadAngle * 0.5f,
            attackData.SpreadAngle * 0.5f,
            t);
        return Quaternion.AngleAxis(yawOffset, Vector3.up) * aimRotation;
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
