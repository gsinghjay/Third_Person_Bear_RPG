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
        // Top rig (high angle)
        camera.m_Orbits[0] = new CinemachineFreeLook.Orbit(height * 1.2f, distance);
        
        // Middle rig (eye level)
        camera.m_Orbits[1] = new CinemachineFreeLook.Orbit(height * 0.8f, distance);
        
        // Bottom rig (low angle)
        camera.m_Orbits[2] = new CinemachineFreeLook.Orbit(height * 0.5f, distance);
        
        // Increase follow speed and responsiveness for each rig
        for (int i = 0; i < 3; i++)
        {
            var rig = camera.GetRig(i);
            var transposer = rig.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer != null)
            {
                transposer.m_XDamping = 0.5f;  // Reduced from 1f
                transposer.m_YDamping = 0.5f;  // Reduced from 1f
                transposer.m_ZDamping = 0.5f;  // Reduced from 1f
                
                // Set follow offset
                transposer.m_FollowOffset = new Vector3(0, height, -distance);
                transposer.m_BindingMode = CinemachineTransposer.BindingMode.LockToTargetWithWorldUp;
            }
        }
        
        // Adjust camera rotation speeds
        camera.m_XAxis.m_MaxSpeed = 200f;  // Reduced from 300f
        camera.m_YAxis.m_MaxSpeed = 1.5f;  // Reduced from 2f
        
        // Make camera more responsive but with some smoothing
        camera.m_XAxis.m_AccelTime = 0.2f;
        camera.m_XAxis.m_DecelTime = 0.2f;
        camera.m_YAxis.m_AccelTime = 0.2f;
        camera.m_YAxis.m_DecelTime = 0.2f;
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
            UpdateCameraRotation(cameraInput);
        }
    }
    
    private void UpdateCameraRotation(Vector2 input)
    {
        var activeCamera = GetActiveCamera();
        if (activeCamera != null)
        {
            activeCamera.m_XAxis.m_InputAxisValue = input.x * mouseSensitivity;
            activeCamera.m_YAxis.m_InputAxisValue = input.y * mouseSensitivity;
        }
    }
    
    private CinemachineFreeLook GetActiveCamera()
    {
        if (sprintCamera.Priority > 0) return sprintCamera;
        if (combatCamera.Priority > 0) return combatCamera;
        return normalCamera;
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

    private void SetupCamera()
    {
        if (cameraTarget == null)
        {
            GameObject targetObj = new GameObject("CameraTarget");
            cameraTarget = targetObj.transform;
            cameraTarget.parent = transform;
            cameraTarget.localPosition = new Vector3(0f, 1.5f, -3f); // Adjust Z for distance
            cameraTarget.localRotation = Quaternion.identity;
        }
    }

    public void SetCameraTarget(Transform target)
    {
        cameraTarget = target;
        if (normalCamera != null) 
        {
            normalCamera.Follow = normalCamera.LookAt = target;
        }
        if (combatCamera != null)
        {
            combatCamera.Follow = combatCamera.LookAt = target;
        }
        if (sprintCamera != null)
        {
            sprintCamera.Follow = sprintCamera.LookAt = target;
        }
        Debug.Log($"CameraController: Camera target set to {target.name}");
    }
} 