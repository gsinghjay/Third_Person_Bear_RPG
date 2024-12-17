// Assets/Scripts/Quest/QuestData.cs
using UnityEngine;

[System.Serializable]
public class QuestData
{
    public string questId;
    public string title;
    public string description;
    public bool isCompleted;
    public int requiredBearKills;
    public int currentBearKills;
}