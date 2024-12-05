using UnityEngine;

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
    
    private Animator animator;
    private CharacterController characterController;
    private CameraController cameraController;
    private IPlayerInput playerInput;
    private Vector3 moveDirection;
    private bool isInCombat;
    private bool isSprinting;
    private bool isJumping;
    private float verticalVelocity;
    private float currentRotationX = 0f;
    private float currentRotationY = 0f;
    private Transform cameraTransform;
    
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        characterController = GetComponent<CharacterController>();
        cameraController = Camera.main.GetComponent<CameraController>();
        playerInput = GetComponent<PlayerInput>();
        cameraTransform = Camera.main.transform;
        
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Setup camera target
        if (cameraTarget == null)
        {
            GameObject targetObj = new GameObject("CameraTarget");
            cameraTarget = targetObj.transform;
            cameraTarget.parent = transform;
            cameraTarget.localPosition = new Vector3(0f, 1.5f, 0f); // Adjust height as needed
            cameraTarget.localRotation = Quaternion.identity;
        }
    }
    
    private void Start()
    {
        SetupSceneElements();
        SetupPlayerTag();
    }
    
    private void SetupSceneElements()
    {
        PositionPlayerAtTerrainCenter();
        
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
    
    private void PositionPlayerAtTerrainCenter()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain != null)
        {
            TerrainData terrainData = terrain.terrainData;
            
            // Get terrain dimensions
            Vector3 terrainSize = terrainData.size;
            Vector3 terrainPosition = terrain.transform.position;
            
            // Calculate village center (which is at 0.5, 0.5 in normalized coordinates)
            float centerX = terrainPosition.x + (terrainSize.x * 0.5f);
            float centerZ = terrainPosition.z + (terrainSize.z * 0.5f);
            
            // Sample height at village center
            Vector3 centerPoint = new Vector3(centerX, 0, centerZ);
            float height = terrain.SampleHeight(centerPoint);
            
            // Add a small offset to prevent clipping
            height += 1f;
            
            // Set position
            transform.position = new Vector3(centerX, height, centerZ);
            
            Debug.Log($"Positioning player at village center: {transform.position}");
        }
        else
        {
            Debug.LogWarning("No active terrain found!");
        }
    }
    
    private void SetupPlayerTag()
    {
        gameObject.tag = "Player";
    }
    
    private void Update()
    {
        HandleMovement();
        HandleCombat();
        HandleJump();
        UpdateAnimations();
    }
    
    private void HandleMovement()
    {
        Vector2 input = playerInput.MovementInput;
        
        if (input.magnitude >= 0.1f)
        {
            // Cache camera transform for better performance
            Transform mainCameraTransform = Camera.main.transform;
            
            // Get the camera's forward and right vectors
            Vector3 cameraForward = mainCameraTransform.forward;
            Vector3 cameraRight = mainCameraTransform.right;
            
            // Project vectors onto the horizontal plane (y = 0)
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();
            
            // Calculate move direction relative to camera orientation
            moveDirection = (cameraForward * input.y + cameraRight * input.x).normalized;
            
            // Optimize rotation calculation
            if (moveDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    Quaternion.LookRotation(moveDirection),
                    rotationSpeed * Time.deltaTime * 100f
                );
            }
            
            // Move in the calculated direction
            float currentSpeed = playerInput.IsSprinting ? sprintSpeed : moveSpeed;
            Vector3 movement = moveDirection * (currentSpeed * Time.deltaTime);
            characterController.Move(movement + new Vector3(0, verticalVelocity * Time.deltaTime, 0));
        }
        else
        {
            moveDirection = Vector3.zero;
            characterController.Move(new Vector3(0, verticalVelocity * Time.deltaTime, 0));
        }
    }
    
    private void HandleCombat()
    {
        // Implement combat logic here
    }
    
    private void HandleJump()
    {
        if (characterController.isGrounded)
        {
            verticalVelocity = -0.5f; // Small downward force when grounded
            
            if (playerInput.IsJumping)
            {
                verticalVelocity = jumpForce;
                animator.SetTrigger("Jump");
            }
        }
        else
        {
            // Apply gravity
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
        }
        
        // Apply vertical movement
        Vector3 verticalMovement = new Vector3(0f, verticalVelocity * Time.deltaTime, 0f);
        characterController.Move(verticalMovement);
        
        // Update animation parameters
        animator.SetFloat("VerticalVelocity", verticalVelocity);
        animator.SetBool("IsGrounded", characterController.isGrounded);
    }
    
    private void UpdateAnimations()
    {
        bool isMoving = moveDirection.magnitude >= 0.1f;
        animator.SetBool("IsMoving", isMoving);
    }
} 