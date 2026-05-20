using System;
using System.Collections.Generic;
using UnityEngine;

//Singleton
public class SaveManager : MonoBehaviour
{
    private const string GLOBAL_SAVE_KEY = "GlobalSave";

    [Serializable]
    private class GlobalSaveData
    {
        public List<string> unlockedEquipment = new();
        //public GameSettings gameSettings = new();
    }

    private GlobalSaveData data = new GlobalSaveData();

    private void Awake()
    {
        G.SaveManager = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public void Load()
    {
        string json = PlayerPrefs.GetString(GLOBAL_SAVE_KEY, "");
        data = string.IsNullOrEmpty(json) ? new GlobalSaveData() : JsonUtility.FromJson<GlobalSaveData>(json);
        Debug.Log($"[SaveManager] Loaded: {data.unlockedEquipment.Count} equipment, settings loaded");
    }

    public void Save()
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(GLOBAL_SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    public bool HasSave => PlayerPrefs.HasKey(GLOBAL_SAVE_KEY);

    public void ClearSave()
    {
        PlayerPrefs.DeleteKey(GLOBAL_SAVE_KEY);
        data = new GlobalSaveData();
        Debug.Log("[SaveManager] Global save cleared");
    }

    public bool IsEquipmentUnlocked(string equipmentName)
    {
        return data.unlockedEquipment.Contains(equipmentName);
    }

    public void UnlockEquipment(string equipmentName)
    {
        if (!data.unlockedEquipment.Contains(equipmentName))
        {
            data.unlockedEquipment.Add(equipmentName);
            Save();
            Debug.Log($"[SaveManager] Equipment unlocked: {equipmentName}");
        }
    }

    public List<string> GetUnlockedEquipment() => new(data.unlockedEquipment);

    public void ResetGlobalProgress()
    {
        var settings = PlayerPrefs.GetString("GameSettings", "");

        ClearSave();

        data = new GlobalSaveData();
        Save();

        // Восстанавливаем настройки
        if (!string.IsNullOrEmpty(settings))
            PlayerPrefs.SetString("GameSettings", settings);

        Debug.Log("[SaveManager] Global progress reset to default");
    }

    // === Настройки игры ===
    //public GameSettings Settings => data.gameSettings;

    //public void SaveSettings(GameSettings settings)
    //{
    //    data.gameSettings = settings;
    //    Save();
    //    ApplySettings();
    //}

    //private void ApplySettings()
    //{
    //    // Применяем настройки
    //    AudioListener.volume = data.gameSettings.masterVolume;
    //    QualitySettings.SetQualityLevel(data.gameSettings.qualityLevel);
    //    // Язык применяется через LocalizationManager
    //    G.LocalizationManager?.SetLanguage(data.gameSettings.language);
    //}

    //public void ResetGlobalProgress()
    //{
    //    var settings = data.gameSettings; // Сохраняем настройки
    //    data = new GlobalSaveData { gameSettings = settings };
    //    Save();
    //    Debug.Log("[SaveManager] Global progress reset (settings kept)");
    //}
}

[Serializable]
public class GameSettings
{
    // Audio
    public float masterVolume = 1f;
    public float musicVolume = 0.8f;
    public float sfxVolume = 1f;

    // Graphics
    public int qualityLevel = 2;
    public int resolutionIndex = 2;
    public bool fullscreen = true;
    public bool vSync = true;

    // Gameplay
    public LocalizationManager.Language language = LocalizationManager.Language.Russian;
    public bool screenShake = true;
    public bool damageNumbers = true;

    // Controls (если будут)
    public float mouseSensitivity = 1f;
}