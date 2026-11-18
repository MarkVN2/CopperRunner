using UnityEngine;

public class FirstMenu : MonoBehaviour
{
    public AudioClip confirmSFX;
    public AudioClip cancelSFX;
    public AudioSource TEMP_soundEffectsSource;

    public GameObject firstScreenPanel;
    public GameObject instructionBooklet;
    public GameObject settingsWindow;

    public void Start()
    {
        AudioManager.Instance.PlayMusicLoop("JazzMusic");
    }

    public void BeginRun()
    {
        ScreenFadeTransition.LoadSceneWithFade("DemoScene");
    }

    public void OpenFirstPanel()
    {
        firstScreenPanel.SetActive(true);
        instructionBooklet.SetActive(false);
        settingsWindow.SetActive(false);
        AudioManager.Instance.PlaySFX("ButtonClick");
    }

    public void InstructionBooklet()
    {
        firstScreenPanel.SetActive(false);
        instructionBooklet.SetActive(true);
        AudioManager.Instance.PlaySFX("ButtonClick");
    }

    public void SettingScreen()
    {
        firstScreenPanel.SetActive(false);
        settingsWindow.SetActive(true);
        AudioManager.Instance.PlaySFX("ButtonClick");
    }

    public void Leave()
    {
        AudioManager.Instance.PlaySFX("ButtonClick");
        Application.Quit();
    }
}
