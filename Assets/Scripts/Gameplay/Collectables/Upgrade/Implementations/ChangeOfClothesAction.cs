using CopperRunner.Gameplay.Upgrade;
using UnityEngine;

[System.Serializable]
public class ChangeOfClothesAction : UpgradeAction
{
    private const float HordeSpeedMultiplier = 0.98f;

    public override void ActivateUpgrade(UpgradeData upgradeData)
    {
        HordeFollower horde = Object.FindFirstObjectByType<HordeFollower>();
        if (horde == null)
        {
            Debug.LogWarning(
                "[ChangeOfClothesAction] No HordeFollower found; speed reduction was not applied."
            );
            return;
        }

        horde.ApplyRunSpeedMultiplier(HordeSpeedMultiplier);
    }
}
