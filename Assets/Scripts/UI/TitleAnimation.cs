using UnityEngine;

public class TitleAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 1.2f;
    [SerializeField] private float overshoot = 1.2f;
    [SerializeField] private float delayBeforeStart = 0.2f;
    [SerializeField] private bool useBounce = true;

    private RectTransform rectTransform;
    private Vector3 targetScale;
    private float elapsedTime = 0f;
    private bool isAnimating = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform != null)
        {
            targetScale = rectTransform.localScale;
            rectTransform.localScale = Vector3.zero;
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
        Debug.Log("<color=cyan>Title animation started!</color>");
    }

    private void Update()
    {
        if (!isAnimating) return;

        elapsedTime += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsedTime / animationDuration);

        float scale;
        if (useBounce)
        {
            scale = EaseOutBack(progress, overshoot);
        }
        else
        {
            scale = EaseOutCubic(progress);
        }

        rectTransform.localScale = targetScale * scale;

        if (progress >= 1f)
        {
            isAnimating = false;
            rectTransform.localScale = targetScale;
            Debug.Log("<color=green>Title animation complete!</color>");
        }
    }

    private float EaseOutBack(float t, float overshoot)
    {
        float c1 = 1.70158f * overshoot;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}
