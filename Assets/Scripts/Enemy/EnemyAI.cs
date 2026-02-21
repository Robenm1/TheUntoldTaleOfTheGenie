using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private EnemyStats stats;
    private Rigidbody2D rb;
    private EnemyHealth enemyHealth;
    private EnemyCombat enemyCombat;
    private PlayerCombat playerHeavyAttack;
    private PlayerLightAttack playerLightAttack;
    private Collider2D enemyCollider;
    private GenieAbility1 genieAbility;
    private GenieAbility2 genieAbility2;
    private StunEffect stunEffect;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float dangerRange = 3f;
    [SerializeField] private float retreatDistance = 7f;

    [Header("Dodge Settings")]
    [SerializeField] private float dodgeChance = 0.8f;
    [SerializeField] private float dodgeCooldown = 0.5f;
    [SerializeField] private float dodgeForce = 15f;

    [Header("Through-Dodge Settings")]
    [SerializeField] private float throughDodgeDistance = 5f;
    [SerializeField] private float throughDodgeDuration = 0.4f;

    [Header("Wall Detection")]
    [SerializeField] private float wallCheckDistance = 2f;
    [SerializeField] private LayerMask wallLayer;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Ability Avoidance")]
    [SerializeField] private float abilityFleeSpeed = 7f;
    [SerializeField] private float abilityFleeDistance = 8f;
    [SerializeField] private float fleeDirectionChangeInterval = 0.5f;

    [Header("Stamina Flee Settings")]
    [SerializeField] private float staminaFleeSpeed = 5f;
    [SerializeField] private float staminaFleeDistance = 10f;
    [SerializeField] private float staminaFleeThreshold = 20f;
    [SerializeField] private float staminaRecoveryThreshold = 0.5f;

    private bool isDodging = false;
    private bool isRetreating = false;
    private bool isDamageImmune = false;
    private float lastDodgeTime = -999f;
    private int lastPlayerHeavyCombo = -1;
    private int lastPlayerLightCombo = -1;
    private bool isGrounded = false;
    private float retreatStartTime = 0f;
    private float minRetreatDuration = 0.4f;
    private bool isThroughDodging = false;
    private bool isFleeingFromAbility = false;
    private float lastFleeDirectionChange = 0f;
    private float currentFleeDirection = 1f;
    private bool isFleeingFromStaminaLoss = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyHealth = GetComponent<EnemyHealth>();
        enemyCombat = GetComponent<EnemyCombat>();
        enemyCollider = GetComponent<Collider2D>();
        stunEffect = GetComponent<StunEffect>();
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

        playerHeavyAttack = player.GetComponent<PlayerCombat>();
        playerLightAttack = player.GetComponent<PlayerLightAttack>();
        genieAbility = player.GetComponent<GenieAbility1>();
        genieAbility2 = player.GetComponent<GenieAbility2>();

        if (playerHeavyAttack != null)
        {
            Debug.Log("✓ Found PlayerCombat component!");
        }

        if (playerLightAttack != null)
        {
            Debug.Log("✓ Found PlayerLightAttack component!");
        }

        if (genieAbility != null)
        {
            Debug.Log("✓ Found GenieAbility1 component!");
        }

        if (genieAbility2 != null)
        {
            Debug.Log("✓ Found GenieAbility2 component!");
        }

        if (rb != null) Debug.Log("✓ Rigidbody2D assigned");
        if (enemyHealth != null) Debug.Log("✓ EnemyHealth assigned");
        if (enemyCombat != null) Debug.Log("✓ EnemyCombat assigned");

        Debug.Log("=== ENEMY AI READY ===");
    }

    private void Update()
    {
        if (!enabled) return;

        if (player == null || enemyHealth == null || !enemyHealth.IsAlive()) return;

        if (stunEffect != null && stunEffect.IsStunned())
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        CheckStaminaState();

        CheckGrounded();

        if (!isThroughDodging)
        {
            CheckForIncomingAttacks();
        }

        if (!isDodging && !IsAttacking() && !isThroughDodging)
        {
            HandleMovement();
        }
        else if (IsAttacking())
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void CheckStaminaState()
    {
        if (enemyCombat == null) return;

        float currentStamina = enemyCombat.GetCurrentStamina();
        float maxStamina = enemyCombat.GetMaxStamina();
        float staminaPercentage = currentStamina / maxStamina;

        if (currentStamina <= staminaFleeThreshold && !isFleeingFromStaminaLoss)
        {
            isFleeingFromStaminaLoss = true;
            Debug.Log($"<color=red>### STAMINA LOW ({currentStamina:F0}/{maxStamina:F0})! FLEEING FROM PLAYER! ###</color>");
        }
        else if (isFleeingFromStaminaLoss && staminaPercentage >= staminaRecoveryThreshold)
        {
            isFleeingFromStaminaLoss = false;
            Debug.Log($"<color=green>### STAMINA RECOVERED ({currentStamina:F0}/{maxStamina:F0} = {staminaPercentage * 100:F0}%)! RESUMING NORMAL BEHAVIOR! ###</color>");
        }
    }

    public void InterruptAllActions()
    {
        StopAllCoroutines();
        CancelInvoke();

        isDodging = false;
        isThroughDodging = false;
        isDamageImmune = false;
        isRetreating = false;
        isFleeingFromAbility = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;

            if (rb.bodyType == RigidbodyType2D.Kinematic)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
        }

        if (player != null)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            if (playerCollider != null && enemyCollider != null)
            {
                Physics2D.IgnoreCollision(enemyCollider, playerCollider, false);
            }
        }

        Debug.Log("<color=red>### ALL ENEMY ACTIONS INTERRUPTED BY STUN! ###</color>");
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

    private bool IsBlockedByWallBehind()
    {
        if (player == null) return false;

        float directionAwayFromPlayer = Mathf.Sign(transform.position.x - player.position.x);
        Vector2 rayOrigin = transform.position;
        Vector2 rayDirection = directionAwayFromPlayer > 0 ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, wallCheckDistance, wallLayer);

        if (hit.collider != null)
        {
            Debug.DrawLine(rayOrigin, hit.point, Color.red);
            return true;
        }

        Debug.DrawLine(rayOrigin, rayOrigin + rayDirection * wallCheckDistance, Color.green);
        return false;
    }

    private void HandleMovement()
    {
        if (stunEffect != null && stunEffect.IsStunned())
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isFleeingFromStaminaLoss)
        {
            FleeFromStaminaLoss();
            return;
        }

        if (genieAbility2 != null && genieAbility2.IsBarrierActive())
        {
            WalkAwayFromBarrier();
            return;
        }

        if (genieAbility != null && genieAbility.IsCasting())
        {
            FleeFromAbility();
            return;
        }

        if (isFleeingFromAbility)
        {
            isFleeingFromAbility = false;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool playerIsAttacking = IsPlayerAttacking();

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

    private void FleeFromStaminaLoss()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float directionFromPlayer = Mathf.Sign(transform.position.x - player.position.x);

        if (distanceToPlayer < staminaFleeDistance)
        {
            rb.linearVelocity = new Vector2(directionFromPlayer * staminaFleeSpeed, rb.linearVelocity.y);
            Debug.DrawLine(transform.position, transform.position + Vector3.right * directionFromPlayer * 3f, Color.red);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void WalkAwayFromBarrier()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float directionFromPlayer = Mathf.Sign(transform.position.x - player.position.x);
        float walkSpeed = stats != null ? stats.moveSpeed : 2f;

        if (distanceToPlayer < retreatDistance)
        {
            rb.linearVelocity = new Vector2(directionFromPlayer * walkSpeed, rb.linearVelocity.y);
            Debug.DrawLine(transform.position, transform.position + Vector3.right * directionFromPlayer * 2f, Color.cyan);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void FleeFromAbility()
    {
        if (!isFleeingFromAbility)
        {
            isFleeingFromAbility = true;
            currentFleeDirection = Random.value > 0.5f ? 1f : -1f;
            lastFleeDirectionChange = Time.time;
            Debug.Log("<color=yellow>Enemy detected ability cast! Fleeing!</color>");
        }

        if (Time.time - lastFleeDirectionChange >= fleeDirectionChangeInterval)
        {
            currentFleeDirection *= -1f;
            lastFleeDirectionChange = Time.time;
            Debug.Log($"<color=yellow>Enemy changed flee direction! Now going {(currentFleeDirection > 0 ? "RIGHT" : "LEFT")}</color>");
        }

        rb.linearVelocity = new Vector2(currentFleeDirection * abilityFleeSpeed, rb.linearVelocity.y);

        Debug.DrawLine(transform.position, transform.position + Vector3.right * currentFleeDirection * 3f, Color.yellow);
    }

    private bool IsPlayerAttacking()
    {
        bool heavyAttacking = playerHeavyAttack != null && playerHeavyAttack.IsAttacking();
        bool lightAttacking = playerLightAttack != null && playerLightAttack.IsAttacking();
        return heavyAttacking || lightAttacking;
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

    private void CheckForIncomingAttacks()
    {
        if (IsAttacking()) return;
        if (stunEffect != null && stunEffect.IsStunned()) return;
        if (isFleeingFromStaminaLoss) return;

        CheckHeavyAttack();
        CheckLightAttack();
    }

    private void CheckHeavyAttack()
    {
        if (playerHeavyAttack == null) return;

        bool isPlayerAttacking = playerHeavyAttack.IsAttacking();
        int currentCombo = playerHeavyAttack.GetCurrentCombo();

        if (isPlayerAttacking && currentCombo != lastPlayerHeavyCombo)
        {
            lastPlayerHeavyCombo = currentCombo;
            TryDodgeAttack("heavy");
        }

        if (!isPlayerAttacking)
        {
            lastPlayerHeavyCombo = -1;
        }
    }

    private void CheckLightAttack()
    {
        if (playerLightAttack == null) return;

        bool isPlayerAttacking = playerLightAttack.IsAttacking();
        int currentCombo = playerLightAttack.GetCurrentCombo();

        if (isPlayerAttacking && currentCombo != lastPlayerLightCombo)
        {
            lastPlayerLightCombo = currentCombo;
            TryDodgeAttack("light");
        }

        if (!isPlayerAttacking)
        {
            lastPlayerLightCombo = -1;
        }
    }

    private void TryDodgeAttack(string attackType)
    {
        if (stunEffect != null && stunEffect.IsStunned())
        {
            Debug.Log("<color=red>DODGE BLOCKED: Enemy is stunned!</color>");
            return;
        }

        if (isFleeingFromStaminaLoss)
        {
            Debug.Log("<color=red>DODGE BLOCKED: Enemy is fleeing from stamina loss!</color>");
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool isBlockedBehind = IsBlockedByWallBehind();

        bool effectivelyGrounded = isGrounded || Mathf.Abs(rb.linearVelocity.y) < 0.1f;
        bool canDodge = effectivelyGrounded && Time.time - lastDodgeTime >= dodgeCooldown;
        bool hasStamina = enemyCombat != null && enemyCombat.CanDodge();

        if (isBlockedBehind)
        {
            Debug.Log($"<color=red>WALL BEHIND! Player {attackType} attack detected! Dodging through player immediately!</color>");

            if (canDodge && hasStamina)
            {
                if (Random.value <= dodgeChance)
                {
                    PerformThroughDodge();
                }
                else
                {
                    Debug.Log("<color=orange>Enemy chose not to dodge (random chance)</color>");
                }
            }
            else
            {
                if (!hasStamina)
                {
                    float currentStamina = enemyCombat != null ? enemyCombat.GetCurrentStamina() : 0;
                    float dodgeCost = stats != null ? stats.dodgeStaminaCost : 30f;
                    Debug.Log($"<color=red>OUT OF STAMINA! Need {dodgeCost}, have {currentStamina:F0}</color>");
                }
            }
        }
        else if (distanceToPlayer <= dangerRange)
        {
            Debug.Log($"<color=yellow>Player {attackType} attack detected! Distance: {distanceToPlayer:F2}, No wall behind</color>");

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
                    Debug.Log($"<color=red>OUT OF STAMINA! Need {dodgeCost}, have {currentStamina:F0}</color>");
                }
                FallbackRetreat();
            }
        }
    }

    private void PerformDodge()
    {
        if (enemyCombat == null || stats == null) return;
        if (stunEffect != null && stunEffect.IsStunned())
        {
            Debug.Log("<color=red>DODGE CANCELLED: Stunned during execution!</color>");
            return;
        }

        bool consumed = enemyCombat.ConsumeStamina(stats.dodgeStaminaCost);

        if (!consumed)
        {
            Debug.Log("<color=red>Failed to consume stamina for dodge!</color>");
            FallbackRetreat();
            return;
        }

        isDodging = true;
        lastDodgeTime = Time.time;
        isRetreating = true;
        retreatStartTime = Time.time;

        float directionFromPlayer = Mathf.Sign(transform.position.x - player.position.x);
        rb.linearVelocity = new Vector2(directionFromPlayer * dodgeForce, rb.linearVelocity.y);

        Debug.Log($"<color=cyan>### ENEMY NORMAL DODGE (AWAY)! ###</color>");

        BleedEffect bleedEffect = GetComponent<BleedEffect>();
        if (bleedEffect != null)
        {
            bleedEffect.OnEnemyDodge();
        }

        Invoke(nameof(EndDodge), 0.2f);
    }

    private void PerformThroughDodge()
    {
        if (enemyCombat == null || stats == null || player == null) return;
        if (stunEffect != null && stunEffect.IsStunned())
        {
            Debug.Log("<color=red>THROUGH-DODGE CANCELLED: Stunned during execution!</color>");
            return;
        }

        bool consumed = enemyCombat.ConsumeStamina(stats.dodgeStaminaCost);

        if (!consumed)
        {
            Debug.Log("<color=red>Failed to consume stamina for through-dodge!</color>");
            return;
        }

        isDodging = true;
        isDamageImmune = true;
        isThroughDodging = true;
        lastDodgeTime = Time.time;

        BleedEffect bleedEffect = GetComponent<BleedEffect>();
        if (bleedEffect != null)
        {
            bleedEffect.OnEnemyDodge();
        }

        FlipEnemyImmediately();

        Debug.Log($"<color=magenta>### STARTING THROUGH-DODGE ###</color>");

        StartCoroutine(ThroughDodgeCoroutine());
    }

    private void FlipEnemyImmediately()
    {
        Transform enemySprite = transform.Find("EnemySprite");
        if (enemySprite != null)
        {
            Vector3 spriteScale = enemySprite.localScale;
            spriteScale.x *= -1;
            enemySprite.localScale = spriteScale;
            Debug.Log($"<color=magenta>Enemy sprite flipped! New scale.x: {spriteScale.x}</color>");
        }

        Transform attackPoint = transform.Find("AttackPoint");
        if (attackPoint != null)
        {
            Vector3 pointPos = attackPoint.localPosition;
            pointPos.x *= -1;
            attackPoint.localPosition = pointPos;
            Debug.Log($"<color=magenta>Enemy attack point flipped! New pos.x: {pointPos.x}</color>");
        }

        FlipEnemyActiveVFX();
    }

    private void FlipEnemyActiveVFX()
    {
        VFXFlipper[] allFlippers = FindObjectsOfType<VFXFlipper>();
        int vfxFlipped = 0;

        foreach (VFXFlipper flipper in allFlippers)
        {
            GameObject obj = flipper.gameObject;

            if (obj.transform.parent != null && obj.transform.parent == transform)
            {
                flipper.FlipX();
                vfxFlipped++;
            }
        }

        if (vfxFlipped == 0)
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("Enemy") && obj.name.Contains("Slash") && obj.name.Contains("(Clone)"))
                {
                    if (obj.transform.parent != null && obj.transform.parent == transform)
                    {
                        Transform objTransform = obj.transform;
                        Vector3 vfxScale = objTransform.localScale;
                        vfxScale.x *= -1;
                        objTransform.localScale = vfxScale;

                        vfxFlipped++;
                        Debug.Log($"<color=magenta>Enemy VFX flipped: {obj.name} | New scale.x: {vfxScale.x}</color>");
                    }
                }
            }
        }

        Debug.Log($"<color=magenta>Total enemy VFX flipped: {vfxFlipped}</color>");
    }

    private IEnumerator ThroughDodgeCoroutine()
    {
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            Physics2D.IgnoreCollision(enemyCollider, playerCollider, true);
        }

        float startX = transform.position.x;
        float playerX = player.position.x;
        float direction = Mathf.Sign(playerX - startX);
        float targetX = playerX + (direction * throughDodgeDistance);

        Debug.Log($"<color=white>Start X: {startX:F2}, Player X: {playerX:F2}, Target X: {targetX:F2}, Direction: {direction}</color>");

        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(targetX, transform.position.y, transform.position.z);

        rb.bodyType = RigidbodyType2D.Kinematic;

        while (elapsed < throughDodgeDuration)
        {
            if (stunEffect != null && stunEffect.IsStunned())
            {
                rb.bodyType = RigidbodyType2D.Dynamic;

                if (playerCollider != null)
                {
                    Physics2D.IgnoreCollision(enemyCollider, playerCollider, false);
                }

                isDodging = false;
                isDamageImmune = false;
                isThroughDodging = false;

                Debug.Log("<color=red>Through-dodge interrupted by stun!</color>");
                yield break;
            }

            elapsed += Time.deltaTime;
            float t = elapsed / throughDodgeDuration;

            transform.position = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        transform.position = targetPos;

        rb.bodyType = RigidbodyType2D.Dynamic;

        if (playerCollider != null)
        {
            Physics2D.IgnoreCollision(enemyCollider, playerCollider, false);
        }

        rb.linearVelocity = Vector2.zero;

        isDodging = false;
        isDamageImmune = false;
        isThroughDodging = false;

        Debug.Log($"<color=green>### THROUGH-DODGE COMPLETE ### Final X: {transform.position.x:F2}</color>");

        FlipPlayerToFaceEnemy();
    }

    private void FlipPlayerToFaceEnemy()
    {
        if (player == null) return;

        Transform playerSprite = player.Find("GenieSprite");
        if (playerSprite != null)
        {
            Vector3 spriteScale = playerSprite.localScale;
            spriteScale.x *= -1;
            playerSprite.localScale = spriteScale;
            Debug.Log($"<color=cyan>Player sprite flipped! New scale.x: {spriteScale.x}</color>");
        }

        Transform[] attackPoints = new Transform[]
        {
            player.Find("HeavyAttack1Point"),
            player.Find("HeavyAttack2Point"),
            player.Find("HeavyAttack3Point"),
            player.Find("LightAttackPoint"),
            player.Find("LightAttackPoint (1)")
        };

        foreach (Transform attackPoint in attackPoints)
        {
            if (attackPoint != null)
            {
                Vector3 pointPos = attackPoint.localPosition;
                pointPos.x *= -1;
                attackPoint.localPosition = pointPos;
            }
        }

        Debug.Log("<color=cyan>Player flipped to face enemy!</color>");
    }

    private void FallbackRetreat()
    {
        isRetreating = true;
        retreatStartTime = Time.time;

        float directionFromPlayer = Mathf.Sign(transform.position.x - player.position.x);
        float normalRetreatSpeed = stats != null ? stats.moveSpeed : 2f;

        rb.linearVelocity = new Vector2(directionFromPlayer * normalRetreatSpeed, rb.linearVelocity.y);
    }

    private void EndDodge()
    {
        isDodging = false;
    }

    public bool IsDamageImmune()
    {
        return isDamageImmune;
    }

    public bool IsFleeingFromStaminaLoss()
    {
        return isFleeingFromStaminaLoss;
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

            if (player != null)
            {
                float directionAwayFromPlayer = Mathf.Sign(transform.position.x - player.position.x);
                Vector2 rayDirection = directionAwayFromPlayer > 0 ? Vector2.right : Vector2.left;

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, transform.position + (Vector3)rayDirection * wallCheckDistance);
            }

            if (isFleeingFromAbility)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, abilityFleeDistance);
            }

            if (isFleeingFromStaminaLoss)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, staminaFleeDistance);
            }
        }
    }
}
