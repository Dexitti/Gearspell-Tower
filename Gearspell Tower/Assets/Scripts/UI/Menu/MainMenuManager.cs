using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum MenuState
{
    MainMenu,
    Settings
}

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button continueBtn;
    [SerializeField] private Button newGameBtn;
    [SerializeField] private Button settingsBtn;
    [SerializeField] private Button exitBtn;

    [Header("Localized Texts")]
    [SerializeField] private TextMeshProUGUI continueText;
    [SerializeField] private TextMeshProUGUI newGameText;
    [SerializeField] private TextMeshProUGUI settingsText;
    [SerializeField] private TextMeshProUGUI exitText;

    [Header("Menus")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject settingsMenu;

    [Header("Language Toggle")]
    [SerializeField] private Button languageToggleBtn;
    [SerializeField] private TextMeshProUGUI languageBtnText;

    private void Start()
    {
        bool hasSave = G.SaveManager != null && G.SaveManager.HasSave;
        continueBtn.interactable = hasSave;

        continueBtn.onClick.AddListener(ContinueGame);
        newGameBtn.onClick.AddListener(StartNewGame);
        settingsBtn.onClick.AddListener(OpenSettings);
        exitBtn.onClick.AddListener(ExitGame);

        if (languageToggleBtn != null)
        {
            languageToggleBtn.onClick.AddListener(ToggleLanguage);
            UpdateLanguageButtonText();
        }

        if (G.LocalizationManager != null)
        {
            G.LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        }

        ShowMainMenu();
        UpdateAllTexts();
    }

    private void OnDestroy()
    {
        if (G.LocalizationManager != null)
        {
            G.LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
        }
    }

    private void OnLanguageChanged(LocalizationManager.Language lang)
    {
        UpdateAllTexts();
        UpdateLanguageButtonText();
    }

    private void UpdateAllTexts()
    {
        var loc = G.LocalizationManager;
        if (loc == null) return;

        if (continueText != null) continueText.text = loc.GetText("Continue");
        if (newGameText != null) newGameText.text = loc.GetText("NewGame");
        if (settingsText != null) settingsText.text = loc.GetText("Settings");
        if (exitText != null) exitText.text = loc.GetText("Exit");
    }

    private void UpdateLanguageButtonText()
    {
        if (languageBtnText != null && G.LocalizationManager != null)
        {
            languageBtnText.text = G.LocalizationManager.CurrentLanguage == LocalizationManager.Language.Russian ? "EN" : "RU";
        }
    }

    private void ToggleLanguage()
    {
        G.LocalizationManager?.ToggleLanguage();
    }

    public void ContinueGame()
    {
        Debug.Log("[MainMenu] Continue game");
        G.GameManager?.StartNewGame();

        // TODO: Загрузить сохранения игры
    }

    public void StartNewGame()
    {
        Debug.Log("[MainMenu] New game");
        G.SaveManager?.ClearSave();
        G.GameManager?.StartNewGame();
    }

    public void OpenSettings()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        if (settingsMenu != null) settingsMenu.SetActive(false);

        // Обновляем ContinueBtn
        if (continueBtn != null && G.SaveManager != null)
            continueBtn.interactable = G.SaveManager.HasSave;
    }

    public void ExitGame()
    {
        Debug.Log("[MainMenu] Exit game");
        Application.Quit();

    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif
    }
}
