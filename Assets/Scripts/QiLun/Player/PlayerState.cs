using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public static PlayerState Instance { get; private set; }

    public Transform playerTransform;
    public float currentHealth;
    public float maxHealth;
    public string activeWeaponID;  // Store the current weapon's ID
    public int bulletsLeft;        // Store bullets left for the active weapon
    public int accumulatedBullets; // Store accumulated bullets for the active weapon
    public int grenadeCount;       // Store current grenade count

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        // Do not load player data automatically; let SaveManager control it
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (currentHealth > 0)
            {
                currentHealth -= 10;
                if (currentHealth < 0)
                {
                    currentHealth = 0;
                }
                Debug.Log($"Current Health after taking damage: {currentHealth}");
                SavePlayerData();
            }
        }
    }

    public void SavePlayerData()
    {
        PlayerPrefs.SetFloat("currentHealth", currentHealth);
        PlayerPrefs.SetFloat("playerPositionX", playerTransform.position.x);
        PlayerPrefs.SetFloat("playerPositionY", playerTransform.position.y);
        PlayerPrefs.SetFloat("playerPositionZ", playerTransform.position.z);
        PlayerPrefs.SetFloat("playerRotationX", playerTransform.rotation.eulerAngles.x);
        PlayerPrefs.SetFloat("playerRotationY", playerTransform.rotation.eulerAngles.y);
        PlayerPrefs.SetFloat("playerRotationZ", playerTransform.rotation.eulerAngles.z);

        if (!string.IsNullOrEmpty(activeWeaponID))
        {
            PlayerPrefs.SetString("activeWeaponID", activeWeaponID);  // Save the active weapon ID
            PlayerPrefs.SetInt("bulletsLeft", bulletsLeft);           // Save bullets left for the active weapon
            PlayerPrefs.SetInt("accumulatedBullets", accumulatedBullets); // Save accumulated bullets for the active weapon
        }

        // Save grenade count
        PlayerPrefs.SetInt("grenadeCount", grenadeCount);

        PlayerPrefs.Save();
        Debug.Log("Player data saved.");
    }

    public void LoadPlayerData()
    {
        if (PlayerPrefs.HasKey("playerStarted"))
        {
            currentHealth = PlayerPrefs.GetFloat("currentHealth", maxHealth);
            Debug.Log($"Loaded Current Health: {currentHealth}");

            // Load position and rotation from PlayerPrefs
            playerTransform.position = new Vector3(
                PlayerPrefs.GetFloat("playerPositionX"),
                PlayerPrefs.GetFloat("playerPositionY"),
                PlayerPrefs.GetFloat("playerPositionZ")
            );
            playerTransform.rotation = Quaternion.Euler(
                PlayerPrefs.GetFloat("playerRotationX"),
                PlayerPrefs.GetFloat("playerRotationY"),
                PlayerPrefs.GetFloat("playerRotationZ")
            );

            // Load active weapon ID if available
            if (PlayerPrefs.HasKey("activeWeaponID"))
            {
                activeWeaponID = PlayerPrefs.GetString("activeWeaponID");
                Debug.Log($"Loaded Active Weapon ID: {activeWeaponID}");

                // Load bullets left and accumulated bullets for the active weapon
                bulletsLeft = PlayerPrefs.GetInt("bulletsLeft", 0);
                accumulatedBullets = PlayerPrefs.GetInt("accumulatedBullets", 0);
            }
            else
            {
                activeWeaponID = null;
                bulletsLeft = 0;
                accumulatedBullets = 0;
            }

            // Load grenade count
            grenadeCount = PlayerPrefs.GetInt("grenadeCount", 0);
            Debug.Log($"Loaded Grenade Count: {grenadeCount}");
        }
        else
        {
            // For a new game, initialize data instead of loading
            InitializeNewPlayerData();
        }
    }

    public void InitializeNewPlayerData()
    {
        currentHealth = maxHealth;

        // Reset position and rotation to the initial spawn point
        playerTransform.position = Vector3.zero; // Customize the spawn point if needed
        playerTransform.rotation = Quaternion.identity;

        activeWeaponID = null; // No active weapon for a new player

        grenadeCount = 0; // Start with zero grenades

        // Notify all weapons to reset their bullets to default values
        Weapon[] weapons = FindObjectsOfType<Weapon>();
        foreach (Weapon weapon in weapons)
        {
            weapon.ResetBullets();
        }

        // Save that the player has started the game
        PlayerPrefs.SetInt("playerStarted", 1);
        PlayerPrefs.Save();

        SavePlayerData();

        Debug.Log("New player data initialized");
    }

    // Method to check if it's a new game
    public bool IsNewGame()
    {
        return !PlayerPrefs.HasKey("playerStarted");
    }
}
