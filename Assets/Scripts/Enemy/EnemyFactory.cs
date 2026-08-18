using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

[DisallowMultipleComponent]
public class EnemyFactory : MonoBehaviour
{
    private sealed class PoolEntry
    {
        public EnemyController Prefab;
        public Transform Root;
        public ObjectPool<EnemyController> Pool;
    }

    [SerializeField, Min(1)] private int initialPoolSize = 4;
    [SerializeField, Min(1)] private int maxPoolSize = 64;
    [SerializeField, Min(0.1f)] private float navMeshSampleRadius = 3f;

    [Header("Spawn VFX")]
    [SerializeField] private GameObject spawnEffectPrefab;
    [SerializeField] private Vector3 spawnEffectOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField, Min(1)] private int spawnEffectInitialPoolSize = 4;
    [SerializeField, Min(1)] private int spawnEffectMaxPoolSize = 16;

    private readonly Dictionary<EnemyController, PoolEntry> pools = new();
    private readonly HashSet<EnemyController> activeEnemies = new();
    private VfxPool vfxPool;

    public event Action<EnemyController> EnemyDefeated;

    private void Awake()
    {
        if (spawnEffectPrefab == null)
        {
            return;
        }

        vfxPool = VfxPool.FindOrCreate();
        if (!vfxPool.Register(
                spawnEffectPrefab,
                spawnEffectInitialPoolSize,
                spawnEffectMaxPoolSize))
        {
            vfxPool = null;
        }
    }

    public bool Register(EnemyController prefab)
    {
        if (prefab == null)
        {
            return false;
        }

        if (pools.ContainsKey(prefab))
        {
            return true;
        }

        int initialSize = Mathf.Max(1, initialPoolSize);
        int maximumSize = Mathf.Max(initialSize, maxPoolSize);
        PoolEntry entry = new PoolEntry
        {
            Prefab = prefab
        };

        GameObject poolObject = new GameObject($"{prefab.name} Pool");
        entry.Root = poolObject.transform;
        entry.Root.SetParent(transform, false);
        entry.Pool = new ObjectPool<EnemyController>(
            () => CreateEnemy(entry),
            null,
            enemy => ReturnToPool(entry, enemy),
            DestroyEnemy,
            false,
            initialSize,
            maximumSize);

        pools.Add(prefab, entry);
        Prewarm(entry, initialSize);
        return true;
    }

    public EnemyController Spawn(
        EnemyController prefab,
        Vector3 position,
        Quaternion rotation)
    {
        if (!Register(prefab) || !pools.TryGetValue(prefab, out PoolEntry entry))
        {
            Debug.LogError("EnemyFactory requires a valid enemy prefab.", this);
            return null;
        }

        if (NavMesh.SamplePosition(
                position,
                out NavMeshHit hit,
                navMeshSampleRadius,
                NavMesh.AllAreas))
        {
            position = hit.position;
        }

        EnemyController enemy = entry.Pool.Get();
        enemy.transform.SetPositionAndRotation(position, rotation);
        enemy.PrepareForSpawn(this, entry.Prefab);
        activeEnemies.Add(enemy);
        enemy.gameObject.SetActive(true);
        PlaySpawnEffect(position);
        return enemy;
    }

    private void PlaySpawnEffect(Vector3 position)
    {
        if (spawnEffectPrefab == null || vfxPool == null)
        {
            return;
        }

        vfxPool.Play(
            spawnEffectPrefab,
            position + spawnEffectOffset,
            spawnEffectPrefab.transform.rotation);
    }

    internal void NotifyDefeated(EnemyController enemy)
    {
        if (enemy != null && activeEnemies.Contains(enemy))
        {
            EnemyDefeated?.Invoke(enemy);
        }
    }

    internal void Release(EnemyController prefab, EnemyController enemy)
    {
        if (enemy == null || !activeEnemies.Remove(enemy))
        {
            return;
        }

        if (prefab != null && pools.TryGetValue(prefab, out PoolEntry entry))
        {
            entry.Pool.Release(enemy);
            return;
        }

        Destroy(enemy.gameObject);
    }

    private EnemyController CreateEnemy(PoolEntry entry)
    {
        EnemyController enemy = Instantiate(entry.Prefab, entry.Root);
        enemy.gameObject.SetActive(false);
        return enemy;
    }

    private static void ReturnToPool(PoolEntry entry, EnemyController enemy)
    {
        enemy.gameObject.SetActive(false);
        enemy.transform.SetParent(entry.Root, false);
    }

    private static void DestroyEnemy(EnemyController enemy)
    {
        if (enemy != null)
        {
            Destroy(enemy.gameObject);
        }
    }

    private static void Prewarm(PoolEntry entry, int count)
    {
        List<EnemyController> enemies = new List<EnemyController>(count);

        for (int i = 0; i < count; i++)
        {
            enemies.Add(entry.Pool.Get());
        }

        foreach (EnemyController enemy in enemies)
        {
            entry.Pool.Release(enemy);
        }
    }

    private void OnDestroy()
    {
        foreach (PoolEntry entry in pools.Values)
        {
            entry.Pool.Clear();
        }

        activeEnemies.Clear();
        pools.Clear();
    }
}
