using UnityEngine;
using UnityEngine.Serialization;

public class HordeFollower : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    [Tooltip("World units per real-time second while the horde approaches in a shop.")]
    [FormerlySerializedAs("followSpeed")]
    [SerializeField]
    [Min(0f)]
    private float shopApproachSpeed = 0.75f;

    [Tooltip("The horde runs at this fraction of the player's normal run speed.")]
    [SerializeField]
    [Range(0f, 1f)]
    private float runSpeedRatio = 0.9f;

    [SerializeField]
    private MapManager mapManager;

    private void Awake()
    {
        if (target == null)
        {
            Player player = FindFirstObjectByType<Player>();
            if (player != null)
                target = player.transform;
        }

        if (mapManager == null)
            mapManager = FindFirstObjectByType<MapManager>();

        if (target == null)
            Debug.LogWarning("[HordeFollower] No player target is assigned or available.", this);
        if (mapManager == null)
            Debug.LogWarning("[HordeFollower] No MapManager is assigned or available.", this);
    }

    private void Update()
    {
        if (target == null)
            return;

        Vector3 position = transform.position;
        UpgradeShopMenu shopMenu = UpgradeShopMenu.Instance;

        if (shopMenu != null && shopMenu.IsOpen)
        {
            position.x = Mathf.MoveTowards(
                position.x,
                target.position.x,
                shopApproachSpeed * Time.unscaledDeltaTime
            );
        }
        else if (mapManager != null)
        {
            float hordeRunSpeed = mapManager.NormalSpeed * runSpeedRatio;
            position.x += (hordeRunSpeed - mapManager.CurrentSpeed) * Time.deltaTime;
        }

        transform.position = position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        KillPlayer(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        KillPlayer(other);
    }

    private void KillPlayer(Collider2D collider)
    {
        if (collider == null)
            return;

        Player player = collider.GetComponentInParent<Player>();
        if (player != null)
            player.Die();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetShopApproachSpeed(float newSpeed)
    {
        shopApproachSpeed = Mathf.Max(0f, newSpeed);
    }
}
