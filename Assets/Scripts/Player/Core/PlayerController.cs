using UnityEngine;
using Player.Input;
using Player.Input.Interfaces;
using Player.States;

namespace Player.Core
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float jumpForce = 5f;
        
        [Header("Combat Settings")]
        [SerializeField] private float attackDuration = 0.5f;
        
        [Header("Camera Settings")]
        [SerializeField] private float minVerticalAngle = -30f;
        [SerializeField] private float maxVerticalAngle = 60f;
        [SerializeField] private Transform cameraTarget;

        // Properties for states to access
        public float MoveSpeed => moveSpeed;
        public float SprintSpeed => sprintSpeed;
        public float JumpForce => jumpForce;
        public float AttackDuration => attackDuration;
        public float VerticalVelocity { get; set; }
        public PlayerAnimationController AnimationController { get; private set; }
        public CharacterController CharacterController { get; private set; }
        public IPlayerInput PlayerInput { get; private set; }

        private IPlayerState currentState;
        private Transform cameraTransform;
        
        private void Awake()
        {
            AnimationController = GetComponentInChildren<PlayerAnimationController>();
            if (AnimationController == null)
            {
                Debug.LogError("PlayerController: No PlayerAnimationController found in children!");
            }
            
            CharacterController = GetComponent<CharacterController>();
            if (CharacterController == null)
            {
                Debug.LogError("PlayerController: No CharacterController found!");
            }
            
            PlayerInput = GetComponent<PlayerInput>();
            if (PlayerInput == null)
            {
                Debug.LogError("PlayerController: No PlayerInput found!");
            }
            
            cameraTransform = Camera.main.transform;
            
            Debug.Log($"PlayerController: Initialized with AnimationController: {AnimationController != null}, CharacterController: {CharacterController != null}, PlayerInput: {PlayerInput != null}");
            
            SetupCamera();
            LockCursor();
        }
        
        private void Start()
        {
            SetupSceneElements();
            ChangeState(new IdleState(this));
        }

        private void Update()
        {
            currentState?.Update();
            HandleInput();
        }

        public void ChangeState(IPlayerState newState)
        {
            currentState?.Exit();
            currentState = newState;
            currentState?.Enter();
        }

        private void HandleInput()
        {
            currentState?.HandleMovement(PlayerInput.MovementInput);
            currentState?.HandleCombat();
            currentState?.HandleJump();
        }

        public Vector3 CalculateMoveDirection(Vector2 input)
        {
            if (input.magnitude < 0.1f) return Vector3.zero;

            // Get camera forward and right vectors
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            
            // Project vectors onto XZ plane
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            // Calculate target direction based on input
            Vector3 moveDirection = (forward * input.y + right * input.x);
            moveDirection.Normalize();

            return moveDirection;
        }

        public void Move(Vector3 moveDirection, float speedMultiplier = 1f)
        {
            Vector3 movement = Vector3.zero;
            
            // Handle horizontal movement
            if (moveDirection.magnitude >= 0.1f)
            {
                movement = moveDirection * (MoveSpeed * speedMultiplier);
            }
            
            // Apply gravity/vertical movement
            movement.y = VerticalVelocity;
            
            // Apply final movement
            CharacterController.Move(movement * Time.deltaTime);
        }

        private void SetupCamera()
        {
            if (cameraTarget == null)
            {
                GameObject targetObj = new GameObject("CameraTarget");
                cameraTarget = targetObj.transform;
                cameraTarget.SetParent(transform);
                cameraTarget.localPosition = new Vector3(0f, 1.5f, 0f);
                cameraTarget.localRotation = Quaternion.identity;
            }
            
            var cameraController = Camera.main.GetComponent<CameraController>();
            if (cameraController != null)
            {
                cameraController.SetCameraTarget(cameraTarget);
                Debug.Log("PlayerController: Camera target set on CameraController");
            }
            else
            {
                Debug.LogError("PlayerController: No CameraController found on Main Camera!");
            }
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void SetupSceneElements()
        {
            gameObject.tag = "Player";
            PositionPlayerAtTerrainCenter();
            SetupLighting();
        }

        private void PositionPlayerAtTerrainCenter()
        {
            Terrain terrain = Terrain.activeTerrain;
            if (terrain != null)
            {
                TerrainData terrainData = terrain.terrainData;
                Vector3 terrainSize = terrainData.size;
                Vector3 terrainPosition = terrain.transform.position;
                
                float centerX = terrainPosition.x + (terrainSize.x * 0.5f);
                float centerZ = terrainPosition.z + (terrainSize.z * 0.5f);
                float height = terrain.SampleHeight(new Vector3(centerX, 0, centerZ)) + 1f;
                
                transform.position = new Vector3(centerX, height, centerZ);
            }
        }

        private void SetupLighting()
        {
            Light mainLight = GameObject.Find("Directional Light")?.GetComponent<Light>();
            if (mainLight != null)
            {
                mainLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                mainLight.intensity = 1.1f;
                mainLight.shadowStrength = 0.6f;
                mainLight.shadows = LightShadows.Hard;
                
                QualitySettings.shadowResolution = ShadowResolution.Medium;
                QualitySettings.shadowDistance = 50f;
                QualitySettings.shadowCascades = 2;
                QualitySettings.shadows = ShadowQuality.HardOnly;
            }
        }

        private void LateUpdate()
        {
            if (cameraTarget != null)
            {
                Vector3 targetPosition = transform.position + new Vector3(0f, 1.5f, 0f);
                cameraTarget.position = Vector3.Lerp(cameraTarget.position, targetPosition, Time.deltaTime * 10f);
            }
        }

        public void RotateTowardsMoveDirection(Vector3 moveDirection)
        {
            if (moveDirection.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Lerp(transform.rotation, 
                    Quaternion.Euler(0f, targetAngle, 0f), 
                    Time.deltaTime * rotationSpeed);
            }
        }
    }
} 