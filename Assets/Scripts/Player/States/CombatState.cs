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

        public override void Exit()
        {
        }

        public override void Update()
        {
            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
            }
            
            // Check for state transitions
            CheckStateTransitions();
        }

        private void CheckStateTransitions()
        {
            // If we're not attacking or defending, allow state changes
            if (attackTimer <= 0 && !playerInput.IsAttacking)
            {
                if (playerInput.IsSprinting && playerInput.MovementInput.magnitude > 0.1f)
                {
                    playerController.ChangeState(new SprintState(playerController));
                }
                else if (!playerInput.IsDefending)
                {
                    playerController.ChangeState(new IdleState(playerController));
                }
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
                    Debug.Log("Combat State: Attack input detected");
                    combatHandler.PerformAttack();
                    attackTimer = playerController.AttackDuration;
                }
            }
            else
            {
                Debug.Log($"Combat State: Attack on cooldown: {attackTimer}");
            }
        }

        public override void HandleJump()
        {
            if (characterController.isGrounded && playerInput.IsJumping)
            {
                playerController.VerticalVelocity = playerController.JumpForce * 0.8f; // Reduced jump height in combat
            }
        }
    }
} 