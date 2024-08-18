using UnityEngine;
using Cinemachine;
using System.Collections.Generic;

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
    public GameObject grenadeHandPrefab;
    public GameObject dronePrefab;
    public Transform droneSpawner;
    public CinemachineVirtualCamera playerCamera;
    public CinemachineVirtualCamera droneCamera;
    public Movement playerMovement;
    public Camera mainCamera;

    private GameObject grenadeHandInstance;
    private GameObject droneInstance;
    private bool isGrenadeActive = false;
    private bool isDroneActive = false;

    public GameObject[] objectsToDisable;
    public MonoBehaviour[] scriptsToDisable;

    [Header("Weapon Configuration")]
    public List<WeaponEntry> weaponEntries;
    public string initialWeaponID;  // Add this field to assign the initial weapon via inspector

    private Dictionary<string, GameObject> weaponDictionary;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        playerCamera.Priority = 10;
        droneCamera.Priority = 0;

        grenadeHandInstance = Instantiate(grenadeHandPrefab, handHolder);
        grenadeHandInstance.transform.localPosition = new Vector3(0, -1.45899999f, -0.479999989f);
        grenadeHandInstance.transform.localRotation = Quaternion.identity;
        grenadeHandInstance.SetActive(false);

        AnimationController grenadeAnimationController = grenadeHandInstance.GetComponent<AnimationController>();
        if (grenadeAnimationController != null)
        {
            grenadeAnimationController.movementScript = playerMovement;
        }

        AmmoManager.Instance.throwForceSlider.gameObject.SetActive(false);

        weaponDictionary = new Dictionary<string, GameObject>();
        foreach (var entry in weaponEntries)
        {
            if (!weaponDictionary.ContainsKey(entry.weaponID))
            {
                weaponDictionary.Add(entry.weaponID, entry.weaponPrefab);
            }
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

    // Ensure AmmoManager references the new weapon
    AmmoManager.Instance.UpdateAmmoDisplay(currentWeapon);
    AmmoManager.Instance.throwForceSlider.gameObject.SetActive(false);
    AmmoManager.Instance.ammoDisplay.gameObject.SetActive(true);
}



    private void SwitchToGrenadeHand()
    {
        if (currentWeapon != null)
        {
            currentWeapon.gameObject.SetActive(false);
        }

        grenadeHandInstance.SetActive(true);
        isGrenadeActive = true;
        AmmoManager.Instance.ammoDisplay.gameObject.SetActive(false);
    }

    private void SwitchToCurrentWeapon()
    {
        if (grenadeHandInstance != null)
        {
            grenadeHandInstance.SetActive(false);
        }

        if (currentWeapon != null)
        {
            currentWeapon.gameObject.SetActive(true);
        }

        isGrenadeActive = false;
        AmmoManager.Instance.ammoDisplay.gameObject.SetActive(true);
    }

    private void ToggleDrone()
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

            isDroneActive = false;
        }
        else
        {
            Vector3 direction = playerCamera.transform.forward;
            droneInstance = Instantiate(dronePrefab, droneSpawner.position, Quaternion.identity);
            droneInstance.GetComponent<DroneMovement>().SetInitialRotation(direction);

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

            isDroneActive = true;
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
}
