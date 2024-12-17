using UnityEngine;
using Player.Input;
using Player.Core;
using Enemies.Types;

namespace Combat
{
    public class CombatInputHandler : MonoBehaviour
    {
        [SerializeField] private WeaponController weaponController;
        [SerializeField] private CooldownManager cooldownManager;
        private PlayerInput playerInput;
        private PlayerController playerController;
        private PlayerAnimationController animationController;
        
        [Header("Cooldown Settings")]
        [SerializeField] private float basicAttackCooldown = 0.5f;
        [SerializeField] private float specialAttackCooldown = 1.5f;
        
        private const string BASIC_ATTACK = "BasicAttack";
        private const string SPECIAL_ATTACK = "SpecialAttack";
        
        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            playerController = GetComponent<PlayerController>();
            animationController = GetComponentInChildren<PlayerAnimationController>();

            if (weaponController == null)
            {
                Debug.LogError("WeaponController not assigned to CombatInputHandler!");
            }

            InitializeCooldownManager();
        }

        private void InitializeCooldownManager()
        {
            if (cooldownManager == null)
            {
                cooldownManager = gameObject.AddComponent<CooldownManager>();
            }
            
            cooldownManager.RegisterCooldown(BASIC_ATTACK, basicAttackCooldown);
            cooldownManager.RegisterCooldown(SPECIAL_ATTACK, specialAttackCooldown);
            
            Debug.Log($"CombatInputHandler: Cooldowns initialized - Basic: {basicAttackCooldown}s, Special: {specialAttackCooldown}s");
        }
        
        private void Update()
        {
            HandleElementSwitching();
            HandleAttackInput();
        }

        private void HandleElementSwitching()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                weaponController.SetDamageType(DamageType.Physical);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                weaponController.SetDamageType(DamageType.Fire);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                weaponController.SetDamageType(DamageType.Ice);
        }

        private void HandleAttackInput()
        {
            // Don't allow attacks while jumping
            if (animationController != null && animationController.IsJumping())
            {
                return;
            }

            // Check for basic attack input and cooldown
            if (Input.GetMouseButtonDown(0))
            {
                if (cooldownManager.IsReady(BASIC_ATTACK))
                {
                    PerformAttack();
                    cooldownManager.TriggerCooldown(BASIC_ATTACK);
                    Debug.Log($"Basic attack performed, cooldown started: {basicAttackCooldown}s");
                }
                else
                {
                    float remainingTime = cooldownManager.GetRemainingTime(BASIC_ATTACK);
                    Debug.Log($"Basic attack on cooldown: {remainingTime:F1}s remaining");
                }
            }
            
            // Check for special attack input and cooldown
            if (Input.GetMouseButtonDown(1))
            {
                if (cooldownManager.IsReady(SPECIAL_ATTACK))
                {
                    PerformSpecialAttack();
                    cooldownManager.TriggerCooldown(SPECIAL_ATTACK);
                    Debug.Log($"Special attack performed, cooldown started: {specialAttackCooldown}s");
                }
                else
                {
                    float remainingTime = cooldownManager.GetRemainingTime(SPECIAL_ATTACK);
                    Debug.Log($"Special attack on cooldown: {remainingTime:F1}s remaining");
                }
            }
        }

        public bool CanBasicAttack() => cooldownManager.IsReady(BASIC_ATTACK);
        public bool CanSpecialAttack() => cooldownManager.IsReady(SPECIAL_ATTACK);

        public void PerformAttack()
        {
            if (!cooldownManager.IsReady(BASIC_ATTACK)) return;

            Debug.Log("CombatInputHandler: PerformAttack called");
            
            if (animationController != null)
            {
                Debug.Log("CombatInputHandler: Playing attack animation");
                animationController.PlayAttack();
            }
            
            if (weaponController != null)
            {
                weaponController.Attack();
            }
            else
            {
                Debug.LogError("WeaponController is null in CombatInputHandler!");
            }
        }

        public void PerformSpecialAttack()
        {
            if (!cooldownManager.IsReady(SPECIAL_ATTACK)) return;

            Debug.Log("CombatInputHandler: Starting special attack");
            
            if (weaponController != null)
            {
                if (animationController != null)
                {
                    Debug.Log("CombatInputHandler: Playing special attack animation");
                    animationController.PlaySpecialAttack();
                }
                else
                {
                    Debug.LogError("CombatInputHandler: Animation controller is null!");
                }
                
                weaponController.PerformSpecialAttack();
            }
            else
            {
                Debug.LogError("CombatInputHandler: Weapon controller is null!");
            }
        }

        public PlayerInput PlayerInput => playerInput;

        #if UNITY_EDITOR
        private void OnGUI()
        {
            // Debug cooldown information in editor
            GUILayout.BeginArea(new Rect(10, 10, 200, 100));
            GUILayout.Label($"Basic Attack Cooldown: {cooldownManager.GetRemainingTime(BASIC_ATTACK):F1}s");
            GUILayout.Label($"Special Attack Cooldown: {cooldownManager.GetRemainingTime(SPECIAL_ATTACK):F1}s");
            GUILayout.EndArea();
        }
        #endif
    }
}