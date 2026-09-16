using System.Collections.Generic;
using CopperRunner.Gameplay.Upgrade;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeShopMenu : MonoBehaviour
{
    [SerializeField]
    private UpgradeShop shop;

    [SerializeField]
    private Player player;

    [SerializeField]
    private GameObject menu;

    [SerializeField]
    private List<Button> offerButtons = new List<Button>();

    [SerializeField]
    private List<TMP_Text> offerLabels = new List<TMP_Text>();

    [SerializeField]
    private List<Image> offerIcons = new List<Image>();

    [Header("Timer Settings")]
    [SerializeField]
    private float shopDuration = 30f; // Total time available before the shop closes automatically

    [SerializeField]
    private TMP_Text timerLabel; // UI text to display the remaining time

    private float remainingTime;
    private bool isOpen;

    private void Awake()
    {
        if (shop == null)
            shop = FindFirstObjectByType<UpgradeShop>();
        if (player == null)
            player = FindFirstObjectByType<Player>();
    }

    private void Update()
    {
        if (!isOpen)
            return;

        // Use unscaledDeltaTime because Time.timeScale is set to 0 when paused
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

        shop.RefreshOffers();
        if (menu != null)
            menu.SetActive(true);
        RefreshButtons();

        // Initialize and start the timer
        remainingTime = shopDuration;
        isOpen = true;
        UpdateTimerDisplay();

        // Pause the game
        Time.timeScale = 0f;
    }

    public void Close()
    {
        isOpen = false;

        if (menu != null)
            menu.SetActive(false);

        // Resume the game
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
                offerIcons[i].sprite = offer.GetIcon();
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
