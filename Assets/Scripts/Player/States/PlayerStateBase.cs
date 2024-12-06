using UnityEngine;
using Player.Core;
using Player.Input.Interfaces;

namespace Player.States
{
    public abstract class PlayerStateBase : IPlayerState
    {
        protected PlayerController playerController;
        protected Animator animator;
        protected CharacterController characterController;
        protected IPlayerInput playerInput;

        public PlayerStateBase(PlayerController controller)
        {
            playerController = controller;
            animator = controller.Animator;
            characterController = controller.CharacterController;
            playerInput = controller.PlayerInput;
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

        protected void UpdateAnimations(bool isMoving)
        {
            float speedValue = 0f;
            if (this is IdleState)
                speedValue = isMoving ? 0.5f : 0f;
            else if (this is SprintState)
                speedValue = 1f;
            else if (this is CombatState)
                speedValue = isMoving ? 0.3f : 0f;
            
            animator.SetFloat("Speed", Mathf.Lerp(
                animator.GetFloat("Speed"),
                speedValue,
                Time.deltaTime * 10f
            ));
        }
    }
} 