using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helicopter : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Debug line to indicate a collision with the helicopter
        Debug.Log("Helicopter: Collision detected.");

        // Check if the object that collided has the Player tag
        if (other.CompareTag("Player"))
        {
            Debug.Log("Helicopter: Player has entered the trigger zone.");

            // Search for the HostageManager component in the player or its children
            HostageManager hostageManager = other.GetComponentInChildren<HostageManager>();

            if (hostageManager != null)
            {
                Debug.Log("Helicopter: HostageManager component found on the player or its children.");

                if (hostageManager.IsCarryingHostage())
                {
                    Debug.Log("Helicopter: Player is carrying a hostage.");

                    // If the player is carrying a hostage, rescue the hostage
                    hostageManager.RescueHostage();
                    Debug.Log("Helicopter: Hostage successfully rescued by helicopter.");
                }
                else
                {
                    Debug.Log("Helicopter: Player is not carrying a hostage.");
                }
            }
            else
            {
                Debug.Log("Helicopter: HostageManager component not found on the player or its children.");
            }
        }
        else
        {
            Debug.Log("Helicopter: The object that entered the trigger zone is not the player.");
        }
    }
}
