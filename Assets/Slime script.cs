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

    [Header("Stomp")]
    public float stompBounceForce = 8f;
    public float stompTolerance = 0.2f;

    private Rigidbody2D rb;
    private Collider2D slimeCollider;
    private float startingX;
    private int patrolDirection = 1;

    private float nextJumpTime;
    private float nextAttackTime;
    private bool isDead;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        slimeCollider = GetComponent<Collider2D>();

        if (rb != null)
        {
            startingX = rb.position.x;
        }
    }

    void FixedUpdate()
    {
        if (isDead || rb == null || player == null || groundCheck == null)
        {
            return;
        }

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
                float direction = Mathf.Sign(
                    player.position.x - transform.position.x
                );

                horizontalSpeed = direction * moveSpeed;
            }
            else if (Time.time >= nextAttackTime)
            {
                Debug.Log("The enemy attacks the player!");
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else
        {
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

        rb.linearVelocity = new Vector2(
            horizontalSpeed,
            rb.linearVelocity.y
        );

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

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead || rb == null || slimeCollider == null)
        {
            return;
        }

        Rigidbody2D playerRb = collision.rigidbody;

        if (playerRb == null)
        {
            return;
        }

        bool isPlayer =
            playerRb.gameObject.CompareTag("Player") ||
            collision.gameObject.CompareTag("Player") ||
            collision.transform.root.CompareTag("Player");

        if (!isPlayer)
        {
            return;
        }

        Collider2D playerCollider =
            playerRb.GetComponentInChildren<Collider2D>();

        if (playerCollider == null)
        {
            return;
        }

        bool feetAreAtSlimeTop =
            playerCollider.bounds.min.y >=
            slimeCollider.bounds.max.y - stompTolerance;

        if (feetAreAtSlimeTop)
        {
            isDead = true;

            playerRb.linearVelocity = new Vector2(
                playerRb.linearVelocity.x,
                stompBounceForce
            );

            rb.linearVelocity = Vector2.zero;
            slimeCollider.enabled = false;

            Destroy(gameObject);
        }
    }
}