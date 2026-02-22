using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class MenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform titleTransform;
    [SerializeField] private GameObject[] pressToStartTexts;
    [SerializeField] private GameObject[] menuButtons;
    [SerializeField] private DoorAnimation doorAnimation;
    [SerializeField] private BGMManager bgmManager;

    [Header("Title Animation")]
    [SerializeField] private Vector2 titleTopPosition = new Vector2(0, 300);
    [SerializeField] private float titleTopScale = 0.5f;
    [SerializeField] private float titleMoveUpDuration = 1f;

    [Header("Button Animation")]
    [SerializeField] private float buttonFadeInDuration = 0.8f;
    [SerializeField] private float delayBetweenButtons = 0.1f;
    [SerializeField] private float buttonFadeOutDuration = 0.5f;

    [Header("Input")]
    [SerializeField] private bool canAcceptInput = false;

    private Vector2 titleOriginalPosition;
    private Vector3 titleOriginalScale;
    private bool isAnimating = false;
    private float animationProgress = 0f;
    private CanvasGroup[] buttonCanvasGroups;
    private bool buttonsVisible = false;

    private void Awake()
    {
        if (titleTransform != null)
        {
            titleOriginalPosition = titleTransform.anchoredPosition;
            titleOriginalScale = titleTransform.localScale;
        }

        SetupButtonCanvasGroups();

        foreach (GameObject button in menuButtons)
        {
            if (button != null)
            {
                button.SetActive(false);
            }
        }
    }

    private void SetupButtonCanvasGroups()
    {
        buttonCanvasGroups = new CanvasGroup[menuButtons.Length];

        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] != null)
            {
                CanvasGroup cg = menuButtons[i].GetComponent<CanvasGroup>();
                if (cg == null)
                {
                    cg = menuButtons[i].AddComponent<CanvasGroup>();
                }
                buttonCanvasGroups[i] = cg;
                cg.alpha = 0f;
            }
        }
    }

    private void Update()
    {
        if (canAcceptInput && !isAnimating && CheckForAnyInput())
        {
            StartTitleTransition();
        }

        if (isAnimating)
        {
            AnimateTitle();
        }
    }

    private bool CheckForAnyInput()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            return true;
        }

        if (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame ||
            Mouse.current.rightButton.wasPressedThisFrame))
        {
            return true;
        }

        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            return true;
        }

        return false;
    }

    public void EnableInput()
    {
        canAcceptInput = true;
        Debug.Log("<color=cyan>Menu input enabled! Press any key to continue...</color>");
    }

    private void StartTitleTransition()
    {
        isAnimating = true;
        animationProgress = 0f;
        canAcceptInput = false;

        foreach (GameObject pressText in pressToStartTexts)
        {
            if (pressText != null)
            {
                pressText.SetActive(false);
            }
        }

        Debug.Log("<color=yellow>Starting title transition to top...</color>");
    }

    private void AnimateTitle()
    {
        animationProgress += Time.deltaTime / titleMoveUpDuration;
        float t = Mathf.Clamp01(animationProgress);
        float easedT = EaseInOutCubic(t);

        Vector2 newPosition = Vector2.Lerp(titleOriginalPosition, titleTopPosition, easedT);
        float newScale = Mathf.Lerp(1f, titleTopScale, easedT);

        titleTransform.anchoredPosition = newPosition;
        titleTransform.localScale = titleOriginalScale * newScale;

        if (t >= 1f)
        {
            isAnimating = false;
            ShowMenuButtons();
            Debug.Log("<color=green>Title transition complete!</color>");
        }
    }

    private void ShowMenuButtons()
    {
        StartCoroutine(FadeInButtons());
    }

    private IEnumerator FadeInButtons()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] != null)
            {
                menuButtons[i].SetActive(true);
                StartCoroutine(FadeInButton(buttonCanvasGroups[i], i));

                if (i < menuButtons.Length - 1)
                {
                    yield return new WaitForSeconds(delayBetweenButtons);
                }
            }
        }

        buttonsVisible = true;
    }

    private IEnumerator FadeInButton(CanvasGroup canvasGroup, int buttonIndex)
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        canvasGroup.alpha = 0f;

        while (elapsed < buttonFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / buttonFadeInDuration);
            canvasGroup.alpha = EaseOutCubic(progress);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        Debug.Log($"<color=green>Button {buttonIndex} fade-in complete!</color>");
    }

    public void OnPlayButtonPressed()
    {
        if (!buttonsVisible) return;

        Debug.Log("<color=yellow>Play button pressed! Starting exit sequence...</color>");
        StartCoroutine(PlayButtonSequence());
    }

    public void OnExitButtonPressed()
    {
        Debug.Log("<color=red>Exit button pressed! Quitting game...</color>");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private IEnumerator PlayButtonSequence()
    {
        buttonsVisible = false;

        yield return StartCoroutine(FadeOutButtons());

        if (bgmManager != null)
        {
            bgmManager.StopMusic(fade: true);
            Debug.Log("<color=yellow>Fading out BGM as doors open...</color>");
        }

        if (doorAnimation != null)
        {
            doorAnimation.OpenDoors();
        }
        else
        {
            Debug.LogError("DoorAnimation reference is missing!");
        }
    }

    private IEnumerator FadeOutButtons()
    {
        float elapsed = 0f;

        while (elapsed < buttonFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / buttonFadeOutDuration);
            float alpha = 1f - EaseOutCubic(progress);

            foreach (CanvasGroup cg in buttonCanvasGroups)
            {
                if (cg != null)
                {
                    cg.alpha = alpha;
                }
            }

            yield return null;
        }

        foreach (CanvasGroup cg in buttonCanvasGroups)
        {
            if (cg != null)
            {
                cg.alpha = 0f;
            }
        }

        Debug.Log("<color=green>Buttons faded out!</color>");
    }

    private float EaseInOutCubic(float t)
    {
        return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}
