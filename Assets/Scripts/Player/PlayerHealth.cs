using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats stats;
    [SerializeField] private Slider healthSlider;

    [Header("Health Regeneration")]
    [SerializeField] private bool enableHealthRegen = true;
    [SerializeField] private float regenDelay = 3f;

    private float currentHealth;
    private float lastDamageTime = -999f;

    private void Start()
    {
        if (stats == null)
        {
            Debug.LogError("PlayerStats not assigned on " + gameObject.name);
            return;
        }

        currentHealth = stats.maxHealth;
        UpdateHealthUI();
    }

    private void Update()
    {
        if (enableHealthRegen && stats != null)
        {
            HandleHealthRegeneration();
        }
    }

    private void HandleHealthRegeneration()
    {
        if (currentHealth < stats.maxHealth && Time.time - lastDamageTime >= regenDelay)
        {
            Heal(stats.healthRegenRate * Time.deltaTime);
        }
    }

    public void TakeDamage(float damage)
    {
        if (stats == null) return;

        float damageAfterArmor = CalculateDamageWithArmor(damage);

        currentHealth -= damageAfterArmor;
        currentHealth = Mathf.Max(0, currentHealth);

        lastDamageTime = Time.time;

        Debug.Log($"Player took {damage} damage → {damageAfterArmor:F1} after armor! Health: {currentHealth:F1}/{stats.maxHealth}");

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private float CalculateDamageWithArmor(float incomingDamage)
    {
        float armorReduction = stats.armor / (stats.armor + 100f);

        float damageAfterArmor = incomingDamage * (1f - armorReduction);

        float damageAfterReduction = damageAfterArmor * (1f - stats.damageReduction);

        return damageAfterReduction;
    }

    public void Heal(float amount)
    {
        if (stats == null) return;

        float oldHealth = currentHealth;
        currentHealth += amount;
        currentHealth = Mathf.Min(stats.maxHealth, currentHealth);

        if (currentHealth > oldHealth)
        {
            UpdateHealthUI();
        }
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
        Debug.Log("Player died!");

        enabled = false;
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

    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    public void ResetHealth()
    {
        if (stats != null)
        {
            currentHealth = stats.maxHealth;
            UpdateHealthUI();
            enabled = true;
            Debug.Log("Player health reset to full");
        }
    }

    public float GetArmorValue()
    {
        return stats != null ? stats.armor : 0;
    }

    public float GetDamageReductionPercentage()
    {
        if (stats == null) return 0;

        float armorReduction = stats.armor / (stats.armor + 100f);
        float totalReduction = 1f - ((1f - armorReduction) * (1f - stats.damageReduction));

        return totalReduction * 100f;
    }

    private void OnValidate()
    {
        if (Application.isPlaying && stats != null)
        {
            UpdateHealthUI();
        }
    }
}
