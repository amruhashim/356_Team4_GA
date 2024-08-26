using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Radar : MonoBehaviour
{
    private Transform pivotTransform;
    private Transform sweepTransform;
    private float rotationSpeed;
    private float radarDistance;
    private float sweepAngle = 30.0f;

    public GameObject enemyDotPrefab;
    public GameObject hostageDotPrefab;
    public RawImage minimapRawImage; // Assign the Raw Image in the inspector
    public Camera minimapCamera; // Assign the minimap camera in the inspector

    private Dictionary<Collider, GameObject> activeDots = new Dictionary<Collider, GameObject>();

    private void Awake()
    {
        pivotTransform = transform.Find("SweepPivot");
        sweepTransform = transform.Find("SweepPivot/Sweep");

        rotationSpeed = 180.0f;
        radarDistance = 20.0f;
    }

    void Update()
    {
        pivotTransform.eulerAngles -= new Vector3(0, rotationSpeed * Time.deltaTime, 0);

        Collider[] hits = Physics.OverlapSphere(transform.position, radarDistance);

        foreach (Collider hit in hits)
        {
            Vector3 directionToTarget = (hit.transform.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(sweepTransform.forward, directionToTarget);

            if (angleToTarget < sweepAngle)
            {
                if (!activeDots.ContainsKey(hit))
                {
                    GameObject dotPrefab = null;
                    if (hit.CompareTag("PatrolAgent"))
                    {
                        dotPrefab = enemyDotPrefab;
                    }
                    else if (hit.CompareTag("Hostage"))
                    {
                        dotPrefab = hostageDotPrefab;
                    }

                    if (dotPrefab != null)
                    {
                        Vector3 dotPosition = GetDotPosition(hit.transform.position);
                        GameObject dot = Instantiate(dotPrefab, dotPosition, Quaternion.identity, minimapRawImage.transform);
                        activeDots.Add(hit, dot);

                        StartCoroutine(FadeDot(dot, hit));
                    }
                }
            }
        }

        List<Collider> removedDots = new List<Collider>();
        foreach (var entry in activeDots)
        {
            if (entry.Key == null || !IsWithinSweep(entry.Key.transform.position))
            {
                Destroy(entry.Value);
                removedDots.Add(entry.Key);
            }
        }
        foreach (Collider collider in removedDots)
        {
            activeDots.Remove(collider);
        }
    }

    private bool IsWithinSweep(Vector3 targetPosition)
    {
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        float angleToTarget = Vector3.Angle(sweepTransform.forward, directionToTarget);
        return angleToTarget < sweepAngle;
    }

    private IEnumerator FadeDot(GameObject dot, Collider target)
    {
        CanvasGroup canvasGroup = dot.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = dot.AddComponent<CanvasGroup>();
        }

        float duration = 2.0f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (target == null || dot == null || canvasGroup == null)
            {
                break;
            }

            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / duration);

            if (dot != null)
            {
                dot.transform.position = GetDotPosition(target.transform.position);
            }

            yield return null;
        }

        if (dot != null)
        {
            Destroy(dot);
            activeDots.Remove(target);
        }
    }

    private Vector3 GetDotPosition(Vector3 worldPosition)
    {
        // Get the relative position of the target to the player
        Vector3 relativePosition = worldPosition - transform.position;

        // Convert the world position to minimap camera viewport coordinates
        Vector3 viewportPos = minimapCamera.WorldToViewportPoint(worldPosition);

        // Calculate the position within the Raw Image based on the viewport coordinates
        RectTransform rectTransform = minimapRawImage.rectTransform;
        Vector2 anchoredPosition = new Vector2(
            (viewportPos.x - 0.5f) * rectTransform.sizeDelta.x,
            (viewportPos.y - 0.5f) * rectTransform.sizeDelta.y
        );

        // Convert the anchored position to the Raw Image's local space
        return rectTransform.TransformPoint(anchoredPosition);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radarDistance);

        // Check if sweepTransform is assigned before using it
        if (sweepTransform != null)
        {
            Vector3 leftBoundary = Quaternion.Euler(0, -sweepAngle, 0) * sweepTransform.forward * radarDistance;
            Vector3 rightBoundary = Quaternion.Euler(0, sweepAngle, 0) * sweepTransform.forward * radarDistance;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
            Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + sweepTransform.forward * radarDistance);
        }
    }
}