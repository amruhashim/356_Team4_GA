using System.Collections.Generic;
using UnityEngine;

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

    // Struct to hold AI state data
    private struct AIStateData
    {
        public bool isDead;
        public float health;

        public AIStateData(bool isDead, float health)
        {
            this.isDead = isDead;
            this.health = health;
        }
    }

    // Struct to hold Hostage state data
    private struct HostageStateData
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool isRescued;

        public HostageStateData(Vector3 position, Quaternion rotation, bool isRescued)
        {
            this.position = position;
            this.rotation = rotation;
            this.isRescued = isRescued;
        }
    }

    // Struct to hold Door state data
    private struct DoorStateData
    {
        public bool isOpen;

        public DoorStateData(bool isOpen)
        {
            this.isOpen = isOpen;
        }
    }

    private Dictionary<string, AIStateData> aiAgentStates = new Dictionary<string, AIStateData>();
    private Dictionary<string, HostageStateData> hostageStates = new Dictionary<string, HostageStateData>();
    private Dictionary<string, DoorStateData> doorStates = new Dictionary<string, DoorStateData>();

    public Vector3 initialSpawnPoint = new Vector3(0, 1, 0);
    public Quaternion initialRotation = Quaternion.identity;
    private bool isGameOverTriggered = false;
     private bool isMissionCompleteTriggered = false;

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
    // Check if the player's health is 0 and trigger game over if necessary
    if (currentHealth <= 0 && !isGameOverTriggered)
    {
        currentHealth = 0;
        isGameOverTriggered = true;
        Debug.Log("Player health is 0. Triggering Game Over.");
        TriggerGameOver();
    }

    // Check if all hostages are rescued and trigger mission complete if necessary
    if (AreAllHostagesRescued() && !isMissionCompleteTriggered)
    {
        isMissionCompleteTriggered = true;
        Debug.Log("All hostages rescued. Triggering Mission Complete.");
        TriggerMissionComplete();
    }
}

    private void TriggerGameOver()
    {
        // Initialize player data for a new game
        InitializeNewPlayerData();

        // Trigger the game over sequence
        GameStateManager gameStateManager = FindObjectOfType<GameStateManager>();
        if (gameStateManager != null)
        {
            gameStateManager.TriggerGameOver();
        }
    }

    private void TriggerMissionComplete()
    {
        // Initialize player data for a new game or next mission
        InitializeNewPlayerData();

        // Trigger the mission complete sequence
        GameStateManager gameStateManager = FindObjectOfType<GameStateManager>();
        if (gameStateManager != null)
        {
            gameStateManager.TriggerMissionComplete();
        }
    }

