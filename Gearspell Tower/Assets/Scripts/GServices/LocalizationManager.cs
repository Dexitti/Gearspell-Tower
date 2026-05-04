using System.Collections.Generic;
using System;
using UnityEngine;

//Singleton
public class LocalizationManager : MonoBehaviour
{
    public enum Language { Russian, English }
    
    public Action<Language> OnLanguageChanged;

    private Language _currentLanguage = Language.Russian;
    public Language CurrentLanguage
    {
        get => _currentLanguage;
        private set
        {
            _currentLanguage = value;
            PlayerPrefs.SetInt("Language", (int)value);
            PlayerPrefs.Save();
            OnLanguageChanged?.Invoke(value);
        }
    }

    private Dictionary<string, Dictionary<string, string>> localizationData;

    private void Awake()
    {
        LoadLanguage();
        LoadLocalizationFile();
    }

    private void Start()
    {
        OnLanguageChanged?.Invoke(_currentLanguage);
    }

    private void LoadLanguage()
    {
        _currentLanguage = (Language)PlayerPrefs.GetInt("Language", 0);
    }

    private void LoadLocalizationFile()
    {
        // Загружаем JSON из Resources
        TextAsset jsonFile = Resources.Load<TextAsset>("Localization/UI_Texts");

        if (jsonFile == null)
        {
            Debug.LogError("Localization file not found in Resources/Localization/UI_Texts.json");
            CreateDefaultLocalizationFile();
            return;
        }

        LocalizationWrapper wrapper = JsonUtility.FromJson<LocalizationWrapper>(jsonFile.text);
        localizationData = wrapper.ToDictionary();
    }

    // Создает дефолтный файл, если его нет
    private void CreateDefaultLocalizationFile()
    {
        localizationData = new Dictionary<string, Dictionary<string, string>>
        {
            ["Continue"] = new Dictionary<string, string> { ["ru"] = "Продолжить", ["en"] = "Continue" },
            ["NewGame"] = new Dictionary<string, string> { ["ru"] = "Новая игра", ["en"] = "New Game" },
            ["Settings"] = new Dictionary<string, string> { ["ru"] = "Настройки", ["en"] = "Settings" },
            ["Exit"] = new Dictionary<string, string> { ["ru"] = "Выйти", ["en"] = "Exit" },
            ["Back"] = new Dictionary<string, string> { ["ru"] = "Назад", ["en"] = "Back" },
            ["Resume"] = new Dictionary<string, string> { ["ru"] = "Продолжить", ["en"] = "Resume" },
            ["MainMenu"] = new Dictionary<string, string> { ["ru"] = "Главное меню", ["en"] = "Main Menu" },
            ["GameOver"] = new Dictionary<string, string> { ["ru"] = "Игра окончена", ["en"] = "Game Over" },
            ["Victory"] = new Dictionary<string, string> { ["ru"] = "Победа!", ["en"] = "Victory!" },
            ["Restart"] = new Dictionary<string, string> { ["ru"] = "Заново", ["en"] = "Restart" },
            ["Wave"] = new Dictionary<string, string> { ["ru"] = "Волна", ["en"] = "Wave" }
        };
    }

    public string GetText(string key)
    {
        string langCode = CurrentLanguage == Language.Russian ? "ru" : "en";

        if (localizationData.TryGetValue(key, out var translations))
        {
            if (translations.TryGetValue(langCode, out var text))
                return text;
        }

        Debug.LogWarning($"[Localization] Missing translation for key: {key}");
        return key;
    }

    public void SetLanguage(Language language)
    {
        CurrentLanguage = language;
    }

    public void ToggleLanguage()
    {
        CurrentLanguage = CurrentLanguage == Language.Russian ? Language.English : Language.Russian;
    }
}

// Вспомогательные классы для парсинга JSON
[Serializable]
public class LocalizationWrapper
{
    public LocalizationEntry[] entries;

    public Dictionary<string, Dictionary<string, string>> ToDictionary()
    {
        var dict = new Dictionary<string, Dictionary<string, string>>();
        foreach (var entry in entries)
        {
            dict[entry.key] = new Dictionary<string, string>
            {
                ["ru"] = entry.ru,
                ["en"] = entry.en
            };
        }
        return dict;
    }
}

[Serializable]
public class LocalizationEntry
{
    public string key;
    public string ru;
    public string en;
}