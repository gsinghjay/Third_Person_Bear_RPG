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
        normalFraming = normalCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        combatFraming = combatCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        sprintFraming = sprintCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
    }
    
    private void InitializeCameraSettings()
    {
        // Normal camera setup
        normalCamera.m_Lens.FieldOfView = normalFOV;
        normalFraming.m_CameraDistance = followDistance;
        normalFraming.m_TrackedObjectOffset.y = followHeight;
        normalFraming.m_XDamping = followDamping;
        normalFraming.m_YDamping = followDamping;
        normalFraming.m_ZDamping = followDamping;
        
        // Combat camera setup
        combatCamera.m_Lens.FieldOfView = combatFOV;
        combatFraming.m_CameraDistance = combatCameraDistance;
        combatFraming.m_TrackedObjectOffset.y = combatCameraHeight;
        
        // Sprint camera setup
        sprintCamera.m_Lens.FieldOfView = sprintFOV;
        sprintFraming.m_CameraDistance = followDistance * 1.2f;
        sprintFraming.m_TrackedObjectOffset.y = followHeight * 1.1f;
        
        // Set initial priorities
        normalCamera.Priority = 10;
        combatCamera.Priority = 0;
        sprintCamera.Priority = 0;
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
} 