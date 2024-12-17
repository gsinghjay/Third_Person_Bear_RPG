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
        // Only handle space bar input when dialogue is running
        if (dialogueRunner != null && dialogueRunner.IsDialogueRunning)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (lineView != null)
                {
                    lineView.OnContinueClicked();
                }
            }
        }
    }
}
