using UnityEngine;

public class GoldPickup : MonoBehaviour
{
    public int amount = 1;

    [Header("Sound")]
    public AudioClip collectSound;
    [Range(0f, 1f)] public float soundVolume = 0.7f;

    private bool collected;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
            return;

        PlayerStats player = other.GetComponentInParent<PlayerStats>();

        if (player == null || player.isDead)
            return;

        collected = true;
        GoldUI.gold += amount;

        PlayCollectSound();

        Destroy(gameObject);
    }

    void PlayCollectSound()
    {
        if (collectSound == null)
            return;

        // This separate object lets the sound finish after the coin is destroyed.
        GameObject soundObject = new GameObject("Gold Pickup Sound");
        AudioSource source = soundObject.AddComponent<AudioSource>();

        source.clip = collectSound;
        source.volume = soundVolume;
        source.spatialBlend = 0f;
        source.Play();

        Destroy(soundObject, collectSound.length);
    }
}