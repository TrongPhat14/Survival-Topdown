using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[DisallowMultipleComponent]
public class VfxPool : MonoBehaviour
{
    private sealed class VfxInstance
    {
        public GameObject GameObject;
        public Transform Transform;
        public ParticleSystem[] ParticleSystems;
        public Vector3 BaseScale;
    }

    private sealed class PoolEntry
    {
        public GameObject Prefab;
        public Transform Root;
        public ObjectPool<VfxInstance> Pool;
    }

    private readonly Dictionary<GameObject, PoolEntry> pools = new();

    public static VfxPool FindOrCreate()
    {
        VfxPool pool = FindFirstObjectByType<VfxPool>();
        if (pool != null)
        {
            return pool;
        }

        GameObject poolRoot = GameObject.Find("PoolRoot");
        if (poolRoot == null)
        {
            poolRoot = new GameObject("PoolRoot");
        }

        pool = poolRoot.GetComponent<VfxPool>();
        return pool != null ? pool : poolRoot.AddComponent<VfxPool>();
    }

    public bool Register(GameObject prefab, int initialSize, int maxSize)
    {
        if (prefab == null ||
            prefab.GetComponentInChildren<ParticleSystem>(true) == null)
        {
            Debug.LogError("VfxPool requires a prefab with a ParticleSystem.", this);
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
        entry.Pool = new ObjectPool<VfxInstance>(
            () => CreateInstance(entry),
            null,
            instance => ReturnToPool(entry, instance),
            DestroyInstance,
            false,
            initialSize,
            maxSize);

        pools.Add(prefab, entry);
        Prewarm(entry, initialSize);
        return true;
    }

    public bool Play(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation,
        float scaleMultiplier = 1f)
    {
        if (prefab == null || !pools.TryGetValue(prefab, out PoolEntry entry))
        {
            Debug.LogError("VFX prefab is not registered in VfxPool.", this);
            return false;
        }

        VfxInstance instance = entry.Pool.Get();
        instance.Transform.SetPositionAndRotation(position, rotation);
        instance.Transform.localScale =
            instance.BaseScale * Mathf.Max(0.01f, scaleMultiplier);
        instance.GameObject.SetActive(true);

        foreach (ParticleSystem particleSystem in instance.ParticleSystems)
        {
            particleSystem.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            particleSystem.Play(false);
        }

        StartCoroutine(ReleaseWhenFinished(entry, instance));
        return true;
    }

    private VfxInstance CreateInstance(PoolEntry entry)
    {
        GameObject effectObject = Instantiate(entry.Prefab, entry.Root);
        VfxInstance instance = new VfxInstance
        {
            GameObject = effectObject,
            Transform = effectObject.transform,
            ParticleSystems = effectObject.GetComponentsInChildren<ParticleSystem>(true),
            BaseScale = effectObject.transform.localScale
        };

        effectObject.SetActive(false);
        return instance;
    }

    private IEnumerator ReleaseWhenFinished(
        PoolEntry entry,
        VfxInstance instance)
    {
        yield return null;

        while (IsAlive(instance))
        {
            yield return null;
        }

        entry.Pool.Release(instance);
    }

    private static bool IsAlive(VfxInstance instance)
    {
        foreach (ParticleSystem particleSystem in instance.ParticleSystems)
        {
            if (particleSystem != null && particleSystem.IsAlive(false))
            {
                return true;
            }
        }

        return false;
    }

    private static void ReturnToPool(PoolEntry entry, VfxInstance instance)
    {
        instance.GameObject.SetActive(false);
        instance.Transform.SetParent(entry.Root, false);
    }

    private static void DestroyInstance(VfxInstance instance)
    {
        if (instance?.GameObject != null)
        {
            Destroy(instance.GameObject);
        }
    }

    private static void Prewarm(PoolEntry entry, int count)
    {
        List<VfxInstance> instances = new List<VfxInstance>(count);

        for (int i = 0; i < count; i++)
        {
            instances.Add(entry.Pool.Get());
        }

        foreach (VfxInstance instance in instances)
        {
            entry.Pool.Release(instance);
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
