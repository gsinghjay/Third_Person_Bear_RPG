using UnityEngine;
using UnityEngine.AI;
using Enemies.States;
using Enemies.Interfaces;
using Enemies.Types;
using System.Collections;
using Player.Core;

namespace Enemies.Core
{
    [ExecuteInEditMode]
    public abstract class BearController : MonoBehaviour, IBear
    {
        [Header("Base Stats")]
        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] protected float baseDamage = 20f;
        [SerializeField] protected float moveSpeed = 5f;
        [SerializeField] protected float attackRange = 2f;
        [SerializeField] protected float detectionRange = 10f;
        [SerializeField] protected float attackSpeed = 1f;
        
        public float MoveSpeed => moveSpeed;
        public float AttackRange => attackRange;
        public float DetectionRange => detectionRange;
        public float AttackSpeed => attackSpeed;
        
        public float Health => currentHealth;
        public abstract BearType Type { get; }
        
        public Animator Animator { get; private set; }
        public NavMeshAgent Agent { get; private set; }
        public Transform PlayerTransform { get; private set; }
        public CapsuleCollider Collider { get; private set; }

        protected IBearState currentState;

        public string QuestId { get; set; }
        public event System.Action<IBear> OnDeath; // Instance event

        protected float currentHealth;

        protected virtual void Awake()
        {
            Animator = GetComponent<Animator>();
            Agent = GetComponent<NavMeshAgent>();
            Collider = GetComponent<CapsuleCollider>();
            PlayerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            currentHealth = maxHealth;
            
            if (Agent != null)
            {
                Agent.speed = moveSpeed;
                Agent.acceleration = 12f;
                Agent.angularSpeed = 120f;
                Agent.stoppingDistance = attackRange;
                Agent.radius = 0.6f;
                Agent.height = 2f;
                
                Agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
                Agent.avoidancePriority = 50;
            }
        }

        public virtual void Initialize(Vector3 spawnPosition)
        {
            transform.position = spawnPosition;
            currentHealth = maxHealth;
            ChangeState(new BearIdleState(this));
        }

        public virtual void TakeDamage(float damage, DamageType damageType)
        {
            float calculatedDamage = CalculateDamage(damage, damageType);
            currentHealth -= calculatedDamage;

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual float CalculateDamage(float damage, DamageType damageType)
        {
            return damage;
        }

        protected virtual void Die()
        {
            // Trigger the death event before destroying
            OnDeath?.Invoke(this);
            
            if (Animator != null)
            {
                Animator.SetTrigger("Death");
            }
            
            if (Collider != null)
            {
                Collider.enabled = false;
            }
            
            // Add delay before destruction to allow for death animation
            Destroy(gameObject, 2f);
        }

        public void ChangeState(IBearState newState)
        {
            currentState?.Exit();
            currentState = newState;
            currentState.Enter();
        }

        protected virtual void Start()
        {
            Debug.Log("Bear Start - Initializing state");
            Initialize(transform.position);
        }

        protected virtual void Update()
        {
            currentState?.Update();
        }

        public void DealDamage()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);
            if (distanceToPlayer <= attackRange)
            {
                var playerHealth = PlayerTransform.GetComponent<PlayerHealthComponent>();
                if (playerHealth != null)
                {
                    float damage = baseDamage;
                    ElementalBearController elementalBear = this as ElementalBearController;
                    if (elementalBear != null)
                    {
                        damage += elementalBear.ElementalDamage;
                    }
                    
                    playerHealth.TakeDamage(damage, GetDamageType());
                    Debug.Log($"{Type} Bear deals {damage} damage to the player!");
                }
            }
        }

        protected virtual DamageType GetDamageType()
        {
            return DamageType.Physical; // Base bears deal physical damage
        }

        // Add debug visualization
        private void OnDrawGizmosSelected()
        {
            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }

        public void StartStateCoroutine(IEnumerator routine)
        {
            StartCoroutine(routine);
        }
    }
} 