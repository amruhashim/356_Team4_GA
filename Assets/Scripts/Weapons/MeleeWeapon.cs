using System.Collections;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    #region FIELDS

    private AnimationController animatorController;
    private bool readyToAttack = true;
    public float leftClickAttackDelay = 1.0f;
    public float rightClickAttackDelay = 1.2f;
    public float hitDelay = 0.1f; 

    [Header("Attack Settings")]
    public float attackRange = 2f; 
    public float leftClickDamageAmount = 5f; 
    public float rightClickDamageAmount = 10f; 

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

        // Deal damage with left-click damage amount
        DealDamage(leftClickDamageAmount);

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

        // Deal damage with right-click damage amount
        DealDamage(rightClickDamageAmount);

        Invoke("ResetAttack", rightClickAttackDelay);
    }

    private void DealDamage(float damageAmount)
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

                // If the component exists, deal damage directly
                if (aiAgent != null)
                {
                    StartCoroutine(DoDamage(aiAgent, damageAmount));
                }
            }
        }
    }

    private IEnumerator DoDamage(PatrolAgent aiAgent, float damageAmount)
    {
        yield return new WaitForSeconds(hitDelay);
        aiAgent.HitByProjectile(damageAmount);
    }

    private void ResetAttack()
    {
        readyToAttack = true;
    }

    #endregion
}
