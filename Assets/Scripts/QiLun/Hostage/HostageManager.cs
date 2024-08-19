using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostageManager : MonoBehaviour
{
    public GameObject hostageArm; // Reference to the Hostage Arm placeholder object
    public Transform player; // Reference to the player or the camera
    public float holdTime = 2.0f; // Time required to hold the button to pick up or drop the hostage

    private bool isHolding = false;
    private float holdTimer = 0f;
    private bool isCarrying = false;
    private Hostage currentHostage;

    void Start()
    {
        // Ensure the hostage arm is initially disabled
        hostageArm.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.E))
        {
            holdTimer += Time.deltaTime;

            if (holdTimer >= holdTime)
            {
                if (!isCarrying)
                {
                    PickUpHostage();
                }
                else
                {
                    DropHostage();
                }

                holdTimer = 0f; // Reset the timer after action
            }
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            holdTimer = 0f; // Reset the timer if the key is released early
        }
    }

public void PickUpHostage()
{
    Collider[] hitColliders = Physics.OverlapSphere(player.position, 1.5f);
    foreach (var hitCollider in hitColliders)
    {
        Hostage hostage = hitCollider.GetComponent<Hostage>();
        if (hostage != null)
        {
            currentHostage = hostage;
            isCarrying = true;
            hostage.gameObject.SetActive(false);      // Disable the Hostage object
            hostageArm.SetActive(true);    // Enable the hostage arm
            Debug.Log($"Hostage picked up and disabled. Hostage ID: {hostage.UniqueID}");
            break;
        }
    }
}

public void DropHostage()
{
    if (currentHostage != null)
    {
        isCarrying = false;
        hostageArm.SetActive(false);   // Disable the hostage arm

        // Re-enable and reposition the hostage object in front of the player
        Vector3 dropPosition = player.position + player.forward * 1.5f; // Drop in front of the player
        currentHostage.transform.position = dropPosition;
        currentHostage.transform.rotation = Quaternion.identity; // Reset rotation if necessary
        currentHostage.gameObject.SetActive(true);      // Re-enable the Hostage object

        Debug.Log($"Hostage dropped and re-enabled. Hostage ID: {currentHostage.UniqueID}");

        currentHostage = null; // Clear the reference after dropping the hostage
    }
}

}