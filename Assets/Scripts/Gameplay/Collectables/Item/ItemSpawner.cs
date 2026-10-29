using System;
using System.Collections.Generic;
using CopperRunner.Gameplay.Item;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Serializable]
    private class SpawnPoint
    {
        public Transform point = null;
        public List<ItemData> possibleItems = new List<ItemData>();
    }

    [SerializeField]
    private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    [SerializeField]
    private bool spawnOnStart = true;

    [SerializeField]
    private int sortingOrder;

    private void Start()
    {
        if (spawnOnStart)
            SpawnItems();
    }

    public void SpawnItems()
    {
        foreach (SpawnPoint spawnPoint in spawnPoints)
        {
            if (spawnPoint == null || spawnPoint.point == null)
            {
                Debug.LogWarning(
                    "[ItemSpawner] Each spawn entry needs a spawn transform.",
                    this
                );
                continue;
            }

            List<ItemData> validItems = new List<ItemData>();
            foreach (ItemData candidate in spawnPoint.possibleItems)
            {
                if (candidate != null)
                    validItems.Add(candidate);
            }

            if (validItems.Count == 0)
            {
                Debug.LogWarning(
                    "[ItemSpawner] Each spawn entry needs at least one valid item.",
                    this
                );
                continue;
            }

            ItemData item = validItems[UnityEngine.Random.Range(0, validItems.Count)];
            Sprite worldTexture = item.GetWorldTexture();
            if (worldTexture == null)
            {
                Debug.LogWarning(
                    $"[ItemSpawner] Item '{item.name}' has no world texture.",
                    item
                );
                continue;
            }

            GameObject pickup = new GameObject($"{item.name} Pickup");
            pickup.transform.SetPositionAndRotation(
                spawnPoint.point.position,
                spawnPoint.point.rotation
            );

            SpriteRenderer spriteRenderer = pickup.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = worldTexture;
            spriteRenderer.sortingOrder = sortingOrder;

            BoxCollider2D pickupCollider = pickup.AddComponent<BoxCollider2D>();
            pickupCollider.isTrigger = true;
            pickupCollider.size = worldTexture.bounds.size;

            ItemPickup itemPickup = pickup.AddComponent<ItemPickup>();
            itemPickup.Initialize(item);
        }
    }
}
