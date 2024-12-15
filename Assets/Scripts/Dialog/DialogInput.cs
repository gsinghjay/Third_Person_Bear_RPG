using UnityEngine;
using Yarn.Unity;

public class DialogueInput : MonoBehaviour
{
    private DialogueRunner dialogueRunner;
    private LineView lineView;

    private void Start()
    {
        dialogueRunner = FindObjectOfType<DialogueRunner>();
        lineView = FindObjectOfType<LineView>();
    }

    private void Update()
    {
        // Only allow input when dialogue is running
        if (dialogueRunner.IsDialogueRunning)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                lineView.OnContinueClicked();
            }
        }
    }
}