private bool AreAllHostagesRescued()
{
    foreach (var hostageState in hostageStates)
    {
        Debug.Log($"Hostage {hostageState.Key}: Rescued = {hostageState.Value.isRescued}");
        if (!hostageState.Value.isRescued)
        {
            return false;
        }
    }
    return true;
}


    public void UpdateHostageState(string hostageID, Vector3 position, Quaternion rotation, bool isRescued)
    {
        hostageStates[hostageID] = new HostageStateData(position, rotation, isRescued);
        PlayerPrefs.SetFloat(hostageID + "_posX", position.x);
        PlayerPrefs.SetFloat(hostageID + "_posY", position.y);
        PlayerPrefs.SetFloat(hostageID + "_posZ", position.z);
        PlayerPrefs.SetFloat(hostageID + "_rotX", rotation.eulerAngles.x);
        PlayerPrefs.SetFloat(hostageID + "_rotY", rotation.eulerAngles.y);
        PlayerPrefs.SetFloat(hostageID + "_rotZ", rotation.eulerAngles.z);
        PlayerPrefs.SetInt(hostageID + "_isRescued", isRescued ? 1 : 0);
        PlayerPrefs.Save();

        // Check if all hostages are rescued
        if (AreAllHostagesRescued() && !isMissionCompleteTriggered)
        {
            isMissionCompleteTriggered = true;
            TriggerMissionComplete();
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

        // Save AI states with health levels
        foreach (var kvp in aiAgentStates)
        {
            PlayerPrefs.SetInt(kvp.Key + "_isDead", kvp.Value.isDead ? 1 : 0);
            PlayerPrefs.SetFloat(kvp.Key + "_health", kvp.Value.health);
        }

        // Save hostage states
        foreach (var kvp in hostageStates)
        {
            PlayerPrefs.SetFloat(kvp.Key + "_posX", kvp.Value.position.x);
            PlayerPrefs.SetFloat(kvp.Key + "_posY", kvp.Value.position.y);
            PlayerPrefs.SetFloat(kvp.Key + "_posZ", kvp.Value.position.z);
            PlayerPrefs.SetFloat(kvp.Key + "_rotX", kvp.Value.rotation.eulerAngles.x);
            PlayerPrefs.SetFloat(kvp.Key + "_rotY", kvp.Value.rotation.eulerAngles.y);
            PlayerPrefs.SetFloat(kvp.Key + "_rotZ", kvp.Value.rotation.eulerAngles.z);
            PlayerPrefs.SetInt(kvp.Key + "_isRescued", kvp.Value.isRescued ? 1 : 0);
        }

        // Save door states
        foreach (var kvp in doorStates)
        {
            PlayerPrefs.SetInt(kvp.Key + "_isOpen", kvp.Value.isOpen ? 1 : 0);
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

            // Load AI states with health levels
            PatrolAgent[] patrolAgents = FindObjectsOfType<PatrolAgent>();
            aiAgentStates.Clear();
            foreach (var agent in patrolAgents)
            {
                string aiID = agent.uniqueID;
                bool isDead = PlayerPrefs.GetInt(aiID + "_isDead", 0) == 1;
                float health = PlayerPrefs.GetFloat(aiID + "_health", agent.maxHits);

                aiAgentStates[aiID] = new AIStateData(isDead, health);

                if (isDead)
                {
                    agent.gameObject.SetActive(false);
                }
                else
                {
                    agent.ResetHealth(health);
                }
            }

            // Load hostage states
            Hostage[] hostages = FindObjectsOfType<Hostage>();
            hostageStates.Clear();
            foreach (var hostage in hostages)
            {
                string hostageID = hostage.UniqueID;
                Vector3 position = new Vector3(
                    PlayerPrefs.GetFloat(hostageID + "_posX"),
                    PlayerPrefs.GetFloat(hostageID + "_posY"),
                    PlayerPrefs.GetFloat(hostageID + "_posZ")
                );
                Quaternion rotation = Quaternion.Euler(
                    PlayerPrefs.GetFloat(hostageID + "_rotX"),
                    PlayerPrefs.GetFloat(hostageID + "_rotY"),
                    PlayerPrefs.GetFloat(hostageID + "_rotZ")
                );
                bool isRescued = PlayerPrefs.GetInt(hostageID + "_isRescued", 0) == 1;

                hostageStates[hostageID] = new HostageStateData(position, rotation, isRescued);

                if (isRescued)
                {
                    hostage.gameObject.SetActive(false); // Disable rescued hostages
                }
                else
                {
                    hostage.transform.position = position;
                    hostage.transform.rotation = rotation;
                    hostage.gameObject.SetActive(true); // Ensure the hostage is active
                }
            }

            // Load door states
            SwingDoor[] swingDoors = FindObjectsOfType<SwingDoor>();
            doorStates.Clear();
            foreach (var door in swingDoors)
            {
                string doorID = door.doorIdentifier;
                bool isOpen = PlayerPrefs.GetInt(doorID + "_isOpen", 0) == 1;
                doorStates[doorID] = new DoorStateData(isOpen);
            }
        }
        else
        {
            InitializeNewPlayerData();
        }
    }

public int GetRescuedHostagesCount()
{
    int count = 0;
    foreach (var hostageState in hostageStates.Values)
    {
        if (hostageState.isRescued)
        {
            count++;
        }
    }
    return count;
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
        hostageStates.Clear();
        doorStates.Clear();

        PatrolAgent[] patrolAgents = FindObjectsOfType<PatrolAgent>();
        foreach (var agent in patrolAgents)
        {
            agent.gameObject.SetActive(true);
            agent.ResetAgentPosition();
            aiAgentStates[agent.uniqueID] = new AIStateData(false, agent.maxHits);
        }

        Hostage[] hostages = FindObjectsOfType<Hostage>();
        foreach (var hostage in hostages)
        {
            hostageStates[hostage.UniqueID] = new HostageStateData(hostage.transform.position, hostage.transform.rotation, false);
        }

        SwingDoor[] swingDoors = FindObjectsOfType<SwingDoor>();
        foreach (var door in swingDoors)
        {
            doorStates[door.doorIdentifier] = new DoorStateData(false); // Assume doors are closed by default
        }

        PlayerPrefs.SetInt("playerStarted", 1);
        PlayerPrefs.Save();

        SavePlayerData();
    }

    public void UpdateAIState(string aiID, bool isDead, float health)
    {
        aiAgentStates[aiID] = new AIStateData(isDead, health);
        PlayerPrefs.SetInt(aiID + "_isDead", isDead ? 1 : 0);
        PlayerPrefs.SetFloat(aiID + "_health", health);
        PlayerPrefs.Save();
    }


    // Update door state
    public void SetDoorState(string doorID, bool isOpen)
    {
        doorStates[doorID] = new DoorStateData(isOpen);
        PlayerPrefs.SetInt(doorID + "_isOpen", isOpen ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Check if a door is open
    public bool IsDoorOpen(string doorID)
    {
        if (doorStates.ContainsKey(doorID))
        {
            return doorStates[doorID].isOpen;
        }
        return false; // Default to closed if the door ID is not found
    }

    // This is the method to get the health of a specific AI
    public float GetAIHealth(string aiID)
    {
        if (aiAgentStates.ContainsKey(aiID))
        {
            return aiAgentStates[aiID].health;
        }
        return 0f; // Default to 0 if the AI ID is not found
    }

    public bool IsAIDead(string aiID)
    {
        return aiAgentStates.ContainsKey(aiID) && aiAgentStates[aiID].isDead;
    }

    public bool IsNewGame()
    {
        return !PlayerPrefs.HasKey("playerStarted");
    }
}
