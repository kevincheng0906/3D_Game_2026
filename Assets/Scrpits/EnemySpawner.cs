
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;

[Serializable]
public class WaveSettings
{
    public string waveName = "Wave";

    [Min(1)]
    public int totalEnemies = 10;

    [Min(0.1f)]
    public float spawnInterval = 2f;

    [Range(0f, 1f)]
    public float fastEnemyChance = 0f;

    [Min(1)]
    public int maxAlive = 6;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject enemyPrefab;
    public GameObject fastEnemyPrefab;
    public Transform player;

    [Header("Spawn Area")]
    public float minSpawnRadius = 10f;
    public float maxSpawnRadius = 14f;
    public float spawnHeight = 1f;

    [Header("Wave Settings")]
    public WaveSettings[] waves =
    {
        new WaveSettings
        {
            waveName = "Warm Up",
            totalEnemies = 10,
            spawnInterval = 2f,
            fastEnemyChance = 0f,
            maxAlive = 6
        },
        new WaveSettings
        {
            waveName = "Faster Enemies",
            totalEnemies = 18,
            spawnInterval = 1.5f,
            fastEnemyChance = 0.3f,
            maxAlive = 10
        },
        new WaveSettings
        {
            waveName = "Final Wave",
            totalEnemies = 26,
            spawnInterval = 1f,
            fastEnemyChance = 0.45f,
            maxAlive = 16
        }
};

    public float timeBetweenWaves = 4f;

    [Header("Difficulty Scaling")]
    public float minimumSpawnInterval = 0.4f;
    public float intervalDecreasePerSecond = 0.015f;

    [Header("Enemy Limit")]
    public int maxEnemies = 30;

    [Header("UI")]
    public TMP_Text waveText;
    public TMP_Text progressText;

    [Header("Debug")]
    public float debugLogInterval = 5f;

    [Header("Rescue System")]
    public RescueManager rescueManager;

    private readonly HashSet<EnemyHealth> activeEnemies =
        new HashSet<EnemyHealth>();

    private float elapsedTime;
    private float waveStartTime;
    private float currentSpawnInterval;
    private float debugTimer;

    private int spawnedThisWave;
    private int defeatedThisWave;
    private int totalSpawned;

    public int CurrentWave { get; private set; }
    public bool AllWavesCleared { get; private set; }

    public event Action<int> OnWaveStarted;
    public event Action<int> OnWaveCleared;
    public event Action OnAllWavesCleared;

    void Start()
    {
        if (enemyPrefab == null || player == null)
        {
            Debug.LogError(
                "EnemySpawner: Assign Enemy Prefab and Player."
            );
            enabled = false;
            return;
        }

        if (enemyPrefab.GetComponent<EnemyHealth>() == null)
        {
            Debug.LogError(
                "EnemySpawner: Prefab needs EnemyHealth."
            );
            enabled = false;
            return;
        }

        if (fastEnemyPrefab != null &&
            fastEnemyPrefab.GetComponent<EnemyHealth>() == null)
        {
            Debug.LogError(
                "EnemySpawner: FastEnemy needs EnemyHealth."
            );
            enabled = false;
            return;
        }

        if (waves == null || waves.Length == 0)
        {
            Debug.LogError("EnemySpawner: No waves configured.");
            enabled = false;
            return;
        }

        foreach (WaveSettings wave in waves)
        {
            if (wave == null)
            {
                Debug.LogError(
                    "EnemySpawner: Wave settings cannot be null."
                );
                enabled = false;
                return;
            }
        }

        StartCoroutine(RunWaves());
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        debugTimer += Time.deltaTime;

        if (debugLogInterval > 0f &&
            debugTimer >= debugLogInterval)
        {
            debugTimer = 0f;

            Debug.Log(
                $"Wave: {CurrentWave}/{waves.Length} | " +
                $"Defeated: {defeatedThisWave} | " +
                $"Alive: {activeEnemies.Count} | " +
                $"Total Spawned: {totalSpawned} | " +
                $"Interval: {currentSpawnInterval:F2}s"
            );
        }
    }

