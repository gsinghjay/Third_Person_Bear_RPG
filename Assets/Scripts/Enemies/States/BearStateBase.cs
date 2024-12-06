using UnityEngine;
using Enemies.Core;

namespace Enemies.States
{
    public abstract class BearStateBase : IBearState
    {
        protected BearController bearController;
        protected Animator animator;
        protected CharacterController characterController;

        public BearStateBase(BearController controller)
        {
            bearController = controller;
            animator = controller.Animator;
            characterController = controller.CharacterController;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update() { }
        public virtual void HandleMovement() { }
        public virtual void HandleCombat() { }

        protected void ApplyGravity()
        {
            if (characterController.isGrounded)
            {
                bearController.VerticalVelocity = -0.5f;
            }
            else
            {
                bearController.VerticalVelocity += Physics.gravity.y * Time.deltaTime;
            }
        }

        protected void UpdateAnimations(bool isMoving)
        {
            animator.SetBool("IsMoving", isMoving);
            animator.SetBool("IsGrounded", characterController.isGrounded);
        }
    }
} 