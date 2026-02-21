using UnityEngine;
using UnityEngine.UI;

public class BleedEffect : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject bleedIconUI;

    private bool isBleeding = false;
    private float bleedEndTime = 0f;
    private float damagePerDodge = 0f;
    private EnemyHealth enemyHealth;
    private EnemyAI enemyAI;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        enemyAI = GetComponent<EnemyAI>();

        if (bleedIconUI == null)
        {
            GameObject canvasObj = GameObject.Find("Canvas");
            if (canvasObj != null)
            {
                Transform bleedIcon = canvasObj.transform.Find("Bleed");
                if (bleedIcon != null)
                {
                    bleedIconUI = bleedIcon.gameObject;
                }
            }
        }
    }

    private void Update()
    {
        if (isBleeding && Time.time >= bleedEndTime)
        {
            RemoveBleed();
        }
    }

    public void ApplyBleed(float duration, float dodgeDamage)
    {
        isBleeding = true;
        bleedEndTime = Time.time + duration;
        damagePerDodge = dodgeDamage;

        if (bleedIconUI != null)
        {
            bleedIconUI.SetActive(true);
        }

        Debug.Log($"<color=red>Bleed applied! Duration: {duration}s, Damage per dodge: {dodgeDamage}</color>");
    }

    public void RemoveBleed()
    {
        isBleeding = false;

        if (bleedIconUI != null)
        {
            bleedIconUI.SetActive(false);
        }

        Debug.Log("<color=green>Bleed expired!</color>");
    }

    public void OnEnemyDodge()
    {
        if (!isBleeding) return;

        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damagePerDodge);
            Debug.Log($"<color=red>Enemy took {damagePerDodge} bleed damage from dodging!</color>");
        }
    }

    public bool IsBleeding()
    {
        return isBleeding;
    }

    public float GetBleedTimeRemaining()
    {
        return Mathf.Max(0, bleedEndTime - Time.time);
    }
}
