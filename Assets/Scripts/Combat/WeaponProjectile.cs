using UnityEngine;
using Enemies.Types;
using Enemies.Interfaces;

public class WeaponProjectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private ParticleSystem hitEffect;
    
    private DamageType damageType;
    private float damage;
    private Vector3 velocity;
    private bool hasHit;

    public void Initialize(DamageType type, float dmg, Vector3 vel)
    {
        damageType = type;
        damage = dmg;
        velocity = vel;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!hasHit)
        {
            transform.position += velocity * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        if (other.TryGetComponent<IBear>(out var bear))
        {
            bear.TakeDamage(damage, damageType);
            hasHit = true;
            
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            
            Destroy(gameObject);
        }
    }
}