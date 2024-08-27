using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public Image healthImpact;  // Image for the health impact effect
    public Image healthFillImage;  // Image that represents the health fill amount
    public TextMeshProUGUI healthText;  // Reference to the TextMeshPro health text

    public Coroutine fadeCoroutine;

    // Color settings for health fill
    public Color healthyColor = Color.green;  
    public Color midHealthColor = Color.yellow;  
    public Color lowHealthColor = Color.red;  

    void Start()
    {
        // Only set to max health if starting a new game, otherwise use the saved health
        if (PlayerState.Instance.IsNewGame())
        {
            PlayerState.Instance.currentHealth = PlayerState.Instance.maxHealth;
        }
        
        // Initialize the health impact UI based on current health
        HealthDamageImpact();
        healthImpact.color = new Color(healthImpact.color.r, healthImpact.color.g, healthImpact.color.b, 0);
        
        // Update the health visuals on start
        UpdateHealthUI();
    }

    void HealthDamageImpact()
    {
        float transparency = 1f - (PlayerState.Instance.currentHealth / PlayerState.Instance.maxHealth);
        Color imageColor = healthImpact.color;
        imageColor.a = Mathf.Clamp(transparency, 0, 1);
        healthImpact.color = imageColor;
    }

    public void PlayerTakingDamage(float damage)
    {
        if (PlayerState.Instance.currentHealth > 0)
        {
            PlayerState.Instance.currentHealth -= damage;
            PlayerState.Instance.SavePlayerData(); // Save the updated health to PlayerState
            HealthDamageImpact();
            UpdateHealthUI();  // Update the health fill and text
            Debug.Log("Player is taking damage");

            if (PlayerState.Instance.currentHealth <= 0)
            {
                
                Debug.Log("Player is dead");
               
            }
        }
    }

    void UpdateHealthUI()
    {
        // Update the health fill amount based on the current health
        float healthPercentage = PlayerState.Instance.currentHealth / PlayerState.Instance.maxHealth;
        healthFillImage.fillAmount = healthPercentage;

        // Update the health fill color based on health percentage
        if (healthPercentage > 0.5f)
        {
            // Lerp between green and yellow
            healthFillImage.color = Color.Lerp(midHealthColor, healthyColor, (healthPercentage - 0.5f) * 2);
        }
        else
        {
            // Lerp between red and yellow
            healthFillImage.color = Color.Lerp(lowHealthColor, midHealthColor, healthPercentage * 2);
        }

        // Update the health text
        healthText.text = Mathf.Max(PlayerState.Instance.currentHealth, 0).ToString("0");  // Display health as an integer
    }

    public IEnumerator FadeOutHealthImpact()
    {
        while (healthImpact.color.a > 0)
        {
            Color imageColor = healthImpact.color;
            imageColor.a -= Time.deltaTime / 2;  
            healthImpact.color = imageColor;
            yield return null;
        }
    }
}
