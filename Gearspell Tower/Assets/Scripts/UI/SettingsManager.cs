using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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

    [Header("Language Toggles")]
    [SerializeField] private Toggle ruLanguageToggle;
    [SerializeField] private Toggle enLanguageToggle;

    [Header("Reset Progress")]
    [SerializeField] private Button resetProgressButton;
    [SerializeField] private GameObject confirmResetPanel;

    private GameObject previousPanel;
    private bool isSyncingUi;
    private bool wasResolutionExpanded;
    private bool wasDisplayModeExpanded;

    public bool IsOpen => settingsPanel != null && settingsPanel.activeSelf;

    private void Awake()
    {
        settingsPanel.SetActive(false);
    }

    private void Start()
    {
        backButton.onClick.AddListener(CloseSettings);
        resetProgressButton.onClick.AddListener(ShowResetConfirm);

        volumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.AddListener(_ => OnResolutionChanged());
            wasResolutionExpanded = resolutionDropdown.IsExpanded;
        }
        if (displayModeDropdown != null)
        {
            displayModeDropdown.onValueChanged.AddListener(_ => OnDisplayModeChanged());
            wasDisplayModeExpanded = displayModeDropdown.IsExpanded;
        }

        LoadSettings();
        SetupLanguageToggles();

        if (G.LocalizationManager != null)
            G.LocalizationManager.OnLanguageChanged += OnLanguageChanged;

        UpdateAllTexts();
    }

    private void Update()
    {
        HandleDropdownExpandState();
    }

    private void OnDestroy()
    {
        if (G.LocalizationManager != null)
            G.LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
    }

    public void Open(GameObject previousPanel)
    {
        this.previousPanel = previousPanel;
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        G.AudioManager?.PlayButtonClick();
        settingsPanel.SetActive(false);
        if (previousPanel != null)
            previousPanel.SetActive(true);

        SaveSettings();
    }

    private void LoadSettings()
    {
        var settings = G.SaveManager?.Settings;
        if (settings == null) return;

        isSyncingUi = true;
        volumeSlider.SetValueWithoutNotify(settings.masterVolume);

        if (resolutionDropdown != null)
            resolutionDropdown.SetValueWithoutNotify(settings.resolutionIndex);
        if (displayModeDropdown != null)
            displayModeDropdown.SetValueWithoutNotify(settings.displayModeIndex);
        isSyncingUi = false;
    }

    public void SaveSettings()
    {
        if (G.SaveManager == null) return;
        G.SaveManager.SaveGameSettings(CollectSettings());
    }

    private PlayerSettings CollectSettings()
    {
        var settings = G.SaveManager.Settings;
        settings.masterVolume = volumeSlider.value;
        settings.resolutionIndex = resolutionDropdown != null ? resolutionDropdown.value : settings.resolutionIndex;
        settings.displayModeIndex = displayModeDropdown != null ? displayModeDropdown.value : settings.displayModeIndex;
        settings.language = G.LocalizationManager?.CurrentLanguage ?? settings.language;
        return settings;
    }

    public void ChangeResolution()
    {
        OnResolutionChanged();
    }

    public void ChangeDisplayMode()
    {
        OnDisplayModeChanged();
    }

    private void OnResolutionChanged()
    {
        if (isSyncingUi || resolutionDropdown == null) return;

        SaveManager.ApplyDisplayMode(displayModeDropdown != null ? displayModeDropdown.value : G.SaveManager.Settings.displayModeIndex);
        SaveManager.ApplyResolution(resolutionDropdown.value);
        SaveSettings();
        ForceCloseDropdown(resolutionDropdown);
    }

    private void OnDisplayModeChanged()
    {
        if (isSyncingUi || displayModeDropdown == null) return;

        SaveManager.ApplyDisplayMode(displayModeDropdown.value);
        SaveManager.ApplyResolution(resolutionDropdown != null ? resolutionDropdown.value : G.SaveManager.Settings.resolutionIndex);
        SaveSettings();
        ForceCloseDropdown(displayModeDropdown);
    }

    private void HandleDropdownExpandState()
    {
        if (resolutionDropdown != null)
        {
            bool isExpanded = resolutionDropdown.IsExpanded;
            if (isExpanded != wasResolutionExpanded)
            {
                G.AudioManager?.PlayButtonClick();
                wasResolutionExpanded = isExpanded;
            }
        }

        if (displayModeDropdown != null)
        {
            bool isExpanded = displayModeDropdown.IsExpanded;
            if (isExpanded != wasDisplayModeExpanded)
            {
                G.AudioManager?.PlayButtonClick();
                wasDisplayModeExpanded = isExpanded;
            }
        }
    }

    private static void ForceCloseDropdown(TMP_Dropdown dropdown)
    {
        if (dropdown == null || !dropdown.IsExpanded) return;
        dropdown.Hide();
    }

    private void OnLanguageChanged(LocalizationManager.Language lang)
    {
        UpdateAllTexts();
        SaveSettings();
    }

    private void OnMasterVolumeChanged(float value)
    {
        if (G.AudioManager != null)
            G.AudioManager.MasterVolume = value;

        if (G.SaveManager != null)
        {
            var settings = G.SaveManager.Settings;
            settings.masterVolume = value;
            G.SaveManager.SaveGameSettings(settings);
        }
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

    private void ShowResetConfirm()
    {
        G.AudioManager?.PlayButtonClick();
        confirmResetPanel.SetActive(true);
    }

    public void ConfirmResetProgress()
    {
        G.AudioManager?.PlayButtonClick();
        G.SaveManager?.ResetGlobalProgress();
        G.ProgressManager?.ClearSession();

        confirmResetPanel.SetActive(false);
        LoadSettings();

        G.GameManager?.ReturnToMainMenu();

        Debug.Log("[Settings] Progress reset, returning to main menu");
    }

    public void CancelResetProgress()
    {
        G.AudioManager?.PlayButtonClick();
        confirmResetPanel.SetActive(false);
    }
}
