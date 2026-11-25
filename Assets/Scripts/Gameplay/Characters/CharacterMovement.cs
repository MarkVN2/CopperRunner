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

    [Header("Jump & Fast Down")]
    [SerializeField]
    [Min(0.1f)]
    private float jumpHeightMultiplier = 1.15f;

    [SerializeField]
    [Range(0f, 1f)]
    private float variableJumpHeightMultiplier = 0.5f;

    [SerializeField]
    [Min(0f)]
    private float coyoteTime = 0.12f;

    [SerializeField]
    [Min(0f)]
    private float jumpBufferTime = 0.12f;

    [SerializeField]
    [Range(0.1f, 1f)]
    private float apexGravityMultiplier = 0.6f;

    [SerializeField]
    private float apexVelocityThreshold = 1.5f;

    [SerializeField]
    [Min(0f)]
    private float fastDownSpeed = 22f;

    [SerializeField]
    private bool slamLockHorizontalAirControl = false;

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
    private InputAction fastDownAction;

    private readonly HashSet<Collider2D> groundedColliders = new HashSet<Collider2D>();
    private bool isGrounded;
    private bool jumpReleasedEarly;
    private bool fastDownHeld;
    private bool isSlamming;

    private float coyoteTimer;
    private float jumpBufferTimer;
    private float defaultGravityScale;

    [SerializeField]
    private float idealWorldX;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        actor = GetComponent<Actor>();
        defaultGravityScale = body.gravityScale;

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
        fastDownAction = input.asset.FindAction("FastDown", throwIfNotFound: false);

        if (fastDownAction == null)
            Debug.LogWarning(
                "[CharacterMovement] 'FastDown' action not found in InputSystem_Actions.",
                this
            );

        idealWorldX = transform.position.x;
    }

    private void OnEnable() => input.Player.Enable();

    private void OnDisable()
    {
        input.Player.Disable();
        fastDownHeld = false;
        isSlamming = false;
        jumpReleasedEarly = false;
        coyoteTimer = 0f;
        jumpBufferTimer = 0f;
        body.gravityScale = defaultGravityScale;

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

        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        if (jumpBufferTimer > 0f)
            jumpBufferTimer -= Time.deltaTime;

        // Poll inputs in Update() to avoid missing quick presses/releases
        if (jumpAction.WasPressedThisFrame())
        {
            jumpBufferTimer = jumpBufferTime;
        }

        if (jumpAction.WasReleasedThisFrame())
        {
            jumpReleasedEarly = true;
        }

        if (!isGrounded && fastDownAction != null)
        {
            fastDownHeld = fastDownAction.IsPressed();
            if (fastDownHeld)
            {
                isSlamming = true;
            }
        }
        else
        {
            fastDownHeld = false;
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

        // 1. Trigger buffered jump if grounded or within coyote time
        if (jumpBufferTimer > 0f && (isGrounded || coyoteTimer > 0f))
        {
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            jumpReleasedEarly = false;
            isSlamming = false;
            SetGrounded(false);

            float jumpForce = actor.GetJumpForce() * jumpHeightMultiplier;
            velocity.y = jumpForce;

            Debug.Log("Jumping with velocity.y: " + velocity.y);
        }

        // 2. Horizontal movement & vertical air modifiers
        if (isGrounded)
        {
            velocity.x = centeringCorrection;
            body.gravityScale = defaultGravityScale;
            jumpReleasedEarly = false;
        }
        else
        {
            if (!(isSlamming && slamLockHorizontalAirControl))
            {
                float airOffset = horizontalInput * maxAirSpeedOffset;
                float desiredAirSpeed = centeringCorrection + airOffset;
                velocity.x = Mathf.Lerp(velocity.x, desiredAirSpeed, airControl);
            }
            else
            {
                velocity.x = centeringCorrection;
            }

            if (fastDownHeld)
            {
                body.gravityScale = defaultGravityScale;
                velocity.y = -fastDownSpeed;
                jumpReleasedEarly = false;
            }
            else
            {
                if (jumpReleasedEarly && velocity.y > 0f)
                {
                    velocity.y *= variableJumpHeightMultiplier;
                    jumpReleasedEarly = false;
                }

                bool nearApex =
                    Mathf.Abs(velocity.y) <= apexVelocityThreshold && jumpAction.IsPressed();
                body.gravityScale = nearApex
                    ? defaultGravityScale * apexGravityMultiplier
                    : defaultGravityScale;
            }
        }

        body.linearVelocity = velocity;
    }

    private void SetGrounded(bool grounded)
    {
        if (isGrounded != grounded)
        {
            isGrounded = grounded;

            if (isGrounded)
            {
                fastDownHeld = false;
                isSlamming = false;
            }

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
