using System.Collections;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    #region FIELDS

    private AnimationController animatorController;
    private bool readyToAttack = true;
    public float leftClickAttackDelay = 1.0f;
    public float rightClickAttackDelay = 1.2f; // You can customize this for different delays

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

        // Implement left-click attack logic here (e.g., detecting hits on nearby enemies)

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

        // Implement right-click attack logic here (e.g., detecting hits on nearby enemies)

        Invoke("ResetAttack", rightClickAttackDelay);
    }

    private void ResetAttack()
    {
        readyToAttack = true;
    }

    #endregion
}
