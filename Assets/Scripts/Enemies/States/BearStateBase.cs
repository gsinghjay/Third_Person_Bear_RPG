using UnityEngine;
using Enemies.Core;

namespace Enemies.States
{
    public abstract class BearStateBase : IBearState
    {
        protected readonly BearController bearController;
        protected readonly Animator animator;
        protected readonly CharacterController characterController;

        protected BearStateBase(BearController controller)
        {
            bearController = controller;
            animator = controller.Animator;
            characterController = controller.CharacterController;
        }

        public virtual void Enter() 
        {
            ResetAllBoolParameters();
        }
        
        public virtual void Exit() { }
        public virtual void Update() 
        {
            ApplyGravity();
        }
        
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

        protected void ResetAllBoolParameters()
        {
            animator.SetBool("Idle", false);
            animator.SetBool("Combat Idle", false);
            animator.SetBool("Run Forward", false);
            animator.SetBool("Walk Forward", false);
            animator.SetBool("Walk Back", false);
            animator.SetBool("Run Back", false);
            animator.SetBool("Jump Loop", false);
            animator.SetBool("Stunned Loop", false);
        }
    }
} 