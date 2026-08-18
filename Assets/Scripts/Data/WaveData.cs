using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Wave", menuName = "Survival/Wave Data")]
public class WaveData : ScriptableObject
{
    [Serializable]
    public sealed class SpawnGroup
    {
        [SerializeField] private EnemyController enemyPrefab;
        [SerializeField, Min(1)] private int minCount = 1;
        [SerializeField, Min(1)] private int maxCount = 1;
        [SerializeField, Min(0f)] private float spawnInterval = 0.5f;

        public EnemyController EnemyPrefab => enemyPrefab;
        public int MinCount => Mathf.Max(1, minCount);
        public int MaxCount => Mathf.Max(MinCount, maxCount);
        public float SpawnInterval => Mathf.Max(0f, spawnInterval);

        public int RollCount()
        {
            return UnityEngine.Random.Range(MinCount, MaxCount + 1);
        }
    }

    [SerializeField] private List<SpawnGroup> spawnGroups = new();

    public IReadOnlyList<SpawnGroup> SpawnGroups => spawnGroups;

    public int TotalEnemyCount
    {
        get
        {
            int total = 0;

            foreach (SpawnGroup group in spawnGroups)
            {
                if (group != null && group.EnemyPrefab != null)
                {
                    total += group.MaxCount;
                }
            }

            return total;
        }
    }
}
