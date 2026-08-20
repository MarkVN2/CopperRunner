using System.Collections.Generic;
using CopperRunner.Gameplay.Upgrade;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewUpgradeCollection",
    menuName = "ScriptableObjects/Collectables/Collections/UpgradeCollection"
)]
public class UpgradeCollection : ScriptableObject
{    
    public RegionID regionID;
    [SerializeField]
    
    List<UpgradeData> upgrades;

    public List<UpgradeData> GetUpgrades()
    {
        return upgrades;
    }

}