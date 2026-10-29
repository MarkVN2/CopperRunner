using System.Collections.Generic;
using CopperRunner.Gameplay.Item;
using CopperRunner.Gameplay.Upgrade;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CopperHud : MonoBehaviour
{
    [SerializeField]
    private Player player;

    [SerializeField]
    private TMP_Text copperText;

    [SerializeField]
    private Transform itemIconContainer;

    [SerializeField]
    private Transform upgradeIconContainer;

    private readonly List<ItemIconView> itemIcons = new List<ItemIconView>();
    private readonly List<Image> upgradeIcons = new List<Image>();

    private void Awake()
    {
        if (player == null)
            player = FindFirstObjectByType<Player>();

        if (itemIconContainer == null)
        {
            GameObject background = GameObject.Find("Active Item Background");
            if (background != null)
                itemIconContainer = background.transform;
        }

        if (upgradeIconContainer == null)
        {
            GameObject upgradeList = GameObject.Find("UpgradeList");
            if (upgradeList != null)
                upgradeIconContainer = upgradeList.transform;
        }
    }

    private void OnEnable()
    {
        if (player == null)
            return;

        player.CoinsChanged += UpdateCopper;
        player.ItemsChanged += UpdateItems;
        player.UpgradesChanged += UpdateUpgrades;
        UpdateCopper(player.GetCoinAmount());
        UpdateItems(player.Items);
        UpdateUpgrades(player.Upgrades);
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.CoinsChanged -= UpdateCopper;
            player.ItemsChanged -= UpdateItems;
            player.UpgradesChanged -= UpdateUpgrades;
        }
    }

    private void UpdateCopper(int amount)
    {
        if (copperText != null)
            copperText.text = $"{amount.ToString()} / {player.CopperCarryingCapacity}";
    }

    private void UpdateItems(IReadOnlyList<ItemData> items)
    {
        if (itemIconContainer == null)
            return;

        EnsureItemIcons(items.Count > 0 ? 1 : 0);
        if (itemIcons.Count == 0)
            return;

        ItemIconView icon = itemIcons[0];
        bool hasItem = items.Count > 0;
        icon.root.gameObject.SetActive(hasItem);
        if (hasItem)
        {
            ItemData item = items[0];
            Sprite sprite = item.GetInventoryTexture();
            if (sprite == null)
                sprite = item.GetWorldTexture();

            icon.image.sprite = sprite;
            icon.image.enabled = sprite != null;
            icon.label.text = item.name;
        }
    }

    private void UpdateUpgrades(IReadOnlyList<UpgradeData> upgrades)
    {
        if (upgradeIconContainer == null)
            return;

        EnsureUpgradeIcons(upgrades.Count);

        RectTransform containerRect = upgradeIconContainer as RectTransform;
        float containerWidth = containerRect == null ? 600f : containerRect.rect.width;
        float iconSize =
            upgrades.Count == 0
                ? 64f
                : Mathf.Min(64f, Mathf.Max(1f, (containerWidth - 16f) / upgrades.Count));
        float spacing = Mathf.Min(8f, iconSize * 0.125f);
        float totalWidth = upgrades.Count * iconSize + Mathf.Max(0, upgrades.Count - 1) * spacing;
        float startX = (containerWidth - totalWidth) * 0.5f;

        for (int i = 0; i < upgradeIcons.Count; i++)
        {
            Image icon = upgradeIcons[i];
            bool hasUpgrade = i < upgrades.Count;
            icon.gameObject.SetActive(hasUpgrade);
            if (!hasUpgrade)
                continue;

            icon.sprite = upgrades[i].GetInventoryTexture();
            icon.enabled = icon.sprite != null;
            RectTransform iconRect = icon.rectTransform;
            iconRect.sizeDelta = new Vector2(iconSize, iconSize);
            iconRect.anchoredPosition = new Vector2(startX + i * (iconSize + spacing), -8f);
        }
    }

    private void EnsureUpgradeIcons(int count)
    {
        while (upgradeIcons.Count < count)
        {
            GameObject iconObject = new GameObject(
                "Upgrade Icon",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );
            RectTransform iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.SetParent(upgradeIconContainer, false);
            iconRect.anchorMin = new Vector2(0f, 1f);
            iconRect.anchorMax = new Vector2(0f, 1f);
            iconRect.pivot = new Vector2(0f, 1f);

            Image image = iconObject.GetComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;
            upgradeIcons.Add(image);
        }
    }

    private void EnsureItemIcons(int count)
    {
        while (itemIcons.Count < count)
        {
            GameObject slot = new GameObject(
                "Item Slot",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );
            RectTransform slotRect = slot.GetComponent<RectTransform>();
            slotRect.SetParent(itemIconContainer, false);
            slotRect.anchorMin = new Vector2(0.5f, 0.5f);
            slotRect.anchorMax = new Vector2(0.5f, 0.5f);
            slotRect.pivot = new Vector2(0.5f, 0.5f);
            slotRect.sizeDelta = new Vector2(180f, 210f);

            Image image = slot.GetComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;
            RectTransform imageRect = image.rectTransform;
            imageRect.anchorMin = new Vector2(0.5f, 0.5f);
            imageRect.anchorMax = new Vector2(0.5f, 0.5f);
            imageRect.pivot = new Vector2(0.5f, 0.5f);
            imageRect.anchoredPosition = new Vector2(0f, 20f);
            imageRect.sizeDelta = new Vector2(150f, 150f);

            GameObject labelObject = new GameObject(
                "Item Name",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI)
            );
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.SetParent(slotRect, false);
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 0f);
            labelRect.pivot = new Vector2(0.5f, 0f);
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = new Vector2(0f, 28f);

            TMP_Text label = labelObject.GetComponent<TMP_Text>();
            if (TMP_Settings.defaultFontAsset != null)
                label.font = TMP_Settings.defaultFontAsset;
            label.fontSize = 18f;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;

            itemIcons.Add(new ItemIconView(slotRect, image, label));
        }
    }

    private sealed class ItemIconView
    {
        public readonly RectTransform root;
        public readonly Image image;
        public readonly TMP_Text label;

        public ItemIconView(RectTransform root, Image image, TMP_Text label)
        {
            this.root = root;
            this.image = image;
            this.label = label;
        }
    }
}
