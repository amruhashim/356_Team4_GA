using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WeaponBox : MonoBehaviour
{
    #region Serialized Fields
    [Tooltip("The weapon prefab that will be instantiated.")]
    public GameObject weaponPrefab;

    [Tooltip("The spotlight indicating cooldown state.")]
    public Light spotlight;

    [Tooltip("Duration of the cooldown period in seconds.")]
    public float cooldownDuration = 15f;

    [Tooltip("Text element to display the cooldown time.")]
    public TextMeshProUGUI cooldownText;

    [Tooltip("Canvas to display the cooldown information.")]
    public Canvas cooldownCanvas;
    #endregion

    #region Private Fields
    private float cooldownTimer = 0f;
    private Camera mainCamera;
    #endregion

    public bool isOnCooldown = false;

    #region Unity Methods
    private void Start()
    {
        // Initialize main camera reference and spotlight color
        mainCamera = Camera.main;
        spotlight.color = Color.green; 
    }

    private void Update()
    {
        // Handle cooldown logic
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            cooldownText.text = $"Cooldown: {Mathf.Ceil(cooldownTimer)}s";
            if (cooldownTimer <= 0)
            {
                EndCooldown();
            }
        }

        // Update the cooldown canvas orientation
        UpdateCooldownCanvas();
    }
    #endregion

    #region Cooldown Methods
    public void StartCooldown()
    {
        // Begin the cooldown process
        isOnCooldown = true;
        cooldownTimer = cooldownDuration;
        spotlight.color = Color.red;
    }

    private void EndCooldown()
    {
        // End the cooldown process
        isOnCooldown = false;
        cooldownText.text = "";
        spotlight.color = Color.green;
    }
    #endregion

    #region Utility Methods
    private void UpdateCooldownCanvas()
    {
        // Rotate the cooldown canvas to face the camera
        Vector3 directionToCamera = mainCamera.transform.position - cooldownCanvas.transform.position;
        directionToCamera.x = directionToCamera.z = 0; 
        cooldownCanvas.transform.LookAt(mainCamera.transform.position - directionToCamera);
        cooldownCanvas.transform.Rotate(0, 180, 0);
    }
    #endregion
}
