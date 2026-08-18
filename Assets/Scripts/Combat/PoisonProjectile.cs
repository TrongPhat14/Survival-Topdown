using UnityEngine;

public class PoisonProjectile : Projectile
{
    [SerializeField, Min(0f)] private float poisonDuration = 3f;
    [SerializeField, Min(0.05f)] private float tickInterval = 1f;

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
