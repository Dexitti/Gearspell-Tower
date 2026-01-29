using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("—сылки на UI элементы")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown displayModeDropdown;
    public Button ruBtn;
    public Button enBtn;
    public Slider volumeSlider;
    public AudioMixer audioMixer;


    public void ChangeResolution()
    {
        if (resolutionDropdown.value == 0) Screen.SetResolution(800, 600, true);
        else if (resolutionDropdown.value == 1) Screen.SetResolution(1280, 720, true);
        else if (resolutionDropdown.value == 2) Screen.SetResolution(1440, 900, true);
        else if (resolutionDropdown.value == 3) Screen.SetResolution(1920, 1080, true);
        else if (resolutionDropdown.value == 4) Screen.SetResolution(2560, 1440, true);
    }

    public void ChangeDisplayMode()
    {

    }

    public void ChangeLanguage()
    {

    }

    public void ChangeMasterVolume()
    {

    }
}
