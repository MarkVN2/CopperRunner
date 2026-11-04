using UnityEngine;

public class FirstMenu : MonoBehaviour
{
    public AudioClip   confirmSFX;
    public AudioClip   cancelSFX;
    public AudioSource TEMP_soundEffectsSource;

    public GameObject firstScreenPanel;
    public GameObject instructionBooklet;
    public GameObject settingsWindow;

    public void BeginRun()
    {
        ScreenFadeTransition.LoadSceneWithFade("DemoScene");
    }

    public void OpenFirstPanel()
    {
        firstScreenPanel.SetActive(true);
        instructionBooklet.SetActive(false);
        settingsWindow.SetActive(false);
        TEMP_soundEffectsSource.PlayOneShot(cancelSFX);
    }
    public void InstructionBooklet()
    {
        firstScreenPanel.SetActive(false);
        instructionBooklet.SetActive(true);
        TEMP_soundEffectsSource.PlayOneShot(confirmSFX);
    }
    public void SettingScreen()
    {
        firstScreenPanel.SetActive(false);
        settingsWindow.SetActive(true);
        TEMP_soundEffectsSource.PlayOneShot(confirmSFX);
    }
    public void Leave()
    {
        TEMP_soundEffectsSource.PlayOneShot(confirmSFX);
        Application.Quit();
    }
}
