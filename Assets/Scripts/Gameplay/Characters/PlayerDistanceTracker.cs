using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDistanceTracker : MonoBehaviour
{
    [SerializeField]
    private float distanceTravelled;
    public float DistanceTravelled
    {
        get { return distanceTravelled; }
        private set { distanceTravelled = value; }
    }

    [SerializeField]
    private bool trackOnXAxisOnly = true;

    private Vector2 lastPosition;
    private Rigidbody2D body;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        lastPosition = body.position;
        DistanceTravelled = 0f;
    }

    private void FixedUpdate()
    {
        Vector2 currentPosition = body.position;

        float delta = trackOnXAxisOnly
            ? Mathf.Max(0f, currentPosition.x - lastPosition.x)
            : Vector2.Distance(currentPosition, lastPosition);

        if (delta > 0f)
        {
            DistanceTravelled += delta;
        }

        lastPosition = currentPosition;
    }

    public void ResetDistance()
    {
        DistanceTravelled = 0f;
        lastPosition = body.position;
    }
}
