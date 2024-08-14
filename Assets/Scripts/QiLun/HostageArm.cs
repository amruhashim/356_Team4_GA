using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostageArm : MonoBehaviour
{
    public Transform playerCamera;  // The player's camera
    public Vector3 armPosition;     // Position relative to the camera
    public Vector3 armRotation;     // Rotation relative to the camera

    void Start()
    {
        // Position the hostage arm relative to the camera
        transform.SetParent(playerCamera, false);
        transform.localPosition = armPosition;
        transform.localRotation = Quaternion.Euler(armRotation);
    }
}
