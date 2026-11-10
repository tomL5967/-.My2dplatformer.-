using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    public Vector3 respawnPoint;
    public GameObject deathMenu;

    private Player player;
    private PlayerStats stats;

    void Awake()
    {
        player = GetComponent<Player>();
        stats = GetComponent<PlayerStats>();

        // If no campfire sets a checkpoint, respawn where the player started.
        respawnPoint = transform.position;

        if (deathMenu != null)
            deathMenu.SetActive(false);
    }

    public void SetRespawnPoint(Vector3 newPoint)
    {
        respawnPoint = newPoint;
    }

    public void ShowDeathMenu()
    {
        if (deathMenu != null)
            deathMenu.SetActive(true);

        Time.timeScale = 0f;
    }

    public void RespawnNow()
    {
        Time.timeScale = 1f;

        transform.position = respawnPoint;

        player.rb.simulated = true;
        player.rb.linearVelocity = Vector2.zero;
        player.animator.enabled = true;
        player.enabled = true;
        player.playerInput.ActivateInput();

        stats.health = stats.maxHealth;
        stats.isDead = false;

        if (deathMenu != null)
            deathMenu.SetActive(false);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}