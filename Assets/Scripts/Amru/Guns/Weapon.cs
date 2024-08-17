using System.Collections;
using TMPro;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    #region FIELDS

    [Header("References")]
    public Camera playerCamera;
    public GameObject bulletPrefab;
    public GameObject muzzleEffect;
    public Transform bulletSpawn;

    [Header("Weapon Type")]
    public gunType GunType;
    public string weaponID;

    [Header("Reloading & Magazine")]
    public float reloadTime;
    public int magazineSize;
    public bool isReloading;

    [Header("Bullet Settings")]
    public float bulletVelocity = 30f;
    public float bulletLifeTime = 3f;

    [Header("Shooting Settings")]
    public bool isShooting;
    public bool readyToShoot;
    public float shootingDelay = 0.5f;

    [HideInInspector]
    public float initialDelay = 0.5f;

    [HideInInspector]
    [Header("Burst Settings")]
    public int bulletsPerShot = 1;

    [Header("Accuracy Settings")]
    public float spreadIntensity;

    [Header("Ammo Management")]
    [HideInInspector] public int accumulatedBullets = 0;
    [HideInInspector] public int bulletsLeft = 0;

    [Header("Audio Settings")]
    public AudioClip shootingSound;
    public AudioClip reloadSound;
    public AudioClip emptySound;
    private AudioSource audioSource;

    public enum gunType
    {
        HandGun,
        ShotGun,
        MachineGun,
    }

    private bool hasPlayedEmptySound = false;
    private AnimationController animatorController;

    #endregion

    #region UNITY METHODS

    private void Awake()
    {
        readyToShoot = true;
        audioSource = GetComponent<AudioSource>();
    }

