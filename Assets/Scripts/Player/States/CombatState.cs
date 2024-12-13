using UnityEngine;
using Player.Core;
using System.Collections;

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
            base.Enter();
            
            if (combatHandler == null)
            {
                combatHandler = playerController?.GetComponent<CombatInputHandler>();
                if (combatHandler == null)
                {
                    Debug.LogError("CombatState: No CombatInputHandler found!");
                }
            }
            
            attackTimer = 0f;
            Debug.Log("CombatState: Entered");
        }

        public override void HandleMovement(Vector2 input)
        {
            // Allow movement during combat, just slower
            if (input.magnitude >= 0.1f)
            {
                Vector3 moveDirection = playerController.CalculateMoveDirection(input);
                playerController.RotateTowardsMoveDirection(moveDirection);
                playerController.Move(moveDirection, 0.7f); // 70% of normal speed
            }

            ApplyGravity();
            UpdateAnimations(input.magnitude >= 0.1f, input);
        }

        public override void HandleCombat()
        {
            if (attackTimer <= 0)
            {
                if (playerInput != null && playerInput.IsAttacking)
                {
                    if (animationController != null)
                    {
                        Debug.Log("CombatState: Handling attack input");
                        animationController.PlayAttack();
                        attackTimer = playerController.AttackDuration;
                        combatHandler?.PerformAttack();
                    }
                    else
                    {
                        Debug.LogError("CombatState: No animation controller available!");
                    }
                }
            }
        }

        public override void HandleJump()
        {
            if (characterController.isGrounded && playerInput.IsJumping)
            {
                playerController.VerticalVelocity = playerController.JumpForce * 0.8f;
                animationController.StartJump();
            }
            
            ApplyGravity();
            HandleJumpAnimation(characterController.isGrounded, playerController.VerticalVelocity);
        }

        public override void Update()
        {
            base.Update();
            
            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
            }
            
            // Check for state transitions only if we're not in the middle of an attack
            if (attackTimer <= 0)
            {
                if (!playerInput.IsAttacking && !playerInput.IsDefending)
                {
                    ReturnToPreviousState();
                }
            }
        }

        private void ReturnToPreviousState()
        {
            // Return to appropriate state based on current input
            if (playerInput.IsSprinting && playerInput.MovementInput.magnitude > 0.1f)
            {
                playerController.ChangeState(new SprintState(playerController));
            }
            else
            {
                playerController.ChangeState(new IdleState(playerController));
            }
        }
    }
} 