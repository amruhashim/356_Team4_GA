using UnityEngine;
using System.IO;
using ProtoBuf;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    // Paths for saving game data
    private string jsonPathPersistent;
    private string binaryPath;

    // Paths for saving settings data
    private string settingsProtoPath;

    // Paths for saving sensitivity data
    private string sensitivityProtoPath;

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
        jsonPathPersistent = Path.Combine(Application.persistentDataPath, "SaveGame.json");
        binaryPath = Path.Combine(Application.persistentDataPath, "save_game.bin");

        settingsProtoPath = Path.Combine(Application.persistentDataPath, "settings.proto");
        sensitivityProtoPath = Path.Combine(Application.persistentDataPath, "sensitivity.proto");

        // Debugging the paths
        Debug.Log("Paths initialized:");
        Debug.Log($"binaryPath: {binaryPath}");
        Debug.Log($"settingsProtoPath: {settingsProtoPath}");
        Debug.Log($"sensitivityProtoPath: {sensitivityProtoPath}");
    }

    private void InitializeSettingsFiles()
    {
        // Check if settings file exists, if not create it with default settings
        if (!File.Exists(settingsProtoPath))
        {
            SaveVolumeSettings(1.0f, 1.0f, 1.0f);
        }

        // Check if sensitivity file exists, if not create it with default settings
        if (!File.Exists(sensitivityProtoPath))
        {
            SaveSensitivitySettings(new SerializableVector2(1.0f, 1.0f), 2.5f); // Default drone sensitivity of 2.5
        }
    }

    #region Save and Load Game Data
    public void SaveGame()
    {
        PlayerState.Instance.SavePlayerData();

        Debug.Log("Saving game data...");
        string jsonData = JsonUtility.ToJson(PlayerState.Instance);

        // Save Binary file
        FileInfo binaryFileInfo = new FileInfo(binaryPath);
        using (FileStream fileStream = binaryFileInfo.Create())
        {
            using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
            {
                binaryWriter.Write(jsonData);
            }
        }

        Debug.Log("Game saved using binary file");
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
    #endregion

    #region Volume Settings
    [ProtoContract]
    public class VolumeSettings
    {
        [ProtoMember(1)]
        public float music;
        [ProtoMember(2)]
        public float effects;
        [ProtoMember(3)]
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

        // Save using protobuf-net
        using (FileStream file = File.Create(settingsProtoPath))
        {
            Serializer.Serialize(file, volumeSettings);
        }

        Debug.Log("Volume settings saved using protobuf-net");
    }

    public VolumeSettings LoadVolumeSettings()
    {
        if (File.Exists(settingsProtoPath))
        {
            using (FileStream file = File.OpenRead(settingsProtoPath))
            {
                return Serializer.Deserialize<VolumeSettings>(file);
            }
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
        using (FileStream file = File.Create(sensitivityProtoPath))
        {
            Serializer.Serialize(file, sensitivitySettings);
        }

        Debug.Log("Sensitivity settings saved using protobuf-net");
    }

    public SensitivitySettings LoadSensitivitySettings()
    {
        if (File.Exists(sensitivityProtoPath))
        {
            using (FileStream file = File.OpenRead(sensitivityProtoPath))
            {
                return Serializer.Deserialize<SensitivitySettings>(file);
            }
        }
        else
        {
            Debug.Log("No sensitivity file found, using default settings.");
            return new SensitivitySettings(new SerializableVector2(1.0f, 1.0f), 2.5f); // Default drone sensitivity of 2.5
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

