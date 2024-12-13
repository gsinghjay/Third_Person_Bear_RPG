using UnityEngine;
using Player.Input;
using Player.Core;
using Enemies.Types;
using System.Collections;

public class CombatInputHandler : MonoBehaviour
{
    [SerializeField] private WeaponController weaponController;
    private PlayerInput playerInput;
    private PlayerController playerController;
    private PlayerAnimationController animationController;
    
    [Header("Cooldown Settings")]
    [SerializeField] private float basicAttackCooldown = 0.5f;
    [SerializeField] private float specialAttackCooldown = 1.5f;
    
    private bool canBasicAttack = true;
    private bool canSpecialAttack = true;
    
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerController = GetComponent<PlayerController>();
        animationController = GetComponentInChildren<PlayerAnimationController>();

        if (weaponController == null)
        {
            Debug.LogError("WeaponController not assigned to CombatInputHandler!");
        }
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

        if (canBasicAttack && Input.GetMouseButtonDown(0))
        {
            PerformAttack();
            StartCoroutine(AttackCooldown(true));
        }
        
        // Add special attack input check
        if (canSpecialAttack && Input.GetMouseButtonDown(1))  // Right click
        {
            PerformSpecialAttack();
            StartCoroutine(AttackCooldown(false));
        }
    }

    private IEnumerator AttackCooldown(bool isBasicAttack)
    {
        if (isBasicAttack)
        {
            canBasicAttack = false;
            yield return new WaitForSeconds(basicAttackCooldown);
            canBasicAttack = true;
        }
        else
        {
            canSpecialAttack = false;
            yield return new WaitForSeconds(specialAttackCooldown);
            canSpecialAttack = true;
        }
    }

    public void PerformAttack()
    {
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
}