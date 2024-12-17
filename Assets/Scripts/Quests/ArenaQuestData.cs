// Assets/Scripts/Quests/ArenaQuestData.cs
using UnityEngine;
using Enemies.Core;
using Enemies.Types;

[CreateAssetMenu(fileName = "ArenaQuest", menuName = "Quests/Arena Quest")]
public class ArenaQuestData : ScriptableObject
{
    public string questId;
    public string title;
    public string description;
    public BearSpawner.ArenaType arenaType;
    public ArenaSettings arenaSettings;

    // Helper property to get required kills from arena settings
    public int RequiredBearKills => arenaSettings.TotalBearCount;

    // Factory methods for creating predefined arena quests
    public static ArenaQuestData CreateNorthwestQuest()
    {
        var quest = CreateInstance<ArenaQuestData>();
        quest.questId = "northwest_arena";
        quest.title = "Clear the Northwest Arena";
        quest.description = "Defeat the bears in the northwest training arena";
        quest.arenaType = BearSpawner.ArenaType.Northwest;
        quest.arenaSettings = ArenaSettings.Northwest;
        return quest;
    }

    public static ArenaQuestData CreateNortheastQuest()
    {
        var quest = CreateInstance<ArenaQuestData>();
        quest.questId = "northeast_arena";
        quest.title = "Clear the Northeast Arena";
        quest.description = "Defeat the fire bears in the northeast arena";
        quest.arenaType = BearSpawner.ArenaType.Northeast;
        quest.arenaSettings = ArenaSettings.Northeast;
        return quest;
    }

    public static ArenaQuestData CreateBossQuest()
    {
        var quest = CreateInstance<ArenaQuestData>();
        quest.questId = "boss_arena";
        quest.title = "The Final Challenge";
        quest.description = "Face both fire and ice bears in the boss arena";
        quest.arenaType = BearSpawner.ArenaType.Boss;
        quest.arenaSettings = ArenaSettings.Boss;
        return quest;
    }
}