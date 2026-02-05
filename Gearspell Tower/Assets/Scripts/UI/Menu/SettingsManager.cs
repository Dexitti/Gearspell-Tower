using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Dropdown resolutionDropdown;
    [SerializeField] private Dropdown displayModeDropdown;
    [SerializeField] private Toggle ruLanguageToggle;
    [SerializeField] private Toggle enLanguageToggle;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button backButton;

    [SerializeField] private AudioMixer audioMixer;

    private void Start()
    {
        
        backButton.onClick.AddListener(CloseSettings);
    }

    public void ChangeResolution()
    {
        switch (resolutionDropdown.value)
        {
            case 0: Screen.SetResolution(800, 600, true); break;
            case 1: Screen.SetResolution(1280, 720, true); break;
            case 2: Screen.SetResolution(1440, 900, true); break;
            case 3: Screen.SetResolution(1920, 1080, true); break;
            case 4: Screen.SetResolution(2560, 1440, true); break;
        }
    }

    public void ChangeDisplayMode()
    {
        switch (displayModeDropdown.value)
        {
            case 0: Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen; break;
            case 1: Screen.fullScreenMode = FullScreenMode.FullScreenWindow; break;
            case 2: Screen.fullScreenMode = FullScreenMode.Windowed; break;
        }
    }

    public void ChangeLanguage()
    {
        bool isRussian = PlayerPrefs.GetInt("Language", 0) == 0;
        ruLanguageToggle.isOn = isRussian;
        enLanguageToggle.isOn = !isRussian;

        ruLanguageToggle.onValueChanged.AddListener((value) => {
            if (value)
            {
                enLanguageToggle.isOn = false;
                //SetLanguage("ru");
            }
        });

        enLanguageToggle.onValueChanged.AddListener((value) => {
            if (value)
            {
                ruLanguageToggle.isOn = false;
                //SetLanguage("en");
            }
        });
    }

    public void ChangeMasterVolume()
    {
        //float volumeDB = Mathf.Log10(volume) * 20;

        //if (audioMixer != null)
        //    audioMixer.SetFloat("MasterVolume", volumeDB);

        //// Сохраняем значение
        //PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }

    public void CloseSettings()
    {
        gameObject.SetActive(false);
    }
}
