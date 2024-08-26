using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float playerHealth;
    public Image healthImpact;
    private Coroutine fadeCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        playerHealth = 100;
        healthImpact.color = new Color(healthImpact.color.r, healthImpact.color.g, healthImpact.color.b, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Bullets"))
        {
            PlayerTakingDamage(10f);
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeOutHealthImpact());

            // Optionally, destroy the bullet if it should disappear after hitting the player
            Destroy(other.gameObject);
        }
    }

    void HealthDamageImpact()
    {
        float transparency = 1f - (playerHealth / 100f);
        Color imageColor = healthImpact.color;
        imageColor.a = Mathf.Clamp(transparency, 0, 1);
        healthImpact.color = imageColor;
    }

    void PlayerTakingDamage(float damage)
    {
        if (playerHealth > 0)
        {
            playerHealth -= damage;
            HealthDamageImpact();
            Debug.Log("Player is taking damage");
        }
    }

    IEnumerator FadeOutHealthImpact()
    {
        while (healthImpact.color.a > 0)
        {
            Color imageColor = healthImpact.color;
            imageColor.a -= Time.deltaTime / 2;  // Adjust fade speed here
            healthImpact.color = imageColor;
            yield return null;
        }
    }
}