private void Start()
{
    // Instantiate the bullet and set its position and rotation
    GameObject instantiated = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);
    instantiated.transform.SetPositionAndRotation(bulletSpawn.position, Quaternion.LookRotation(bulletSpawn.forward));

    // Destroy the instantiated bullet immediately so it's not visible in the game
    Destroy(instantiated);

    // Get the AnimationController component
    animatorController = GetComponent<AnimationController>();

    // Load bullets left and accumulated bullets from PlayerState or reset for a new game
    if (PlayerState.Instance != null && PlayerState.Instance.IsNewGame())
    {
        ResetBullets(); // Initialize with default values for a new game
    }
    else
    {
        LoadBulletsFromPlayerState(); // Load from saved data
    }
}


    private void Update()
    {
        // Ensure the weapon only reacts to input when the cursor is locked
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        // Handle input for shooting
        HandleShootingInput();

        // Handle reloading
        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !isReloading && accumulatedBullets > 0)
        {
            Reload();
        }

        // Update ammo display
        UpdateAmmoDisplay();
    }

    #endregion

    #region SHOOTING METHODS

    private void HandleShootingInput()
    {
        if (GunType == gunType.MachineGun)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                isShooting = true;
                hasPlayedEmptySound = false;
            }
            else if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                isShooting = false;
                hasPlayedEmptySound = false;
            }
        }
        else if (GunType == gunType.HandGun || GunType == gunType.ShotGun)
        {
            isShooting = Input.GetKeyDown(KeyCode.Mouse0);
            hasPlayedEmptySound = false;
        }

        if (readyToShoot && isShooting)
        {
            if (bulletsLeft > 0)
            {
                FireWeapon();
            }
            else if (!hasPlayedEmptySound)
            {
                PlayEmptySound();
                hasPlayedEmptySound = true;
            }
        }
    }

    private void FireWeapon()
    {
        readyToShoot = false;

        if (GunType == gunType.ShotGun)
        {
            PlayShootingSound();
            animatorController.SetShooting(true);
            for (int i = 0; i < bulletsPerShot; i++)
            {
                FireBullet();
            }
            bulletsLeft -= bulletsPerShot;
            Invoke("ResetShot", shootingDelay);
        }
        else if (GunType == gunType.MachineGun)
        {
            StartCoroutine(FireMachineGun());
        }
        else // HandGun
        {
            PlayShootingSound();
            animatorController.SetShooting(true);
            FireBullet();
            bulletsLeft--;
            Invoke("ResetShot", shootingDelay);
        }

        // Save bullets left to PlayerState
        SaveBulletsToPlayerState();
    }

    private IEnumerator FireMachineGun()
    {
        yield return new WaitForSeconds(initialDelay);

        while (isShooting && bulletsLeft > 0 && !isReloading)
        {
            PlayShootingSound();
            animatorController.SetShooting(true);
            FireBullet();
            bulletsLeft--;
            yield return new WaitForSeconds(shootingDelay);

            // Save bullets left to PlayerState
            SaveBulletsToPlayerState();
        }

        animatorController.SetShooting(false);
        readyToShoot = true;
    }

    private void FireBullet()
    {
        muzzleEffect.GetComponent<ParticleSystem>().Play();

        Vector3 shootingDirection = CalculateDirectionAndSpread();
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.LookRotation(shootingDirection));
        bullet.GetComponent<Rigidbody>().velocity = shootingDirection * bulletVelocity;

        StartCoroutine(DestroyBulletAfterTime(bullet, bulletLifeTime));
    }

    private void ResetShot()
    {
        readyToShoot = true;
        animatorController.SetShooting(false);
    }

    private void PlayShootingSound()
    {
        if (audioSource != null && shootingSound != null)
        {
            audioSource.PlayOneShot(shootingSound);
        }
    }

    private void PlayEmptySound()
    {
        if (audioSource != null && emptySound != null)
        {
            audioSource.PlayOneShot(emptySound);
        }
    }

    #endregion

    #region RELOADING METHODS

    private void Reload()
    {
        StopAllCoroutines();
        animatorController.SetShooting(false);
        animatorController.SetReloading(true);
        isReloading = true;
        readyToShoot = false;
        if (audioSource != null && reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }
        Invoke("ReloadCompleted", reloadTime);
    }

    private void ReloadCompleted()
    {
        animatorController.SetReloading(false);
        int bulletsToReload = Mathf.Min(magazineSize - bulletsLeft, accumulatedBullets);
        bulletsLeft += bulletsToReload;
        accumulatedBullets -= bulletsToReload;
        isReloading = false;
        readyToShoot = true;

        // Save bullets left and accumulated bullets to PlayerState
        SaveBulletsToPlayerState();

        UpdateAmmoDisplay();
    }

    #endregion

    #region AMMO METHODS

    public void CollectAmmo(int ammoAmount)
    {
        accumulatedBullets += ammoAmount;

        // Save accumulated bullets to PlayerState
        SaveBulletsToPlayerState();

        UpdateAmmoDisplay();
    }

    private void UpdateAmmoDisplay()
    {
        if (AmmoManager.Instance.ammoDisplay != null)
        {
            AmmoManager.Instance.ammoDisplay.text = $"{bulletsLeft}/{accumulatedBullets}";
        }
    }

    private void SaveBulletsToPlayerState()
    {
        if (PlayerState.Instance.activeWeaponID == weaponID)
        {
            PlayerState.Instance.bulletsLeft = bulletsLeft;
            PlayerState.Instance.accumulatedBullets = accumulatedBullets;
        }
    }

    private void LoadBulletsFromPlayerState()
    {
        if (PlayerState.Instance.activeWeaponID == weaponID)
        {
            bulletsLeft = PlayerState.Instance.bulletsLeft;
            accumulatedBullets = PlayerState.Instance.accumulatedBullets;
        }
    }

    public void ResetBullets()
    {
        bulletsLeft = magazineSize;
        accumulatedBullets = magazineSize;
        SaveBulletsToPlayerState();
    }

    #endregion

    #region UTILITY METHODS

    public Vector3 CalculateDirectionAndSpread()
    {
        // Calculate the forward direction of the camera
        Vector3 forwardDirection = playerCamera.transform.forward;

        // Calculate the spread using camera's right and up directions
        float x = Random.Range(-spreadIntensity, spreadIntensity);
        float y = Random.Range(-spreadIntensity, spreadIntensity);

        Vector3 spread = playerCamera.transform.right * x + playerCamera.transform.up * y;
        Vector3 finalDirection = (forwardDirection + spread).normalized;

        return finalDirection;
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }

    #endregion
}
