using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Actor))]
[RequireComponent(typeof(Animator))]
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

    [Header("Jump Physics")]
    [SerializeField]
    [Range(0f, 1f)]
    private float variableJumpHeightMultiplier = 0.5f;

    [Header("Animation")]
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private ParticleSystem groundDust;

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
    private bool jumpReleasedEarly;

    [SerializeField]
    private float idealWorldX;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        actor = GetComponent<Actor>();

        if (animator == null)
            animator = GetComponent<Animator>();

        mapManager = FindFirstObjectByType<MapManager>();
        mainCamera = Camera.main;

        if (groundDust == null)
            groundDust = GetComponentInChildren<ParticleSystem>(true);

        if (groundDust == null)
            Debug.LogWarning("[CharacterMovement] Assign the GroundDust particle system.", this);

        input = new InputSystem_Actions();
        moveAction = input.Player.Move;
        jumpAction = input.Player.Jump;

        idealWorldX = transform.position.x;
    }

    private void OnEnable() => input.Player.Enable();

    private void OnDisable()
    {
        input.Player.Disable();
        if (groundDust != null && groundDust.isEmitting)
            groundDust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    private void OnDestroy()
    {
        input.Dispose();
    }

    private void Update()
    {
        if (PauseMenu.IsPaused)
            return;

        UpdateGroundDust();

        // Poll inputs in Update() to avoid missing quick presses/releases
        if (jumpAction.WasPressedThisFrame() && isGrounded)
        {
            jumpRequested = true;
        }

        if (jumpAction.WasReleasedThisFrame())
        {
            jumpReleasedEarly = true;
        }
    }

    private void UpdateGroundDust()
    {
        if (groundDust == null)
            return;

        bool isMoving =
            (mapManager != null && mapManager.CurrentSpeed > 0.05f)
            || body.linearVelocity.sqrMagnitude > 0.01f;
        bool shouldEmit = isGrounded && isMoving;
        if (shouldEmit && !groundDust.isPlaying)
            groundDust.Play();
        else if (!shouldEmit && groundDust.isEmitting)
            groundDust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    private void FixedUpdate()
    {
        if (PauseMenu.IsPaused)
            return;

        Vector2 velocity = body.linearVelocity;
        float horizontalInput = moveAction.ReadValue<Vector2>().x;

        // Centering X position logic
        float positionError = idealWorldX - transform.position.x;
        float centeringCorrection = positionError * centeringSpeed;

        if (isGrounded)
        {
            velocity.x = centeringCorrection;
        }
        else
        {
            float airOffset = horizontalInput * maxAirSpeedOffset;
            float desiredAirSpeed = centeringCorrection + airOffset;
            velocity.x = Mathf.Lerp(velocity.x, desiredAirSpeed, airControl);

            // Variable jump height check
            if (jumpReleasedEarly && velocity.y > 0f)
            {
                velocity.y *= variableJumpHeightMultiplier;
                jumpReleasedEarly = false;
            }
        }

        // Handle Jump execution cleanly via velocity modification
        if (jumpRequested)
        {
            jumpRequested = false;
            SetGrounded(false);

            // Use direct velocity assignment for the jump to ensure it triggers instantly and reliably
            float jumpForce = actor.GetJumpForce();
            // If Actor returns a force value instead of velocity, you can use body.AddForce instead,
            // but setting velocity.y directly gives tight platformer control:
            velocity.y = jumpForce;

            Debug.Log("Jumping with velocity.y: " + velocity.y);
        }

        body.linearVelocity = velocity;

        // Reset single-frame input flags
        jumpReleasedEarly = false;
    }

    private void SetGrounded(bool grounded)
    {
        if (isGrounded != grounded)
        {
            isGrounded = grounded;
            if (animator != null)
            {
                animator.SetBool("IsGrounded", isGrounded);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) => EvaluateCollision(collision);

    private void OnCollisionStay2D(Collision2D collision) => EvaluateCollision(collision);

    private void EvaluateCollision(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Lowered threshold slightly to 0.3f to catch sloped floors or minor angle discrepancies
            if (contact.normal.y > 0.3f)
            {
                groundedColliders.Add(collision.collider);
                SetGrounded(true);
                return;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        groundedColliders.Remove(collision.collider);
        if (groundedColliders.Count == 0)
        {
            SetGrounded(false);
        }
    }
}
