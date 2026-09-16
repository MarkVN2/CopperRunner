using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDistanceTracker : MonoBehaviour
{
    [SerializeField]
    private MapManager mapManager;

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
    private float lastMapDistance;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        if (mapManager == null)
            mapManager = FindFirstObjectByType<MapManager>();
    }

    private void Start()
    {
        lastPosition = body.position;
        DistanceTravelled = 0f;
        lastMapDistance = mapManager != null ? mapManager.TotalDistanceTravelled : 0f;
    }

    private void FixedUpdate()
    {
        if (mapManager != null)
        {
            float currentMapDistance = mapManager.TotalDistanceTravelled;
            float _delta = Mathf.Max(0f, currentMapDistance - lastMapDistance);
            DistanceTravelled += _delta;
            lastMapDistance = currentMapDistance;
            return;
        }

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
        lastMapDistance = mapManager != null ? mapManager.TotalDistanceTravelled : 0f;
    }
}
