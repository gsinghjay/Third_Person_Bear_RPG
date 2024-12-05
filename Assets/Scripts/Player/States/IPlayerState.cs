using UnityEngine;

namespace Player.States
{
    public interface IPlayerState
    {
        void Enter();
        void Exit();
        void Update();
        void HandleMovement(Vector2 input);
        void HandleCombat();
        void HandleJump();
    }
} 