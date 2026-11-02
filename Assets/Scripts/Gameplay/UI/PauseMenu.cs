using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    private static PauseMenu instance;

    private InputSystem_Actions input;
    private InputAction pauseAction;

    [SerializeField]
    private GameObject pauseUI;

    [SerializeField]
    private Button resumeButton;

    private bool isDuplicate;
    private float previousTimeScale = 1f;
    private float previousFixedDeltaTime;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            isDuplicate = true;
            Debug.LogWarning("[PauseMenu] Duplicate pause controller ignored.", this);
            enabled = false;
            return;
        }

        instance = this;
        input = new InputSystem_Actions();
        pauseAction = input.asset.FindAction("Player/Pause", throwIfNotFound: true);
        previousFixedDeltaTime = Time.fixedDeltaTime;

        if (pauseUI == null)
            Debug.LogWarning("[PauseMenu] Assign a pause UI object in the Inspector.", this);
        else
            pauseUI.SetActive(false);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);
        else
            Debug.LogWarning("[PauseMenu] Assign a Continue button in the Inspector.", this);
    }

    private void OnEnable()
    {
        pauseAction?.Enable();
    }

    private void OnDisable()
    {
        if (isDuplicate)
            return;

        pauseAction?.Disable();
        Resume();
    }

    private void OnDestroy()
    {
        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(Resume);

        input?.Dispose();

        if (instance == this)
            instance = null;
    }

    private void Update()
    {
        if (instance != this)
            return;

        if (pauseAction == null || !pauseAction.WasPressedThisFrame())
            return;

        if (IsRunOver())
            return;

        if (IsPaused)
            Resume();
        else
            Pause();
    }

    private void LateUpdate()
    {
        if (IsPaused && Time.timeScale != 0f)
            Time.timeScale = 0f;
    }

    private void Pause()
    {
        previousTimeScale = Time.timeScale;
        previousFixedDeltaTime = Time.fixedDeltaTime;
        IsPaused = true;
        Time.timeScale = 0f;

        if (pauseUI == null)
            Debug.LogWarning("[PauseMenu] Game paused, but pause UI is not assigned.", this);
        else
        {
            pauseUI.SetActive(true);
            pauseUI.transform.SetAsLastSibling();
        }

        if (resumeButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
    }

    public void Resume()
    {
        if (instance != null && instance != this)
            return;

        if (!IsPaused && (pauseUI == null || !pauseUI.activeSelf))
            return;

        IsPaused = false;
        Time.timeScale = previousTimeScale;
        Time.fixedDeltaTime = previousFixedDeltaTime;

        if (pauseUI != null)
            pauseUI.SetActive(false);
    }

    public static void SetGameTimeScale(float timeScale, float fixedDeltaTime)
    {
        if (IsPaused && instance != null)
        {
            instance.previousTimeScale = timeScale;
            instance.previousFixedDeltaTime = fixedDeltaTime;
            return;
        }

        Time.timeScale = timeScale;
        Time.fixedDeltaTime = fixedDeltaTime;
    }

    private bool IsRunOver()
    {
        EndRunMenu endRunMenu = FindFirstObjectByType<EndRunMenu>();
        return endRunMenu != null && endRunMenu.IsShown;
    }
}
