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

                float dynamicVisionAngle = maxVisionAngle * (1 + (1 - distanceToTarget / visionRange));

                if (distanceToTarget <= minimumDetectionDistance || angle <= dynamicVisionAngle / 2f)
                {
                    isTargetInRange = true;
                    isTargetInVisionAngle = true;
                    targetTransform = hit.transform;

                    Debug.Log($"Chase: Target with tag {tag} detected within range and vision angle.");
                    return;
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

        if (distanceToTarget <= shootingStartRange && distanceToTarget >= minimumDetectionDistance)
        {
            Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, directionToTarget);
            if (dotProduct >= 0.15f)
            {
                if (Time.time >= nextFireTime && targetTransform != null)
                {
                    animator.SetTrigger("Fire");
                    Shoot();
                    nextFireTime = Time.time + 1f / fireRate;
                }
            }
        }
        else
        {
            animator.ResetTrigger("Fire");
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null || shootingPoint == null || targetTransform == null)
        {
            return;
        }

        Vector3 direction = (targetTransform.position - shootingPoint.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, shootingPoint.position, Quaternion.identity);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = direction * bulletSpeed;
        }

        RaycastHit hit;
        if (Physics.Raycast(shootingPoint.position, direction, out hit, Mathf.Infinity))
        {
            if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Drone"))
            {
                if (shootingSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(shootingSound);
                }

                if (hit.collider.CompareTag("Player"))
                {
                    PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.PlayerTakingDamage(3f);

                        if (playerHealth.fadeCoroutine != null)
                        {
                            playerHealth.StopCoroutine(playerHealth.fadeCoroutine);
                        }
                        playerHealth.fadeCoroutine = playerHealth.StartCoroutine(playerHealth.FadeOutHealthImpact());
                    }

                    Debug.Log("Raycast hit the player directly!");
                }
                else if (hit.collider.CompareTag("Drone"))
                {
                    WeaponSwitcher weaponSwitcher = FindObjectOfType<WeaponSwitcher>();
                    if (weaponSwitcher != null)
                    {
                        weaponSwitcher.ToggleDrone();
                    }

                    Debug.Log("Raycast hit the drone directly!");
                }
            }
            else
            {
                Debug.Log($"Raycast hit: {hit.collider.name}");
            }
        }

        float bulletLifetime = 5.0f;
        Destroy(bullet, bulletLifetime);

        Debug.Log("Chase: Shooting bullet at the target.");
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        GUIStyle labelStyle = new GUIStyle
        {
            normal = { textColor = Color.black },
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(2, 2, 2, 2),
            fontSize = 7
        };

        Color backgroundColor = Color.white;

        DrawBoldSphere(transform.position, visionRange, new Color(1.0f, 0.5f, 0.0f));
        DrawLabelWithBackground(transform.position + Vector3.up * (visionRange + 0.5f), $"Chasing Vision Range: {visionRange}m", labelStyle, backgroundColor);

        Vector3 forwardDirection = transform.forward * visionRange;
        DrawBoldRay(transform.position, forwardDirection, new Color(0.2f, 0.2f, 0.8f));
        DrawLabelWithBackground(transform.position + forwardDirection.normalized * (visionRange + 0.5f), "Forward Direction", labelStyle, backgroundColor);

        if (targetTransform != null)
        {
            Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;

            DrawBoldRay(transform.position, directionToTarget * visionRange, new Color(1.0f, 0.8f, 0.0f));
            DrawLabelWithBackground(transform.position + directionToTarget.normalized * (visionRange + 0.5f), "Direction to Target", labelStyle, backgroundColor);

            float dotProduct = Vector3.Dot(transform.forward, directionToTarget);

            Color dotColor = Color.Lerp(new Color(0.8f, 0.2f, 0.2f), new Color(0.2f, 0.8f, 0.2f), (dotProduct + 1) / 2);
            Gizmos.color = dotColor;

            Vector3 dotPosition = transform.position + forwardDirection.normalized * 1.5f;
            Gizmos.DrawSphere(dotPosition, 0.2f);
            DrawLabelWithBackground(dotPosition + Vector3.up * 0.5f, $"Dot Product: {dotProduct:F2}", labelStyle, backgroundColor);

            float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);
            float dynamicVisionAngle = maxVisionAngle * (1 + (1 - distanceToTarget / visionRange));
            Vector3 leftBoundary = Quaternion.Euler(0, -dynamicVisionAngle / 2, 0) * forwardDirection;
            Vector3 rightBoundary = Quaternion.Euler(0, dynamicVisionAngle / 2, 0) * forwardDirection;

            Color dynamicColor = isTargetInRange ? Color.yellow : Color.green;

            Handles.color = new Color(dynamicColor.r, dynamicColor.g, dynamicColor.b, 0.05f);
            Handles.DrawSolidArc(transform.position, Vector3.up, leftBoundary, dynamicVisionAngle, visionRange);

            DrawBoldRay(transform.position, leftBoundary, dynamicColor);
            DrawBoldRay(transform.position, rightBoundary, dynamicColor);

            DrawLabelWithBackground(transform.position + forwardDirection.normalized * (visionRange + 2f), $"Dynamic Vision Angle: {dynamicVisionAngle:F2}°", labelStyle, backgroundColor);
        }

        DrawBoldSphere(transform.position, shootingStartRange, Color.red);
        DrawLabelWithBackground(transform.position + Vector3.up * (shootingStartRange + 0.5f), $"Shooting Start Range: {shootingStartRange}m", labelStyle, backgroundColor);
    }

    private void DrawBoldSphere(Vector3 position, float radius, Color color)
    {
        Gizmos.color = color;
        for (float i = -0.1f; i <= 0.1f; i += 0.05f)
        {
            Gizmos.DrawWireSphere(position, radius + i);
        }
    }

    private void DrawBoldRay(Vector3 start, Vector3 direction, Color color)
    {
        Gizmos.color = color;
        for (float i = -0.1f; i <= 0.1f; i += 0.05f)
        {
            Gizmos.DrawRay(start + new Vector3(i, 0, 0), direction);
        }
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
