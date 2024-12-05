using UnityEngine;
using Cinemachine;
using System.Collections;
using Player.Input;

public class CameraController : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private CinemachineFreeLook normalCamera;
    [SerializeField] private CinemachineFreeLook combatCamera;
    [SerializeField] private CinemachineFreeLook sprintCamera;
    
    [Header("Camera Settings")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float sprintFOV = 70f;
    [SerializeField] private float combatFOV = 55f;
    
    [Header("Combat Settings")]
    [SerializeField] private float combatCameraHeight = 2f;
    [SerializeField] private float combatCameraDistance = 4f;
    
    [Header("Follow Settings")]
    [SerializeField] private float followDistance = 5f;
    [SerializeField] private float followHeight = 2f;
    [SerializeField] private float followDamping = 2f;
    [SerializeField] private Transform cameraTarget;
    
    [Header("Input Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    
    private void Awake()
    {
        SetupCameraComponents();
        InitializeCameraSettings();
    }
    
    private void SetupCameraComponents()
    {
        if (normalCamera == null || combatCamera == null || sprintCamera == null)
        {
            Debug.LogError("One or more virtual cameras are not assigned in CameraController!");
            return;
        }

        if (cameraTarget == null)
        {
            Debug.LogError("Camera target is not assigned!");
            return;
        }

        // Set the camera targets
        normalCamera.Follow = normalCamera.LookAt = cameraTarget;
        combatCamera.Follow = combatCamera.LookAt = cameraTarget;
        sprintCamera.Follow = sprintCamera.LookAt = cameraTarget;
    }
    
    private void InitializeCameraSettings()
    {
        // Normal camera setup
        normalCamera.m_Lens.FieldOfView = normalFOV;
        SetupFreeLookRig(normalCamera, followDistance, followHeight);
        
        // Combat camera setup
        combatCamera.m_Lens.FieldOfView = combatFOV;
        SetupFreeLookRig(combatCamera, combatCameraDistance, combatCameraHeight);
        
        // Sprint camera setup
        sprintCamera.m_Lens.FieldOfView = sprintFOV;
        SetupFreeLookRig(sprintCamera, followDistance * 1.2f, followHeight * 1.1f);
        
        // Set initial priorities
        normalCamera.Priority = 10;
        combatCamera.Priority = 0;
        sprintCamera.Priority = 0;
        
        // Configure input settings for all cameras
        ConfigureCameraInput(normalCamera);
        ConfigureCameraInput(combatCamera);
        ConfigureCameraInput(sprintCamera);
    }

    private void SetupFreeLookRig(CinemachineFreeLook camera, float distance, float height)
    {
        // Setup the three rigs (top, middle, bottom)
        camera.m_Orbits[0] = new CinemachineFreeLook.Orbit(height * 1.5f, distance * 0.8f); // Top rig
        camera.m_Orbits[1] = new CinemachineFreeLook.Orbit(height, distance);               // Middle rig
        camera.m_Orbits[2] = new CinemachineFreeLook.Orbit(height * 0.5f, distance * 0.8f); // Bottom rig
        
        // Set common properties
        camera.m_XAxis.Value = 0f;
        camera.m_YAxis.Value = 0.5f; // Center the camera vertically
    }
    
    public void SetCombatMode(bool isInCombat)
    {
        combatCamera.Priority = isInCombat ? 20 : 0;
        normalCamera.Priority = isInCombat ? 0 : 10;
    }
    
    public void SetSprintMode(bool isSprinting)
    {
        sprintCamera.Priority = isSprinting ? 30 : 0;
        normalCamera.Priority = isSprinting ? 0 : 10;
    }
    
    private void LateUpdate()
    {
        // Manual input handling for WebGL
        var input = GetComponent<PlayerInput>();
        if (input != null)
        {
            Vector2 cameraInput = input.CameraInput;
            normalCamera.m_XAxis.m_InputAxisValue = cameraInput.x;
            normalCamera.m_YAxis.m_InputAxisValue = cameraInput.y;
            
            combatCamera.m_XAxis.m_InputAxisValue = cameraInput.x;
            combatCamera.m_YAxis.m_InputAxisValue = cameraInput.y;
            
            sprintCamera.m_XAxis.m_InputAxisValue = cameraInput.x;
            sprintCamera.m_YAxis.m_InputAxisValue = cameraInput.y;
        }
        
        // Update camera position based on priorities
        if (sprintCamera.Priority > 0)
        {
            UpdateCameraSettings(sprintCamera, sprintFOV);
        }
        else if (combatCamera.Priority > 0)
        {
            UpdateCameraSettings(combatCamera, combatFOV);
        }
        else
        {
            UpdateCameraSettings(normalCamera, normalFOV);
        }
    }
    
    private void UpdateCameraSettings(CinemachineFreeLook camera, float fov)
    {
        camera.m_Lens.FieldOfView = fov;
    }

    private void ConfigureCameraInput(CinemachineFreeLook camera)
    {
        // Reset initial values
        camera.m_XAxis.Value = 0f;
        camera.m_YAxis.Value = 0.5f;
        
        // Use Unity's input system through Cinemachine
        camera.m_XAxis.m_InputAxisName = "Mouse X";
        camera.m_YAxis.m_InputAxisName = "Mouse Y";
        
        // WebGL-friendly settings
        camera.m_XAxis.m_MaxSpeed = 175f;
        camera.m_YAxis.m_MaxSpeed = 1.5f;
        
        // Minimal response times for WebGL
        camera.m_XAxis.m_AccelTime = 0f;
        camera.m_XAxis.m_DecelTime = 0f;
        camera.m_YAxis.m_AccelTime = 0f;
        camera.m_YAxis.m_DecelTime = 0f;
    }
} 