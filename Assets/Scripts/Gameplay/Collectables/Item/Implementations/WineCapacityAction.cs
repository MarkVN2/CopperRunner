using CopperRunner.Gameplay.Item;
using UnityEngine;

[System.Serializable]
public class WineCapacityAction : ItemAction
{
    [Min(0.1f)]
    public float capacityReduction = 5f;

    public override void ActivateItem(ItemData itemData)
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null || gameManager.ActivePlayer == null)
        {
            Debug.LogWarning(
                "[WineCapacityAction] No active player found; carrying capacity was not reduced."
            );
            return;
        }

        gameManager.ActivePlayer.ReduceCarryingCapacity(capacityReduction);
    }
}
