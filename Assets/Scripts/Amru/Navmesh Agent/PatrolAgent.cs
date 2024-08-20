#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PatrolAgent : MonoBehaviour
{ 
    #region Serialized Fields
    [Tooltip("Unique ID for this AI.")]
    public string uniqueID;  // A

    [Tooltip("Spawn points for the AI.")]
public Transform[] spawnPoints;  // Add this field

    
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

    [Tooltip("Prefab for the sound waypoint.")]
    public GameObject soundWaypointPrefab;

    [Tooltip("Range within which the AI can hear sounds.")]
    public float hearingRange = 15f;
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
    private Transform detectedTarget;
    private bool isTargetInRange = false;
    private bool isTargetInVisionAngle = false;

    // Separate list for sound waypoints
    private List<Transform> soundWaypoints = new List<Transform>();
    private bool isUsingSoundWaypoints = false;
    #endregion

    #region Unity Methods
private void Start()
{
    if (PlayerState.Instance != null && PlayerState.Instance.IsAIDead(uniqueID))
    {
        Debug.Log($"[Start] PatrolAgent {uniqueID} is dead. Deactivating.");
        gameObject.SetActive(false); // Deactivate the AI if it is dead
        return;
    }

    Debug.Log($"[Start] PatrolAgent {uniqueID} is alive. Initializing.");

    agent = GetComponent<NavMeshAgent>();
    animator = GetComponent<Animator>();
    audioSource = GetComponent<AudioSource>();
    healthBarBackground = healthBar.transform.Find("Background").GetComponent<Image>();
    stateManager = GetComponent<AIStateManager>();

    InitializeHealthBar();

    // Use the first waypoint as the initial spawn point
    if (waypoints.Length > 0)
    {
        initialPosition = waypoints[0].position;
        agent.Warp(initialPosition);
    }
    else
    {
        Debug.LogWarning("No waypoints assigned. Using current position as the spawn point.");
        initialPosition = transform.position;
    }

    MoveToNextWaypoint();
}



    private void Update()
    {
        if (stateManager.currentState == AIStateManager.AIState.Patrolling)
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f && !isWaiting)
            {
                ReachedWaypoint(agent.transform.position);
            }

            DetectTarget();

            if (isTargetInRange && isTargetInVisionAngle)
            {
                stateManager.ChangeState(AIStateManager.AIState.Chasing);
            }

            DetectSoundSources();
            UpdateHealthBar();
        }
    }
    #endregion

    #region Detection Methods
    public float minDetectionDistance = 2.0f;

    private void DetectTarget()
    {
        isTargetInRange = false;
        isTargetInVisionAngle = false;

        Collider[] hits = Physics.OverlapSphere(transform.position, visionRange);
        foreach (Collider hit in hits)
        {
            foreach (AIStateManager.TargetType targetType in System.Enum.GetValues(typeof(AIStateManager.TargetType)))
            {
                string tag = stateManager.GetTagForTarget(targetType);

                if (hit.CompareTag(tag))
                {
                    Vector3 directionToTarget = (hit.transform.position - transform.position).normalized;
                    float angle = Vector3.Angle(transform.forward, directionToTarget);
                    float distanceToTarget = Vector3.Distance(transform.position, hit.transform.position);

                    if (distanceToTarget <= minDetectionDistance || angle <= visionAngle / 2f)
                    {
                        isTargetInRange = true;
                        isTargetInVisionAngle = true;
                        detectedTarget = hit.transform;

                        Debug.Log($"PatrolAgent: Target with tag {tag} detected within range and vision angle.");
                        stateManager.ChangeState(AIStateManager.AIState.Chasing, targetType);
                        return;
                    }
                }
            }
        }
    }

    private void DetectSoundSources()
    {
        Collider[] soundHits = Physics.OverlapSphere(transform.position, hearingRange);
        foreach (Collider hit in soundHits)
        {
            SoundSource soundSource = hit.GetComponent<SoundSource>();
            if (soundSource != null && soundSource.IsPlaying())
            {
                CreateSoundWaypoint(soundSource.GetSoundPosition());
                break;
            }
        }
    }

    private void CreateSoundWaypoint(Vector3 soundPosition)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(soundPosition, out hit, hearingRange, NavMesh.AllAreas))
        {
            Vector3 navMeshPosition = hit.position;
            bool waypointExists = soundWaypoints.Exists(wp => Vector3.Distance(wp.position, navMeshPosition) < 1f);

            if (!waypointExists)
            {
                GameObject soundWaypoint = Instantiate(soundWaypointPrefab, navMeshPosition, Quaternion.identity);
                soundWaypoint.tag = "SoundWaypoint";
                soundWaypoints.Add(soundWaypoint.transform);
            }
        }
    }

    public Transform GetDetectedTarget()
    {
        return detectedTarget;
    }
    #endregion

    #region Waypoint Methods
    public void MoveToNextWaypoint()
    {
        // Prioritize sound waypoints if they exist
        if (soundWaypoints.Count > 0)
        {
            isUsingSoundWaypoints = true;
            agent.SetDestination(soundWaypoints[0].position);
        }
        else
        {
            isUsingSoundWaypoints = false;
            agent.SetDestination(waypoints[currentWaypointIndex].position);
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }

        animator.SetBool("isWalking", true);
        animator.SetBool("isIdle", false);
    }
