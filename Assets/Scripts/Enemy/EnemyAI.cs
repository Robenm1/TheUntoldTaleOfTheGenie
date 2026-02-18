using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private EnemyStats stats;
    private Rigidbody2D rb;
    private EnemyHealth enemyHealth;
    private EnemyCombat enemyCombat;
    private PlayerCombat playerCombat;
    private Collider2D enemyCollider;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float dangerRange = 3f;
    [SerializeField] private float retreatDistance = 7f;

    [Header("Dodge Settings")]
    [SerializeField] private float dodgeChance = 0.8f;
    [SerializeField] private float dodgeCooldown = 0.5f;
    [SerializeField] private float dodgeForce = 15f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 2f;
    [SerializeField] private LayerMask groundLayer;

    private bool isDodging = false;
    private bool isRetreating = false;
    private float lastDodgeTime = -999f;
    private int lastPlayerCombo = -1;
    private bool isGrounded = false;
    private float retreatStartTime = 0f;
    private float minRetreatDuration = 0.4f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyHealth = GetComponent<EnemyHealth>();
        enemyCombat = GetComponent<EnemyCombat>();
        enemyCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        Debug.Log("=== ENEMY AI STARTING ===");

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("✓ Found Player!");
            }
            else
            {
                Debug.LogError("✗ Player NOT found! Check Player tag!");
                return;
            }
        }

        playerCombat = player.GetComponent<PlayerCombat>();
        if (playerCombat != null)
        {
            Debug.Log("✓ Found PlayerCombat component!");
        }

        if (rb != null) Debug.Log("✓ Rigidbody2D assigned");
        if (enemyHealth != null) Debug.Log("✓ EnemyHealth assigned");
        if (enemyCombat != null) Debug.Log("✓ EnemyCombat assigned");

        Debug.Log("=== ENEMY AI READY ===");
    }

    private void Update()
    {
        if (player == null || enemyHealth == null || !enemyHealth.IsAlive()) return;

        CheckGrounded();
        CheckForIncomingAttack();

        if (!isDodging && !IsAttacking())
        {
            HandleMovement();
        }
        else if (IsAttacking())
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private bool IsAttacking()
    {
        return enemyCombat != null && enemyCombat.IsAttacking();
    }

    private void CheckGrounded()
    {
        Vector2 rayOrigin = transform.position;

        RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.down, groundCheckDistance, groundLayer);

        isGrounded = false;
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider != enemyCollider)
            {
                isGrounded = true;
                Debug.DrawLine(rayOrigin, hit.point, Color.green);
                break;
            }
        }

        if (!isGrounded)
        {
            Debug.DrawLine(rayOrigin, rayOrigin + Vector2.down * groundCheckDistance, Color.red);
        }
    }

    private void HandleMovement()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool playerIsAttacking = playerCombat != null && playerCombat.IsAttacking();

        if (playerIsAttacking || (isRetreating && Time.time - retreatStartTime < minRetreatDuration))
        {
            if (!isRetreating)
            {
                isRetreating = true;
                retreatStartTime = Time.time;
            }

            Retreat();
        }
        else
        {
            if (isRetreating)
            {
                isRetreating = false;
            }

            if (distanceToPlayer > detectionRange)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            else if (distanceToPlayer <= attackRange && enemyCombat != null && enemyCombat.CanAttack())
            {
                enemyCombat.StartAttackCombo(player);
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            else if (distanceToPlayer > attackRange)
            {
                ApproachPlayer();
            }
            else
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
    }

    private void Retreat()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float directionFromPlayer = Mathf.Sign(transform.position.x - player.position.x);
        float retreatSpeed = stats != null ? stats.moveSpeed * 2f : 4f;

        if (distanceToPlayer < retreatDistance)
        {
            rb.linearVelocity = new Vector2(directionFromPlayer * retreatSpeed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void ApproachPlayer()
    {
        float directionToPlayer = Mathf.Sign(player.position.x - transform.position.x);
        float moveSpeed = stats != null ? stats.chaseSpeed : 3f;
        rb.linearVelocity = new Vector2(directionToPlayer * moveSpeed, rb.linearVelocity.y);
    }

    private void CheckForIncomingAttack()
    {
        if (playerCombat == null || IsAttacking()) return;

        bool isPlayerAttacking = playerCombat.IsAttacking();
        int currentCombo = playerCombat.GetCurrentCombo();

        if (isPlayerAttacking && currentCombo != lastPlayerCombo)
        {
            lastPlayerCombo = currentCombo;

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer <= dangerRange)
            {
                bool effectivelyGrounded = isGrounded || Mathf.Abs(rb.linearVelocity.y) < 0.1f;
                bool canDodge = effectivelyGrounded && Time.time - lastDodgeTime >= dodgeCooldown;
                bool hasStamina = enemyCombat != null && enemyCombat.CanDodge();

                if (canDodge && hasStamina)
                {
                    if (Random.value <= dodgeChance)
                    {
                        PerformDodge();
                    }
                    else
                    {
                        Debug.Log("<color=orange>Enemy chose not to dodge (random chance)</color>");
                        FallbackRetreat();
                    }
                }
                else
                {
                    if (!hasStamina)
                    {
                        float currentStamina = enemyCombat != null ? enemyCombat.GetCurrentStamina() : 0;
                        float dodgeCost = stats != null ? stats.dodgeStaminaCost : 30f;
                        Debug.Log($"<color=red>OUT OF STAMINA! Need {dodgeCost}, have {currentStamina:F0}. Running away...</color>");
                    }
                    FallbackRetreat();
                }
            }
        }

        if (!isPlayerAttacking)
        {
            lastPlayerCombo = -1;
        }
    }

    private void PerformDodge()
    {
        if (enemyCombat == null || stats == null) return;

        bool consumed = enemyCombat.ConsumeStamina(stats.dodgeStaminaCost);

        if (!consumed)
        {
            Debug.Log("<color=red>Failed to consume stamina for dodge! Falling back to retreat.</color>");
            FallbackRetreat();
            return;
        }

        isDodging = true;
        lastDodgeTime = Time.time;
        isRetreating = true;
        retreatStartTime = Time.time;

        float directionFromPlayer = Mathf.Sign(transform.position.x - player.position.x);
        rb.linearVelocity = new Vector2(directionFromPlayer * dodgeForce, rb.linearVelocity.y);

        Debug.Log($"<color=cyan>### ENEMY DODGED! Consumed {stats.dodgeStaminaCost} stamina. Remaining: {enemyCombat.GetCurrentStamina():F0}/{enemyCombat.GetMaxStamina()} ###</color>");

        Invoke(nameof(EndDodge), 0.2f);
    }

    private void FallbackRetreat()
    {
        isRetreating = true;
        retreatStartTime = Time.time;

        float directionFromPlayer = Mathf.Sign(transform.position.x - player.position.x);
        float normalRetreatSpeed = stats != null ? stats.moveSpeed : 2f;

        rb.linearVelocity = new Vector2(directionFromPlayer * normalRetreatSpeed, rb.linearVelocity.y);

        Debug.Log("<color=yellow>Enemy retreating with normal speed (no stamina for dodge)</color>");
    }

    private void EndDodge()
    {
        isDodging = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dangerRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, retreatDistance);

        if (Application.isPlaying)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
        }
    }
}
