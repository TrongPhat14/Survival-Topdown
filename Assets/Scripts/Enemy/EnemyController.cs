using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyMotor))]
[RequireComponent(typeof(Health))]
public class EnemyController : MonoBehaviour
{
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int IdleStateHash = Animator.StringToHash("Base Layer.IDLE");
    private static readonly int MeleeAttackStateHash = Animator.StringToHash("Base Layer.Attack 2");
    private static readonly int DieStateHash = Animator.StringToHash("Base Layer.Die");
    private static readonly int VictoryStateHash = Animator.StringToHash("Base Layer.Victory");

    [SerializeField] private EnemyData data;
    [SerializeField] private Transform target;
    [SerializeField] private Animator animator;

    private EnemyMotor motor;
    private Health health;
    private Collider bodyCollider;
    private Health targetHealth;
    private float repathTimer;
    private bool isAttacking;
    private bool isDead;
    private bool playerDefeated;
    private EnemyFactory enemyFactory;
    private EnemyController sourcePrefab;

    protected EnemyData Data => data;
    protected virtual int AttackAnimationStateHash => MeleeAttackStateHash;
    public int ExperienceReward => data != null ? data.ExperienceReward : 0;

    protected virtual void Awake()
    {
        motor = GetComponent<EnemyMotor>();
        health = GetComponent<Health>();
        bodyCollider = GetComponent<Collider>();

        if (data == null)
        {
            Debug.LogError("EnemyData is not assigned.", this);
            enabled = false;
            return;
        }

        health.SetMaxHealth(data.MaxHealth);
        motor.Configure(data.MoveSpeed, data.AttackRange);

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    protected virtual void OnEnable()
    {
        health.Died += HandleDied;
        PlayerController.PlayerDied += HandlePlayerDied;
        motor.EnableAgent();

        if (PlayerController.IsPlayerDead)
        {
            HandlePlayerDied();
        }
        else if (animator != null && !isDead)
        {
            animator.Play(IdleStateHash, 0, 0f);
        }
    }

    protected virtual void OnDisable()
    {
        health.Died -= HandleDied;
        PlayerController.PlayerDied -= HandlePlayerDied;
    }

    protected virtual void Start()
    {
        ResolveTarget();
    }

    protected virtual void Update()
    {
        if (isDead || playerDefeated)
        {
            return;
        }

        if (animator != null)
        {
            animator.SetBool(IsMovingHash, motor.IsMoving);
        }

        if (target == null)
        {
            motor.Stop();
            return;
        }

        if (GetFlatDistanceToTarget() <= data.AttackRange)
        {
            motor.Stop();
            FaceTarget();

            if (!isAttacking)
            {
                StartCoroutine(AttackRoutine());
            }

            return;
        }

        if (isAttacking)
        {
            return;
        }

        repathTimer -= Time.deltaTime;
        if (repathTimer > 0f)
        {
            return;
        }

        repathTimer = data.RepathInterval;
        motor.SetDestination(target.position);
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        if (animator != null)
        {
            animator.SetBool(IsMovingHash, false);
            animator.CrossFadeInFixedTime(AttackAnimationStateHash, 0.08f);
        }

        yield return new WaitForSeconds(Mathf.Min(
            data.DamageDelay,
            data.AttackDuration));

        if (!isDead &&
            target != null &&
            GetFlatDistanceToTarget() <= data.AttackRange)
        {
            PerformAttack();
        }

        float remainingDuration = Mathf.Max(
            0f,
            data.AttackDuration - data.DamageDelay);
        yield return new WaitForSeconds(remainingDuration);

        if (!isDead && animator != null)
        {
            animator.CrossFadeInFixedTime(IdleStateHash, 0.08f);
        }

        isAttacking = false;
        repathTimer = 0f;
    }

    protected virtual void PerformAttack()
    {
        targetHealth?.TakeDamage(data.AttackDamage);
    }

    internal void PrepareForSpawn(EnemyFactory factory, EnemyController prefab)
    {
        enemyFactory = factory;
        sourcePrefab = prefab;
        isDead = false;
        isAttacking = false;
        playerDefeated = false;
        repathTimer = 0f;
        StopAllCoroutines();

        if (bodyCollider != null)
        {
            bodyCollider.enabled = true;
        }

        health.SetMaxHealth(data.MaxHealth);
        motor.Configure(data.MoveSpeed, data.AttackRange);
        ResolveTarget();
    }

    private void HandleDied()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        isAttacking = false;
        StopAllCoroutines();
        motor.Stop();
        motor.DisableAgent();

        if (bodyCollider != null)
        {
            bodyCollider.enabled = false;
        }

        if (animator != null)
        {
            animator.SetBool(IsMovingHash, false);
            animator.CrossFadeInFixedTime(DieStateHash, 0.08f);
        }

        if (enemyFactory == null)
        {
            Destroy(gameObject, data.DestroyDelay);
            return;
        }

        enemyFactory.NotifyDefeated(this);
        StartCoroutine(ReturnToPoolAfterDeath());
    }

    private void HandlePlayerDied()
    {
        if (isDead || playerDefeated)
        {
            return;
        }

        playerDefeated = true;
        isAttacking = false;
        StopAllCoroutines();
        motor.Stop();

        if (animator != null)
        {
            animator.SetBool(IsMovingHash, false);
            animator.CrossFadeInFixedTime(VictoryStateHash, 0.08f);
        }
    }

    private IEnumerator ReturnToPoolAfterDeath()
    {
        if (data.DestroyDelay > 0f)
        {
            yield return new WaitForSeconds(data.DestroyDelay);
        }

        enemyFactory.Release(sourcePrefab, this);
    }

    private void ResolveTarget()
    {
        if (target != null && !target.IsChildOf(transform))
        {
            targetHealth = target.GetComponent<Health>();
            return;
        }

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            target = player.transform;
            targetHealth = player.GetComponent<Health>();
        }
    }

    private float GetFlatDistanceToTarget()
    {
        Vector3 offset = target.position - transform.position;
        offset.y = 0f;
        return offset.magnitude;
    }

    private void FaceTarget()
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
    }
}
