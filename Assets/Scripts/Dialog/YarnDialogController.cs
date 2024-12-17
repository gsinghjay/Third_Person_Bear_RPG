using UnityEngine;
using Yarn.Unity;
using System.Linq;

public class YarnDialogController : MonoBehaviour
{
    public static YarnDialogController Instance { get; private set; }
    
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private QuestManager questManager;
    [SerializeField] private QuestUI questUI;
    [SerializeField] private VariableStorageBehaviour variableStorage;
    [SerializeField] private YarnProject yarnProject;
    
    private bool isInDialogue = false;
    private bool isCurrentConversation = false;
    private string currentDialogueNode = "";
    private bool isQuestUIVisible = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeReferences();
            RegisterVariablesAndCommands();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeReferences()
    {
        if (dialogueRunner == null)
            dialogueRunner = FindObjectOfType<DialogueRunner>();
                
        if (questManager == null)
            questManager = FindObjectOfType<QuestManager>();
                
        if (questUI == null)
            questUI = FindObjectOfType<QuestUI>();

        if (variableStorage == null)
            variableStorage = FindObjectOfType<VariableStorageBehaviour>();
                
        if (dialogueRunner != null && dialogueRunner.yarnProject == null)
        {
            Debug.LogError("No Yarn Project assigned to DialogueRunner!");
        }

        if (!ValidateReferences())
        {
            Debug.LogError("Missing required components in YarnDialogController!");
        }
    }

    private void RegisterVariablesAndCommands()
    {
        if (dialogueRunner != null && questManager != null)
        {
            // Set initial variable values
            variableStorage.SetValue("$currentQuestMessage", string.Empty);
            variableStorage.SetValue("$hasActiveQuest", false);
            variableStorage.SetValue("$currentQuestId", string.Empty);
            
            // Register commands
            dialogueRunner.AddCommandHandler<string>("startQuest", StartQuestWithUI);
            dialogueRunner.AddCommandHandler<string>("declineQuest", DeclineQuestWithUI);
            dialogueRunner.AddFunction<string, string>("getQuestMessage", GetActiveQuestMessage);
            
            // Subscribe to dialogue events
            dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
            dialogueRunner.onNodeStart.AddListener(OnNodeStart);
            dialogueRunner.onNodeComplete.AddListener(OnNodeComplete);
        }
    }
    
    private void StartQuestWithUI(string questId)
    {
        isCurrentConversation = true;
        questManager.StartQuest(questId);
        isQuestUIVisible = true;
        variableStorage.SetValue("$currentQuestId", questId);
        variableStorage.SetValue("$hasActiveQuest", true);
        
        if (dialoguePanel != null)
        {
            Invoke(nameof(ShowDialoguePanel), 0.5f);
        }
    }
    
    private void DeclineQuestWithUI(string questId)
    {
        questManager.DeclineQuest(questId);
        isQuestUIVisible = false;
        variableStorage.SetValue("$currentQuestId", string.Empty);
        variableStorage.SetValue("$hasActiveQuest", false);
        
        if (questUI != null)
        {
            questUI.HideQuestPanel();
        }
    }
    
    private string GetActiveQuestMessage(string questId)
    {
        if (QuestManager.Instance.HasActiveQuest())
        {
            var currentQuest = QuestManager.Instance.GetCurrentActiveQuest();
            return currentQuest?.questId switch
            {
                "northwest_arena" => "The northwest arena needs to be cleared first. Focus on defeating those bears!",
                "northeast_arena" => "The fire bears in the northeast arena are waiting. Deal with them before moving on!",
                "boss_arena" => "You must complete the final challenge in the boss arena. Both fire and ice bears await!",
                _ => "Complete your current quest before taking on new challenges!"
            };
        }
        return string.Empty;
    }
    
