using System;
using System.Collections.Generic;
using System.Linq;
using CopperRunner.Gameplay.Upgrade;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewUpgradeDatabase",
    menuName = "ScriptableObjects/Collectables/Database/UpgradeDatabase"
)]
public class UpgradeDatabase : ScriptableObject
{
    [SerializeField]
    private List<UpgradeCollection> itemPool;

    public List<UpgradeData> GetRandomFromSpecificRarity(
        Rarity rarity = Rarity.COMMON,
        int itemQuantity = 3
    )
    {
        List<UpgradeData> upgradesOfSpecificRarity = new();
        List<UpgradeData> finalList = new();
        if (itemPool == null)
            return finalList;

        foreach (UpgradeCollection collection in itemPool)
        {
            if (collection != null && collection.GetUpgrades() != null)
                upgradesOfSpecificRarity.AddRange(
                    collection.GetUpgrades().Where(data => data != null && data.rarity == rarity)
                );
        }
        return PickUnique(upgradesOfSpecificRarity, itemQuantity);
    }

    public List<UpgradeData> GetRandomFromSpecificRegion(
        int itemQuantity = 3,
        RegionID regionID = RegionID.BABYLON
    )
    {
        UpgradeCollection upgradesOfSpecificRegion = new();
        List<UpgradeData> cachedListOfUpgrades;
        List<UpgradeData> finalList = new();

        if (itemPool == null)
            return finalList;

        upgradesOfSpecificRegion = itemPool.Find(collection =>
            collection != null && collection.regionID == regionID
        );
        if (upgradesOfSpecificRegion == null)
            return finalList;

        cachedListOfUpgrades = upgradesOfSpecificRegion.GetUpgrades();
        return PickUnique(cachedListOfUpgrades, itemQuantity);
    }

    public List<UpgradeData> GetRandomFromActivePool(int itemQuantity, RegionID[] activeRegions)
    {
        List<UpgradeData> upgradesOfActiveRegions = new();
        if (itemPool == null || activeRegions == null)
            return new List<UpgradeData>();

        List<UpgradeCollection> filteredPool = new List<UpgradeCollection>(
            itemPool.Where(collection =>
                collection != null && activeRegions.Contains(collection.regionID)
            )
        );

        foreach (UpgradeCollection collection in filteredPool)
        {
            if (collection.GetUpgrades() != null)
                upgradesOfActiveRegions.AddRange(
                    collection.GetUpgrades().Where(data => data != null)
                );
        }

        return PickUnique(upgradesOfActiveRegions, itemQuantity);
    }

    private List<UpgradeData> PickUnique(List<UpgradeData> candidates, int itemQuantity)
    {
        List<UpgradeData> finalList = new();
        if (candidates == null || itemQuantity <= 0)
            return finalList;

        List<UpgradeData> remaining = new(candidates.Distinct());
        int amountToPick = Math.Min(itemQuantity, remaining.Count);
        for (int i = 0; i < amountToPick; i++)
        {
            int index = UnityEngine.Random.Range(0, remaining.Count);
            finalList.Add(remaining[index]);
            remaining.RemoveAt(index);
        }

        return finalList;
    }
}
