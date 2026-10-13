using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Rigidbody2D rb;
    public PlayerInput playerInput;
    public Animator animator;

    [Header("Movement")]
    public float speed = 5f;
    public float jumpForce = 8f;
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

        // Idle / Run animation
        animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
    }


    void FixedUpdate()
    {
        // Left and right movement
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
        // Press Space = jump
        if (value.isPressed)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );
        }

        // Let go of Space early = smaller jump
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

        // Face right
        if (moveInput.x > 0.1f)
        {
            scale.x = Mathf.Abs(scale.x);
        }

        // Face left
        else if (moveInput.x < -0.1f)
        {
            scale.x = -Mathf.Abs(scale.x);
        }

        transform.localScale = scale;
    }


    void ApplyGravity()
    {
        // Going upwards
        if (rb.linearVelocity.y > 0.1f)
        {
            rb.gravityScale = jumpGravity;
        }

        // Falling
        else if (rb.linearVelocity.y < -0.1f)
        {
            rb.gravityScale = fallGravity;
        }

        // Standing / top of jump
        else
        {
            rb.gravityScale = normalGravity;
        }
    }


    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
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