using Cinemachine;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class WeaponEntry
{
    public string weaponID;
    public GameObject weaponPrefab;
    public Sprite weaponIcon;
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
    private GameObject meleeWeaponInstance;

    public GameObject[] objectsToDisable;
    public MonoBehaviour[] scriptsToDisable;

    [Header("Weapon Configuration")]
    public List<WeaponEntry> weaponEntries;
    public string initialWeaponID;
    public string meleeWeaponID;
    public string grenadeWeaponID;

    private Dictionary<string, WeaponEntry> weaponDictionary;
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

        weaponDictionary = new Dictionary<string, WeaponEntry>();
        foreach (var entry in weaponEntries)
        {
            if (!weaponDictionary.ContainsKey(entry.weaponID))
            {
                weaponDictionary.Add(entry.weaponID, entry);
            }
        }

        // Ensure melee and grenade icons are passed to AmmoManager at the start
        if (weaponDictionary.TryGetValue(meleeWeaponID, out WeaponEntry meleeWeaponEntry))
        {
            AmmoManager.Instance.HighlightMeleeWeaponIcon(meleeWeaponEntry.weaponIcon);
        }

        if (weaponDictionary.TryGetValue(grenadeWeaponID, out WeaponEntry grenadeWeaponEntry))
        {
            AmmoManager.Instance.HighlightGrenadeIcon(grenadeWeaponEntry.weaponIcon);
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
        // Switch to grenade hand if 'G' is pressed and grenade hand is not active
        if (Input.GetKeyDown(KeyCode.G))
        {
            // Always switch to grenade hand, even if melee weapon is active
            SwitchToGrenadeHand();
        }
        // Handle mouse scroll to switch between grenade hand, melee weapon, and current weapon
        else if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            CycleWeapons();
        }

        // Switch back to the current weapon if '1' is pressed and grenade hand or melee weapon is active
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (isGrenadeActive || meleeWeaponInstance != null)
            {
                SwitchToCurrentWeapon();
            }
        }

        // Switch to melee weapon if '2' is pressed
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchToMeleeWeapon();
        }

        // Toggle the drone on/off when 'Q' is pressed
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleDrone();
        }
    }


    private void CycleWeapons()
    {
        if (isGrenadeActive)
        {
            SwitchToMeleeWeapon();
        }
        else if (meleeWeaponInstance != null)
        {
            SwitchToCurrentWeapon();
        }
        else
        {
            SwitchToGrenadeHand();
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
        // Toggle off the drone if it's active
        if (isDroneActive)
        {
            ToggleDrone();
        }

        if (currentWeapon != null)
        {
            Destroy(currentWeapon.gameObject);
        }

        if (isGrenadeActive)
        {
            grenadeHandInstance.SetActive(false);
            isGrenadeActive = false;
        }

        if (meleeWeaponInstance != null)
        {
            Destroy(meleeWeaponInstance);
            meleeWeaponInstance = null;
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

    private void SwitchToMeleeWeapon()
    {
        // Toggle off the drone if it's active
        if (isDroneActive)
        {
            ToggleDrone();
        }

        if (meleeWeaponInstance != null) return;

        if (isGrenadeActive)
        {
            grenadeHandInstance.SetActive(false);
            isGrenadeActive = false;
        }

        if (currentWeapon != null)
        {
            Destroy(currentWeapon.gameObject);
        }

        if (weaponDictionary.TryGetValue(meleeWeaponID, out WeaponEntry meleeWeaponEntry))
        {
            meleeWeaponInstance = Instantiate(meleeWeaponEntry.weaponPrefab, handHolder);
            meleeWeaponInstance.transform.localPosition = new Vector3(0, -1.45899999f, -0.479999989f);
            meleeWeaponInstance.transform.localRotation = Quaternion.identity;

            AnimationController meleeAnimationController = meleeWeaponInstance.GetComponent<AnimationController>();
            if (meleeAnimationController != null)
            {
                meleeAnimationController.movementScript = playerMovement;
            }

            // Highlight the melee weapon icon using its sprite
            AmmoManager.Instance.HighlightMeleeWeaponIcon(meleeWeaponEntry.weaponIcon);
            AmmoManager.Instance.ammoDisplay.gameObject.SetActive(false);
            AmmoManager.Instance.grenadeDisplay.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Melee weapon ID not found in dictionary: " + meleeWeaponID);
        }
    }

    private void SwitchToGrenadeHand()
    {
        // Toggle off the drone if it's active
        if (isDroneActive)
        {
            ToggleDrone();
        }

        if (grenadeHandInstance != null) return;

        if (meleeWeaponInstance != null)
        {
            Destroy(meleeWeaponInstance);
            meleeWeaponInstance = null;
        }

        if (currentWeapon != null)
        {
            currentWeapon.gameObject.SetActive(false);
        }

        if (weaponDictionary.TryGetValue(grenadeWeaponID, out WeaponEntry grenadeWeaponEntry))
        {
            grenadeHandInstance = Instantiate(grenadeWeaponEntry.weaponPrefab, handHolder);
            grenadeHandInstance.transform.localPosition = new Vector3(0, -1.45899999f, -0.479999989f);
            grenadeHandInstance.transform.localRotation = Quaternion.identity;

            AnimationController grenadeAnimationController = grenadeHandInstance.GetComponent<AnimationController>();
            if (grenadeAnimationController != null)
            {
                grenadeAnimationController.movementScript = playerMovement;
            }

            grenadeHandInstance.SetActive(true);
            isGrenadeActive = true;

            // Highlight the grenade icon using its sprite
            AmmoManager.Instance.HighlightGrenadeIcon(grenadeWeaponEntry.weaponIcon);
            AmmoManager.Instance.UpdateGrenadeDisplay(PlayerState.Instance.grenadeCount);
            AmmoManager.Instance.ammoDisplay.gameObject.SetActive(false);
            AmmoManager.Instance.grenadeDisplay.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Grenade weapon ID not found in dictionary: " + grenadeWeaponID);
        }
    }



    private void SwitchToCurrentWeapon()
    {
        if (grenadeHandInstance != null)
        {
            Destroy(grenadeHandInstance);
            grenadeHandInstance = null;
        }

        if (meleeWeaponInstance != null)
        {
            Destroy(meleeWeaponInstance);
            meleeWeaponInstance = null;
        }

        isGrenadeActive = false;

        if (!string.IsNullOrEmpty(PlayerState.Instance.activeWeaponID))
        {
            SwitchWeaponByID(PlayerState.Instance.activeWeaponID);
        }

        AmmoManager.Instance.ammoDisplay.gameObject.SetActive(true);
        AmmoManager.Instance.grenadeDisplay.gameObject.SetActive(false);
    }

    public void ToggleDrone()
    {
        if (isDroneActive)
        {
            Destroy(droneInstance);
            droneCamera.Priority = 0;
            playerCamera.Priority = 10;
            droneCamera.Follow = null;
            droneCamera.LookAt = null;

            foreach (GameObject obj in objectsToDisable)
            {
                obj.SetActive(true);
            }

            foreach (MonoBehaviour script in scriptsToDisable)
            {
                script.enabled = true;
            }

            playerMovement.freezePosition = false;
            isDroneActive = false;

            outOfRangeText.gameObject.SetActive(false);
            warningSpotlight.intensity = defaultIntensity;
            timeOutOfRange = 0f;

            if (currentWeapon != null)
            {
                AmmoManager.Instance.HighlightActiveWeaponIcon(weaponDictionary[currentWeapon.weaponID].weaponIcon);
            }
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

                    float droneSensitivity = SaveManager.Instance.LoadSensitivitySettings().droneSensitivity;
                    DroneMovement droneMovement = droneInstance.GetComponent<DroneMovement>();
                    if (droneMovement != null)
                    {
                        droneMovement.SetSensitivity(droneSensitivity);
                    }

                    droneCamera.Priority = 10;
                    playerCamera.Priority = 0;
                    droneCamera.Follow = droneInstance.transform;
                    droneCamera.LookAt = droneInstance.transform;

                    foreach (GameObject obj in objectsToDisable)
                    {
                        obj.SetActive(false);
                    }

                    foreach (MonoBehaviour script in scriptsToDisable)
                    {
                        script.enabled = false;
                    }

                    playerMovement.freezePosition = true;
                    isDroneActive = true;

                    AmmoManager.Instance.HighlightDroneIcon();
                }
            }
        }
    }

    public void SwitchWeaponByID(string weaponID, bool isLoadingGame = false)
    {
        if (weaponDictionary.TryGetValue(weaponID, out WeaponEntry weaponEntry))
        {
            SwitchWeapon(weaponEntry.weaponPrefab, isLoadingGame);
            AmmoManager.Instance.HighlightActiveWeaponIcon(weaponEntry.weaponIcon);
        }
    }
}
