using System.Collections.Generic;
using CopperRunner.Gameplay.Upgrade;
using UnityEngine;

[RequireComponent(typeof(ShopProvider))]
public class UpgradeShop : MonoBehaviour
{
    [SerializeField]
    private ShopProvider shopProvider;

    [SerializeField]
    [Min(1)]
    private int offerCount = 3;

    private readonly List<UpgradeData> currentOffers = new List<UpgradeData>();

    public IReadOnlyList<UpgradeData> CurrentOffers => currentOffers;

    private void Awake()
    {
        if (shopProvider == null)
            shopProvider = GetComponent<ShopProvider>();
    }

    public void RefreshOffers()
    {
        currentOffers.Clear();
        currentOffers.AddRange(shopProvider.GetRandomOffers(offerCount));
    }

    public bool TryPurchase(Player player, int offerIndex)
    {
        if (player == null || offerIndex < 0 || offerIndex >= currentOffers.Count)
            return false;

        UpgradeData offer = currentOffers[offerIndex];
        if (!player.CanAddUpgrade(offer))
            return false;

        int price = Mathf.Max(0, offer.price);
        if (!player.TrySpendCoins(price))
            return false;

        if (!player.AddUpgrade(offer))
        {
            player.AddCoin(price);
            return false;
        }

        currentOffers.RemoveAt(offerIndex);
        return true;
    }
}
