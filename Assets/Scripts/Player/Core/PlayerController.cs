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
        [SerializeField] private float defendTransitionSpeed = 0.3f;
        
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
        public Animator Animator { get; private set; }
        public CharacterController CharacterController { get; private set; }
        public IPlayerInput PlayerInput { get; private set; }

        private IPlayerState currentState;
        private Transform cameraTransform;
        
        private void Awake()
        {
            Animator = GetComponentInChildren<Animator>();
            CharacterController = GetComponent<CharacterController>();
            PlayerInput = GetComponent<PlayerInput>();
            cameraTransform = Camera.main.transform;
            
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
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;
            
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();
            
            return (cameraForward * input.y + cameraRight * input.x).normalized;
        }

        public void RotateTowardsMoveDirection(Vector3 moveDirection)
        {
            if (moveDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    Quaternion.LookRotation(moveDirection),
                    rotationSpeed * Time.deltaTime * 100f
                );
            }
        }

        public void Move(Vector3 movement)
        {
            CharacterController.Move(movement * Time.deltaTime + new Vector3(0, VerticalVelocity * Time.deltaTime, 0));
        }

        private void SetupCamera()
        {
            if (cameraTarget == null)
            {
                GameObject targetObj = new GameObject("CameraTarget");
                cameraTarget = targetObj.transform;
                cameraTarget.parent = transform;
                cameraTarget.localPosition = new Vector3(0f, 1.5f, 0f);
                cameraTarget.localRotation = Quaternion.identity;
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
    }
} 