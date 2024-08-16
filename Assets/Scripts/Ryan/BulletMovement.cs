using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMovement : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 3f; // How long the bullet lasts before being destroyed

    private Vector3 direction; // Direction the bullet will move

    void Start()
    {
        // Find the player and determine the direction to shoot
        Transform target = GameObject.FindGameObjectWithTag("Player").transform;
        if (target != null)
        {
            direction = (target.position - transform.position).normalized; // Calculate initial direction
            direction.y = 0; // Set Y direction to 0 to keep bullet level
        }

        Destroy(gameObject, lifetime); // Destroy bullet after a certain time
    }

    void Update()
    {
        // Move bullet in the predetermined direction, but keep the original Y position
        Vector3 newPosition = transform.position + direction * speed * Time.deltaTime;
        newPosition.y = transform.position.y; // Keep the original Y position
        transform.position = newPosition;
    }
}