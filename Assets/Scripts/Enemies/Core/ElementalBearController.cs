using UnityEngine;
using Enemies.Types;

namespace Enemies.Core
{
    public abstract class ElementalBearController : BearController
    {
        [Header("Elemental Settings")]
        [SerializeField] protected float elementalDamage = 20f;
        [SerializeField] protected ParticleSystem elementalEffect;
        
        protected override float CalculateDamage(float damage, DamageType damageType)
        {
            // Apply elemental resistance/weakness logic
            float finalDamage = base.CalculateDamage(damage, damageType);
            
            if (IsWeakAgainst(damageType))
                finalDamage *= 1.5f;
            else if (IsResistantTo(damageType))
                finalDamage *= 0.5f;
                
            return finalDamage;
        }
        
        protected abstract bool IsWeakAgainst(DamageType damageType);
        protected abstract bool IsResistantTo(DamageType damageType);
    }
} 