// Assets/Scripts/UI/QuestUI.cs
using UnityEngine;
using TMPro;

public class QuestUI : MonoBehaviour
{
    [SerializeField] private GameObject questPanel;
    [SerializeField] private TextMeshProUGUI questTitle;
    [SerializeField] private TextMeshProUGUI questProgress;
    [SerializeField] private TextMeshProUGUI questDescription;
    
    // Add victory panel reference
    [SerializeField] private GameObject victoryPanel;

    private void Start()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted += UpdateQuestUI;
            QuestManager.Instance.OnQuestUpdated += UpdateQuestUI;
            QuestManager.Instance.OnQuestCompleted += OnQuestCompleted;
        }
        else
        {
            Debug.LogError("QuestUI: QuestManager instance not found!");
        }

        // Hide panels initially
        if (questPanel != null)
            questPanel.SetActive(false);
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }

    public void UpdateQuestUI(QuestData quest)
    {
        if (questPanel != null)
        {
            questPanel.SetActive(true);
        }

        if (questTitle != null)
        {
            questTitle.text = quest.title;
        }

        if (questProgress != null)
        {
            questProgress.text = $"Bears defeated: {quest.currentBearKills}/{quest.requiredBearKills}";
        }

        if (questDescription != null)
        {
            questDescription.text = quest.description;
        }
    }

    private void OnQuestCompleted(QuestData quest)
    {
        if (quest.questId == "boss_arena")
        {
            // Show victory panel
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
                questPanel.SetActive(false);
            }
            
            if (questProgress != null)
            {
                questProgress.text = "VICTORY! All arenas cleared!";
            }
            
            // Optional: Add some victory effects or animations here
        }
        else
        {
            if (questProgress != null)
            {
                questProgress.text = "Quest Completed!";
            }
            
            // Hide the panel after a delay for non-boss quests
            Invoke(nameof(HideQuestPanel), 3f);
        }
    }

    public void HideQuestPanel()
    {
        if (questPanel != null)
        {
            questPanel.SetActive(false);
        }
    }

    // Add method to hide victory panel if needed
    public void HideVictoryPanel()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted -= UpdateQuestUI;
            QuestManager.Instance.OnQuestUpdated -= UpdateQuestUI;
            QuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;
        }
    }
}