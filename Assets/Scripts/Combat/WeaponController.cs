using UnityEngine;
using Enemies.Types;
using Enemies.Interfaces;

public class WeaponController : MonoBehaviour 
{
    [Header("Weapon Settings")]
    [SerializeField] private float baseDamage = 40f;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float attackRadius = 2f;

    private DamageType currentDamageType = DamageType.Physical;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void Attack()
    {
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        Ray ray = mainCamera.ScreenPointToRay(screenCenter);
        Debug.DrawRay(ray.origin, ray.direction * attackRange, Color.red, 1f);

        Vector3 attackPoint = ray.origin + ray.direction * attackRange;
        Debug.Log($"Attacking at point: {attackPoint}");

        Collider[] hits = Physics.OverlapSphere(attackPoint, attackRadius, enemyLayer);
        Debug.Log($"Found {hits.Length} potential targets in range");

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
            Debug.Log($"No bears hit. Layer mask: {enemyLayer.value}, Attack radius: {attackRadius}");
        }
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
            Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
            Ray ray = mainCamera.ScreenPointToRay(screenCenter);
            Vector3 attackPoint = ray.origin + ray.direction * attackRange;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint, attackRadius);
        }
    }
} 