using UnityEngine;
using Player.Core;

namespace Player.States
{
    public class CombatState : PlayerStateBase
    {
        private float attackTimer;

        public CombatState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            animator.SetBool("IsCombat", true);
            attackTimer = 0f;
        }

        public override void Exit()
        {
            animator.SetBool("IsCombat", false);
        }

        public override void Update()
        {
            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
            }
        }

        public override void HandleMovement(Vector2 input)
        {
            if (input.magnitude >= 0.1f)
            {
                Vector3 moveDirection = playerController.CalculateMoveDirection(input);
                float currentSpeed = playerController.MoveSpeed * 0.7f; // Slower movement in combat

                playerController.RotateTowardsMoveDirection(moveDirection);
                playerController.Move(moveDirection * currentSpeed);
            }

            ApplyGravity();
            UpdateAnimations(input.magnitude >= 0.1f);
        }

        public override void HandleCombat()
        {
            if (attackTimer <= 0)
            {
                if (playerInput.IsAttacking)
                {
                    animator.SetTrigger("Attack");
                    attackTimer = playerController.AttackDuration;
                }
                else if (playerInput.IsDefending)
                {
                    animator.SetBool("IsDefending", true);
                }
                else if (!playerInput.IsDefending)
                {
                    animator.SetBool("IsDefending", false);
                    playerController.ChangeState(new IdleState(playerController));
                }
            }
        }

        public override void HandleJump()
        {
            if (characterController.isGrounded && playerInput.IsJumping)
            {
                playerController.VerticalVelocity = playerController.JumpForce * 0.8f; // Reduced jump height in combat
                animator.SetTrigger("Jump");
            }
        }
    }
} 