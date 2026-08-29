using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(WeaponController))]
public sealed class PlayerTargeting : MonoBehaviour
{
    public enum TargetPriority
    {
        Nearest,
        LowestHealth
    }

    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private TargetPriority targetPriority = TargetPriority.Nearest;
    [SerializeField, Min(0.02f)] private float refreshInterval = 0.1f;

    private readonly Collider[] targetBuffer = new Collider[64];

    private WeaponController weapon;
    private Transform currentTarget;
    private Health currentTargetHealth;
    private float nextRefreshTime;

    public Transform CurrentTarget => IsCurrentTargetValid()
        ? currentTarget
        : null;

    private void Awake()
    {
        weapon = GetComponent<WeaponController>();

        if (enemyLayers.value == 0)
        {
            enemyLayers = LayerMask.GetMask("Enemy");
        }
    }

    private void OnEnable()
    {
        nextRefreshTime = 0f;
        ClearTarget();
    }

    private void OnDisable()
    {
        ClearTarget();
    }

    public bool TryGetAimDirection(Vector3 origin, out Vector3 direction)
    {
        RefreshTargetIfNeeded();

        if (!IsCurrentTargetValid())
        {
            direction = Vector3.zero;
            return false;
        }

        direction = currentTarget.position - origin;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            direction = Vector3.zero;
            return false;
        }

        direction.Normalize();
        return true;
    }

    private void RefreshTargetIfNeeded()
    {
        if (weapon == null || weapon.AttackRange <= 0f)
        {
            ClearTarget();
            return;
        }

        if (IsCurrentTargetValid() && Time.time < nextRefreshTime)
        {
            return;
        }

        FindBestTarget();
        nextRefreshTime = Time.time + refreshInterval;
    }

    private void FindBestTarget()
    {
        int targetCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            weapon.AttackRange,
            targetBuffer,
            enemyLayers,
            QueryTriggerInteraction.Ignore);

        Health bestHealth = null;
        Transform bestTarget = null;
        float bestDistanceSqr = float.PositiveInfinity;
        float bestCurrentHealth = float.PositiveInfinity;

        for (int i = 0; i < targetCount; i++)
        {
            Collider candidateCollider = targetBuffer[i];
            if (candidateCollider == null)
            {
                continue;
            }

            Health candidateHealth = candidateCollider.GetComponentInParent<Health>();
            if (candidateHealth == null || candidateHealth.IsDead)
            {
                continue;
            }

            Transform candidateTarget = candidateHealth.transform;
            Vector3 offset = candidateTarget.position - transform.position;
            offset.y = 0f;
            float distanceSqr = offset.sqrMagnitude;

            if (!IsBetterTarget(
                    candidateHealth,
                    distanceSqr,
                    bestCurrentHealth,
                    bestDistanceSqr))
            {
                continue;
            }

            bestHealth = candidateHealth;
            bestTarget = candidateTarget;
            bestCurrentHealth = candidateHealth.CurrentHealth;
            bestDistanceSqr = distanceSqr;
        }

        currentTargetHealth = bestHealth;
        currentTarget = bestTarget;
    }

    private bool IsBetterTarget(
        Health candidateHealth,
        float candidateDistanceSqr,
        float bestCurrentHealth,
        float bestDistanceSqr)
    {
        if (targetPriority == TargetPriority.Nearest)
        {
            return candidateDistanceSqr < bestDistanceSqr;
        }

        if (candidateHealth.CurrentHealth < bestCurrentHealth)
        {
            return true;
        }

        return Mathf.Approximately(candidateHealth.CurrentHealth, bestCurrentHealth) &&
               candidateDistanceSqr < bestDistanceSqr;
    }

    private bool IsCurrentTargetValid()
    {
        if (currentTarget == null ||
            currentTargetHealth == null ||
            currentTargetHealth.IsDead ||
            weapon == null)
        {
            return false;
        }

        Vector3 offset = currentTarget.position - transform.position;
        offset.y = 0f;
        float range = weapon.AttackRange;
        return offset.sqrMagnitude <= range * range;
    }

    private void ClearTarget()
    {
        currentTarget = null;
        currentTargetHealth = null;
    }

    private void OnDrawGizmosSelected()
    {
        WeaponController currentWeapon = weapon != null
            ? weapon
            : GetComponent<WeaponController>();

        if (currentWeapon == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, currentWeapon.AttackRange);
    }
}
