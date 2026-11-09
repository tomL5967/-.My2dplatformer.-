using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Rigidbody2D rb;
    public PlayerInput playerInput;
    public Animator animator;

    [Header("Movement")]
    public float speed = 3f;
    public float jumpForce = 12f;
    public float jumpCutMultiplier = 0.5f;

    [Header("Run Boost")]
    public float timeToRunBoost = 3f;
    public float runSpeedMultiplier = 1.1f;
    public ParticleSystem speedEffect;

    [Header("Jump Combo")]
    public int jumpsForBoost = 3;
    public float jumpComboResetTime = 1.5f;
    public float jumpHeightMultiplier = 1.07f;
    public float jumpComboSpeedMultiplier = 1.15f;
    public float jumpComboSpeedDuration = 1.5f;
    public ParticleSystem jumpBoostEffect;

    [Header("Attack Combo")]
    [Range(0f, 1f)]
    public float comboInputStart = 0.35f;

    [Header("Gravity")]
    public float normalGravity = 6f;
    public float jumpGravity = 4f;
    public float fallGravity = 12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Sword")]
    public Transform attackPoint;
    public float attackRadius = 0.6f;
    public int attackDamage = 1;
    public LayerMask enemyLayer;

    public Vector2 moveInput;

    private bool isGrounded;
    private bool secondAttackQueued;
    private PlayerStats stats;

    private float continuousRunTime;
    private float lastRunDirection;
    private int consecutiveJumps;
    private float lastJumpTime = -999f;
    private float jumpSpeedBoostUntil;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();
    }

    void Update()
    {
        if (stats != null && stats.isDead)
            return;

        CheckGrounded();
        UpdateRunTime();
        UpdateSpeedEffect();
        Flip();
        ApplyGravity();

        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
            animator.SetFloat("YVelocity", rb.linearVelocity.y);
            animator.SetBool("Grounded", isGrounded);
        }
    }

    void FixedUpdate()
    {
        if (stats != null && stats.isDead)
            return;

        float currentSpeed = speed;

        if (Mathf.Abs(moveInput.x) > 0.1f &&
            continuousRunTime >= timeToRunBoost)
        {
            currentSpeed *= runSpeedMultiplier;
        }

        if (Time.time < jumpSpeedBoostUntil)
        {
            currentSpeed *= jumpComboSpeedMultiplier;
        }

        rb.linearVelocity = new Vector2(
            moveInput.x * currentSpeed,
            rb.linearVelocity.y
        );
    }

    void UpdateRunTime()
    {
        if (Mathf.Abs(moveInput.x) <= 0.1f)
        {
            continuousRunTime = 0f;
            lastRunDirection = 0f;
            return;
        }

        float currentDirection = Mathf.Sign(moveInput.x);

        if (lastRunDirection != 0f &&
            currentDirection != lastRunDirection)
        {
            continuousRunTime = 0f;
        }

        continuousRunTime += Time.deltaTime;
        lastRunDirection = currentDirection;
    }

    void UpdateSpeedEffect()
    {
        if (speedEffect == null)
            return;

        bool hasRunBoost =
            Mathf.Abs(moveInput.x) > 0.1f &&
            continuousRunTime >= timeToRunBoost;

        bool hasJumpSpeedBoost = Time.time < jumpSpeedBoostUntil;
        bool shouldPlay = hasRunBoost || hasJumpSpeedBoost;

        if (shouldPlay && !speedEffect.isPlaying)
            speedEffect.Play();
        else if (!shouldPlay && speedEffect.isPlaying)
            speedEffect.Stop();
    }

    public void OnMove(InputValue value)
    {
        if (stats != null && stats.isDead)
            return;

        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (stats != null && stats.isDead)
            return;

        CheckGrounded();

        if (value.isPressed && isGrounded)
        {
            if (Time.time - lastJumpTime > jumpComboResetTime)
                consecutiveJumps = 0;

            consecutiveJumps++;
            lastJumpTime = Time.time;

            float currentJumpForce = jumpForce;

            if (consecutiveJumps >= jumpsForBoost)
            {
                currentJumpForce *= jumpHeightMultiplier;
                jumpSpeedBoostUntil =
                    Time.time + jumpComboSpeedDuration;

                consecutiveJumps = 0;

                if (jumpBoostEffect != null)
                    jumpBoostEffect.Play();
            }

            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                currentJumpForce
            );

            isGrounded = false;

            if (animator != null)
            {
                animator.SetBool("Grounded", false);
                animator.SetFloat("YVelocity", rb.linearVelocity.y);
                animator.SetTrigger("Jump");
            }
        }

        if (!value.isPressed && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y * jumpCutMultiplier
            );
        }
    }

    public void OnAttack(InputValue value)
    {
        if (!value.isPressed ||
            (stats != null && stats.isDead) ||
            !isGrounded ||
            animator == null)
        {
            return;
        }

        AnimatorStateInfo state =
            animator.GetCurrentAnimatorStateInfo(0);

        if (state.IsName("Attack1"))
        {
            if (!secondAttackQueued &&
                state.normalizedTime >= comboInputStart &&
                (stats == null || stats.UseAttackStamina()))
            {
                animator.SetTrigger("Attack2");
                secondAttackQueued = true;
            }

            return;
        }

        if (state.IsName("Attack2"))
            return;

        if (stats != null && !stats.UseAttackStamina())
            return;

        secondAttackQueued = false;
        animator.SetTrigger("Attack1");
    }

    public void DealDamage()
    {
        // Add this as an Animation Event on the hit frame
        // of both attack animations.
        if ((stats != null && stats.isDead) || attackPoint == null)
            return;

        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRadius,
            enemyLayer
        );

        // Prevent one attack hitting the same enemy twice
        // if it has multiple colliders.
        HashSet<EnemyController> hitEnemies =
            new HashSet<EnemyController>();

        foreach (Collider2D enemyCollider in enemies)
        {
            EnemyController enemy =
                enemyCollider.GetComponentInParent<EnemyController>();

            if (enemy != null && hitEnemies.Add(enemy))
                enemy.TakeDamage(attackDamage);
        }
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;

        if (moveInput.x > 0.1f)
            scale.x = Mathf.Abs(scale.x);
        else if (moveInput.x < -0.1f)
            scale.x = -Mathf.Abs(scale.x);

        transform.localScale = scale;
    }

    void ApplyGravity()
    {
        if (rb.linearVelocity.y > 0.1f)
            rb.gravityScale = jumpGravity;
        else if (rb.linearVelocity.y < -0.1f)
            rb.gravityScale = fallGravity;
        else
            rb.gravityScale = normalGravity;
    }

    void CheckGrounded()
    {
        if (groundCheck == null || rb == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded =
            Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            ) != null &&
            rb.linearVelocity.y <= 0.1f;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(
                groundCheck.position,
                groundCheckRadius
            );
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(
                attackPoint.position,
                attackRadius
            );
        }
    }
}