using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Trap : MonoBehaviour
{
    [SerializeField]
    private MapManager mapManager;

    [SerializeField]
    private Transform horde;

    [SerializeField]
    private float slowMultiplier = 0.75f;

    [SerializeField]
    private float slowDuration = 999999f;

    [SerializeField]
    private float playerPullToHorde = 1.5f;

    [SerializeField]
    private float hitCooldown = 0.75f;

    private float lastHitTime;
    private int mistakeCount;

    private void Awake()
    {
        if (mapManager == null)
            mapManager = FindFirstObjectByType<MapManager>();

        if (horde == null)
        {
            GameObject hordeObject = GameObject.FindGameObjectWithTag("Enemy");
            if (hordeObject != null)
                horde = hordeObject.transform;
        }

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
        mistakeCount++;

        if (mapManager != null)
            mapManager.ApplySpeedModifier(slowMultiplier, slowDuration);

        Rigidbody2D body = other.attachedRigidbody;
        float pushAmount = playerPullToHorde * mistakeCount;

        if (body != null)
        {
            body.position += Vector2.left * pushAmount;
        }
        else
        {
            other.transform.position += Vector3.left * pushAmount;
        }
    }
}
