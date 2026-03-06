using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField]
    private float rotationSpeed = 10f;

    [SerializeField]
    private float slopeCheckDistance = 0.5f;

    [SerializeField]
    private float slopeSlideSpeed = 1.5f;

    [SerializeField]
    private float gravityScale = 2.5f;

    [Header("Jumping")]
    [SerializeField]
    private float jumpForce = 10f;

    [SerializeField]
    private LayerMask groundLayer;

    [SerializeField]
    private float groundCheckDistance = 0.1f;

    [Header("Wall Jump")]
    [SerializeField]
    private float wallCheckDistance = 0.1f;

    [SerializeField]
    private float wallSlideSpeed = 4f;

    [SerializeField]
    private float wallJumpForceX = 8f;

    [SerializeField]
    private float wallJumpForceY = 10f;

    [SerializeField]
    private float wallJumpLockTime = 1f;

    [Header("Dash")]
    [SerializeField]
    private float dashSpeed = 18f;

    [SerializeField]
    private float dashDuration = 0.15f;

    [SerializeField]
    private float dashCooldown = 3f;

    [SerializeField]
    private Image dashIndicatorOverlay;

    [SerializeField]
    private Image dashIndicatorUnderlay;

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;

    private float moveInputX;
    private float lastDirection = 1f;
    private float slopeSlideTime;
    private Vector2 slopeNormal = Vector2.up;
    private bool jumpQueued;
    private bool isTouchingWall;
    private bool isWallSliding;
    private int wallDirection; // -1 = left, 1 = right
    private float wallJumpLockTimer;
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private float dashExitTimer;
    private Vector2 dashExitStartVelocity;
    private Vector2 dashDirection;

    private void Awake()
    {
        animator = GetComponent<Animator>(); // the player animator component
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.gravityScale = gravityScale;
    }

    private void Update()
    {
        moveInputX = 0f;

        // Keys for player movement
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                moveInputX -= 1f;

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                moveInputX += 1f;

            if (
                Keyboard.current.spaceKey.wasPressedThisFrame
                || Keyboard.current.upArrowKey.wasPressedThisFrame
            )
                jumpQueued = true;
        }
    }

    private void FixedUpdate()
    {
        // A cooldown for dashing
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.fixedDeltaTime;

            dashIndicatorOverlay.enabled = true;
            dashIndicatorUnderlay.enabled = true;

            dashIndicatorOverlay.fillAmount = dashCooldownTimer / dashCooldown;
        }
        else
        {
            dashIndicatorOverlay.enabled = false;
            dashIndicatorUnderlay.enabled = false;
        }

        bool isGrounded = Physics2D.Raycast(
            col.bounds.center,
            Vector2.down,
            col.bounds.extents.y + groundCheckDistance,
            groundLayer
        );

        AlignWithSurface();

        bool touchingRight = CheckWall(Vector2.right);
        bool touchingLeft = CheckWall(Vector2.left);
        isTouchingWall = touchingRight || touchingLeft;
        wallDirection = touchingRight ? 1 : (touchingLeft ? -1 : 0); // basically: 1 if touchingRight else (-1 if touchingLeft else 0)

        /* "Wall sliding" is essentially when the player is "moving" in the direction of the wall while already touching the wall.
         * This mechanic should cause the player to slide down the wall slower than if they were to simply fall straight down.
         * The player is pushing into the wall if either (a) the player is moving right and the wall is on the right, or (b) if the player is moving left and the wall is on the left.
         * If the player is touching the wall, isn't grounded, is pushing into the wall, and the wall jump lock timer has ended, then the player is wall sliding.
         */
        bool pushingIntoWall =
            (moveInputX > 0.1f && wallDirection == 1)
            || (moveInputX < -0.1f && wallDirection == -1);
        isWallSliding = isTouchingWall && !isGrounded && pushingIntoWall;

        if (wallJumpLockTimer > 0f)
            wallJumpLockTimer -= Time.fixedDeltaTime;

        // Launch the player away from the wall after the wall jump
        float horizontalVelocity;
        if (wallJumpLockTimer > 0f)
        {
            float t = wallJumpLockTimer / wallJumpLockTime;
            float T = t * t;
            float targetVelocity = moveInputX * moveSpeed;
            horizontalVelocity = Mathf.Lerp(targetVelocity, rb.linearVelocity.x, T);
        }
        else
            horizontalVelocity = moveInputX * moveSpeed;

        /* Well, isn't this great!
         * Hardcoding `0.1f` here because I doubt the slope distance threshold would ever change in this case.
         * Probably not the best practice, but if anything goes wrong I'll know why!
         */
        bool isOnSlope = isGrounded && Mathf.Abs(slopeNormal.x) > 0.1f;
        Vector2 velocity = isOnSlope
            ? new Vector2(slopeNormal.y, -slopeNormal.x) * (moveInputX * moveSpeed) // vector points up the slope
            : new Vector2(horizontalVelocity, rb.linearVelocity.y); // vector goes horizontally

        // Slide down slope
        if (isOnSlope && moveInputX == 0f)
        {
            slopeSlideTime += Time.fixedDeltaTime;
            float slideDir = Mathf.Sign(slopeNormal.x);
            float acceleratedSpeed = slopeSlideSpeed * (1f + slopeSlideTime * slopeSlideTime);
            velocity += new Vector2(slopeNormal.y, -slopeNormal.x) * (slideDir * acceleratedSpeed);
        }
        else
        {
            slopeSlideTime = 0f;
        }

        if (moveInputX > 0)
            lastDirection = 1f;
        else if (moveInputX < 0)
            lastDirection = -1f;

        if (isWallSliding)
        {
            rb.gravityScale = 0f;
            velocity.y = -wallSlideSpeed;
        }
        else
            rb.gravityScale = gravityScale;

        animator.SetFloat("Speed", Mathf.Abs(moveInputX));
        animator.SetFloat("Direction", lastDirection);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("YVelocity", rb.linearVelocity.y);

        if (jumpQueued)
        {
            if (isGrounded)
            {
                // Since the player is on the ground, just jump normally
                velocity.y = jumpForce;
                wallJumpLockTimer = wallJumpLockTime;
                animator.SetTrigger("Jump");
            }
            else if (isTouchingWall && !isGrounded)
            {
                // If the player is on the wall, do a wall hop
                velocity.x = -wallDirection * wallJumpForceX;
                velocity.y = wallJumpForceY;
                wallJumpLockTimer = wallJumpLockTime;
                animator.SetTrigger("Jump");
            }
            else if (!isGrounded && !isDashing && dashCooldownTimer <= 0f)
            {
                // Dash through the air
                isDashing = true;
                dashTimer = dashDuration;
                dashDirection = new Vector2(lastDirection, 0f);
                rb.gravityScale = 0f;
                animator.ResetTrigger("Dash");
                animator.SetTrigger("Dash");
            }
        }

        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false; // finish the dash
                dashCooldownTimer = dashCooldown;
                rb.gravityScale = gravityScale;

                dashExitTimer = dashDuration * (dashSpeed / moveSpeed);
                dashExitStartVelocity = dashDirection * dashSpeed;
            }
            else
                velocity = dashDirection * dashSpeed;
        }

        // This block is basically for the smooth dash stop
        if (dashExitTimer > 0f)
        {
            dashExitTimer -= Time.fixedDeltaTime;
            float t = dashExitTimer / (dashDuration * (dashSpeed / moveSpeed));
            velocity.x = Mathf.Lerp(moveInputX * moveSpeed, dashExitStartVelocity.x, t * t);
        }

        jumpQueued = false;
        if (!isDashing)
            animator.ResetTrigger("Dash");
        rb.linearVelocity = velocity;
    }

    private void AlignWithSurface()
    {
        Vector2 rayOrigin = new Vector2(col.bounds.center.x, col.bounds.min.y + 0.05f); // bottom of collider

        RaycastHit2D hit = Physics2D.Raycast(
            rayOrigin,
            Vector2.down,
            slopeCheckDistance,
            groundLayer
        );
        slopeNormal = hit.collider != null ? hit.normal : Vector2.up; // perpendicular

        if (hit.collider != null)
        {
            float angle = Mathf.Atan2(slopeNormal.y, slopeNormal.x) * Mathf.Rad2Deg - 90f; // angle of rotation

            float smoothAngle = Mathf.LerpAngle(
                transform.eulerAngles.z,
                angle,
                rotationSpeed * Time.fixedDeltaTime
            );
            transform.rotation = Quaternion.Euler(0, 0, smoothAngle);
        }
        else
        {
            float smoothAngle = Mathf.LerpAngle(
                transform.eulerAngles.z,
                0f,
                rotationSpeed * Time.fixedDeltaTime
            );
            transform.rotation = Quaternion.Euler(0, 0, smoothAngle);
        }
    }

    private bool IsWall(RaycastHit2D hit) => hit.collider && Mathf.Abs(hit.normal.y) < 0.5f; // if the wall is mostly pointing sideways

    // This method checks whether or not the player is touching a wall on either side
    private bool CheckWall(Vector2 direction)
    {
        Bounds bounds = col.bounds;
        float inset = bounds.extents.y * 0.1f;

        /* One raycast could work fundamentally, but using three raycasts is more precise.
         * This is because a single ray from the center can miss a wall if the player only partially overlaps it (for example, at a ledge corner).
         */
        Vector2 top = new Vector2(bounds.center.x, bounds.max.y - inset);
        Vector2 center = new Vector2(bounds.center.x, bounds.center.y);
        Vector2 bottom = new Vector2(bounds.center.x, bounds.min.y + inset);

        float dist = bounds.extents.x + wallCheckDistance;

        return IsWall(Physics2D.Raycast(top, direction, dist, groundLayer))
            || IsWall(Physics2D.Raycast(center, direction, dist, groundLayer))
            || IsWall(Physics2D.Raycast(bottom, direction, dist, groundLayer));
    }
}
