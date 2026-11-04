using CopperRunner.Gameplay.Item;
using UnityEngine;

[System.Serializable]
public class StaffLightningAction : ItemAction
{
    [Min(0.1f)]
    public float stopDuration = 3f;

    public override void ActivateItem(ItemData itemData)
    {
        HordeFollower horde = Object.FindFirstObjectByType<HordeFollower>();
        if (horde == null)
        {
            Debug.LogWarning(
                "[StaffLightningAction] No HordeFollower found; stop effect was not applied."
            );
            return;
        }

        horde.StopForSeconds(stopDuration);
    }
}
