using UnityEngine;

public class UIFlipper : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerSprite;
    [SerializeField] private RectTransform uiElement;

    [Header("Settings")]
    [SerializeField] private bool flipOnStart = false;

    private float originalScaleX;
    private float lastPlayerFacing = 1f;

    private void Awake()
    {
        if (playerSprite == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerSprite = player.transform.Find("GenieSprite");
            }
        }

        if (uiElement == null)
        {
            uiElement = GetComponent<RectTransform>();
        }

        if (uiElement != null)
        {
            originalScaleX = Mathf.Abs(uiElement.localScale.x);
        }
    }

    private void Start()
    {
        if (flipOnStart && playerSprite != null)
        {
            lastPlayerFacing = Mathf.Sign(playerSprite.localScale.x);
            UpdateFlip();
        }
    }

    private void Update()
    {
        if (playerSprite == null || uiElement == null) return;

        float currentFacing = Mathf.Sign(playerSprite.localScale.x);

        if (currentFacing != lastPlayerFacing)
        {
            lastPlayerFacing = currentFacing;
            UpdateFlip();
        }
    }

    private void UpdateFlip()
    {
        if (uiElement == null) return;

        Vector3 newScale = uiElement.localScale;
        newScale.x = originalScaleX * lastPlayerFacing;
        uiElement.localScale = newScale;

        Debug.Log($"<color=cyan>UI {uiElement.name} flipped! Facing: {(lastPlayerFacing > 0 ? "RIGHT" : "LEFT")}, Scale.x: {newScale.x}</color>");
    }

    public void FlipToMatchPlayer()
    {
        if (playerSprite != null)
        {
            lastPlayerFacing = Mathf.Sign(playerSprite.localScale.x);
            UpdateFlip();
        }
    }
}
