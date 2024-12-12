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
        public virtual void HandleJump() { }

        protected void ApplyGravity()
        {
            if (characterController.isGrounded)
            {
                playerController.VerticalVelocity = -0.5f;
            }
            else
            {
                playerController.VerticalVelocity += Physics.gravity.y * Time.deltaTime;
            }
        }

        protected void UpdateAnimations(bool isMoving, Vector2 movementInput)
        {
            if (animationController == null)
            {
                Debug.LogError($"{GetType().Name}: No animation controller available!");
                return;
            }

            float speedValue = 0f;
            string currentState = GetType().Name;
            
            if (this is IdleState)
                speedValue = isMoving ? 0.5f : 0f;
            else if (this is SprintState)
                speedValue = 1f;
            else if (this is CombatState)
                speedValue = isMoving ? 0.3f : 0f;
            
            Debug.Log($"{currentState}: Updating animation speed to {speedValue:F2}");
            animationController.UpdateMovementAnimation(speedValue, movementInput);
        }
    }
} 