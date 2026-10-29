using CopperRunner.Gameplay.Upgrade;
using UnityEngine;

[System.Serializable]
public class MoreCapacityAction : UpgradeAction
{
    public override void ActivateUpgrade(UpgradeData upgradeData)
    {
        if (GameManager.Instance == null || GameManager.Instance.ActivePlayer == null)
        {
            Debug.LogWarning(
                "[MoreCapacityAction] No active player found; carrying capacity was not increased."
            );
            return;
        }

        GameManager.Instance.ActivePlayer.IncreaseCarryingCapacity(20f);
    }
}
