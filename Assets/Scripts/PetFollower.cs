using UnityEngine;
using UnityEngine.AI;

public class PetFollower : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float minDistanceToPlayer = 2f;
    [SerializeField] private float maxDistanceToPlayer = 10f;
    
    [Header("NavMesh Settings")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float updatePathInterval = 0.2f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    private readonly int moveSpeedHash = Animator.StringToHash("MoveSpeed");

    private float nextPathUpdate;

    private void Start()
    {
        // Get required components
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
        if (playerTransform == null)
        {
            playerTransform = Camera.main?.transform.parent;
        }

        // Configure NavMeshAgent
        if (agent != null)
        {
            agent.stoppingDistance = minDistanceToPlayer;
            agent.updateRotation = true;
            agent.updatePosition = true;
        }
    }

    private void Update()
    {
        if (playerTransform == null || agent == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Update path periodically to avoid performance overhead
        if (Time.time >= nextPathUpdate)
        {
            UpdatePetMovement(distanceToPlayer);
            nextPathUpdate = Time.time + updatePathInterval;
        }

        // Update animation based on agent's velocity
        if (animator != null)
        {
            animator.SetFloat(moveSpeedHash, agent.velocity.magnitude / agent.speed);
        }

        // Teleport if too far from player
        if (distanceToPlayer > maxDistanceToPlayer)
        {
            TeleportNearPlayer();
        }
    }

    private void UpdatePetMovement(float distanceToPlayer)
    {
        if (distanceToPlayer > minDistanceToPlayer)
        {
            agent.SetDestination(playerTransform.position);
        }
    }

    private void TeleportNearPlayer()
    {
        Vector3 randomOffset = Random.insideUnitSphere * minDistanceToPlayer;
        randomOffset.y = 0;
        
        NavMeshHit hit;
        Vector3 targetPosition = playerTransform.position + randomOffset;
        
        // Find nearest valid position on NavMesh
        if (NavMesh.SamplePosition(targetPosition, out hit, 5f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
    }
} 