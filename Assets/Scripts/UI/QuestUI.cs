// Assets/Scripts/UI/QuestUI.cs
using UnityEngine;
using TMPro;

public class QuestUI : MonoBehaviour
{
    [SerializeField] private GameObject questPanel;
    [SerializeField] private TextMeshProUGUI questTitle;
    [SerializeField] private TextMeshProUGUI questProgress;
    [SerializeField] private TextMeshProUGUI questDescription;

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

        // Hide panel initially
        if (questPanel != null)
        {
            questPanel.SetActive(false);
        }
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
        if (questProgress != null)
        {
            questProgress.text = "Quest Completed!";
        }

        // Optional: Hide the panel after a delay
        Invoke(nameof(HideQuestPanel), 3f);
    }

    public void HideQuestPanel()
    {
        if (questPanel != null)
        {
            questPanel.SetActive(false);
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