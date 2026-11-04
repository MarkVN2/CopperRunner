using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Trap : MonoBehaviour
{
    [SerializeField]
    private MapManager mapManager;

    [SerializeField]
    private float slowMultiplier = 0.75f;

    [SerializeField]
    private float slowDuration = 5f;

    [SerializeField]
    private float hitCooldown = 0.75f;

    private float lastHitTime;

    private void Awake()
    {
        if (mapManager == null)
            mapManager = FindFirstObjectByType<MapManager>();

        Collider2D trigger = GetComponent<Collider2D>();
        if (trigger != null)
            trigger.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null)
            return;

        Player player = other.GetComponentInParent<Player>();
        if (player == null)
            return;

        if (Time.time - lastHitTime < hitCooldown)
            return;

        lastHitTime = Time.time;

        if (mapManager != null)
            mapManager.ApplySpeedModifier(slowMultiplier, slowDuration);
    }
}
