using UnityEngine;

public class SlashVFX : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lifetime = 0.5f;
    [SerializeField] private bool followParent = true;

    private Transform parentTransform;
    private Vector3 localOffset;

    private void Start()
    {
        if (followParent && transform.parent != null)
        {
            parentTransform = transform.parent;
            localOffset = transform.localPosition;
        }

        Destroy(gameObject, lifetime);
    }

    private void LateUpdate()
    {
        if (followParent && parentTransform != null)
        {
            transform.position = parentTransform.TransformPoint(localOffset);
        }
    }
}
