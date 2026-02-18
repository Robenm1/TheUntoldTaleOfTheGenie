using UnityEngine;
using UnityEngine.UI;

public class EnemyCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyStats stats;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask playerLayer;

    [Header("UI")]
    [SerializeField] private Slider staminaSlider;

    [Header("Slash VFX")]
    [SerializeField] private GameObject slash1VFX;
    [SerializeField] private GameObject slash2VFX;
    [SerializeField] private GameObject slash3VFX;

    private float currentStamina;
    private float lastAttackTime = -999f;
    private float lastStaminaUseTime = -999f;
    private int currentCombo = 0;
    private bool isAttacking = false;
    private Transform target;
    private const int MAX_COMBO = 3;

    private void Start()
    {
        if (stats == null)
        {
            Debug.LogError("EnemyStats not assigned on " + gameObject.name);
        }

        currentStamina = stats.maxStamina;
        UpdateStaminaUI();

        if (attackPoint == null)
        {
            GameObject attackPointObj = new GameObject("AttackPoint");
            attackPointObj.transform.SetParent(transform);
            attackPointObj.transform.localPosition = new Vector3(1f, 0f, 0f);
            attackPoint = attackPointObj.transform;
        }
    }

    private void Update()
    {
        if (stats == null) return;

        HandleStaminaRegeneration();
    }

    private void HandleStaminaRegeneration()
    {
        if (currentStamina < stats.maxStamina && Time.time - lastStaminaUseTime >= stats.staminaRegenDelay)
        {
            currentStamina += stats.staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, stats.maxStamina);
            UpdateStaminaUI();
        }
    }

    private void UpdateStaminaUI()
    {
        if (staminaSlider != null && stats != null)
        {
            staminaSlider.maxValue = stats.maxStamina;
            staminaSlider.value = currentStamina;
        }
    }

    public bool CanAttack()
    {
        if (isAttacking) return false;
        if (Time.time - lastAttackTime < stats.attackCooldown) return false;
        if (currentStamina < stats.GetSlashStaminaCost(0)) return false;
        return true;
    }

    public bool CanDodge()
    {
        return currentStamina >= stats.dodgeStaminaCost;
    }

    public bool ConsumeStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            lastStaminaUseTime = Time.time;
            UpdateStaminaUI();
            Debug.Log($"Enemy consumed {amount} stamina. Remaining: {currentStamina:F0}/{stats.maxStamina}");
            return true;
        }
        return false;
    }

    public void StartAttackCombo(Transform playerTarget)
    {
        if (!CanAttack()) return;

        target = playerTarget;
        currentCombo = 0;
        PerformNextSlash();
    }

    private void PerformNextSlash()
    {
        if (currentCombo >= MAX_COMBO)
        {
            EndCombo();
            return;
        }

        float staminaCost = stats.GetSlashStaminaCost(currentCombo);

        if (currentStamina < staminaCost)
        {
            Debug.Log($"{gameObject.name} out of stamina! Ending combo early.");
            EndCombo();
            return;
        }

        isAttacking = true;
        lastAttackTime = Time.time;
        lastStaminaUseTime = Time.time;
        currentStamina -= staminaCost;
        UpdateStaminaUI();

        FaceTarget();
        SpawnSlashVFX(currentCombo);

        float damageDelay = stats.GetSlashDamageDelay(currentCombo);
        float attackDuration = stats.GetSlashDuration(currentCombo);

        Invoke(nameof(DealCurrentSlashDamage), damageDelay);
        Invoke(nameof(FinishCurrentSlash), attackDuration);

        Debug.Log($"<color=red>Enemy Slash {currentCombo + 1}! Duration: {attackDuration}s, Stamina: {currentStamina:F0}/{stats.maxStamina}</color>");
    }

    private void FaceTarget()
    {
        if (target != null)
        {
            float direction = Mathf.Sign(target.position.x - transform.position.x);
            if (direction != 0)
            {
                transform.localScale = new Vector3(direction > 0 ? 1 : -1, 1, 1);

                attackPoint.localPosition = new Vector3(Mathf.Abs(attackPoint.localPosition.x) * direction, attackPoint.localPosition.y, attackPoint.localPosition.z);
            }
        }
    }

    private void SpawnSlashVFX(int slashIndex)
    {
        GameObject vfxPrefab = slashIndex switch
        {
            0 => slash1VFX,
            1 => slash2VFX,
            2 => slash3VFX,
            _ => null
        };

        if (vfxPrefab != null && attackPoint != null)
        {
            GameObject vfx = Instantiate(vfxPrefab, attackPoint.position, attackPoint.rotation);
            Destroy(vfx, stats.GetSlashDuration(slashIndex) + 0.5f);
        }
        else
        {
            Debug.LogWarning($"Slash {slashIndex + 1} VFX or attack point not assigned!");
        }
    }

    private void DealCurrentSlashDamage()
    {
        float damage = stats.GetSlashDamage(currentCombo);
        float damageRadius = stats.attackRange;

        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, damageRadius, playerLayer);

        foreach (Collider2D playerCollider in hitPlayers)
        {
            PlayerHealth playerHealth = playerCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Enemy hit player with slash {currentCombo + 1} for {damage} damage!");
            }
        }

        if (hitPlayers.Length == 0)
        {
            Debug.Log($"Enemy slash {currentCombo + 1} missed!");
        }
    }

    private void FinishCurrentSlash()
    {
        currentCombo++;

        if (currentCombo < MAX_COMBO && currentStamina >= stats.GetSlashStaminaCost(currentCombo))
        {
            PerformNextSlash();
        }
        else
        {
            EndCombo();
        }
    }

    private void EndCombo()
    {
        isAttacking = false;
        currentCombo = 0;
        Debug.Log($"<color=yellow>Enemy combo finished! Stamina: {currentStamina:F0}/{stats.maxStamina}</color>");
    }

    public bool IsAttacking()
    {
        return isAttacking;
    }

    public float GetCurrentStamina()
    {
        return currentStamina;
    }

    public float GetMaxStamina()
    {
        return stats != null ? stats.maxStamina : 0;
    }

    public float GetStaminaPercentage()
    {
        return stats != null ? currentStamina / stats.maxStamina : 0;
    }

    private void OnDrawGizmosSelected()
    {
        if (stats == null || attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, stats.attackRange);
    }
}
