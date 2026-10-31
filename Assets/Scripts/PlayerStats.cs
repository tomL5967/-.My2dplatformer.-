using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int health;
    public int slimeDamage = 20;
    public float damageDelay = 1f;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float stamina;
    public float attackCost = 20f;
    public float staminaRecovery = 25f;
    public float recoveryDelay = 1f;

    [Header("UI")]
    public Slider healthBar;
    public Slider staminaBar;
    public GameObject deathText;

    public bool isDead;

    private float nextDamageTime;
    private float recoveryTime;
    private Player player;

    void Awake()
    {
        player = GetComponent<Player>();

        // Start the game with full health and stamina.
        health = maxHealth;
        stamina = maxStamina;
        isDead = false;
    }

    void Start()
    {
        // Hide the death message while we are alive.
        if (deathText != null)
        {
            deathText.SetActive(false);
        }

        UpdateBars();
    }

    void Update()
    {
        if (isDead)
        {
            return;
        }

        // Recover stamina after a short pause.
        if (Time.time >= recoveryTime)
        {
            stamina = Mathf.Min(
                maxStamina,
                stamina + staminaRecovery * Time.deltaTime
            );
        }

        UpdateBars();
    }

    public bool UseAttackStamina()
    {
        // Do not attack if we are dead or too tired.
        if (isDead || stamina < attackCost)
        {
            return false;
        }

        stamina = stamina - attackCost;

        // Restart the wait before stamina begins refilling.
        recoveryTime = Time.time + recoveryDelay;

        UpdateBars();
        return true;
    }

    public void TakeDamage(int damage)
    {
        // Allow a short break between hits.
        // Otherwise, touching a slime would hurt every frame.
        if (isDead || Time.time < nextDamageTime)
        {
            return;
        }

        health = Mathf.Max(0, health - damage);
        nextDamageTime = Time.time + damageDelay;

        UpdateBars();

        if (health <= 0)
        {
            Die();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        CheckSlime(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        CheckSlime(collision);
    }

    void CheckSlime(Collision2D collision)
    {
        // Check whether the object touching us is a slime.
        Slime slime = collision.gameObject.GetComponentInParent<Slime>();

        if (slime != null)
        {
            TakeDamage(slimeDamage);
        }
    }

    void UpdateBars()
    {
        // Make the health bar match our health.
        if (healthBar != null)
        {
            healthBar.minValue = 0;
            healthBar.maxValue = maxHealth;
            healthBar.value = health;
        }

        // Make the stamina bar match our stamina.
        if (staminaBar != null)
        {
            staminaBar.minValue = 0;
            staminaBar.maxValue = maxStamina;
            staminaBar.value = stamina;
        }
    }

    void Die()
    {
        isDead = true;

        // Turn off the player's controls.
        player.playerInput.DeactivateInput();
        player.moveInput = Vector2.zero;
        player.enabled = false;

        // Freeze the player and stop their animation.
        player.rb.linearVelocity = Vector2.zero;
        player.rb.simulated = false;
        player.animator.enabled = false;

        if (deathText != null)
        {
            deathText.SetActive(true);
        }

        Debug.Log("You died!");
    }
}