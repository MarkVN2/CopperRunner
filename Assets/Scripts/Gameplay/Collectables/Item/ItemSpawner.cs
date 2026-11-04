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
    private int sortingOrder;

    [SerializeField]
    private Color glowColor = new Color(1f, 0.78f, 0.32f, 0.2f);

    [SerializeField]
    [Min(1f)]
    private float outerGlowScale = 1.55f;

    [SerializeField]
    [Min(1f)]
    private float innerGlowScale = 1.28f;

    private readonly List<GameObject> spawnedPickups = new List<GameObject>();

    private void Awake()
    {
        if (spawnPoints.Count == 0)
            Debug.LogWarning("[ItemSpawner] No spawn points defined.", this);
        SpawnItems();
    }

    public void SpawnItems()
    {
        ClearSpawnedPickups();

        foreach (SpawnPoint spawnPoint in spawnPoints)
        {
            if (spawnPoint == null || spawnPoint.point == null)
            {
                Debug.LogWarning("[ItemSpawner] Each spawn entry needs a spawn transform.", this);
                continue;
            }

            if (spawnPoint.possibleItems == null)
            {
                Debug.LogWarning(
                    "[ItemSpawner] Each spawn entry needs a list of possible items.",
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
                Debug.LogWarning($"[ItemSpawner] Item '{item.name}' has no world texture.", item);
                continue;
            }

            GameObject pickup = new GameObject($"{item.name} Pickup");
            pickup.transform.SetPositionAndRotation(
                spawnPoint.point.position,
                spawnPoint.point.rotation
            );
            pickup.transform.SetParent(transform, true);

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(pickup.transform, false);

            SpriteRenderer spriteRenderer = visual.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = worldTexture;
            spriteRenderer.sortingOrder = sortingOrder;
            visual.AddComponent<FloatingPickupEffect>();

            CreateGlowLayer(
                visual.transform,
                worldTexture,
                glowColor,
                outerGlowScale,
                sortingOrder - 2,
                "Outer Glow"
            );
            Color innerGlowColor = glowColor;
            innerGlowColor.a *= 0.75f;
            CreateGlowLayer(
                visual.transform,
                worldTexture,
                innerGlowColor,
                innerGlowScale,
                sortingOrder - 1,
                "Inner Glow"
            );

            BoxCollider2D pickupCollider = pickup.AddComponent<BoxCollider2D>();
            pickupCollider.isTrigger = true;
            pickupCollider.size = worldTexture.bounds.size;

            ItemPickup itemPickup = pickup.AddComponent<ItemPickup>();
            itemPickup.Initialize(item);
            spawnedPickups.Add(pickup);
        }
    }

    private static void CreateGlowLayer(
        Transform parent,
        Sprite sprite,
        Color color,
        float scale,
        int layerOrder,
        string layerName
    )
    {
        GameObject glowObject = new GameObject(layerName);
        glowObject.transform.SetParent(parent, false);
        glowObject.transform.localScale = Vector3.one * Mathf.Max(1f, scale);

        SpriteRenderer glowRenderer = glowObject.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = sprite;
        glowRenderer.color = color;
        glowRenderer.sortingOrder = layerOrder;
    }

    private void ClearSpawnedPickups()
    {
        foreach (GameObject pickup in spawnedPickups)
        {
            if (pickup == null)
                continue;

            pickup.SetActive(false);
            Destroy(pickup);
        }

        spawnedPickups.Clear();
    }
}
