using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject settingsMenu;

    private void Start()
    {
        continueBtn.interactable = false;

        continueBtn.onClick.AddListener(Continue);
        newGameBtn.onClick.AddListener(PlayNew);
        settingsBtn.onClick.AddListener(OpenSettings);
        exitBtn.onClick.AddListener(ExitGame);

        if (mainMenu != null) mainMenu.SetActive(true);
        if (settingsMenu != null) settingsMenu.SetActive(false);
    }

    public void Continue()
    {
        SceneManager.LoadScene("Game");
    }

    public void PlayNew()
    {
        SceneManager.LoadScene("Game");
    }

    public void OpenSettings()
    {
        if (mainMenu != null) mainMenu.SetActive(false);
        if (settingsMenu != null) settingsMenu.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Exit pressed!");
    }
}
