using UnityEngine;

public class RuneJumpPad : MonoBehaviour
{
    public float launchForce = 16f;
    public Animator padAnimator;

    private void Awake()
    {
        if (padAnimator == null)
        {
            padAnimator = GetComponentInParent<Animator>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponentInParent<Player>();

        if (player == null || player.rb == null)
        {
            return;
        }

        // Ignore the player if they are moving upwards through the trigger.
        if (player.rb.linearVelocity.y > 0.1f)
        {
            return;
        }

        // Launch the player upwards.
        player.rb.linearVelocity = new Vector2(
            player.rb.linearVelocity.x,
            launchForce
        );

        // Play the pad's activation animation.
        if (padAnimator != null)
        {
            padAnimator.SetTrigger("Activate");
        }
    }
}