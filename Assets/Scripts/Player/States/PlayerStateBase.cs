using UnityEngine;
using Player.Core;
using Player.Input.Interfaces;

namespace Player.States
{
    public abstract class PlayerStateBase : IPlayerState
    {
        protected PlayerController playerController;
        protected PlayerAnimationController animationController;
        protected CharacterController characterController;
        protected IPlayerInput playerInput;

        public PlayerStateBase(PlayerController controller)
        {
            playerController = controller;
            if (controller == null)
            {
                Debug.LogError($"{GetType().Name}: Controller is null!");
                return;
            }

            animationController = controller.AnimationController;
            if (animationController == null)
            {
                Debug.LogError($"{GetType().Name}: AnimationController is null on {controller.name}!");
            }

            characterController = controller.CharacterController;
            if (characterController == null)
            {
                Debug.LogError($"{GetType().Name}: CharacterController is null on {controller.name}!");
            }

            playerInput = controller.PlayerInput;
            if (playerInput == null)
            {
                Debug.LogError($"{GetType().Name}: PlayerInput is null on {controller.name}!");
            }

            Debug.Log($"{GetType().Name}: Initialized with controller {controller.name}");
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update() { }
        public virtual void HandleMovement(Vector2 input) { }
        public virtual void HandleCombat() { }
        public virtual void HandleJump()
        {
            if (characterController.isGrounded && playerInput.IsJumping && !animationController.IsJumping())
            {
                // Apply jump force
                playerController.VerticalVelocity = playerController.JumpForce;
                animationController.StartJump();
                
                // Ensure immediate upward movement
                Vector3 jumpMovement = new Vector3(0, playerController.VerticalVelocity, 0);
                playerController.Move(jumpMovement);
                
                Debug.Log($"Jump initiated - Force: {playerController.JumpForce}, " +
                         $"Initial Movement: {jumpMovement}");
            }
            
            ApplyGravity();
            HandleJumpAnimation(characterController.isGrounded, playerController.VerticalVelocity);
        }

        protected void ApplyGravity()
        {
            float gravityValue = Physics.gravity.y;
            float gravityMultiplier = 2.5f;

            if (characterController.isGrounded && playerController.VerticalVelocity <= 0)
            {
                playerController.VerticalVelocity = -0.5f;
                Debug.Log("Grounded, resetting vertical velocity");
            }
            else
            {
                float previousVelocity = playerController.VerticalVelocity;
                playerController.VerticalVelocity += gravityValue * gravityMultiplier * Time.deltaTime;
                
                // Terminal velocity
                if (playerController.VerticalVelocity < -20f)
                    playerController.VerticalVelocity = -20f;

                if (previousVelocity != playerController.VerticalVelocity)
                {
                    Debug.Log($"Applying gravity - Previous: {previousVelocity}, Current: {playerController.VerticalVelocity}");
                }
            }
        }

        protected void UpdateAnimations(bool isMoving, Vector2 movementInput)
        {
            if (animationController == null) return;

            // Don't override animation if jumping
            if (animationController.IsJumping())
            {
                return;
            }

            // Use thresholds for different states instead of speed multipliers
            float speedValue;
            if (this is SprintState)
                speedValue = 1f;
            else if (this is CombatState)
                speedValue = isMoving ? 0.3f : 0f;
            else // IdleState
                speedValue = isMoving ? 0.5f : 0f;
            
            animationController.UpdateMovementAnimation(speedValue, movementInput);
        }

        protected void HandleJumpAnimation(bool isGrounded, float verticalVelocity)
        {
            if (animationController == null) return;

            if (!isGrounded && verticalVelocity > 0 && !animationController.IsJumping())
            {
                animationController.StartJump();
                Debug.Log($"Starting jump animation. Vertical Velocity: {verticalVelocity}");
            }
        }
    }
} 