using UnityEngine;
using UnityEngine.Events;

public class RunnerCheckpoint : MonoBehaviour
{
    [SerializeField]
    private PlayerDistanceTracker distanceTracker;

    [SerializeField]
    [Min(0.01f)]
    private float targetDistance = 100f;

    [SerializeField]
    private bool triggerOnlyOnce = true;

    [SerializeField]
    private UnityEvent onCheckpointReached;

    private bool hasTriggered;

    private void Update()
    {
        if (distanceTracker == null)
            return;

        if (triggerOnlyOnce && hasTriggered)
            return;

        if (distanceTracker.DistanceTravelled >= targetDistance)
        {
            TriggerCheckpoint();

            if (!triggerOnlyOnce)
            {
                targetDistance = distanceTracker.DistanceTravelled + targetDistance;
            }
        }
    }

    public void TriggerCheckpoint()
    {
        if (triggerOnlyOnce && hasTriggered)
            return;

        hasTriggered = true;
        onCheckpointReached?.Invoke();
    }

    public void ResetCheckpoint()
    {
        hasTriggered = false;
    }
}
