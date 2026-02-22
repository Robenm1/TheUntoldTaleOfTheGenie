using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InstructionsManager : MonoBehaviour
{
    [Header("Instruction Panels (In Order)")]
    [SerializeField] private GameObject[] instructionPanels;

    [Header("Navigation Buttons")]
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button rightArrowButton;

    [Header("Action Buttons")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button backToMenuButton;

    [Header("Button Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private int currentPanelIndex = 0;
    private bool[] panelsViewed;

    private void Start()
    {
        if (instructionPanels == null || instructionPanels.Length == 0)
        {
            Debug.LogError("No instruction panels assigned!");
            return;
        }

        panelsViewed = new bool[instructionPanels.Length];

        SetupButtons();

        ShowPanel(0);
    }

    private void SetupButtons()
    {
        if (leftArrowButton != null)
        {
            leftArrowButton.onClick.AddListener(PreviousPanel);
        }

        if (rightArrowButton != null)
        {
            rightArrowButton.onClick.AddListener(NextPanel);
        }

        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(StartGame);
        }

        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.AddListener(BackToMenu);
        }
    }

    private void ShowPanel(int index)
    {
        if (index < 0 || index >= instructionPanels.Length) return;

        for (int i = 0; i < instructionPanels.Length; i++)
        {
            if (instructionPanels[i] != null)
            {
                instructionPanels[i].SetActive(i == index);
            }
        }

        currentPanelIndex = index;
        panelsViewed[index] = true;

        UpdateNavigationButtons();
        UpdateStartGameButton();

        Debug.Log($"<color=cyan>Showing panel {index + 1}/{instructionPanels.Length}</color>");
    }

    private void UpdateNavigationButtons()
    {
        if (leftArrowButton != null)
        {
            bool isFirstPanel = currentPanelIndex == 0;
            leftArrowButton.interactable = !isFirstPanel;

            Image leftImage = leftArrowButton.GetComponent<Image>();
            if (leftImage != null)
            {
                leftImage.color = isFirstPanel ? disabledColor : normalColor;
            }
        }

        if (rightArrowButton != null)
        {
            bool isLastPanel = currentPanelIndex == instructionPanels.Length - 1;
            rightArrowButton.interactable = !isLastPanel;

            Image rightImage = rightArrowButton.GetComponent<Image>();
            if (rightImage != null)
            {
                rightImage.color = isLastPanel ? disabledColor : normalColor;
            }
        }
    }

    private void UpdateStartGameButton()
    {
        if (startGameButton == null) return;

        bool allViewed = AllPanelsViewed();
        startGameButton.interactable = allViewed;

        Image buttonImage = startGameButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = allViewed ? normalColor : disabledColor;
        }

        if (allViewed)
        {
            Debug.Log("<color=green>All panels viewed! Start Game button enabled!</color>");
        }
    }

    private bool AllPanelsViewed()
    {
        foreach (bool viewed in panelsViewed)
        {
            if (!viewed) return false;
        }
        return true;
    }

    public void NextPanel()
    {
        if (currentPanelIndex < instructionPanels.Length - 1)
        {
            ShowPanel(currentPanelIndex + 1);
        }
    }

    public void PreviousPanel()
    {
        if (currentPanelIndex > 0)
        {
            ShowPanel(currentPanelIndex - 1);
        }
    }

    public void StartGame()
    {
        if (!AllPanelsViewed())
        {
            Debug.Log("<color=yellow>Please view all instruction panels first!</color>");
            return;
        }

        Debug.Log($"<color=green>Loading {gameSceneName}...</color>");
        SceneManager.LoadScene(gameSceneName);
    }

    public void BackToMenu()
    {
        Debug.Log($"<color=yellow>Returning to {mainMenuSceneName}...</color>");
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
