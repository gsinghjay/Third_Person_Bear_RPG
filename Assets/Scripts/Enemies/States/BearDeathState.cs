using UnityEngine;
using Enemies.Core;

namespace Enemies.States
{
    public class BearDeathState : BearStateBase
    {
        private readonly float deathAnimationDuration = 2f;
        private float deathTimer;
        private bool hasTriggeredQuestUpdate = false;

        public BearDeathState(BearController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();
            Debug.Log($"[{bearController.name}] Entering Death State");

            // Stop all movement
            if (agent != null && agent.isActiveAndEnabled)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }

            // Play death animation - only use "Death" parameter
            if (animator != null)
            {
                animator.SetTrigger("Death");
            }

            // Disable collider to prevent further interactions
            if (bearController.Collider != null)
            {
                bearController.Collider.enabled = false;
            }

            deathTimer = deathAnimationDuration;
        }

        public override void Update()
        {
            base.Update();

            if (!hasTriggeredQuestUpdate && !bearController.QuestUpdateHandled)
            {
                Debug.Log($"Updating quest progress for bear death in {bearController.QuestId}");
                QuestManager.Instance?.OnBearKilled(bearController.QuestId);
                bearController.QuestUpdateHandled = true;
                hasTriggeredQuestUpdate = true;
            }

            deathTimer -= Time.deltaTime;
            
            if (deathTimer <= 0)
            {
                // Destroy the bear after animation completes
                Object.Destroy(bearController.gameObject);
            }
        }

        // Override and empty these methods since dead bears shouldn't move or fight
        public override void HandleMovement() { }
        public override void HandleCombat() { }

        public override void Exit()
        {
            base.Exit();
            Debug.Log($"[{bearController.name}] Exiting Death State (This shouldn't happen)");
        }
    }
}
