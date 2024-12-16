using UnityEngine;
using UnityEngine.AI;
using Enemies.States;
using Enemies.Interfaces;
using Enemies.Types;
using System.Collections;
using Player.Core;
using Items;

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
        protected bool isDead = false;

        private bool questUpdateHandled = false;
        
        public bool QuestUpdateHandled 
        { 
            get => questUpdateHandled;
            set => questUpdateHandled = value;
        }

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
            isDead = false;

            // Set quest ID based on position if not already set
            if (string.IsNullOrEmpty(QuestId))
            {
                QuestId = DetermineQuestId(spawnPosition);
            }

            ChangeState(new BearIdleState(this));
        }

        private string DetermineQuestId(Vector3 position)
        {
            // Convert to 2D position for arena check
            Vector2 position2D = new Vector2(position.x, position.z);
            
            // Check each arena's bounds
            if (IsInArenaRange(position2D, new Vector2(-35f, 35f), 35f))  // Northwest arena
                return "northwest_arena";
            if (IsInArenaRange(position2D, new Vector2(35f, 35f), 35f))   // Northeast arena
                return "northeast_arena";
            if (IsInArenaRange(position2D, new Vector2(0f, -35f), 35f))   // Boss arena
                return "boss_arena";
            
            Debug.LogWarning($"Bear spawned outside known arena bounds at {position}");
            return "unknown";
        }

        private bool IsInArenaRange(Vector2 position, Vector2 arenaCenter, float radius)
        {
            return Vector2.Distance(position, arenaCenter) <= radius;
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
            if (currentHealth <= 0 && !isDead)
            {
                isDead = true;
                Debug.Log($"{Type} Bear died!");
                
                // Set the flag before invoking OnDeath
                questUpdateHandled = false;
                
                // Spawn food drop
                FoodDropManager.Instance?.SpawnFoodDrop(transform.position, transform.rotation);
                
                // Notify about death
                OnDeath?.Invoke(this);
                
                // Change to death state
                ChangeState(new BearDeathState(this));
            }
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