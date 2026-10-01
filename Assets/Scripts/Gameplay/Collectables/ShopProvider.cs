using System.Collections.Generic;
using CopperRunner.Gameplay.Upgrade;
using UnityEngine;

public class ShopProvider : MonoBehaviour
{
    [SerializeField]
    private UpgradeDatabase currentPool;

    [SerializeField]
    private RegionID[] unlockedRegions = {RegionID.BABYLON};

    public List<UpgradeData> GetRandomOffers(int offerCount)
    {
        if (currentPool == null)
        {
            Debug.LogWarning("[ShopProvider] No upgrade database assigned to the shop.");
            return new List<UpgradeData>();
        }

        return currentPool.GetRandomFromActivePool(offerCount, unlockedRegions);
    }
    public List<UpgradeData> GetRandomBuyableOffers(int offerCount)
    {
        if (currentPool == null)
        {
            Debug.LogWarning("[ShopProvider] No upgrade database assigned to the shop.");
            return new List<UpgradeData>();
        }

        return currentPool.GetRandomFromActivePool(offerCount, unlockedRegions);
    }
}