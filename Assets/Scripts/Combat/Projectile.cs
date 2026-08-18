using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float speed = 12f;
    [SerializeField, Min(0.1f)] private float lifetime = 2.5f;
    [SerializeField] private LayerMask hitLayers;

    private Rigidbody body;
    private ProjectileFactory projectileFactory;
    private Projectile sourcePrefab;
    private Transform ownerRoot;
    private VfxPool vfxPool;
    private GameObject impactEffectPrefab;
    private Coroutine lifetimeCoroutine;
    private float currentDamage;
    private bool isReleased;

    protected float CurrentDamage => currentDamage;

    protected virtual void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    protected virtual void OnEnable()
    {
        ownerRoot = null;
        vfxPool = null;
        impactEffectPrefab = null;
        currentDamage = 0f;
        isReleased = false;
        lifetimeCoroutine = StartCoroutine(LifetimeRoutine());
    }

    protected virtual void OnDisable()
    {
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }

        if (body != null)
        {
            body.linearVelocity = Vector3.zero;
        }
    }

    public void Launch(
        Transform owner,
        Vector3 direction,
        float launchDamage,
        VfxPool effectPool = null,
        GameObject hitEffectPrefab = null)
    {
        ownerRoot = owner;
        vfxPool = effectPool;
        impactEffectPrefab = hitEffectPrefab;
        currentDamage = Mathf.Max(0f, launchDamage);
        direction.y = 0f;

        if (body == null || direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Vector3 normalizedDirection = direction.normalized;
        transform.rotation = Quaternion.LookRotation(normalizedDirection, Vector3.up);
        body.linearVelocity = normalizedDirection * speed;
    }

    public void SetFactory(ProjectileFactory factory, Projectile prefab)
    {
        projectileFactory = factory;
        sourcePrefab = prefab;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root == ownerRoot)
        {
            return;
        }

        int otherLayerMask = 1 << other.gameObject.layer;

        if ((hitLayers.value & otherLayerMask) == 0)
        {
            return;
        }

        bool damageApplied = ApplyHit(other);
        if (damageApplied &&
            ownerRoot != null &&
            ownerRoot.GetComponent<PlayerController>() != null)
        {
            CameraShake.PlayPlayerHitEnemy();
        }

        if (damageApplied && vfxPool != null && impactEffectPrefab != null)
        {
            Vector3 impactPosition = other.ClosestPoint(transform.position);
            vfxPool.Play(
                impactEffectPrefab,
                impactPosition,
                impactEffectPrefab.transform.rotation);
        }

        Release();
    }

    protected virtual bool ApplyHit(Collider other)
    {
        Health health = other.GetComponentInParent<Health>();
        return health != null && health.TakeDamage(currentDamage);
    }

    private void Release()
    {
        if (isReleased)
        {
            return;
        }

        isReleased = true;

        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }

        if (projectileFactory != null)
        {
            projectileFactory.Release(sourcePrefab, this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(lifetime);
        lifetimeCoroutine = null;
        Release();
    }
}
