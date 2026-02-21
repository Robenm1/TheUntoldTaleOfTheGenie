using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class GenieAbility2 : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference ability2Action;

    [Header("Barrier UI - Right Side")]
    [SerializeField] private GameObject barrierVisualRight;
    [SerializeField] private Slider barrierSliderRight;

    [Header("Barrier UI - Left Side")]
    [SerializeField] private GameObject barrierVisualLeft;
    [SerializeField] private Slider barrierSliderLeft;

    [Header("Cooldown UI")]
    [SerializeField] private Image abilityIcon;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color cooldownColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    [Header("Barrier Settings")]
    [SerializeField] private float maxBarrierDuration = 5f;
    [SerializeField] private float maxCooldown = 15f;
    [SerializeField] private float minCooldown = 5f;
    [SerializeField] private float minHoldTime = 0.5f;
    [SerializeField] private float stunDuration = 3f;

    [Header("References")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float stunCheckRadius = 3f;

    private bool isBarrierActive = false;
    private bool isOnCooldown = false;
    private float currentBarrierTime = 0f;
    private float lastCastTime = -999f;
    private float currentCooldown = 0f;
    private float barrierHoldDuration = 0f;
    private Transform playerSprite;

    private void Awake()
    {
        playerSprite = transform.Find("GenieSprite");

        if (barrierVisualRight != null)
        {
            barrierVisualRight.SetActive(false);
        }

        if (barrierVisualLeft != null)
        {
            barrierVisualLeft.SetActive(false);
        }

        if (barrierSliderRight != null)
        {
            barrierSliderRight.gameObject.SetActive(false);
            barrierSliderRight.maxValue = maxBarrierDuration;
            barrierSliderRight.value = maxBarrierDuration;
        }

        if (barrierSliderLeft != null)
        {
            barrierSliderLeft.gameObject.SetActive(false);
            barrierSliderLeft.maxValue = maxBarrierDuration;
            barrierSliderLeft.value = maxBarrierDuration;
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
        if (ability2Action != null)
            ability2Action.action.Enable();
    }

    private void OnDisable()
    {
        if (ability2Action != null)
            ability2Action.action.Disable();
    }

    private void Update()
    {
        if (ability2Action != null)
        {
            if (ability2Action.action.WasPressedThisFrame())
            {
                if (CanActivateBarrier())
                {
                    ActivateBarrier();
                }
            }

            if (ability2Action.action.IsPressed() && isBarrierActive)
            {
                HoldBarrier();
            }

            if (ability2Action.action.WasReleasedThisFrame() && isBarrierActive)
            {
                DeactivateBarrier();
            }
        }

        if (isOnCooldown)
        {
            UpdateCooldownUI();
        }

        if (isBarrierActive)
        {
            UpdateBarrierSide();
        }
    }

    private bool CanActivateBarrier()
    {
        return !isBarrierActive && !isOnCooldown;
    }

    private void ActivateBarrier()
    {
        isBarrierActive = true;
        currentBarrierTime = maxBarrierDuration;
        barrierHoldDuration = 0f;

        UpdateBarrierSide();

        CheckForStunningEnemies();

        Debug.Log("<color=cyan>Barrier activated!</color>");
    }

    private void UpdateBarrierSide()
    {
        if (playerSprite == null) return;

        float playerFacing = Mathf.Sign(playerSprite.localScale.x);
        bool facingRight = playerFacing > 0;

        if (facingRight)
        {
            if (barrierVisualRight != null) barrierVisualRight.SetActive(true);
            if (barrierSliderRight != null)
            {
                barrierSliderRight.gameObject.SetActive(true);
                barrierSliderRight.value = currentBarrierTime;
            }

            if (barrierVisualLeft != null) barrierVisualLeft.SetActive(false);
            if (barrierSliderLeft != null) barrierSliderLeft.gameObject.SetActive(false);

            Debug.Log("<color=cyan>Showing RIGHT barrier</color>");
        }
        else
        {
            if (barrierVisualLeft != null) barrierVisualLeft.SetActive(true);
            if (barrierSliderLeft != null)
            {
                barrierSliderLeft.gameObject.SetActive(true);
                barrierSliderLeft.value = currentBarrierTime;
            }

            if (barrierVisualRight != null) barrierVisualRight.SetActive(false);
            if (barrierSliderRight != null) barrierSliderRight.gameObject.SetActive(false);

            Debug.Log("<color=cyan>Showing LEFT barrier</color>");
        }
    }

    private void HoldBarrier()
    {
        currentBarrierTime -= Time.deltaTime;
        barrierHoldDuration += Time.deltaTime;

        if (playerSprite != null)
        {
            float playerFacing = Mathf.Sign(playerSprite.localScale.x);
            bool facingRight = playerFacing > 0;

            if (facingRight && barrierSliderRight != null)
            {
                barrierSliderRight.value = currentBarrierTime;
            }
            else if (!facingRight && barrierSliderLeft != null)
            {
                barrierSliderLeft.value = currentBarrierTime;
            }
        }

        if (currentBarrierTime <= 0f)
        {
            DeactivateBarrier();
        }
    }

    private void DeactivateBarrier()
    {
        isBarrierActive = false;

        if (barrierVisualRight != null)
        {
            barrierVisualRight.SetActive(false);
        }

        if (barrierVisualLeft != null)
        {
            barrierVisualLeft.SetActive(false);
        }

        if (barrierSliderRight != null)
        {
            barrierSliderRight.gameObject.SetActive(false);
        }

        if (barrierSliderLeft != null)
        {
            barrierSliderLeft.gameObject.SetActive(false);
        }

        if (barrierHoldDuration < minHoldTime)
        {
            currentCooldown = maxCooldown;
            Debug.Log("<color=yellow>Barrier released too quickly! Full cooldown applied.</color>");
        }
        else
        {
            float cooldownRatio = barrierHoldDuration / maxBarrierDuration;
            float calculatedCooldown = maxCooldown * cooldownRatio;
            currentCooldown = Mathf.Max(minCooldown, calculatedCooldown);
            Debug.Log($"<color=yellow>Barrier held for {barrierHoldDuration:F1}s. Calculated cooldown: {calculatedCooldown:F1}s, Final cooldown: {currentCooldown:F1}s (min: {minCooldown}s)</color>");
        }

        StartCooldown();
    }

    private void CheckForStunningEnemies()
    {
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, stunCheckRadius, enemyLayer);

        foreach (Collider2D enemyCollider in nearbyEnemies)
        {
            EnemyCombat enemyCombat = enemyCollider.GetComponent<EnemyCombat>();
            if (enemyCombat != null && enemyCombat.IsAttacking())
            {
                StunEffect stunEffect = enemyCollider.GetComponent<StunEffect>();
                if (stunEffect == null)
                {
                    stunEffect = enemyCollider.gameObject.AddComponent<StunEffect>();
                }

                stunEffect.ApplyStun(stunDuration);

                enemyCombat.InterruptAttack();

                Debug.Log($"<color=magenta>Enemy stunned for {stunDuration}s!</color>");
            }
        }
    }

    private void StartCooldown()
    {
        isOnCooldown = true;
        lastCastTime = Time.time;

        if (abilityIcon != null)
        {
            abilityIcon.color = cooldownColor;
        }

        StartCoroutine(CooldownCoroutine());
    }

    private IEnumerator CooldownCoroutine()
    {
        yield return new WaitForSeconds(currentCooldown);

        isOnCooldown = false;

        if (abilityIcon != null)
        {
            abilityIcon.color = normalColor;
        }

        if (cooldownText != null)
        {
            cooldownText.text = "";
        }

        Debug.Log("<color=cyan>Barrier ready!</color>");
    }

    private void UpdateCooldownUI()
    {
        if (cooldownText != null)
        {
            float remaining = GetCooldownRemaining();
            cooldownText.text = remaining > 0 ? remaining.ToString("F1") : "";
        }
    }

    private float GetCooldownRemaining()
    {
        return Mathf.Max(0, currentCooldown - (Time.time - lastCastTime));
    }

    public bool IsBarrierActive()
    {
        return isBarrierActive;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, stunCheckRadius);
    }
}
