// Editor/CreatureDataGenerator.cs
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CreatureDataGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Creatures from JSON")]
    public static void Generate()
    {
        string jsonPath = EditorUtility.OpenFilePanel("Select creatures_database.json", "Assets/Resources/Data", "json");
        if (string.IsNullOrEmpty(jsonPath)) return;

        string jsonText = File.ReadAllText(jsonPath);
        JsonCreatureDatabase db = JsonUtility.FromJson<JsonCreatureDatabase>(jsonText);

        if (db == null || db.creatures == null)
        {
            Debug.LogError("Invalid JSON format!");
            return;
        }

        GenerateCreatures(db);
    }

    private static void GenerateCreatures(JsonCreatureDatabase db)
    {
        string creatureFolder = "Assets/Resources/Data/CreaturesData";
        EnsureDirectory(creatureFolder);

        foreach (var jsonCreature in db.creatures)
        {
            string id = jsonCreature.name.Replace(" ", "_");
            string path = $"{creatureFolder}/{id}.asset";

            CreatureData so = AssetDatabase.LoadAssetAtPath<CreatureData>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<CreatureData>();
                AssetDatabase.CreateAsset(so, path);
            }

            so.creatureName = jsonCreature.name;
            so.type = ParseCreatureType(jsonCreature.type);
            so.shortDescription = jsonCreature.description;
            so.health = jsonCreature.health;
            so.damage = jsonCreature.damage;
            so.speed = jsonCreature.speed;

            // Дроп зависит от типа
            var (min, max) = GetDropRange(so.type);
            so.minGearsDrop = min;
            so.maxGearsDrop = max;

            EditorUtility.SetDirty(so);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[Generator] Processed {db.creatures.Count} creatures");
    }

    private static CreatureType ParseCreatureType(string type) => type switch
    {
        "Easy" => CreatureType.Easy,
        "Regular" => CreatureType.Regular,
        "Medium" => CreatureType.Medium,
        "Heavy" => CreatureType.Heavy,
        "Boss" => CreatureType.Boss,
        _ => CreatureType.Regular
    };

    private static (int min, int max) GetDropRange(CreatureType type) => type switch
    {
        CreatureType.Easy => (1, 5),
        CreatureType.Regular => (2, 8),
        CreatureType.Medium => (10, 20),
        CreatureType.Heavy => (16, 40),
        CreatureType.Boss => (85, 110),
        _ => (1, 1)
    };

    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }
}

[Serializable]
public class JsonCreatureDatabase
{
    public List<JsonCreature> creatures;
}

[Serializable]
public class JsonCreature
{
    public string name;
    public string type;
    public string description;
    public int health;
    public int damage;
    public float speed;
    public int wave;
}