using UnityEngine;

public static class DamageCalculator
{
    public static float CalculatePlayerDamage(
        float baseDamage,
        float damageMultiplier)
    {
        float validBaseDamage = Mathf.Max(0f, baseDamage);
        float validMultiplier = Mathf.Max(0f, damageMultiplier);
        return validBaseDamage * (1f + validMultiplier);
    }
}
