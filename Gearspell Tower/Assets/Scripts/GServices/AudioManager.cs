using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string masterVolumeParameter = "MasterVolume";

    [Header("Music Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip gameplayMusic;

    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;

    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();
    private Coroutine musicFadeCoroutine;

    public float MasterVolume
    {
        set
        {
            masterVolume = Mathf.Clamp01(value);
            audioMixer.SetFloat(masterVolumeParameter, ConvertToDecibels(masterVolume));
        }
        get => masterVolume;
    }

    private void Awake()
    {
        G.AudioManager = this;
        DontDestroyOnLoad(gameObject);
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
        InitializeSFXDictionary();
    }

    private void Start()
    {
        masterVolume = G.SaveManager != null
            ? G.SaveManager.Settings.masterVolume
            : PlayerPrefs.GetFloat("MasterVolume", 1f);
        ApplyVolumeSettings();
        G.EventManager.OnGameStateChanged += OnGameStateChanged;
        //G.EventManager.OnEnemyKilled += OnEnemyKilled;
        //G.EventManager.OnEnemyReachedTower += OnEnemyReachedTower;
    }

    private void InitializeSFXDictionary()
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Audio/Sounds");
        foreach (var clip in clips)
        {
            sfxDictionary[clip.name] = clip;
        }
        Debug.Log($"[AudioManager] Loaded {sfxDictionary.Count} SFX clips");
    }

    private void OnDestroy()
    {
        G.EventManager.OnGameStateChanged -= OnGameStateChanged;
    }

    // === Музыка ===
    private void OnGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.MainMenu:
                PlayMainMenuMusic();
                break;
            case GameState.Playing:
                PlayGameplayMusic();
                break;
        }
    }

    public void PlayMainMenuMusic()
    {
        if (musicSource.clip != mainMenuMusic || !musicSource.isPlaying)
        {
            StartFade(mainMenuMusic, 0.5f);
        }
    }

    public void PlayGameplayMusic()
    {
        if (musicSource.clip != gameplayMusic || !musicSource.isPlaying)
        {
            StartFade(gameplayMusic, 0.5f);
        }
    }

    private void StartFade(AudioClip newClip, float fadeDuration)
    {
        if (musicFadeCoroutine != null)
            StopCoroutine(musicFadeCoroutine);

        musicFadeCoroutine = StartCoroutine(FadeMusicCoroutine(newClip, fadeDuration));
    }

    private IEnumerator FadeMusicCoroutine(AudioClip newClip, float fadeDuration)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        // Fade out
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in
        elapsed = 0f;
        float targetVolume = masterVolume;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, t);
            yield return null;
        }
    }

    public void StopMusic()
    {
        if (musicFadeCoroutine != null)
            StopCoroutine(musicFadeCoroutine);
        musicSource.Stop();
    }

    // === SFX ===
    public void PlaySFX(string soundKey, float volumeScale = 1f)
    {
        if (sfxDictionary.TryGetValue(soundKey, out AudioClip clip) && clip != null)
        {
            sfxSource.PlayOneShot(clip, volumeScale);
        }
    }

    public void PlaySFXAtPosition(string soundKey, Vector3 position, float volumeScale = 1f)
    {
        if (sfxDictionary.TryGetValue(soundKey, out AudioClip clip) && clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, position, volumeScale * masterVolume);
        }
    }

    // === Play SFX ===
    public void PlayButtonClick()
    {
        PlaySFX("UI btn click sound");
    }

    public void PlayHUDClick()
    {
        PlaySFX("tick");
    }

    // === Settings ===
    private void ApplyVolumeSettings()
    {
        audioMixer.SetFloat(masterVolumeParameter, ConvertToDecibels(masterVolume));
    }

    private float ConvertToDecibels(float volume)
    {
        if (volume <= 0.0001f) return -80f;
        return Mathf.Log10(volume) * 20f;
    }

}
