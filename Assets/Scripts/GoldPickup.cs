using UnityEngine;

public class GoldPickup : MonoBehaviour
{
    public int amount = 1;
    private bool collected;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
        {
            return;
        }

        PlayerStats player = other.GetComponentInParent<PlayerStats>();

        if (player == null || player.isDead)
        {
            return;
        }

        collected = true;
        GoldUI.gold += amount;

        Destroy(gameObject);
    }
}