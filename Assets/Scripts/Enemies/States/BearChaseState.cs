using UnityEngine;
using Enemies.Core;

namespace Enemies.States
{
    public class BearChaseState : BearStateBase
    {
        private readonly float attackRange;
        private Vector3 targetPosition;

        public BearChaseState(BearController controller) : base(controller)
        {
            attackRange = controller.AttackRange;
        }

        public override void Enter()
        {
            base.Enter();
            animator.SetBool("Run Forward", true);
            
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.isStopped = false;
            }
        }

        public override void Exit()
        {
            base.Exit();
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }
        }

        public override void Update()
        {
            base.Update();
            HandleMovement();
            CheckForAttackRange();
        }

        public override void HandleMovement()
        {
            if (bearController.PlayerTransform == null || 
                agent == null || 
                !agent.isActiveAndEnabled || 
                !agent.isOnNavMesh) return;

            targetPosition = bearController.PlayerTransform.position;
            agent.SetDestination(targetPosition);

            if (agent.velocity.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
                bearController.transform.rotation = Quaternion.Slerp(
                    bearController.transform.rotation,
                    targetRotation,
                    10f * Time.deltaTime
                );
            }
        }

        private void CheckForAttackRange()
        {
            if (bearController.PlayerTransform == null) return;

            float distanceToPlayer = Vector3.Distance(
                bearController.transform.position, 
                targetPosition
            );
            
            if (distanceToPlayer <= attackRange)
            {
                bearController.ChangeState(new BearAttackState(bearController));
            }
            else if (distanceToPlayer > bearController.DetectionRange * 1.5f)
            {
                bearController.ChangeState(new BearIdleState(bearController));
            }
        }
    }
} 