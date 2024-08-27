using UnityEngine;

public class Ammo : MonoBehaviour
{
    public string weaponID; 
    public int ammoAmount = 10;
    public bool isGrenade = false;  // Flag to indicate if this ammo is for grenades

   private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Player"))
    {
        if (isGrenade)
        {
            // Handle grenade inventory
            GrenadeManager grenadeManager = other.GetComponentInChildren<GrenadeManager>();
            if (grenadeManager != null)
            {
                // Only add grenades if the current grenade count is less than the maximum
                if (grenadeManager.GetCurrentGrenades() < grenadeManager.MaxGrenades)
                {
                    grenadeManager.AddGrenades(ammoAmount);
                    Destroy(gameObject); // Destroy the ammo object after collection
                }
                else
                {
                    Debug.Log("Grenade count is at max, cannot collect more.");
                }
            }
        }
        else
        {
            // Handle regular weapon ammo
            WeaponSwitcher weaponSwitcher = other.GetComponentInChildren<WeaponSwitcher>();
            if (weaponSwitcher != null && weaponSwitcher.currentWeapon != null)
            {
                Weapon currentWeapon = weaponSwitcher.currentWeapon;

                // Ensure the ammo is collected only if the weaponID matches
                if (currentWeapon.weaponID == weaponID)
                {
                    currentWeapon.CollectAmmo(ammoAmount);
                    Destroy(gameObject); // Destroy the ammo object after collection
                }
                else
                {
                    Debug.LogWarning($"Ammo for weaponID {weaponID} does not match the current weaponID {currentWeapon.weaponID}.");
                }
            }
        }
    }
}

}
