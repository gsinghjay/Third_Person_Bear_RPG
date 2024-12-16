using UnityEngine;
using TMPro;
using Combat;

public class CooldownUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI basicAttackText;
    [SerializeField] private TextMeshProUGUI specialAttackText;
    
    [Header("Text Format")]
    [SerializeField] private string basicAttackFormat = "Basic Attack: {0:0.0}s";
    [SerializeField] private string specialAttackFormat = "Special Attack: {0:0.0}s";
    
    private CooldownManager cooldownManager;
    private const string BASIC_ATTACK = "BasicAttack";
    private const string SPECIAL_ATTACK = "SpecialAttack";

    private void Start()
    {
        // Find the CooldownManager in the scene
        cooldownManager = FindObjectOfType<CooldownManager>();
        
        if (cooldownManager == null)
        {
            Debug.LogError("CooldownUI: No CooldownManager found in scene!");
            enabled = false;
            return;
        }

        // Initialize text
        UpdateCooldownTexts();
    }

    private void Update()
    {
        UpdateCooldownTexts();
    }

    private void UpdateCooldownTexts()
    {
        if (basicAttackText != null)
        {
            float basicCooldown = cooldownManager.GetRemainingTime(BASIC_ATTACK);
            basicAttackText.text = string.Format(basicAttackFormat, 
                basicCooldown > 0 ? basicCooldown : 0);
            
            // Optional: Change color based on availability
            basicAttackText.color = cooldownManager.IsReady(BASIC_ATTACK) ? 
                Color.green : Color.red;
        }

        if (specialAttackText != null)
        {
            float specialCooldown = cooldownManager.GetRemainingTime(SPECIAL_ATTACK);
            specialAttackText.text = string.Format(specialAttackFormat, 
                specialCooldown > 0 ? specialCooldown : 0);
            
            // Optional: Change color based on availability
            specialAttackText.color = cooldownManager.IsReady(SPECIAL_ATTACK) ? 
                Color.green : Color.red;
        }
    }
}
