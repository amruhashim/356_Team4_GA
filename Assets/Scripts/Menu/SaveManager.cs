using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProtoBuf;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    // Paths for saving game data
    private string jsonSaveGamePath;
    private string binarySaveGamePath;

    // Paths for saving settings data
    private string volumeSettingsProtoPath;
    private string sensitivitySettingsProtoPath;

    // Path for saving save status data
    private string saveStatusProtoPath;

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

            // Initialize paths
            InitializePaths();

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
        // Load or create settings files
        InitializeSettingsFiles();

        // Load and apply settings
        LoadAndApplyVolumeSettings();
        LoadAndApplySensitivitySettings();
    }

    private void InitializePaths()
    {
        jsonSaveGamePath = Path.Combine(Application.persistentDataPath, "saveGame.json");
        binarySaveGamePath = Path.Combine(Application.persistentDataPath, "saveGame.bin");

        volumeSettingsProtoPath = Path.Combine(Application.persistentDataPath, "volumeSettings.proto");
        sensitivitySettingsProtoPath = Path.Combine(Application.persistentDataPath, "sensitivitySettings.proto");

        // Path for saving the save status
        saveStatusProtoPath = Path.Combine(Application.persistentDataPath, "saveStatus.proto");
    }

    private void InitializeSettingsFiles()
    {
        // Check if volume settings file exists, if not create it with default settings
        if (!File.Exists(volumeSettingsProtoPath))
        {
            SaveVolumeSettings(10.0f, 6.0f, 8.0f); // Default values: master = 10.0, music = 6.0, effects = 8.0
        }

        // Check if sensitivity settings file exists, if not create it with default settings
        if (!File.Exists(sensitivitySettingsProtoPath))
        {
            SaveSensitivitySettings(new SerializableVector2(2.0f, 2.0f), 2.5f); // Default drone sensitivity of 2.5
        }
    }

    #region Save Status Management
    [ProtoContract]
    public class SaveStatus
    {
        [ProtoMember(1)]
        public bool SaveFileExists;
    }

    public void UpdateSaveStatus(bool exists)
    {
        SaveStatus saveStatus = new SaveStatus
        {
            SaveFileExists = exists
        };

        using (FileStream file = File.Create(saveStatusProtoPath))
        {
            Serializer.Serialize(file, saveStatus);
        }

        Debug.Log($"Save status updated using protobuf-net at: {saveStatusProtoPath}");
    }

    public bool LoadSaveStatus()
    {
        if (File.Exists(saveStatusProtoPath))
        {
            using (FileStream file = File.OpenRead(saveStatusProtoPath))
            {
                SaveStatus saveStatus = Serializer.Deserialize<SaveStatus>(file);
                return saveStatus.SaveFileExists;
            }
        }

        // Return false as the default if the file doesn't exist or is not initialized
        Debug.LogWarning("Save status file not found. Assuming no save file exists.");
        return false;
    }
    #endregion

    #region Save and Load Game Data
    public void SaveGame()
    {
        PlayerState.Instance.SavePlayerData();

        Debug.Log("Saving game data...");
        string jsonData = JsonUtility.ToJson(PlayerState.Instance);

        // Save Binary file
        FileInfo binaryFileInfo = new FileInfo(binarySaveGamePath);
        using (FileStream fileStream = binaryFileInfo.Create())
        {
            using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
            {
                binaryWriter.Write(jsonData);
            }
        }

        Debug.Log($"Game saved using binary file at: {binarySaveGamePath}");
        
        // Update save status to true
        UpdateSaveStatus(true);
    }

    public void StartLoadedGame(string sceneName)
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayerState.Instance.LoadPlayerData();
        AssignSceneSpecificReferences();
        LoadAndApplySensitivitySettings();
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

    public void ClearSaveGame()
    {
        // Delete save files
        if (File.Exists(jsonSaveGamePath)) File.Delete(jsonSaveGamePath);
        if (File.Exists(binarySaveGamePath)) File.Delete(binarySaveGamePath);

        // Update save status to false
        UpdateSaveStatus(false);

        Debug.Log("Save game cleared.");
    }
    #endregion

    #region Volume Settings
    [ProtoContract]
    public class VolumeSettings
    {
        [ProtoMember(1)]
        public float master;
        [ProtoMember(2)]
        public float music;
        [ProtoMember(3)]
        public float effects;
    }

    public void SaveVolumeSettings(float master, float music, float effects)
    {
        VolumeSettings volumeSettings = new VolumeSettings
        {
            master = master,
            music = music,
            effects = effects,
        };

        // Save using protobuf-net
        using (FileStream file = File.Create(volumeSettingsProtoPath))
        {
            Serializer.Serialize(file, volumeSettings);
        }

        Debug.Log($"Volume settings saved using protobuf-net at: {volumeSettingsProtoPath}");
    }

    public VolumeSettings LoadVolumeSettings()
    {
        if (File.Exists(volumeSettingsProtoPath))
        {
            using (FileStream file = File.OpenRead(volumeSettingsProtoPath))
            {
                return Serializer.Deserialize<VolumeSettings>(file);
            }
        }
        else
        {
            Debug.Log("No volume settings file found, using default settings.");
            return new VolumeSettings { master = 10.0f, music = 6.0f, effects = 8.0f }; // Default values: master = 10.0, music = 6.0, effects = 8.0
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

    [ProtoContract]
    public class SerializableVector2
    {
        [ProtoMember(1)]
        public float x;
        [ProtoMember(2)]
        public float y;

        public SerializableVector2() { }

        public SerializableVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public SerializableVector2(Vector2 vector)
        {
            this.x = vector.x;
            this.y = vector.y;
        }

        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }
    }

    [ProtoContract]
    public class SensitivitySettings
    {
        [ProtoMember(1)]
        public SerializableVector2 mouseSensitivity;
        [ProtoMember(2)]
        public float droneSensitivity;

        public SensitivitySettings() { }

        public SensitivitySettings(SerializableVector2 mouseSensitivity, float droneSensitivity)
        {
            this.mouseSensitivity = mouseSensitivity;
            this.droneSensitivity = droneSensitivity;
        }
    }

    public void SaveSensitivitySettings(SerializableVector2 mouseSensitivity, float droneSensitivity)
    {
        SensitivitySettings sensitivitySettings = new SensitivitySettings(mouseSensitivity, droneSensitivity);

        // Save using protobuf-net
        using (FileStream file = File.Create(sensitivitySettingsProtoPath))
        {
            Serializer.Serialize(file, sensitivitySettings);
        }

        Debug.Log($"Sensitivity settings saved using protobuf-net at: {sensitivitySettingsProtoPath}");
    }

    public SensitivitySettings LoadSensitivitySettings()
    {
        if (File.Exists(sensitivitySettingsProtoPath))
        {
            using (FileStream file = File.OpenRead(sensitivitySettingsProtoPath))
            {
                return Serializer.Deserialize<SensitivitySettings>(file);
            }
        }
        else
        {
            Debug.Log("No sensitivity settings file found, using default settings.");
            return new SensitivitySettings(new SerializableVector2(2.0f, 2.0f), 2.5f); // Default drone sensitivity of 2.5
        }
    }

    public void LoadAndApplySensitivitySettings()
    {
        SensitivitySettings sensitivitySettings = LoadSensitivitySettings();
        Debug.Log($"Applying sensitivity settings: Mouse X: {sensitivitySettings.mouseSensitivity.x}, Mouse Y: {sensitivitySettings.mouseSensitivity.y}, Drone: {sensitivitySettings.droneSensitivity}");

        // Apply sensitivity if references are set
        if (cameraLook != null)
        {
            cameraLook.SetSensitivity(sensitivitySettings.mouseSensitivity.ToVector2());
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
        cameraLook = FindObjectOfType<CameraLook>();
        droneMovement = FindObjectOfType<DroneMovement>();
    }
}
