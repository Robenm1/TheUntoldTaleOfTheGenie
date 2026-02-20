using UnityEngine;

public class VFXFlipper : MonoBehaviour
{
    private Vector3 originalScale;
    private bool hasRecordedScale = false;
    private Transform playerTransform;
    private Transform playerSprite;

    private void Awake()
    {
        RecordOriginalScale();
        FindPlayer();
        AdjustToPlayerFacing();
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerSprite = player.transform.Find("GenieSprite");
        }
    }

    private void AdjustToPlayerFacing()
    {
        if (playerSprite == null) return;

        float playerFacingDirection = Mathf.Sign(playerSprite.localScale.x);

        if (playerFacingDirection < 0)
        {
            Vector3 newScale = transform.localScale;
            newScale.x *= -1;
            transform.localScale = newScale;
            Debug.Log($"<color=cyan>VFX {gameObject.name} flipped for left-facing player! Original: {originalScale.x}, New: {newScale.x}</color>");
        }
        else
        {
            Debug.Log($"<color=green>VFX {gameObject.name} keeping default for right-facing player! Scale.x: {transform.localScale.x}</color>");
        }
    }

    public void RecordOriginalScale()
    {
        if (!hasRecordedScale)
        {
            originalScale = transform.localScale;
            hasRecordedScale = true;
        }
    }

    public void FlipX()
    {
        if (!hasRecordedScale)
        {
            RecordOriginalScale();
        }

        Vector3 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;

        Debug.Log($"<color=yellow>VFX {gameObject.name} flipped! Original X: {originalScale.x}, New X: {newScale.x}</color>");
    }

    public float GetOriginalScaleX()
    {
        return originalScale.x;
    }
}
