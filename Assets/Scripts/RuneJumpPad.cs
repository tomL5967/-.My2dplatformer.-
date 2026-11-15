using UnityEngine;

public class RuneJumpPad : MonoBehaviour
{
    public float launchForce = 16f;
    public Animator padAnimator;
    public AudioClip bounceSound;
    [Range(0f, 1f)] public float soundVolume = 1f;

    private void Awake()
    {
        if (padAnimator == null)
            padAnimator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponentInParent<Player>();

        if (player == null || player.rb == null)
            return;

        // Don't activate when the player is moving upward through the pad.
        if (player.rb.linearVelocity.y > 0.1f)
            return;

        player.rb.linearVelocity = new Vector2(
            player.rb.linearVelocity.x,
            launchForce
        );

        if (padAnimator != null && HasActivateTrigger())
            padAnimator.SetTrigger("Activate");

        if (bounceSound != null)
            AudioSource.PlayClipAtPoint(
                bounceSound,
                transform.position,
                soundVolume
            );
    }

    private bool HasActivateTrigger()
    {
        foreach (AnimatorControllerParameter parameter in padAnimator.parameters)
        {
            if (parameter.name == "Activate" &&
                parameter.type == AnimatorControllerParameterType.Trigger)
                return true;
        }

        return false;
    }
}