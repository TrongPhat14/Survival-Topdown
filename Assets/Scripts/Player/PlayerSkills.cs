using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ActorMotor))]
[RequireComponent(typeof(ActorFacing))]
[RequireComponent(typeof(Health))]
public class PlayerSkills : MonoBehaviour
{
    [Header("Skill Data")]
    [SerializeField] private BombSkillData bombData;
    [SerializeField] private DashSkillData dashData;

    [Header("Targets")]
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private AttackRangeIndicator rangeIndicatorTemplate;

    public event Action<float> BombCooldownStarted;
    public event Action<float> DashCooldownStarted;

    public float BombCooldownRemaining =>
        Mathf.Max(0f, nextBombReadyTime - Time.time);

    public float DashCooldownRemaining =>
        Mathf.Max(0f, nextDashReadyTime - Time.time);

    public float BombCooldown => bombData != null ? bombData.Cooldown : 0f;
    public float DashCooldown => dashData != null ? dashData.Cooldown : 0f;

    public bool IsDashing => dashRoutine != null;

    private Rigidbody body;
    private ActorMotor motor;
    private ActorFacing facing;
    private Health health;
    private GameInput gameInput;
    private Coroutine dashRoutine;
    private AttackRangeIndicator dashRangeIndicator;
    private VfxPool vfxPool;
    private float nextBombReadyTime;
    private float nextDashReadyTime;
    private float damageMultiplier;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        motor = GetComponent<ActorMotor>();
        facing = GetComponent<ActorFacing>();
        health = GetComponent<Health>();
        ResolveVfxPool();

