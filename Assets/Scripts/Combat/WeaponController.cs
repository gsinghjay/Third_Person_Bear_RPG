using UnityEngine;
using System.Collections;
using Enemies.Types;
using Enemies.Interfaces;

public class WeaponController : MonoBehaviour 
{
    [Header("Weapon Settings")]
    [SerializeField] private float baseDamage = 20f;
    [SerializeField] private float elementalDamageMultiplier = 1.5f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float rangedAttackSpeed = 15f;
    
    [Header("Collision Detection")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Transform weaponTip;
    [SerializeField] private Transform weaponBase;
    [SerializeField] private float weaponTraceWidth = 0.5f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem[] elementalEffects;
    [SerializeField] private GameObject rangedAttackPrefab;
    
    private DamageType currentDamageType = DamageType.Physical;
    private bool canAttack = true;
    private Animator animator;
    private readonly int attackHash = Animator.StringToHash("Attack");

    private void Awake()
    {
        animator = GetComponentInParent<Animator>();
    }

    public void Attack()
    {
        if (!canAttack) return;
        
        animator.SetTrigger(attackHash);
        StartCoroutine(PerformAttack());
    }

    public void RangedAttack()
    {
        if (!canAttack) return;
        
        animator.SetTrigger(attackHash);
        StartCoroutine(PerformRangedAttack());
    }

    private IEnumerator PerformAttack()
    {
        canAttack = false;

        // Wait for the attack animation to reach its damage frame
        yield return new WaitForSeconds(0.2f);

        // Perform weapon trace
        DetectHits();

        // Apply attack cooldown
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private IEnumerator PerformRangedAttack()
    {
        canAttack = false;

        // Wait for the attack animation to reach its projectile spawn frame
        yield return new WaitForSeconds(0.3f);

        SpawnRangedAttack();

        // Apply attack cooldown
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void DetectHits()
    {
        // Create a capsule collider between weapon base and tip
        Vector3 direction = weaponTip.position - weaponBase.position;
        float distance = direction.magnitude;
        
        RaycastHit[] hits = Physics.SphereCastAll(
            weaponBase.position,
            weaponTraceWidth,
            direction.normalized,
            distance,
            enemyLayer
        );

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.TryGetComponent<IBear>(out var bear))
            {
                float damage = CalculateDamage();
                bear.TakeDamage(damage, currentDamageType);
            }
        }
    }

    private void SpawnRangedAttack()
    {
        GameObject projectile = Instantiate(rangedAttackPrefab, weaponTip.position, transform.rotation);
        WeaponProjectile projectileScript = projectile.GetComponent<WeaponProjectile>();
        
        if (projectileScript != null)
        {
            projectileScript.Initialize(
                currentDamageType,
                CalculateDamage(),
                transform.forward * rangedAttackSpeed
            );
        }
    }

    private float CalculateDamage()
    {
        return currentDamageType == DamageType.Physical ? 
            baseDamage : 
            baseDamage * elementalDamageMultiplier;
    }

    public void SetDamageType(DamageType type)
    {
        currentDamageType = type;
        UpdateElementalEffects();
    }

    private void UpdateElementalEffects()
    {
        // Disable all effects first
        foreach (var effect in elementalEffects)
        {
            effect.Stop();
        }

        // Enable effect for current type if it exists
        if (currentDamageType != DamageType.Physical && 
            (int)currentDamageType - 1 < elementalEffects.Length)
        {
            elementalEffects[(int)currentDamageType - 1].Play();
        }
    }
} 