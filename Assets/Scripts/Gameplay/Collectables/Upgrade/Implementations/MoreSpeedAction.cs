using CopperRunner.Gameplay.Upgrade;
using UnityEngine;

[System.Serializable]
public class MoreSpeedAction : UpgradeAction
{
    public override void ActivateUpgrade(UpgradeData upgradeData)
    {
        MapManager mapManager = Object.FindFirstObjectByType<MapManager>();
        if (mapManager == null)
        {
            Debug.LogWarning("[MoreSpeedAction] No MapManager found; speed upgrade was not applied.");
            return;
        }

        mapManager.IncreaseSpeed(1.15f);
    }
}
