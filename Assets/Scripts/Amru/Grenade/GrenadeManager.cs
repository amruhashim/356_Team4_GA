using UnityEngine;

public class GrenadeManager : MonoBehaviour
{
    [SerializeField] private float maxThrowForce = 50f;
    [SerializeField] private float quickThrowForce = 30f;
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private Transform throwableSpawn;
    [SerializeField] public int maxGrenades = 5;

    private bool canThrow = true;
    private Animator animator;
    private int currentGrenades;
    private float selectedThrowForce;
    private bool isHoldingGrenade = false;
    private AnimationController animationController;

    public int MaxGrenades { get { return maxGrenades; } }

    void Start()
    {
        animator = GetComponent<Animator>();
        currentGrenades = PlayerState.Instance.grenadeCount;

        // Update the grenade display at the start
        AmmoManager.Instance.UpdateGrenadeDisplay(currentGrenades);

        animationController = GetComponent<AnimationController>();
    }

    void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        if (Input.GetMouseButtonDown(0) && canThrow && currentGrenades > 0)
        {
            NormalThrow();
        }
        else if (Input.GetMouseButtonUp(0) && isHoldingGrenade)
        {
            ReleaseGrenade();
        }
        else if (Input.GetMouseButtonDown(1) && canThrow && currentGrenades > 0)
        {
            QuickThrow();
        }
    }

    void NormalThrow()
    {
        canThrow = false;
        selectedThrowForce = maxThrowForce;
        animator.SetBool("isThrowing", true);
        isHoldingGrenade = true;
        animationController.StartHoldingGrenade();  // Pause the animation at the hand-raising point
    }

    void ReleaseGrenade()
    {
        isHoldingGrenade = false;
        animationController.StopHoldingGrenade();  // Resume the animation
    }

    void QuickThrow()
    {
        canThrow = false;
        selectedThrowForce = quickThrowForce;
        animator.SetBool("isThrowing", true);
    }

    public void OnThrowAnimationEvent()
    {
        // Use the stored throw force determined when the throw was initiated
        LaunchGrenade(selectedThrowForce);
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

    void LaunchGrenade(float throwForce)
    {
        GameObject grenade = Instantiate(grenadePrefab, throwableSpawn.position, Camera.main.transform.rotation);
        Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();
        grenadeRb.AddForce(Camera.main.transform.forward * throwForce, ForceMode.Impulse);

        Grenade grenadeScript = grenade.GetComponent<Grenade>();
        grenadeScript.SetInitialBounceForce(throwForce);
        grenadeScript.DelayedExplosion(2.0f); // Example delay time
    }

    public void AddGrenades(int amount)
    {
        currentGrenades = Mathf.Min(currentGrenades + amount, maxGrenades);
        PlayerState.Instance.grenadeCount = currentGrenades;
        PlayerState.Instance.SavePlayerData();

        // Update the grenade display when grenades are added
        AmmoManager.Instance.UpdateGrenadeDisplay(currentGrenades);
    }

    public int GetCurrentGrenades()
    {
        return currentGrenades;
    }
}
