using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Game/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [Header("Health")]
    public float maxHealth = 100f;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float staminaRegenRate = 10f;
    public float staminaRegenDelay = 2f;
    public float dodgeStaminaCost = 30f;

    [Header("Combat")]
    public float attackRange = 2.5f;
    public float attackCooldown = 1.5f;
    public float[] slashDamage = new float[] { 10f, 12f, 15f };
    public float[] slashStaminaCost = new float[] { 20f, 25f, 30f };
    public float[] slashDuration = new float[] { 0.5f, 0.6f, 0.7f };
    public float[] slashDamageDelay = new float[] { 0.2f, 0.25f, 0.3f };

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float chaseSpeed = 3f;

    public float GetSlashDamage(int slashIndex)
    {
        if (slashIndex >= 0 && slashIndex < slashDamage.Length)
            return slashDamage[slashIndex];
        return slashDamage[0];
    }

    public float GetSlashStaminaCost(int slashIndex)
    {
        if (slashIndex >= 0 && slashIndex < slashStaminaCost.Length)
            return slashStaminaCost[slashIndex];
        return slashStaminaCost[0];
    }

    public float GetSlashDuration(int slashIndex)
    {
        if (slashIndex >= 0 && slashIndex < slashDuration.Length)
            return slashDuration[slashIndex];
        return slashDuration[0];
    }

    public float GetSlashDamageDelay(int slashIndex)
    {
        if (slashIndex >= 0 && slashIndex < slashDamageDelay.Length)
            return slashDamageDelay[slashIndex];
        return slashDamageDelay[0];
    }
}
