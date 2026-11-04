using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenFadeTransition : MonoBehaviour
{
    private static ScreenFadeTransition instance;

    [SerializeField]
    private UIFader fader;

    [SerializeField]
    [Min(0f)]
    private float transitionDuration = 0.5f;

    private bool isLoading;
    private Coroutine startupFade;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        if (fader == null)
            fader = GetComponent<UIFader>();

        if (fader == null)
        {
            Debug.LogError("[ScreenFadeTransition] Assign a UIFader to the fade prefab.", this);
            enabled = false;
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (instance == this && SceneManager.GetActiveScene().name == "DemoScene")
            startupFade = StartCoroutine(FadeInAtStartup());
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public static void LoadSceneWithFade(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[ScreenFadeTransition] A scene name is required.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"[ScreenFadeTransition] Scene '{sceneName}' is not enabled in the build settings."
            );
            return;
        }

        if (instance == null)
        {
            instance = FindFirstObjectByType<ScreenFadeTransition>();
            if (instance == null)
            {
                Debug.LogError(
                    "[ScreenFadeTransition] Add the ScreenFade prefab to the scene before requesting a fade."
                );
                return;
            }
        }

        if (instance.isLoading)
            return;

        if (instance.startupFade != null)
        {
            instance.StopCoroutine(instance.startupFade);
            instance.startupFade = null;
        }

        instance.StartCoroutine(instance.LoadSceneRoutine(sceneName));
    }

    private IEnumerator FadeInAtStartup()
    {
        yield return fader.FadeToAlpha(0f, transitionDuration);
        startupFade = null;
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isLoading = true;
        yield return fader.FadeToAlpha(1f, transitionDuration);

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
        if (loadOperation == null)
        {
            Debug.LogError(
                $"[ScreenFadeTransition] Failed to start loading scene '{sceneName}'.",
                this
            );
            isLoading = false;
            yield return fader.FadeToAlpha(0f, transitionDuration);
            yield break;
        }

        while (!loadOperation.isDone)
            yield return null;

        yield return fader.FadeToAlpha(0f, transitionDuration);
        isLoading = false;
    }
}
