using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[DisallowMultipleComponent]
public class ProjectileFactory : MonoBehaviour
{
    private sealed class PoolEntry
    {
        public Projectile Prefab;
        public Transform Root;
        public ObjectPool<Projectile> Pool;
    }

    private readonly Dictionary<Projectile, PoolEntry> pools = new();

    public static ProjectileFactory FindOrCreate()
    {
        ProjectileFactory factory = FindFirstObjectByType<ProjectileFactory>();
        if (factory != null)
        {
            return factory;
        }

        GameObject poolRoot = GameObject.Find("PoolRoot");
        if (poolRoot == null)
        {
            poolRoot = new GameObject("PoolRoot");
        }

        factory = poolRoot.GetComponent<ProjectileFactory>();
        return factory != null
            ? factory
            : poolRoot.AddComponent<ProjectileFactory>();
    }

    public bool Register(Projectile prefab, int initialSize, int maxSize)
    {
        if (prefab == null)
        {
            Debug.LogError("ProjectileFactory requires a projectile prefab.", this);
            return false;
        }

        if (pools.ContainsKey(prefab))
        {
            return true;
        }

        initialSize = Mathf.Max(1, initialSize);
        maxSize = Mathf.Max(initialSize, maxSize);

        PoolEntry entry = new PoolEntry
        {
            Prefab = prefab
        };

        GameObject poolObject = new GameObject($"{prefab.name} Pool");
        entry.Root = poolObject.transform;
        entry.Root.SetParent(transform, false);
        entry.Pool = new ObjectPool<Projectile>(
            () => CreateProjectile(entry),
            null,
            projectile => ReturnToPool(entry, projectile),
            DestroyProjectile,
            false,
            initialSize,
            maxSize);

        pools.Add(prefab, entry);
        Prewarm(entry, initialSize);
        return true;
    }

    public Projectile Spawn(
        Projectile prefab,
        Transform owner,
        Vector3 position,
        Quaternion rotation,
        Vector3 direction,
        float damage,
        VfxPool vfxPool = null,
        GameObject impactEffectPrefab = null)
    {
        if (prefab == null || !pools.TryGetValue(prefab, out PoolEntry entry))
        {
            Debug.LogError("Projectile prefab is not registered in the factory.", this);
            return null;
        }

        Projectile projectile = entry.Pool.Get();
        projectile.transform.SetPositionAndRotation(position, rotation);
        projectile.gameObject.SetActive(true);
        projectile.Launch(
            owner,
            direction,
            damage,
            vfxPool,
            impactEffectPrefab);
        return projectile;
    }

    public void Release(Projectile prefab, Projectile projectile)
    {
        if (projectile == null)
        {
            return;
        }

        if (prefab != null && pools.TryGetValue(prefab, out PoolEntry entry))
        {
            entry.Pool.Release(projectile);
            return;
        }

        Destroy(projectile.gameObject);
    }

    private Projectile CreateProjectile(PoolEntry entry)
    {
        Projectile projectile = Instantiate(entry.Prefab, entry.Root);
        projectile.SetFactory(this, entry.Prefab);
        projectile.gameObject.SetActive(false);
        return projectile;
    }

    private static void ReturnToPool(PoolEntry entry, Projectile projectile)
    {
        projectile.transform.SetParent(entry.Root, false);
        projectile.gameObject.SetActive(false);
    }

    private static void DestroyProjectile(Projectile projectile)
    {
        if (projectile != null)
        {
            Destroy(projectile.gameObject);
        }
    }

    private static void Prewarm(PoolEntry entry, int count)
    {
        List<Projectile> projectiles = new List<Projectile>(count);

        for (int i = 0; i < count; i++)
        {
            projectiles.Add(entry.Pool.Get());
        }

        for (int i = 0; i < projectiles.Count; i++)
        {
            entry.Pool.Release(projectiles[i]);
        }
    }

    private void OnDestroy()
    {
        foreach (PoolEntry entry in pools.Values)
        {
            entry.Pool.Clear();
        }

        pools.Clear();
    }
}
