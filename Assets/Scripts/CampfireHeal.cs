using UnityEngine;

public class CampfireHeal : MonoBehaviour
{
    public AudioClip healSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerStats stats = other.GetComponentInParent<PlayerStats>();

        if (stats == null || stats.isDead || stats.health >= stats.maxHealth)
            return;

        stats.HealToFull();

        if (healSound != null)
            AudioSource.PlayClipAtPoint(healSound, transform.position);
    }
}