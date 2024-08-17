using System.Collections;
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
    public int accumulatedBullets = 0;
    public int bulletsLeft = 0;

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
        animatorController = GetComponent<AnimationController>();
    }

    private void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        HandleShootingInput();

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !isReloading && accumulatedBullets > 0)
        {
            Reload();
        }

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

        SaveBulletsToPlayerState(); // Save bullets state after firing
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

            SaveBulletsToPlayerState(); // Save bullets state after firing
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

        SaveBulletsToPlayerState(); // Save bullets state after reloading
        UpdateAmmoDisplay();
    }

    #endregion

    #region AMMO METHODS

    public void CollectAmmo(int ammoAmount)
    {
        accumulatedBullets += ammoAmount;
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
        if (PlayerState.Instance != null && PlayerState.Instance.activeWeaponID == weaponID)
        {
            PlayerState.Instance.bulletsLeft = bulletsLeft;
            PlayerState.Instance.accumulatedBullets = accumulatedBullets;
        }
    }

    public void LoadBulletsFromPlayerState()
    {
        if (PlayerState.Instance != null && PlayerState.Instance.activeWeaponID == weaponID)
        {
            bulletsLeft = PlayerState.Instance.bulletsLeft;
            accumulatedBullets = PlayerState.Instance.accumulatedBullets;
        }
    }

    public void ResetBullets()
    {
        bulletsLeft = magazineSize;
        accumulatedBullets = magazineSize;
        SaveBulletsToPlayerState(); // Save the full magazine to PlayerState
    }

    #endregion

    #region UTILITY METHODS

    public Vector3 CalculateDirectionAndSpread()
    {
        Vector3 forwardDirection = playerCamera.transform.forward;
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
