using System;
using System.Collections.Generic;
using UnityEngine;

//Singleton
public class SaveManager : MonoBehaviour
{
    private const string GLOBAL_SAVE_KEY = "GlobalSave";
    private const string SETTINGS_KEY = "PlayerSettings";

    [Serializable]
    private class GlobalSaveData
    {
        public List<string> unlockedEquipment = new();
    }

    private GlobalSaveData data = new GlobalSaveData();

    public PlayerSettings Settings { get; private set; } = new PlayerSettings();

    private void Awake()
    {
        G.SaveManager = this;
        DontDestroyOnLoad(gameObject);
        Load();
        LoadGameSettings();
    }

    private void Start()
    {
        ApplySettings();
    }

    public void Load()
    {
        string json = PlayerPrefs.GetString(GLOBAL_SAVE_KEY, "");
        data = string.IsNullOrEmpty(json) ? new GlobalSaveData() : JsonUtility.FromJson<GlobalSaveData>(json);
        Debug.Log($"[SaveManager] Loaded: {data.unlockedEquipment.Count} unlocked equipment");
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
        var settingsJson = PlayerPrefs.GetString(SETTINGS_KEY, "");

        ClearSave();

        data = new GlobalSaveData();
        Save();

        if (!string.IsNullOrEmpty(settingsJson))
            PlayerPrefs.SetString(SETTINGS_KEY, settingsJson);

        LoadGameSettings();
        ApplySettings();

        Debug.Log("[SaveManager] Global progress reset (settings kept)");
    }

    // === Настройки игры ===
    public void LoadGameSettings()
    {
        if (!PlayerPrefs.HasKey(SETTINGS_KEY))
        {
            Settings = new PlayerSettings();
            return;
        }

        string json = PlayerPrefs.GetString(SETTINGS_KEY);
        Settings = JsonUtility.FromJson<PlayerSettings>(json) ?? new PlayerSettings();
    }

    public void SaveGameSettings(PlayerSettings settings)
    {
        Settings = settings ?? new PlayerSettings();
        PlayerPrefs.SetString(SETTINGS_KEY, JsonUtility.ToJson(Settings));
        PlayerPrefs.Save();
        ApplySettings();
    }

    public void ApplySettings()
    {
        if (G.AudioManager != null)
            G.AudioManager.MasterVolume = Settings.masterVolume;

        G.LocalizationManager?.SetLanguage(Settings.language);

        ApplyDisplayMode(Settings.displayModeIndex);
        ApplyResolution(Settings.resolutionIndex);
    }

    public static void ApplyResolution(int index)
    {
        switch (index)
        {
            case 0: Screen.SetResolution(2560, 1440, Screen.fullScreenMode); break;
            case 1: Screen.SetResolution(1920, 1080, Screen.fullScreenMode); break;
            case 2: Screen.SetResolution(1440, 900, Screen.fullScreenMode); break;
            case 3: Screen.SetResolution(1280, 720, Screen.fullScreenMode); break;
            case 4: Screen.SetResolution(800, 600, Screen.fullScreenMode); break;
            default: Screen.SetResolution(1920, 1080, Screen.fullScreenMode); break;
        }
    }

    public static void ApplyDisplayMode(int index)
    {
        switch (index)
        {
            case 0: Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen; break;
            case 1: Screen.fullScreenMode = FullScreenMode.FullScreenWindow; break;
            case 2: Screen.fullScreenMode = FullScreenMode.Windowed; break;
            default: Screen.fullScreenMode = FullScreenMode.FullScreenWindow; break;
        }
    }
}

[Serializable]
public class PlayerSettings
{
    public float masterVolume = 1f;
    public int resolutionIndex = 2;
    public int displayModeIndex = 1;
    public LocalizationManager.Language language = LocalizationManager.Language.Russian;
}