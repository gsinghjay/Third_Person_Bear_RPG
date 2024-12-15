using UnityEngine;
using Yarn.Unity;

public class YarnDialogController : MonoBehaviour
{
    public static YarnDialogController Instance { get; private set; }
    
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private GameObject dialoguePanel;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void StartDialogue(string nodeName)
    {
        dialoguePanel.SetActive(true);
        dialogueRunner.StartDialogue(nodeName);
    }
    
    [YarnCommand("completeQuest")]
    public void CompleteQuest()
    {
        // We'll implement this later when we add the quest system
        Debug.Log("Quest completed!");
    }
}
