using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerState : MonoBehaviour
{
    public static PlayerState Instance { get; private set; }

    public Transform playerTransform;
    public float currentHealth;
    public float maxHealth;
    public string activeWeaponID;
    public int bulletsLeft;
    public int accumulatedBullets;
    public int grenadeCount;

    private Dictionary<string, bool> aiAgentStates = new Dictionary<string, bool>();

    public Vector3 initialSpawnPoint = new Vector3(0, 1, 0);
    public Quaternion initialRotation = Quaternion.identity;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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

    if (!string.IsNullOrEmpty(activeWeaponID))
    {
        PlayerPrefs.SetString("activeWeaponID", activeWeaponID);
        PlayerPrefs.SetInt("bulletsLeft", bulletsLeft);
        PlayerPrefs.SetInt("accumulatedBullets", accumulatedBullets);
    }

    PlayerPrefs.SetInt("grenadeCount", grenadeCount);

    // Print and save AI states
    Debug.Log("Saving AI States:");
    foreach (var kvp in aiAgentStates)
    {
        Debug.Log($"AI ID: {kvp.Key}, Is Dead: {kvp.Value}");
        PlayerPrefs.SetInt(kvp.Key, kvp.Value ? 1 : 0);
    }

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

        if (PlayerPrefs.HasKey("activeWeaponID"))
        {
            activeWeaponID = PlayerPrefs.GetString("activeWeaponID");
            bulletsLeft = PlayerPrefs.GetInt("bulletsLeft", 0);
            accumulatedBullets = PlayerPrefs.GetInt("accumulatedBullets", 0);
        }
        else
        {
            activeWeaponID = null;
            bulletsLeft = 0;
            accumulatedBullets = 0;
        }

        grenadeCount = PlayerPrefs.GetInt("grenadeCount", 0);

        // Load AI states and ensure all agents are registered
        PatrolAgent[] patrolAgents = FindObjectsOfType<PatrolAgent>();
        aiAgentStates.Clear();
        foreach (var agent in patrolAgents)
        {
            string aiID = agent.uniqueID;
            bool isDead = PlayerPrefs.GetInt(aiID, 0) == 1;
            aiAgentStates[aiID] = isDead;

            Debug.Log($"[LoadPlayerData] AI ID: {aiID} is marked as dead: {isDead}");
            if (isDead)
            {
                agent.gameObject.SetActive(false);
                Debug.Log($"[LoadPlayerData] Deactivated AI ID: {aiID}");
            }
        }
    }
    else
    {
        InitializeNewPlayerData();
    }
}



    public void InitializeNewPlayerData()
    {
        currentHealth = maxHealth;

        playerTransform.position = initialSpawnPoint;
        playerTransform.rotation = initialRotation;

        activeWeaponID = null;

        Weapon[] weapons = FindObjectsOfType<Weapon>();
        foreach (Weapon weapon in weapons)
        {
            weapon.ResetBullets();
        }

        grenadeCount = 5;

        aiAgentStates.Clear();

        PatrolAgent[] patrolAgents = FindObjectsOfType<PatrolAgent>();
        foreach (var agent in patrolAgents)
        {
            agent.gameObject.SetActive(true);
            agent.ResetAgentPosition();
            aiAgentStates[agent.uniqueID] = false;
            PlayerPrefs.SetInt(agent.uniqueID, 0);
        }

        PlayerPrefs.SetInt("playerStarted", 1);
        PlayerPrefs.Save();

        SavePlayerData();
    }

    public void UpdateAIState(string aiID, bool isDead)
    {
        aiAgentStates[aiID] = isDead;
        PlayerPrefs.SetInt(aiID, isDead ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool IsAIDead(string aiID)
    {
        return aiAgentStates.ContainsKey(aiID) && aiAgentStates[aiID];
    }

    public bool IsNewGame()
    {
        return !PlayerPrefs.HasKey("playerStarted");
    }
}
