using UnityEngine;
using Player.Core;

namespace Player.States
{
    public class DeathState : PlayerStateBase
    {
        private bool deathAnimationPlayed = false;

        public DeathState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();
            Debug.Log("Player entered death state");
            
            if (animationController != null)
            {
                animationController.PlayDeathAnimation();
                deathAnimationPlayed = true;
            }
            
            // Disable player input and movement
            if (playerController != null)
            {
                playerController.enabled = false;
            }
            
            if (characterController != null)
            {
                characterController.enabled = false;
            }
        }

        public override void Update()
        {
            base.Update();
            
            // Optional: Check if death animation has finished playing
            if (deathAnimationPlayed && animationController != null)
            {
                if (!animationController.IsDeathAnimationPlaying())
                {
                    // Handle post-death logic here (e.g., show game over screen)
                    Debug.Log("Death animation completed");
                }
            }
        }

        public override void HandleMovement(Vector2 input) { }
        public override void HandleCombat() { }
        public override void HandleJump() { }
    }
}
