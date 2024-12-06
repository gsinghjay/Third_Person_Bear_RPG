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
    private bool canAttack = true;
    [SerializeField] private float attackCooldown = 0.5f;
    
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerController = GetComponent<PlayerController>();

        if (weaponController == null)
        {
            Debug.LogError("WeaponController not assigned to CombatInputHandler!");
        }
    }
    
    private void Update()
    {
        // Handle element switching
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            weaponController.SetDamageType(DamageType.Physical);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            weaponController.SetDamageType(DamageType.Fire);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            weaponController.SetDamageType(DamageType.Ice);
        }

        // Handle attack input
        if (canAttack && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            PerformAttack();
            StartCoroutine(AttackCooldown());
        }
    }

    private IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    public void PerformAttack()
    {
        Debug.Log("CombatInputHandler: PerformAttack called");
        if (weaponController != null)
        {
            weaponController.Attack();
        }
        else
        {
            Debug.LogError("WeaponController is null in CombatInputHandler!");
        }
    }

    public PlayerInput PlayerInput => playerInput;
}