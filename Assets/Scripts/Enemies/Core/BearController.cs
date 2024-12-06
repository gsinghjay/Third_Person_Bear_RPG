using UnityEngine;
using UnityEngine.AI;
using Enemies.States;
using Enemies.Interfaces;
using Enemies.Types;
using System.Collections;

namespace Enemies.Core
{
    [ExecuteInEditMode]
    public abstract class BearController : MonoBehaviour, IBear
    {
        [Header("Base Stats")]
        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] protected float moveSpeed = 5f;
        [SerializeField] protected float attackRange = 2f;
        [SerializeField] protected float detectionRange = 10f;
        
        public float MoveSpeed => moveSpeed;
        public float AttackRange => attackRange;
        public float DetectionRange => detectionRange;
        
        public float Health { get; protected set; }
        public abstract BearType Type { get; }
        
        public Animator Animator { get; private set; }
        public NavMeshAgent Agent { get; private set; }
        public Transform PlayerTransform { get; private set; }
        public CapsuleCollider Collider { get; private set; }

        protected IBearState currentState;

        protected virtual void Awake()
        {
            Animator = GetComponent<Animator>();
            Agent = GetComponent<NavMeshAgent>();
            Collider = GetComponent<CapsuleCollider>();
            PlayerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            Health = maxHealth;
            
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
            ChangeState(new BearIdleState(this));
        }

        public virtual void TakeDamage(float damage, DamageType damageType)
        {
            float finalDamage = CalculateDamage(damage, damageType);
            Health -= finalDamage;
            
            if (Health <= 0)
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
            Animator.SetTrigger("Death");
            
            if (Agent != null)
                Agent.enabled = false;
            
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
            Debug.Log($"{Type} Bear deals damage to the player!");
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