using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GrenadeManager : MonoBehaviour
{
    [SerializeField] private float maxThrowForce = 25f;
    [SerializeField] private float forceMultiplier = 1f;
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private Transform throwableSpawn;
    [SerializeField] public float maxChargeTime = 6.0f;
    [SerializeField] private float chargeSpeedMultiplier = 2.0f;
    [SerializeField] private float sliderResetDuration = 0.5f;
    [SerializeField] public int maxGrenades = 5;

    private float throwForce = 0f;
    private float chargeTime = 0f;
    private bool isCharging = false;
    private bool canThrow = true;
    private Animator animator;
    private int currentGrenades;

    public int MaxGrenades
    {
        get { return maxGrenades; }
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        AmmoManager.Instance.throwForceSlider.gameObject.SetActive(false);
        currentGrenades = PlayerState.Instance.grenadeCount;

        // Update the grenade display at the start
        AmmoManager.Instance.UpdateGrenadeDisplay(currentGrenades);
    }

    void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        if (Input.GetMouseButtonDown(0) && canThrow && currentGrenades > 0)
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

        if (chargeTime >= 1.0f)
        {
            canThrow = false;
            animator.SetBool("isThrowing", true);
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
        canThrow = true;
        animator.SetBool("isThrowing", false);
        currentGrenades--;

        PlayerState.Instance.grenadeCount = currentGrenades;
        PlayerState.Instance.SavePlayerData();

        // Update the grenade display when a grenade is used
        AmmoManager.Instance.UpdateGrenadeDisplay(currentGrenades);
    }

    void LaunchGrenade(float delayTime)
    {
        GameObject grenade = Instantiate(grenadePrefab, throwableSpawn.position, Camera.main.transform.rotation);
        Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();
        grenadeRb.AddForce(Camera.main.transform.forward * (throwForce * forceMultiplier), ForceMode.Impulse);
        Grenade grenadeScript = grenade.GetComponent<Grenade>();
        grenadeScript.SetInitialBounceForce(throwForce);
        grenadeScript.DelayedExplosion(delayTime);
    }

    public void AddGrenades(int amount)
    {
        currentGrenades = Mathf.Min(currentGrenades + amount, maxGrenades);
        PlayerState.Instance.grenadeCount = currentGrenades;
        PlayerState.Instance.SavePlayerData();

        // Update the grenade display when grenades are added
        AmmoManager.Instance.UpdateGrenadeDisplay(currentGrenades);

        Debug.Log($"Grenades added: {amount}. Total grenades: {currentGrenades}");
    }

    public int GetCurrentGrenades()
    {
        return currentGrenades;
    }
}
