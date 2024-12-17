using UnityEngine;
using UnityEngine.AI;
using Animancer;

public class PetFollower : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float minDistanceToPlayer = 2f;
    [SerializeField] private float maxDistanceToPlayer = 10f;
    
    [Header("NavMesh Settings")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float updatePathInterval = 0.2f;

    [Header("Animation Settings")]
    [SerializeField] private AnimancerComponent _animancer;
    [SerializeField] private ClipTransition _idleAnimation;
    [SerializeField] private ClipTransition _walkAnimation;
    [SerializeField] private ClipTransition _runAnimation;
    [SerializeField] private ClipTransition _alertAnimation; // For when detecting bears
    [SerializeField] private float _transitionDuration = 0.25f;

    private float nextPathUpdate;
    private bool isAlerted;

    private void Start()
    {
        // Get required components
        if (_animancer == null)
        {
            _animancer = GetComponent<AnimancerComponent>();
            if (_animancer == null)
            {
                Debug.LogError("PetFollower: No AnimancerComponent found!");
            }
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

        InitializeAnimations();
    }

    private void InitializeAnimations()
    {
        if (_animancer == null) return;

        // Set up animation events
        if (_alertAnimation != null)
        {
            _alertAnimation.Events.OnEnd = () =>
            {
                isAlerted = false;
                UpdateAnimation();
            };
        }

        // Start with idle animation
        PlayAnimation(_idleAnimation);
    }

    private void Update()
    {
        if (playerTransform == null || agent == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Update path periodically
        if (Time.time >= nextPathUpdate)
        {
            UpdatePetMovement(distanceToPlayer);
            nextPathUpdate = Time.time + updatePathInterval;
        }

        // Update animation based on movement
        UpdateAnimation();

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

    private void UpdateAnimation()
    {
        if (_animancer == null) return;

        if (isAlerted && _alertAnimation != null)
        {
            PlayAnimation(_alertAnimation);
            return;
        }

        // Determine animation based on velocity
        float speed = agent.velocity.magnitude;
        
        if (speed < 0.1f)
        {
            PlayAnimation(_idleAnimation);
        }
        else if (speed < agent.speed * 0.5f)
        {
            PlayAnimation(_walkAnimation);
        }
        else
        {
            PlayAnimation(_runAnimation);
        }
    }

    private void PlayAnimation(ClipTransition animation)
    {
        if (animation == null || _animancer == null) return;

        // Don't replay the same animation
        if (_animancer.IsPlaying(animation)) return;

        // Stop current animation and play new one
        _animancer.Play(animation, _transitionDuration);
    }

    private void TeleportNearPlayer()
    {
        Vector3 randomOffset = Random.insideUnitSphere * minDistanceToPlayer;
        randomOffset.y = 0;
        
        NavMeshHit hit;
        Vector3 targetPosition = playerTransform.position + randomOffset;
        
        if (NavMesh.SamplePosition(targetPosition, out hit, 5f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            // Play alert animation when teleporting
            isAlerted = true;
            PlayAnimation(_alertAnimation);
        }
    }

    // Method to trigger alert animation (will be used when detecting bears)
    public void TriggerAlert()
    {
        isAlerted = true;
        PlayAnimation(_alertAnimation);
    }
} 