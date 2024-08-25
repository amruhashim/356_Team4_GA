using UnityEngine;
using Cinemachine;
using System.Collections.Generic;
using UnityEngine.AI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class WeaponEntry
{
    public string weaponID;
    public GameObject weaponPrefab;
}

public class WeaponSwitcher : MonoBehaviour
{
    public Transform handHolder;
    public Weapon currentWeapon;
    public GameObject dronePrefab;
    public Transform droneSpawner;
    public CinemachineVirtualCamera playerCamera;
    public CinemachineVirtualCamera droneCamera;
    public Movement playerMovement;
    public Camera mainCamera;
 public TextMeshProUGUI outOfRangeText;

   [Header("Spotlight Settings")]
    public float outOfRangeIntensity = 10f; // Intensity when out of range
    private float defaultIntensity = 0f;
  public Light warningSpotlight;


    private GameObject droneInstance;
    private bool isGrenadeActive = false;
    private bool isDroneActive = false;
    private GameObject grenadeHandInstance;

    public GameObject[] objectsToDisable;
    public MonoBehaviour[] scriptsToDisable;

    [Header("Weapon Configuration")]
    public List<WeaponEntry> weaponEntries;
    public string initialWeaponID;
    public string grenadeWeaponID; // ID for the grenade hand

    private Dictionary<string, GameObject> weaponDictionary;
    private CharacterController characterController;
    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        characterController = GetComponent<CharacterController>();
        playerCamera.Priority = 10;
        droneCamera.Priority = 0;
        outOfRangeText.gameObject.SetActive(false);
        weaponDictionary = new Dictionary<string, GameObject>();
        foreach (var entry in weaponEntries)
        {
            if (!weaponDictionary.ContainsKey(entry.weaponID))
            {
                weaponDictionary.Add(entry.weaponID, entry.weaponPrefab);
            }
        }

        // Add grenade hand to the dictionary
        if (!weaponDictionary.ContainsKey(grenadeWeaponID))
        {
            weaponDictionary.Add(grenadeWeaponID, weaponDictionary[grenadeWeaponID]);
        }

