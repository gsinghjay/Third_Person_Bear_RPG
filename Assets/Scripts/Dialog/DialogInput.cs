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
}
