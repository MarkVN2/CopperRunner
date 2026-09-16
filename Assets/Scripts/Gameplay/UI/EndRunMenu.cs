using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndRunMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject endMenu;

    [SerializeField]
    private GameObject[] menusToDisable;

    [SerializeField]
    private TMP_Text timeText;

    [SerializeField]
    private TMP_Text coinsText;

    [SerializeField]
    private Button restartButton;

    [SerializeField]
    private Player player;

    private float runStartedAt;
    private bool isShown;

    private void Awake()
    {
        runStartedAt = Time.time;

        if (player == null)
            player = FindFirstObjectByType<Player>();

        if (endMenu != null)
            endMenu.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartRun);
    }

    public void Show()
    {
        if (isShown)
            return;

        isShown = true;
        if (menusToDisable != null)
        {
            for (int i = 0; i < menusToDisable.Length; i++)
            {
                if (menusToDisable[i] != null)
                    menusToDisable[i].SetActive(false);
            }
        }

        if (timeText != null)
            timeText.text = FormatTime(Time.time - runStartedAt);

        if (coinsText != null)
            coinsText.text = player != null ? player.GetCoinAmount().ToString() : "0";

        if (endMenu != null)
            endMenu.SetActive(true);
        else
            CreateFallbackMenu();
    }

    public void RestartRun()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int remainingSeconds = Mathf.FloorToInt(seconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, remainingSeconds);
    }

    private void CreateFallbackMenu()
    {
        GameObject canvasObject = new GameObject("End Run Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject panelObject = new GameObject("End Menu");
        panelObject.transform.SetParent(canvasObject.transform, false);
        Image panel = panelObject.AddComponent<Image>();
        panel.color = new Color(0f, 0f, 0f, 0.85f);

        RectTransform panelRect = panel.rectTransform;
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(700f, 450f);

        CreateFallbackText(panelObject.transform, "RUN OVER", new Vector2(0f, 120f), 48);
        CreateFallbackText(
            panelObject.transform,
            "Time: " + FormatTime(Time.time - runStartedAt),
            new Vector2(0f, 40f),
            32
        );
        CreateFallbackText(
            panelObject.transform,
            "Copper: " + (player != null ? player.GetCoinAmount() : 0),
            new Vector2(0f, -30f),
            32
        );

        GameObject buttonObject = new GameObject("Restart Button");
        buttonObject.transform.SetParent(panelObject.transform, false);
        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.55f, 0.25f, 1f);
        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(RestartRun);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(260f, 70f);
        buttonRect.anchoredPosition = new Vector2(0f, -130f);
        CreateFallbackText(buttonObject.transform, "RESTART", Vector2.zero, 28);
    }

    private void CreateFallbackText(
        Transform parent,
        string value,
        Vector2 position,
        int fontSize
    )
    {
        GameObject textObject = new GameObject(value + " Text");
        textObject.transform.SetParent(parent, false);
        TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(600f, 70f);
        textRect.anchoredPosition = position;
    }
}
