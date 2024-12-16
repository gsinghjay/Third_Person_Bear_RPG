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
        
        // Check quest states in proper sequential order
        if (!QuestManager.Instance.IsQuestActive("northwest_arena") && 
            !QuestManager.Instance.IsQuestCompleted("northwest_arena"))
        {
            dialogueRunner.StartDialogue("FirstNPC");
        }
        else if (QuestManager.Instance.IsQuestCompleted("northwest_arena") && 
                 !QuestManager.Instance.IsQuestActive("northeast_arena"))
        {
            dialogueRunner.StartDialogue("NorthwestComplete");
        }
        else if (QuestManager.Instance.IsQuestCompleted("northeast_arena") && 
                 !QuestManager.Instance.IsQuestActive("boss_arena"))
        {
            dialogueRunner.StartDialogue("NortheastComplete");
        }
        else if (QuestManager.Instance.IsQuestCompleted("boss_arena"))
        {
            dialogueRunner.StartDialogue("Victory");
        }
        else
        {
            // If a quest is in progress, start the appropriate dialogue
            if (QuestManager.Instance.IsQuestActive("boss_arena"))
            {
                dialogueRunner.StartDialogue("NortheastComplete");
            }
            else if (QuestManager.Instance.IsQuestActive("northeast_arena"))
            {
                dialogueRunner.StartDialogue("NorthwestComplete");
            }
            else if (QuestManager.Instance.IsQuestActive("northwest_arena"))
            {
                dialogueRunner.StartDialogue("FirstNPC");
            }
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
