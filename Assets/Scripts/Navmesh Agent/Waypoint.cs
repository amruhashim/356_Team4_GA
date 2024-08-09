using UnityEngine;
using System.Collections;

public class Waypoint : MonoBehaviour
{
    private Collider waypointCollider;

    void Start()
    {
        waypointCollider = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PatrolAgent"))
        {
            PatrolAgent patrolAgent = other.GetComponent<PatrolAgent>();
            if (patrolAgent != null)
            {
                Debug.Log("Waypoint triggered by: " + patrolAgent.name);
                patrolAgent.ReachedWaypoint(other.transform.position);
                StartCoroutine(DisableColliderTemporarily(patrolAgent.waitTimeMax * 2));
            }
        }
    }

    private IEnumerator DisableColliderTemporarily(float disableTime)
    {
        waypointCollider.enabled = false;
        yield return new WaitForSeconds(disableTime);
        waypointCollider.enabled = true;
    }
}
