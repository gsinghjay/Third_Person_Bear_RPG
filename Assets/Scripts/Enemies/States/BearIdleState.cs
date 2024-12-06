using UnityEngine;
using Enemies.Core;

namespace Enemies.States
{
    public class BearIdleState : BearStateBase
    {
        private float detectionCheckInterval = 0.5f;
        private float detectionCheckTimer;
        private float sleepTimer = 0f;
        private const float TIME_UNTIL_SLEEP = 5f;

        public BearIdleState(BearController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();
            animator.SetBool("Idle", true);
            detectionCheckTimer = 0f;
            sleepTimer = 0f;
            agent.isStopped = true;
        }

        public override void Update()
        {
            base.Update();
            
            detectionCheckTimer += Time.deltaTime;
            if (detectionCheckTimer >= detectionCheckInterval)
            {
                detectionCheckTimer = 0f;
                CheckForPlayer();
            }

            // Increment sleep timer and check for sleep transition
            sleepTimer += Time.deltaTime;
            if (sleepTimer >= TIME_UNTIL_SLEEP)
            {
                Debug.Log($"[{bearController.name}] Getting sleepy...");
                bearController.ChangeState(new BearSleepState(bearController));
            }
        }

        private void CheckForPlayer()
        {
            if (bearController.PlayerTransform == null) return;

            float distanceToPlayer = Vector3.Distance(
                bearController.transform.position, 
                bearController.PlayerTransform.position
            );
            
            if (distanceToPlayer <= bearController.DetectionRange)
            {
                sleepTimer = 0f; // Reset sleep timer when player is detected
                bearController.ChangeState(new BearChaseState(bearController));
            }
        }
    }
} 