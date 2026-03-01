using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class CharacterMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravityScale = 3f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;
    private float lastDirection = 1f;
    private float moveInputX;
    private bool jumpQueued;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.gravityScale = gravityScale;
    }

    private void Update()
    {
        moveInputX = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                moveInputX -= 1f;
            }

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                moveInputX += 1f;
            }
        }

        if (Gamepad.current != null)
        {
            moveInputX += Gamepad.current.leftStick.x.ReadValue();
            moveInputX += Gamepad.current.dpad.x.ReadValue();

            if (Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                jumpQueued = true;
            }
        }

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpQueued = true;
        }

        moveInputX = Mathf.Clamp(moveInputX, -1f, 1f);

        if (jumpQueued && Physics2D.Raycast(col.bounds.center, Vector2.down,
            col.bounds.extents.y + groundCheckDistance, groundLayer))
        {
            animator.SetTrigger("Jump");
            jumpQueued = false;
        }
    }

    private void FixedUpdate()
    {
        bool isGrounded = Physics2D.Raycast(
            col.bounds.center,
            Vector2.down,
            col.bounds.extents.y + groundCheckDistance,
            groundLayer
        );

        Vector2 velocity = new Vector2(moveInputX * moveSpeed, rb.linearVelocity.y);

        if (moveInputX > 0)
        {
            lastDirection = 1f;
        }
        else if (moveInputX < 0)
        {
            lastDirection = -1f;
        }

        animator.SetFloat("Speed", Mathf.Abs(moveInputX));
        animator.SetFloat("Direction", lastDirection);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("YVelocity", rb.linearVelocity.y);

        jumpQueued = false;
        rb.linearVelocity = velocity;
    }
}
