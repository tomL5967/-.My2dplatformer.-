using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int health;
    public int enemyDamage = 20;
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

    private void Awake()
    {
        player = GetComponent<Player>();

        health = maxHealth;
        stamina = maxStamina;
        isDead = false;
    }

    private void Start()
    {
        if (deathText != null)
        {
            deathText.SetActive(false);
        }

        UpdateBars();
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

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
        if (isDead || stamina < attackCost)
        {
            return false;
        }

        stamina -= attackCost;
        recoveryTime = Time.time + recoveryDelay;

        UpdateBars();
        return true;
    }

    public void TakeDamage(int damage)
    {
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckEnemy(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckEnemy(collision);
    }

    private void CheckEnemy(Collision2D collision)
    {
        EnemyController enemy =
            collision.gameObject.GetComponentInParent<EnemyController>();

        if (enemy != null)
        {
            TakeDamage(enemyDamage);
        }
    }

    private void UpdateBars()
    {
        if (healthBar != null)
        {
            healthBar.minValue = 0;
            healthBar.maxValue = maxHealth;
            healthBar.value = health;
        }

        if (staminaBar != null)
        {
            staminaBar.minValue = 0;
            staminaBar.maxValue = maxStamina;
            staminaBar.value = stamina;
        }
    }

    private void Die()
    {
        isDead = true;

        if (player != null)
        {
            if (player.playerInput != null)
            {
                player.playerInput.DeactivateInput();
            }

            player.moveInput = Vector2.zero;
            player.enabled = false;

            if (player.rb != null)
            {
                player.rb.linearVelocity = Vector2.zero;
                player.rb.simulated = false;
            }

            if (player.animator != null)
            {
                player.animator.enabled = false;
            }
        }

        if (deathText != null)
        {
            deathText.SetActive(true);
        }

        Debug.Log("You died!");
    }
}