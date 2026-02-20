using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLightAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats stats;
    [SerializeField] private PlayerCombat heavyAttack;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference lightAttackAction;

    [Header("Attack Points")]
    [SerializeField] private Transform lightAttack1Point;
    [SerializeField] private Transform lightAttack2Point;
    [SerializeField] private LayerMask enemyLayers;

    [Header("VFX Prefabs")]
    [SerializeField] private GameObject lightAttack1VFX;
    [SerializeField] private GameObject lightAttack2VFX;

    [Header("Combo Settings")]
    [SerializeField] private float comboWindow = 1.5f;

    private int currentLightCombo = 0;
    private float lastLightAttackTime;
    private bool isAttacking;

    private const int TOTAL_LIGHT_ATTACKS = 2;

    private void Start()
    {
        if (stats == null)
        {
            Debug.LogError("PlayerStats not assigned on " + gameObject.name);
        }

        if (heavyAttack == null)
        {
            heavyAttack = GetComponent<PlayerCombat>();
        }
    }

    private void OnEnable()
    {
        if (lightAttackAction != null)
            lightAttackAction.action.Enable();
    }

    private void OnDisable()
    {
        if (lightAttackAction != null)
            lightAttackAction.action.Disable();
    }

    private void Update()
    {
        if (stats == null) return;

        HandleLightAttackInput();
        CheckComboReset();
    }

    private void HandleLightAttackInput()
    {
        if (lightAttackAction != null && lightAttackAction.action.WasPressedThisFrame())
        {
            bool heavyAttacking = heavyAttack != null && heavyAttack.IsAttacking();

            if (!isAttacking && !heavyAttacking)
            {
                PerformLightAttack();
            }
            else if (heavyAttacking)
            {
                Debug.Log("Can't light attack - heavy attack is playing!");
            }
            else
            {
                Debug.Log($"Can't light attack - still performing previous attack. Combo: {currentLightCombo}");
            }
        }
    }

    private void CheckComboReset()
    {
        if (Time.time - lastLightAttackTime > comboWindow && !isAttacking)
        {
            if (currentLightCombo > 0)
            {
                Debug.Log($"Light combo reset! Was at combo {currentLightCombo}");
            }
            currentLightCombo = 0;
        }
    }

    private void PerformLightAttack()
    {
        if (currentLightCombo >= TOTAL_LIGHT_ATTACKS)
        {
            currentLightCombo = 0;
        }

        isAttacking = true;
        lastLightAttackTime = Time.time;

        Debug.Log($"Performing light attack {currentLightCombo + 1} / {TOTAL_LIGHT_ATTACKS}");

        if (currentLightCombo == 0)
        {
            SpawnAttack1VFX();
            Invoke(nameof(ResetAttackState), stats.GetLightAttackDuration(0));
        }
        else if (currentLightCombo == 1)
        {
            SpawnAttack2VFX();
            Invoke(nameof(ResetAttackState), stats.GetLightAttackDuration(1));
        }

        currentLightCombo++;
    }

    private void SpawnAttack1VFX()
    {
        if (lightAttack1VFX != null && lightAttack1Point != null)
        {
            Debug.Log("Spawning Light Attack 1 VFX at " + lightAttack1Point.position);
            GameObject vfx = Instantiate(lightAttack1VFX, lightAttack1Point.position, lightAttack1Point.rotation);
            Destroy(vfx, stats.GetLightAttackDuration(0));

            Invoke(nameof(DealAttack1Damage), stats.GetLightAttackDamageDelay(0));
        }
        else
        {
            Debug.LogWarning("Light Attack 1 VFX or spawn point not assigned!");
        }
    }

    private void SpawnAttack2VFX()
    {
        if (lightAttack2VFX != null && lightAttack2Point != null)
        {
            Debug.Log("Spawning Light Attack 2 VFX at " + lightAttack2Point.position);
            GameObject vfx = Instantiate(lightAttack2VFX, lightAttack2Point.position, lightAttack2Point.rotation);
            Destroy(vfx, stats.GetLightAttackDuration(1));

            Invoke(nameof(DealAttack2Damage), stats.GetLightAttackDamageDelay(1));
        }
        else
        {
            Debug.LogWarning("Light Attack 2 VFX or spawn point not assigned!");
        }
    }

    private void DealAttack1Damage()
    {
        if (lightAttack1Point != null)
        {
            DealDamageInArea(lightAttack1Point.position, 0);
        }
    }

    private void DealAttack2Damage()
    {
        if (lightAttack2Point != null)
        {
            DealDamageInArea(lightAttack2Point.position, 1);
        }
    }

    private void DealDamageInArea(Vector3 position, int comboIndex)
    {
        float damage = stats.GetLightAttackDamage(comboIndex);
        float damageRadius = stats.attackRange;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(position, damageRadius, enemyLayers);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            EnemyHealth enemyHealth = enemyCollider.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                Debug.Log($"Hit {enemyCollider.name} with light attack {comboIndex + 1} for {damage} damage!");
            }
            else
            {
                Debug.LogWarning($"Enemy {enemyCollider.name} doesn't have EnemyHealth component!");
            }
        }

        if (hitEnemies.Length == 0)
        {
            Debug.Log($"Light attack {comboIndex + 1} hit no enemies!");
        }
    }

    private void ResetAttackState()
    {
        isAttacking = false;
        Debug.Log($"Light attack finished - ready for next attack. Current combo: {currentLightCombo}");
    }

    public bool IsAttacking()
    {
        return isAttacking;
    }

    public int GetCurrentCombo()
    {
        return currentLightCombo;
    }

    private void OnDrawGizmosSelected()
    {
        if (stats == null) return;

        if (lightAttack1Point != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(lightAttack1Point.position, stats.attackRange);
        }

        if (lightAttack2Point != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(lightAttack2Point.position, stats.attackRange);
        }
    }
}
