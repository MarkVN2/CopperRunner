using CopperRunner.Gameplay.Item;
using UnityEngine;

[System.Serializable]
public class SlingPushAction : ItemAction
{
    [Min(0.1f)]
    public float pushBackSeconds = 1f;

    public override void ActivateItem(ItemData itemData)
    {
        HordeFollower horde = Object.FindFirstObjectByType<HordeFollower>();
        if (horde == null)
        {
            Debug.LogWarning("[SlingPushAction] No HordeFollower found; push-back was not applied.");
            return;
        }

        horde.PushBackForSeconds(pushBackSeconds);
    }
}
