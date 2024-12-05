using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    
    private Animator animator;
    private CharacterController characterController;
    private Vector3 moveDirection;
    
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        characterController = GetComponent<CharacterController>();
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
        HandleAnimation();
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
    
    private void HandleAnimation()
    {
        bool isMoving = moveDirection.magnitude >= 0.1f;
        animator.SetBool("IsMoving", isMoving);
    }
} 