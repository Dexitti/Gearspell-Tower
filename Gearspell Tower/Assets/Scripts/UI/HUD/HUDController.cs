using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Game Values")]
    [SerializeField] private Scrollbar healthBar;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI waveNumberText;
    [SerializeField] private TextMeshProUGUI gearsText;

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

        if (G.EventManager == null) return;
        G.EventManager.OnGameStateChanged += OnGameStateChanged;
        G.EventManager.OnTowerHealthChanged += UpdateHealthBar;
        G.EventManager.OnWaveStarted += UpdateWaveNumber;
        G.EventManager.OnGearsChanged += UpdateGears;
        G.EventManager.OnEquipmentUpgraded += OnEquipmentUpgraded;

        SetupButtons();

        if (G.GameManager != null)
            OnGameStateChanged(G.GameManager.CurrentState);
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
        G.EventManager.OnWaveStarted -= UpdateWaveNumber;
        G.EventManager.OnGearsChanged -= UpdateGears;
        G.EventManager.OnEquipmentUpgraded -= OnEquipmentUpgraded;
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
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        healthBar.size = currentHealth / maxHealth;
        healthBar.value = 1f;

        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(currentHealth)}";
            //healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{maxHealth}";
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

    private void OnEquipmentUpgraded(EquipmentData data, int newLevel)
    {
        CheckUpgradeAvailability();
    }

    private void CheckUpgradeAvailability()
    {
        if (upgradeIndicator == null) return;

        int cheapestCost = G.UpgradeSystem.GetCheapestUpgradeCost();
        int currentGears = G.ResourceManager?.Gears ?? 0;

        bool isAvailable = cheapestCost > 0 && currentGears >= cheapestCost;
        upgradeIndicator.SetActive(isAvailable);
    }

    private void OpenUpgradeMenu()
    {
        if (G.GameManager != null && G.GameManager.CurrentState == GameState.Playing)
            G.UpgradeSystem?.OpenUpgradeMenu();
    }

    private void ToggleSpeed()
    {
        isSpeedX2 = !isSpeedX2;
        Time.timeScale = isSpeedX2 ? 2f : 1f;
        speedButtonImage.sprite = isSpeedX2 ? speedSprite[1] : speedSprite[0];
    }

    private void OnGameStateChanged(GameState state)
    {
        if (pausePanel != null)
            pausePanel.SetActive(state == GameState.Paused);

        if (pauseButton != null)
            pauseButton.interactable = (state == GameState.Playing || state == GameState.Paused);
    }

    private void TogglePause()
    {
        if (G.GameManager == null) return;

        if (G.GameManager.CurrentState == GameState.Paused)
            G.GameManager.ResumeGame();
        else if (G.GameManager.CurrentState == GameState.Playing)
            G.GameManager.PauseGame();
    }

    private void ResumeGame()
    {
        G.GameManager?.ResumeGame();
    }

    private void OpenSettings()
    {
        settingsManager.Open(pausePanel);
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        G.GameManager?.ReturnToMainMenu();
    }

    // === Публичные методы для обновления извне ===
    public void ShowGameOver()
    {
        // TODO: Показать панель Game Over
    }

    public void ShowVictory()
    {
        // TODO: Показать панель Victory
    }
}