using UnityEngine;
using Player.Core;

namespace Player.States
{
    public class IdleState : PlayerStateBase
    {
        private bool wasInAir = false;

        public IdleState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();
            wasInAir = !characterController.isGrounded;
            Debug.Log("IdleState: Entered");
        }

        public override void Update()
        {
            base.Update();
            wasInAir = !characterController.isGrounded;
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
            if (playerInput == null || playerController == null || animationController.IsJumping()) return;

            if (playerInput.IsAttacking)
            {
                Debug.Log("IdleState: Transitioning to CombatState due to attack");
                var newState = new CombatState(playerController);
                playerController.ChangeState(newState);
                
                // Only forward the combat input if the state change was successful
                if (playerController.CurrentState == newState)
                {
                    playerController.CurrentState.HandleCombat();
                }
            }
            else if (playerInput.IsDefending)
            {
                playerController.ChangeState(new CombatState(playerController));
            }
        }

        public override void HandleJump()
        {
            base.HandleJump();

            if (!characterController.isGrounded)
            {
                Vector3 moveDirection = Vector3.zero;
                moveDirection.y = playerController.VerticalVelocity;
                playerController.Move(moveDirection);
            }
        }

        public override void Exit()
        {
            base.Exit();
            Debug.Log("IdleState: Exited");
        }
    }
} 