using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Управляет волной
/// </summary>
public class SpawnManager : MonoBehaviour
{
    public Action<GameObject> OnEnemySpawned;
    public Action<GameObject> OnEnemyDespawned;
    public Action OnAllEnemiesDefeated;

    [SerializeField] private float spawnDistance = 5.5f;
    [SerializeField] private Transform poolContainer;

    private Dictionary<GameObject, Queue<GameObject>> enemyPools = new();
    private List<GameObject> activeEnemies = new();

    private WaveData currentWave;
    private Coroutine waveCoroutine;

    public bool HasActiveEnemies => activeEnemies.Count > 0;
    public IReadOnlyList<GameObject> ActiveEnemies => activeEnemies;

    private void Awake()
    {
        G.SpawnManager = this;
        if (poolContainer == null)
        {
            poolContainer = new GameObject("EnemyPool").transform;
            poolContainer.SetParent(transform);
        }
    }

    public void StartWave(WaveData waveData)
    {
        currentWave = waveData;

        if (waveCoroutine != null)
            StopCoroutine(waveCoroutine);

        waveCoroutine = StartCoroutine(RunWave());
    }

    IEnumerator RunWave()
    {
        activeEnemies.Clear();

        foreach (var enemiesConfig in currentWave.enemySpawns)
        {
            // Предварительно создаём врагов в пуле
            PrewarmPool(enemiesConfig.enemyPrefab, enemiesConfig.count);

            for (int i = 0; i < enemiesConfig.count; i++)
            {
                Vector3 spawnPos = GetSpawnPosition();
                GameObject enemy = SpawnEnemy(enemiesConfig, spawnPos);
                activeEnemies.Add(enemy);

                yield return new WaitForSeconds(enemiesConfig.spawnInterval);
            }
        }

        // Ждём пока все враги будут убиты
        yield return new WaitWhile(() => activeEnemies.Count > 0);

        OnAllEnemiesDefeated?.Invoke();
        waveCoroutine = null;
    }

    private Vector3 GetSpawnPosition()
    {
        if (G.Tower == null) return Vector3.zero;
        // tower angle*distance
        float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = spawnDistance;

        float x = Mathf.Cos(angle) * distance;
        float y = Mathf.Sin(angle) * distance * 0.75f;

        return G.Tower.Position + new Vector3(x, y);
    }

    private GameObject SpawnEnemy(EnemySpawnConfig config, Vector3 position)
    {
        GameObject enemy = GetFromPool(config.enemyPrefab);
        enemy.transform.position = position;
        enemy.SetActive(true);
        
        var health = enemy.GetComponent<HealthComponent>();
        if (health != null)
        {
            health.OnDeath -= () => OnEnemyDeath(enemy);
            health.OnDeath += () => OnEnemyDeath(enemy);
        }

        OnEnemySpawned?.Invoke(enemy);
        return enemy;
    }

    private void OnEnemyDeath(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
        OnEnemyDespawned?.Invoke(enemy);
        StartCoroutine(ReturnToPoolDelayed(enemy, 0.5f));
    }

    private IEnumerator ReturnToPoolDelayed(GameObject enemy, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (enemy != null)
        {
            enemy.SetActive(false);
            ReturnToPool(enemy);
        }
    }


    // ========= Pooling =========
    private void PrewarmPool(GameObject prefab, int neededCount)
    {
        if (!enemyPools.ContainsKey(prefab))
            enemyPools[prefab] = new Queue<GameObject>();

        int current = enemyPools[prefab].Count;
        for (int i = current; i < neededCount; i++)
        {
            GameObject enemy = Instantiate(prefab, poolContainer);
            enemy.name = prefab.name;
            enemy.SetActive(false);
            enemyPools[prefab].Enqueue(enemy);
        }
    }

    private GameObject GetFromPool(GameObject prefab)
    {
        if (!enemyPools.ContainsKey(prefab))
            enemyPools[prefab] = new Queue<GameObject>();

        if (enemyPools[prefab].Count > 0)
            return enemyPools[prefab].Dequeue();

        GameObject enemy = Instantiate(prefab, poolContainer);
        enemy.name = prefab.name;
        return enemy;
    }

    private void ReturnToPool(GameObject enemy)
    {
        string prefabName = enemy.name.Replace("(Clone)", "").Trim();
        foreach (var kvp in enemyPools)
        {
            if (kvp.Key.name == prefabName)
            {
                kvp.Value.Enqueue(enemy);
                break;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (G.Tower != null)
        {
            // Рисуем кольцо спавна
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(G.Tower.Position, spawnDistance);
        }
    }
}