using UnityEngine;

public class SlideUpAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 1.5f;
    [SerializeField] private float slideDistance = 500f;
    [SerializeField] private float delayBeforeStart = 0.5f;
    [SerializeField] private AnimationType animationType = AnimationType.EaseOutBack;
    [SerializeField] private float overshoot = 1.1f;

    public enum AnimationType
    {
        Linear,
        EaseOut,
        EaseOutBack
    }

    private RectTransform rectTransform;
    private Vector2 targetPosition;
    private Vector2 startPosition;
    private float elapsedTime = 0f;
    private bool isAnimating = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform != null)
        {
            targetPosition = rectTransform.anchoredPosition;
            startPosition = new Vector2(targetPosition.x, targetPosition.y - slideDistance);
            rectTransform.anchoredPosition = startPosition;
        }
    }

    private void Start()
    {
        Invoke(nameof(StartAnimation), delayBeforeStart);
    }

    private void StartAnimation()
    {
        isAnimating = true;
        elapsedTime = 0f;
        Debug.Log("<color=cyan>Slide-up animation started!</color>");
    }

    private void Update()
    {
        if (!isAnimating) return;

        elapsedTime += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsedTime / animationDuration);

        float easedProgress = animationType switch
        {
            AnimationType.Linear => progress,
            AnimationType.EaseOut => EaseOutCubic(progress),
            AnimationType.EaseOutBack => EaseOutBack(progress, overshoot),
            _ => progress
        };

        rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, easedProgress);

        if (progress >= 1f)
        {
            isAnimating = false;
            rectTransform.anchoredPosition = targetPosition;
            Debug.Log("<color=green>Slide-up animation complete!</color>");
        }
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    private float EaseOutBack(float t, float overshoot)
    {
        float c1 = 1.70158f * overshoot;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
