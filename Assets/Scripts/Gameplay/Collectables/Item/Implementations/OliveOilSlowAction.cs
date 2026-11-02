using CopperRunner.Gameplay.Item;
using UnityEngine;

[System.Serializable]
public class OliveOilSlowAction : ItemAction
{
    [Range(0.01f, 1f)]
    public float speedMultiplier = 0.5f;

    [Min(0.1f)]
    public float duration = 3f;

    public override void ActivateItem(ItemData itemData)
    {
        HordeFollower horde = Object.FindFirstObjectByType<HordeFollower>();
        if (horde == null)
        {
            Debug.LogWarning(
                "[OliveOilSlowAction] No HordeFollower found; slow effect was not applied."
            );
            return;
        }

        horde.ApplySlow(speedMultiplier, duration);
    }
}
