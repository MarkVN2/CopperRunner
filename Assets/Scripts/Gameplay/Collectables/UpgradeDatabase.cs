using System.Collections.Generic;
using CopperRunner.Gameplay.Upgrade;
using System.Linq;
using System;
using UnityEngine;
[CreateAssetMenu(
    fileName = "NewUpgradeDatabase",
    menuName = "ScriptableObjects/Collectables/Database/UpgradeDatabase"
)]
public class UpgradeDatabase: ScriptableObject {

    [SerializeField]
    private List<UpgradeCollection> itemPool;
    
    public List<UpgradeData> GetRandomFromSpecificRarity(Rarity rarity = Rarity.COMMON, int itemQuantity = 3)
    {
        List<UpgradeData> upgradesOfSpecificRarity = new ();
        List<UpgradeData> finalList = new();
        foreach(UpgradeCollection collection in itemPool)
        {
            upgradesOfSpecificRarity.AddRange(collection.GetUpgrades().Where(data => data.rarity == rarity));
        } 
        while(finalList.Count < itemQuantity)
        {
            int index = UnityEngine.Random.Range(0,upgradesOfSpecificRarity.Count);
            UpgradeData randomizedItem = upgradesOfSpecificRarity[index];

            if (!finalList.Contains(randomizedItem))
                finalList.Add(randomizedItem);
        }
        return finalList;
    }
    public List<UpgradeData> GetRandomFromSpecificRegion(int itemQuantity = 3, RegionID regionID = RegionID.BABYLON)
    {
        UpgradeCollection upgradesOfSpecificRegion = new();
        List<UpgradeData> cachedListOfUpgrades;
        List<UpgradeData> finalList = new();

        upgradesOfSpecificRegion = itemPool.Find(collection => collection.regionID == regionID);

        cachedListOfUpgrades = upgradesOfSpecificRegion.GetUpgrades();
        while (finalList.Count < itemQuantity)
        {
            int index = UnityEngine.Random.Range(0,cachedListOfUpgrades.Count);
            UpgradeData randomizedItem = cachedListOfUpgrades[index];

            if (!finalList.Contains(randomizedItem))
                finalList.Add(randomizedItem);
        }
        return finalList;
    }
    public List<UpgradeData> GetRandomFromActivePool(int itemQuantity, RegionID[] activeRegions)
    {
        List<UpgradeData> upgradesOfSpecificRarity = new();
        List<UpgradeCollection> filteredPool = new List<UpgradeCollection>(itemPool.Where(collection => activeRegions.Contains(collection.regionID)));
        if (filteredPool.Count < 0 || filteredPool == null)
        {
            Debug.LogWarning("No equivalent collection with active regions" + activeRegions.ToString());
            return null;
        }

        foreach (UpgradeCollection collection in filteredPool)
        {
            Debug.Log(collection.regionID);
            upgradesOfSpecificRarity.AddRange(collection.GetUpgrades());
        };
        
        
        List<UpgradeData> finalList = new();

        while (finalList.Count < itemQuantity)
        {
            int index = UnityEngine.Random.Range(0,upgradesOfSpecificRarity.Count);
            UpgradeData randomizedItem = upgradesOfSpecificRarity[index];

            if (!finalList.Contains(randomizedItem))
                finalList.Add(randomizedItem);
        }
        return finalList;
    }
}
