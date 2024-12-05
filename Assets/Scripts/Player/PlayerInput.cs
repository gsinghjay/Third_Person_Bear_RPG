using UnityEngine;

public class PlayerInput : MonoBehaviour, IPlayerInput
{
    public Vector2 MovementInput => new Vector2(
        Input.GetAxisRaw("Horizontal"),
        Input.GetAxisRaw("Vertical")
    );
    
    public bool IsJumping => Input.GetButtonDown("Jump");
} 