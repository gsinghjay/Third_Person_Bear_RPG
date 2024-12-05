using UnityEngine;
using Enemies.Core;

namespace Enemies.States
{
    public class BearAttackState : BearStateBase
    {
        private float attackDuration = 1.5f;
        private float attackTimer;
        private bool hasDealtDamage;

        public BearAttackState(BearController controller) : base(controller) { }

        public override void Enter()
        {
            animator.SetBool("IsMoving", false);
            animator.SetTrigger("Attack");
            attackTimer = attackDuration;
            hasDealtDamage = false;
        }

        public override void Update()
        {
            ApplyGravity();
            
            // Look at player during attack
            Vector3 direction = (bearController.PlayerTransform.position - bearController.transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                bearController.transform.rotation = Quaternion.LookRotation(direction);
            }

            attackTimer -= Time.deltaTime;

            // Deal damage at specific point in animation
            if (!hasDealtDamage && attackTimer <= attackDuration * 0.5f)
            {
                TryDealDamage();
                hasDealtDamage = true;
            }

            if (attackTimer <= 0)
            {
                // Check if player is still in range
                float distanceToPlayer = Vector3.Distance(
                    bearController.transform.position,
                    bearController.PlayerTransform.position
                );

                if (distanceToPlayer <= bearController.AttackRange)
                {
                    // Start another attack
                    bearController.ChangeState(new BearAttackState(bearController));
                }
                else
                {
                    bearController.ChangeState(new BearChaseState(bearController));
                }
            }
        }

        private void TryDealDamage()
        {
            // Check if player is in attack arc
            Vector3 directionToPlayer = bearController.PlayerTransform.position - bearController.transform.position;
            float angleToPlayer = Vector3.Angle(bearController.transform.forward, directionToPlayer);

            if (angleToPlayer <= 45f && directionToPlayer.magnitude <= bearController.AttackRange)
            {
                bearController.DealDamage();
            }
        }
    }
} 