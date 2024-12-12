using UnityEngine;
using Enemies.Types;
using Enemies.Interfaces;
using System.Collections.Generic;

public class WeaponProjectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private float hitRadius = 1f;
    
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
    }

    public void Initialize(DamageType type, float dmg, Vector3 vel)
    {
        damageType = type;
        damage = dmg;
        velocity = vel;
        Debug.Log($"WeaponProjectile: Initialized with damage: {damage}, type: {damageType}");
        
        if (TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.color = damageTypeColors[type];
        }
        
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!hasHit)
        {
            transform.position += velocity * Time.deltaTime;
            CheckForHits();
        }
    }

    private void CheckForHits()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, hitRadius, enemyLayer);
        Debug.Log($"WeaponProjectile: Found {hits.Length} potential targets. Layer mask: {enemyLayer}");

        bool hitSomething = false;
        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<IBear>(out var bear))
            {
                hitSomething = true;
                Debug.Log($"WeaponProjectile: Hit bear: {hit.gameObject.name}");
                bear.TakeDamage(damage, damageType);
                hasHit = true;
                
                if (hitEffect != null)
                {
                    Instantiate(hitEffect, transform.position, Quaternion.identity);
                }
                
                Destroy(gameObject);
                break;
            }
        }

        if (!hitSomething)
        {
            Debug.Log("WeaponProjectile: No bears hit. Check if bears are on the correct layer.");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}