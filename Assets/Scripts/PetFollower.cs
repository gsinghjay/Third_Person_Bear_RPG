using UnityEngine;
using UnityEngine.AI;
using Animancer;
using Enemies.Interfaces;
using System.Collections;

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

    [Header("Detection Settings")]
    [SerializeField] private float bearDetectionRange = 15f;
    [SerializeField] private float bearCheckInterval = 0.5f;
    [SerializeField] private LayerMask bearLayerMask; // Set this in Inspector
    [SerializeField] private Transform alertIndicator; // Optional UI element to show direction

    private float nextPathUpdate;
    private bool isAlerted;
    private IBear detectedBear;
    private float nextBearCheck;
    private bool isLeadingToTarget;
    private Vector3 lastAlertPosition;

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

        // Check for bears periodically
        if (Time.time >= nextBearCheck)
        {
            CheckForBears();
            nextBearCheck = Time.time + bearCheckInterval;
        }

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
        if (isLeadingToTarget && lastAlertPosition != Vector3.zero)
        {
            // Lead player to the bear
            float distanceToTarget = Vector3.Distance(transform.position, lastAlertPosition);
            
            if (distanceToTarget > minDistanceToPlayer)
            {
                agent.SetDestination(lastAlertPosition);
            }
            else
            {
                // Wait for player to catch up
                if (distanceToPlayer > minDistanceToPlayer * 2)
                {
                    agent.SetDestination(playerTransform.position);
                }
            }
        }
        else if (distanceToPlayer > minDistanceToPlayer)
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

        // Play the new animation
        _animancer.Play(animation, _transitionDuration);
        
        // If this is the alert animation, set up a coroutine to handle the end
        if (animation == _alertAnimation)
        {
            StartCoroutine(HandleAlertAnimationEnd(animation.Clip.length));
        }
    }

    private IEnumerator HandleAlertAnimationEnd(float duration)
    {
        yield return new WaitForSeconds(duration);
        
        isAlerted = false;
        if (alertIndicator != null)
        {
            alertIndicator.gameObject.SetActive(false);
        }
        UpdateAnimation();
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

    private void CheckForBears()
    {
        // If already alerted, don't check for new bears
        if (isAlerted) return;

        Collider[] bearColliders = Physics.OverlapSphere(transform.position, bearDetectionRange, bearLayerMask);
        
        foreach (Collider bearCollider in bearColliders)
        {
            if (bearCollider.TryGetComponent<IBear>(out IBear bear))
            {
                // Store the detected bear's position
                lastAlertPosition = bearCollider.transform.position;
                detectedBear = bear;
                
                // Trigger alert animation and behavior
                TriggerBearAlert();
                return;
            }
        }
    }

    private void TriggerBearAlert()
    {
        isAlerted = true;
        isLeadingToTarget = true;
        
        // Face the direction of the bear
        Vector3 directionToBear = (lastAlertPosition - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(directionToBear);
        
        // Update the alert indicator direction if it exists
        if (alertIndicator != null)
        {
            alertIndicator.gameObject.SetActive(true);
            alertIndicator.forward = directionToBear;
        }
        
        PlayAnimation(_alertAnimation);
    }

    // Add OnDrawGizmosSelected for debugging
    private void OnDrawGizmosSelected()
    {
        // Draw bear detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, bearDetectionRange);
    }
} 