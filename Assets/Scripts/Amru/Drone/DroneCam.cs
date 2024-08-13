using UnityEngine;
using System.Collections;

public class DroneCam : MonoBehaviour
{
     private Transform ourDrone;

    void Awake()
    {
        ourDrone = GameObject.FindGameObjectWithTag("Drone").transform;
    }

    private Vector3 velocityCameraFollow;
    public Vector3 behindPosition = new Vector3(0, 2, -4);
    public float angle;

    void FixedUpdate()
    {
        // Follow the drone's position but maintain a consistent height
        Vector3 targetPosition = ourDrone.transform.TransformPoint(behindPosition);
        targetPosition.y = Mathf.SmoothDamp(transform.position.y, targetPosition.y, ref velocityCameraFollow.y, 0.1f);

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocityCameraFollow, 0.1f);
        transform.rotation = Quaternion.Euler(new Vector3(angle, ourDrone.GetComponent<DroneMovement>().currentYRotation, 0));
    }
}
