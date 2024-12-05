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
        HandleCameraRotation();
        HandleMovement();
        HandleCombat();
        HandleJump();
        UpdateAnimations();
    }
    
    private void HandleCameraRotation()
    {
        // Get mouse input
        Vector2 cameraInput = playerInput.CameraInput;
        
        // Update rotation values
        currentRotationY += cameraInput.x;
        currentRotationX -= cameraInput.y; // Inverted for natural camera feel
        currentRotationX = Mathf.Clamp(currentRotationX, minVerticalAngle, maxVerticalAngle);
        
        // Apply rotation to player (horizontal only)
        transform.rotation = Quaternion.Euler(0f, currentRotationY, 0f);
        
        // Apply full rotation to camera
        cameraTransform.rotation = Quaternion.Euler(currentRotationX, currentRotationY, 0f);
    }
    
    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        Vector3 movement = new Vector3(horizontal, 0f, vertical).normalized;
        
        if (movement.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
            float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            
            moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        }
    }
    
    private void HandleCombat()
    {
        // Implement combat logic here
    }
    
    private void HandleJump()
    {
        // Implement jump logic here
    }
    
    private void UpdateAnimations()
    {
        bool isMoving = moveDirection.magnitude >= 0.1f;
        animator.SetBool("IsMoving", isMoving);
    }
} 