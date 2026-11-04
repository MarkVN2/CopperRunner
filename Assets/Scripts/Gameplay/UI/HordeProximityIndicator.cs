using TMPro;
using UnityEngine;

public class HordeProximityIndicator : MonoBehaviour
{
    [SerializeField]
    private Player player;

    [SerializeField]
    private HordeFollower horde;

    [Header("Distance Levels")]
    [SerializeField]
    [Min(0f)]
    private float warningDistance = 10f;

    [SerializeField]
    [Min(0f)]
    private float dangerDistance = 5f;

    [SerializeField]
    [Min(0f)]
    private float criticalDistance = 2f;

    [SerializeField]
    [Min(0f)]
    private float distanceHysteresis = 1.5f;

    [Header("Catch-up Warning")]
    [SerializeField]
    [Min(0f)]
    private float catchUpSpeedThreshold = 0.5f;

    [SerializeField]
    [Min(0f)]
    private float catchUpSmoothingTime = 0.4f;

    [SerializeField]
    [Min(0f)]
    private float catchUpWarningHoldTime = 1f;

    [SerializeField]
    private TMP_Text alertText;

    private float previousGap;
    private bool hasPreviousGap;
    private int displayedLevel = -1;
    private int distanceLevel;
    private float smoothedClosingSpeed;
    private float closingSpeedSmoothVelocity;
    private float catchUpWarningRemaining;

    private void Awake()
    {
        if (player == null)
            player = FindFirstObjectByType<Player>();
        if (horde == null)
            horde = FindFirstObjectByType<HordeFollower>();

        if (alertText == null)
        {
            Debug.LogError(
                "[HordeProximityIndicator] Assign the Horde Alert text object in the Inspector.",
                this
            );
            enabled = false;
            return;
        }

        alertText.gameObject.SetActive(false);

        if (player != null && horde != null)
        {
            previousGap = GetHorizontalGap();
            hasPreviousGap = true;
        }
        else
        {
            Debug.LogWarning(
                "[HordeProximityIndicator] A player or horde could not be found.",
                this
            );
        }
    }

    private void Update()
    {
        if (player == null || horde == null || alertText == null)
            return;

        float currentGap = GetHorizontalGap();
        float deltaTime = Time.unscaledDeltaTime;
        float closingSpeed = 0f;

        if (hasPreviousGap && deltaTime > 0f)
            closingSpeed = Mathf.Max(0f, (previousGap - currentGap) / deltaTime);

        previousGap = currentGap;
        hasPreviousGap = true;

        smoothedClosingSpeed = Mathf.SmoothDamp(
            smoothedClosingSpeed,
            closingSpeed,
            ref closingSpeedSmoothVelocity,
            catchUpSmoothingTime,
            Mathf.Infinity,
            deltaTime
        );

        if (catchUpSpeedThreshold > 0f && smoothedClosingSpeed >= catchUpSpeedThreshold)
            catchUpWarningRemaining = catchUpWarningHoldTime;
        else
            catchUpWarningRemaining = Mathf.Max(0f, catchUpWarningRemaining - deltaTime);

        UpdateDistanceLevel(currentGap);
        int level = catchUpWarningRemaining > 0f ? Mathf.Max(distanceLevel, 1) : distanceLevel;

        if (level == displayedLevel)
            return;

        displayedLevel = level;
        alertText.text = new string('!', level);
        alertText.color = GetAlertColor(level);
        alertText.gameObject.SetActive(level > 0);
    }

    private void UpdateDistanceLevel(float distance)
    {
        while (distanceLevel < 3 && distance <= GetEntryDistance(distanceLevel + 1))
            distanceLevel++;

        while (distanceLevel > 0 && distance > GetExitDistance(distanceLevel))
            distanceLevel--;
    }

    private float GetEntryDistance(int level)
    {
        switch (level)
        {
            case 1:
                return warningDistance;
            case 2:
                return dangerDistance;
            default:
                return criticalDistance;
        }
    }

    private float GetExitDistance(int level)
    {
        return GetEntryDistance(level) + distanceHysteresis;
    }

    private float GetHorizontalGap()
    {
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        Collider2D hordeCollider = horde.GetComponent<Collider2D>();
        if (playerCollider == null || hordeCollider == null)
            return Mathf.Abs(player.transform.position.x - horde.transform.position.x);

        Bounds playerBounds = playerCollider.bounds;
        Bounds hordeBounds = hordeCollider.bounds;

        if (playerBounds.min.x >= hordeBounds.max.x)
            return playerBounds.min.x - hordeBounds.max.x;
        if (hordeBounds.min.x >= playerBounds.max.x)
            return hordeBounds.min.x - playerBounds.max.x;

        return 0f;
    }

    private static Color GetAlertColor(int level)
    {
        switch (level)
        {
            case 1:
                return new Color(1f, 0.9f, 0.3f);
            case 2:
                return new Color(1f, 0.55f, 0.2f);
            case 3:
                return new Color(1f, 0.2f, 0.2f);
            default:
                return Color.white;
        }
    }

    private void OnValidate()
    {
        warningDistance = Mathf.Max(0f, warningDistance);
        dangerDistance = Mathf.Clamp(dangerDistance, 0f, warningDistance);
        criticalDistance = Mathf.Clamp(criticalDistance, 0f, dangerDistance);
        distanceHysteresis = Mathf.Max(0f, distanceHysteresis);
        catchUpSpeedThreshold = Mathf.Max(0f, catchUpSpeedThreshold);
        catchUpSmoothingTime = Mathf.Max(0f, catchUpSmoothingTime);
        catchUpWarningHoldTime = Mathf.Max(0f, catchUpWarningHoldTime);
    }
}
