using CopperRunner.Gameplay.Item;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    private ItemData itemData;
    private bool collected;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    public void Initialize(ItemData item)
    {
        itemData = item;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || itemData == null || other == null)
            return;

        Player player = other.GetComponentInParent<Player>();
        if (player == null || player.IsDead)
            return;

        collected = true;
        player.AddItem(itemData);
        Destroy(gameObject);
    }
}
