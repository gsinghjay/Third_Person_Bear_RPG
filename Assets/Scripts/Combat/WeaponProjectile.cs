using UnityEngine;
using Enemies.Types;
using Enemies.Interfaces;
using System.Collections.Generic;

public class WeaponProjectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private float maxRayDistance = 1f;
    
    private DamageType damageType;
    private float damage;
    private Vector3 velocity;
    private bool hasHit;
    private int enemyLayer;

    private readonly Dictionary<DamageType, Color> damageTypeColors = new()
    {
        { DamageType.Physical, Color.white },
        { DamageType.Fire, new Color(1f, 0.3f, 0f) },
        { DamageType.Ice, new Color(0.3f, 0.7f, 1f) }
    };

    private void Awake()
    {
        enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
        Debug.Log($"WeaponProjectile: Enemy layer initialized: {enemyLayer}");
        
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }
    }

    public void Initialize(DamageType type, float dmg, Vector3 vel)
    {
        damageType = type;
        damage = dmg;
        velocity = vel;
        Debug.Log($"WeaponProjectile: Initialized with damage: {damage}, type: {damageType}");
        
        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
            if (meshRenderer.material != null)
            {
                meshRenderer.material.color = damageTypeColors[type];
            }
        }
        
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!hasHit)
        {
            Vector3 oldPosition = transform.position;
            transform.position += velocity * Time.deltaTime;
            CheckForHits(oldPosition);
        }
    }

    private void CheckForHits(Vector3 previousPosition)
    {
        Vector3 rayDirection = (transform.position - previousPosition).normalized;
        float rayDistance = Vector3.Distance(transform.position, previousPosition) + maxRayDistance;
        
        Ray ray = new Ray(previousPosition, rayDirection);
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red);
        
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, enemyLayer))
        {
            if (hit.collider.TryGetComponent<IBear>(out var bear))
            {
                Debug.Log($"WeaponProjectile: Hit bear: {hit.collider.gameObject.name}");
                bear.TakeDamage(damage, damageType);
                hasHit = true;
                
                if (hitEffect != null)
                {
                    Instantiate(hitEffect, hit.point, Quaternion.identity);
                }
                
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.Log("WeaponProjectile: No bears hit. Check if bears are on the correct layer.");
        }
    }

    private void OnDrawGizmos()
    {
        if (!hasHit && Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, velocity.normalized * maxRayDistance);
        }
    }
}