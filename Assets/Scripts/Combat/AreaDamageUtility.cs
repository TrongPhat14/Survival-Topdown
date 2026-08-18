using System.Collections.Generic;
using UnityEngine;

public static class AreaDamageUtility
{
    private static readonly Collider[] HitBuffer = new Collider[128];
    private static readonly HashSet<Health> DamagedTargets = new HashSet<Health>();

    public static int Apply(
        Vector3 center,
        float radius,
        float damage,
        LayerMask targetLayers)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(
            center,
            radius,
            HitBuffer,
            targetLayers,
            QueryTriggerInteraction.Collide);

        DamagedTargets.Clear();
        int damagedCount = 0;

        for (int i = 0; i < hitCount; i++)
        {
            Health health = HitBuffer[i].GetComponentInParent<Health>();
            if (health == null || !DamagedTargets.Add(health))
            {
                continue;
            }

            if (health.TakeDamage(damage))
            {
                damagedCount++;
            }
        }

        return damagedCount;
    }
}
