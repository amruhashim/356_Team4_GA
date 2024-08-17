#if UNITY_EDITOR
using UnityEditor;
#endif

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
    private Transform detectedTarget;
    private bool isTargetInRange = false;
    private bool isTargetInVisionAngle = false;



    #endregion

    #region Unity Methods
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        healthBarBackground = healthBar.transform.Find("Background").GetComponent<Image>();
        stateManager = GetComponent<AIStateManager>();

        InitializeHealthBar();
        initialPosition = transform.position;
        MoveToNextWaypoint();
        stateManager = GetComponent<AIStateManager>();
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

            // Detect target while patrolling
            DetectTarget();

            // If the target is detected, switch to Chasing state
            if (isTargetInRange && isTargetInVisionAngle)
            {
                stateManager.ChangeState(AIStateManager.AIState.Chasing);
            }

            UpdateHealthBar();
        }
    }
    #endregion

    #region Detection Methods
    public float minDetectionDistance = 2.0f; // adjust this value as needed

    private void DetectTarget()
    {
        isTargetInRange = false;
        isTargetInVisionAngle = false;

        // Use OverlapSphere to detect all objects within vision range
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

                    // Detect target if within a minimum distance regardless of angle
                    if (distanceToTarget <= minDetectionDistance || angle <= visionAngle / 2f)
                    {
                        isTargetInRange = true;
                        isTargetInVisionAngle = true;
                        detectedTarget = hit.transform;

                        Debug.Log($"PatrolAgent: Target with tag {tag} detected within range and vision angle.");
                        stateManager.ChangeState(AIStateManager.AIState.Chasing, targetType);
                        return; // Exit once the correct target is found
                    }
                }
            }
        }

        //Debug.Log("PatrolAgent: No valid targets found.");
    }


    public Transform GetDetectedTarget()
    {
        return detectedTarget;
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


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Custom GUIStyle for text with background
        GUIStyle labelStyle = new GUIStyle
        {
            normal = { textColor = Color.black },
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(5, 5, 5, 5) // Padding around the text
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
        DrawLabelWithBackground(transform.position + Vector3.up * (visionRange + 1f), "Patrol Vision Range", labelStyle, backgroundColor);

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
        DrawLabelWithBackground(transform.position + forward.normalized * (visionRange + 1f), "Vision Range", labelStyle, backgroundColor);

        // Draw labels for the state and vision range
        DrawLabelWithBackground(transform.position + Vector3.up * 2, $"State: {stateManager?.currentState ?? AIStateManager.AIState.Patrolling}", labelStyle, backgroundColor);
        DrawLabelWithBackground(transform.position + forward.normalized * visionRange, $"Vision Range: {visionRange}m", labelStyle, backgroundColor);
    }

    private void DrawLabelWithBackground(Vector3 position, string text, GUIStyle style, Color backgroundColor)
    {
        Handles.BeginGUI();

        // Convert the 3D position to a 2D GUI position
        Vector2 guiPosition = HandleUtility.WorldToGUIPoint(position);

        // Calculate the size of the text
        Vector2 size = style.CalcSize(new GUIContent(text));

        // Draw a background rectangle
        EditorGUI.DrawRect(new Rect(guiPosition.x - size.x / 2, guiPosition.y - size.y / 2, size.x, size.y), backgroundColor);

        // Draw the label
        GUI.Label(new Rect(guiPosition.x - size.x / 2, guiPosition.y - size.y / 2, size.x, size.y), text, style);

        Handles.EndGUI();
    }
#endif



    #endregion
}
