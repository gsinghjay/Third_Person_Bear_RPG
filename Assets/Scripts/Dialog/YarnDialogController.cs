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
    
    public void StartNPCDialogue()
    {
        dialoguePanel.SetActive(true);
        
        // Check quest progress and start appropriate dialogue
        if (QuestManager.Instance.IsQuestCompleted("boss_arena"))
        {
            dialogueRunner.StartDialogue("Victory");
        }
        else if (QuestManager.Instance.IsQuestCompleted("northeast_arena"))
        {
            dialogueRunner.StartDialogue("NortheastComplete");
        }
        else if (QuestManager.Instance.IsQuestCompleted("northwest_arena"))
        {
            dialogueRunner.StartDialogue("NorthwestComplete");
        }
        else
        {
            dialogueRunner.StartDialogue("FirstNPC");
        }
    }
    
    public void StartDialogue(string nodeName)
    {
        dialoguePanel.SetActive(true);
        dialogueRunner.StartDialogue(nodeName);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