    IEnumerator RunWaves()
    {
        AllWavesCleared = false;

        for (int i = 0; i < waves.Length; i++)
        {
            WaveSettings wave = waves[i];

            CurrentWave = i + 1;
            spawnedThisWave = 0;
            defeatedThisWave = 0;
            waveStartTime = Time.time;

            Debug.Log(
                $"Wave {CurrentWave} Started: {wave.waveName}"
            );

            UpdateWaveUI();
            OnWaveStarted?.Invoke(CurrentWave);

            // Spawn enemies until this wave's quota is reached.
            while (spawnedThisWave < wave.totalEnemies)
            {
                float waveElapsedTime =
                    Time.time - waveStartTime;

                currentSpawnInterval = Mathf.Max(
                    Mathf.Max(0.1f, minimumSpawnInterval),
                    wave.spawnInterval -
                    waveElapsedTime * intervalDecreasePerSecond
                );

                int aliveLimit = Mathf.Min(
                    Mathf.Max(1, maxEnemies),
                    Mathf.Max(1, wave.maxAlive)
                );

                if (activeEnemies.Count < aliveLimit)
                {
                    SpawnEnemy(wave);

                    yield return new WaitForSeconds(
                        currentSpawnInterval
                    );
                }
                else
                {
                    // Wait for enemies to be defeated.
                    yield return new WaitForSeconds(0.1f);
                }
            }

            // Wait until every enemy in this wave is defeated.
            yield return new WaitUntil(() =>
                defeatedThisWave >= wave.totalEnemies &&
                activeEnemies.Count == 0 &&
                (rescueManager == null ||
                 rescueManager.IsRescueCompleteForWave(CurrentWave))
            );

            Debug.Log($"Wave {CurrentWave} Cleared!");

            OnWaveCleared?.Invoke(CurrentWave);

            if (i < waves.Length - 1)
            {
                if (waveText != null)
                {
                    waveText.text =
                        $"Wave {CurrentWave} Cleared!";
                }

                yield return new WaitForSeconds(
                    Mathf.Max(0f, timeBetweenWaves)
                );
            }
        }

        AllWavesCleared = true;

        Debug.Log("ALL WAVES CLEARED!");

        if (waveText != null)
            waveText.text = "ALL WAVES CLEARED!";

        OnAllWavesCleared?.Invoke();
    }

    void SpawnEnemy(WaveSettings wave)
    {
        Vector2 randomDirection =
            Random.insideUnitCircle.normalized;

        if (randomDirection.sqrMagnitude < 0.001f)
            randomDirection = Vector2.up;

        float distance = Random.Range(
            Mathf.Min(minSpawnRadius, maxSpawnRadius),
            Mathf.Max(minSpawnRadius, maxSpawnRadius)
        );

        Vector3 spawnPosition = new Vector3(
            player.position.x + randomDirection.x * distance,
            spawnHeight,
            player.position.z + randomDirection.y * distance
        );

        GameObject selectedPrefab = enemyPrefab;

        if (fastEnemyPrefab != null &&
            Random.value < wave.fastEnemyChance)
        {
            selectedPrefab = fastEnemyPrefab;
        }

        GameObject enemyObject = Instantiate(
            selectedPrefab,
            spawnPosition,
            Quaternion.identity
        );

        EnemyHealth health =
            enemyObject.GetComponent<EnemyHealth>();

        if (health == null)
        {
            Destroy(enemyObject);
            return;
        }

        activeEnemies.Add(health);
        health.OnDied += HandleEnemyDied;

        spawnedThisWave++;
        totalSpawned++;

        UpdateWaveUI();
    }

    void HandleEnemyDied(EnemyHealth enemy)
    {
        enemy.OnDied -= HandleEnemyDied;

        if (!activeEnemies.Remove(enemy))
            return;

        defeatedThisWave++;

        UpdateWaveUI();
    }

    void UpdateWaveUI()
    {
        if (waveText != null)
        {
            waveText.text =
                $"Wave {CurrentWave}/{waves.Length}";
        }

        if (progressText != null && CurrentWave > 0)
        {
            int total = waves[CurrentWave - 1].totalEnemies;

            progressText.text =
                $"Defeated: {defeatedThisWave}/{total} | " +
                $"Alive: {activeEnemies.Count}";
        }
    }

    void OnDestroy()
    {
        foreach (EnemyHealth enemy in activeEnemies)
        {
            if (enemy != null)
                enemy.OnDied -= HandleEnemyDied;
        }

        activeEnemies.Clear();
    }
}
