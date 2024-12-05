using UnityEngine;

public interface IPlayerInput
{
    Vector2 MovementInput { get; }
    bool IsJumping { get; }
} 