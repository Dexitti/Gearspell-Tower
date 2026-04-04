using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } //Singleton

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else Instance = this;
    }

    public void SetTimeScale(float scale)
    {
        Time.timeScale = scale; // 0 - freeze, 1 - normal, 2 - time x2
    }
}
