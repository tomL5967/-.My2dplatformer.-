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

    [Header("Sword")]
    public Transform attackPoint;
    public float attackRadius = 0.6f;
    public int attackDamage = 1;
    public LayerMask enemyLayer;

    public Vector2 moveInput;

    private bool isGrounded;
    private bool attackRequested;
    private PlayerStats stats;

    void Awake()
    {
        // Find the health and stamina script on this player.
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (stats.isDead)
        {
            return;
        }

        CheckGrounded();
        Flip();
        ApplyGravity();

        // Tell the Animator how we are moving.
        animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
        animator.SetFloat("YVelocity", rb.linearVelocity.y);
        animator.SetBool("Grounded", isGrounded);

        // Once the Animator has used the attack trigger,
        // we no longer have an attack waiting to start.
        if (attackRequested && !animator.GetBool("Attack"))
        {
            attackRequested = false;
        }
    }

    void FixedUpdate()
    {
        if (stats.isDead)
        {
            return;
        }

        // Move sideways without changing our vertical speed.
        rb.linearVelocity = new Vector2(
            moveInput.x * speed,
            rb.linearVelocity.y
        );
    }

    public void OnMove(InputValue value)
    {
        if (stats.isDead)
        {
            return;
        }

        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (stats.isDead)
        {
            return;
        }

        CheckGrounded();

        // Only start a jump while touching the ground.
        if (value.isPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

            isGrounded = false;

            // Tell the Animator immediately that we have jumped.
            animator.SetBool("Grounded", false);
            animator.SetFloat("YVelocity", rb.linearVelocity.y);
            animator.SetTrigger("Jump");
        }

        // Letting go early makes the jump shorter.
        if (!value.isPressed && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y * jumpCutMultiplier
            );
        }
    }

    public void OnAttack(InputValue value)
    {
        // Ignore releases, death and an attack already requested.
        if (!value.isPressed || stats.isDead || attackRequested)
        {
            return;
        }

        AnimatorStateInfo state =
            animator.GetCurrentAnimatorStateInfo(0);

        // Start attacks from idle or running while grounded.
        if (isGrounded &&
            !animator.IsInTransition(0) &&
            (state.IsName("idle") || state.IsName("Run")))
        {
            // Only swing if we have enough stamina.
            if (stats.UseAttackStamina())
            {
                attackRequested = true;
                animator.SetTrigger("Attack");
            }
        }
    }

    public void DealDamage()
    {
        // This method is called by the Attack animation event.
        if (stats.isDead || attackPoint == null)
        {
            return;
        }

        // Find enemy colliders inside the sword's hit area.
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRadius,
            enemyLayer
        );

        foreach (Collider2D enemy in enemies)
        {
            Slime slime = enemy.GetComponent<Slime>();

            if (slime != null)
            {
                slime.TakeDamage(attackDamage);
            }
        }
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;

        // Face right.
        if (moveInput.x > 0.1f)
        {
            scale.x = Mathf.Abs(scale.x);
        }

        // Face left.
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
        // Check for ground beneath our feet.
        // While rising, count as airborne.
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        ) != null && rb.linearVelocity.y <= 0.1f;
    }

    private void OnDrawGizmosSelected()
    {
        // Show the ground-check circle.
        if (groundCheck != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(
                groundCheck.position,
                groundCheckRadius
            );
        }

        // Show the sword's damage area.
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(
                attackPoint.position,
                attackRadius
            );
        }
    }
}