using UnityEngine;
using Enemies.Types;
using Enemies.Interfaces;

public class WeaponController : MonoBehaviour 
{
    [Header("Basic Attack Settings")]
    [SerializeField] private float baseDamage = 40f;
    [SerializeField] private float basicAttackRange = 2.5f;
    [SerializeField] private float basicAttackRadius = 1.5f;
    
    [Header("Special Attack Settings")]
    [SerializeField] private float specialAttackDamage = 30f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private Transform projectileSpawnPoint;
    
    [Header("General Settings")]
    [SerializeField] private LayerMask enemyLayer;
    
    private DamageType currentDamageType = DamageType.Physical;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        
        // Set up enemy layer mask with the actual layer number
        int enemyLayerNumber = LayerMask.NameToLayer("Enemy");
        if (enemyLayerNumber == -1)
        {
            Debug.LogError("Enemy layer not found! Please create a layer named 'Enemy' in Project Settings.");
            return;
        }
        
        enemyLayer = 1 << enemyLayerNumber;
        Debug.Log($"Enemy layer mask initialized: {enemyLayer.value}, Layer number: {enemyLayerNumber}");
    }

    public void Attack()
    {
        Debug.Log("WeaponController: Attack called");
        PerformBasicAttack();
    }

    private void PerformBasicAttack()
    {
        Vector3 attackPoint = CalculateAimPoint(basicAttackRange);
        Debug.Log($"Performing basic attack at point: {attackPoint}");
        
        // Draw debug sphere for attack range
        Debug.DrawLine(mainCamera.transform.position, attackPoint, Color.red, 1f);
        
        Collider[] hits = Physics.OverlapSphere(attackPoint, basicAttackRadius, enemyLayer);
        Debug.Log($"Found {hits.Length} potential targets in range. Layer mask: {enemyLayer}");

        bool hitSomething = false;
        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<IBear>(out var bear))
            {
                hitSomething = true;
                Debug.Log($"Hit bear: {hit.gameObject.name}");
                bear.TakeDamage(baseDamage, currentDamageType);
            }
        }

        if (!hitSomething)
        {
            Debug.Log("No bears hit. Check if bears are on the correct layer.");
        }
    }

    public void PerformSpecialAttack()
    {
        Debug.Log("Performing special attack"); // Debug attack initiation
        
        Vector3 spawnPosition = projectileSpawnPoint != null ? 
            projectileSpawnPoint.position : 
            transform.position + transform.forward + Vector3.up;
            
        Vector3 direction = CalculateAimDirection();
        Vector3 projectileVelocity = direction * projectileSpeed;
        
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(direction));
        
        if (projectile.TryGetComponent<WeaponProjectile>(out var weaponProjectile))
        {
            Debug.Log($"Initializing projectile with damage: {specialAttackDamage}, type: {currentDamageType}"); // Debug projectile setup
            weaponProjectile.Initialize(currentDamageType, specialAttackDamage, projectileVelocity);
        }
    }

    private Vector3 CalculateAimPoint(float range)
    {
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        Ray ray = mainCamera.ScreenPointToRay(screenCenter);
        
        // Try to hit terrain or other surfaces first
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            return hit.point;
        }
        
        // If no hit, use a point at the attack range distance
        return transform.position + transform.forward * range;
    }

    private Vector3 CalculateAimDirection()
    {
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
        Vector3 targetPoint = ray.GetPoint(20f);
        return (targetPoint - transform.position).normalized;
    }

    public void SetDamageType(DamageType type)
    {
        currentDamageType = type;
        Debug.Log($"Switched to {type} damage type");
    }

    private void OnDrawGizmosSelected()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // Draw basic attack range
            Vector3 basicAttackPoint = CalculateAimPoint(basicAttackRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(basicAttackPoint, basicAttackRadius);
            
            // Draw projectile trajectory
            if (projectileSpawnPoint != null)
            {
                Vector3 aimPoint = CalculateAimPoint(100f);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(projectileSpawnPoint.position, aimPoint);
            }
        }
    }
} 