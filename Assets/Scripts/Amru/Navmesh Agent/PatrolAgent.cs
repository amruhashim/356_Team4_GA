using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PatrolAgent : MonoBehaviour
{
    #region Serialized Fields
    [Tooltip("Reference to the health bar Slider.")]
    public Slider healthBar; // UI Slider to display health

    [Tooltip("Reference to the canvas holding the health bar.")]
    public Transform healthBarCanvas; // Canvas holding the health bar

    [Tooltip("Reference to the main camera, set this in the editor.")]
    public Camera mainCamera; // Main camera in the scene

    [Tooltip("Minimum time to wait when idling.")]
    public float waitTimeMin = 2f; // Minimum wait time at waypoints

    [Tooltip("Maximum time to wait when idling.")]
    public float waitTimeMax = 5f; // Maximum wait time at waypoints

    [Tooltip("Maximum number of hits before resetting.")]
    public int maxHits = 10; // Maximum number of hits before death

    [Tooltip("Array of waypoints for patrolling.")]
    public Transform[] waypoints; // Array of waypoints for the agent to patrol

    [Tooltip("Audio clip to play when the agent dies.")]
    public AudioClip deathSound; // Death sound clip
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
    #endregion

    #region Unity Methods
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        healthBarBackground = healthBar.transform.Find("Background").GetComponent<Image>(); // Get the health bar background image
        InitializeHealthBar(); // Initialize the health bar
        initialPosition = transform.position; // Store the initial position
        MoveToNextWaypoint(); // Start patrolling
    }

    private void Update()
    {
        // Check if the agent has reached the waypoint and is not waiting
        if (!agent.pathPending && agent.remainingDistance < 0.5f && !isWaiting)
        {
            ReachedWaypoint(agent.transform.position); // Handle waypoint reached
        }

        UpdateHealthBar(); // Update the health bar orientation
    }
    #endregion

    #region Waypoint Methods
    private void MoveToNextWaypoint()
    {
        // Return if there are no waypoints
        if (waypoints.Length == 0)
            return;

        // Set the destination to the next waypoint
        agent.SetDestination(waypoints[currentWaypointIndex].position);
        animator.SetBool("isWalking", true); // Play walking animation
        animator.SetBool("isIdle", false); // Ensure idle animation is off

        // Update the waypoint index
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }

    public void ReachedWaypoint(Vector3 collisionPosition)
    {
        StartCoroutine(IdleAtPoint(collisionPosition)); // Start idling at the waypoint
    }

    private IEnumerator IdleAtPoint(Vector3 collisionPosition)
    {
        isWaiting = true; // Set waiting flag
        agent.isStopped = true; // Stop the agent
        agent.Warp(collisionPosition); // Stop at the exact position
        animator.SetBool("isWalking", false); // Stop walking animation
        animator.SetBool("isIdle", true); // Play idle animation

        // Wait for a random time between min and max wait times
        yield return new WaitForSeconds(Random.Range(waitTimeMin, waitTimeMax));

        animator.SetBool("isIdle", false); // Stop idle animation
        agent.isStopped = false; // Resume movement
        isWaiting = false; // Reset waiting flag

        MoveToNextWaypoint(); // Move to the next waypoint
    }
    #endregion

    #region Health Methods
    public void HitByProjectile()
    {
        hitCount++; // Increment hit counter
        if (hitCount >= maxHits)
        {
            StartCoroutine(HandleDeath()); // Handle death if max hits reached
        }
        else
        {
            UpdateHealthBar(); // Update health bar if not dead
        }
    }

    public void HitByGrenade(float damage)
    {
        hitCount += Mathf.CeilToInt(damage); // Increment hit counter based on damage
        if (hitCount >= maxHits)
        {
            StartCoroutine(HandleDeath()); // Handle death if max hits reached
        }
        else
        {
            UpdateHealthBar(); // Update health bar if not dead
        }
        healthBarBackground.color = GetRandomColorExcludingRedAndGreen(); // Change health bar background to a random color but not red
    }

    private Color GetRandomColorExcludingRedAndGreen()
    {
        Color randomColor;
        do
        {
            randomColor = new Color(Random.value, Random.value, Random.value);
        } while ((randomColor.r > 0.5f && randomColor.g < 0.5f && randomColor.b < 0.5f) || // Check if the color is too close to red
                (randomColor.r < 0.5f && randomColor.g > 0.5f && randomColor.b < 0.5f)); // Check if the color is too close to green
        return randomColor;
    }

    private IEnumerator HandleDeath()
    {
        StopAllCoroutines(); // Stop all running coroutines
        agent.isStopped = true; // Stop the agent's movement
        animator.SetBool("isWalking", false); // Stop walking animation
        animator.Rebind(); // Reset animator state
        audioSource.PlayOneShot(deathSound); // Play death sound

        isWaiting = false; // Reset the waiting flag

        ResetAgentPosition(); // Reset agent position
        hitCount = 0; // Reset hit count
        healthBarBackground.color = Color.red; // Reset health bar background to red
        UpdateHealthBar(); // Reset health bar

        yield return new WaitForSeconds(1f); // Delay before respawning

        RespawnAgent(); // Respawn the agent
    }

    private void RespawnAgent()
    {
        agent.isStopped = false; // Resume the agent's movement
        MoveToNextWaypoint(); // Resume patrolling
    }

    private void ResetAgentPosition()
    {
        agent.Warp(initialPosition); // Immediately set the agent's position to its initial position
    }

    private void InitializeHealthBar()
    {
        healthBar.maxValue = maxHits; // Set max value of health bar
        healthBar.minValue = 0; // Set min value of health bar
        healthBar.value = maxHits; // Start with full health
        healthBarBackground.color = Color.red; // Set initial background color to red
    }

    private void UpdateHealthBar()
    {
        healthBar.value = maxHits - hitCount; // Update health bar value

        // Make the health bar face the camera
        Vector3 directionToCamera = mainCamera.transform.position - healthBarCanvas.position;
        directionToCamera.x = directionToCamera.z = 0;
        healthBarCanvas.LookAt(mainCamera.transform.position - directionToCamera);

        healthBarCanvas.Rotate(0, 0, 0);
    }
    #endregion
}
