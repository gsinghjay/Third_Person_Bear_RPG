using UnityEngine;
using Enemies.Core;

namespace Enemies.States
{
    public class BearAttackState : BearStateBase
    {
        private readonly float attackDuration = 1.5f;
        private float attackTimer;
        private bool hasDealtDamage;
        private readonly int attackVariations = 5;

        public BearAttackState(BearController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();
            animator.SetBool("Combat Idle", true);
            TriggerAttackAnimation();
            attackTimer = attackDuration;
            hasDealtDamage = false;
        }

        public override void Update()
        {
            base.Update();
            HandleCombat();
            UpdateAttackState();
        }

        public override void HandleCombat()
        {
            if (!hasDealtDamage && attackTimer <= attackDuration * 0.5f)
            {
                bearController.DealDamage();
                hasDealtDamage = true;
            }
        }

        private void UpdateAttackState()
        {
            // Look at player during attack
            Vector3 direction = (bearController.PlayerTransform.position - bearController.transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                bearController.transform.rotation = Quaternion.LookRotation(direction);
            }

            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                float distanceToPlayer = Vector3.Distance(
                    bearController.transform.position, 
                    bearController.PlayerTransform.position
                );

                if (distanceToPlayer <= bearController.AttackRange)
                {
                    // Chain into another attack
                    Enter();
                }
                else
                {
                    bearController.ChangeState(new BearChaseState(bearController));
                }
            }
        }

        private void TriggerAttackAnimation()
        {
            int attackNumber = Random.Range(1, attackVariations + 1);
            animator.SetTrigger($"Attack{attackNumber}");
        }
    }
} 