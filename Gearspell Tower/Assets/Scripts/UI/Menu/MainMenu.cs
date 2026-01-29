using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
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
        SceneManager.LoadScene("Settings", LoadSceneMode.Single);
    }

    public void Exit()
    {
        Application.Quit();
        Debug.Log("Exit pressed!");
    }

}
