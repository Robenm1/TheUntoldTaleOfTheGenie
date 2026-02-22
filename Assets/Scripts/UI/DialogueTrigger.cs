using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private bool startOnSceneLoad = true;
    [SerializeField] private float delayBeforeStart = 0.5f;

    private DialogueManager dialogueManager;
    private bool hasTriggered = false;

    private void Start()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();

        if (dialogueManager == null)
        {
            Debug.LogError("DialogueManager not found in scene!");
            return;
        }

        if (startOnSceneLoad)
        {
            Invoke(nameof(TriggerDialogue), delayBeforeStart);
        }
    }

    public void TriggerDialogue()
    {
        if (hasTriggered) return;

        if (dialogueManager != null)
        {
            dialogueManager.StartDialogue();
            hasTriggered = true;
        }
    }
}
