using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform player;

    public float sightRange = 8f;
    public float attackRange = 1f;
    public float moveSpeed = 3.5f;
    public float patrolSpeed = 1.5f;
    public float patrolDistance = 2.5f;
    public float attackCooldown = 1.5f;

    private Rigidbody2D rb;
    private float nextAttackTime;
    private float startingX;
    private int patrolDirection = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startingX = rb.position.x;
    }

    void FixedUpdate()
    {
        if (player == null)
        {
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        // Chase the player when they are within sight range.
        if (distance <= sightRange)
        {
            if (distance > attackRange)
            {
                float direction = Mathf.Sign(player.position.x - transform.position.x);
                rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
            }
            else
            {
                // Stop before attacking.
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

                if (Time.time >= nextAttackTime)
                {
                    Debug.Log("The enemy attacks the player!");
                    nextAttackTime = Time.time + attackCooldown;
                }
            }
        }
        else
        {
            // Patrol back and forth until the player is nearby.
            Patrol();
        }
    }

    void Patrol()
    {
        if (rb.position.x >= startingX + patrolDistance)
        {
            patrolDirection = -1;
        }
        else if (rb.position.x <= startingX - patrolDistance)
        {
            patrolDirection = 1;
        }

        rb.linearVelocity = new Vector2(patrolDirection * patrolSpeed, rb.linearVelocity.y);
    }
}   