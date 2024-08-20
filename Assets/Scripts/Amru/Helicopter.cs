using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helicopter : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that collided has the Player tag
        if (other.CompareTag("Player"))
        {
            // Get the HostageManager component from the player
            HostageManager hostageManager = other.GetComponent<HostageManager>();

            if (hostageManager != null && hostageManager.IsCarryingHostage())
            {
                // If the player is carrying a hostage, rescue the hostage
                hostageManager.RescueHostage();
                Debug.Log("Hostage successfully rescued by helicopter.");
            }
        }
    }
}
