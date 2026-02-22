using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform titleTransform;
    [SerializeField] private GameObject[] pressToStartTexts;
    [SerializeField] private GameObject[] menuButtons;
    [SerializeField] private DoorAnimation doorAnimation;
    [SerializeField] private BGMManager bgmManager;
    [SerializeField] private Camera mainCamera;

    [Header("Title Animation")]
    [SerializeField] private Vector2 titleTopPosition = new Vector2(0, 300);
    [SerializeField] private float titleTopScale = 0.5f;
    [SerializeField] private float titleMoveUpDuration = 1f;

    [Header("Audio")]
    [SerializeField] private AudioClip pressStartSound;
    [SerializeField] private AudioClip playButtonSound;
    [SerializeField][Range(0f, 1f)] private float soundVolume = 1f;

    [Header("Camera Zoom")]
    [SerializeField] private float zoomTargetSize = 3f;
    [SerializeField] private float zoomDuration = 1.5f;
    [SerializeField] private Vector3 zoomTargetPosition = Vector3.zero;

    [Header("Button Animation")]
    [SerializeField] private float buttonFadeOutDuration = 0.5f;

    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string instructionsSceneName = "InstructionsScene";

    [Header("Input")]
    [SerializeField] private bool canAcceptInput = false;

    private Vector2 titleOriginalPosition;
    private Vector3 titleOriginalScale;
    private CanvasGroup[] buttonCanvasGroups;
    private bool buttonsVisible = false;
    private bool doorsOpened = false;
    private bool isAnimatingTitle = false;
    private float titleAnimationProgress = 0f;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

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
                button.SetActive(true);
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
                cg.alpha = 1f;
            }
        }
    }

    private void Update()
    {
        if (canAcceptInput && !doorsOpened && CheckForAnyInput())
        {
            OnPressToStartPressed();
        }

        if (isAnimatingTitle)
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
        Debug.Log("<color=cyan>Press to start enabled!</color>");
    }

    private void OnPressToStartPressed()
    {
        doorsOpened = true;
        canAcceptInput = false;

        PlaySound(pressStartSound);

        foreach (GameObject pressText in pressToStartTexts)
        {
            if (pressText != null)
            {
                pressText.SetActive(false);
            }
        }

        isAnimatingTitle = true;
        titleAnimationProgress = 0f;

        Debug.Log("<color=yellow>Press to start activated! Moving title up and opening doors...</color>");
    }

    private void AnimateTitle()
    {
        titleAnimationProgress += Time.deltaTime / titleMoveUpDuration;
        float t = Mathf.Clamp01(titleAnimationProgress);
        float easedT = EaseInOutCubic(t);

        Vector2 newPosition = Vector2.Lerp(titleOriginalPosition, titleTopPosition, easedT);
        float newScale = Mathf.Lerp(1f, titleTopScale, easedT);

        titleTransform.anchoredPosition = newPosition;
        titleTransform.localScale = titleOriginalScale * newScale;

        if (t >= 1f)
        {
            isAnimatingTitle = false;
            OpenDoorsAndEnableButtons();
            Debug.Log("<color=green>Title moved to top!</color>");
        }
    }

    private void OpenDoorsAndEnableButtons()
    {
        if (doorAnimation != null)
        {
            doorAnimation.OpenDoorsOnly();
        }

        StartCoroutine(EnableButtonsAfterDoors());
    }

    private IEnumerator EnableButtonsAfterDoors()
    {
        yield return new WaitForSeconds(1.5f);

        buttonsVisible = true;
        Debug.Log("<color=green>Buttons are now clickable!</color>");
    }

    public void OnPlayButtonPressed()
    {
        if (!buttonsVisible) return;

        PlaySound(playButtonSound);

        Debug.Log("<color=yellow>Play button pressed! Starting game sequence...</color>");
        StartCoroutine(PlayButtonSequence());
    }

    public void OnInstructionsButtonPressed()
    {
        if (!buttonsVisible) return;

        Debug.Log("<color=cyan>Instructions button pressed! Loading instructions...</color>");
        StartCoroutine(InstructionsButtonSequence());
    }

    public void OnExitButtonPressed()
    {
        Debug.Log("<color=red>Exit button pressed! Quitting game...</color>");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, soundVolume);
        }
    }

    private IEnumerator PlayButtonSequence()
    {
        buttonsVisible = false;

        StartCoroutine(FadeOutButtons());
        StartCoroutine(ZoomCamera());

        yield return new WaitForSeconds(zoomDuration + 0.5f);

        SceneManager.LoadScene(gameSceneName);
    }

    private IEnumerator InstructionsButtonSequence()
    {
        buttonsVisible = false;

        yield return StartCoroutine(FadeOutButtons());

        if (bgmManager != null)
        {
            bgmManager.StopMusic(fade: true);
        }

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene(instructionsSceneName);
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

    private IEnumerator ZoomCamera()
    {
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera is null!");
            yield break;
        }

        float startSize = mainCamera.orthographicSize;
        Vector3 startPosition = mainCamera.transform.position;

        Debug.Log($"<color=cyan>Camera zooming from size {startSize} to {zoomTargetSize}...</color>");
        Debug.Log($"<color=cyan>Camera zooming from position {startPosition} to {zoomTargetPosition}...</color>");

        float elapsed = 0f;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / zoomDuration);
            float easedProgress = EaseInOutCubic(progress);

            mainCamera.orthographicSize = Mathf.Lerp(startSize, zoomTargetSize, easedProgress);
            mainCamera.transform.position = Vector3.Lerp(startPosition, zoomTargetPosition, easedProgress);

            yield return null;
        }

        mainCamera.orthographicSize = zoomTargetSize;
        mainCamera.transform.position = zoomTargetPosition;

        Debug.Log($"<color=green>Camera zoom complete! Final size: {mainCamera.orthographicSize}, Final position: {mainCamera.transform.position}</color>");
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
