using UnityEngine;

public class Ammo : MonoBehaviour
{
    public string weaponID; 
    public int ammoAmount = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
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
