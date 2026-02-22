using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class VictoryScreenManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas endGameCanvas;
    [SerializeField] private GameObject panel;
    [SerializeField] private RectTransform victoryText1;
    [SerializeField] private RectTransform victoryText2;
    [SerializeField] private Button returnButton;

    [Header("Animation Settings")]
    [SerializeField] private bool fadePanelIn = true;
    [SerializeField] private float panelFadeDuration = 0.5f;
    [SerializeField] private float textGrowDuration = 1f;
    [SerializeField] private float delayBetweenTexts = 0.2f;
    [SerializeField] private float buttonFadeInDuration = 0.8f;
    [SerializeField] private AnimationCurve textGrowCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private CanvasGroup panelCanvasGroup;
    private CanvasGroup buttonCanvasGroup;
    private Vector3 text1OriginalScale;
    private Vector3 text2OriginalScale;

    private void Awake()
    {
        if (endGameCanvas != null)
        {
            endGameCanvas.gameObject.SetActive(false);
        }

        if (panel != null)
        {
            panelCanvasGroup = panel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = panel.AddComponent<CanvasGroup>();
            }
        }

        if (returnButton != null)
        {
            buttonCanvasGroup = returnButton.GetComponent<CanvasGroup>();
            if (buttonCanvasGroup == null)
            {
                buttonCanvasGroup = returnButton.gameObject.AddComponent<CanvasGroup>();
            }
            buttonCanvasGroup.alpha = 0f;

            returnButton.onClick.AddListener(OnReturnButtonPressed);
        }

        if (victoryText1 != null)
        {
            text1OriginalScale = victoryText1.localScale;
        }

        if (victoryText2 != null)
        {
            text2OriginalScale = victoryText2.localScale;
        }
    }

    public void ShowVictoryScreen()
    {
        if (endGameCanvas != null)
        {
            endGameCanvas.gameObject.SetActive(true);
        }

        Time.timeScale = 0f;
        Debug.Log("<color=yellow>Game paused! Victory screen showing...</color>");

        StartCoroutine(VictorySequence());
    }

    private IEnumerator VictorySequence()
    {
        if (victoryText1 != null) victoryText1.localScale = Vector3.zero;
        if (victoryText2 != null) victoryText2.localScale = Vector3.zero;

        if (fadePanelIn && panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;
            yield return StartCoroutine(FadeInPanel());
        }
        else if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 1f;
        }

        if (victoryText1 != null)
        {
            yield return StartCoroutine(GrowText(victoryText1, text1OriginalScale));
        }

        yield return new WaitForSecondsRealtime(delayBetweenTexts);

        if (victoryText2 != null)
        {
            yield return StartCoroutine(GrowText(victoryText2, text2OriginalScale));
        }

        if (buttonCanvasGroup != null)
        {
            yield return StartCoroutine(FadeInButton());
        }

        Debug.Log("<color=green>Victory sequence complete!</color>");
    }

    private IEnumerator FadeInPanel()
    {
        float elapsed = 0f;

        while (elapsed < panelFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / panelFadeDuration);
            panelCanvasGroup.alpha = progress;
            yield return null;
        }

        panelCanvasGroup.alpha = 1f;
        Debug.Log("<color=green>Panel faded in!</color>");
    }

    private IEnumerator GrowText(RectTransform textTransform, Vector3 targetScale)
    {
        float elapsed = 0f;

        while (elapsed < textGrowDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / textGrowDuration);
            float curveValue = textGrowCurve.Evaluate(progress);

            textTransform.localScale = targetScale * curveValue;
            yield return null;
        }

        textTransform.localScale = targetScale;
        Debug.Log($"<color=green>{textTransform.name} grew to full size!</color>");
    }

    private IEnumerator FadeInButton()
    {
        float elapsed = 0f;

        while (elapsed < buttonFadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / buttonFadeInDuration);
            buttonCanvasGroup.alpha = EaseOutCubic(progress);
            yield return null;
        }

        buttonCanvasGroup.alpha = 1f;
        Debug.Log("<color=green>Return button faded in!</color>");
    }

    private void OnReturnButtonPressed()
    {
        Debug.Log("<color=cyan>Returning to main menu...</color>");
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}
