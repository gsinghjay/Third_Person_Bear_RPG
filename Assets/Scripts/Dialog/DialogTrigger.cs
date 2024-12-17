using UnityEngine;
using Yarn.Unity;
using TMPro;

public class DialogTrigger : MonoBehaviour
{
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TextMeshProUGUI promptText;
    private bool isInRange = false;

    private void Start()
    {
        if (dialogueRunner == null)
            dialogueRunner = FindObjectOfType<DialogueRunner>();
            
        // Initialize prompt as hidden
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
            
        if (promptText != null)
            promptText.text = "Press E to talk";
    }

    private void Update()
    {
        // Only handle E for starting dialogue, not continuing
        if (isInRange && Input.GetKeyDown(KeyCode.E) && !dialogueRunner.IsDialogueRunning)
        {
            YarnDialogController.Instance.StartNPCDialogue();
            // Hide prompt when dialogue starts
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = true;
            // Show prompt when player is in range
            if (interactionPrompt != null && !dialogueRunner.IsDialogueRunning)
                interactionPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = false;
            // Hide prompt when player leaves range
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
    }
}
