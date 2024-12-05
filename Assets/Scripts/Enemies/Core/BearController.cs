using UnityEngine;
using Enemies.States;
using Enemies.Interfaces;

namespace Enemies.Core
{
    public abstract class BearController : MonoBehaviour, IBear
    {
        [Header("Base Stats")]
        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] protected float moveSpeed = 5f;
        [SerializeField] protected float attackRange = 2f;
        [SerializeField] protected float detectionRange = 10f;
        
        public float Health { get; protected set; }
        public abstract BearType Type { get; }
        
        public Animator Animator { get; private set; }
        public CharacterController CharacterController { get; private set; }
        public float VerticalVelocity { get; set; }

        protected Transform player;
        protected IBearState currentState;

        protected virtual void Awake()
        {
            Animator = GetComponent<Animator>();
            CharacterController = GetComponent<CharacterController>();
            player = GameObject.FindGameObjectWithTag("Player").transform;
            Health = maxHealth;
        }

        public virtual void Initialize(Vector3 spawnPosition)
        {
            transform.position = spawnPosition;
            ChangeState(new BearIdleState(this));
        }

        public virtual void TakeDamage(float damage, DamageType damageType)
        {
            Health -= CalculateDamage(damage, damageType);
            
            if (Health <= 0)
            {
                Die();
            }
        }

        protected virtual float CalculateDamage(float damage, DamageType damageType)
        {
            return damage; // Override in derived classes for elemental resistances
        }

        protected virtual void Die()
        {
            // Handle death animation and cleanup
            Destroy(gameObject, 2f);
        }

        protected void ChangeState(IBearState newState)
        {
            currentState?.Exit();
            currentState = newState;
            currentState.Enter();
        }

        protected virtual void Update()
        {
            currentState?.Update();
        }
    }
} 