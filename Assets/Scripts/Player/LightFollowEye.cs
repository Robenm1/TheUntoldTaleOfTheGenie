using UnityEngine;

public class LightFollowEye : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform eyeTarget;
    [SerializeField] private Vector2 localOffset = Vector2.zero;
    [SerializeField] private bool followX = true;
    [SerializeField] private bool followY = true;

    private void LateUpdate()
    {
        if (eyeTarget != null)
        {
            Vector3 targetPosition = eyeTarget.position + (Vector3)localOffset;

            Vector3 newPosition = transform.position;

            if (followX)
                newPosition.x = targetPosition.x;

            if (followY)
                newPosition.y = targetPosition.y;

            transform.position = newPosition;
        }
    }
}
