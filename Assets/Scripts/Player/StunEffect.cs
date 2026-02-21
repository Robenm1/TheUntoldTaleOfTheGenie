using UnityEngine;

public class StunEffect : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject stunIconCanvas;

    private bool isStunned = false;
    private float stunEndTime = 0f;
    private StunIconSprite stunIconSprite;
    private EnemyAI enemyAI;
    private EnemyCombat enemyCombat;
    private Rigidbody2D rb;

    private void Awake()
    {
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            Transform stunIcon = canvasObj.transform.Find("Stunned");
            if (stunIcon != null)
            {
                stunIconCanvas = stunIcon.gameObject;
            }
        }

        stunIconSprite = GetComponentInChildren<StunIconSprite>(true);

        if (stunIconSprite == null)
        {
            Transform enemyStunIcon = transform.Find("StunnedIcon");
            if (enemyStunIcon == null)
            {
                enemyStunIcon = transform.Find("EnemyStunned");
            }

            if (enemyStunIcon != null)
            {
                stunIconSprite = enemyStunIcon.GetComponent<StunIconSprite>();
                if (stunIconSprite == null)
                {
                    stunIconSprite = enemyStunIcon.gameObject.AddComponent<StunIconSprite>();
                }
            }
        }

        if (stunIconSprite != null)
        {
            Debug.Log("<color=cyan>StunIconSprite found and ready!</color>");
        }
        else
        {
            Debug.LogWarning("<color=red>StunIconSprite not found! Make sure StunnedIcon child exists.</color>");
        }

        enemyAI = GetComponent<EnemyAI>();
        enemyCombat = GetComponent<EnemyCombat>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isStunned)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            if (Time.time >= stunEndTime)
            {
                RemoveStun();
            }
        }
    }

    public void ApplyStun(float duration)
    {
        isStunned = true;
        stunEndTime = Time.time + duration;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;

            if (rb.bodyType == RigidbodyType2D.Kinematic)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
        }

        if (enemyAI != null)
        {
            enemyAI.StopAllCoroutines();
            enemyAI.CancelInvoke();
            enemyAI.enabled = false;
            Debug.Log("<color=red>EnemyAI DISABLED!</color>");
        }

        if (enemyCombat != null)
        {
            enemyCombat.InterruptAttack();
            enemyCombat.CancelInvoke();
            enemyCombat.enabled = false;
            Debug.Log("<color=red>EnemyCombat DISABLED!</color>");
        }

        Collider2D enemyCollider = GetComponent<Collider2D>();
        Collider2D[] playerColliders = FindObjectsOfType<Collider2D>();
        foreach (Collider2D col in playerColliders)
        {
            if (col.gameObject.CompareTag("Player"))
            {
                Physics2D.IgnoreCollision(enemyCollider, col, false);
            }
        }

        if (stunIconCanvas != null)
        {
            stunIconCanvas.SetActive(true);
        }

        if (stunIconSprite != null)
        {
            stunIconSprite.ShowIcon();
            Debug.Log("<color=magenta>Calling ShowIcon on StunIconSprite</color>");
        }
        else
        {
            Debug.LogWarning("<color=red>stunIconSprite is null! Cannot show icon.</color>");
        }

        Debug.Log($"<color=magenta>### STUN APPLIED! ENEMY SCRIPTS COMPLETELY DISABLED! Duration: {duration}s ###</color>");
    }

    public void RemoveStun()
    {
        isStunned = false;

        if (enemyAI != null)
        {
            enemyAI.enabled = true;
            Debug.Log("<color=green>EnemyAI RE-ENABLED!</color>");
        }

        if (enemyCombat != null)
        {
            enemyCombat.enabled = true;
            Debug.Log("<color=green>EnemyCombat RE-ENABLED!</color>");
        }

        if (stunIconCanvas != null)
        {
            stunIconCanvas.SetActive(false);
        }

        if (stunIconSprite != null)
        {
            stunIconSprite.HideIcon();
        }

        Debug.Log("<color=green>### STUN EXPIRED! Enemy scripts re-enabled and can act again. ###</color>");
    }

    public bool IsStunned()
    {
        return isStunned;
    }

    public float GetStunTimeRemaining()
    {
        return Mathf.Max(0, stunEndTime - Time.time);
    }
}
