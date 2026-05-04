using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class WaveDataGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Waves from JSON")]
    public static void Generate()
    {
        string jsonPath = EditorUtility.OpenFilePanel("Select waves_database.json", "Assets/Resources/Data", "json");
        if (string.IsNullOrEmpty(jsonPath)) return;

        string jsonText = File.ReadAllText(jsonPath);
        JsonWaveDatabase db = JsonUtility.FromJson<JsonWaveDatabase>(jsonText);

        if (db == null || db.waves == null)
        {
            Debug.LogError("Invalid JSON format!");
            return;
        }

        GenerateWaves(db);
    }

    private static void GenerateWaves(JsonWaveDatabase db)
    {
        string waveFolder = "Assets/Resources/Data/WavesData";
        string creatureFolder = "Assets/Resources/Prefabs/Creatures";
        string equipmentFolder = "Assets/Resources/Data/EquipmentData";
        EnsureDirectory(waveFolder);

        var creatureAssets = AssetDatabase.FindAssets("t:GameObject", new[] { creatureFolder });
        Dictionary<string, GameObject> creaturePrefabs = new();

        foreach (var guid in creatureAssets)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject enemy = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (enemy != null && enemy.GetComponent<Creature>() != null)
            {
                creaturePrefabs[enemy.name] = enemy;
            }
        }

        var equipmentAssets = AssetDatabase.FindAssets("t:EquipmentData", new[] { equipmentFolder });
        Dictionary<string, EquipmentData> equipmentData = new();
        foreach (var guid in equipmentAssets)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EquipmentData eq = AssetDatabase.LoadAssetAtPath<EquipmentData>(path);
            if (eq != null)
            {
                equipmentData[eq.equipmentName] = eq;
            }
        }

        for (int i = 0; i < db.waves.Count; i++)
        {
            var jsonWave = db.waves[i];
            string waveId = $"Wave_{i + 1:D2}";
            string path = $"{waveFolder}/{waveId}.asset";

            WaveData so = AssetDatabase.LoadAssetAtPath<WaveData>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<WaveData>();
                AssetDatabase.CreateAsset(so, path);
            }

            so.waveName = jsonWave.waveName;
            so.gearsReward = jsonWave.gearsReward;

            // Создаём EnemySpawnConfig
            var spawnConfigs = new List<EnemySpawnConfig>();
            foreach (var jsonEnemy in jsonWave.enemies)
            {
                if (creaturePrefabs.TryGetValue(jsonEnemy.creatureName, out GameObject prefab))
                {
                    spawnConfigs.Add(new EnemySpawnConfig
                    {
                        enemyPrefab = prefab,
                        count = jsonEnemy.count,
                        spawnInterval = jsonEnemy.spawnInterval
                    });
                }
                else
                {
                    Debug.LogWarning($"[Generator] Creature prefab not found: {jsonEnemy.creatureName}");
                }
            }

            so.enemySpawns = spawnConfigs.ToArray();

            var unlocks = new List<EquipmentData>();
            if (jsonWave.equipmentUnlocks != null)
            {
                foreach (string eqName in jsonWave.equipmentUnlocks)
                {
                    if (equipmentData.TryGetValue(eqName, out EquipmentData eq))
                    {
                        unlocks.Add(eq);
                    }
                    else
                    {
                        Debug.LogWarning($"[Generator] Equipment not found: {eqName}");
                    }
                }
            }
            so.equipmentUnlocks = unlocks.ToArray();

            so.waveDialogs = new DialogData[0]; // Пустой массив для диалогов (потом)

            EditorUtility.SetDirty(so);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[Generator] Processed {db.waves.Count} waves");
    }

    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }
}

[Serializable]
public class JsonWaveDatabase
{
    public List<JsonWave> waves;
}

[Serializable]
public class JsonWave
{
    public string waveName;
    public List<JsonWaveEnemy> enemies;
    public int gearsReward;
    public string[] equipmentUnlocks;
}

[Serializable]
public class JsonWaveEnemy
{
    public string creatureName;
    public int count;
    public float spawnInterval;
}