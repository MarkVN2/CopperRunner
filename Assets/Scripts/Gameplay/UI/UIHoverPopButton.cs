using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIHoverPopButton
    : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler
{
    [Header("Animation Settings")]
    [SerializeField]
    private float hoverScale = 1.1f;

    [SerializeField]
    private float clickScale = 0.9f;

    [SerializeField]
    private float lerpSpeed = 15f;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isPressed = false;
    private bool isHovered = false;

    private string upgradeDescription;

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.unscaledDeltaTime * lerpSpeed
        );
    }

    public void SetupTooltipData(string description)
    {
        upgradeDescription = description;
        if (
            isHovered
            && !string.IsNullOrEmpty(upgradeDescription)
            && UpgradeTooltipUI.Instance != null
        )
        {
            UpgradeTooltipUI.Instance.Show(upgradeDescription);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateTargetScale();

        if (!string.IsNullOrEmpty(upgradeDescription) && UpgradeTooltipUI.Instance != null)
        {
            UpgradeTooltipUI.Instance.Show(upgradeDescription);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        isPressed = false;
        UpdateTargetScale();

        if (UpgradeTooltipUI.Instance != null)
        {
            UpgradeTooltipUI.Instance.Hide();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            isPressed = true;
            UpdateTargetScale();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            isPressed = false;
            UpdateTargetScale();
        }
    }

    private void UpdateTargetScale()
    {
        if (isPressed)
            targetScale = originalScale * clickScale;
        else if (isHovered)
            targetScale = originalScale * hoverScale;
        else
            targetScale = originalScale;
    }

    private void OnDisable()
    {
        isPressed = false;
        isHovered = false;
        targetScale = originalScale;
        transform.localScale = originalScale;

        if (UpgradeTooltipUI.Instance != null)
        {
            UpgradeTooltipUI.Instance.Hide();
        }
    }
}
