using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyStats stats;

    [Header("UI")]
    [SerializeField] private Slider healthSlider;

    [Header("Stamina Flee Damage Reduction")]
    [SerializeField] private float staminaFleeDamageReduction = 0.5f;

    private float currentHealth;

    private void Start()
    {
        if (stats == null)
        {
            Debug.LogError("EnemyStats not assigned on " + gameObject.name);
            return;
        }

        currentHealth = stats.maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        if (stats == null) return;

        EnemyAI enemyAI = GetComponent<EnemyAI>();

        if (enemyAI != null && enemyAI.IsDamageImmune())
        {
            Debug.Log($"{gameObject.name} is damage immune during through-dodge!");
            return;
        }

        float finalDamage = damage;

        if (enemyAI != null && enemyAI.IsFleeingFromStaminaLoss())
        {
            finalDamage = damage * staminaFleeDamageReduction;
            Debug.Log($"<color=cyan>{gameObject.name} is fleeing from stamina loss! Damage reduced: {damage:F0} -> {finalDamage:F0} ({staminaFleeDamageReduction * 100}%)</color>");
        }

        currentHealth -= finalDamage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"{gameObject.name} took {finalDamage:F0} damage! Health: {currentHealth:F0}/{stats.maxHealth}");

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (stats == null) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(stats.maxHealth, currentHealth);

        Debug.Log($"{gameObject.name} healed {amount}! Health: {currentHealth}/{stats.maxHealth}");

        UpdateHealthUI();
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null && stats != null)
        {
            healthSlider.maxValue = stats.maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died!");
        Destroy(gameObject);
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return stats != null ? stats.maxHealth : 0;
    }

    public float GetHealthPercentage()
    {
        return stats != null ? currentHealth / stats.maxHealth : 0;
    }

    public void ResetHealth()
    {
        if (stats != null)
        {
            currentHealth = stats.maxHealth;
            UpdateHealthUI();
            Debug.Log($"{gameObject.name} health reset to full");
        }
    }
}
