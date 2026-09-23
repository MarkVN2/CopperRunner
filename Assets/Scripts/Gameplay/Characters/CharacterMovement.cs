using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Actor))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Runner")]
    [SerializeField]
    private bool autoRun = true;

    [SerializeField]
    [Min(0f)]
    private float forwardSpeed = 5f;

    [Header("Air Control & Centering")]
    [SerializeField]
    [Range(0f, 1f)]
    private float airControl = 0.5f;

    [SerializeField]
    private float centeringSpeed = 5f;

    [SerializeField]
    private float maxAirSpeedOffset = 3f;

    private Rigidbody2D body;
    private Actor actor;
    private MapManager mapManager;
    private Camera mainCamera;

    private InputSystem_Actions input;
    private InputAction moveAction;
    private InputAction jumpAction;

    private readonly HashSet<Collider2D> groundedColliders = new HashSet<Collider2D>();
    private bool isGrounded;
    private bool jumpRequested;

    private float idealWorldX;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        actor = GetComponent<Actor>();
        mapManager = FindFirstObjectByType<MapManager>();
        mainCamera = Camera.main;

        input = new InputSystem_Actions();
        moveAction = input.Player.Move;
        jumpAction = input.Player.Jump;
    }

    private void Start()
    {
        idealWorldX = transform.position.x;
    }

    private void OnEnable() => input.Player.Enable();

    private void OnDisable() => input.Player.Disable();

    private void OnDestroy() => input.Dispose();

    private void Update()
    {
        if (jumpAction.WasPressedThisFrame() && isGrounded)
            jumpRequested = true;
    }

    private void FixedUpdate()
    {
        Vector2 velocity = body.linearVelocity;
        float horizontalInput = moveAction.ReadValue<Vector2>().x;

        float targetHorizontalSpeed = 0f;

        if (isGrounded)
        {
            targetHorizontalSpeed = 0f;
            idealWorldX = transform.position.x;
        }
        else
        {
            float airOffset = horizontalInput * maxAirSpeedOffset;
            float positionError = idealWorldX - transform.position.x;
            float centeringCorrection = positionError * centeringSpeed;

            float desiredAirSpeed = centeringCorrection + airOffset;
            targetHorizontalSpeed = Mathf.Lerp(velocity.x, desiredAirSpeed, airControl);
        }

        velocity.x = targetHorizontalSpeed;

        if (jumpRequested)
        {
            velocity.y = actor.GetJumpForce();
            jumpRequested = false;
            isGrounded = false;
        }

        body.linearVelocity = velocity;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                groundedColliders.Add(collision.collider);
                isGrounded = true;
                return;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        groundedColliders.Remove(collision.collider);
        isGrounded = groundedColliders.Count > 0;
    }
}
