using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AmmoManager : MonoBehaviour
{
    public static AmmoManager Instance { get; set; }
    public TextMeshProUGUI ammoDisplay;
    public TextMeshProUGUI grenadeDisplay;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple AmmoManager instances detected. Destroying extra instances.");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("AmmoManager instance set.");
            Instance = this;
        }
    }

    public void UpdateAmmoDisplay(Weapon weapon)
    {
        ammoDisplay.text = $"{weapon.bulletsLeft}/{weapon.accumulatedBullets}";
    }


    // Updated method to access GrenadeManager and display currentGrenades / maxGrenades
        public void UpdateGrenadeDisplay(int currentGrenades)
    {
        GrenadeManager grenadeManager = FindObjectOfType<GrenadeManager>();
        if (grenadeManager != null && grenadeDisplay != null)
        {
            grenadeDisplay.text = $"Grenades: {currentGrenades} / {grenadeManager.MaxGrenades}";
        }
    }

}
