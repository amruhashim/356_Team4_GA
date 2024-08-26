using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBullet : MonoBehaviour
{
    public LayerMask interactionLayerMask;  // Set this in the inspector to the layer you want the bullet to interact with
    public float damageAmount = 10f;        // Amount of damage this bullet will cause

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object is on the interaction layer
        if (((1 << collision.gameObject.layer) & interactionLayerMask) != 0)
        {
            // Check if the collided object is the player
            if (collision.gameObject.CompareTag("Player"))
            {
                Debug.Log("Bullet collided with the player: " + collision.gameObject.name);

                // Get the PlayerHealth component from the player
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    // Call the method to decrement health
                    playerHealth.PlayerTakingDamage(damageAmount);

                    // Handle the health impact effect and coroutine for fading
                    if (playerHealth.fadeCoroutine != null)
                    {
                        playerHealth.StopCoroutine(playerHealth.fadeCoroutine);
                    }
                    playerHealth.fadeCoroutine = playerHealth.StartCoroutine(playerHealth.FadeOutHealthImpact());
                }
            }
            else
            {
                Debug.Log("Bullet collided with an object on the interaction layer: " + collision.gameObject.name);
            }

            // Optionally destroy the bullet on collision
            Destroy(gameObject);
        }
    }
}
