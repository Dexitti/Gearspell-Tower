using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private float spawnDistance = 5.5f;
    private int enemiesPerWave = 10;
    [SerializeField] private float timeBetweenWaves = 3f;
    [SerializeField] private float _spawnInterval = 1f;

    private Tower tower;
    public GameObject[] enemyPrefabs;
    private Queue<GameObject> enemyPool = new Queue<GameObject>();
    private List<GameObject> availableEnemies = new List<GameObject>();

    public void Awake()
    {
        tower = GameObject.Find("Tower").GetComponent<Tower>();
        if (tower == null) Debug.LogError("Tower не найден на сцене!");
        InitializePool();
    }

    public void Start()
    {
        StartCoroutine(WaveManager());
    }

    private void InitializePool()
    {
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            GameObject enemy = Instantiate(enemyPrefabs[i], transform);
            enemy.SetActive(false);
            enemyPool.Enqueue(enemy);
        }
    }

    IEnumerator WaveManager()
    {
        int waveNumber = 1;

        while (true)
        {
            Debug.Log($"Начинается волна {waveNumber}");
            yield return StartCoroutine(SpawnWave(waveNumber));

            Debug.Log($"Волна {waveNumber} завершена. Ожидание следующей волны...");
            yield return new WaitForSeconds(timeBetweenWaves);

            waveNumber++;

            // Увеличиваем сложность
            enemiesPerWave = Mathf.RoundToInt(enemiesPerWave * 1.2f);
        }
    }

    IEnumerator SpawnWave(int waveNumber)
    {
        int enemiesToSpawn = enemiesPerWave;

        while (enemiesToSpawn > 0)
        {
            int enemiesThisBatch = Mathf.Min(Random.Range(1, 4), enemiesToSpawn);

            for (int i = 0; i < enemiesThisBatch; i++)
            {
                SpawnEnemy(waveNumber);
                enemiesToSpawn--;

                if (enemiesToSpawn <= 0) break;
            }

            yield return new WaitForSeconds(_spawnInterval);
        }

        // Ждем пока все враги будут убиты
        yield return new WaitWhile(() => availableEnemies.Count > 0);
    }


    private void SpawnEnemy(int waveNumber)
    {
        if (enemyPrefabs.Length == 0) return;
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        GameObject enemy;

        // Используем пул или создаем новый объект
        if (enemyPool.Count > 0)
        {
            enemy = enemyPool.Dequeue();
            enemy.SetActive(true);
        }
        else enemy = Instantiate(enemyPrefab, transform);

        enemy.transform.position = GetSpawnPosition();
        availableEnemies.Add(enemy);

        Debug.Log($"Враг создан на позиции: {enemy.transform.position}");
    }

    private Vector3 GetSpawnPosition()
    {
        Vector3 towerPosition = tower.transform.position;

        // tower angle*distance
        float angle = Random.Range(0f, 360f);
        float distance = spawnDistance;

        float x = Mathf.Cos(angle * Mathf.Deg2Rad) * distance;
        float y = Mathf.Sin(angle * Mathf.Deg2Rad) * distance * 0.75f;

        return towerPosition + new Vector3(x, y, 0);
    }

    void OnDrawGizmosSelected()
    {
        if (tower != null)
        {
            // Рисуем кольцо спавна
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(tower.transform.position, spawnDistance);

            // Рисуем линии до врагов
            Gizmos.color = Color.yellow;
            foreach (var enemy in availableEnemies)
            {
                if (enemy != null)
                    Gizmos.DrawLine(enemy.transform.position, tower.transform.position);
            }
        }
    }
}