    private void ShowDialoguePanel()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
    }
    
    public void StartNPCDialogue()
    {
        // Prevent multiple dialogue instances
        if (isInDialogue)
        {
            Debug.Log("Already in dialogue, waiting for completion...");
            return;
        }

        // Validate references
        if (dialogueRunner == null || dialoguePanel == null || questManager == null)
        {
            Debug.LogError("Missing required components!");
            return;
        }

        isInDialogue = true;
        dialoguePanel.SetActive(true);
        
        string nextNode = DetermineNextDialogueNode();
        StartDialogue(nextNode);
    }

    private string DetermineNextDialogueNode()
    {
        // First check if quest is completed
        if (QuestManager.Instance.IsQuestCompleted("boss_arena"))
            return "Victory";
            
        if (QuestManager.Instance.IsQuestCompleted("northeast_arena"))
            return "NortheastComplete";
            
        if (QuestManager.Instance.IsQuestCompleted("northwest_arena"))
            return "NorthwestComplete";
            
        // Then check for active quests
        if (QuestManager.Instance.HasActiveQuest())
            return "QuestInProgress";
            
        // Finally check for declined quests
        if (QuestManager.Instance.WasQuestDeclined("boss_arena"))
            return "NortheastComplete";
            
        if (QuestManager.Instance.WasQuestDeclined("northeast_arena"))
            return "NorthwestComplete";
            
        if (QuestManager.Instance.WasQuestDeclined("northwest_arena"))
            return "ReturnToQuest";
            
        return "FirstNPC";
    }
    
    public void StartDialogue(string nodeName)
    {
        if (string.IsNullOrEmpty(nodeName))
        {
            Debug.LogError("Invalid node name!");
            return;
        }

        if (dialogueRunner.yarnProject == null)
        {
            Debug.LogError("No Yarn Project assigned!");
            return;
        }

        // Check if the node exists in the project
        if (!dialogueRunner.NodeExists(nodeName))
        {
            Debug.LogError($"Node '{nodeName}' not found in Yarn Project!");
            return;
        }

        dialoguePanel.SetActive(true);
        currentDialogueNode = nodeName;
        dialogueRunner.StartDialogue(nodeName);
    }

    private void OnDialogueComplete()
    {
        isInDialogue = false;
        
        // Only hide dialogue panel if quest UI isn't showing
        if (!isQuestUIVisible)
        {
            dialoguePanel.SetActive(false);
        }
        
        currentDialogueNode = "";
        SaveDialogueState();
    }

    private void OnNodeStart(string nodeName)
    {
        currentDialogueNode = nodeName;
        SaveDialogueState();
    }

    private void SaveDialogueState()
    {
        PlayerPrefs.SetString("LastDialogueNode", currentDialogueNode);
        PlayerPrefs.SetInt("QuestUIVisible", isQuestUIVisible ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadDialogueState()
    {
        currentDialogueNode = PlayerPrefs.GetString("LastDialogueNode", "FirstNPC");
        isQuestUIVisible = PlayerPrefs.GetInt("QuestUIVisible", 0) == 1;
        
        // Restore UI state
        if (questUI != null && isQuestUIVisible)
        {
            var currentQuest = QuestManager.Instance.GetCurrentActiveQuest();
            if (currentQuest != null)
            {
                questUI.UpdateQuestUI(currentQuest);
            }
        }
    }

    private void OnEnable()
    {
        LoadDialogueState();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            if (dialogueRunner != null)
            {
                dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);
                dialogueRunner.onNodeStart.RemoveListener(OnNodeStart);
            }
            Instance = null;
        }
        
        SaveDialogueState();
    }

    private bool ValidateReferences()
    {
        return dialogueRunner != null && questManager != null && questUI != null && variableStorage != null;
    }

    private void UpdateDialogueVariables()
    {
        var currentQuest = QuestManager.Instance.GetCurrentActiveQuest();
        variableStorage.SetValue("$hasActiveQuest", currentQuest != null);
        variableStorage.SetValue("$currentQuestId", currentQuest?.questId ?? string.Empty);
        if (currentQuest != null)
        {
            variableStorage.SetValue("$currentQuestMessage", GetActiveQuestMessage(currentQuest.questId));
        }
    }

    private void OnNodeComplete(string nodeName)
    {
        // Handle any cleanup needed when a node completes
        if (isCurrentConversation)
        {
            UpdateDialogueVariables();
        }
    }
}
