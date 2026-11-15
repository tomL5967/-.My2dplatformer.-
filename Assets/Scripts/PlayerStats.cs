using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int health;
    public int enemyDamage = 20;
    public float damageDelay = 1f;

    public void HealToFull()
    {
        if (isDead) return;

        health = maxHealth;
        UpdateBars();
    }

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float stamina;
    public float attackCost = 20f;
    public float staminaRecovery = 25f;
    public float recoveryDelay = 1f;

    [Header("Damage Flash")]
    public SpriteRenderer spriteRenderer;
    public float redFlashTime = 0.12f;

    [Header("UI")]
    public Slider healthBar;
    public Slider staminaBar;
    public GameObject deathText;

    public bool isDead;

    private float nextDamageTime;
    private float recoveryTime;
    private Player player;

    private Color originalColor;
    private Coroutine flashRoutine;

    private static int savedHealth = -1;

    private void Awake()
    {
        player = GetComponent<Player>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        health = maxHealth;
        stamina = maxStamina;
        isDead = false;

        if (savedHealth == -1)
        {
            savedHealth = maxHealth;
        }

        health = savedHealth;
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
        if (isDead || damage <= 0 || Time.time < nextDamageTime)
        {
            return;
        }

        health = Mathf.Max(0, health - damage);
        nextDamageTime = Time.time + damageDelay;
        savedHealth = health;

        // Only play effects when damage is accepted.
        if (player != null)
        {
            player.PlayHurtSound();
        }

        FlashRed();
        UpdateBars();

        if (health <= 0)
        {
            Die();
        }
    }

    private void FlashRed()
    {
        if (spriteRenderer == null)
            return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(RedFlash());
    }

    private IEnumerator RedFlash()
    {
        spriteRenderer.color = Color.red;

        // Restores the colour even if the death menu pauses time.
        yield return new WaitForSecondsRealtime(redFlashTime);

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        flashRoutine = null;
    }

    private void OnDisable()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
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

        PlayerRespawn respawn = GetComponent<PlayerRespawn>();

        if (respawn != null)
        {
            respawn.ShowDeathMenu();
        }
    }
}