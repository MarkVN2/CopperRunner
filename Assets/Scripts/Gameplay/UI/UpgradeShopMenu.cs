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
    [SerializeField]
    private float shopDuration = 30f; 

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
        else
        {
            Instance = null;
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

        remainingTime -= Time.deltaTime;

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

        remainingTime = GetDistanceBasedShopDuration() + nextCheckpointTimeBonus;
        nextCheckpointTimeBonus = 0f;
        isOpen = true;
        UpdateTimerDisplay();

    }

    public void AddTimeToNextCheckpoint(float seconds)
    {
        if (seconds <= 0f)
        {
            Debug.LogWarning("[UpgradeShopMenu] Checkpoint time bonus must be greater than zero.", this);
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

    private float GetDistanceBasedShopDuration()
    {
        if (horde == null)
        {
            GameObject hordeObject = GameObject.FindGameObjectWithTag("Enemy");
            if (hordeObject != null)
                horde = hordeObject.transform;
        }

        if (horde == null || mapManager == null)
        {
            Debug.LogWarning(
                "[UpgradeShopMenu] Horde or MapManager not found; using the configured shop duration.",
                this
            );
            return Mathf.Max(0f, shopDuration);
        }

        float distanceToHorde;
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        Collider2D hordeCollider = horde.GetComponent<Collider2D>();
        if (playerCollider != null && hordeCollider != null)
        {
            Bounds playerBounds = playerCollider.bounds;
            Bounds hordeBounds = hordeCollider.bounds;

            if (hordeBounds.max.x <= playerBounds.min.x)
                distanceToHorde = playerBounds.min.x - hordeBounds.max.x;
            else if (playerBounds.max.x <= hordeBounds.min.x)
                distanceToHorde = hordeBounds.min.x - playerBounds.max.x;
            else
                distanceToHorde = 0f;
        }
        else
        {
            Debug.LogWarning(
                "[UpgradeShopMenu] Player or horde collider not found; using transform distance.",
                this
            );
            distanceToHorde = Mathf.Abs(player.transform.position.x - horde.position.x);
        }

        if (mapManager.CurrentSpeed <= 0f)
        {
            Debug.LogWarning(
                "[UpgradeShopMenu] Map speed must be greater than zero; using the configured shop duration.",
                this
            );
            return Mathf.Max(0f, shopDuration);
        }

        return distanceToHorde / mapManager.CurrentSpeed;
    }
}
