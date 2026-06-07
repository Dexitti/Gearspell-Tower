using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class UpgradeDataGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Upgrades from JSON")]
    public static void Generate()
    {
        string jsonPath = EditorUtility.OpenFilePanel("Select upgrades_database.json", "Assets/Resources/Data", "json");
        if (string.IsNullOrEmpty(jsonPath)) return;

        string jsonText = File.ReadAllText(jsonPath);
        JsonDatabase db = JsonUtility.FromJson<JsonDatabase>(jsonText);

        if (db == null || db.equipments == null)
        {
            Debug.LogError("Invalid JSON format!");
            return;
        }

        GenerateUpgrades(db);
    }

    private static void GenerateUpgrades(JsonDatabase db)
    {
        string upgradeFolder = "Assets/Resources/Data/UpgradesData";
        string equipmentFolder = "Assets/Resources/Data/EquipmentData";
        EnsureDirectory(upgradeFolder);
        EnsureDirectory(equipmentFolder);

        List<LocalizationEntry> newLocalizationEntries = new();

        foreach (var eq in db.equipments)
        {
            int upgradeNumber = 0;
            List<List<UpgradeData>> stagesData = new();

            foreach (var stage in eq.stages)
            {
                List<UpgradeData> stageUpgrades = new();
                foreach (var jsonUpgrade in stage.upgrades)
                {
                    UpgradeData so = CreateUpgradeData(jsonUpgrade, eq.name, upgradeNumber, upgradeFolder);
                    stageUpgrades.Add(so);
                    upgradeNumber++;
                }
                stagesData.Add(stageUpgrades);
            }

            UpdateEquipmentData(eq, stagesData, equipmentFolder);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[Generator] Processed {db.equipments.Count} equipments");
    }

    private static UpgradeData CreateUpgradeData(JsonUpgrade json, string equipmentName, int num, string folder)
    {
        string id = $"{equipmentName}_{num}";
        string path = $"{folder}/{id}.asset";

        UpgradeData so = AssetDatabase.LoadAssetAtPath<UpgradeData>(path);
        if (so == null)
        {
            so = ScriptableObject.CreateInstance<UpgradeData>();
            AssetDatabase.CreateAsset(so, path);
        }

        so.id = id;
        so.upgradeNameKey = json.upgradeNameKey;
        so.descriptionKey = json.descriptionKey;

        if (json.formatArgs != null && json.formatArgs.Length > 0)
        {
            so.formatArgs = new float[json.formatArgs.Length];
            for (int i = 0; i < json.formatArgs.Length; i++)
            {
                so.formatArgs[i] = json.formatArgs[i];
            }
        }
        else so.formatArgs = new float[0];
        
        so.cardType = ParseCardType(json.cardType);
        so.cost = GetUpgradeCost(json.cardType);

        EditorUtility.SetDirty(so);
        return so;
    }

    private static int GetUpgradeCost(string cardType)
    {
        return cardType switch
        {
            "Equipment" => 50,
            "Common" => 25,
            "Fork" => 75,
            "ActiveAbility" => 120,
            _ => 1
        };
    }

    private static void UpdateEquipmentData(JsonEquipment json, List<List<UpgradeData>> stages, string folder)
    {
        string path = $"{folder}/{json.name}Data.asset";
        EquipmentData eq = AssetDatabase.LoadAssetAtPath<EquipmentData>(path);

        eq.stages.Clear();
        for (int i = 0; i < stages.Count; i++)
        {
            eq.stages.Add(new UpgradeStages
            {
                isFork = json.stages[i].isFork,
                upgradeData = stages[i]
            });
        }

        EditorUtility.SetDirty(eq);
    }

    private static UpgradeCardType ParseCardType(string type)
    {
        return type switch
        {
            "Equipment" => UpgradeCardType.Equipment,
            "Common" => UpgradeCardType.Common,
            "Fork" => UpgradeCardType.Fork,
            "ActiveAbility" => UpgradeCardType.ActiveAbility,
            _ => UpgradeCardType.Common
        };
    }

    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }
}

[Serializable]
public class JsonDatabase
{
    public List<JsonEquipment> equipments;
}

[Serializable]
public class JsonEquipment
{
    public string name;
    public List<JsonStage> stages;
}

[Serializable]
public class JsonStage
{
    public bool isFork;
    public List<JsonUpgrade> upgrades;
}

[Serializable]
public class JsonUpgrade
{
    public string cardType;
    public string upgradeNameKey;
    public string descriptionKey;
    public float[] formatArgs;
}