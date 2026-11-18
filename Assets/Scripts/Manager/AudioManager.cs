using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Singleton instance that persists across scenes
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public struct Sound
    {
        public string soundName;
        public AudioClip clip;
    }

    [Header("Audio Sources")]
    [SerializeField]
    private AudioSource musicSource;

    [SerializeField]
    private AudioSource sfxSource;

    [Header("Sound Library")]
    [SerializeField]
    private List<Sound> soundLibrary = new List<Sound>();

    private Dictionary<string, AudioClip> soundDictionary;

    private void Awake()
    {
        // Enforce Singleton and ensure it is never destroyed across scene loads
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Auto-assign AudioSources if not set in inspector
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        // Populate dictionary for fast string-based lookups
        soundDictionary = new Dictionary<string, AudioClip>();
        foreach (var sound in soundLibrary)
        {
            if (!string.IsNullOrEmpty(sound.soundName) && sound.clip != null)
            {
                if (!soundDictionary.ContainsKey(sound.soundName))
                {
                    soundDictionary.Add(sound.soundName, sound.clip);
                }
            }
        }
    }

    /// <summary>
    /// Play background music using its string name.
    /// </summary>
    public void PlayMusic(string soundName)
    {
        if (soundDictionary.TryGetValue(soundName, out AudioClip clip))
        {
            if (musicSource.clip == clip && musicSource.isPlaying)
                return;

            musicSource.clip = clip;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning(
                $"[AudioManager] Music track '{soundName}' not found in sound library!"
            );
        }
    }

    /// <summary>
    /// Stop the background music.
    /// </summary>
    public void StopMusic()
    {
        musicSource.Stop();
    }

    /// <summary>
    /// Play a sound effect (SFX) using its string name.
    /// </summary>
    public void PlaySFX(string soundName, float volumeScale = 1f)
    {
        if (soundDictionary.TryGetValue(soundName, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip, volumeScale);
        }
        else
        {
            Debug.LogWarning($"[AudioManager] SFX '{soundName}' not found in sound library!");
        }
    }

    /// <summary>
    /// Plays background music and ensures it loops continuously.
    /// </summary>
    public void PlayMusicLoop(string soundName)
    {
        if (soundDictionary.TryGetValue(soundName, out AudioClip clip))
        {
            if (musicSource.clip == clip && musicSource.isPlaying)
                return;

            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning(
                $"[AudioManager] Music loop track '{soundName}' not found in sound library!"
            );
        }
    }

    /// <summary>
    /// Plays a looping sound effect on the SFX source.
    /// Note: This uses the main sfxSource, so it will loop this specific sound until StopSFXLoop() is called.
    /// </summary>
    public void PlaySFXLoop(string soundName)
    {
        if (soundDictionary.TryGetValue(soundName, out AudioClip clip))
        {
            sfxSource.clip = clip;
            sfxSource.loop = true;
            sfxSource.Play();
        }
        else
        {
            Debug.LogWarning($"[AudioManager] SFX loop '{soundName}' not found in sound library!");
        }
    }

    /// <summary>
    /// Stops the looping sound effect source.
    /// </summary>
    public void StopSFXLoop()
    {
        sfxSource.Stop();
        sfxSource.loop = false; // Reset loop flag
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp01(volume);
    }
}
