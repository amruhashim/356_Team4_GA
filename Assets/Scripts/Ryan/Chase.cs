using UnityEngine;
using UnityEngine.AI;

public class Chase : MonoBehaviour
{
    public float visionRange = 20f;
    public float visionAngle = 60f;
    public float chasingSpeed = 5f;
    public float lostPlayerFreezeTime = 3f; // Time the agent freezes after losing the player

    private bool isPlayerInRange = false;
    private bool isPlayerInVisionAngle = false;
    private Transform playerTransform;
    private Vector3 lastKnownPlayerPosition;
    private NavMeshAgent agent;
    private Animator animator;
    private bool wasPlayerInRange = false;
    private bool wasPlayerInVisionAngle = false;
    private float lostPlayerTimer = 0f;
    private bool isReturningToLastKnownPosition = false;
    private Sneak playerSneakScript;
    private PatrolAgent patrolAgent;
    private bool isPatrolling = true;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        patrolAgent = GetComponent<PatrolAgent>();
        agent.speed = patrolAgent.waitTimeMin; // Set the initial speed to the minimum patrol speed

        // Get the Sneak script from the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerSneakScript = player.GetComponent<Sneak>();
    }

    private void Update()
    {
        DetectPlayer();

        if (isPlayerInRange)
        {
            if (playerSneakScript.IsSneaking())
            {
                // If the player is in sneak mode and within the vision range, ignore them
                if (isPlayerInVisionAngle)
                {
                    ChasePlayer();
                    lastKnownPlayerPosition = playerTransform.position;
                    isReturningToLastKnownPosition = false;
                    isPatrolling = false;
                    Debug.Log("Agent is in chase mode");
                }
                else
                {
                    if (!isPatrolling)
                    {
                        isPatrolling = true;
                        agent.speed = patrolAgent.waitTimeMin;
                        patrolAgent.MoveToNextWaypoint(); // Resume patrolling
                        Debug.Log("Agent is in patrol mode");
                    }
                }
            }
            else
            {
                // If the player is not in sneak mode, always turn to face them
                TurnToFacePlayer();

                if (isPlayerInVisionAngle)
                {
                    ChasePlayer();
                    lastKnownPlayerPosition = playerTransform.position;
                    isReturningToLastKnownPosition = false;
                    isPatrolling = false;
                    Debug.Log("Agent is in chase mode");
                }
                else
                {
                    if (!isPatrolling)
                    {
                        isPatrolling = true;
                        agent.speed = patrolAgent.waitTimeMin;
                        patrolAgent.MoveToNextWaypoint(); // Resume patrolling
                        Debug.Log("Agent is in patrol mode");
                    }
                }
            }
        }
        else
        {
            if (wasPlayerInRange)
            {
                StopChasing();
                isPatrolling = true;
                patrolAgent.MoveToNextWaypoint(); // Resume patrolling
            }
        }

        if (isReturningToLastKnownPosition)
        {
            ReturnToLastKnownPosition();
            Debug.Log("Agent is returning to last known position");
        }
        else
        {
            if (lostPlayerTimer > 0f)
            {
                lostPlayerTimer -= Time.deltaTime;
            }
        }

        wasPlayerInRange = isPlayerInRange;
        wasPlayerInVisionAngle = isPlayerInVisionAngle;
    }

    private void DetectPlayer()
    {
        // Find the player transform
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;

            // Check if the player is within the vision range
            if (Vector3.Distance(transform.position, playerTransform.position) <= visionRange)
            {
                // Check if the player is within the vision angle
                Vector3 agentForward = transform.forward;
                Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
                float angle = Vector3.Angle(agentForward, directionToPlayer);
                isPlayerInVisionAngle = angle <= visionAngle / 2f;

                if (isPlayerInVisionAngle)
                {
                    // Perform a raycast to check if there's a direct line of sight to the player
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, directionToPlayer, out hit, visionRange))
                    {
                        if (hit.collider.CompareTag("Player"))
                        {
                            
                            if (!isPlayerInRange)
                            {
                                Debug.Log("Player entered vision range and line of sight");
                            }
                            isPlayerInRange = true;
                        }
                        else
                        {
                        
                            isPlayerInRange = false;
                            isPlayerInVisionAngle = false;
                        }
                    }
                }
                else
                {
                    isPlayerInRange = false;
                }
            }
            else
            {
                isPlayerInRange = false;
                isPlayerInVisionAngle = false;
            }
        }
        else
        {
            isPlayerInRange = false;
            isPlayerInVisionAngle = false;
        }
    }


    private void ChasePlayer()
    {
        agent.speed = chasingSpeed;
        agent.SetDestination(playerTransform.position);
        animator.SetBool("isWalking", true);
        animator.SetBool("isIdle", false);
    }

    private void TurnToFacePlayer()
    {
        // Calculate the rotation to face the player
        Quaternion targetRotation = Quaternion.LookRotation(playerTransform.position - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    private void StopChasing()
    {
        agent.speed = patrolAgent.waitTimeMin;
        agent.ResetPath();
        animator.SetBool("isWalking", false);
        animator.SetBool("isIdle", true);
        isReturningToLastKnownPosition = true;
        lostPlayerTimer = lostPlayerFreezeTime;
        Debug.Log("Heading to last seen location");
    }

    private void ReturnToLastKnownPosition()
    {
        agent.SetDestination(lastKnownPlayerPosition);
        animator.SetBool("isWalking", true);
        animator.SetBool("isIdle", false);

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.speed = patrolAgent.waitTimeMin;
            animator.SetBool("isWalking", false);
            animator.SetBool("isIdle", true);
            isReturningToLastKnownPosition = false;
            Debug.Log("Returning to patrol");
        }
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