using UnityEngine;

[RequireComponent(typeof(Health))]
public class DamagePopupEmitter : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Transform popupPoint;
    [SerializeField] private Vector3 positionOffset = new Vector3(0f, 2f, 0f);
    [SerializeField] private GameObject damagePopupPrefab;
    [SerializeField] private Color directDamageColor = new Color(1f, 0.03f, 0.03f, 1f);
    [SerializeField] private Color poisonDamageColor = new Color(0.45f, 1f, 0.12f, 1f);
    [SerializeField, Min(1)] private int initialPoolSize = 16;
    [SerializeField, Min(1)] private int maxPoolSize = 64;

    private DamagePopupPool popupPool;

    private void Awake()
    {
        if (health == null)
        {
            health = GetComponent<Health>();
        }

        ResolvePool();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.Damaged += HandleDamaged;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.Damaged -= HandleDamaged;
        }
    }

    private void HandleDamaged(float damage, DamageType damageType)
    {
        if (popupPool == null)
        {
            return;
        }

        Vector3 spawnPosition = popupPoint != null
            ? popupPoint.position
            : transform.position + positionOffset;

        spawnPosition += new Vector3(Random.Range(-0.15f, 0.15f), 0f, 0f);
        Color popupColor = damageType == DamageType.Poison
            ? poisonDamageColor
            : directDamageColor;
        popupPool.Show(damage, spawnPosition, popupColor);
    }

    private void ResolvePool()
    {
        popupPool = FindFirstObjectByType<DamagePopupPool>();

        if (popupPool == null)
        {
            GameObject poolRoot = GameObject.Find("PoolRoot");
            if (poolRoot == null)
            {
                poolRoot = new GameObject("PoolRoot");
            }

            popupPool = poolRoot.GetComponent<DamagePopupPool>();
            if (popupPool == null)
            {
                popupPool = poolRoot.AddComponent<DamagePopupPool>();
            }
        }

        popupPool.Initialize(damagePopupPrefab, initialPoolSize, maxPoolSize);

        if (!popupPool.IsInitialized)
        {
            popupPool = null;
        }
    }
}
