using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HordeFollower : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    [SerializeField]
    [Min(0f)]
    private float followSpeed = 3f;

    [SerializeField]
    private bool followVerticalPosition;

    private Rigidbody2D body;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (target == null)
            return;

        Vector2 targetPosition = body.position;
        targetPosition.x = Mathf.MoveTowards(
            targetPosition.x,
            target.position.x,
            followSpeed * Time.fixedDeltaTime
        );

        if (followVerticalPosition)
            targetPosition.y = Mathf.MoveTowards(
                targetPosition.y,
                target.position.y,
                followSpeed * Time.fixedDeltaTime
            );

        body.MovePosition(targetPosition);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetFollowSpeed(float newSpeed)
    {
        followSpeed = Mathf.Max(0f, newSpeed);
    }
}
