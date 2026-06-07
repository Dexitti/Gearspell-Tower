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
        TextAsset jsonFile = Resources.Load<TextAsset>("Localization/UI_Texts");
        LocalizationWrapper wrapper = JsonUtility.FromJson<LocalizationWrapper>(jsonFile.text);
        localizationData = wrapper.ToDictionary();
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

    public string GetText(string key, float[] args)
    {
        if (args == null || args.Length == 0)
            return GetText(key);

        object[] objArgs = new object[args.Length];
        for (int i = 0; i < args.Length; i++)
            objArgs[i] = args[i];

        return GetText(key, objArgs);
    }

    public string GetText(string key, params object[] args)
    {
        string template = GetText(key);

        if (args != null && args.Length > 0)
        {
            try
            {
                return string.Format(template, args);
            }
            catch (FormatException)
            {
                Debug.LogError($"[Localization] Invalid format for key '{key}': {template}");
                return template;
            }
        }

        return template;
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