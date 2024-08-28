using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SwingDoor : MonoBehaviour
{
    public GameObject swingObject; // Reference to the game object that will swing open
    public GameObject uiElement; // Reference to the UI element to activate/deactivate
    public Image xButtonSlider; // Reference to the Image UI element to use as the slider
    public string doorIdentifier; // Unique identifier for the door
    public Collider doorCollider; // Reference to the door's collider

    private Quaternion originalRotation;
    private Quaternion targetRotation;
    private float swingAngle = -135f; // Angle to swing the door open
    private bool isOpen = false;
    private bool isXKeyPressed = false;
    private float xKeyPressedTime = 0f;
    private float xKeyHoldDuration = 1f; // 1 second hold duration
    private bool isCoroutineRunning = false;

    private MeshRenderer doorMeshRenderer;
    private Collider swingObjectCollider;
    private WeaponSwitcher weaponSwitcher; // Private reference to the WeaponSwitcher component

    private void Start()
    {
        // Get the MeshRenderer and the Collider from the swingObject
        doorMeshRenderer = swingObject.GetComponent<MeshRenderer>();
        swingObjectCollider = swingObject.GetComponent<Collider>();

        // Get the WeaponSwitcher component from the parent or any relevant GameObject
        weaponSwitcher = FindObjectOfType<WeaponSwitcher>();

        // Check if the door was previously open and open it immediately if it was
        if (PlayerState.Instance.IsDoorOpen(doorIdentifier))
        {
            OpenDoorInstantly();
        }

        // Ensure UI is initially set based on the drone's state
        UpdateUIVisibility(false);
    }

    private void Update()
    {
        // Check if the drone is interacting with the door
        if (weaponSwitcher != null && weaponSwitcher.isDroneActive && IsDroneInCollider())
        {
            // Update UI visibility based on the drone's active state
            UpdateUIVisibility(true);

            // Check for X key press and hold
            if (Input.GetKeyDown(KeyCode.X))
            {
                isXKeyPressed = true;
                xKeyPressedTime = 0f;
                StartCoroutine(FillXButtonSlider());
            }
            else if (Input.GetKey(KeyCode.X))
            {
                isXKeyPressed = true;
                xKeyPressedTime += Time.deltaTime;
            }
            else
            {
                isXKeyPressed = false;
                xKeyPressedTime = 0f;
                // Reset the slider value only if the drone is not in the collider
                xButtonSlider.fillAmount = 0f;
            }

            // If the door is open, deactivate the UI element
            if (isOpen)
            {
                uiElement.SetActive(false);
            }
            // If the door is not open and the drone is in the collider, activate the UI element
            else
            {
                uiElement.SetActive(true);
            }

            // If the X key is pressed and held for the required duration, and the coroutine is not running, start the coroutine
            if (isXKeyPressed && xKeyPressedTime >= xKeyHoldDuration && !isCoroutineRunning)
            {
                StartCoroutine(SwingOpen());
                isCoroutineRunning = true;
            }
        }
        else
        {
            UpdateUIVisibility(false);
        }
    }

    private void UpdateUIVisibility(bool shouldShowUI)
    {
        uiElement.SetActive(shouldShowUI);
        xButtonSlider.gameObject.SetActive(shouldShowUI);
    }

    private bool IsDroneInCollider()
    {
        // Check for the drone's presence using the tag
        Collider[] colliders = Physics.OverlapBox(doorCollider.bounds.center, doorCollider.bounds.extents, doorCollider.transform.rotation);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Drone"))
            {
                return true;
            }
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Drone"))
        {
            Debug.Log("Drone entered the collider"); // Verify the collider interaction
            xButtonSlider.gameObject.SetActive(true); // Activate the slider when the drone enters the collider
            xButtonSlider.fillAmount = 0f; // Reset the slider value when the drone enters the collider
            UpdateUIVisibility(true); // Ensure UI visibility is updated
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Drone"))
        {
            Debug.Log("Drone exited the collider"); // Verify the collider interaction
            xButtonSlider.gameObject.SetActive(false); // Deactivate the slider when the drone exits the collider
            uiElement.SetActive(false); // Deactivate the UI element when the drone exits the collider
        }
    }

    private IEnumerator SwingOpen()
    {
        // Store the original rotation of the swingObject
        originalRotation = swingObject.transform.rotation;

        // Calculate the target rotation by applying the swing angle to the original rotation
        targetRotation = originalRotation * Quaternion.Euler(0f, swingAngle, 0f);

        float elapsedTime = 0f;
        float duration = 1.5f; // 1.5 second duration

        while (elapsedTime < duration)
        {
            // Lerp the rotation from the original to the target over the duration
            Quaternion newRotation = Quaternion.Lerp(originalRotation, targetRotation, elapsedTime / duration);

            // Apply the new rotation to both the mesh renderer and collider
            if (doorMeshRenderer != null)
            {
                doorMeshRenderer.transform.rotation = newRotation;
            }

            if (swingObjectCollider != null)
            {
                swingObjectCollider.transform.rotation = newRotation;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the swingObject ends up at the target rotation
        swingObject.transform.rotation = targetRotation;
        isOpen = true;
        isCoroutineRunning = false;

        // Disable the collider to prevent further interaction
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }

        // Save the state in PlayerState
        PlayerState.Instance.SetDoorState(doorIdentifier, true);
    }

    private IEnumerator FillXButtonSlider()
    {
        float elapsedTime = 0f;
        float duration = 1f; // 1 second duration

        while (elapsedTime < duration)
        {
            // Lerp the slider fill amount from 0 to 1 over the duration
            xButtonSlider.fillAmount = Mathf.Lerp(0f, 1f, elapsedTime / duration);

            // Check if the X key is released before the slider fills up
            if (!Input.GetKey(KeyCode.X))
            {
                // Reset the slider to the current value
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the slider ends up at the maximum fill amount
        xButtonSlider.fillAmount = 1f;
    }

    private void OpenDoorInstantly()
    {
        // Directly set the door to the open position
        originalRotation = swingObject.transform.rotation;
        Quaternion finalRotation = originalRotation * Quaternion.Euler(0f, swingAngle, 0f);

        swingObject.transform.rotation = finalRotation;
        isOpen = true;

        // Disable the collider to prevent further interaction
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }

        // Hide UI since the door is already open
        uiElement.SetActive(false);
    }
}
