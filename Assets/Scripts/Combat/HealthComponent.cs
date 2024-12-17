// Assets/Scripts/Combat/HealthComponent.cs
using UnityEngine;
using Combat.Interfaces;
using Enemies.Types;
using System;

namespace Combat
{
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] protected float currentHealth;
        
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsAlive => currentHealth > 0;
        
        public event Action<float> OnHealthChanged;
        public event Action OnDeath;

        // Add this protected method to allow derived classes to trigger the event
        protected void NotifyHealthChanged()
        {
            OnHealthChanged?.Invoke(currentHealth);
        }

        protected virtual void Awake()
        {
            currentHealth = maxHealth;
        }

        public virtual void TakeDamage(float amount, DamageType damageType)
        {
            if (!IsAlive) return;

            float previousHealth = currentHealth;
            currentHealth = Mathf.Max(0, currentHealth - amount);
            
            OnHealthChanged?.Invoke(currentHealth);
            
            if (previousHealth > 0 && currentHealth <= 0)
            {
                Die();
            }
            
            Debug.Log($"{gameObject.name} took {amount} damage. Health: {currentHealth}/{maxHealth}");
        }

        public virtual void Heal(float amount)
        {
            if (!IsAlive) return;

            float previousHealth = currentHealth;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            
            if (previousHealth != currentHealth)
            {
                OnHealthChanged?.Invoke(currentHealth);
            }
        }

        protected virtual void Die()
        {
            OnDeath?.Invoke();
            Debug.Log($"{gameObject.name} died!");
        }
    }
}