        // Load the saved weapon if present, otherwise use the initial weapon
        if (!string.IsNullOrEmpty(PlayerState.Instance.activeWeaponID))
        {
            SwitchWeaponByID(PlayerState.Instance.activeWeaponID, true);  // Load with saved data
        }
        else if (!string.IsNullOrEmpty(initialWeaponID))
        {
            SwitchWeaponByID(initialWeaponID, false);  // Load initial weapon
        }
    }

    private void Update()
    {
        HandleInput();
        if (isDroneActive)
        {
            CheckPlayerDistance();
        }
    }

    public float timeOutOfRange = 0f; // Current time out of range
    public float maxTimeOutOfRange = 2f; // 2 seconds out of range allowed
    public float maxDistance = 200f; // Maximum allowed distance between drone and player

 private void CheckPlayerDistance()
    {
        Vector3 playerPosition = characterController.transform.position;
        Vector3 dronePosition = droneInstance.transform.position;

        float distanceToPlayer = Vector3.SqrMagnitude(dronePosition - playerPosition);
        float maxDistanceSquared = maxDistance * maxDistance;

        if (distanceToPlayer > maxDistanceSquared)
        {
            timeOutOfRange += Time.deltaTime;

            // Show the out-of-range text with the countdown timer
            outOfRangeText.gameObject.SetActive(true);
            float timeRemaining = maxTimeOutOfRange - timeOutOfRange;
            outOfRangeText.text = $"Losing Range! Return to Range in {timeRemaining:F1} seconds!";

            // Set spotlight intensity to the out-of-range value
            warningSpotlight.intensity = outOfRangeIntensity;

            if (timeOutOfRange >= maxTimeOutOfRange)
            {
                Debug.Log("Player out of range for too long. Toggling drone off.");
                ToggleDrone();
                outOfRangeText.gameObject.SetActive(false);
                warningSpotlight.intensity = defaultIntensity; // Reset intensity when drone toggled off
            }
        }
        else
        {
            timeOutOfRange = 0f;
            outOfRangeText.gameObject.SetActive(false); // Hide text when within range
            warningSpotlight.intensity = defaultIntensity; // Set spotlight intensity to 0 when in range
        }
    }



    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.G) && !isGrenadeActive)
        {
            SwitchToGrenadeHand();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            if (isGrenadeActive)
            {
                SwitchToCurrentWeapon();
            }
            else
            {
                SwitchToGrenadeHand();
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (isGrenadeActive)
            {
                SwitchToCurrentWeapon();
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleDrone();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("WeaponBox"))
        {
            WeaponBox weaponBox = other.gameObject.GetComponent<WeaponBox>();

            if (!weaponBox.isOnCooldown)
            {
                string weaponID = weaponBox.weaponID;
                SwitchWeaponByID(weaponID);
                weaponBox.StartCooldown();
            }
        }
    }

    private void SwitchWeapon(GameObject weaponPrefab, bool isLoadingGame = false)
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon.gameObject);
        }

        if (isGrenadeActive)
        {
            grenadeHandInstance.SetActive(false);
            isGrenadeActive = false;
        }

        GameObject newWeapon = Instantiate(weaponPrefab, handHolder);
        newWeapon.transform.localPosition = new Vector3(0, -1.45899999f, -0.479999989f);
        newWeapon.transform.localRotation = Quaternion.identity;

        currentWeapon = newWeapon.GetComponent<Weapon>();
        currentWeapon.playerCamera = mainCamera;

        AnimationController animationController = newWeapon.GetComponent<AnimationController>();
        if (animationController != null)
        {
            animationController.movementScript = playerMovement;
        }

        if (isLoadingGame)
        {
            currentWeapon.LoadBulletsFromPlayerState();
        }
        else
        {
            currentWeapon.ResetBullets();
            PlayerState.Instance.activeWeaponID = currentWeapon.weaponID;
        }

        AmmoManager.Instance.UpdateAmmoDisplay(currentWeapon);
        AmmoManager.Instance.ammoDisplay.gameObject.SetActive(true);
        AmmoManager.Instance.grenadeDisplay.gameObject.SetActive(false);
    }

    private void SwitchToGrenadeHand()
    {
        // Destroy the existing grenade hand instance if it exists
        if (grenadeHandInstance != null)
        {
            Destroy(grenadeHandInstance);
        }

        // Create a new grenade hand instance
        grenadeHandInstance = Instantiate(weaponDictionary[grenadeWeaponID], handHolder);
        grenadeHandInstance.transform.localPosition = new Vector3(0, -1.45899999f, -0.479999989f);
        grenadeHandInstance.transform.localRotation = Quaternion.identity;

        // Assign the movement script to the grenade hand's animation controller
        AnimationController grenadeAnimationController = grenadeHandInstance.GetComponent<AnimationController>();
        if (grenadeAnimationController != null)
        {
            grenadeAnimationController.movementScript = playerMovement;
        }

        // Deactivate the current weapon if it exists
        if (currentWeapon != null)
        {
            currentWeapon.gameObject.SetActive(false);
        }

        // Activate the grenade hand and update the UI
        grenadeHandInstance.SetActive(true);
        isGrenadeActive = true;

        AmmoManager.Instance.ammoDisplay.gameObject.SetActive(false);
        AmmoManager.Instance.grenadeDisplay.gameObject.SetActive(true);
        AmmoManager.Instance.UpdateGrenadeDisplay(PlayerState.Instance.grenadeCount);
    }


    private void SwitchToCurrentWeapon()
    {
        if (grenadeHandInstance != null)
        {
            grenadeHandInstance.SetActive(false);
        }

        isGrenadeActive = false;

        if (!string.IsNullOrEmpty(PlayerState.Instance.activeWeaponID))
        {
            SwitchWeaponByID(PlayerState.Instance.activeWeaponID);
        }

        AmmoManager.Instance.ammoDisplay.gameObject.SetActive(true);
        AmmoManager.Instance.grenadeDisplay.gameObject.SetActive(false);
        AmmoManager.Instance.UpdateGrenadeDisplay(PlayerState.Instance.grenadeCount);
    }
   private void ToggleDrone()
{
    if (isDroneActive)
    {
        // Destroy the drone and reset cameras
        Destroy(droneInstance);
        droneCamera.Priority = 0;
        playerCamera.Priority = 10;
        droneCamera.Follow = null;
        droneCamera.LookAt = null;

        // Enable objects and scripts that were disabled
        foreach (GameObject obj in objectsToDisable)
        {
            obj.SetActive(true);
        }

        foreach (MonoBehaviour script in scriptsToDisable)
        {
            script.enabled = true;
        }

        // Reset player movement
        playerMovement.freezePosition = false;
        isDroneActive = false;

        // Reset the warning text and spotlight
        outOfRangeText.gameObject.SetActive(false);
        warningSpotlight.intensity = defaultIntensity;
        timeOutOfRange = 0f; // Reset the out-of-range timer
    }
    else
    {
        if (droneInstance == null)
        {
            Vector3 spawnPosition = droneSpawner.position;
            Ray ray = new Ray(spawnPosition, Vector3.down);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 10f, LayerMask.GetMask("Ground")))
            {
                spawnPosition = hit.point + Vector3.up * 1.0f;
                droneInstance = Instantiate(dronePrefab, spawnPosition, Quaternion.identity);
                droneInstance.GetComponent<DroneMovement>().SetInitialRotation(playerCamera.transform.forward);

                // Apply sensitivity to the spawned drone
                float droneSensitivity = SaveManager.Instance.LoadSensitivitySettings().droneSensitivity;
                DroneMovement droneMovement = droneInstance.GetComponent<DroneMovement>();
                if (droneMovement != null)
                {
                    droneMovement.SetSensitivity(droneSensitivity);
                }

                // Switch to the drone camera
                droneCamera.Priority = 10;
                playerCamera.Priority = 0;
                droneCamera.Follow = droneInstance.transform;
                droneCamera.LookAt = droneInstance.transform;

                // Disable objects and scripts related to the player
                foreach (GameObject obj in objectsToDisable)
                {
                    obj.SetActive(false);
                }

                foreach (MonoBehaviour script in scriptsToDisable)
                {
                    script.enabled = false;
                }

                // Freeze player movement
                playerMovement.freezePosition = true;
                isDroneActive = true;
            }
            else
            {
                Debug.LogWarning("No suitable surface found for drone spawn.");
            }
        }
    }
}






    public void SwitchWeaponByID(string weaponID, bool isLoadingGame = false)
    {
        if (weaponDictionary.TryGetValue(weaponID, out GameObject weaponPrefab))
        {
            SwitchWeapon(weaponPrefab, isLoadingGame);
        }
        else
        {
            Debug.LogWarning("Weapon ID not found in dictionary: " + weaponID);
        }
    }



#if UNITY_EDITOR
private void OnDrawGizmos()
{
    if (characterController != null)
    {
        Vector3 playerPosition = characterController.transform.position;

        // Draw the max distance range as a wireframe sphere
        Gizmos.color = new Color(0f, 1f, 0f, 0.5f); // Semi-transparent green
        Gizmos.DrawWireSphere(playerPosition, maxDistance);

        // Mark the player's position
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(playerPosition, 0.3f); // Draw a small sphere at the player's position
    }

    if (droneSpawner != null)
    {
        // Draw a line from the drone spawner to the ground
        Vector3 spawnPosition = droneSpawner.position;
        Ray ray = new Ray(spawnPosition, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 10f, LayerMask.GetMask("Ground")))
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(spawnPosition, hit.point);
        }
    }
}
#endif
}