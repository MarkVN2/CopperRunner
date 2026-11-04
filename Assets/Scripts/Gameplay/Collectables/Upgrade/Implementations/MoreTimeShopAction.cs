using CopperRunner.Gameplay.Upgrade;
using UnityEngine;

[System.Serializable]
public class MoreShopTimeAction : UpgradeAction
{
    public override void ActivateUpgrade(UpgradeData upgradeData)
    {
        UpgradeShopMenu shopMenu = Object.FindFirstObjectByType<UpgradeShopMenu>();
        if (shopMenu == null)
        {
            Debug.LogWarning(
                "[MoreShopTimeAction] No shop menu found; checkpoint time was not increased."
            );
            return;
        }

        shopMenu.AddTimeToNextCheckpoint(10f);
    }
}
