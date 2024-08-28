using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoManager : MonoBehaviour
{
    public static AmmoManager Instance { get; private set; }

    public TextMeshProUGUI ammoDisplay;
    public TextMeshProUGUI grenadeDisplay;

    public Image meleeWeaponIcon;
    public Image grenadeIcon;
    public Image activeWeaponIcon;
    public Image droneIcon;

    public float inactiveOpacity = 0.5f;
    public float activeOpacity = 1.0f;

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
        ammoDisplay.text = $"{weapon.bulletsLeft} | {weapon.accumulatedBullets}";
    }

    public void UpdateGrenadeDisplay(int currentGrenades)
    {
        GrenadeManager grenadeManager = FindObjectOfType<GrenadeManager>();
        if (grenadeManager != null && grenadeDisplay != null)
        {
           grenadeDisplay.text = $"{currentGrenades} | {grenadeManager.MaxGrenades}";
        }
    }

    public void HighlightMeleeWeaponIcon(Sprite meleeIcon)
    {
        meleeWeaponIcon.sprite = meleeIcon; 
        HighlightIcon(meleeWeaponIcon);
    }

    public void HighlightGrenadeIcon(Sprite grenadeSprite)
    {
        grenadeIcon.sprite = grenadeSprite; 
        HighlightIcon(grenadeIcon);
    }

    public void HighlightActiveWeaponIcon(Sprite newIcon)
    {
        activeWeaponIcon.sprite = newIcon;
        AdjustIconSize(activeWeaponIcon);
        HighlightIcon(activeWeaponIcon);
    }

    public void HighlightDroneIcon()
    {
        HighlightIcon(droneIcon);
    }

    private void HighlightIcon(Image activeIcon)
    {
        SetIconOpacity(meleeWeaponIcon, inactiveOpacity);
        SetIconOpacity(grenadeIcon, inactiveOpacity);
        SetIconOpacity(activeWeaponIcon, inactiveOpacity);
        SetIconOpacity(droneIcon, inactiveOpacity);

        SetIconOpacity(activeIcon, activeOpacity);
    }

    private void SetIconOpacity(Image icon, float opacity)
    {
        Color iconColor = icon.color;
        iconColor.a = opacity;
        icon.color = iconColor;
    }

    private void AdjustIconSize(Image icon)
    {
        RectTransform rectTransform = icon.GetComponent<RectTransform>();
        if (rectTransform != null && icon.sprite != null)
        {
            float spriteAspectRatio = (float)icon.sprite.rect.width / icon.sprite.rect.height;

            // Adjust RectTransform size to maintain the aspect ratio
            if (rectTransform.rect.width > rectTransform.rect.height)
            {
                rectTransform.sizeDelta = new Vector2(rectTransform.rect.height * spriteAspectRatio, rectTransform.rect.height);
            }
            else
            {
                rectTransform.sizeDelta = new Vector2(rectTransform.rect.width, rectTransform.rect.width / spriteAspectRatio);
            }
        }
    }
}
