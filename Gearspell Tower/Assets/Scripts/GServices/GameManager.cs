using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    Upgrade,
    Victory,
    GameOver
}

/// <summary>
/// Singleton.
/// Управляет основным игровым циклом
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] private float pausedTimeScale = 0f;
    [SerializeField] private float normalTimeScale = 1f;

    private GameState _currentState = GameState.MainMenu;

    public GameState CurrentState
    {
        get => _currentState;
        private set
        {
            if (_currentState != value)
            {
                _currentState = value;
                G.EventManager?.TriggerGameStateChanged(_currentState);
                OnStateChanged(_currentState);
            }
        }
    }

    private void Awake()
    {
        G.GameManager = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Авто-определение сцены при запуске из редактора
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "Game" && _currentState == GameState.MainMenu)
        {
            _currentState = GameState.Playing;
            OnStateChanged(_currentState);
        }

        G.EventManager?.TriggerGameStateChanged(_currentState); // Для работы UI при отладке
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "MainMenu":
                CurrentState = GameState.MainMenu;
                break;
            case "Game":
                CurrentState = GameState.Playing;
                break;
        }
    }

    private void OnStateChanged(GameState newState)
    {
        Debug.Log($"[GameManager] State changed to: {newState}");

        switch (newState)
        {
            case GameState.Playing:
            case GameState.MainMenu:
                Time.timeScale = normalTimeScale;
                break;
            case GameState.Paused:
            case GameState.Upgrade:
            case GameState.Victory:
            case GameState.GameOver:
                Time.timeScale = pausedTimeScale;
                break;
        }
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
            CurrentState = GameState.Paused;
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused || _currentState == GameState.Upgrade)
            CurrentState = GameState.Playing;
    }

    public void OpenUpgrade()
    {
        if (_currentState == GameState.Playing)
            CurrentState = GameState.Upgrade;
    }

    public void CloseUpgrade()
    {
        if (_currentState == GameState.Upgrade)
            CurrentState = GameState.Playing;
    }

    public void GameOver()
    {
        if (CurrentState == GameState.Playing)
            CurrentState = GameState.GameOver;
    }

    public void Victory()
    {
        if (CurrentState == GameState.Playing)
            CurrentState = GameState.Victory;
    }

    public void ReturnToMainMenu()
    {
        CurrentState = GameState.MainMenu;
        SceneManager.LoadScene("MainMenu");
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }
}
