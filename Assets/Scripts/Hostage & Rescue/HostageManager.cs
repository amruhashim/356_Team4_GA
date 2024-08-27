using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AI;

public class HostageManager : MonoBehaviour
{
    public GameObject hostageArm; // Reference to the Hostage Arm placeholder object
    public Transform player; // Reference to the player or the camera
    public float holdTime = 2.0f; // Time required to hold the button to pick up or drop the hostage
    public Slider holdSlider; // Reference to the UI Slider
    public TextMeshProUGUI warningMessage; // Reference to the TextMeshPro warning message
    public TextMeshProUGUI carryingMessage; // Reference to the TextMeshPro for the carrying message
    public TextMeshProUGUI hostageStatusText; // Reference to the TextMeshPro for showing hostage status
    public float yOffset = 0.5f; // Adjustable Y-axis offset for the hostage position

    private float holdTimer = 0f;
    private bool isCarrying = false;
    private Hostage currentHostage;

    private int totalHostages;
    private int rescuedHostages;

    void Start()
    {
        // Ensure the hostage arm is initially disabled
        hostageArm.SetActive(false);
        // Ensure the slider and messages are initially disabled
        holdSlider.gameObject.SetActive(false);
        warningMessage.gameObject.SetActive(false);
        carryingMessage.gameObject.SetActive(false); // Hide the carrying message initially

        // Initialize hostage counts
        totalHostages = FindObjectsOfType<Hostage>().Length;
        rescuedHostages = PlayerState.Instance.GetRescuedHostagesCount(); // Retrieve the number of rescued hostages from PlayerState

        // Update the hostage status text initially
        UpdateHostageStatusText();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.E))
        {
            // Prevent interaction if the player is moving
            if (Movement.isMoving)
            {
                warningMessage.gameObject.SetActive(true);
                warningMessage.text = "Cannot pick up or drop hostages while moving!";
                holdSlider.gameObject.SetActive(false); // Ensure slider is hidden
                return; // Exit early to prevent further actions
            }

            // Check if in range or carrying a hostage
            if (isCarrying || IsHostageInRange())
            {
                holdTimer += Time.deltaTime;

                // Update and show slider
                UpdateHoldSlider(holdTimer);
                holdSlider.gameObject.SetActive(true);
                warningMessage.gameObject.SetActive(false); // Hide the warning message

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
                if (holdTimer == 0f) // Only show the out-of-range message if holdTimer hasn't started
                {
                    warningMessage.gameObject.SetActive(true); // Show out-of-range message
                    warningMessage.text = "Get closer to the hostage to pick them up!";
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            holdTimer = 0f; // Reset the timer if the key is released early
            holdSlider.gameObject.SetActive(false); // Hide the slider if the key is released early
            warningMessage.gameObject.SetActive(false); // Hide the warning message if the key is released early
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

            // Show the carrying message
            carryingMessage.gameObject.SetActive(true);
            carryingMessage.text = "You are carrying a hostage. Carry them to the rescue helicopter.";

            Debug.Log($"Hostage picked up and disabled. Hostage ID: {currentHostage.UniqueID}");
        }
    }

    public void DropHostage()
    {
        if (currentHostage != null)
        {
            isCarrying = false;
            hostageArm.SetActive(false);
            carryingMessage.gameObject.SetActive(false); // Hide the carrying message when dropping the hostage

            // Get the main camera
            Camera mainCamera = Camera.main;

            // Calculate a point in front of the player
            Vector3 forwardPoint = player.position + player.forward * 1.5f;
            Vector3 dropPosition = forwardPoint;

            // Perform a raycast downward from a fixed height above the forward point
            Ray ray = new Ray(forwardPoint + Vector3.up * 10f, Vector3.down);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
            {
                dropPosition = hit.point; // Use the hit point as the initial drop position

                // Ensure the drop position is on the NavMesh
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(dropPosition, out navHit, 1.0f, NavMesh.AllAreas))
                {
                    dropPosition = navHit.position;
                }
                else
                {
                    // Perform a secondary raycast to fine-tune the position on the terrain
                    if (Physics.Raycast(dropPosition + Vector3.up * 5f, Vector3.down, out hit, 10f))
                    {
                        dropPosition = hit.point; // Adjust to the terrain surface
                    }
                }
            }

            // Apply the Y-axis offset
            dropPosition.y += yOffset;

            // Set the hostage's position
            currentHostage.transform.position = dropPosition;

            // Make the hostage face the player
            Vector3 directionToFace = (player.position - dropPosition).normalized;
            directionToFace.y = 0; // Keep the rotation on the Y axis only
            currentHostage.transform.rotation = Quaternion.LookRotation(directionToFace);

            currentHostage.gameObject.SetActive(true);

            // Update the hostage state in PlayerState
            PlayerState.Instance.UpdateHostageState(currentHostage.UniqueID, dropPosition, currentHostage.transform.rotation, false);

            Debug.Log($"Hostage dropped, re-enabled, and facing the player. Hostage ID: {currentHostage.UniqueID}");

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
            carryingMessage.gameObject.SetActive(false); // Hide the carrying message upon rescue

            // Update rescued hostage count and UI
            rescuedHostages++;
            UpdateHostageStatusText();
        }
    }

    private void UpdateHostageStatusText()
    {
        if (hostageStatusText != null)
        {
            int remainingHostages = totalHostages - rescuedHostages;
            hostageStatusText.text = $"Rescued: {rescuedHostages}\nRemaining: {remainingHostages}";
        }
    }
}
