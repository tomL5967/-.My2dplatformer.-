using UnityEngine;

public class Slime : MonoBehaviour
{
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float patrolSpeed = 2f;
    public float sightRange = 7f;
    public float patrolDistance = 2f;
    public float stopDistance = 0.8f;

    [Header("Health")]
    public int health = 3;

    private Rigidbody2D rb;
    private float startingX;
    private int direction = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Remember where the slime started its patrol.
        startingX = rb.position.x;
    }

    void FixedUpdate()
    {
        float movement = 0f;

        if (player != null &&
            Vector2.Distance(rb.position, player.position) < sightRange)
        {
            // Move towards the player when they are nearby.
            float distanceX = player.position.x - rb.position.x;

            if (Mathf.Abs(distanceX) > stopDistance)
            {
                movement = Mathf.Sign(distanceX) * moveSpeed;
            }
        }
        else
        {
            // Turn around at either end of the patrol.
            if (rb.position.x >= startingX + patrolDistance)
            {
                direction = -1;
            }
            else if (rb.position.x <= startingX - patrolDistance)
            {
                direction = 1;
            }

            movement = direction * patrolSpeed;
        }

        // Keep gravity working while moving sideways.
        rb.linearVelocity = new Vector2(
            movement,
            rb.linearVelocity.y
        );
    }

    public void TakeDamage(int damage)
    {
        health = health - damage;

        // Show the remaining health in the Console.
        Debug.Log("Slime health: " + health);

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}