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
            if (animationController.IsJumping()) return;

            if (playerInput.IsAttacking)
            {
                Debug.Log("SprintState: Transitioning to CombatState due to attack");
                playerController.ChangeState(new CombatState(playerController));
                
                // Important: Forward the attack input to the new state
                playerController.CurrentState.HandleCombat();
            }
            else if (playerInput.IsDefending)
            {
                playerController.ChangeState(new CombatState(playerController));
            }
        }

        public override void Exit()
        {
            base.Exit();
            Debug.Log("SprintState: Exited");
        }
    }
} 