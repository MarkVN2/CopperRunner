using TMPro;
using UnityEngine;

public class UpgradeTooltipUI : MonoBehaviour
{
    public static UpgradeTooltipUI Instance;

    [SerializeField]
    private GameObject tooltipPanel;

    [SerializeField]
    private TMP_Text tooltipText;

    [Header("Optional Offset")]
    [SerializeField]
    private Vector2 offset = new Vector2(15f, 15f);

    private RectTransform rectTransform;
    private RectTransform canvasRectTransform;
    private Canvas parentCanvas;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            canvasRectTransform = parentCanvas.GetComponent<RectTransform>();
        }

        Hide();
    }

    private void Update()
    {
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                Input.mousePosition,
                parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                    ? null
                    : parentCanvas.worldCamera,
                out localPoint
            );

            rectTransform.anchoredPosition = localPoint + offset;
        }
    }

    public void Show(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            Hide();
            return;
        }

        if (tooltipText != null)
            tooltipText.text = content;

        if (tooltipPanel != null)
            tooltipPanel.SetActive(true);
    }

    public void Hide()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }
}
