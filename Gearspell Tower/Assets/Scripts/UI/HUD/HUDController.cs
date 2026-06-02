using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HUDController : MonoBehaviour
{
    [Header("Game Values")]
    [SerializeField] private Scrollbar healthBar;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI waveNumberText;
    [SerializeField] private TextMeshProUGUI gearsText;
    [SerializeField] private TextMeshProUGUI waveNameText;
    [SerializeField] private EquipmentInventoryPanel inventoryPanel;

    [Header("Upgrade Available Indicator")]
    [SerializeField] private GameObject upgradeIndicator;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private UpgradeScreen upgradeScreen;

    [Header("Control Buttons")]
    [SerializeField] private Button speedToggleButton;
    [SerializeField] private Image speedButtonImage;
    [SerializeField] private Sprite[] speedSprite = new Sprite[2];
    [SerializeField] private Button pauseButton;

    [Header("Pause Screen")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private SettingsManager settingsManager;
    [SerializeField] private Button mainMenuButton;

    [Header("End Panels")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private Button victoryRestartButton;
    [SerializeField] private Button victoryMainMenuButton;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button gameOverRestartButton;
    [SerializeField] private Button gameOverMainMenuButton;

    private bool isSpeedX2 = false;

    private void Awake()
    {
        healthBar = GetComponentInChildren<Scrollbar>();
        speedButtonImage = speedToggleButton.GetComponent<Image>();
    }

    private void Start()
    {
        // Начальные значения
        waveNumberText.text = "1";
        gearsText.text = "0";
        speedButtonImage.sprite = speedSprite[0];
        upgradeIndicator.SetActive(false);
        pausePanel.SetActive(false);
        victoryPanel.SetActive(false);
        gameOverPanel.SetActive(false);

        if (G.EventManager == null) return;
        G.EventManager.OnGameStateChanged += OnGameStateChanged;
        G.EventManager.OnTowerHealthChanged += UpdateHealthBar;
        G.EventManager.OnGameplayInitialized += OnGameplayInitialized;
        G.EventManager.OnWaveStarted += UpdateWaveNumber;
        G.EventManager.OnGearsChanged += UpdateGears;
        G.EventManager.OnWaveStarted += ShowWaveName;
        G.EventManager.OnEquipmentUnlocked += OnEquipmentUnlocked;
        G.EventManager.OnSlotUnlocked += (int slot) => inventoryPanel?.Refresh();
        G.EventManager.OnEquipmentEquipped += (data, slot) => inventoryPanel?.Refresh();
        G.EventManager.OnEquipmentUpgraded += OnEquipmentUpgraded;
        G.EventManager.OnTowerDestroyed += OnTowerDestroyed;
        G.EventManager.OnWaveCompleted += OnWaveCompleted;

        SetupButtons();

        if (G.EventManager.IsGameplayInitialized)
            OnGameplayInitialized();

        if (G.GameManager != null)
            OnGameStateChanged(G.GameManager.CurrentState);
    }

    private void OnGameplayInitialized()
    {
        RefreshHealthBar();

        if (G.ResourceManager != null)
            UpdateGears(G.ResourceManager.Gears);

        if (G.GameLoopManager != null)
            UpdateWaveNumber(G.GameLoopManager.GetCurrentWaveNumber());

        inventoryPanel?.Refresh();
    }

    private void SetupButtons()
    {
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OpenUpgradeMenu);

        if (speedToggleButton != null)
            speedToggleButton.onClick.AddListener(ToggleSpeed);

        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);

        if (victoryRestartButton != null)
            victoryRestartButton.onClick.AddListener(RestartGame);

        if (victoryMainMenuButton != null)
            victoryMainMenuButton.onClick.AddListener(ReturnToMainMenu);

        if (gameOverRestartButton != null)
            gameOverRestartButton.onClick.AddListener(RestartGame);

        if (gameOverMainMenuButton != null)
            gameOverMainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && upgradeIndicator.activeSelf)
            OpenUpgradeMenu();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (upgradeScreen != null && upgradeScreen.IsOpen)
            {
                upgradeScreen.Close();
                return;
            }

            if (settingsManager != null && settingsManager.IsOpen)
            {
                settingsManager.CloseSettings();
                return;
            }

            TogglePause();
        }
    }

    private void OnDestroy()
    {
        if (G.EventManager == null) return;
        G.EventManager.OnGameStateChanged -= OnGameStateChanged;
        G.EventManager.OnTowerHealthChanged -= UpdateHealthBar;
        G.EventManager.OnGameplayInitialized -= OnGameplayInitialized;
        G.EventManager.OnWaveStarted -= UpdateWaveNumber;
        G.EventManager.OnGearsChanged -= UpdateGears;
        G.EventManager.OnWaveStarted -= ShowWaveName;
        G.EventManager.OnEquipmentUpgraded -= OnEquipmentUpgraded;
        G.EventManager.OnEquipmentUnlocked -= OnEquipmentUnlocked;
        G.EventManager.OnTowerDestroyed -= OnTowerDestroyed;
        G.EventManager.OnWaveCompleted -= OnWaveCompleted;
    }

    private void RefreshHealthBar()
    {
        var health = G.Tower?.GetComponent<HealthComponent>();
        if (health == null) return;
        UpdateHealthBar(health.CurrentHealth, health.MaxHealth);
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBar == null || maxHealth <= 0f) return;

        healthBar.size = currentHealth / maxHealth;
        healthBar.value = 1f;

        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(currentHealth)}";
    }

    private void UpdateWaveNumber(int waveNumber)
    {
        waveNumberText?.SetText(waveNumber.ToString());
    }

    private void UpdateGears(int amount)
    {
        gearsText?.SetText(amount.ToString());
        CheckUpgradeAvailability();
    }

    private void ShowWaveName(int waveNumber)
    {
        StartCoroutine(ShowWaveNameCoroutine(waveNumber));
    }

    private IEnumerator ShowWaveNameCoroutine(int waveNumber)
    {
        waveNameText.text = G.LocalizationManager.GetText($"WaveName_{G.GameLoopManager.GetCurrentWaveNumber()}");
        yield return new WaitForSeconds(2f);
        waveNameText.text = "";
    }

    private void OnEquipmentChanged(EquipmentData data, int level)
    {
        inventoryPanel?.Refresh();
        CheckUpgradeAvailability();
    }

    private void OnEquipmentUnlocked(EquipmentData[] equipment)
    {
        inventoryPanel?.Refresh();
        CheckUpgradeAvailability();
    }

    private void OnEquipmentUpgraded(EquipmentData data, int newLevel)
    {
        inventoryPanel?.Refresh();
        CheckUpgradeAvailability();
    }

    private void CheckUpgradeAvailability()
    {
        int cheapestCost = G.UpgradeSystem.GetCheapestUpgradeCost();
        int currentGears = G.ResourceManager?.Gears ?? 0;

        bool isAvailable = cheapestCost > 0 && currentGears >= cheapestCost;
        upgradeIndicator.SetActive(isAvailable);
    }

    private void OpenUpgradeMenu()
    {
        if (G.GameManager.CurrentState == GameState.Playing)
        {
            G.AudioManager?.PlayButtonClick();
            G.UpgradeSystem?.OpenUpgradeMenu();
        }
    }

    private void ToggleSpeed()
    {
        G.AudioManager?.PlayHUDClick();
        isSpeedX2 = !isSpeedX2;
        Time.timeScale = isSpeedX2 ? 2f : 1f;
        speedButtonImage.sprite = isSpeedX2 ? speedSprite[1] : speedSprite[0];
    }

    private void OnGameStateChanged(GameState state)
    {
        pausePanel.SetActive(state == GameState.Paused);
        pauseButton.interactable = (state == GameState.Playing || state == GameState.Paused);
    }

    private void TogglePause()
    {
        G.AudioManager?.PlayHUDClick();
        if (G.GameManager.CurrentState == GameState.Paused)
            G.GameManager.ResumeGame();
        else if (G.GameManager.CurrentState == GameState.Playing)
            G.GameManager.PauseGame();
    }

    private void ResumeGame()
    {
        G.AudioManager?.PlayButtonClick();
        G.GameManager?.ResumeGame();
    }

    private void OpenSettings()
    {
        G.AudioManager?.PlayButtonClick();
        settingsManager.Open(pausePanel);
    }

    private void RestartGame()
    {
        G.AudioManager?.PlayButtonClick();
        Time.timeScale = 1f;
        G.SaveManager?.ClearSave();
        G.ProgressManager?.ClearSession();
        G.ResourceManager?.ResetResources();
        G.GameManager?.StartNewGame();
    }

    private void ReturnToMainMenu()
    {
        G.AudioManager?.PlayButtonClick();
        Time.timeScale = 1f;
        G.GameManager?.ReturnToMainMenu();
    }

    private void OnWaveCompleted(int waveNumber)
    {
        // Проверка, что последняя волна
        int totalWaves = G.GameLoopManager?.GetTotalWaves() ?? 0;
        if (waveNumber >= totalWaves)
            ShowVictory();
    }

    public void ShowVictory()
    {
        victoryPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void OnTowerDestroyed()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}