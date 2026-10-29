using System.Collections.Generic;
using CopperRunner.Gameplay.Upgrade;
using UnityEngine;

public class ShopProvider : MonoBehaviour
{
    [SerializeField]
    private UpgradeDatabase currentPool;

    [SerializeField]
    private RegionID[] unlockedRegions = {RegionID.BABYLON};

    public List<UpgradeData> GetRandomOffers(int offerCount, Player player)
    {
        if (currentPool == null)
        {
            Debug.LogWarning("[ShopProvider] No upgrade database assigned to the shop.");
            return new List<UpgradeData>();
        }

        if (player == null)
        {
            Debug.LogWarning("[ShopProvider] No player assigned; cannot filter max-rank upgrades.");
            return new List<UpgradeData>();
        }

        return currentPool.GetRandomFromActivePool(
            offerCount,
            unlockedRegions,
            player.CanAddUpgrade
        );
    }
    public List<UpgradeData> GetRandomBuyableOffers(int offerCount, Player player)
    {
        return GetRandomOffers(offerCount, player);
    }
}