using UnityEngine;
using Enemies.Core;

namespace Enemies.States
{
    public class BearIdleState : BearStateBase
    {
        private float detectionCheckInterval = 0.5f;
        private float detectionCheckTimer;

        public BearIdleState(BearController controller) : base(controller) { }

        public override void Enter()
        {
            animator.SetBool("IsMoving", false);
            animator.SetTrigger("Idle");
            detectionCheckTimer = 0f;
        }

        public override void Update()
        {
            ApplyGravity();
            
            detectionCheckTimer += Time.deltaTime;
            if (detectionCheckTimer >= detectionCheckInterval)
            {
                detectionCheckTimer = 0f;
                CheckForPlayer();
            }
        }

        private void CheckForPlayer()
        {
            float distanceToPlayer = Vector3.Distance(bearController.transform.position, bearController.PlayerTransform.position);
            if (distanceToPlayer <= bearController.DetectionRange)
            {
                bearController.ChangeState(new BearChaseState(bearController));
            }
        }
    }
} 