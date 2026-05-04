using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject settingsPanel;
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

    [Header("Language Toggles")]
    [SerializeField] private Toggle ruLanguageToggle;
    [SerializeField] private Toggle enLanguageToggle;

    [Header("Reset Progress")]
    [SerializeField] private Button resetProgressButton;
    [SerializeField] private GameObject confirmResetPanel;

    private GameObject previousPanel;

    private void Awake()
    {
        settingsPanel.SetActive(false);
    }

    private void Start()
    {
        backButton.onClick.AddListener(CloseSettings);
        resetProgressButton.onClick.AddListener(ShowResetConfirm);
        LoadSettings();
        SetupLanguageToggles();

        if (G.LocalizationManager != null)
        {
            G.LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        }

        UpdateAllTexts();
    }

    private void OnDestroy()
    {
        if (G.LocalizationManager != null)
        {
            G.LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
        }
    }

    public void Open(GameObject previousPanel)
    {
        this.previousPanel = previousPanel;
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        if (previousPanel != null)
            previousPanel.SetActive(true);
        else
            Debug.LogWarning("[SettingsManager] No previous panel to return to");

        SaveSettings();
    }

    private void LoadSettings()
    {
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        volumeSlider.value = savedVolume;
        SetVolume(savedVolume);

        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", volumeSlider.value);
        PlayerPrefs.Save();
        //G.SaveManager?.SaveSettings(settings);
    }

    public void ChangeResolution()
    {
        switch (resolutionDropdown.value)
        {
            case 0: Screen.SetResolution(2560, 1440, Screen.fullScreenMode); break;
            case 1: Screen.SetResolution(1920, 1080, Screen.fullScreenMode); break;
            case 2: Screen.SetResolution(1440, 900, Screen.fullScreenMode); break;
            case 3: Screen.SetResolution(1280, 720, Screen.fullScreenMode); break;
            case 4: Screen.SetResolution(800, 600, Screen.fullScreenMode); break;
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
        if (displayModeDropdown != null && displayModeDropdown.options.Count >= 3)
        {
            var label = displayModeDropdown.captionText;
            if (label != null) label.text = loc.GetText("DisplayMode");
            displayModeDropdown.options[0].text = loc.GetText("Fullscreen");
            displayModeDropdown.options[1].text = loc.GetText("Borderless");
            displayModeDropdown.options[2].text = loc.GetText("Windowed");
        }
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
            if (value)
            {
                enLanguageToggle.SetIsOnWithoutNotify(false);
                G.LocalizationManager?.SetLanguage(LocalizationManager.Language.Russian);
            }
        });

        enLanguageToggle.onValueChanged.AddListener((value) =>
        {
            if (value)
            {
                ruLanguageToggle.SetIsOnWithoutNotify(false);
                G.LocalizationManager?.SetLanguage(LocalizationManager.Language.English);
            }
        });
    }

    private void SetVolume(float volume)
    {
        //float volumeDB = Mathf.Log10(Mathf.Max(volume, 0.001f)) * 20;
        //audioMixer?.SetFloat("MasterVolume", volumeDB);
        //PlayerPrefs.SetFloat("MasterVolume", volume);
        //PlayerPrefs.Save();
    }

    private void ShowResetConfirm()
    {
        confirmResetPanel.SetActive(true);
    }

    public void ConfirmResetProgress()
    {
        G.SaveManager?.ResetGlobalProgress();
        G.ProgressManager?.ClearSession();

        confirmResetPanel.SetActive(false);

        // Возвращаем в главное меню
        G.GameManager?.ReturnToMainMenu();

        Debug.Log("[Settings] Progress reset, returning to main menu");
    }

    public void CancelResetProgress()
    {
        confirmResetPanel.SetActive(false);
    }
}
