using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown displayModeDropdown;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button backButton;

    [Header("Localized Texts")]
    [SerializeField] private TextMeshProUGUI settingsTitle;
    [SerializeField] private TextMeshProUGUI languageLabel;
    [SerializeField] private TextMeshProUGUI volumeLabel;
    [SerializeField] private TextMeshProUGUI resolutionLabel;
    [SerializeField] private TextMeshProUGUI displayModeLabel;
    [SerializeField] private TextMeshProUGUI backButtonText;
    [SerializeField] private TextMeshProUGUI russianText;
    [SerializeField] private TextMeshProUGUI englishText;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Menus")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject settingsMenu;

    [Header("Language Toggles")]
    [SerializeField] private Toggle ruLanguageToggle;
    [SerializeField] private Toggle enLanguageToggle;

    private void Start()
    {
        backButton.onClick.AddListener(CloseSettings);
        LoadSettings();

        if (G.LocalizationManager != null)
        {
            G.LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        }

        UpdateAllTexts();
        SetupLanguageToggles();
    }

    private void OnDestroy()
    {
        if (G.LocalizationManager != null)
        {
            G.LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
        }
    }

    private void LoadSettings()
    {
        //var settings = G.SaveManager?.Settings;
        //if (settings == null) return;

        //// Разрешение и режим экрана загружаются автоматически при старте Unity
        //volumeSlider.value = settings.masterVolume;


        //volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SaveSettings()
    {
        //var settings = new GameSettings
        //{
        //    masterVolume = volumeSlider.value,
        //};
        //G.SaveManager?.SaveSettings(settings);
    }

    public void ChangeResolution()
    {
        switch (resolutionDropdown.value)
        {
            case 0: Screen.SetResolution(800, 600, Screen.fullScreenMode); break;
            case 1: Screen.SetResolution(1280, 720, Screen.fullScreenMode); break;
            case 2: Screen.SetResolution(1440, 900, Screen.fullScreenMode); break;
            case 3: Screen.SetResolution(1920, 1080, Screen.fullScreenMode); break;
            case 4: Screen.SetResolution(2560, 1440, Screen.fullScreenMode); break;
        }
    }

    public void ChangeDisplayMode()
    {
        switch (displayModeDropdown.value)
        {
            case 0: Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen; break;
            case 1: Screen.fullScreenMode = FullScreenMode.FullScreenWindow; break;
            case 2: Screen.fullScreenMode = FullScreenMode.Windowed; break;
        }
    }

    private void OnLanguageChanged(LocalizationManager.Language lang)
    {
        UpdateAllTexts();
    }

    private void UpdateAllTexts()
    {
        var loc = G.LocalizationManager;
        if (loc == null) return;

        if (settingsTitle != null) settingsTitle.text = loc.GetText("Settings");
        if (languageLabel != null) languageLabel.text = loc.GetText("Language");
        if (volumeLabel != null) volumeLabel.text = loc.GetText("Volume");
        if (resolutionLabel != null) resolutionLabel.text = loc.GetText("Resolution");
        if (displayModeLabel != null) displayModeLabel.text = loc.GetText("DisplayMode");
        if (backButtonText != null) backButtonText.text = loc.GetText("Back");
        if (russianText != null) russianText.text = loc.GetText("Russian");
        if (englishText != null) englishText.text = loc.GetText("English");
    }

    private void SetupLanguageToggles()
    {
        var currentLang = G.LocalizationManager?.CurrentLanguage ?? LocalizationManager.Language.Russian;

        ruLanguageToggle.isOn = currentLang == LocalizationManager.Language.Russian;
        enLanguageToggle.isOn = currentLang == LocalizationManager.Language.English;

        ruLanguageToggle.onValueChanged.AddListener((value) =>
        {
            if (value) G.LocalizationManager?.SetLanguage(LocalizationManager.Language.Russian);
        });

        enLanguageToggle.onValueChanged.AddListener((value) =>
        {
            if (value) G.LocalizationManager?.SetLanguage(LocalizationManager.Language.English);
        });
    }

    private void SetVolume(float volume)
    {
        float volumeDB = Mathf.Log10(Mathf.Max(volume, 0.001f)) * 20;
        audioMixer?.SetFloat("MasterVolume", volumeDB);
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }

    public void CloseSettings()
    {
        settingsMenu.SetActive(false);
        if (mainMenu != null) mainMenu.SetActive(true);
    }
}
