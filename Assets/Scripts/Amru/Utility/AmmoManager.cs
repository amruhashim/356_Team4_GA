using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AmmoManager : MonoBehaviour
{
    public static AmmoManager Instance { get; set; }
    public TextMeshProUGUI ammoDisplay;
    public Slider throwForceSlider;
    public TextMeshProUGUI chargeTimeDisplay;  

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

    public void UpdateThrowForceSlider(float value)
    {
        if (throwForceSlider != null)
        {
            throwForceSlider.value = value;
            throwForceSlider.maxValue = 6.0f; 
        }

        if (chargeTimeDisplay != null)
        {
            chargeTimeDisplay.text = $"{value:F1}";  // Display charge time 
        }
    }
}
