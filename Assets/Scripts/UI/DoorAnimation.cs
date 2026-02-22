using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorAnimation : MonoBehaviour
{
    [Header("Door References")]
    [SerializeField] private RectTransform leftDoor;
    [SerializeField] private RectTransform rightDoor;

    [Header("Door Animation Settings")]
    [SerializeField] private float doorSlideDistance = 1000f;
    [SerializeField] private float doorOpenDuration = 1.5f;
    [SerializeField] private AnimationCurve doorCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private float delayBeforeSceneLoad = 0.5f;

    private Vector2 leftDoorOriginalPos;
    private Vector2 rightDoorOriginalPos;
    private bool isAnimating = false;

    private void Awake()
    {
        if (leftDoor != null)
        {
            leftDoorOriginalPos = leftDoor.anchoredPosition;
        }

        if (rightDoor != null)
        {
            rightDoorOriginalPos = rightDoor.anchoredPosition;
        }

        if (leftDoor != null) leftDoor.gameObject.SetActive(true);
        if (rightDoor != null) rightDoor.gameObject.SetActive(true);
    }

    public void OpenDoors()
    {
        if (isAnimating) return;

        Debug.Log("<color=yellow>Opening doors...</color>");
        StartCoroutine(OpenDoorsCoroutine());
    }

    private IEnumerator OpenDoorsCoroutine()
    {
        isAnimating = true;
        float elapsed = 0f;

        Vector2 leftDoorTarget = leftDoorOriginalPos + new Vector2(-doorSlideDistance, 0);
        Vector2 rightDoorTarget = rightDoorOriginalPos + new Vector2(doorSlideDistance, 0);

        while (elapsed < doorOpenDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / doorOpenDuration);
            float curveValue = doorCurve.Evaluate(progress);

            if (leftDoor != null)
            {
                leftDoor.anchoredPosition = Vector2.Lerp(leftDoorOriginalPos, leftDoorTarget, curveValue);
            }

            if (rightDoor != null)
            {
                rightDoor.anchoredPosition = Vector2.Lerp(rightDoorOriginalPos, rightDoorTarget, curveValue);
            }

            yield return null;
        }

        if (leftDoor != null) leftDoor.anchoredPosition = leftDoorTarget;
        if (rightDoor != null) rightDoor.anchoredPosition = rightDoorTarget;

        Debug.Log("<color=green>Doors opened!</color>");

        yield return new WaitForSeconds(delayBeforeSceneLoad);

        LoadGameScene();
    }

    private void LoadGameScene()
    {
        Debug.Log($"<color=cyan>Loading scene: {gameSceneName}</color>");
        SceneManager.LoadScene(gameSceneName);
    }
}
