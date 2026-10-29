using System.Collections.Generic;
using CopperRunner.Gameplay.Upgrade;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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
    private List<Image> offerIcons = new List<Image>();

    [Header("Timer Settings")]
    [Header("Step-Based Timer Settings")]
    [Tooltip("How many seconds of real-world shop time the player gets PER REMAINING STEP.")]
    [SerializeField]
    private float secondsPerStep = 2f;

    [Tooltip("A baseline absolute minimum time given to the player even if steps are near 0.")]
    [SerializeField]
    private float safetyMinimumDuration = 5f;

    [SerializeField]
    private TMP_Text timerLabel;

    private float remainingTime;
    private bool isOpen;
    private float nextCheckpointTimeBonus;

    private void Awake()
    {
        if (Instance != this)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (shop == null)
            shop = FindFirstObjectByType<UpgradeShop>();
        if (player == null)
            player = FindFirstObjectByType<Player>();
        if (mapManager == null)
            mapManager = FindFirstObjectByType<MapManager>();
    }

    private void Update()
    {
        if (!isOpen)
            return;

        remainingTime -= Time.unscaledDeltaTime;

        UpdateTimerDisplay();

        if (remainingTime <= 0f)
        {
            Close();
        }
    }

    public void Open()
    {
        if (shop == null || player == null)
        {
            Debug.LogWarning("[UpgradeShopMenu] Shop or player is not assigned.", this);
            return;
        }

        shop.RefreshOffers(player);
        if (menu != null)
            menu.SetActive(true);
        RefreshButtons();

        float calculatedDuration = player.RemainingSteps * secondsPerStep;
        if (calculatedDuration < safetyMinimumDuration)
        {
            calculatedDuration = safetyMinimumDuration;
        }

        remainingTime = calculatedDuration + nextCheckpointTimeBonus;
        nextCheckpointTimeBonus = 0f;
        isOpen = true;
        UpdateTimerDisplay();
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
        isOpen = false;

        if (menu != null)
            menu.SetActive(false);
        OnClose.Invoke();
        Time.timeScale = 1f;
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

            UpgradeData offer = offers[i];
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
                offerLabels[i].text =
                    offer.GetName().GetLocalizedString() + "\n" + Mathf.Max(0, offer.price);

            if (i < offerIcons.Count && offerIcons[i] != null)
                offerIcons[i].sprite = offer.GetInventoryTexture();
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
