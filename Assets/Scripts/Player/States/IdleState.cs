using UnityEngine;
using Player.Core;

namespace Player.States
{
    public class IdleState : PlayerStateBase
    {
        public IdleState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            // No need to set animation parameters here
        }

        public override void HandleMovement(Vector2 input)
        {
            if (input.magnitude >= 0.1f)
            {
                Vector3 moveDirection = playerController.CalculateMoveDirection(input);
                float currentSpeed = playerController.MoveSpeed;
                
                if (playerInput.IsSprinting)
                {
                    playerController.ChangeState(new SprintState(playerController));
                    return;
                }

                playerController.RotateTowardsMoveDirection(moveDirection);
                playerController.Move(moveDirection * currentSpeed);
            }

            ApplyGravity();
            UpdateAnimations(input.magnitude >= 0.1f, input);
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
            }
        }
    }
} 