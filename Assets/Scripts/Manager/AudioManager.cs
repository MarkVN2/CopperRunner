using System.Collections;
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
    private AudioSource secondaryMusicSource;

    [SerializeField]
    private AudioSource sfxSource;

    [Header("Sound Library")]
    [SerializeField]
    private List<Sound> soundLibrary = new List<Sound>();

    private Dictionary<string, AudioClip> soundDictionary;
    private float musicVolume = 1f;
    private Coroutine crossfadeCoroutine;

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

        if (secondaryMusicSource == null)
        {
            secondaryMusicSource = gameObject.AddComponent<AudioSource>();
            secondaryMusicSource.loop = true;
            secondaryMusicSource.volume = 0f;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        musicVolume = musicSource.volume > 0f ? musicSource.volume : 1f;

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
    /// Play background music immediately without fading.
    /// </summary>
    public void PlayMusic(string soundName)
    {
        if (soundDictionary.TryGetValue(soundName, out AudioClip clip))
        {
            if (crossfadeCoroutine != null)
            {
                StopCoroutine(crossfadeCoroutine);
                crossfadeCoroutine = null;
            }

            secondaryMusicSource.Stop();
            secondaryMusicSource.volume = 0f;

            if (musicSource.clip == clip && musicSource.isPlaying)
            {
                musicSource.volume = musicVolume;
                return;
            }

            musicSource.clip = clip;
            musicSource.volume = musicVolume;
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
    /// Smoothly crossfade from the current music track to a new track over 'duration' seconds.
    /// </summary>
    public void CrossfadeMusic(string soundName, float duration = 1.0f, bool loop = true)
    {
        if (soundDictionary.TryGetValue(soundName, out AudioClip nextClip))
        {
            if (musicSource.clip == nextClip && musicSource.isPlaying && crossfadeCoroutine == null)
            {
                musicSource.loop = loop;
                return;
            }

            if (duration <= 0f)
            {
                PlayMusic(soundName);
                musicSource.loop = loop;
                return;
            }

            if (crossfadeCoroutine != null)
            {
                StopCoroutine(crossfadeCoroutine);
            }

            crossfadeCoroutine = StartCoroutine(CrossfadeRoutine(nextClip, duration, loop));
        }
        else
        {
            Debug.LogWarning(
                $"[AudioManager] Crossfade track '{soundName}' not found in sound library!"
            );
        }
    }

    private IEnumerator CrossfadeRoutine(AudioClip nextClip, float duration, bool loop)
    {
        // If nothing is currently playing on musicSource, just fade the new track in
        if (!musicSource.isPlaying || musicSource.clip == null)
        {
            musicSource.clip = nextClip;
            musicSource.loop = loop;
            musicSource.volume = 0f;
            musicSource.Play();

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0f, musicVolume, elapsed / duration);
                yield return null;
            }

            musicSource.volume = musicVolume;
            crossfadeCoroutine = null;
            yield break;
        }

        // Prepare incoming track on secondaryMusicSource
        secondaryMusicSource.clip = nextClip;
        secondaryMusicSource.loop = loop;
        secondaryMusicSource.volume = 0f;
        secondaryMusicSource.Play();

        float startPrimaryVolume = musicSource.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);

            musicSource.volume = Mathf.Lerp(startPrimaryVolume, 0f, t);
            secondaryMusicSource.volume = Mathf.Lerp(0f, musicVolume, t);
            yield return null;
        }

        // Swap references so musicSource always holds the active track
        float syncedTime = secondaryMusicSource.time;
        musicSource.Stop();
        musicSource.clip = nextClip;
        musicSource.loop = loop;
        musicSource.volume = musicVolume;
        musicSource.Play();
        if (!loop && nextClip.length > 0f)
        {
            musicSource.time = Mathf.Min(syncedTime, nextClip.length - 0.01f);
        }
        else
        {
            musicSource.time = syncedTime;
        }

        secondaryMusicSource.Stop();
        secondaryMusicSource.clip = null;
        secondaryMusicSource.volume = 0f;

        crossfadeCoroutine = null;
    }

    /// <summary>
    /// Stop the background music.
    /// </summary>
    public void StopMusic()
    {
        if (crossfadeCoroutine != null)
        {
            StopCoroutine(crossfadeCoroutine);
            crossfadeCoroutine = null;
        }

        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
            fadeOutCoroutine = null;
        }

        musicSource.Stop();
        secondaryMusicSource.Stop();
        secondaryMusicSource.volume = 0f;
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
            if (crossfadeCoroutine != null)
            {
                StopCoroutine(crossfadeCoroutine);
                crossfadeCoroutine = null;
            }

            secondaryMusicSource.Stop();
            secondaryMusicSource.volume = 0f;

            if (musicSource.clip == clip && musicSource.isPlaying)
            {
                musicSource.volume = musicVolume;
                return;
            }

            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.volume = musicVolume;
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

    private Coroutine fadeOutCoroutine;

    /// <summary>
    /// Smoothly fades out the current background music over 'duration' seconds and stops it.
    /// </summary>
    public void FadeOutMusic(float duration = 1.0f)
    {
        if (crossfadeCoroutine != null)
        {
            StopCoroutine(crossfadeCoroutine);
            crossfadeCoroutine = null;
        }

        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
        }

        if (duration <= 0f || (!musicSource.isPlaying && !secondaryMusicSource.isPlaying))
        {
            StopMusic();
            return;
        }

        fadeOutCoroutine = StartCoroutine(FadeOutRoutine(duration));
    }

    private IEnumerator FadeOutRoutine(float duration)
    {
        float startPrimaryVolume = musicSource.volume;
        float startSecondaryVolume = secondaryMusicSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            musicSource.volume = Mathf.Lerp(startPrimaryVolume, 0f, t);
            secondaryMusicSource.volume = Mathf.Lerp(startSecondaryVolume, 0f, t);
            yield return null;
        }

        StopMusic();
        musicSource.volume = musicVolume;
        fadeOutCoroutine = null;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);

        if (crossfadeCoroutine == null)
        {
            musicSource.volume = musicVolume;
        }
        else
        {
            secondaryMusicSource.volume = Mathf.Min(secondaryMusicSource.volume, musicVolume);
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp01(volume);
    }
}
