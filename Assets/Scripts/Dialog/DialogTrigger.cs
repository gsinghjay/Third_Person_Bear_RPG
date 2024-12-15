using UnityEngine;
using Yarn.Unity;

public class DialogTrigger : MonoBehaviour
{
    [SerializeField] private string dialogueNodeName = "FirstNPC";
    [SerializeField] private DialogueRunner dialogueRunner;
    private bool isInRange = false;

    private void Start()
    {
        if (dialogueRunner == null)
        {
            dialogueRunner = FindObjectOfType<DialogueRunner>();
        }
    }

    private void Update()
    {
        // Start dialogue when E is pressed and player is in range
        if (isInRange && Input.GetKeyDown(KeyCode.E) && !dialogueRunner.IsDialogueRunning)
        {
            dialogueRunner.StartDialogue(dialogueNodeName);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = false;
        }
    }
}
