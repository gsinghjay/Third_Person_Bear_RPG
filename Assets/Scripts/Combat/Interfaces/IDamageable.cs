// Assets/Scripts/Combat/Interfaces/IDamageable.cs
using Enemies.Types;

namespace Combat.Interfaces
{
    public interface IDamageable
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        bool IsAlive { get; }
        void TakeDamage(float amount, DamageType damageType);
        void Heal(float amount);
    }
}