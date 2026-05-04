using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Global Service Locator — единая точка доступа ко всем системам.
/// Заполняется через GameBootstrap на сцене.
/// </summary>
public static class G
{
    // GServices
    public static GameManager GameManager;
    public static EventManager EventManager;
    public static ResourceManager ResourceManager;
    //public static AudioManager AudioManager;
    public static ProgressManager ProgressManager;
    public static SaveManager SaveManager;
    public static LocalizationManager LocalizationManager;

    // Game Systems
    public static GameLoopManager GameLoopManager;
    public static SpawnManager SpawnManager;
    public static EquipmentManager EquipmentManager;
    public static UpgradeSystem UpgradeSystem;

    // Scene Objects
    public static Tower Tower;
    public static Camera MainCamera;

    // UI
    public static MainMenuManager MainMenu;
    public static HUDController HUD;
    public static SettingsManager Settings;
}

[DefaultExecutionOrder(-9999)]
public class GameBootstrap : MonoBehaviour
{
    private static GameObject _coreInstance;
    private static bool _coreInitialized = false;

    [SerializeField] private GameObject corePrefab;

    private void Awake()
    {
#if UNITY_EDITOR
        // В редакторе EditorAutoInit уже всё создал, выходим
        if (_coreInitialized)
        {
            CacheSceneReferences();
            return;
        }
#endif
        if (!_coreInitialized)
        {
            InitCoreManagers();
            _coreInitialized = true;
        }

        CacheSceneReferences();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InitCoreManagers()
    {
        if (corePrefab != null)
        {
            _coreInstance = Instantiate(corePrefab);
            _coreInstance.name = "Core";
        }
        else
        {
            _coreInstance = new GameObject("Core");
            _coreInstance.AddComponent<GameManager>();
            _coreInstance.AddComponent<EventManager>();
            _coreInstance.AddComponent<ResourceManager>();
            _coreInstance.AddComponent<ProgressManager>();
            _coreInstance.AddComponent<SaveManager>();
            _coreInstance.AddComponent<LocalizationManager>();
        }

        DontDestroyOnLoad(_coreInstance);

        G.EventManager = _coreInstance.GetComponent<EventManager>();
        G.GameManager = _coreInstance.GetComponent<GameManager>();
        G.ResourceManager = _coreInstance.GetComponent<ResourceManager>();
        G.ProgressManager = _coreInstance.GetComponent<ProgressManager>();
        G.SaveManager = _coreInstance.GetComponent<SaveManager>();
        G.LocalizationManager = _coreInstance.GetComponent<LocalizationManager>();

        Debug.Log("[GameBootstrap] Core managers initialized");
    }

    private void CacheSceneReferences()
    {
        // Game Systems
        G.GameLoopManager = FindFirstObjectByType<GameLoopManager>();
        G.SpawnManager = FindFirstObjectByType<SpawnManager>();
        G.EquipmentManager = FindFirstObjectByType<EquipmentManager>();
        G.UpgradeSystem = FindFirstObjectByType<UpgradeSystem>();

        // Scene Objects
        G.Tower = FindFirstObjectByType<Tower>();
        G.MainCamera = Camera.main;

        // UI
        G.MainMenu = FindFirstObjectByType<MainMenuManager>();
        G.HUD = FindFirstObjectByType<HUDController>();
        G.Settings = FindFirstObjectByType<SettingsManager>();

        Debug.Log($"[GameBootstrap] Scene '{SceneManager.GetActiveScene().name}' references cached");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Invoke(nameof(CacheSceneReferences), 0.01f);
    }

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EditorAutoInit()
    {
        _coreInitialized = false;
        // Всегда создаём менеджеры в редакторе при запуске ЛЮБОЙ сцены
        if (_coreInstance != null)
        {
            DestroyImmediate(_coreInstance);
            _coreInstance = null;
        }

        _coreInstance = new GameObject("Core (Editor Auto)");
        _coreInstance.AddComponent<GameManager>();
        _coreInstance.AddComponent<EventManager>();
        _coreInstance.AddComponent<ResourceManager>();
        _coreInstance.AddComponent<SaveManager>();
        _coreInstance.AddComponent<LocalizationManager>();
        _coreInstance.AddComponent<ProgressManager>();

        DontDestroyOnLoad(_coreInstance);

        G.EventManager = _coreInstance.GetComponent<EventManager>();
        G.GameManager = _coreInstance.GetComponent<GameManager>();
        G.ResourceManager = _coreInstance.GetComponent<ResourceManager>();
        G.SaveManager = _coreInstance.GetComponent<SaveManager>();
        G.LocalizationManager = _coreInstance.GetComponent<LocalizationManager>();
        G.ProgressManager = _coreInstance.GetComponent<ProgressManager>();

        _coreInitialized = true;
        Debug.Log("[GameBootstrap] Editor auto-init: All Core managers created for testing");
    }
#endif
}
