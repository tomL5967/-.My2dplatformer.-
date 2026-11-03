using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public float speed = 1.5f;

    private Rigidbody2D rb;
    private Rigidbody2D playerRb;
    private int direction = 1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("MovingPlatform needs a Rigidbody2D.");
        }
    }

    private void FixedUpdate()
    {
        if (rb == null || startPoint == null || endPoint == null)
        {
            return;
        }

        Vector2 oldPosition = rb.position;

        Vector2 target = direction == 1
            ? endPoint.position
            : startPoint.position;

        Vector2 newPosition = Vector2.MoveTowards(
            oldPosition,
            target,
            speed * Time.fixedDeltaTime
        );

        Vector2 platformMovement = newPosition - oldPosition;

        rb.MovePosition(newPosition);

        // Carry the player's Rigidbody2D along with the platform.
        if (playerRb != null)
        {
            playerRb.position += platformMovement;
        }

        if (Vector2.Distance(newPosition, target) < 0.01f)
        {
            direction *= -1;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Player playerScript = collision.gameObject.GetComponentInParent<Player>();

        if (playerScript != null)
        {
            playerRb = playerScript.rb;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Player playerScript = collision.gameObject.GetComponentInParent<Player>();

        if (playerScript != null && playerScript.rb == playerRb)
        {
            playerRb = null;
        }
    }

    private void OnDrawGizmos()
    {
        if (startPoint != null && endPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPoint.position, endPoint.position);
        }
    }
}