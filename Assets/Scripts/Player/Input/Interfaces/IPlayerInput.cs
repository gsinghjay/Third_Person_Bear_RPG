using UnityEngine;

namespace Player.Input.Interfaces
{
    public interface IPlayerInput
    {
        Vector2 MovementInput { get; }
        Vector2 CameraInput { get; }
        bool IsJumping { get; }
        bool IsSprinting { get; }
        bool IsAttacking { get; }
        bool IsDefending { get; }
    }
} 