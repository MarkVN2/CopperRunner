using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Actor))]
public class CharacterMovement : MonoBehaviour
{
    private Rigidbody2D body;
    private Actor actor;
    private InputSystem_Actions input;
    private InputAction moveAction;
    private InputAction jumpAction;
    private readonly HashSet<Collider2D> groundedColliders = new HashSet<Collider2D>();
    private bool isGrounded;
    private bool jumpRequested;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        actor = GetComponent<Actor>();

        input = new InputSystem_Actions();
        moveAction = input.Player.Move;
        jumpAction = input.Player.Jump;
    }

    private void OnEnable()
    {
        input.Player.Enable();
    }

    private void OnDisable()
    {
        input.Player.Disable();
    }

    private void OnDestroy()
    {
        input.Dispose();
    }

    private void Update()
    {
        if (jumpAction.WasPressedThisFrame() && isGrounded)
            jumpRequested = true;
    }

    private void FixedUpdate()
    {
        Vector2 velocity = body.linearVelocity;
        velocity.x = moveAction.ReadValue<Vector2>().x * actor.GetMoveSpeed();

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
