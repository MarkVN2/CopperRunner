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

    private float slowMultiplier = 1f;
    private float slowRemaining;
    private float stopRemaining;

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
        if (PauseMenu.IsPaused)
            return;

        UpdateEffects();

        if (target == null)
            return;

        Vector3 position = transform.position;
        UpgradeShopMenu shopMenu = UpgradeShopMenu.Instance;

        if (shopMenu != null && shopMenu.IsOpen)
        {
            if (stopRemaining <= 0f)
            {
                position.x = Mathf.MoveTowards(
                    position.x,
                    target.position.x,
                    shopApproachSpeed * slowMultiplier * Time.unscaledDeltaTime
                );
            }
        }
        else if (mapManager != null)
        {
            float hordeRunSpeed =
                stopRemaining > 0f ? 0f : mapManager.NormalSpeed * runSpeedRatio * slowMultiplier;
            position.x += (hordeRunSpeed - mapManager.CurrentSpeed) * Time.deltaTime;
        }

        transform.position = position;
    }

    private void UpdateEffects()
    {
        float elapsed = Time.unscaledDeltaTime;

        if (slowRemaining > 0f)
        {
            slowRemaining = Mathf.Max(0f, slowRemaining - elapsed);
            if (slowRemaining == 0f)
                slowMultiplier = 1f;
        }

        if (stopRemaining > 0f)
            stopRemaining = Mathf.Max(0f, stopRemaining - elapsed);
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

    public void ApplyRunSpeedMultiplier(float multiplier)
    {
        if (multiplier <= 0f || multiplier > 1f)
        {
            Debug.LogWarning(
                "[HordeFollower] Run speed multiplier must be greater than zero and at most one.",
                this
            );
            return;
        }

        runSpeedRatio *= multiplier;
    }

    public void PushBackForSeconds(float seconds)
    {
        if (seconds <= 0f)
        {
            Debug.LogWarning("[HordeFollower] Push-back duration must be greater than zero.", this);
            return;
        }

        if (mapManager == null)
        {
            Debug.LogWarning("[HordeFollower] Cannot push back without a MapManager.", this);
            return;
        }

        Vector3 position = transform.position;
        position.x -= mapManager.NormalSpeed * runSpeedRatio * seconds;
        transform.position = position;
    }

    public void ApplySlow(float multiplier, float duration)
    {
        if (multiplier <= 0f || duration <= 0f)
        {
            Debug.LogWarning(
                "[HordeFollower] Slow multiplier and duration must be greater than zero.",
                this
            );
            return;
        }

        slowMultiplier = Mathf.Min(slowMultiplier, Mathf.Clamp(multiplier, 0.01f, 1f));
        slowRemaining = Mathf.Max(slowRemaining, duration);
    }

    public void StopForSeconds(float duration)
    {
        if (duration <= 0f)
        {
            Debug.LogWarning("[HordeFollower] Stop duration must be greater than zero.", this);
            return;
        }

        stopRemaining = Mathf.Max(stopRemaining, duration);
    }
}
