
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject enemyPrefab;
    public Transform player;

    [Header("Spawn Area")]
    public float minSpawnRadius = 10f;
    public float maxSpawnRadius = 14f;
    public float spawnHeight = 1f;

    [Header("Spawn Timing")]
    public float initialSpawnInterval = 2f;
    public float minimumSpawnInterval = 0.4f;
    public float intervalDecreasePerSecond = 0.015f;

    [Header("Enemy Limit")]
    public int maxEnemies = 30;

    [Header("Debug")]
    public float debugLogInterval = 5f;

    private readonly HashSet<EnemyHealth> activeEnemies =
        new HashSet<EnemyHealth>();

    private float elapsedTime;
    private float currentSpawnInterval;
    private float debugTimer;
    private int totalSpawned;

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

        currentSpawnInterval = initialSpawnInterval;

        StartCoroutine(SpawnRoutine());
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        debugTimer += Time.deltaTime;

        currentSpawnInterval = Mathf.Max(
            minimumSpawnInterval,
            initialSpawnInterval -
            elapsedTime * intervalDecreasePerSecond
        );

        if (debugLogInterval > 0f &&
            debugTimer >= debugLogInterval)
        {
            debugTimer = 0f;

            Debug.Log(
                $"Time: {elapsedTime:F0}s | " +
                $"Enemies: {activeEnemies.Count}/{maxEnemies} | " +
                $"Total Spawned: {totalSpawned} | " +
                $"Interval: {currentSpawnInterval:F2}s"
            );
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (activeEnemies.Count < maxEnemies)
            {
                SpawnEnemy();
            }

            yield return new WaitForSeconds(
                currentSpawnInterval
            );
        }
    }

    void SpawnEnemy()
    {
        Vector2 randomDirection =
            Random.insideUnitCircle.normalized;

        // Handle the extremely rare zero vector.
        if (randomDirection.sqrMagnitude < 0.001f)
            randomDirection = Vector2.up;

        float distance = Random.Range(
            minSpawnRadius,
            maxSpawnRadius
        );

        Vector3 spawnPosition = new Vector3(
            player.position.x + randomDirection.x * distance,
            spawnHeight,
            player.position.z + randomDirection.y * distance
        );

        GameObject enemyObject = Instantiate(
            enemyPrefab,
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

        totalSpawned++;
    }

    void HandleEnemyDied(EnemyHealth enemy)
    {
        enemy.OnDied -= HandleEnemyDied;
        activeEnemies.Remove(enemy);
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
