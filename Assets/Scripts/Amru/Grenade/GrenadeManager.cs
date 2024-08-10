using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GrenadeManager : MonoBehaviour
{
    [SerializeField] private float maxThrowForce = 25f;
    [SerializeField] private float forceMultiplier = 1f;
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private Transform throwableSpawn;
    [SerializeField] public float maxChargeTime = 6.0f; // Max time to charge the grenade
    [SerializeField] private float chargeSpeedMultiplier = 2.0f; // Multiplier to increase charge speed
    [SerializeField] private float sliderResetDuration = 0.5f; // Time to reset slider to zero

    private float throwForce = 0f;
    private float chargeTime = 0f;
    private bool isCharging = false;
    private bool canThrow = true;  // Flag to prevent charging another grenade until animation finishes
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        AmmoManager.Instance.throwForceSlider.gameObject.SetActive(false);
    }

    void Update()
    {
        // Ensure the grenade only reacts to input when the cursor is locked
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        if (Input.GetMouseButtonDown(0) && canThrow)
        {
            StartCharging();
        }

        if (Input.GetMouseButton(0) && isCharging)
        {
            ChargeThrow();
        }

        if (Input.GetMouseButtonUp(0) && isCharging)
        {
            ReleaseGrenade();
        }
    }

    void StartCharging()
    {
        isCharging = true;
        AmmoManager.Instance.throwForceSlider.gameObject.SetActive(true);
        AmmoManager.Instance.UpdateThrowForceSlider(0);
        throwForce = 0f;
        chargeTime = 0f;
    }

    void ChargeThrow()
    {
        if (chargeTime < maxChargeTime)
        {
            chargeTime += Time.deltaTime * chargeSpeedMultiplier;
            throwForce = (chargeTime / maxChargeTime) * maxThrowForce;
            AmmoManager.Instance.UpdateThrowForceSlider(chargeTime);
        }
    }

    void ReleaseGrenade()
    {
        isCharging = false;
        StartCoroutine(SmoothSliderReset());

        if (chargeTime >= 1.0f) // Only throw if charge time is greater than one second
        {
            canThrow = false;
            animator.SetBool("isThrowing", true);  // Trigger the throwing animation
        }
    }

    IEnumerator SmoothSliderReset()
    {
        float elapsedTime = 0;
        float startValue = AmmoManager.Instance.throwForceSlider.value;
        while (elapsedTime < sliderResetDuration)
        {
            elapsedTime += Time.deltaTime;
            AmmoManager.Instance.UpdateThrowForceSlider(Mathf.Lerp(startValue, 0, elapsedTime / sliderResetDuration));
            yield return null;
        }
        AmmoManager.Instance.throwForceSlider.gameObject.SetActive(false);
    }

    public void OnThrowAnimationEvent()
    {
        LaunchGrenade(chargeTime);
    }

    public void OnThrowAnimationEnd()
    {
        canThrow = true;  // Reset the flag when the animation ends
        animator.SetBool("isThrowing", false);  // Reset the animation state
    }

    void LaunchGrenade(float delayTime)
    {
        GameObject grenade = Instantiate(grenadePrefab, throwableSpawn.position, Camera.main.transform.rotation);
        Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();
        grenadeRb.AddForce(Camera.main.transform.forward * (throwForce * forceMultiplier), ForceMode.Impulse);
        Grenade grenadeScript = grenade.GetComponent<Grenade>();
        grenadeScript.SetInitialBounceForce(throwForce); // Set the initial bounce force based on throw force
        grenadeScript.DelayedExplosion(delayTime);
    }
}
