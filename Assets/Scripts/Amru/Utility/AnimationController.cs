using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator animator;
    public Movement movementScript;  // Reference to Movement script

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // Check if movementScript is assigned and use its isMoving property
        if (movementScript != null)
        {
            SetWalking(movementScript.isMoving);
        }
    }

    public void SetWalking(bool isWalking)
    {
        animator.SetBool("isWalking", isWalking);
    }

    public void SetShooting(bool isShooting)
    {
        animator.SetBool("isShooting", isShooting);
    }

    public void SetReloading(bool isReloading)
    {
        animator.SetBool("isReloading", isReloading);
    }

    public void SetThrowing(bool isThrowing)
    {
        animator.SetBool("isThrowing", isThrowing);
    }

    // This method will be called by an AnimationEvent at the point in the animation where the grenade should be thrown
    public void ThrowGrenade()
    {
        GrenadeManager grenadeManager = GetComponent<GrenadeManager>();
        if (grenadeManager != null)
        {
            grenadeManager.OnThrowAnimationEvent();
        }
    }

    // This method will be called by an AnimationEvent at the end of the throwing animation
    public void ThrowingAnimationEnd()
    {
        GrenadeManager grenadeManager = GetComponent<GrenadeManager>();
        if (grenadeManager != null)
        {
            grenadeManager.OnThrowAnimationEnd();
        }
    }
}
