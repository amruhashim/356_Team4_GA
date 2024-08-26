using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletDebug : MonoBehaviour
{
    public LayerMask interactionLayerMask;  // Set this in the inspector to the layer you want the bullet to interact with

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object is on the interaction layer
        if (((1 << collision.gameObject.layer) & interactionLayerMask) != 0)
        {
            // Check if the collided object is the player
            if (collision.gameObject.CompareTag("Player"))
            {
                Debug.Log("Bullet collided with the player: " + collision.gameObject.name);
                // Implement any additional logic for when the bullet hits the player
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
