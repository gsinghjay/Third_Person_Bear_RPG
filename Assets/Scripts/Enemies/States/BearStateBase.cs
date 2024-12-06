using UnityEngine;
using UnityEngine.AI;
using Enemies.Core;

namespace Enemies.States
{
    public abstract class BearStateBase : IBearState
    {
        protected readonly BearController bearController;
        protected readonly Animator animator;
        protected readonly NavMeshAgent agent;

        protected BearStateBase(BearController controller)
        {
            bearController = controller;
            animator = controller.Animator;
            agent = controller.Agent;
        }

        public virtual void Enter() 
        {
            ResetAllBoolParameters();
        }
        
        public virtual void Exit() { }
        public virtual void Update() { }
        public virtual void HandleMovement() { }
        public virtual void HandleCombat() { }

        protected void ResetAllBoolParameters()
        {
            animator.SetBool("Idle", false);
            animator.SetBool("Combat Idle", false);
            animator.SetBool("Run Forward", false);
        }
    }
} 