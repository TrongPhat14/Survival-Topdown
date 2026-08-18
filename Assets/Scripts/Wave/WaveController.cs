using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class WaveController : MonoBehaviour
{
    public enum WaveState
    {
        Idle,
        Preparing,
        Spawning,
        Fighting,
        Completed
    }

    [SerializeField] private WaveData[] waves;
    [SerializeField] private EnemyFactory enemyFactory;
    [SerializeField] private WaveHUD waveHUD;
    [SerializeField] private Transform spawnPointRoot;
    [SerializeField, Min(0f)] private float preparationDuration = 2f;
    [SerializeField] private bool playOnStart = true;

    private readonly List<Transform> spawnPoints = new();
    private Coroutine waveRoutine;
    private int aliveEnemyCount;

    public event Action<int> WaveStarted;
    public event Action<int> WaveCompleted;
    public event Action AllWavesCompleted;

    public WaveState State { get; private set; } = WaveState.Idle;
    public int CurrentWave { get; private set; }
    public int AliveEnemyCount => aliveEnemyCount;

    private void Awake()
    {
        CacheSpawnPoints();
        RegisterEnemyPrefabs();
        waveHUD?.SetState(0, TotalWaves, 0);
    }

    private void OnEnable()
    {
        if (enemyFactory != null)
        {
            enemyFactory.EnemyDefeated += HandleEnemyDefeated;
        }
    }

    private void Start()
    {
        if (playOnStart)
        {
            StartWaves();
        }
    }

    private void OnDisable()
    {
        if (enemyFactory != null)
        {
            enemyFactory.EnemyDefeated -= HandleEnemyDefeated;
        }

        if (waveRoutine != null)
        {
            StopCoroutine(waveRoutine);
            waveRoutine = null;
        }
    }

    public void StartWaves()
    {
        if (waveRoutine != null)
        {
            return;
        }

        if (enemyFactory == null || waveHUD == null || spawnPoints.Count == 0)
        {
            Debug.LogError("WaveController is missing EnemyFactory, WaveHUD, or spawn points.", this);
            return;
        }

        if (TotalWaves == 0)
        {
            Debug.LogWarning("WaveController has no WaveData assigned.", this);
            return;
        }

        waveRoutine = StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        aliveEnemyCount = 0;

        for (int waveIndex = 0; waveIndex < waves.Length; waveIndex++)
        {
            WaveData wave = waves[waveIndex];
            if (wave == null)
            {
                continue;
            }

            CurrentWave = waveIndex + 1;
            State = WaveState.Preparing;
            waveHUD.SetState(CurrentWave, TotalWaves, aliveEnemyCount);
            WaveStarted?.Invoke(CurrentWave);

            if (preparationDuration > 0f)
            {
                yield return new WaitForSeconds(preparationDuration);
            }

            State = WaveState.Spawning;
            yield return SpawnWave(wave);

            State = WaveState.Fighting;
            while (aliveEnemyCount > 0)
            {
                yield return null;
            }

            State = WaveState.Completed;
            WaveCompleted?.Invoke(CurrentWave);
        }

        waveRoutine = null;
        AllWavesCompleted?.Invoke();
        GameResultUI.Instance?.Show(GameResult.Victory);
    }

    private IEnumerator SpawnWave(WaveData wave)
    {
        foreach (WaveData.SpawnGroup group in wave.SpawnGroups)
        {
            if (group == null || group.EnemyPrefab == null)
            {
                continue;
            }

            int spawnCount = group.RollCount();

            for (int i = 0; i < spawnCount; i++)
            {
                Transform spawnPoint = spawnPoints[
                    UnityEngine.Random.Range(0, spawnPoints.Count)];
                EnemyController enemy = enemyFactory.Spawn(
                    group.EnemyPrefab,
                    spawnPoint.position,
                    spawnPoint.rotation);

                if (enemy != null)
                {
                    aliveEnemyCount++;
                    waveHUD.SetAliveCount(aliveEnemyCount);
                }

                if (group.SpawnInterval > 0f)
                {
                    yield return new WaitForSeconds(group.SpawnInterval);
                }
            }
        }
    }

    private void HandleEnemyDefeated(EnemyController enemy)
    {
        aliveEnemyCount = Mathf.Max(0, aliveEnemyCount - 1);
        waveHUD.SetAliveCount(aliveEnemyCount);
    }

    private void CacheSpawnPoints()
    {
        spawnPoints.Clear();

        if (spawnPointRoot == null)
        {
            return;
        }

        for (int i = 0; i < spawnPointRoot.childCount; i++)
        {
            Transform spawnPoint = spawnPointRoot.GetChild(i);
            if (spawnPoint.gameObject.activeInHierarchy)
            {
                spawnPoints.Add(spawnPoint);
            }
        }
    }

    private void RegisterEnemyPrefabs()
    {
        if (enemyFactory == null || waves == null)
        {
            return;
        }

        foreach (WaveData wave in waves)
        {
            if (wave == null)
            {
                continue;
            }

            foreach (WaveData.SpawnGroup group in wave.SpawnGroups)
            {
                if (group != null && group.EnemyPrefab != null)
                {
                    enemyFactory.Register(group.EnemyPrefab);
                }
            }
        }
    }

    private int TotalWaves => waves != null ? waves.Length : 0;
}
