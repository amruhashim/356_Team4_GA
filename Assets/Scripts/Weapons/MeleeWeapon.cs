using System.Collections;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    #region FIELDS

    private AnimationController animatorController;
    private bool readyToAttack = true;
    public float leftClickAttackDelay = 1.0f;
    public float rightClickAttackDelay = 1.2f;
    public float hitDelay = 0.1f; // Adjustable delay for HitByProjectile() call

    [Header("Attack Settings")]
    public float attackRange = 2f; // Adjust the attack range as needed
    public float damageAmount = 10f; // Adjust the damage amount as needed

    [Header("Audio Settings")]
    public AudioClip leftClickAttackSound;
    public AudioClip rightClickAttackSound;
    private AudioSource audioSource;

    #endregion

    #region UNITY METHODS

    private void Awake()
    {
        animatorController = GetComponent<AnimationController>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (readyToAttack)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                PerformLeftClickAttack();
            }
            else if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                PerformRightClickAttack();
            }
        }
    }

    #endregion

    #region MELEE METHODS

    private void PerformLeftClickAttack()
    {
        readyToAttack = false;
        animatorController.TriggerLeftClickAttack();

        if (audioSource != null && leftClickAttackSound != null)
        {
            audioSource.PlayOneShot(leftClickAttackSound);
        }

        // Call the dealDamage method with half the maxHits as damage
        DealDamage(0.5f);

        Invoke("ResetAttack", leftClickAttackDelay);
    }

    private void PerformRightClickAttack()
    {
        readyToAttack = false;
        animatorController.TriggerRightClickAttack();

        if (audioSource != null && rightClickAttackSound != null)
        {
            audioSource.PlayOneShot(rightClickAttackSound);
        }

        // Call the dealDamage method with maxHits as damage
        DealDamage(1.0f);

        Invoke("ResetAttack", rightClickAttackDelay);
    }

    private void DealDamage(float damageMultiplier)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, attackRange))
        {
            Debug.Log($"Hit: {hit.transform.name} with tag: {hit.transform.tag}");

            // Apply damage to the hit object
            if (hit.transform.CompareTag("PatrolAgent"))
            {
                // Get the PatrolAgent component from the hit object
                PatrolAgent aiAgent = hit.transform.GetComponent<PatrolAgent>();

                // If the component exists, deal damage based on the multiplier
                if (aiAgent != null)
                {
                    int damageToDeal = Mathf.CeilToInt(aiAgent.maxHits * damageMultiplier);
                    StartCoroutine(DoDamage(aiAgent, damageToDeal));
                }
            }
        }
    }

    private IEnumerator DoDamage(PatrolAgent aiAgent, int damageToDeal)
    {
        for (int i = 0; i < damageToDeal; i++)
        {
            yield return new WaitForSeconds(hitDelay);
            aiAgent.HitByProjectile();
        }
    }

    private void ResetAttack()
    {
        readyToAttack = true;
    }

    #endregion
} 