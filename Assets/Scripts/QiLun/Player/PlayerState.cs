using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public static PlayerState Instance { get; set; }

    public Transform playerTransform;
    public float currentHealth;
    public float maxHealth;

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
        PlayerPrefs.Save();
    }

    public void LoadPlayerData()
    {
        if (PlayerPrefs.HasKey("playerStarted"))
        {
            currentHealth = PlayerPrefs.GetFloat("currentHealth", maxHealth);
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
        }
        else
        {
            currentHealth = maxHealth;
            PlayerPrefs.SetInt("playerStarted", 1);
            SavePlayerData();
        }
    }

    public void InitializeNewPlayerData()
    {
        currentHealth = maxHealth;

        // Reset position and rotation to the initial spawn point
        playerTransform.position = Vector3.zero; // Customize the spawn point if needed
        playerTransform.rotation = Quaternion.identity;

        SavePlayerData();

        Debug.Log("New player data initialized");
    }
}
