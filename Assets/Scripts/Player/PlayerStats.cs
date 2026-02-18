using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Game/Player Stats")]
public class PlayerStats : ScriptableObject
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float healthRegenRate = 5f;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float acceleration = 50f;
    public float deceleration = 50f;

    [Header("Dash")]
    public float dashDistance = 5f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Light Attack Combo")]
    public int lightAttackComboCount = 3;
    public float lightAttackSpeed = 0.3f;
    public float lightAttackComboDuration = 1.5f;
    public float[] lightAttackDamage = new float[] { 10f, 12f, 15f };

    [Header("Heavy Attack Combo")]
    public int heavyAttackComboCount = 2;
    public float heavyAttackSpeed = 0.6f;
    public float heavyAttackComboDuration = 2f;
    public float[] heavyAttackDamage = new float[] { 25f, 35f };

    [Header("Combat Settings")]
    public float attackRange = 2f;
    public float comboResetTime = 1f;

    [Header("Abilities")]
    public float ability1Cooldown = 5f;
    public float ability1Damage = 30f;
    public float ability1Duration = 2f;

    public float ability2Cooldown = 8f;
    public float ability2Damage = 50f;
    public float ability2Duration = 3f;

    [Header("Defense")]
    public float armor = 10f;
    public float damageReduction = 0.1f;

    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        float actualDamage = damage * (1f - damageReduction);
        currentHealth = Mathf.Max(0, currentHealth - actualDamage);
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    public float GetLightAttackDamage(int comboIndex)
    {
        if (comboIndex >= 0 && comboIndex < lightAttackDamage.Length)
            return lightAttackDamage[comboIndex];
        return lightAttackDamage[0];
    }

    public float GetHeavyAttackDamage(int comboIndex)
    {
        if (comboIndex >= 0 && comboIndex < heavyAttackDamage.Length)
            return heavyAttackDamage[comboIndex];
        return heavyAttackDamage[0];
    }
}
