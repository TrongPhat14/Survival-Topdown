using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Health))]
public class PoisonStatus : MonoBehaviour
{
    private Health health;
    private Coroutine poisonRoutine;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnDisable()
    {
        ClearPoison();
    }

    public void ApplyPoison(
        float damagePerTick,
        float duration,
        float tickInterval)
    {
        if (health.IsDead || damagePerTick <= 0f)
        {
            return;
        }

        ClearPoison();
        health.TakeDamage(damagePerTick);

        if (!health.IsDead && duration > 0f && tickInterval > 0f)
        {
            poisonRoutine = StartCoroutine(PoisonRoutine(
                damagePerTick,
                duration,
                tickInterval));
        }
    }

    private IEnumerator PoisonRoutine(
        float damagePerTick,
        float duration,
        float tickInterval)
    {
        float elapsed = 0f;

        while (elapsed + tickInterval <= duration + Mathf.Epsilon)
        {
            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;

            if (health.IsDead)
            {
                break;
            }

            health.TakeDamage(damagePerTick);
        }

        poisonRoutine = null;
    }

    private void ClearPoison()
    {
        if (poisonRoutine == null)
        {
            return;
        }

        StopCoroutine(poisonRoutine);
        poisonRoutine = null;
    }
}
