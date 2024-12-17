// Assets/Scripts/Enemies/Types/ArenaSettings.cs
using UnityEngine;

namespace Enemies.Types
{
    [System.Serializable]
    public class ArenaSettings
    {
        public string questId;
        public string name;
        public float radius = 30f;
        public Vector2 position;
        public int normalBearCount;
        public int fireBearCount;
        public int iceBearCount;

        // Predefined arena configurations
        public static ArenaSettings Northwest => new ArenaSettings
        {
            questId = "northwest_arena",
            name = "Northwest Training Arena",
            radius = 35f,
            normalBearCount = 4,
            fireBearCount = 0,
            iceBearCount = 0
        };

        public static ArenaSettings Northeast => new ArenaSettings
        {
            questId = "northeast_arena",
            name = "Northeast Fire Arena",
            radius = 40f,
            normalBearCount = 2,
            fireBearCount = 2,
            iceBearCount = 0
        };

        public static ArenaSettings Boss => new ArenaSettings
        {
            questId = "boss_arena",
            name = "Boss Arena",
            radius = 50f,
            normalBearCount = 2,
            fireBearCount = 2,
            iceBearCount = 2
        };

        // Helper property to get total bears
        public int TotalBearCount => normalBearCount + fireBearCount + iceBearCount;
    }
}