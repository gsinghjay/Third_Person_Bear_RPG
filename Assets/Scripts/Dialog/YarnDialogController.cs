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
            
            // Register only the commands we need
            dialogueRunner.AddCommandHandler<string>("startQuest", StartQuest);
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
    
    private void StartQuest(string questId)
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.StartQuest(questId);
            Debug.Log($"Starting quest: {questId}");
        }
        else
        {
            Debug.LogError("QuestManager instance not found!");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        // Only remove commands we actually registered
        if (dialogueRunner != null)
        {
            dialogueRunner.RemoveCommandHandler("startQuest");
        }
    }
}
