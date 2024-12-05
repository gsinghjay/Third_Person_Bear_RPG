using UnityEngine;
using Enemies.Core;

namespace Enemies.States
{
    public class BearChaseState : BearStateBase
    {
        private float moveSpeed;
        private float attackRange;
        private Vector3 targetPosition;

        public BearChaseState(BearController controller) : base(controller)
        {
            moveSpeed = controller.MoveSpeed;
            attackRange = controller.AttackRange;
        }

        public override void Enter()
        {
            animator.SetBool("IsMoving", true);
            animator.SetTrigger("Run");
        }

        public override void Update()
        {
            ApplyGravity();
            HandleMovement();
            CheckForAttackRange();
        }

        public override void HandleMovement()
        {
            targetPosition = bearController.PlayerTransform.position;
            Vector3 direction = (targetPosition - bearController.transform.position).normalized;
            
            // Rotate towards target
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                bearController.transform.rotation = Quaternion.Slerp(
                    bearController.transform.rotation,
                    targetRotation,
                    10f * Time.deltaTime
                );
            }

            // Move towards target
            Vector3 movement = direction * moveSpeed * Time.deltaTime;
            movement.y = bearController.VerticalVelocity;
            characterController.Move(movement);
        }

        private void CheckForAttackRange()
        {
            float distanceToPlayer = Vector3.Distance(bearController.transform.position, targetPosition);
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