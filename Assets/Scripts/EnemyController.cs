using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum MovementStyle
    {
        PatrolAndChase,
        PatrolOnly,
        ChaseOnly,
        Stationary
    }

    [Header("Enemy Setup")]
    public MovementStyle movementStyle = MovementStyle.PatrolAndChase;
    public Transform player;
    public int maxHealth = 3;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float detectionRange = 5f;
    public float patrolDistance = 2.5f;

    [Header("Hopping")]
    public bool canHop = true;
    public Transform groundCheck;
    public float groundCheckRadius = 0.12f;
    public LayerMask groundLayer;
    public float jumpForce = 4f;
    public float jumpDelay = 1f;

    [Header("Damage")]
    public float knockbackForce = 3f;
    public float knockbackUpForce = 2f;
    public float knockbackTime = 0.2f;
    public float redFlashTime = 0.12f;

    [Header("Sprite")]
    public SpriteRenderer spriteRenderer;
    public bool spriteFacesRight = true;

    [Header("Animation")]
    public Animator animator;

    private Rigidbody2D rb;
    private int currentHealth;
    private float startingX;
    private float nextJumpTime;
    private float stunnedUntil;
    private int patrolDirection = 1;
    private Color originalColor;
    private Coroutine flashRoutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        currentHealth = maxHealth;
        startingX = transform.position.x;
        nextJumpTime = Time.time + jumpDelay;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    void FixedUpdate()
    {
        if (rb == null || currentHealth <= 0)
            return;

        bool grounded = IsGrounded();
        SetBoolIfPresent("Grounded", grounded);

        if (Time.time < stunnedUntil)
        {
            SetFloatIfPresent("Speed", Mathf.Abs(rb.linearVelocity.x));
            return;
        }

        float direction = GetMoveDirection();

        rb.linearVelocity = new Vector2(
            direction * moveSpeed,
            rb.linearVelocity.y
        );

        SetFloatIfPresent("Speed", Mathf.Abs(direction * moveSpeed));
        FaceDirection(direction);

        if (canHop &&
            grounded &&
            rb.linearVelocity.y <= 0.1f &&
            Time.time >= nextJumpTime)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

            nextJumpTime = Time.time + jumpDelay;
        }
    }

    float GetMoveDirection()
    {
        if (movementStyle == MovementStyle.Stationary)
            return 0f;

        float distanceToPlayer = float.MaxValue;

        if (player != null)
        {
            distanceToPlayer = Mathf.Abs(
                player.position.x - transform.position.x
            );
        }

        bool shouldChase =
            player != null &&
            distanceToPlayer <= detectionRange &&
            movementStyle != MovementStyle.PatrolOnly;

        if (shouldChase)
        {
            float difference = player.position.x - transform.position.x;

            if (Mathf.Abs(difference) < 0.1f)
                return 0f;

            return Mathf.Sign(difference);
        }

        if (movementStyle == MovementStyle.ChaseOnly)
            return 0f;

        if (transform.position.x >= startingX + patrolDistance)
            patrolDirection = -1;
        else if (transform.position.x <= startingX - patrolDistance)
            patrolDirection = 1;

        return patrolDirection;
    }

    bool IsGrounded()
    {
        if (groundCheck == null)
            return false;

        return Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        ) != null;
    }

    void FaceDirection(float direction)
    {
        if (spriteRenderer == null || Mathf.Abs(direction) < 0.01f)
            return;

        // Set this based on which way the original sprite faces.
        spriteRenderer.flipX = (direction < 0f) == spriteFacesRight;
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0)
            return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
            return;
        }

        SetTriggerIfPresent("Hurt");
        FlashRed();

        float knockbackDirection = 1f;

        if (player != null)
        {
            knockbackDirection = Mathf.Sign(
                transform.position.x - player.position.x
            );
        }

        rb.linearVelocity = new Vector2(
            knockbackDirection * knockbackForce,
            knockbackUpForce
        );

        stunnedUntil = Time.time + knockbackTime;
    }

    void FlashRed()
    {
        if (spriteRenderer == null)
            return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(RedFlash());
    }

    IEnumerator RedFlash()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(redFlashTime);

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        flashRoutine = null;
    }

    bool HasParameter(string parameterName, AnimatorControllerParameterType type)
    {
        if (animator == null)
            return false;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == type)
                return true;
        }

        return false;
    }

    void SetBoolIfPresent(string parameterName, bool value)
    {
        if (HasParameter(parameterName, AnimatorControllerParameterType.Bool))
            animator.SetBool(parameterName, value);
    }

    void SetFloatIfPresent(string parameterName, float value)
    {
        if (HasParameter(parameterName, AnimatorControllerParameterType.Float))
            animator.SetFloat(parameterName, value);
    }

    void SetTriggerIfPresent(string parameterName)
    {
        if (HasParameter(parameterName, AnimatorControllerParameterType.Trigger))
            animator.SetTrigger(parameterName);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}