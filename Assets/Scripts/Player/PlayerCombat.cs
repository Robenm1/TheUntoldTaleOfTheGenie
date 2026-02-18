using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats stats;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference heavyAttackAction;

    [Header("Attack Points")]
    [SerializeField] private Transform heavyAttack1Point;
    [SerializeField] private Transform heavyAttack2Point;
    [SerializeField] private Transform heavyAttack3Point;
    [SerializeField] private LayerMask enemyLayers;

    [Header("VFX Prefabs")]
    [SerializeField] private GameObject heavyAttack1VFX;
    [SerializeField] private GameObject heavyAttack2VFX;
    [SerializeField] private GameObject heavyAttack3VFX;

    [Header("VFX Settings")]
    [SerializeField] private float attack1Duration = 1.0f;
    [SerializeField] private float attack2Duration = 1.7f;
    [SerializeField] private float attack3Duration = 1.8f;
    [SerializeField] private float attack3MaxRange = 28.5f;
    [SerializeField] private float attack3HeightAboveEnemy = 2f;

    [Header("Combo Settings")]
    [SerializeField] private float comboWindow = 3f;

    private int currentHeavyCombo = 0;
    private float lastHeavyAttackTime;
    private bool isAttacking;

    private const int TOTAL_HEAVY_ATTACKS = 3;

    private void OnEnable()
    {
        if (heavyAttackAction != null)
            heavyAttackAction.action.Enable();
    }

    private void OnDisable()
    {
        if (heavyAttackAction != null)
            heavyAttackAction.action.Disable();
    }

    private void Start()
    {
        if (stats == null)
        {
            Debug.LogError("PlayerStats not assigned on " + gameObject.name);
        }
    }

    private void Update()
    {
        if (stats == null) return;

        HandleHeavyAttackInput();
        CheckComboReset();
    }

    private void HandleHeavyAttackInput()
    {
        if (heavyAttackAction != null && heavyAttackAction.action.WasPressedThisFrame())
        {
            if (!isAttacking)
            {
                PerformHeavyAttack();
            }
            else
            {
                Debug.Log($"Can't attack - still performing previous attack. Combo: {currentHeavyCombo}");
            }
        }
    }

    private void CheckComboReset()
    {
        if (Time.time - lastHeavyAttackTime > comboWindow && !isAttacking)
        {
            if (currentHeavyCombo > 0)
            {
                Debug.Log($"Combo reset! Was at combo {currentHeavyCombo}");
            }
            currentHeavyCombo = 0;
        }
    }

    private void PerformHeavyAttack()
    {
        if (currentHeavyCombo >= TOTAL_HEAVY_ATTACKS)
        {
            currentHeavyCombo = 0;
        }

        isAttacking = true;
        lastHeavyAttackTime = Time.time;

        Debug.Log($"Performing attack {currentHeavyCombo + 1} / {TOTAL_HEAVY_ATTACKS}");

        if (currentHeavyCombo == 0)
        {
            SpawnAttack1VFX();
            Invoke(nameof(ResetAttackState), attack1Duration);
        }
        else if (currentHeavyCombo == 1)
        {
            SpawnAttack2VFX();
            Invoke(nameof(ResetAttackState), attack2Duration);
        }
        else if (currentHeavyCombo == 2)
        {
            SpawnAttack3VFX();
            Invoke(nameof(ResetAttackState), attack3Duration);
        }

        currentHeavyCombo++;
    }

    private void SpawnAttack1VFX()
    {
        if (heavyAttack1VFX != null && heavyAttack1Point != null)
        {
            Debug.Log("Spawning Attack 1 VFX at " + heavyAttack1Point.position);
            GameObject vfx = Instantiate(heavyAttack1VFX, heavyAttack1Point.position, heavyAttack1Point.rotation);
            Destroy(vfx, attack1Duration);

            DealDamageInArea(heavyAttack1Point.position, 0);
        }
        else
        {
            Debug.LogWarning("Attack 1 VFX or spawn point not assigned!");
        }
    }

    private void SpawnAttack2VFX()
    {
        if (heavyAttack2VFX != null && heavyAttack2Point != null)
        {
            Debug.Log("Spawning Attack 2 VFX at " + heavyAttack2Point.position);
            GameObject vfx = Instantiate(heavyAttack2VFX, heavyAttack2Point.position, heavyAttack2Point.rotation);
            Destroy(vfx, attack2Duration);

            DealDamageInArea(heavyAttack2Point.position, 1);
        }
        else
        {
            Debug.LogWarning("Attack 2 VFX or spawn point not assigned!");
        }
    }

    private void SpawnAttack3VFX()
    {
        Debug.Log("ATTACK 3 TRIGGERED!");

        if (heavyAttack3VFX == null)
        {
            Debug.LogWarning("Attack 3 VFX not assigned!");
            return;
        }

        Transform nearestEnemy = FindNearestEnemy();

        if (nearestEnemy != null)
        {
            Vector3 spawnPosition = new Vector3(
                nearestEnemy.position.x,
                nearestEnemy.position.y + attack3HeightAboveEnemy,
                nearestEnemy.position.z
            );

            Debug.Log($"Spawning Attack 3 VFX on enemy: {nearestEnemy.name} at position: {spawnPosition}");
            GameObject vfx = Instantiate(heavyAttack3VFX, spawnPosition, Quaternion.identity);
            Destroy(vfx, attack3Duration);

            DealDamageInArea(nearestEnemy.position, 2);
        }
        else
        {
            Debug.LogWarning("No enemy found within range for Attack 3!");
        }
    }

    private void DealDamageInArea(Vector3 position, int comboIndex)
    {
        float damage = stats.GetHeavyAttackDamage(comboIndex);
        float damageRadius = stats.attackRange;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(position, damageRadius, enemyLayers);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            EnemyHealth enemyHealth = enemyCollider.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                Debug.Log($"Hit {enemyCollider.name} with attack {comboIndex + 1} for {damage} damage!");
            }
            else
            {
                Debug.LogWarning($"Enemy {enemyCollider.name} doesn't have EnemyHealth component!");
            }
        }

        if (hitEnemies.Length == 0)
        {
            Debug.Log($"Attack {comboIndex + 1} hit no enemies!");
        }
    }

    private Transform FindNearestEnemy()
    {
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, attack3MaxRange, enemyLayers);

        Debug.Log($"Found {nearbyEnemies.Length} enemies within range {attack3MaxRange}");

        if (nearbyEnemies.Length == 0)
        {
            return null;
        }

        Transform closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D enemyCollider in nearbyEnemies)
        {
            float distance = Vector2.Distance(transform.position, enemyCollider.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemyCollider.transform;
            }
        }

        Debug.Log($"Closest enemy: {closestEnemy?.name} at distance: {closestDistance}");
        return closestEnemy;
    }

    private void ResetAttackState()
    {
        isAttacking = false;
        Debug.Log($"Attack finished - ready for next attack. Current combo: {currentHeavyCombo}");
    }

    private void OnDrawGizmosSelected()
    {
        if (stats == null) return;

        if (heavyAttack1Point != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(heavyAttack1Point.position, stats.attackRange);
        }

        if (heavyAttack2Point != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(heavyAttack2Point.position, stats.attackRange);
        }

        if (heavyAttack3Point != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(heavyAttack3Point.position, stats.attackRange);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attack3MaxRange);
    }
}
