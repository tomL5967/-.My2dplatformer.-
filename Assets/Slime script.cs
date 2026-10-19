using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform player;

    public float sightRange = 8f;
    public float attackRange = 1f;

    public float moveSpeed = 5f;
    public float patrolSpeed = 2.5f;
    public float patrolDistance = 2.5f;

    public float jumpForce = 4f;
    public float jumpDelay = 1f;

    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    public float attackCooldown = 1.5f;

    private Rigidbody2D rb;
    private float startingX;
    private int patrolDirection = 1;

    private float nextJumpTime;
    private float nextAttackTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Remember where the slime started its patrol.
        startingX = rb.position.x;
    }

    void FixedUpdate()
    {
        if (player == null || groundCheck == null)
        {
            return;
        }

        // Check for ground just underneath the slime.
        bool grounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        ) != null;

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        float horizontalSpeed = 0f;

        if (distance <= sightRange)
        {
            if (distance > attackRange)
            {
                // Move left or right towards the player.
                float direction = Mathf.Sign(
                    player.position.x - transform.position.x
                );

                horizontalSpeed = direction * moveSpeed;
            }
            else
            {
                // Stop moving sideways when close enough to attack.
                if (Time.time >= nextAttackTime)
                {
                    Debug.Log("The enemy attacks the player!");
                    nextAttackTime = Time.time + attackCooldown;
                }
            }
        }
        else
        {
            // Turn around at either end of the patrol.
            if (rb.position.x >= startingX + patrolDistance)
            {
                patrolDirection = -1;
            }
            else if (rb.position.x <= startingX - patrolDistance)
            {
                patrolDirection = 1;
            }

            horizontalSpeed = patrolDirection * patrolSpeed;
        }

        // Change sideways movement while keeping gravity working.
        rb.linearVelocity = new Vector2(
            horizontalSpeed,
            rb.linearVelocity.y
        );

        // Only hop while moving, touching the ground and ready.
        if (grounded &&
            horizontalSpeed != 0f &&
            rb.linearVelocity.y <= 0f &&
            Time.time >= nextJumpTime)
        {
            rb.linearVelocity = new Vector2(
                horizontalSpeed,
                jumpForce
            );

            nextJumpTime = Time.time + jumpDelay;
        }
    }
}