using UnityEngine;
using Player.Core;

namespace Player.States
{
    public class SprintState : PlayerStateBase
    {
        private bool wasInAir = false;

        public SprintState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();
            wasInAir = !characterController.isGrounded;
            Debug.Log("SprintState: Entered");
        }

        public override void Update()
        {
            base.Update();
            wasInAir = !characterController.isGrounded;
        }

        public override void HandleMovement(Vector2 input)
        {
            if (input.magnitude >= 0.1f && playerInput.IsSprinting)
            {
                Vector3 moveDirection = playerController.CalculateMoveDirection(input);
                
                playerController.RotateTowardsMoveDirection(moveDirection);
                playerController.Move(moveDirection, playerController.SprintSpeed);
            }
            else
            {
                playerController.ChangeState(new IdleState(playerController));
                return;
            }

            ApplyGravity();
            UpdateAnimations(true, input);
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
            if (characterController.isGrounded && playerInput.IsJumping && !animationController.IsJumping())
            {
                playerController.VerticalVelocity = playerController.JumpForce;
                animationController.StartJump();
                Debug.Log("SprintState: Starting new jump");
            }
            
            ApplyGravity();
            HandleJumpAnimation(characterController.isGrounded, playerController.VerticalVelocity);
        }

        public override void Exit()
        {
            base.Exit();
            Debug.Log("SprintState: Exited");
        }
    }
} 