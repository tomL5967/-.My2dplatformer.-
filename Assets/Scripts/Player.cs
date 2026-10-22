using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Rigidbody2D rb;
    public PlayerInput playerInput;
    public Animator animator;

    [Header("Movement")]
    public float speed = 3f;
    public float jumpForce = 12f;
    public float jumpCutMultiplier = 0.5f;

    [Header("Gravity")]
    public float normalGravity = 6f;
    public float jumpGravity = 4f;
    public float fallGravity = 12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public Vector2 moveInput;

    private bool isGrounded;

    void Update()
    {
        CheckGrounded();
        Flip();
        ApplyGravity();

        // Tell the Animator whether we are moving sideways.
        animator.SetFloat("Speed", Mathf.Abs(moveInput.x));

        // Positive means rising. Negative means falling.
        animator.SetFloat("YVelocity", rb.linearVelocity.y);

        // Tell the Animator whether we are on the ground.
        animator.SetBool("Grounded", isGrounded);
    }

    void FixedUpdate()
    {
        // Move sideways while keeping our upward or downward speed.
        rb.linearVelocity = new Vector2(
            moveInput.x * speed,
            rb.linearVelocity.y
        );
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        CheckGrounded();

        // Only start a jump when we are on the ground.
        if (value.isPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

            isGrounded = false;

            // Update these immediately so Takeoff does not
            // mistake the start of our jump for a landing.
            animator.SetBool("Grounded", false);
            animator.SetFloat("YVelocity", rb.linearVelocity.y);

            // Start the takeoff animation.
            animator.SetTrigger("Jump");
        }

        // Releasing the button early makes the jump shorter.
        if (!value.isPressed && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y * jumpCutMultiplier
            );
        }
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;

        if (moveInput.x > 0.1f)
        {
            scale.x = Mathf.Abs(scale.x);
        }
        else if (moveInput.x < -0.1f)
        {
            scale.x = -Mathf.Abs(scale.x);
        }

        transform.localScale = scale;
    }

    void ApplyGravity()
    {
        // Going upwards.
        if (rb.linearVelocity.y > 0.1f)
        {
            rb.gravityScale = jumpGravity;
        }

        // Falling.
        else if (rb.linearVelocity.y < -0.1f)
        {
            rb.gravityScale = fallGravity;
        }

        // Standing still or at the top of the jump.
        else
        {
            rb.gravityScale = normalGravity;
        }
    }

    void CheckGrounded()
    {
        // Check for ground under our feet.
        // While rising, we count as airborne even if the
        // small checking circle still overlaps the floor.
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        ) != null && rb.linearVelocity.y <= 0.1f;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.DrawWireSphere(
                groundCheck.position,
                groundCheckRadius
            );
        }
    }
}