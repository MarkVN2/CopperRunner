using System.Collections.Generic;
using CopperRunner.Gameplay.Upgrade;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UpgradeShopMenu : MonoBehaviour
{
    public static UpgradeShopMenu Instance;

    public UnityEvent OnClose;

    [SerializeField]
    private UpgradeShop shop;

    [SerializeField]
    private Player player;

    [SerializeField]
    private Transform horde;

    [SerializeField]
    private MapManager mapManager;

    [SerializeField]
    private GameObject menu;

    [SerializeField]
    private List<Button> offerButtons = new List<Button>();

    [SerializeField]
    private List<TMP_Text> offerLabels = new List<TMP_Text>();

    [SerializeField]
    private List<TMP_Text> offerPrices = new List<TMP_Text>();

    [SerializeField]
    private List<TMP_Text> offerDescriptions = new List<TMP_Text>();

    [SerializeField]
    private Button closeButton;

    [SerializeField]
    private List<Image> offerIcons = new List<Image>();

    [Header("Timer Settings")]
    [Tooltip("The minimum base time when the horde is close.")]
    [FormerlySerializedAs("safetyMinimumDuration")]
    [SerializeField]
    [Min(0f)]
    private float minimumShopDuration = 5f;

    [SerializeField]
    [Min(0f)]
    private float maximumBaseShopDuration = 30f;

    [Tooltip("Edge-to-edge distance at which the base shop duration reaches its maximum.")]
    [SerializeField]
    [Min(0.01f)]
    private float distanceForMaximumShopDuration = 50f;

    [SerializeField]
    private TMP_Text timerLabel;

    private float remainingTime;
    public float RemainingTime => remainingTime;
    private bool isOpen;
    public bool IsOpen => isOpen;
    private float nextCheckpointTimeBonus;
    private float defaultFixedDeltaTime;
    private Transform activeShopSection;

    private void Awake()
    {
        if (Instance != this)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        defaultFixedDeltaTime = Time.fixedDeltaTime;

        if (shop == null)
            shop = FindFirstObjectByType<UpgradeShop>();
        if (player == null)
            player = FindFirstObjectByType<Player>();
        if (mapManager == null)
            mapManager = FindFirstObjectByType<MapManager>();
        if (horde == null)
        {
            HordeFollower hordeFollower = FindFirstObjectByType<HordeFollower>();
            if (hordeFollower != null)
                horde = hordeFollower.transform;
        }

        if (horde == null)
        {
            Debug.LogWarning(
                "[UpgradeShopMenu] Horde is not assigned; shop time will be minimal.",
                this
            );
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }
        else
        {
            Debug.LogWarning(
                "[UpgradeShopMenu] CloseButton is not assigned in the Inspector!",
                this
            );
        }
    }

    private void OnDisable()
    {
        if (!isOpen)
            return;

        isOpen = false;
        activeShopSection = null;
        if (menu != null)
            menu.SetActive(false);
        PauseMenu.SetGameTimeScale(1f, defaultFixedDeltaTime);
        OnClose.Invoke();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (!isOpen || PauseMenu.IsPaused)
            return;

        remainingTime -= Time.unscaledDeltaTime;

        if (remainingTime <= 0f)
        {
            Close();
            return;
        }

        UpdateShopTimeScale();
        UpdateTimerDisplay();
    }

    public void Open()
    {
        Open(null);
    }

    public void Open(Transform shopSection)
    {
        if (isOpen)
            return;

        if (shop == null || player == null)
        {
            Debug.LogWarning("[UpgradeShopMenu] Shop or player is not assigned.", this);
            return;
        }

        shop.RefreshOffers(player);
        if (menu != null)
            menu.SetActive(true);
        RefreshButtons();

        float distanceFromHorde = GetHorizontalDistanceFromHorde();
        float distanceForMaximum = Mathf.Max(0.01f, distanceForMaximumShopDuration);
        float normalizedDistance = Mathf.Clamp01(distanceFromHorde / distanceForMaximum);
        float minimumDuration = Mathf.Max(0f, minimumShopDuration);
        float maximumDuration = Mathf.Max(minimumDuration, maximumBaseShopDuration);
        float calculatedDuration = Mathf.Lerp(
            minimumDuration,
            maximumDuration,
            normalizedDistance
        );

        remainingTime = calculatedDuration + nextCheckpointTimeBonus;
        nextCheckpointTimeBonus = 0f;
        activeShopSection = shopSection;
        isOpen = true;
        UpdateTimerDisplay();
        UpdateShopTimeScale();
    }

    private float GetHorizontalDistanceFromHorde()
    {
        if (horde == null)
            return 0f;

        Collider2D playerCollider = player.GetComponent<Collider2D>();
        Collider2D hordeCollider = horde.GetComponent<Collider2D>();
        if (playerCollider == null || hordeCollider == null)
            return Mathf.Abs(player.transform.position.x - horde.position.x);

        Bounds playerBounds = playerCollider.bounds;
        Bounds hordeBounds = hordeCollider.bounds;

        if (playerBounds.min.x >= hordeBounds.max.x)
            return playerBounds.min.x - hordeBounds.max.x;
        if (hordeBounds.min.x >= playerBounds.max.x)
            return hordeBounds.min.x - playerBounds.max.x;

        return 0f;
    }

    private void UpdateShopTimeScale()
    {
        float targetScale = Mathf.Clamp(remainingTime / 10f, 0.1f, 0.6f);
        if (activeShopSection != null && mapManager != null && mapManager.CurrentSpeed > 0f)
        {
            float sectionExitX = activeShopSection.position.x - mapManager.SectionLength * 0.5f;
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            float playerLeftEdgeX = playerCollider != null
                ? playerCollider.bounds.min.x
                : player.transform.position.x;
            float safetyDistance =
                mapManager.CurrentSpeed * defaultFixedDeltaTime * 0.6f;
            float remainingSectionDistance = Mathf.Max(
                0f,
                playerLeftEdgeX - sectionExitX - safetyDistance
            );
            targetScale = remainingSectionDistance / (
                mapManager.CurrentSpeed * Mathf.Max(remainingTime, 0.001f)
            );
        }

        float timeScale = Mathf.Clamp(targetScale, 0f, 0.6f);
        PauseMenu.SetGameTimeScale(timeScale, defaultFixedDeltaTime * timeScale);
    }

    public void AddTimeToNextCheckpoint(float seconds)
    {
        if (seconds <= 0f)
        {
            Debug.LogWarning(
                "[UpgradeShopMenu] Checkpoint time bonus must be greater than zero.",
                this
            );
            return;
        }

        nextCheckpointTimeBonus += seconds;
    }

    public void Close()
    {
        if (!isOpen)
            return;

        isOpen = false;
        activeShopSection = null;

        if (menu != null)
            menu.SetActive(false);

        PauseMenu.SetGameTimeScale(1f, defaultFixedDeltaTime);
        OnClose.Invoke();
    }

    public void PurchaseOffer(int offerIndex)
    {
        if (shop == null || player == null)
            return;

        if (shop.TryPurchase(player, offerIndex))
            Close();
        else
            RefreshButtons();
    }

    private void RefreshButtons()
    {
        IReadOnlyList<UpgradeData> offers = shop.CurrentOffers;
        for (int i = 0; i < offerButtons.Count; i++)
        {
            bool hasOffer = i < offers.Count && offers[i] != null;
            offerButtons[i].gameObject.SetActive(hasOffer);
            if (!hasOffer)
                continue;

            UIHoverPopButton hoverButton = offerButtons[i].GetComponent<UIHoverPopButton>();
            if (hoverButton == null)
            {
                hoverButton = offerButtons[i].gameObject.AddComponent<UIHoverPopButton>();
            }

            UpgradeData offer = offers[i];

            string description = offer.GetDescription()?.GetLocalizedString() ?? string.Empty;
            hoverButton.SetupTooltipData(description);

            offerButtons[i].onClick.RemoveAllListeners();
            int index = i;
            offerButtons[i]
                .onClick.AddListener(
                    delegate
                    {
                        PurchaseOffer(index);
                    }
                );

            if (i < offerLabels.Count && offerLabels[i] != null)
                offerLabels[i].text = offer.GetName().GetLocalizedString();
            if (i < offerPrices.Count && offerPrices[i] != null)
                offerPrices[i].text = Mathf.Max(0, offer.price).ToString();
            if (i < offerIcons.Count && offerIcons[i] != null)
                offerIcons[i].sprite = offer.GetInventoryTexture();
            if (i < offerDescriptions.Count && offerDescriptions[i] != null)
                offerDescriptions[i].text = offer.GetDescription().GetLocalizedString();
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerLabel == null)
            return;

        float clampedTime = Mathf.Max(0f, remainingTime);
        int seconds = Mathf.CeilToInt(clampedTime);
        timerLabel.text = $"Time Left: {seconds}s";
    }
}
