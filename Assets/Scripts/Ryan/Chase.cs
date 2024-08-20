#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.AI;
using System.Collections;


public class Chase : MonoBehaviour
{
    public float visionRange = 20f;
    public float visionAngle = 60f;
    public float chasingSpeed = 5f;
    public float lostPlayerFreezeTime = 3f;
    public float minimumDetectionDistance = 1.5f;
    public float maxVisionAngle = 180f;
    public GameObject bulletPrefab;
    public Transform shootingPoint;
    public float bulletSpeed = 20f;
    public float fireRate = 1f;
    public AudioClip shootingSound;

    private bool isTargetInRange = false;
    private bool isTargetInVisionAngle = false;
    private Transform targetTransform;
    private AIStateManager.TargetType currentTargetType;
    private NavMeshAgent agent;
    private Animator animator;
    private AIStateManager stateManager;
    private PatrolAgent patrolAgent;
    private float lostTargetTimer = 0f;
    private bool isIdling = false;
    private float idleElapsedTime = 0f;
    private float nextFireTime = 0f;
    private AudioSource audioSource;

    public float shootingStartRange = 10f;
    public float shootingStopRange = 15f;


    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        stateManager = GetComponent<AIStateManager>();
        patrolAgent = GetComponent<PatrolAgent>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("Chase: No AudioSource found on the AI. Please add an AudioSource component.");
        }
    }

    public void Update()
    {
        if (targetTransform == null)
        {
            HandleTargetLost();
            return;
        }

        DetectTarget();

        if (isTargetInRange && isTargetInVisionAngle)
        {
            HandleChaseState();
            float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);
            if (distanceToTarget <= shootingStartRange)
            {
                TryShoot();
            }
        }
        else
        {
            HandleLostTargetState();
        }
    }


    private void HandleChaseState()
    {
        lostTargetTimer = lostPlayerFreezeTime;

        if (isIdling)
        {
            isIdling = false;
            idleElapsedTime = 0f;
        }

        ChaseTarget();
    }

    private void HandleTargetLost()
    {
        Debug.Log("Chase: Target lost or destroyed. Returning to patrolling.");
        ReturnToPatrolling();
    }




    private void HandleLostTargetState()
    {
        lostTargetTimer -= Time.deltaTime;

        if (lostTargetTimer <= 0f)
        {
            if (!isIdling)
            {
                StartIdling();
            }
            else
            {
                ContinueIdling();
            }
        }
        else
        {
            // Stop immediately if the target is lost
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            animator.SetBool("isWalking", false);
            animator.SetBool("isIdle", true);
        }
    }

    private void StartIdling()
    {
        isIdling = true;
        idleElapsedTime = 0f;


        agent.ResetPath();
        agent.velocity = Vector3.zero;

        animator.SetBool("isWalking", false);
        animator.SetBool("isIdle", true);

        Debug.Log("Chase: Idling before returning to patrolling.");
    }

    private void ContinueIdling()
    {
        idleElapsedTime += Time.deltaTime;

        if (idleElapsedTime >= patrolAgent.waitTimeMin)
        {
            ReturnToPatrolling();
        }
        else
        {
            DetectTargetWhileIdling();
        }
    }

    private void DetectTargetWhileIdling()
    {
        DetectTarget();

        if (isTargetInRange && isTargetInVisionAngle)
        {
            isIdling = false;
            idleElapsedTime = 0f;
            stateManager.ChangeState(AIStateManager.AIState.Chasing, currentTargetType);
            ChaseTarget();
        }
    }

    public void SetTarget(Transform target, AIStateManager.TargetType targetType)
    {
        targetTransform = target;
        currentTargetType = targetType;
    }

    private void DetectTarget()
    {
        isTargetInRange = false;
        isTargetInVisionAngle = false;

        string tag = stateManager.GetTagForTarget(currentTargetType);

        Collider[] hits = Physics.OverlapSphere(transform.position, visionRange);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag(tag))
            {
                Vector3 directionToTarget = (hit.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, directionToTarget);

                float distanceToTarget = Vector3.Distance(transform.position, hit.transform.position);

                // Calculate a dynamic vision angle based on distance to target
                float dynamicVisionAngle = maxVisionAngle * (1 + (1 - distanceToTarget / visionRange));

                // Detect target if within a minimum distance regardless of angle
                if (distanceToTarget <= minimumDetectionDistance || angle <= dynamicVisionAngle / 2f)
                {
                    isTargetInRange = true;
                    isTargetInVisionAngle = true;
                    targetTransform = hit.transform;

                    Debug.Log($"Chase: Target with tag {tag} detected within range and vision angle.");
                    return; // Exit once the correct target is found
                }
            }
        }

        Debug.Log("Chase: No valid targets found.");
    }

    private void ChaseTarget()
    {
        agent.speed = chasingSpeed;
        agent.SetDestination(targetTransform.position);
        animator.SetBool("isWalking", true);
        animator.SetBool("isIdle", false);

        Debug.Log($"Chase: Chasing target of type {currentTargetType} at position {targetTransform.position}");
    }


    private void ReturnToPatrolling()
    {
        targetTransform = null;
        stateManager.ChangeState(AIStateManager.AIState.Patrolling);
        patrolAgent.MoveToNextWaypoint();
        isIdling = false;
        idleElapsedTime = 0f;
        animator.SetBool("isWalking", true);
        animator.SetBool("isIdle", false);
        Debug.Log("Chase: Resuming patrolling.");
    }


    private void TryShoot()
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);

        // Check if the target is within the shooting start range and not beyond the shooting stop range
        if (distanceToTarget <= shootingStartRange && distanceToTarget >= minimumDetectionDistance)
        {
            // Check if the AI is facing the target
            Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, directionToTarget);
            if (dotProduct >= 0.5f) 
            {
                if (Time.time >= nextFireTime && targetTransform != null)
                {
                    // Trigger the firing animation
                    animator.SetTrigger("Fire");

                    // Perform the shooting
                    Shoot();

                    // Set next fire time based on fire rate
                    nextFireTime = Time.time + 1f / fireRate;
                }
            }
        }
        else
        {
            // Reset the firing trigger if the target is out of range
            animator.ResetTrigger("Fire");
        }
    }


    private void Shoot()
    {
        if (bulletPrefab == null || shootingPoint == null || targetTransform == null)
        {
            return;
        }

        // Instantiate the bullet
        GameObject bullet = Instantiate(bulletPrefab, shootingPoint.position, Quaternion.identity);

        // Calculate direction towards the target
        Vector3 direction = (targetTransform.position - shootingPoint.position).normalized;

        // Apply force to the bullet
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = direction * bulletSpeed;
        }

        // Play the shooting sound
        if (shootingSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootingSound);
        }

        Debug.Log("Chase: Shooting bullet at the target.");
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
            padding = new RectOffset(2, 2, 2, 2), // Reduced padding around the text
            fontSize = 7 // Reduced font size for a smaller label
        };

        // Background color for labels
        Color backgroundColor = Color.white; // White background for all labels

        // Draw bold chasing vision range (orange) with label
        DrawBoldSphere(transform.position, visionRange, new Color(1.0f, 0.5f, 0.0f)); // Orange
        DrawLabelWithBackground(transform.position + Vector3.up * (visionRange + 0.5f), $"Chasing Vision Range: {visionRange}m", labelStyle, backgroundColor);

        // Draw the AI's forward direction with bold lines and label it
        Vector3 forwardDirection = transform.forward * visionRange;
        DrawBoldRay(transform.position, forwardDirection, new Color(0.2f, 0.2f, 0.8f)); // Blue
        DrawLabelWithBackground(transform.position + forwardDirection.normalized * (visionRange + 0.5f), "Forward Direction", labelStyle, backgroundColor);

        if (targetTransform != null)
        {
            Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;

            // Draw the direction to the target with bold lines and label it
            DrawBoldRay(transform.position, directionToTarget * visionRange, new Color(1.0f, 0.8f, 0.0f)); // Yellow
            DrawLabelWithBackground(transform.position + directionToTarget.normalized * (visionRange + 0.5f), "Direction to Target", labelStyle, backgroundColor);

            // Calculate dot product
            float dotProduct = Vector3.Dot(transform.forward, directionToTarget);

            // Visualize the dot product as a color gradient
            Color dotColor = Color.Lerp(new Color(0.8f, 0.2f, 0.2f), new Color(0.2f, 0.8f, 0.2f), (dotProduct + 1) / 2);
            Gizmos.color = dotColor;

            // Draw a small sphere to represent the dot product value and label it
            Vector3 dotPosition = transform.position + forwardDirection.normalized * 1.5f;
            Gizmos.DrawSphere(dotPosition, 0.2f);
            DrawLabelWithBackground(dotPosition + Vector3.up * 0.5f, $"Dot Product: {dotProduct:F2}", labelStyle, backgroundColor);

            // Calculate the dynamic vision angle
            float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);
            float dynamicVisionAngle = maxVisionAngle * (1 + (1 - distanceToTarget / visionRange));
            Vector3 leftBoundary = Quaternion.Euler(0, -dynamicVisionAngle / 2, 0) * forwardDirection;
            Vector3 rightBoundary = Quaternion.Euler(0, dynamicVisionAngle / 2, 0) * forwardDirection;

            // Change color based on whether the target is in range
            Color dynamicColor = isTargetInRange ? Color.yellow : Color.green; // Yellow when in range, green when escaped

            // Draw the filled dynamic vision cone with very low opacity
            Handles.color = new Color(dynamicColor.r, dynamicColor.g, dynamicColor.b, 0.05f); // Very low opacity
            Handles.DrawSolidArc(transform.position, Vector3.up, leftBoundary, dynamicVisionAngle, visionRange);

            // Draw the boundary rays of the vision cone
            DrawBoldRay(transform.position, leftBoundary, dynamicColor);
            DrawBoldRay(transform.position, rightBoundary, dynamicColor);

            // Optionally, label the dynamic vision angle
            DrawLabelWithBackground(transform.position + forwardDirection.normalized * (visionRange + 2f), $"Dynamic Vision Angle: {dynamicVisionAngle:F2}°", labelStyle, backgroundColor);
        }

        // Draw bold shooting start range (red) with label
        DrawBoldSphere(transform.position, shootingStartRange, Color.red);
        DrawLabelWithBackground(transform.position + Vector3.up * (shootingStartRange + 0.5f), $"Shooting Start Range: {shootingStartRange}m", labelStyle, backgroundColor);

        // Draw bold shooting stop range (green) with label
        DrawBoldSphere(transform.position, shootingStopRange, Color.green);
        DrawLabelWithBackground(transform.position + Vector3.up * (shootingStopRange + 0.5f), $"Shooting Stop Range: {shootingStopRange}m", labelStyle, backgroundColor);
    }

    // Method to draw a bold sphere by drawing multiple slightly offset spheres
    private void DrawBoldSphere(Vector3 position, float radius, Color color)
    {
        Gizmos.color = color;
        for (float i = -0.1f; i <= 0.1f; i += 0.05f)
        {
            Gizmos.DrawWireSphere(position, radius + i);
        }
    }

    // Method to draw a bold line by drawing multiple slightly offset lines
    private void DrawBoldRay(Vector3 start, Vector3 direction, Color color)
    {
        Gizmos.color = color;
        for (float i = -0.1f; i <= 0.1f; i += 0.05f)
        {
            Gizmos.DrawRay(start + new Vector3(i, 0, 0), direction);
        }
    }

    // Method to draw a label with a background
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

}






