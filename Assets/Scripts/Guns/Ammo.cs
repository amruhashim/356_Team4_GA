using UnityEngine;

public class Ammo : MonoBehaviour
{
    public Weapon.gunType GunType;
    public int ammoAmount = 10; 
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            
            Weapon weapon = other.GetComponentInChildren<Weapon>();
            if (weapon != null && weapon.GunType == GunType)
            {
                weapon.CollectAmmo(ammoAmount);
                Destroy(gameObject); // Destroy ammo object after collection
            }
        }
    }
}
