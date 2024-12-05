using UnityEngine;
using Player.Core;

namespace Player.States
{
    public class SprintState : PlayerStateBase
    {
        public SprintState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            animator.SetBool("IsSprinting", true);
        }

        public override void Exit()
        {
            animator.SetBool("IsSprinting", false);
        }

        public override void HandleMovement(Vector2 input)
        {
            if (input.magnitude >= 0.1f && playerInput.IsSprinting)
            {
                Vector3 moveDirection = playerController.CalculateMoveDirection(input);
                float currentSpeed = playerController.SprintSpeed;

                playerController.RotateTowardsMoveDirection(moveDirection);
                playerController.Move(moveDirection * currentSpeed);
            }
            else
            {
                playerController.ChangeState(new IdleState(playerController));
                return;
            }

            ApplyGravity();
            UpdateAnimations(true);
        }

        public override void HandleCombat()
        {
            if (playerInput.IsAttacking || playerInput.IsDefending)
            {
                playerController.ChangeState(new CombatState(playerController));
            }
        }

        public override void HandleJump()
        {
            if (characterController.isGrounded && playerInput.IsJumping)
            {
                playerController.VerticalVelocity = playerController.JumpForce;
                animator.SetTrigger("Jump");
            }
        }
    }
} 