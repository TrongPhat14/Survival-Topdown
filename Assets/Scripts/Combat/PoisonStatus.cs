using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Health))]
public class PoisonStatus : MonoBehaviour
{
    private Health health;
    private Coroutine poisonRoutine;
    private float poisonEndTime;

    public event Action PoisonStarted;
    public event Action PoisonEnded;

    public bool IsPoisoned { get; private set; }
    public float RemainingDuration => IsPoisoned
        ? Mathf.Max(0f, poisonEndTime - Time.time)
        : 0f;

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

        ClearPoison(false);
        BeginPoison(duration);
        health.TakeDamage(damagePerTick, DamageType.Poison);

        if (!health.IsDead && duration > 0f && tickInterval > 0f)
        {
            poisonRoutine = StartCoroutine(PoisonRoutine(
                damagePerTick,
                duration,
                tickInterval));

            return;
        }

        EndPoison();
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

            health.TakeDamage(damagePerTick, DamageType.Poison);
        }

        poisonRoutine = null;
        EndPoison();
    }

    private void BeginPoison(float duration)
    {
        IsPoisoned = true;
        poisonEndTime = Time.time + Mathf.Max(0f, duration);
        PoisonStarted?.Invoke();
    }

    private void EndPoison()
    {
        if (!IsPoisoned)
        {
            return;
        }

        IsPoisoned = false;
        poisonEndTime = 0f;
        PoisonEnded?.Invoke();
    }

    private void ClearPoison(bool notifyEnded = true)
    {
        if (poisonRoutine != null)
        {
            StopCoroutine(poisonRoutine);
            poisonRoutine = null;
        }

        if (notifyEnded)
        {
            EndPoison();
        }
    }
}
