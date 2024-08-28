using UnityEngine;
using System.Collections;

public class Grenade : MonoBehaviour
{
    public GameObject explosionEffect;
    public float explosionRadius = 50f;
    public float explosionForce = 500f;
    public float maxDamage = 8f; // Maximum damage from the grenade to AI
    public AudioClip bounceSound;
    public AudioClip explosionSound;
    private AudioSource audioSource;
    private bool hasExploded = false;

    private float initialBounceForce;
    private int bounceCount = 0;
    public const int maxBounces = 3; // Maximum number of bounces before stopping
    public const float bounceReductionFactor = 0.3f; // Reduction factor for each bounce

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void SetInitialBounceForce(float force)
    {
        initialBounceForce = force * 0.3f; // Reduced the initial bounce force
    }

    public void DelayedExplosion(float delay)
    {
        Invoke("Explode", delay);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Only play the bounce sound if the grenade hasn't exploded
        if (!hasExploded && collision.relativeVelocity.magnitude > 0.1f)
        {
            audioSource.PlayOneShot(bounceSound);

            if (bounceCount < maxBounces)
            {
                float bounceForce = initialBounceForce * Mathf.Pow(bounceReductionFactor, bounceCount);
                Vector3 bounceDirection = collision.contacts[0].normal;
                GetComponent<Rigidbody>().AddForce(bounceDirection * bounceForce, ForceMode.Impulse);
                bounceCount++;
            }
        }
    }

    void Explode()
    {
        // Set the flag to indicate the grenade has exploded
        hasExploded = true;

        // Play the explosion sound
        audioSource.PlayOneShot(explosionSound);

        Instantiate(explosionEffect, transform.position, Quaternion.identity);
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            // Avoid player 
            if (nearbyObject.CompareTag("Player"))
                continue;

            // Apply explosion force to any rigidbody within the radius
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }

            // Apply damage to PatrolAgent if within the radius
            PatrolAgent agent = nearbyObject.GetComponent<PatrolAgent>();
            if (agent != null)
            {
                float distance = Vector3.Distance(transform.position, nearbyObject.transform.position);
                float damage = Mathf.Lerp(maxDamage, 0, distance / explosionRadius);
                agent.HitByGrenade(damage);
            }

            // Change color of any object with tag "Breakable" within the radius

        }

        // Turn off all SkinnedMeshRenderers in the children of this grenade
        SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var renderer in skinnedMeshRenderers)
        {
            renderer.enabled = false;
        }

        // Disable the collider to stop physical interactions
        GetComponent<Collider>().enabled = false;

        // Delay the destruction of the game object for a shorter time after the explosion
        StartCoroutine(DestroyAfterSound());
    }

    IEnumerator DestroyAfterSound()
    {
        yield return new WaitForSeconds(Mathf.Min(1.0f, explosionSound.length)); // Destroy after 1 second or when the sound finishes, whichever is shorter
        Destroy(gameObject);
    }
}
