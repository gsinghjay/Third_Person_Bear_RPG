using UnityEngine;

public class PetFollower : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float minDistanceToPlayer = 2f;
    [SerializeField] private float maxDistanceToPlayer = 10f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    private readonly int moveSpeedHash = Animator.StringToHash("MoveSpeed");

    private void Start()
    {
        // If animator is not assigned, try to get it from this GameObject
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // If player transform is not assigned, try to find the main camera's parent
        if (playerTransform == null)
        {
            playerTransform = Camera.main?.transform.parent;
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        Vector3 directionToPlayer = playerTransform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // Only move if we're further than the minimum distance
        if (distanceToPlayer > minDistanceToPlayer)
        {
            // Calculate target position
            Vector3 targetPosition = playerTransform.position;
            
            // If beyond max distance, teleport near player
            if (distanceToPlayer > maxDistanceToPlayer)
            {
                Vector3 randomOffset = Random.insideUnitSphere * minDistanceToPlayer;
                randomOffset.y = 0;
                transform.position = playerTransform.position + randomOffset;
            }
            // Normal following behavior
            else
            {
                Vector3 movement = directionToPlayer.normalized * (followSpeed * Time.deltaTime);
                transform.position += movement;
                
                // Rotate towards movement direction
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            }

            // Update animation if animator exists
            if (animator != null)
            {
                animator.SetFloat(moveSpeedHash, distanceToPlayer > minDistanceToPlayer ? 1f : 0f);
            }
        }
        else if (animator != null)
        {
            animator.SetFloat(moveSpeedHash, 0f);
        }
    }
} 