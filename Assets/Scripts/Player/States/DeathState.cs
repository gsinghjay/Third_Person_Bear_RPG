using UnityEngine;
using Player.Core;

namespace Player.States
{
    public class DeathState : PlayerStateBase
    {
        public DeathState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();
            Debug.Log("Player entered death state");
            
            if (animationController != null)
            {
                animationController.PlayDeathAnimation();
            }
            
            // Disable only movement and input temporarily
            if (characterController != null)
            {
                characterController.enabled = false;
            }
        }

        // Keep all handle methods empty to prevent any actions during death
        public override void HandleMovement(Vector2 input) { }
        public override void HandleCombat() { }
        public override void HandleJump() { }
    }
}
