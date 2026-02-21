using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class GenieAbility1 : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference ability1Action;

    [Header("UI Indicators")]
    [SerializeField] private GameObject greenBoxUI;
    [SerializeField] private GameObject redBoxUI;

    [Header("Cooldown UI")]
    [SerializeField] private Image abilityIcon;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color cooldownColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    [Header("Box Follow Settings")]
    [SerializeField] private float boxFollowSpeed = 5f;
    [SerializeField] private float boxHeightOffset = 0f;

    [Header("Ability Settings")]
    [SerializeField] private float castTime = 2f;
    [SerializeField] private float redBoxWarningTime = 0.5f;
    [SerializeField] private float cooldown = 10f;
    [SerializeField] private float spikeRiseDuration = 0.5f;
    [SerializeField] private float spikeDamage = 15f;
    [SerializeField] private float bleedDuration = 8f;
    [SerializeField] private float bleedDamagePerDodge = 10f;

    [Header("VFX")]
    [SerializeField] private GameObject spikePrefab;

    [Header("References")]
    [SerializeField] private LayerMask enemyLayer;

    private Transform targetEnemy;
    private bool isCasting = false;
    private bool isOnCooldown = false;
    private float lastCastTime = -999f;
    private Vector3 targetPosition;
    private Vector2 greenBoxOriginalSize;
    private Vector2 redBoxOriginalSize;
    private float greenBoxFixedY;

    private void Awake()
    {
        if (greenBoxUI != null)
        {
            RectTransform greenRect = greenBoxUI.GetComponent<RectTransform>();
            if (greenRect != null)
            {
                greenBoxOriginalSize = greenRect.sizeDelta;
            }
        }

        if (redBoxUI != null)
        {
            RectTransform redRect = redBoxUI.GetComponent<RectTransform>();
            if (redRect != null)
            {
                redBoxOriginalSize = redRect.sizeDelta;
            }
        }

        if (abilityIcon == null)
        {
            GameObject iconObj = GameObject.Find("PlayerAbility1");
            if (iconObj != null)
            {
                abilityIcon = iconObj.GetComponent<Image>();
            }
        }

        if (cooldownText == null)
        {
            GameObject textObj = GameObject.Find("Ability1Text");
            if (textObj != null)
            {
                cooldownText = textObj.GetComponent<TextMeshProUGUI>();
            }
        }

        if (cooldownText != null)
        {
            cooldownText.text = "";
        }

        if (abilityIcon != null)
        {
            abilityIcon.color = normalColor;
        }
    }

    private void OnEnable()
    {
        if (ability1Action != null)
            ability1Action.action.Enable();
    }

    private void OnDisable()
    {
        if (ability1Action != null)
            ability1Action.action.Disable();
    }

    private void Update()
    {
        if (ability1Action != null && ability1Action.action.WasPressedThisFrame())
        {
            if (CanCast())
            {
                StartCasting();
            }
            else if (isOnCooldown)
            {
                Debug.Log($"Ability on cooldown! {GetCooldownRemaining():F1}s remaining");
            }
        }

        if (isCasting && targetEnemy != null && greenBoxUI != null)
        {
            UpdateGreenBoxPosition();
        }

        if (isOnCooldown)
        {
            UpdateCooldownUI();
        }
    }

    private bool CanCast()
    {
        return !isCasting && !isOnCooldown && Time.time >= lastCastTime + cooldown;
    }

    private void StartCasting()
    {
        targetEnemy = FindNearestEnemy();

        if (targetEnemy == null)
        {
            Debug.Log("<color=yellow>No enemy found for ability!</color>");
            return;
        }

        isCasting = true;

        if (greenBoxUI != null)
        {
            greenBoxUI.SetActive(true);
            Vector3 startPos = new Vector3(targetEnemy.position.x, targetEnemy.position.y + boxHeightOffset, targetEnemy.position.z);
            greenBoxUI.transform.position = startPos;
            greenBoxFixedY = startPos.y;
            Debug.Log("<color=green>Green box activated!</color>");
        }

        Debug.Log("<color=green>Genie Ability 1 - Casting started!</color>");

        StartCoroutine(CastAbilityCoroutine());
    }

    private IEnumerator CastAbilityCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < castTime)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        FinishCast();
    }

    private void FinishCast()
    {
        isCasting = false;

        if (greenBoxUI != null)
        {
            greenBoxUI.SetActive(false);
            Debug.Log("<color=green>Green box deactivated!</color>");
        }

        if (targetEnemy != null)
        {
            targetPosition = targetEnemy.position;
        }
        else if (greenBoxUI != null)
        {
            targetPosition = greenBoxUI.transform.position;
        }

        if (redBoxUI != null)
        {
            redBoxUI.SetActive(true);
            Vector3 redBoxPos = new Vector3(targetPosition.x, targetPosition.y + boxHeightOffset, targetPosition.z);
            redBoxUI.transform.position = redBoxPos;
            Debug.Log("<color=red>Red box activated! Warning phase...</color>");
        }

        StartCoroutine(SpikeSequenceCoroutine());
    }

    private IEnumerator SpikeSequenceCoroutine()
    {
        yield return new WaitForSeconds(redBoxWarningTime);

        Debug.Log("<color=red>Genie Ability 1 - Spikes rising!</color>");

        SpawnSpikes();

        if (redBoxUI != null)
        {
            redBoxUI.SetActive(false);
            Debug.Log("<color=red>Red box deactivated!</color>");
        }

        lastCastTime = Time.time;
        isOnCooldown = true;

        if (abilityIcon != null)
        {
            abilityIcon.color = cooldownColor;
        }

        StartCoroutine(CooldownCoroutine());
    }

    private IEnumerator CooldownCoroutine()
    {
        yield return new WaitForSeconds(cooldown);

        isOnCooldown = false;

        if (abilityIcon != null)
        {
            abilityIcon.color = normalColor;
        }

        if (cooldownText != null)
        {
            cooldownText.text = "";
        }

        Debug.Log("<color=cyan>Genie Ability 1 ready!</color>");
    }

    private void UpdateCooldownUI()
    {
        if (cooldownText != null)
        {
            float remaining = GetCooldownRemaining();
            cooldownText.text = remaining > 0 ? remaining.ToString("F1") : "";
        }
    }

    private void SpawnSpikes()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(targetPosition, 1.5f, enemyLayer);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            EnemyHealth enemyHealth = enemyCollider.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(spikeDamage);

                BleedEffect bleedEffect = enemyCollider.GetComponent<BleedEffect>();
                if (bleedEffect == null)
                {
                    bleedEffect = enemyCollider.gameObject.AddComponent<BleedEffect>();
                }

                bleedEffect.ApplyBleed(bleedDuration, bleedDamagePerDodge);

                Debug.Log($"<color=red>Enemy hit by spikes! Damage: {spikeDamage}, Bleed applied!</color>");
            }
        }

        if (spikePrefab != null)
        {
            GameObject spikes = Instantiate(spikePrefab, targetPosition, Quaternion.identity);
            Destroy(spikes, spikeRiseDuration + 1f);
        }
    }

    private void UpdateGreenBoxPosition()
    {
        if (greenBoxUI == null || targetEnemy == null) return;

        Vector3 currentPos = greenBoxUI.transform.position;
        Vector3 targetPos = new Vector3(targetEnemy.position.x, greenBoxFixedY, targetEnemy.position.z);

        float newX = Mathf.Lerp(currentPos.x, targetPos.x, boxFollowSpeed * Time.deltaTime);

        greenBoxUI.transform.position = new Vector3(newX, greenBoxFixedY, currentPos.z);
    }

    private Transform FindNearestEnemy()
    {
        GameObject enemyObj = GameObject.FindGameObjectWithTag("Enemy");
        if (enemyObj != null)
        {
            return enemyObj.transform;
        }

        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 50f, enemyLayer);

        if (enemies.Length > 0)
        {
            Transform closest = null;
            float closestDistance = Mathf.Infinity;

            foreach (Collider2D enemy in enemies)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = enemy.transform;
                }
            }

            return closest;
        }

        return null;
    }

    private float GetCooldownRemaining()
    {
        return Mathf.Max(0, cooldown - (Time.time - lastCastTime));
    }

    public bool IsCasting()
    {
        return isCasting;
    }
}
