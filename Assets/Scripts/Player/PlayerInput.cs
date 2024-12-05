using UnityEngine;

public class PlayerInput : MonoBehaviour, IPlayerInput
{
    [Header("Input Settings")]
    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private bool invertMouseY = false;
    
    public Vector2 MovementInput => new Vector2(
        Input.GetAxisRaw("Horizontal"),
        Input.GetAxisRaw("Vertical")
    ).normalized;
    
    public Vector2 CameraInput => new Vector2(
        Input.GetAxis("Mouse X") * mouseSensitivity,
        Input.GetAxis("Mouse Y") * mouseSensitivity * (invertMouseY ? -1 : 1)
    );
    
    public bool IsJumping => Input.GetButtonDown("Jump");
    public bool IsSprinting => Input.GetKey(KeyCode.LeftShift);
    public bool IsAttacking => Input.GetMouseButtonDown(0);
    public bool IsDefending => Input.GetMouseButton(1);
} 