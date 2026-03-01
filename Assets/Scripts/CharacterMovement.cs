using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravityScale = 3f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;

    [Header("Wall Jump")]
    [SerializeField] private float wallCheckDistance = 0.3f;
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float wallJumpForceX = 8f;
    [SerializeField] private float wallJumpForceY = 12f;
    [SerializeField] private float wallJumpLockTime = 0.2f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.6f;

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;
    private float moveInputX;
    private float lastDirection = 1f;
    private bool jumpQueued;
    private bool isTouchingWall;
    private bool isWallSliding;
    private int wallDirection; // -1 = wall on left, 1 = wall on right
    private float wallJumpLockTimer;
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private Vector2 dashDirection;

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

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpQueued = true;
        }

        moveInputX = Mathf.Clamp(moveInputX, -1f, 1f);
    }

    // Casts a box sideways from the collider edge to detect walls.
    // Uses three raycasts (top, center, bottom) for reliability with tilemap composite colliders.
    private bool CheckWall(Vector2 direction)
    {
        Bounds bounds = col.bounds;
        float castDist = wallCheckDistance;

        // Cast three horizontal rays: top, center, bottom of the collider (inset slightly)
        float inset = bounds.extents.y * 0.1f;
        Vector2 top    = new Vector2(bounds.center.x, bounds.max.y - inset);
        Vector2 center = new Vector2(bounds.center.x, bounds.center.y);
        Vector2 bottom = new Vector2(bounds.center.x, bounds.min.y + inset);

        float dist = bounds.extents.x + castDist;

        return Physics2D.Raycast(top, direction, dist, groundLayer)
            || Physics2D.Raycast(center, direction, dist, groundLayer)
            || Physics2D.Raycast(bottom, direction, dist, groundLayer);
    }

    private void FixedUpdate()
    {
        // Tick dash cooldown
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.fixedDeltaTime;

        bool isGrounded = Physics2D.Raycast(
            col.bounds.center,
            Vector2.down,
            col.bounds.extents.y + groundCheckDistance,
            groundLayer
        );

        // Reset dash availability when landing
        if (isGrounded && !isDashing)
            dashCooldownTimer = 0f;

        // Wall detection
        bool touchingRight = CheckWall(Vector2.right);
        bool touchingLeft  = CheckWall(Vector2.left);
        isTouchingWall = touchingRight || touchingLeft;
        wallDirection = touchingRight ? 1 : (touchingLeft ? -1 : 0);

        // Touching wall, not grounded, moving into wall, and not mid-wall-jump
        bool pushingIntoWall = (moveInputX > 0.1f && wallDirection == 1)
                            || (moveInputX < -0.1f && wallDirection == -1);
        isWallSliding = isTouchingWall && !isGrounded && pushingIntoWall && wallJumpLockTimer <= 0f;

        // Tick down the wall-jump input lock timer
        if (wallJumpLockTimer > 0f)
        {
            wallJumpLockTimer -= Time.fixedDeltaTime;
        }

        // Ignore input briefly after a wall jump so the player launches away
        float horizontalVelocity;
        if (wallJumpLockTimer > 0f)
        {
            horizontalVelocity = rb.linearVelocity.x;
        }
        else
        {
            horizontalVelocity = moveInputX * moveSpeed;
        }

        Vector2 velocity = new Vector2(horizontalVelocity, rb.linearVelocity.y);

        if (moveInputX > 0)
        {
            lastDirection = 1f;
        }
        else if (moveInputX < 0)
        {
            lastDirection = -1f;
        }

        // Reduce gravity and clamp fall speed so the player slides slowly
        if (isWallSliding)
        {
            rb.gravityScale = 0f;
            velocity.y = -wallSlideSpeed;
        }
        else
        {
            rb.gravityScale = gravityScale;
        }

        animator.SetFloat("Speed", Mathf.Abs(moveInputX));
        animator.SetFloat("Direction", lastDirection);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("YVelocity", rb.linearVelocity.y);

        if (jumpQueued)
        {
            if (isGrounded)
            {
                // Normal ground jump
                velocity.y = jumpForce;
                animator.SetTrigger("Jump");
            }
            else if (isTouchingWall && !isGrounded)
            {
                // Push away from wall and upward
                velocity.x = -wallDirection * wallJumpForceX;
                velocity.y = wallJumpForceY;
                wallJumpLockTimer = wallJumpLockTime;
                animator.SetTrigger("Jump");
            }
            else if (!isGrounded && !isDashing && dashCooldownTimer <= 0f)
            {
                // Mid-air dash in facing direction
                isDashing = true;
                dashTimer = dashDuration;
                dashDirection = new Vector2(lastDirection, 0f);
                rb.gravityScale = 0f;
                animator.SetTrigger("Dash");
            }
        }

        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f)
            {
                // End dash
                isDashing = false;
                dashCooldownTimer = dashCooldown;
                rb.gravityScale = gravityScale;
                velocity = new Vector2(moveInputX * moveSpeed, 0f); // gentle exit
            }
            else
            {
                velocity = dashDirection * dashSpeed;
            }
        }

        jumpQueued = false;
        rb.linearVelocity = velocity;
    }
}
