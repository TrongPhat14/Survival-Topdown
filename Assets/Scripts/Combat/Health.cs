using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField, Min(1f)] private float maxHealth = 100f;
    [SerializeField, Min(0f)] private float armor;
    private float currentHealth;
    private bool isPlayer;

    public event Action<float, float> HealthChanged;
    public event Action<float> Damaged;
    public event Action Died;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float Armor => armor;
    public bool IsDead => CurrentHealth <= 0f;

    private void Awake()
    {
        isPlayer = GetComponent<PlayerController>() != null;
        ResetHealth();
    }

    public bool TakeDamage(float damage)
    {
        if (damage <= 0f || IsDead)
        {
            return false;
        }

        float mitigatedDamage = Mathf.Max(0f, damage - armor);
        if (mitigatedDamage <= 0f)
        {
            return false;
        }

        float appliedDamage = Mathf.Min(mitigatedDamage, currentHealth);
        currentHealth = Mathf.Max(0f, currentHealth - appliedDamage);
        SoundManager.PlayAt(SoundId.Hit, transform.position);
        HealthChanged?.Invoke(currentHealth, maxHealth);
        Damaged?.Invoke(appliedDamage);

        if (isPlayer)
        {
            CameraShake.PlayPlayerDamaged();
        }

        if (IsDead)
        {
            Died?.Invoke();
        }

        return true;
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetMaxHealth(float value, bool refill = true)
    {
        maxHealth = Mathf.Max(1f, value);

        if (refill)
        {
            ResetHealth();
            return;
        }

        currentHealth = Mathf.Min(currentHealth, maxHealth);
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void IncreaseHealth(float maxIncrease, float currentIncrease)
    {
        maxHealth = Mathf.Max(1f, maxHealth + Mathf.Max(0f, maxIncrease));
        currentHealth = Mathf.Min(
            maxHealth,
            currentHealth + Mathf.Max(0f, currentIncrease));
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetArmor(float value)
    {
        armor = Mathf.Max(0f, value);
    }
}
