using UnityEngine;
using Player.Input.Interfaces;

namespace Player.Input
{
    public class PlayerInput : MonoBehaviour, IPlayerInput
    {
        [Header("Input Settings")]
        [SerializeField] private float mouseSensitivity = 1f;
        [SerializeField] private bool invertMouseY = false;
        
        public Vector2 MovementInput => new Vector2(
            UnityEngine.Input.GetAxisRaw("Horizontal"),
            UnityEngine.Input.GetAxisRaw("Vertical")
        ).normalized;
        
        public Vector2 CameraInput => new Vector2(
            UnityEngine.Input.GetAxis("Mouse X") * mouseSensitivity,
            UnityEngine.Input.GetAxis("Mouse Y") * mouseSensitivity * (invertMouseY ? -1 : 1)
        );
        
        public bool IsJumping => UnityEngine.Input.GetButtonDown("Jump");
        public bool IsSprinting => UnityEngine.Input.GetKey(KeyCode.LeftShift);
        public bool IsAttacking => UnityEngine.Input.GetMouseButtonDown(0);

        private void Update()
        {
            if (IsAttacking)
            {
                Debug.Log("PlayerInput: Attack input detected");
            }
        }
    }
} 