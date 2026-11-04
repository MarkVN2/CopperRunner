using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public float bgmVolume = .3f;

    public AudioSource BGMSource;
    public AudioClip   mainTheme;

    // FUNCTIONS
    private void Awake()
    {
        if (Instance != this)
        {
            Instance = this;
        }
        else
        {
            Instance = null;
        }
    }

    private void Start()
    {
        BackgroundMusic_start_loopingOn();
    }

    // COMMANDS
    public void BackgroundMusic_start_loopingOn()
    {
        Debug.Log(mainTheme.name + "is playing on loop on background");
        BGMSource.loop = true;
        BGMSource.volume = bgmVolume;
        BGMSource.Play();
    }
    public void BackgroundMusic_suddenStop()
    {
        BGMSource.Stop();
    }
}
