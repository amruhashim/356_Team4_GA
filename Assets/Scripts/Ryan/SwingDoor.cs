using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SwingDoor : MonoBehaviour
{
    public GameObject swingObject; // Reference to the game object that will swing open
    public GameObject uiElement; // Reference to the UI element to activate/deactivate
    private Quaternion originalRotation;
    private Quaternion targetRotation;
    private float swingAngle = -135f; // Angle to swing the door open
    private bool isOpen = false;
    private bool isXKeyPressed = false;
    private float xKeyPressedTime = 0f;
    private float xKeyHoldDuration = 1f; // 1 second hold duration
    private bool isCoroutineRunning = false;
    private bool isPlayerInCollider = false;

    private void Update()
    {
        // Check for X key press and hold
        if (Input.GetKeyDown(KeyCode.X))
        {
            isXKeyPressed = true;
            xKeyPressedTime = 0f;
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
        }

        // If the door is open, deactivate the UI element
        if (isOpen)
        {
            uiElement.SetActive(false);
        }
        // If the door is not open and the player is in the collider, activate the UI element
        else if (isPlayerInCollider)
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Drone"))
        {
            isPlayerInCollider = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Drone"))
        {
            isPlayerInCollider = false;
        }
    }

    private IEnumerator SwingOpen()
    {
        // Store the original rotation of the swingObject
        originalRotation = swingObject.transform.rotation;

        // Calculate the target rotation by applying the swing angle to the original rotation
        targetRotation = originalRotation * Quaternion.Euler(0f, 0f, swingAngle);

        float elapsedTime = 0f;
        float duration = 1.5f; // 1.5 second duration

        while (elapsedTime < duration)
        {
            // Lerp the rotation from the original to the target over the duration
            swingObject.transform.rotation = Quaternion.Lerp(originalRotation, targetRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the swingObject ends up at the target rotation
        swingObject.transform.rotation = targetRotation;
        isOpen = true;
        isCoroutineRunning = false;
    }
}