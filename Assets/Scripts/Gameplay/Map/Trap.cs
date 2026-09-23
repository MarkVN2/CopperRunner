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
    private float slowDuration = 5f;

    [SerializeField]
    private float playerPullToHorde = 1.5f;

    [SerializeField]
    private float hitCooldown = 0.75f;

    [SerializeField]
    private float idealWorldX;
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

        CharacterMovement movement = player.GetComponent<CharacterMovement>();

        float pushAmount = playerPullToHorde * mistakeCount;

        if (movement != null)
        {
            movement.ShiftIdealWorldX(pushAmount);
        }
    }
}
