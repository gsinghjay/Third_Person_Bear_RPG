using UnityEngine;
using Enemies.Types;

namespace Enemies.Interfaces
{
    public interface IBear
    {
        float Health { get; }
        BearType Type { get; }
        void TakeDamage(float damage, DamageType damageType);
        void Initialize(Vector3 spawnPosition);
    }
} 