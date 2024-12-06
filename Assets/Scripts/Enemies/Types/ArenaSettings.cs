using UnityEngine;

namespace Enemies.Types
{
    [System.Serializable]
    public class ArenaSettings
    {
        public string name;
        public float radius = 30f;
        public float flatness = 0.9f;
        public Vector2 position;
        public int normalBearCount = 2;
        public int fireBearCount = 0;
        public int iceBearCount = 0;
    }
} 