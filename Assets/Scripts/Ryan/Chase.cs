using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Chase : MonoBehaviour
{
    public float visionRange = 20f;
    public float visionAngle = 60f;
    public float chasingSpeed = 5f;
    public float lostPlayerFreezeTime = 3f;

    private bool isPlayerInRange = false;
    private bool isPlayerInVisionAngle = false;
    private Transform playerTransform;
    private Vector3 lastKnownPlayerPosition;
    private NavMeshAgent agent;
    private Animator animator;
    private AIStateManager stateManager;
    private PatrolAgent patrolAgent;
    private float lostPlayerTimer = 0f;
    private bool isIdling = false;
    private Coroutine idleCoroutine;

    private float chaseTimer = 0f;
    private float shootTimer = 0f;
    private bool isChasing = true;

    public GameObject bulletPrefab; // Assign the bullet prefab in the Inspector
    public Transform firePoint; // The point from which bullets will be fired
    public float shootInterval = 0.5f; // Interval between shots
    private AudioSource audioSource;
    public AudioClip shootSound;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        stateManager = GetComponent<AIStateManager>();
        patrolAgent = GetComponent<PatrolAgent>();
        audioSource = GetComponent<AudioSource>();

        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void Update()
    {
        DetectPlayer();

        if (isPlayerInRange && isPlayerInVisionAngle)
        {
            lastKnownPlayerPosition = playerTransform.position;
            lostPlayerTimer = lostPlayerFreezeTime; // Reset the timer

            // Stop idling if currently idling
            if (isIdling && idleCoroutine != null)
            {
                StopCoroutine(idleCoroutine);
                isIdling = false;
            }

            // Switch to chase state
            stateManager.ChangeState(AIStateManager.AIState.Chasing);
            ChasePlayer();
        }
        else if (!isPlayerInRange && !isIdling)
        {
            lostPlayerTimer -= Time.deltaTime;

            if (lostPlayerTimer <= 0f)
            {
                // Start idling
                isIdling = true;
                idleCoroutine = StartCoroutine(IdleBeforePatrolling());
            }
        }
    }

    private void DetectPlayer()
    {
        // Find the player transform
        //GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject player = GameObject.FindGameObjectWithTag("Drone");
        Debug.Log(player.name);
        if (player != null)
        {
            playerTransform = player.transform;

            // Check if the player is within the vision range
            if (Vector3.Distance(transform.position, playerTransform.position) <= visionRange)
            {
                // Calculate the direction to the player
                Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;

                // Check if the player is within the vision angle
                Vector3 agentForward = transform.forward;
                float angle = Vector3.Angle(agentForward, directionToPlayer);
                if (angle <= visionAngle / 2f)
                {
                    // Perform a Raycast to check if there's a clear line of sight to the player
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out hit, visionRange))
                    {
                        if (hit.transform.CompareTag("Player"))
                        {
                            isPlayerInRange = true;
                            isPlayerInVisionAngle = true;
                            return;
                        }
                    }
                }
            }
        }

        // If the player is not detected, reset the flags
        isPlayerInRange = false;
        isPlayerInVisionAngle = false;
    }


    private void ChasePlayer()
    {
        if (isChasing)
        {
            agent.speed = chasingSpeed;
            agent.SetDestination(playerTransform.position);
            animator.SetBool("isWalking", true);
            animator.SetBool("isIdle", false);
            animator.SetBool("isFiring", false);

            chaseTimer += Time.deltaTime;
            if (chaseTimer >= 2f)
            {
                isChasing = false;
                chaseTimer = 0f;
            }
        }
        else
        {
            agent.speed = 0f;
            agent.ResetPath();
            animator.SetBool("isWalking", false);
            animator.SetBool("isIdle", false);
            animator.SetBool("isFiring", true);

            shootTimer += Time.deltaTime;
            if (shootTimer >= shootInterval)
            {
                audioSource.PlayOneShot(shootSound); // Play the shooting sound
                FireBullet(); // Call the function to fire a bullet
                shootTimer = 0f; // Reset the timer
            }

            chaseTimer += Time.deltaTime;
            if (chaseTimer >= 2f)
            {
                isChasing = true;
                chaseTimer = 0f;
            }
        }
    }

        void FireBullet()
    {
        // Create a bullet from the prefab
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }

    private IEnumerator IdleBeforePatrolling()
    {
        float idleTime = 0f;

        // Idle while waiting for waitTimeMin
        animator.SetBool("isWalking", false);
        animator.SetBool("isIdle", true);

        while (idleTime < patrolAgent.waitTimeMin)
        {
            // Check if the player is detected during idling
            DetectPlayer();
            if (isPlayerInRange && isPlayerInVisionAngle)
            {
                // Break out of idling if the player is seen
                stateManager.ChangeState(AIStateManager.AIState.Chasing);
                ChasePlayer();
                yield break;
            }

            idleTime += Time.deltaTime;
            yield return null;
        }

        // Return to patrolling state after idling
        stateManager.ChangeState(AIStateManager.AIState.Patrolling);
        patrolAgent.MoveToNextWaypoint();
        isIdling = false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = isPlayerInRange && isPlayerInVisionAngle ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Vector3 leftDirection = Quaternion.Euler(0f, -visionAngle / 2f, 0f) * transform.forward;
        Vector3 rightDirection = Quaternion.Euler(0f, visionAngle / 2f, 0f) * transform.forward;

        Gizmos.DrawRay(transform.position, leftDirection * visionRange);
        Gizmos.DrawRay(transform.position, rightDirection * visionRange);
    }
#endif
}
