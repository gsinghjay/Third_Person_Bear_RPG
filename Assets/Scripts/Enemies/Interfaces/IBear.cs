// Assets/Scripts/Enemies/Interfaces/IBear.cs
using UnityEngine;
using Enemies.Types;

namespace Enemies.Interfaces
{
    public interface IBear
    {
        float Health { get; }
        BearType Type { get; }
        string QuestId { get; set; }
        void TakeDamage(float damage, DamageType damageType);
        void Initialize(Vector3 spawnPosition);
        event System.Action<IBear> OnDeath;  // This is the correct event name
    }
}