using UnityEngine;

public class RangedEnemyController : EnemyController
{
    private static readonly int RangedAttackStateHash =
        Animator.StringToHash("Base Layer.Attack 1");

    [SerializeField] private Transform muzzle;
    [SerializeField] private PoisonProjectile projectilePrefab;
    [SerializeField, Min(1)] private int initialPoolSize = 8;
    [SerializeField, Min(1)] private int maxPoolSize = 32;

    private ProjectileFactory projectileFactory;

    protected override int AttackAnimationStateHash => RangedAttackStateHash;

    protected override void Awake()
    {
        base.Awake();

        if (!enabled || projectilePrefab == null)
        {
            return;
        }

        projectileFactory = ProjectileFactory.FindOrCreate();
        if (!projectileFactory.Register(
                projectilePrefab,
                initialPoolSize,
                maxPoolSize))
        {
            projectileFactory = null;
        }
    }

    protected override void PerformAttack()
    {
        if (muzzle == null || projectilePrefab == null || projectileFactory == null)
        {
            Debug.LogWarning("Ranged enemy is missing its muzzle or poison projectile.", this);
            return;
        }

        projectileFactory.Spawn(
            projectilePrefab,
            transform.root,
            muzzle.position,
            muzzle.rotation,
            muzzle.forward,
            Data.AttackDamage);
    }
}
