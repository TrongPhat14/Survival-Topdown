using UnityEngine;

public class PoisonProjectile : Projectile
{
    [SerializeField, Min(0f)] private float poisonDuration = 3f;
    [SerializeField, Min(0.05f)] private float tickInterval = 1f;
    [SerializeField, Min(0f)] private float homingTurnSpeed = 240f;

    private Transform homingTarget;

    protected override void OnEnable()
    {
        homingTarget = null;
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        homingTarget = null;
        base.OnDisable();
    }

    public void SetHomingTarget(Transform target)
    {
        homingTarget = target;
    }

    private void FixedUpdate()
    {
        if (homingTarget == null || Body == null || homingTurnSpeed <= 0f)
        {
            return;
        }

        Vector3 targetDirection = homingTarget.position - transform.position;
        targetDirection.y = 0f;

        if (targetDirection.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Vector3 currentDirection = Body.linearVelocity.sqrMagnitude > 0.001f
            ? Body.linearVelocity.normalized
            : transform.forward;
        Vector3 homingDirection = Vector3.RotateTowards(
            currentDirection,
            targetDirection.normalized,
            homingTurnSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime,
            0f);

        Body.linearVelocity = homingDirection * MoveSpeed;
        Body.MoveRotation(Quaternion.LookRotation(homingDirection, Vector3.up));
    }

    protected override bool ApplyHit(Collider other)
    {
        PoisonStatus poisonStatus = other.GetComponentInParent<PoisonStatus>();
        if (poisonStatus == null)
        {
            return false;
        }

        poisonStatus.ApplyPoison(
            CurrentDamage,
            poisonDuration,
            tickInterval);
        return true;
    }
}
