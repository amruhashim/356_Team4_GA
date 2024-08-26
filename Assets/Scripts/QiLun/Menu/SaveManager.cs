using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    // Paths for saving game data
    private string jsonPathProject;
    private string jsonPathPersistent;
    private string binaryPath;

    // Paths for saving settings data
    private string settingsJsonPathProject;
    private string settingsJsonPathPersistent;
    private string settingsBinaryPath;

    // Paths for saving sensitivity data
    private string sensitivityJsonPathProject;
    private string sensitivityJsonPathPersistent;
    private string sensitivityBinaryPath;

    public bool isSavingToJson;

    // Reference to AudioMixerController
    private AudioMixerController audioMixerController;

    // References to CameraLook and DroneMovement
    private CameraLook cameraLook;
    private DroneMovement droneMovement;

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

            // Initialize AudioMixerController
            audioMixerController = GetComponent<AudioMixerController>();
            if (audioMixerController == null)
            {
                Debug.LogError("AudioMixerController not found on SaveManager. Ensure the component is attached.");
            }
        }
    }

    private void Start()
    {
        // Initialize paths
        InitializePaths();

        // Load or create settings files
        InitializeSettingsFiles();

        // Load and apply settings
        LoadAndApplyVolumeSettings();
        LoadAndApplySensitivitySettings();
    }

    // Method to assign references to CameraLook and DroneMovement
    public void SetReferences(CameraLook cameraLookInstance, DroneMovement droneMovementInstance)
    {
        cameraLook = cameraLookInstance;
        droneMovement = droneMovementInstance;
    }

    private void InitializePaths()
    {
        jsonPathProject = Application.dataPath + Path.AltDirectorySeparatorChar + "SaveGame.json";
        jsonPathPersistent = Application.persistentDataPath + Path.AltDirectorySeparatorChar + "SaveGame.json";
        binaryPath = Application.persistentDataPath + "/save_game.bin";

        settingsJsonPathProject = Application.dataPath + Path.AltDirectorySeparatorChar + "Settings.json";
        settingsJsonPathPersistent = Application.persistentDataPath + Path.AltDirectorySeparatorChar + "Settings.json";
        settingsBinaryPath = Application.persistentDataPath + "/settings.bin";

        sensitivityJsonPathProject = Application.dataPath + Path.AltDirectorySeparatorChar + "Sensitivity.json";
        sensitivityJsonPathPersistent = Application.persistentDataPath + Path.AltDirectorySeparatorChar + "Sensitivity.json";
        sensitivityBinaryPath = Application.persistentDataPath + "/sensitivity.bin";

        // Debugging the paths
        Debug.Log("Paths initialized:");
        Debug.Log($"jsonPathProject: {jsonPathProject}");
        Debug.Log($"jsonPathPersistent: {jsonPathPersistent}");
        Debug.Log($"binaryPath: {binaryPath}");
        Debug.Log($"settingsJsonPathProject: {settingsJsonPathProject}");
        Debug.Log($"settingsJsonPathPersistent: {settingsJsonPathPersistent}");
        Debug.Log($"settingsBinaryPath: {settingsBinaryPath}");
        Debug.Log($"sensitivityJsonPathProject: {sensitivityJsonPathProject}");
        Debug.Log($"sensitivityJsonPathPersistent: {sensitivityJsonPathPersistent}");
        Debug.Log($"sensitivityBinaryPath: {sensitivityBinaryPath}");
    } 

    private void InitializeSettingsFiles()
    {
        // Check if settings file exists, if not create it with default settings
        if (!File.Exists(settingsJsonPathPersistent))
        {
            SaveVolumeSettings(1.0f, 1.0f, 1.0f);
        }

        // Check if sensitivity file exists, if not create it with default settings
        if (!File.Exists(sensitivityJsonPathPersistent))
        {
            SaveSensitivitySettings(new Vector2(1.0f, 1.0f), 2.5f); // Default drone sensitivity of 2.5
        }
    }

    #region Save and Load Game Data
    public void SaveGame()
    {
        PlayerState.Instance.SavePlayerData();

        Debug.Log("Saving game data...");
        string jsonData = JsonUtility.ToJson(PlayerState.Instance);
        File.WriteAllText(jsonPathProject, jsonData);
        File.WriteAllText(jsonPathPersistent, jsonData);

        using (FileStream fileStream = new FileStream(binaryPath, FileMode.Create))
        {
            using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
            {
                binaryWriter.Write(jsonData);
            }
        }

        Debug.Log("Game saved using multiple files");
    }

    public void StartLoadedGame(string sceneName)
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayerState.Instance.LoadPlayerData();
        AssignSceneSpecificReferences();  // Assign references for CameraLook and DroneMovement when the scene loads
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void StartNewGame(string sceneName)
    {
        PlayerPrefs.DeleteAll();
        SceneManager.sceneLoaded += OnNewGameSceneLoaded;
        SceneManager.LoadScene(sceneName);
    }

    private void OnNewGameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayerState.Instance.InitializeNewPlayerData();
        AssignSceneSpecificReferences();  // Assign references for CameraLook and DroneMovement when the scene loads
        SceneManager.sceneLoaded -= OnNewGameSceneLoaded;
    }
    #endregion

    #region Volume Settings
    [System.Serializable]
    public class VolumeSettings
    {
        public float music;
        public float effects;
        public float master;
    }

    public void SaveVolumeSettings(float master, float music, float effects)
    {
        VolumeSettings volumeSettings = new VolumeSettings
        {
            master = master,
            music = music,
            effects = effects,
        };

        string settingsJson = JsonUtility.ToJson(volumeSettings);

        Debug.Log($"Saving volume settings: Music: {music}, Effects: {effects}, Master: {master}");
        File.WriteAllText(settingsJsonPathProject, settingsJson);
        File.WriteAllText(settingsJsonPathPersistent, settingsJson);

        using (FileStream fileStream = new FileStream(settingsBinaryPath, FileMode.Create))
        {
            using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
            {
                binaryWriter.Write(settingsJson);
            }
        }

        Debug.Log("Volume settings saved using multiple files");
    }

    public VolumeSettings LoadVolumeSettings()
    {
        if (File.Exists(settingsJsonPathPersistent))
        {
            string settingsJson = File.ReadAllText(settingsJsonPathPersistent);
            return JsonUtility.FromJson<VolumeSettings>(settingsJson);
        }
        else
        {
            Debug.Log("No settings file found, using default settings.");
            return new VolumeSettings { music = 1.0f, effects = 1.0f, master = 1.0f };
        }
    }

    public void LoadAndApplyVolumeSettings()
    {
        VolumeSettings volumeSettings = LoadVolumeSettings();
        if (audioMixerController != null)
        {
            audioMixerController.SetMasterVolume(volumeSettings.master);
            audioMixerController.SetMusicVolume(volumeSettings.music);
            audioMixerController.SetEffectsVolume(volumeSettings.effects);
        }
        else
        {
            Debug.LogError("AudioMixerController is null. Cannot apply volume settings.");
        }
    }
    #endregion

    #region Sensitivity Settings
    [System.Serializable]
    public class SensitivitySettings
    {
        public Vector2 mouseSensitivity;
        public float droneSensitivity;

        public SensitivitySettings(Vector2 mouseSensitivity, float droneSensitivity)
        {
            this.mouseSensitivity = mouseSensitivity;
            this.droneSensitivity = droneSensitivity;
        }
    }

    public void SaveSensitivitySettings(Vector2 mouseSensitivity, float droneSensitivity)
    {
        SensitivitySettings sensitivitySettings = new SensitivitySettings(mouseSensitivity, droneSensitivity);

        string sensitivityJson = JsonUtility.ToJson(sensitivitySettings);

        Debug.Log($"Saving sensitivity settings: Mouse X: {mouseSensitivity.x}, Mouse Y: {mouseSensitivity.y}, Drone: {droneSensitivity}");
        File.WriteAllText(sensitivityJsonPathProject, sensitivityJson);
        File.WriteAllText(sensitivityJsonPathPersistent, sensitivityJson);

        using (FileStream fileStream = new FileStream(sensitivityBinaryPath, FileMode.Create))
        {
            using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
            {
                binaryWriter.Write(sensitivityJson);
            }
        }

        Debug.Log("Sensitivity settings saved using multiple files");
    }

    public SensitivitySettings LoadSensitivitySettings()
    {
        if (File.Exists(sensitivityJsonPathPersistent))
        {
            string sensitivityJson = File.ReadAllText(sensitivityJsonPathPersistent);
            Debug.Log($"Loaded sensitivity settings: {sensitivityJson}");
            return JsonUtility.FromJson<SensitivitySettings>(sensitivityJson);
        }
        else
        {
            Debug.Log("No sensitivity file found, using default settings.");
            return new SensitivitySettings(new Vector2(1.0f, 1.0f), 2.5f); // Default drone sensitivity of 2.5
        }
    }

    public void LoadAndApplySensitivitySettings()
    {
        SensitivitySettings sensitivitySettings = LoadSensitivitySettings();
        Debug.Log($"Applying sensitivity settings: Mouse X: {sensitivitySettings.mouseSensitivity.x}, Mouse Y: {sensitivitySettings.mouseSensitivity.y}, Drone: {sensitivitySettings.droneSensitivity}");

        // Apply sensitivity if references are set
        if (cameraLook != null)
        {
            cameraLook.SetSensitivity(sensitivitySettings.mouseSensitivity);
        }
        else
        {
            Debug.LogWarning("CameraLook reference is null. Mouse sensitivity settings not applied.");
        }

        if (droneMovement != null)
        {
            droneMovement.SetSensitivity(sensitivitySettings.droneSensitivity);
        }
        else
        {
            Debug.LogWarning("DroneMovement reference is null. Drone sensitivity settings not applied.");
        }
    }
    #endregion

    // Assign scene-specific references for CameraLook and DroneMovement
    private void AssignSceneSpecificReferences()
    {
        if (SceneManager.GetActiveScene().name == "YourSceneName") // Replace "YourSceneName" with the actual scene name
        {
            cameraLook = FindObjectOfType<CameraLook>();
            droneMovement = FindObjectOfType<DroneMovement>();
        }
    }
}
