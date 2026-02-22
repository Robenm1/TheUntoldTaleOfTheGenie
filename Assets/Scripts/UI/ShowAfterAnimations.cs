using UnityEngine;

public class ShowAfterAnimations : MonoBehaviour
{
    [Header("Objects to Show")]
    [SerializeField] private GameObject[] objectsToShow;

    [Header("Timing")]
    [SerializeField] private float delayBeforeShow = 3.5f;
    [SerializeField] private bool fadeIn = true;
    [SerializeField] private float fadeInDuration = 1f;

    [Header("References")]
    [SerializeField] private MenuController menuController;

    private CanvasGroup[] canvasGroups;
    private float fadeElapsedTime = 0f;
    private bool isFading = false;

    private void Awake()
    {
        if (objectsToShow == null || objectsToShow.Length == 0)
        {
            Debug.LogWarning("No objects assigned to show!");
            return;
        }

        foreach (GameObject obj in objectsToShow)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        if (fadeIn)
        {
            SetupFadeComponents();
        }
    }

    private void Start()
    {
        Invoke(nameof(ShowObjects), delayBeforeShow);
    }

    private void SetupFadeComponents()
    {
        canvasGroups = new CanvasGroup[objectsToShow.Length];

        for (int i = 0; i < objectsToShow.Length; i++)
        {
            if (objectsToShow[i] != null)
            {
                CanvasGroup cg = objectsToShow[i].GetComponent<CanvasGroup>();
                if (cg == null)
                {
                    cg = objectsToShow[i].AddComponent<CanvasGroup>();
                }
                canvasGroups[i] = cg;
                cg.alpha = 0f;
            }
        }
    }

    private void ShowObjects()
    {
        foreach (GameObject obj in objectsToShow)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        if (fadeIn)
        {
            isFading = true;
            fadeElapsedTime = 0f;
            Debug.Log("<color=cyan>Fading in PressToStart texts...</color>");
        }
        else
        {
            EnableMenuInput();
            Debug.Log("<color=green>PressToStart texts shown!</color>");
        }
    }

    private void Update()
    {
        if (!isFading) return;

        fadeElapsedTime += Time.deltaTime;
        float progress = Mathf.Clamp01(fadeElapsedTime / fadeInDuration);

        foreach (CanvasGroup cg in canvasGroups)
        {
            if (cg != null)
            {
                cg.alpha = progress;
            }
        }

        if (progress >= 1f)
        {
            isFading = false;
            EnableMenuInput();
            Debug.Log("<color=green>PressToStart fade-in complete!</color>");
        }
    }

    private void EnableMenuInput()
    {
        if (menuController != null)
        {
            menuController.EnableInput();
        }
    }
}