        if (rangeIndicatorTemplate == null)
        {
            rangeIndicatorTemplate = GetComponentInChildren<AttackRangeIndicator>();
        }
    }

    private void OnEnable()
    {
        gameInput = GameInput.Instance;

        if (gameInput != null)
        {
            gameInput.BlastPressed += HandleBombPressed;
            gameInput.RepulsePressed += HandleDashPressed;
        }

        health.Died += HandleOwnerDied;
    }

    private void OnDisable()
    {
        if (gameInput != null)
        {
            gameInput.BlastPressed -= HandleBombPressed;
            gameInput.RepulsePressed -= HandleDashPressed;
            gameInput = null;
        }

        health.Died -= HandleOwnerDied;
        CancelDash();
    }

    public bool TryPlaceBomb()
    {
        if (bombData == null || health.IsDead || Time.time < nextBombReadyTime)
        {
            return false;
        }

        GameObject bombObject = CreateBombObject();
        TimedBomb bomb = bombObject.GetComponent<TimedBomb>();

        if (bomb == null)
        {
            bomb = bombObject.AddComponent<TimedBomb>();
        }

        bomb.Arm(
            bombData.FuseDuration,
            DamageCalculator.CalculatePlayerDamage(
                bombData.BaseDamage,
                damageMultiplier),
            bombData.ExplosionRadius,
            enemyLayers,
            rangeIndicatorTemplate,
            vfxPool,
            bombData.ExplosionEffectPrefab,
            bombData.ExplosionEffectScale);

        nextBombReadyTime = Time.time + bombData.Cooldown;
        BombCooldownStarted?.Invoke(bombData.Cooldown);
        return true;
    }

    public bool TryDash()
    {
        if (dashData == null || health.IsDead || IsDashing || Time.time < nextDashReadyTime)
        {
            return false;
        }

        Vector3 direction = transform.forward;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return false;
        }

        nextDashReadyTime = Time.time + dashData.Cooldown;
        DashCooldownStarted?.Invoke(dashData.Cooldown);
        dashRoutine = StartCoroutine(DashRoutine(direction.normalized));
        return true;
    }

    private void HandleBombPressed()
    {
        SoundManager.Play(SoundId.ClickButton);
        TryPlaceBomb();
    }

    private void HandleDashPressed()
    {
        SoundManager.Play(SoundId.ClickButton);
        TryDash();
    }

    private GameObject CreateBombObject()
    {
        if (bombData.BombPrefab != null)
        {
            GameObject prefabBombObject = Instantiate(
                bombData.BombPrefab,
                transform.position,
                Quaternion.identity);
            DisableBombColliders(prefabBombObject);
            return prefabBombObject;
        }

        GameObject bombObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bombObject.name = "Bomb";
        bombObject.transform.position = transform.position + Vector3.up * 0.35f;
        bombObject.transform.localScale = Vector3.one * 0.7f;

        DisableBombColliders(bombObject);
        return bombObject;
    }

    private static void DisableBombColliders(GameObject bombObject)
    {
        Collider[] colliders = bombObject.GetComponentsInChildren<Collider>(true);

        foreach (Collider bombCollider in colliders)
        {
            bombCollider.enabled = false;
        }
    }

    private IEnumerator DashRoutine(Vector3 direction)
    {
        motor.SetMoveDirection(Vector3.zero);
        motor.enabled = false;
        facing.enabled = false;
        dashRangeIndicator = AttackRangeIndicator.CreateSkillIndicator(
            rangeIndicatorTemplate,
            transform,
            dashData.ExplosionRadius);

        float elapsed = 0f;
        float dashSpeed = dashData.Distance / dashData.Duration;

        while (elapsed < dashData.Duration)
        {
            Vector3 velocity = direction * dashSpeed;
            velocity.y = body.linearVelocity.y;
            body.linearVelocity = velocity;

            yield return new WaitForFixedUpdate();
            elapsed += Time.fixedDeltaTime;
        }

        RestoreMovement();
        dashRoutine = null;
        DisposeDashIndicator();

        PlayExplosionEffect(dashData.ExplosionEffectPrefab, transform.position);
        SoundManager.PlayAt(SoundId.Bomb, transform.position);
        AreaDamageUtility.Apply(
            transform.position,
            dashData.ExplosionRadius,
            DamageCalculator.CalculatePlayerDamage(
                dashData.BaseDamage,
                damageMultiplier),
            enemyLayers);
    }

    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = Mathf.Max(0f, multiplier);
    }

    private void HandleOwnerDied()
    {
        CancelDash();
    }

    private void CancelDash()
    {
        if (dashRoutine == null)
        {
            return;
        }

        StopCoroutine(dashRoutine);
        dashRoutine = null;
        RestoreMovement();
        DisposeDashIndicator();
    }

    private void RestoreMovement()
    {
        Vector3 velocity = body.linearVelocity;
        velocity.x = 0f;
        velocity.z = 0f;
        body.linearVelocity = velocity;
        motor.SetMoveDirection(Vector3.zero);
        motor.enabled = true;
        facing.enabled = true;
    }

    private void DisposeDashIndicator()
    {
        dashRangeIndicator?.Dispose();
        dashRangeIndicator = null;
    }

    private void ResolveVfxPool()
    {
        GameObject bombEffect = bombData != null
            ? bombData.ExplosionEffectPrefab
            : null;
        GameObject dashEffect = dashData != null
            ? dashData.ExplosionEffectPrefab
            : null;

        if (bombEffect == null && dashEffect == null)
        {
            return;
        }

        vfxPool = VfxPool.FindOrCreate();

        if (bombEffect != null)
        {
            vfxPool.Register(bombEffect, 2, 8);
        }

        if (dashEffect != null)
        {
            vfxPool.Register(dashEffect, 2, 8);
        }
    }

    private void PlayExplosionEffect(GameObject effectPrefab, Vector3 position)
    {
        if (vfxPool == null || effectPrefab == null)
        {
            return;
        }

        vfxPool.Play(
            effectPrefab,
            position,
            effectPrefab.transform.rotation);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.35f, 0.1f, 0.35f);
        if (bombData != null)
        {
            Gizmos.DrawWireSphere(transform.position, bombData.ExplosionRadius);
        }

        if (dashData == null)
        {
            return;
        }

        Vector3 dashEnd = transform.position + transform.forward * dashData.Distance;
        Gizmos.color = new Color(0.1f, 0.8f, 1f, 0.35f);
        Gizmos.DrawLine(transform.position, dashEnd);
        Gizmos.DrawWireSphere(dashEnd, dashData.ExplosionRadius);
    }
}
