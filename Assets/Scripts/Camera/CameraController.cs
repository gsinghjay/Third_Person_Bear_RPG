using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private CinemachineVirtualCamera normalCamera;
    [SerializeField] private CinemachineVirtualCamera combatCamera;
    [SerializeField] private CinemachineVirtualCamera sprintCamera;
    
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
    
    private CinemachineFramingTransposer normalFraming;
    private CinemachineFramingTransposer combatFraming;
    private CinemachineFramingTransposer sprintFraming;
    
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

        normalFraming = normalCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        combatFraming = combatCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        sprintFraming = sprintCamera.GetCinemachineComponent<CinemachineFramingTransposer>();

        if (normalFraming == null || combatFraming == null || sprintFraming == null)
        {
            Debug.LogError("One or more cameras are missing CinemachineFramingTransposer component!");
            return;
        }
    }
    
    private void InitializeCameraSettings()
    {
        if (normalFraming == null || combatFraming == null || sprintFraming == null)
        {
            return;
        }

        // Normal camera setup
        normalCamera.m_Lens.FieldOfView = normalFOV;
        normalFraming.m_CameraDistance = followDistance;
        normalFraming.m_TrackedObjectOffset.y = followHeight;
        normalFraming.m_XDamping = 0.05f;
        normalFraming.m_YDamping = 0.05f;
        normalFraming.m_ZDamping = 0.1f;
        
        // Disable Aim component if it exists
        var aimComponent = normalCamera.GetCinemachineComponent<CinemachineComposer>();
        if (aimComponent != null)
        {
            aimComponent.enabled = false;
        }
        
        // Combat camera setup
        combatCamera.m_Lens.FieldOfView = combatFOV;
        combatFraming.m_CameraDistance = combatCameraDistance;
        combatFraming.m_TrackedObjectOffset.y = combatCameraHeight;
        combatFraming.m_XDamping = 0.1f;
        combatFraming.m_YDamping = 0.1f;
        combatFraming.m_ZDamping = 0.5f;
        
        // Sprint camera setup
        sprintCamera.m_Lens.FieldOfView = sprintFOV;
        sprintFraming.m_CameraDistance = followDistance * 1.2f;
        sprintFraming.m_TrackedObjectOffset.y = followHeight * 1.1f;
        sprintFraming.m_XDamping = 0.1f;
        sprintFraming.m_YDamping = 0.1f;
        sprintFraming.m_ZDamping = 0.5f;
        
        // Set initial priorities
        normalCamera.Priority = 10;
        combatCamera.Priority = 0;
        sprintCamera.Priority = 0;
        
        // Set the update method
        var brain = Camera.main.GetComponent<CinemachineBrain>();
        if (brain != null)
        {
            brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.FixedUpdate;
            brain.m_BlendUpdateMethod = CinemachineBrain.BrainUpdateMethod.FixedUpdate;
        }
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
    
    private void UpdateCameraSettings(CinemachineVirtualCamera camera, float fov)
    {
        camera.m_Lens.FieldOfView = fov;
    }
} 