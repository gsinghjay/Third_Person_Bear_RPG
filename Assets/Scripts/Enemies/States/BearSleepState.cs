using UnityEngine;
using System.Collections;
using Enemies.Core;

namespace Enemies.States
{
    public class BearSleepState : BearStateBase
    {
        private const float STARTLED_WAKE_DISTANCE = 5f;
        private const float ALERT_WAKE_DISTANCE = 8f;
        private float wakeUpCheckInterval = 0.5f;
        private float wakeUpCheckTimer;

        public BearSleepState(BearController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();
            Debug.Log($"[{bearController.name}] Entering Sleep State");
            
            // Stop the agent from moving
            agent.isStopped = true;
            
            // Set sleep animation
            animator.SetBool("Sleep", true);
            wakeUpCheckTimer = 0f;
        }

        public override void Exit()
        {
            animator.SetBool("Sleep", false);
            agent.isStopped = false;
        }

        public override void Update()
        {
            base.Update();
            
            wakeUpCheckTimer += Time.deltaTime;
            if (wakeUpCheckTimer >= wakeUpCheckInterval)
            {
                wakeUpCheckTimer = 0f;
                CheckForDisturbance();
            }
        }

        private void WakeUp(float distanceToPlayer)
        {
            // Player very close - startled wake-up
            if (distanceToPlayer <= STARTLED_WAKE_DISTANCE)
            {
                Debug.Log($"[{bearController.name}] Startled wake-up!");
                animator.SetTrigger("Jump");
                // Immediately transition to chase if startled
                bearController.ChangeState(new BearChaseState(bearController));
            }
            // Player at medium distance - alert wake-up
            else if (distanceToPlayer <= ALERT_WAKE_DISTANCE)
            {
                Debug.Log($"[{bearController.name}] Alert wake-up!");
                animator.SetTrigger("Get Hit Front");
                // Short delay before chasing
                bearController.StartStateCoroutine(DelayedStateChange(0.5f));
            }
            // Natural wake-up (player far away or other trigger)
            else
            {
                Debug.Log($"[{bearController.name}] Natural wake-up");
                animator.SetTrigger("Buff");
                // Longer delay before returning to idle
                bearController.StartStateCoroutine(DelayedStateChange(1.0f, true));
            }
        }

        private IEnumerator DelayedStateChange(float delay, bool toIdle = false)
        {
            yield return new WaitForSeconds(delay);
            bearController.ChangeState(toIdle ? 
                new BearIdleState(bearController) : 
                new BearChaseState(bearController));
        }

        private void CheckForDisturbance()
        {
            if (bearController.PlayerTransform == null) return;

            float distanceToPlayer = Vector3.Distance(
                bearController.transform.position,
                bearController.PlayerTransform.position
            );

            if (distanceToPlayer <= bearController.DetectionRange * 1.5f)
            {
                WakeUp(distanceToPlayer);
            }
        }
    }
}