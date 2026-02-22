using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        public Speaker speaker;
        [TextArea(3, 5)]
        public string text;
    }

    public enum Speaker
    {
        Player,
        Enemy
    }

    [Header("Dialogue Lines")]
    [SerializeField] private DialogueLine[] dialogueLines;

    [Header("UI References")]
    [SerializeField] private GameObject chatCanvas;
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private GameObject playerBubble;
    [SerializeField] private GameObject enemyBubble;
    [SerializeField] private TextMeshProUGUI playerText;
    [SerializeField] private TextMeshProUGUI enemyText;

    [Header("Settings")]
    [SerializeField] private float textSpeed = 0.05f;
    [SerializeField] private bool skipToEndOnInput = true;

    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;
    private Coroutine typingCoroutine;

    private void Start()
    {
        if (chatCanvas != null) chatCanvas.SetActive(false);
        if (playerBubble != null) playerBubble.SetActive(false);
        if (enemyBubble != null) enemyBubble.SetActive(false);
    }

    private void Update()
    {
        if (!dialogueActive) return;

        if (CheckForAnyInput())
        {
            if (isTyping && skipToEndOnInput)
            {
                SkipToEndOfLine();
            }
            else if (!isTyping)
            {
                ShowNextLine();
            }
        }
    }

    public void StartDialogue()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning("No dialogue lines configured!");
            return;
        }

        dialogueActive = true;
        currentLineIndex = 0;

        if (chatCanvas != null) chatCanvas.SetActive(true);
        if (mainCanvas != null) mainCanvas.SetActive(false);

        StopGameplay();

        ShowNextLine();

        Debug.Log("<color=cyan>Dialogue started!</color>");
    }

    private void ShowNextLine()
    {
        if (currentLineIndex >= dialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = dialogueLines[currentLineIndex];

        if (playerBubble != null) playerBubble.SetActive(false);
        if (enemyBubble != null) enemyBubble.SetActive(false);

        if (line.speaker == Speaker.Player)
        {
            if (playerBubble != null) playerBubble.SetActive(true);
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(playerText, line.text));
        }
        else
        {
            if (enemyBubble != null) enemyBubble.SetActive(true);
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(enemyText, line.text));
        }

        currentLineIndex++;
    }

    private IEnumerator TypeText(TextMeshProUGUI textComponent, string text)
    {
        isTyping = true;
        textComponent.text = "";

        foreach (char c in text)
        {
            textComponent.text += c;
            yield return new WaitForSecondsRealtime(textSpeed);
        }

        isTyping = false;
    }

    private void SkipToEndOfLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        DialogueLine line = dialogueLines[currentLineIndex - 1];

        if (line.speaker == Speaker.Player && playerText != null)
        {
            playerText.text = line.text;
        }
        else if (line.speaker == Speaker.Enemy && enemyText != null)
        {
            enemyText.text = line.text;
        }

        isTyping = false;
    }

    private void EndDialogue()
    {
        dialogueActive = false;

        if (chatCanvas != null) chatCanvas.SetActive(false);
        if (mainCanvas != null) mainCanvas.SetActive(true);
        if (playerBubble != null) playerBubble.SetActive(false);
        if (enemyBubble != null) enemyBubble.SetActive(false);

        ResumeGameplay();

        Debug.Log("<color=green>Dialogue ended! Gameplay resumed.</color>");
    }

    private void StopGameplay()
    {
        Time.timeScale = 0f;

        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null) playerMovement.enabled = false;

        PlayerCombat playerCombat = FindObjectOfType<PlayerCombat>();
        if (playerCombat != null) playerCombat.enabled = false;

        PlayerLightAttack playerLight = FindObjectOfType<PlayerLightAttack>();
        if (playerLight != null) playerLight.enabled = false;

        GenieAbility1 ability1 = FindObjectOfType<GenieAbility1>();
        if (ability1 != null) ability1.enabled = false;

        GenieAbility2 ability2 = FindObjectOfType<GenieAbility2>();
        if (ability2 != null) ability2.enabled = false;

        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemy in enemies)
        {
            enemy.enabled = false;
        }

        Debug.Log("<color=yellow>Gameplay stopped for dialogue</color>");
    }

    private void ResumeGameplay()
    {
        Time.timeScale = 1f;

        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null) playerMovement.enabled = true;

        PlayerCombat playerCombat = FindObjectOfType<PlayerCombat>();
        if (playerCombat != null) playerCombat.enabled = true;

        PlayerLightAttack playerLight = FindObjectOfType<PlayerLightAttack>();
        if (playerLight != null) playerLight.enabled = true;

        GenieAbility1 ability1 = FindObjectOfType<GenieAbility1>();
        if (ability1 != null) ability1.enabled = true;

        GenieAbility2 ability2 = FindObjectOfType<GenieAbility2>();
        if (ability2 != null) ability2.enabled = true;

        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemy in enemies)
        {
            enemy.enabled = true;
        }

        Debug.Log("<color=green>Gameplay resumed</color>");
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

    public bool IsDialogueActive()
    {
        return dialogueActive;
    }
}
