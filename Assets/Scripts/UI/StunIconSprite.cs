using UnityEngine;

public class StunIconSprite : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        transform.localPosition = offset;

        Debug.Log($"<color=cyan>StunIconSprite initialized at local offset: {offset}</color>");
    }

    private void LateUpdate()
    {
        transform.localPosition = offset;
    }

    public void ShowIcon()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            Debug.Log("<color=magenta>Stun sprite shown!</color>");
        }
    }

    public void HideIcon()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
            Debug.Log("<color=green>Stun sprite hidden!</color>");
        }
    }

    public bool IsIconVisible()
    {
        return spriteRenderer != null && spriteRenderer.enabled;
    }
}