public void ReachedWaypoint(Vector3 collisionPosition)
{
    if (isUsingSoundWaypoints)
    {
        // Remove the sound waypoint and continue patrolling without waiting
        Destroy(soundWaypoints[0].gameObject);
        soundWaypoints.RemoveAt(0);

        if (soundWaypoints.Count == 0)
        {
            currentWaypointIndex = 0; // Reset index for normal waypoints
        }

        // Directly move to the next waypoint without waiting
        MoveToNextWaypoint();
    }
    else
    {
        // Normal waypoint logic with idle time
        StartCoroutine(IdleAtPoint(collisionPosition));
    }
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
            HandleDeath();
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
            HandleDeath();
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

private void HandleDeath()
{
    Debug.Log($"[HandleDeath] PatrolAgent {uniqueID} has died.");
    StopAllCoroutines();
    agent.isStopped = true;
    animator.SetBool("isWalking", false);
    animator.Rebind();
    audioSource.PlayOneShot(deathSound);

    // Mark this AI as dead in PlayerState
    PlayerState.Instance?.UpdateAIState(uniqueID, true);
    Debug.Log($"[HandleDeath] PatrolAgent {uniqueID} state updated in PlayerState.");

    healthBarBackground.color = Color.red;
    UpdateHealthBar();

    gameObject.SetActive(false); // Deactivate the AI immediately without delay
}



// RespawnAgent method checks if the AI should respawn
private void RespawnAgent()
{
    if (PlayerState.Instance != null && !PlayerState.Instance.IsAIDead(uniqueID))
    {
        agent.isStopped = false;
        stateManager.ChangeState(AIStateManager.AIState.Patrolling);
        MoveToNextWaypoint();
    }
    else
    {
        gameObject.SetActive(false); // Deactivate the AI if it should not respawn
    }
}



public void ResetAgentPosition()
{
    if (agent == null)
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component is missing!");
            return;
        }
    }

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

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Custom GUIStyle for small text with background
        GUIStyle smallLabelStyle = new GUIStyle
        {
            normal = { textColor = Color.black },
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(2, 2, 2, 2), // Reduced padding
            fontSize = 7 // Smaller font size
        };

        // Background color for labels
        Color backgroundColor = new Color(1f, 1f, 0.8f, 0.8f); // Light yellow

        // Fluorescent purple color
        Color fluorescentPurple = new Color(0.75f, 0.0f, 1.0f); // Strong purple

        // Set Gizmo color based on detection status
        Gizmos.color = isTargetInRange && isTargetInVisionAngle ? fluorescentPurple : new Color(0.5f, 0.0f, 0.75f); // Fluorescent purple or a slightly darker purple

        // Draw a bold detection range sphere by layering multiple spheres
        for (float i = 0; i < 3; i += 0.1f)
        {
            Gizmos.DrawWireSphere(transform.position, visionRange + i * 0.05f);
        }

        // Label the patrol vision range sphere
        DrawLabelWithBackground(transform.position + Vector3.up * (visionRange + 1f), "Patrol Vision Range", smallLabelStyle, backgroundColor);

        // Draw the vision cone with thicker lines
        Vector3 forward = transform.forward * visionRange;
        Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle / 2, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, visionAngle / 2, 0) * forward;

        // Draw multiple lines for a thicker effect
        Gizmos.color = fluorescentPurple;
        for (float offset = -0.1f; offset <= 0.1f; offset += 0.05f)
        {
            Gizmos.DrawRay(transform.position + new Vector3(offset, 0, 0), leftBoundary);
            Gizmos.DrawRay(transform.position + new Vector3(offset, 0, 0), rightBoundary);
        }

        // Draw an arc for the vision cone with low opacity
        Handles.color = new Color(0.75f, 0.0f, 1.0f, 0.1f); // Very low opacity fluorescent purple
        Handles.DrawSolidArc(transform.position, Vector3.up, leftBoundary.normalized, visionAngle, visionRange);

        // Label the vision range
        DrawLabelWithBackground(transform.position + forward.normalized * (visionRange + 1f), "Vision Range", smallLabelStyle, backgroundColor);

        // Draw labels for the state and vision range
        DrawLabelWithBackground(transform.position + Vector3.up * 2, $"State: {stateManager?.currentState ?? AIStateManager.AIState.Patrolling}", smallLabelStyle, backgroundColor);
        DrawLabelWithBackground(transform.position + forward.normalized * visionRange, $"Vision Range: {visionRange}m", smallLabelStyle, backgroundColor);

        // Visualize the hearing range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
        DrawLabelWithBackground(transform.position + Vector3.up * (hearingRange + 1f), "Hearing Range", smallLabelStyle, backgroundColor);

        // Visualize all waypoints and the travel path
        Gizmos.color = Color.blue;
        Vector3 previousPosition = transform.position;
        for (int i = 0; i < waypoints.Length; i++)
        {
            Transform waypoint = waypoints[i];
            if (waypoint == null) continue;

            Gizmos.color = Color.blue; // Normal waypoints are blue
            Gizmos.DrawSphere(waypoint.position, 0.5f);

            // Draw lines connecting waypoints
            Gizmos.DrawLine(previousPosition, waypoint.position);
            previousPosition = waypoint.position;
        }

        // Visualize sound waypoints and the travel path
        Gizmos.color = Color.green; // Sound waypoints are green
        foreach (Transform soundWaypoint in soundWaypoints)
        {
            Gizmos.DrawSphere(soundWaypoint.position, 0.5f);
            Gizmos.DrawLine(previousPosition, soundWaypoint.position);
            previousPosition = soundWaypoint.position;
        }

        // Close the path if looping back to the initial waypoint
        Gizmos.DrawLine(previousPosition, waypoints[currentWaypointIndex].position);
    }

    private void DrawLabelWithBackground(Vector3 position, string text, GUIStyle style, Color backgroundColor)
    {
        Handles.BeginGUI();
        Vector2 guiPosition = HandleUtility.WorldToGUIPoint(position);
        Vector2 size = style.CalcSize(new GUIContent(text));
        EditorGUI.DrawRect(new Rect(guiPosition.x - size.x / 2, guiPosition.y - size.y / 2, size.x, size.y), backgroundColor);
        GUI.Label(new Rect(guiPosition.x - size.x / 2, guiPosition.y - size.y / 2, size.x, size.y), text, style);
        Handles.EndGUI();
    }
#endif

}
