using UnityEngine;
using Enemies.Core;

namespace Enemies.States
{
    public class BearDeathState : BearStateBase
    {
        private readonly float deathAnimationDuration = 2f;
        private float deathTimer;
        private bool hasTriggeredQuestUpdate = false;
        private bool hasStartedCleanup = false;

        public BearDeathState(BearController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();
            Debug.Log($"[{bearController.name}] Entering Death State");

            // Disable components immediately
            DisableComponents();

            // Play death animation
            if (animator != null)
            {
                animator.SetTrigger("Death");
            }

            deathTimer = deathAnimationDuration;
        }

        private void DisableComponents()
        {
            // Add safety check for NavMeshAgent
            if (IsAgentValid())
            {
                agent.isStopped = true;
                agent.enabled = false;
            }

            // Disable Collider
            if (bearController.Collider != null)
            {
                bearController.Collider.enabled = false;
            }

            // Disable Rigidbody if present
            var rigidbody = bearController.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.isKinematic = true;
            }
        }

        public override void Update()
        {
            if (!hasTriggeredQuestUpdate && !bearController.QuestUpdateHandled)
            {
                QuestManager.Instance?.OnBearKilled(bearController.QuestId);
                bearController.QuestUpdateHandled = true;
                hasTriggeredQuestUpdate = true;
            }

            deathTimer -= Time.deltaTime;
            
            if (deathTimer <= 0 && !hasStartedCleanup)
            {
                hasStartedCleanup = true;
                CleanupBear();
            }
        }

        private void CleanupBear()
        {
            Debug.Log($"Destroying bear: {bearController.name}");
            Object.Destroy(bearController.gameObject);
        }

        // Override and empty these methods since dead bears shouldn't move or fight
        public override void HandleMovement() { }
        public override void HandleCombat() { }

        public override void Exit()
        {
            // Don't call base.Exit() since we don't want to modify the agent
            Debug.Log($"[{bearController.name}] Exiting Death State (This shouldn't happen)");
        }
    }
}
