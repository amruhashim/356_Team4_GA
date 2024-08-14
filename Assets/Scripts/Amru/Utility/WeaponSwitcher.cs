using UnityEngine;
using Cinemachine;

public class WeaponSwitcher : MonoBehaviour
{
    public Transform handHolder;
    public Weapon currentWeapon;
    public GameObject grenadeHandPrefab;
    public GameObject dronePrefab;
    public Transform droneSpawner;
    public CinemachineVirtualCamera playerCamera; // This is the Cinemachine virtual camera
    public CinemachineVirtualCamera droneCamera;
    public Movement playerMovement;

    public Camera mainCamera; // This is the actual Camera component used for shooting

    private GameObject grenadeHandInstance;
    private GameObject droneInstance;
    private bool isGrenadeActive = false;
    private bool isDroneActive = false;

    public GameObject[] objectsToDisable;
    public MonoBehaviour[] scriptsToDisable;

private void Start()
{
    if (mainCamera == null)
    {
        mainCamera = Camera.main; // Automatically assign the main camera in the scene
    }

    playerCamera.Priority = 10;  
    droneCamera.Priority = 0;    

    // Instantiate and set up the grenade hand model
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
                GameObject weaponPrefab = weaponBox.weaponPrefab;
                SwitchWeapon(weaponPrefab);
                weaponBox.StartCooldown();
            }
        }
    }

    private void SwitchWeapon(GameObject weaponPrefab)
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

        // Assign the main camera to the weapon for shooting
        currentWeapon.playerCamera = mainCamera;

        AnimationController animationController = newWeapon.GetComponent<AnimationController>();
        if (animationController != null)
        {
            animationController.movementScript = playerMovement;
        }

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
            // Destroy the drone and reset the camera priorities
            Destroy(droneInstance);
            droneCamera.Priority = 0;
            playerCamera.Priority = 10;
            droneCamera.Follow = null;
            droneCamera.LookAt = null;

            // Re-enable objects and scripts
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
            // Capture the current forward direction of the player's camera
            Vector3 direction = playerCamera.transform.forward;
            Debug.Log("Player Camera Forward: " + direction);

            // Instantiate the drone
            droneInstance = Instantiate(dronePrefab, droneSpawner.position, Quaternion.identity);

            // Set the initial rotation of the drone to match the player's camera direction
            droneInstance.GetComponent<DroneMovement>().SetInitialRotation(direction);
            Debug.Log("Drone Rotation: " + droneInstance.transform.rotation.eulerAngles);

            // Now switch to the drone camera
            droneCamera.Priority = 10;
            playerCamera.Priority = 0;
            droneCamera.Follow = droneInstance.transform; // Set the camera to follow the drone
            droneCamera.LookAt = droneInstance.transform; // Set the camera to look at the drone

            // Disable objects and scripts
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
}
