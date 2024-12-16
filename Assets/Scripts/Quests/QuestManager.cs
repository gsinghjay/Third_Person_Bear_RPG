using UnityEngine;
using System.Collections.Generic;
using Enemies.Core;
using Enemies.Types;
using Enemies.Interfaces;
using Yarn.Unity;

public class QuestManager : MonoBehaviour
{
    private static QuestManager _instance;
    public static QuestManager Instance 
    {
        get 
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<QuestManager>();
                if (_instance == null)
                {
                    Debug.LogError("No QuestManager found in scene!");
                }
            }
            return _instance;
        }
    }
    
    private Dictionary<string, QuestData> activeQuests = new Dictionary<string, QuestData>();
    public event System.Action<QuestData> OnQuestStarted;
    public event System.Action<QuestData> OnQuestCompleted;
    public event System.Action<QuestData> OnQuestUpdated;
    
    [SerializeField] private BearSpawner bearSpawner;
    [SerializeField] private DialogueRunner dialogueRunner;

    private bool commandsRegistered = false;

    private Dictionary<string, ArenaSettings> arenaSettings = new();

    private void OnEnable()
    {
        if (dialogueRunner == null)
            dialogueRunner = FindObjectOfType<DialogueRunner>();
            
        RegisterYarnCommands();

        // Subscribe to bear death events using the correct event name
        foreach (var bear in FindObjectsOfType<BearController>())
        {
            bear.OnDeath += HandleBearDeath;
        }
    }

    private void OnDisable()
    {
        UnregisterYarnCommands();

        // Unsubscribe from bear death events using the correct event name
        foreach (var bear in FindObjectsOfType<BearController>())
        {
            bear.OnDeath -= HandleBearDeath;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeArenaSettings();
            InitializeReferences();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeArenaSettings()
    {
        // Northwest Arena (Training Arena)
        arenaSettings["northwest_arena"] = new ArenaSettings
        {
            questId = "northwest_arena",
            name = "Northwest Training Arena",
            position = new Vector2(-35f, 35f),
            radius = 35f,
            normalBearCount = 3,
            fireBearCount = 0,
            iceBearCount = 0
        };

        // Northeast Arena (Fire Arena)
        arenaSettings["northeast_arena"] = new ArenaSettings
        {
            questId = "northeast_arena",
            name = "Northeast Fire Arena",
            position = new Vector2(35f, 35f),
            radius = 35f,
            normalBearCount = 2,
            fireBearCount = 2,
            iceBearCount = 0
        };

        // Boss Arena
        arenaSettings["boss_arena"] = new ArenaSettings
        {
            questId = "boss_arena",
            name = "Boss Arena",
            position = new Vector2(0f, -35f),
            radius = 35f,
            normalBearCount = 1,
            fireBearCount = 2,
            iceBearCount = 2
        };
    }

    private void InitializeReferences()
    {
        if (bearSpawner == null)
        {
            bearSpawner = FindObjectOfType<BearSpawner>();
            if (bearSpawner == null)
            {
                Debug.LogError("BearSpawner not found in scene!");
            }
        }

        if (dialogueRunner == null)
        {
            dialogueRunner = FindObjectOfType<DialogueRunner>();
            if (dialogueRunner == null)
            {
                Debug.LogError("DialogueRunner not found in scene!");
            }
        }
    }

    private void RegisterYarnCommands()
    {
        if (!commandsRegistered && dialogueRunner != null)
        {
            try
            {
                if (!dialogueRunner.IsCommandRegistered("startQuest"))
                {
                    dialogueRunner.AddCommandHandler<string>("startQuest", StartQuestCommand);
                    commandsRegistered = true;
                    Debug.Log("Successfully registered startQuest command");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Error registering startQuest command: {e.Message}");
            }
        }
    }

    private void UnregisterYarnCommands()
    {
        if (commandsRegistered && dialogueRunner != null)
        {
            try
            {
                if (dialogueRunner.IsCommandRegistered("startQuest"))
                {
                    dialogueRunner.RemoveCommandHandler("startQuest");
                    commandsRegistered = false;
                    Debug.Log("Successfully unregistered startQuest command");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Error unregistering startQuest command: {e.Message}");
            }
        }
    }

    private void StartQuestCommand(string questId)
    {
        StartQuest(questId);
    }

    public void StartQuest(string questId)
    {
        if (string.IsNullOrEmpty(questId))
        {
            Debug.LogError("Invalid quest ID: null or empty");
            return;
        }

        if (!activeQuests.ContainsKey(questId))
        {
            QuestData questData = CreateQuestData(questId);
            if (questData != null)
            {
                activeQuests[questId] = questData;
                OnQuestStarted?.Invoke(questData);
                
                // Validate BearSpawner before spawning
                if (bearSpawner != null && bearSpawner.IsInitialized)
                {
                    SpawnBearsForQuest(questId);
                }
                else
                {
                    Debug.LogError("BearSpawner not properly initialized!");
                }
            }
        }
    }

    private BearSpawner.ArenaType GetArenaTypeFromQuestId(string questId)
    {
        return questId switch
        {
            "northwest_arena" => BearSpawner.ArenaType.Northwest,
            "northeast_arena" => BearSpawner.ArenaType.Northeast,
            "boss_arena" => BearSpawner.ArenaType.Boss,
            _ => throw new System.ArgumentException($"Invalid quest ID: {questId}")
        };
    }

    private void SpawnBearsForQuest(string questId)
    {
        if (bearSpawner == null)
        {
            Debug.LogError("BearSpawner reference is missing!");
            return;
        }

        try
        {
            // Validate arena centers before spawning
            if (!ValidateArenaReferences(questId))
            {
                Debug.LogError($"Required arena references missing for quest {questId}");
                return;
            }

            var arenaType = GetArenaTypeFromQuestId(questId);
            bearSpawner.SpawnArena(arenaType);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error spawning bears for quest {questId}: {e.Message}");
        }
    }

    private bool ValidateArenaReferences(string questId)
    {
        if (bearSpawner == null) return false;

        // Check if we have settings for this arena
        if (!arenaSettings.ContainsKey(questId))
        {
            Debug.LogError($"No arena settings found for quest {questId}");
            return false;
        }

        var settings = arenaSettings[questId];
        
        // Validate the settings
        if (settings.position == null || settings.radius <= 0)
        {
            Debug.LogError($"Invalid arena settings for quest {questId}");
            return false;
        }

        // If we got here, the arena is properly configured
        return true;
    }

    private QuestData CreateQuestData(string questId)
    {
        try
        {
            var arenaType = GetArenaTypeFromQuestId(questId);
            return new QuestData
            {
                questId = questId,
                title = $"{arenaType} Arena Quest",
                description = $"Clear the {arenaType} arena of bears",
                requiredBearKills = GetRequiredKillsForArena(arenaType),
                currentBearKills = 0,
                isCompleted = false
            };
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating quest data: {e.Message}");
            return null;
        }
    }

    private int GetRequiredKillsForArena(BearSpawner.ArenaType arenaType)
    {
        return arenaType switch
        {
            BearSpawner.ArenaType.Northwest => 4, // From BearSpawner normalBearCount
            BearSpawner.ArenaType.Northeast => 4, // normalBearCount + fireBearCount
            BearSpawner.ArenaType.Boss => 6,      // normalBearCount + fireBearCount + iceBearCount
            _ => throw new System.ArgumentException($"Invalid arena type: {arenaType}")
        };
    }

    public void UpdateQuestProgress(string questId, int bearKills)
    {
        if (activeQuests.TryGetValue(questId, out QuestData questData))
        {
            questData.currentBearKills = bearKills;
            OnQuestUpdated?.Invoke(questData);

            if (bearKills >= questData.requiredBearKills && !questData.isCompleted)
            {
                CompleteQuest(questData);
            }
        }
    }

    private void CompleteQuest(QuestData quest)
    {
        if (!quest.isCompleted)
        {
            quest.isCompleted = true;
            OnQuestCompleted?.Invoke(quest);

            // Trigger the appropriate next dialogue node
            switch (quest.questId)
            {
                case "northwest_arena":
                    if (dialogueRunner != null)
                        dialogueRunner.StartDialogue("NorthwestComplete");
                    break;
                case "northeast_arena":
                    if (dialogueRunner != null)
                        dialogueRunner.StartDialogue("NortheastComplete");
                    break;
                case "boss_arena":
                    if (dialogueRunner != null)
                        dialogueRunner.StartDialogue("Victory");
                    break;
            }
        }
    }

    public bool IsQuestActive(string questId)
    {
        return activeQuests.ContainsKey(questId);
    }

    public bool IsQuestCompleted(string questId)
    {
        return activeQuests.TryGetValue(questId, out QuestData questData) && questData.isCompleted;
    }

    public QuestData GetQuestData(string questId)
    {
        return activeQuests.TryGetValue(questId, out QuestData questData) ? questData : null;
    }

    private void HandleBearDeath(IBear bear)
    {
        if (string.IsNullOrEmpty(bear.QuestId)) return;

        if (activeQuests.TryGetValue(bear.QuestId, out QuestData quest))
        {
            quest.currentBearKills++;
            OnQuestUpdated?.Invoke(quest);

            if (quest.currentBearKills >= quest.requiredBearKills)
            {
                CompleteQuest(quest);
            }
        }
    }

    private string GetQuestIdFromPosition(Vector3 position)
    {
        // Check each arena's radius to determine which quest the bear belonged to
        Vector2 pos2D = new Vector2(position.x, position.z);
        
        if (IsPositionInArena(pos2D, BearSpawner.ArenaType.Northwest))
            return "northwest_arena";
        if (IsPositionInArena(pos2D, BearSpawner.ArenaType.Northeast))
            return "northeast_arena";
        if (IsPositionInArena(pos2D, BearSpawner.ArenaType.Boss))
            return "boss_arena";
            
        return null;
    }

    private bool IsPositionInArena(Vector2 position, BearSpawner.ArenaType arenaType)
    {
        var settings = bearSpawner.GetArenaSettings(arenaType);
        if (settings != null)
        {
            float distance = Vector2.Distance(position, settings.position);
            return distance <= settings.radius;
        }
        return false;
    }

    private void OnDestroy()
    {
        UnregisterYarnCommands();
    }

    public void RegisterQuest(QuestData quest)
    {
        if (!activeQuests.ContainsKey(quest.questId))
        {
            activeQuests.Add(quest.questId, quest);
            OnQuestStarted?.Invoke(quest);
        }
    }

    public void OnBearKilled(string questId)
    {
        if (activeQuests.TryGetValue(questId, out QuestData quest))
        {
            quest.currentBearKills++;
            OnQuestUpdated?.Invoke(quest);

            if (quest.currentBearKills >= quest.requiredBearKills)
            {
                CompleteQuest(quest);
            }
        }
    }

    public ArenaSettings GetArenaSettings(string questId)
    {
        if (arenaSettings.TryGetValue(questId, out var settings))
        {
            return settings;
        }
        
        Debug.LogError($"Required arena references missing for quest {questId}");
        return null;
    }
}