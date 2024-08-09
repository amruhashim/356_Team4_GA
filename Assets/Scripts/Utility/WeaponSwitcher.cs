using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    public Transform handHolder; 
    public Weapon currentWeapon; 
    public GameObject grenadeHandPrefab; // The grenade hand model prefab
    public Camera playerCamera; // Reference to the player's camera
    public Movement playerMovement; // Reference to the player's movement script

    private GameObject grenadeHandInstance; // Instance of the grenade hand model
    private bool isGrenadeActive = false; // Whether the grenade hand is active

    #region Initialization
    private void Start()
    {
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
    #endregion

    #region Update
    private void Update()
    {
        HandleInput();
    }
    #endregion

    #region Input Handling
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
    }
    #endregion

    #region Trigger Handling
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
    #endregion

    #region Weapon Switching
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
        currentWeapon.playerCamera = playerCamera;

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
    #endregion
}
