using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PatrolAgent : MonoBehaviour
{
    #region Serialized Fields
    [Tooltip("Reference to the health bar Slider.")]
    public Slider healthBar;

    [Tooltip("Reference to the canvas holding the health bar.")]
    public Transform healthBarCanvas;

    [Tooltip("Reference to the main camera, set this in the editor.")]
    public Camera mainCamera;

    [Tooltip("Minimum time to wait when idling.")]
    public float waitTimeMin = 2f;

    [Tooltip("Maximum time to wait when idling.")]
    public float waitTimeMax = 5f;

    [Tooltip("Maximum number of hits before resetting.")]
    public int maxHits = 10;

    [Tooltip("Array of waypoints for patrolling.")]
    public Transform[] waypoints;

    [Tooltip("Audio clip to play when the agent dies.")]
    public AudioClip deathSound;

    [Tooltip("Vision range for detecting the player.")]
    public float visionRange = 20f;

    [Tooltip("Vision angle for detecting the player.")]
    public float visionAngle = 60f;
    #endregion

    #region Private Fields
    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;
    private int hitCount = 0;
    private bool isWaiting = false;
    private int currentWaypointIndex = 0;
    private Vector3 initialPosition;
    private Image healthBarBackground;
    private AIStateManager stateManager;
    private Transform playerTransform;
    private bool isPlayerInRange = false;
    private bool isPlayerInVisionAngle = false;
    #endregion

    #region Unity Methods
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        healthBarBackground = healthBar.transform.Find("Background").GetComponent<Image>();
        stateManager = GetComponent<AIStateManager>();

        // Get the player transform
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        InitializeHealthBar();
        initialPosition = transform.position;
        MoveToNextWaypoint();
    }

    private void Update()
    {
        // Only handle patrol logic if the AI is in the Patrolling state
        if (stateManager.currentState == AIStateManager.AIState.Patrolling)
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f && !isWaiting)
            {
                ReachedWaypoint(agent.transform.position);
            }

            // Detect player while patrolling
            DetectPlayer();

            // If the player is detected, switch to Chasing state
            if (isPlayerInRange && isPlayerInVisionAngle)
            {
                stateManager.ChangeState(AIStateManager.AIState.Chasing);
            }

            UpdateHealthBar();
        }
    }
    #endregion

    #region Detection Methods
    private void DetectPlayer()
    {
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= visionRange)
            {
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

    #endregion

    #region Waypoint Methods
    public void MoveToNextWaypoint()
    {
        if (waypoints.Length == 0)
            return;

        agent.SetDestination(waypoints[currentWaypointIndex].position);
        animator.SetBool("isWalking", true);
        animator.SetBool("isIdle", false);

        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }

    public void ReachedWaypoint(Vector3 collisionPosition)
    {
        StartCoroutine(IdleAtPoint(collisionPosition));
    }

    private IEnumerator IdleAtPoint(Vector3 collisionPosition)
    {
        isWaiting = true;
        agent.isStopped = true;
        agent.Warp(collisionPosition);
        animator.SetBool("isWalking", false);
        animator.SetBool("isIdle", true);

        yield return new WaitForSeconds(Random.Range(waitTimeMin, waitTimeMax));

        animator.SetBool("isIdle", false);
        agent.isStopped = false;
        isWaiting = false;

        MoveToNextWaypoint();
    }
    #endregion

    #region Health Methods
    public void HitByProjectile()
    {
        hitCount++;
        if (hitCount >= maxHits)
        {
            StartCoroutine(HandleDeath());
        }
        else
        {
            UpdateHealthBar();
        }
    }

    public void HitByGrenade(float damage)
    {
        hitCount += Mathf.CeilToInt(damage);
        if (hitCount >= maxHits)
        {
            StartCoroutine(HandleDeath());
        }
        else
        {
            UpdateHealthBar();
        }
        healthBarBackground.color = GetRandomColorExcludingRedAndGreen();
    }

    private Color GetRandomColorExcludingRedAndGreen()
    {
        Color randomColor;
        do
        {
            randomColor = new Color(Random.value, Random.value, Random.value);
        } while ((randomColor.r > 0.5f && randomColor.g < 0.5f && randomColor.b < 0.5f) ||
                (randomColor.r < 0.5f && randomColor.g > 0.5f && randomColor.b < 0.5f));
        return randomColor;
    }

    private IEnumerator HandleDeath()
    {
        StopAllCoroutines();
        agent.isStopped = true;
        animator.SetBool("isWalking", false);
        animator.Rebind();
        audioSource.PlayOneShot(deathSound);

        isWaiting = false;

        ResetAgentPosition();
        hitCount = 0;
        healthBarBackground.color = Color.red;
        UpdateHealthBar();

        yield return new WaitForSeconds(1f);

        RespawnAgent();
    }

    private void RespawnAgent()
    {
        agent.isStopped = false;
        stateManager.ChangeState(AIStateManager.AIState.Patrolling); // Set state back to patrolling
        MoveToNextWaypoint();
    }

    private void ResetAgentPosition()
    {
        agent.Warp(initialPosition);
    }

    private void InitializeHealthBar()
    {
        healthBar.maxValue = maxHits;
        healthBar.minValue = 0;
        healthBar.value = maxHits;
        healthBarBackground.color = Color.red;
    }

    private void UpdateHealthBar()
    {
        healthBar.value = maxHits - hitCount;

        Vector3 directionToCamera = mainCamera.transform.position - healthBarCanvas.position;
        directionToCamera.x = directionToCamera.z = 0;
        healthBarCanvas.LookAt(mainCamera.transform.position - directionToCamera);

        healthBarCanvas.Rotate(0, 0, 0);
    }
    #endregion
}
