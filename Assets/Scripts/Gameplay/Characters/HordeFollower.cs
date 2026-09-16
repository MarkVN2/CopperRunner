using UnityEngine;

// [RequireComponent(typeof(Rigidbody2D))]
public class HordeFollower : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    [SerializeField]
    [Min(0f)]
    private float followSpeed = 3f;

    [SerializeField]
    private bool followHorizontalPosition;

    [SerializeField]
    private bool followVerticalPosition;

    // private Rigidbody2D body;

    private void Awake()
    {
        // body = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (target == null)
            return;

        // Vector2 targetPosition = body.position;
        // if (followHorizontalPosition)
        // {
        //     targetPosition.x = Mathf.MoveTowards(
        //         targetPosition.x,
        //         target.position.x,
        //         followSpeed * Time.fixedDeltaTime
        //     );
        // }

        // if (followVerticalPosition)
        //     targetPosition.y = Mathf.MoveTowards(
        //         targetPosition.y,
        //         target.position.y,
        //         followSpeed * Time.fixedDeltaTime
        //     );

        // body.MovePosition(targetPosition);
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

    public void SetFollowSpeed(float newSpeed)
    {
        followSpeed = Mathf.Max(0f, newSpeed);
    }
}
