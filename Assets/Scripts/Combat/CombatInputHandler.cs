using UnityEngine;
using UnityEngine.InputSystem;
using Enemies.Types;

[RequireComponent(typeof(PlayerInput))]
public class CombatInputHandler : MonoBehaviour
{
    [SerializeField] private WeaponController weaponController;
    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }
    
    public void OnMeleeAttack(InputValue value)
    {
        if (value.isPressed)
        {
            weaponController.Attack();
        }
    }
    
    public void OnRangedAttack(InputValue value)
    {
        if (value.isPressed)
        {
            weaponController.RangedAttack();
        }
    }
    
    public void OnSwitchElement(InputValue value)
    {
        float elementValue = value.Get<float>();
        
        DamageType newType = elementValue switch
        {
            1 => DamageType.Physical,
            2 => DamageType.Fire,
            3 => DamageType.Ice,
            _ => DamageType.Physical
        };
        
        weaponController.SetDamageType(newType);
    }
} 