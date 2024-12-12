using UnityEngine;
using Player.Core;

namespace Player.States
{
    public class CombatState : PlayerStateBase
    {
        private float attackTimer;
        private CombatInputHandler combatHandler;

        public CombatState(PlayerController controller) : base(controller) 
        {
            combatHandler = controller.GetComponent<CombatInputHandler>();
        }

        public override void Enter()
        {
            attackTimer = 0f;
        }

        private void CheckStateTransitions()
        {
            if (attackTimer <= 0 && !playerInput.IsAttacking)
            {
                string nextState = playerInput.IsSprinting && playerInput.MovementInput.magnitude > 0.1f 
                    ? "SprintState" 
                    : "IdleState";
                Debug.Log($"CombatState: Transitioning to {nextState}");
                
                if (nextState == "SprintState")
                    playerController.ChangeState(new SprintState(playerController));
                else
                    playerController.ChangeState(new IdleState(playerController));
            }
        }

        public override void HandleMovement(Vector2 input)
        {
            if (input.magnitude >= 0.1f)
            {
                Vector3 moveDirection = playerController.CalculateMoveDirection(input);
                float currentSpeed = playerController.MoveSpeed * 0.7f;

                playerController.RotateTowardsMoveDirection(moveDirection);
                playerController.Move(moveDirection * currentSpeed);
            }

            ApplyGravity();
            UpdateAnimations(input.magnitude >= 0.1f, input);
        }

        public override void HandleCombat()
        {
            if (attackTimer <= 0)
            {
                if (playerInput.IsAttacking)
                {
                    Debug.Log($"CombatState: Starting attack with duration {playerController.AttackDuration}");
                    combatHandler.PerformAttack();
                    animationController.PlayAttack();
                    attackTimer = playerController.AttackDuration;
                }
            }
            else
            {
                Debug.Log($"CombatState: Attack cooldown remaining: {attackTimer:F2}s");
            }
        }

        public override void HandleJump()
        {
            if (characterController.isGrounded && playerInput.IsJumping)
            {
                playerController.VerticalVelocity = playerController.JumpForce * 0.8f; // Reduced jump height in combat
            }
        }

        public override void Update()
        {
            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
            }
            
            CheckStateTransitions();
        }
    }
} 