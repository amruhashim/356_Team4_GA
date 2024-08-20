using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class HostageManager : MonoBehaviour
{
    public GameObject hostageArm; // Reference to the Hostage Arm placeholder object
    public Transform player; // Reference to the player or the camera
    public float holdTime = 2.0f; // Time required to hold the button to pick up or drop the hostage
    public Slider holdSlider; // Reference to the UI Slider
    public TextMeshProUGUI outOfRangeMessage; // Reference to the TextMeshPro message

    private float holdTimer = 0f;
    private bool isCarrying = false;
    private Hostage currentHostage;

    void Start()
    {
        // Ensure the hostage arm is initially disabled
        hostageArm.SetActive(false);
        // Ensure the slider and message are initially disabled
        holdSlider.gameObject.SetActive(false);
        outOfRangeMessage.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.E))
        {
            holdTimer += Time.deltaTime;

            // Check if in range or carrying a hostage
            if (isCarrying || IsHostageInRange())
            {
                // Update and show slider
                UpdateHoldSlider(holdTimer);
                holdSlider.gameObject.SetActive(true);
                outOfRangeMessage.gameObject.SetActive(false); // Hide the out-of-range message

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
                    holdSlider.gameObject.SetActive(false); // Hide the slider after the action
                }
            }
            else
            {
                holdSlider.gameObject.SetActive(false);
                outOfRangeMessage.gameObject.SetActive(true); // Show out-of-range message
                outOfRangeMessage.text = "Get closer to the hostage to pick them up!";
            }
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            holdTimer = 0f; // Reset the timer if the key is released early
            holdSlider.gameObject.SetActive(false); // Hide the slider if the key is released early
            outOfRangeMessage.gameObject.SetActive(false); // Hide the out-of-range message if the key is released early
        }
    }

    private bool IsHostageInRange()
    {
        Collider[] hitColliders = Physics.OverlapSphere(player.position, 1.5f);
        foreach (var hitCollider in hitColliders)
        {
            Hostage hostage = hitCollider.GetComponent<Hostage>();
            if (hostage != null)
            {
                currentHostage = hostage;
                return true; // Hostage is in range
            }
        }
        return false; // No hostage in range
    }

    private void UpdateHoldSlider(float time)
    {
        holdSlider.value = Mathf.Clamp(time, 0, holdTime); // Update the slider value based on hold time
    }

    public void PickUpHostage()
    {
        if (currentHostage != null)
        {
            isCarrying = true;
            currentHostage.gameObject.SetActive(false);      // Disable the Hostage object
            hostageArm.SetActive(true);    // Enable the hostage arm
            Debug.Log($"Hostage picked up and disabled. Hostage ID: {currentHostage.UniqueID}");
        }
    }

    public void DropHostage()
    {
        if (currentHostage != null)
        {
            isCarrying = false;
            hostageArm.SetActive(false);

            Vector3 dropPosition = player.position + player.forward * 1.5f;
            currentHostage.transform.position = dropPosition;
            currentHostage.transform.rotation = Quaternion.identity;
            currentHostage.gameObject.SetActive(true);

            // Update the hostage state in PlayerState
            PlayerState.Instance.UpdateHostageState(currentHostage.UniqueID, dropPosition, Quaternion.identity, false);

            Debug.Log($"Hostage dropped and re-enabled. Hostage ID: {currentHostage.UniqueID}");

            currentHostage = null;
        }
    }

    public bool IsCarryingHostage()
    {
        return isCarrying;
    }



    public void RescueHostage()
    {
        if (currentHostage != null)
        {
            // Update the hostage state in PlayerState as rescued
            PlayerState.Instance.UpdateHostageState(currentHostage.UniqueID, currentHostage.transform.position, currentHostage.transform.rotation, true);

            Destroy(currentHostage.gameObject); // Remove the hostage from the game as they are rescued
            currentHostage = null; // Clear the reference
            isCarrying = false;
            hostageArm.SetActive(false);
        }
    }



}
