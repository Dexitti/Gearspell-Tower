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

    [Header("Control Buttons")]
    [SerializeField] private Button speedToggleButton;
    [SerializeField] private Image speedButtonImage;
    [SerializeField] private Sprite[] speedSprite = new Sprite[2];
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button settingsButton;

    [Header("Pause Panel")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;

    private bool isSpeedX2 = false;
    private bool isPaused = false;

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

        if (G.EventManager == null) return;
        G.EventManager.OnGameStateChanged += OnGameStateChanged;
        G.EventManager.OnTowerHealthChanged += UpdateHealthBar;
        G.EventManager.OnWaveStarted += UpdateWaveNumber;
        G.EventManager.OnGearsChanged += UpdateGears;

        SetupButtons();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && upgradeIndicator.activeSelf)
            OpenUpgradeMenu();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
        }
    }

    private void OnDestroy()
    {
        if (G.EventManager == null) return;
        G.EventManager.OnGameStateChanged -= OnGameStateChanged;
        G.EventManager.OnTowerHealthChanged -= UpdateHealthBar;
        G.EventManager.OnWaveStarted -= UpdateWaveNumber;
        G.EventManager.OnGearsChanged -= UpdateGears;
    }

    private void SetupButtons()
    {
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OpenUpgradeMenu);

        if (speedToggleButton != null)
            speedToggleButton.onClick.AddListener(ToggleSpeed);

        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

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

    private void OnEquipmentUpgraded(int amount)
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
        G.UpgradeSystem?.OpenUpgradeMenu();
        G.GameManager?.PauseGame();
    }


    private void ToggleSpeed()
    {
        isSpeedX2 = !isSpeedX2;
        Time.timeScale = isSpeedX2 ? 2f : 1f;
        speedButtonImage.sprite = isSpeedX2 ? speedSprite[1] : speedSprite[0];
    }

    private void TogglePause()
    {
        if (G.GameManager == null) return;

        if (isPaused)
            G.GameManager.ResumeGame();
        else
            G.GameManager.PauseGame();
    }

    private void OpenSettings()
    {
        // TODO: Открыть панель настроек поверх игры
        Debug.Log("[HUD] Open Settings");
        G.GameManager?.PauseGame();
    }

    private void OnGameStateChanged(GameState state)
    {
        isPaused = (state == GameState.Paused);

        if (pausePanel != null)
            pausePanel.SetActive(isPaused);
    }

    private void ResumeGame()
    {
        G.GameManager?.ResumeGame();
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