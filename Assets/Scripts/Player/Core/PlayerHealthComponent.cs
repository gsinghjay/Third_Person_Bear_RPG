using UnityEngine;
using Combat;
using Player.States;
using Enemies.Types;
using System.Collections;

namespace Player.Core
{
    public class PlayerHealthComponent : HealthComponent
    {
        [Header("Player Specific Settings")]
        [SerializeField] private float invulnerabilityDuration = 1f;
        private bool isInvulnerable;
        private PlayerController playerController;
        private Animator animator;
        [SerializeField] private float respawnDelay = 2f;

        protected override void Awake()
        {
            base.Awake();
            playerController = GetComponent<PlayerController>();
            animator = GetComponent<Animator>();
        }

        public override void TakeDamage(float amount, DamageType damageType)
        {
            if (isInvulnerable) return;

            base.TakeDamage(amount, damageType);

            // Visual feedback
            if (animator != null)
            {
                animator.SetTrigger("GetHit");
            }

            // Start invulnerability period
            StartCoroutine(InvulnerabilityRoutine());
        }

        protected override void Die()
        {
            base.Die();
            if (playerController != null)
            {
                playerController.ChangeState(new DeathState(playerController));
                StartCoroutine(RespawnRoutine());
            }
        }

        private float CalculateDamage(float amount, DamageType damageType)
        {
            float finalDamage = amount;

            // Example damage type modifiers
            switch (damageType)
            {
                case DamageType.Fire:
                    finalDamage *= 1.2f; // Fire does more damage
                    break;
                case DamageType.Ice:
                    finalDamage *= 0.8f; // Ice does less damage
                    break;
            }

            return finalDamage;
        }

        private System.Collections.IEnumerator InvulnerabilityRoutine()
        {
            isInvulnerable = true;
            yield return new WaitForSeconds(invulnerabilityDuration);
            isInvulnerable = false;
        }

        private IEnumerator RespawnRoutine()
        {
            // Wait for death animation
            yield return new WaitForSeconds(respawnDelay);
            
            // Reset health and notify UI
            ResetHealth();
            
            // Respawn player
            if (playerController != null)
            {
                playerController.Respawn();
            }
        }

        private void ResetHealth()
        {
            currentHealth = maxHealth;
            // Use the protected method from base class to notify of health change
            NotifyHealthChanged();
            Debug.Log($"Health reset to {currentHealth}");
        }
    }
}
