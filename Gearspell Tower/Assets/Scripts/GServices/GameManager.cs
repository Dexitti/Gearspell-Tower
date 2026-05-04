using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    Victory,
    GameOver
}

/// <summary>
/// Singleton.
/// ”правл€ет основным игровым циклом
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

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        G.EventManager?.TriggerGameStateChanged(_currentState); // ƒл€ работы UI при отладке
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

        if (newState == GameState.Playing || newState == GameState.MainMenu)
            Time.timeScale = normalTimeScale;
        else
            Time.timeScale = pausedTimeScale;
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
            CurrentState = GameState.Paused;
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
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

    public void StartNewGame()
    {
        SceneManager.LoadScene("Game");
    }
}
