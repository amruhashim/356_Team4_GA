using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    // Paths for saving game data
    private string jsonPathProject;
    private string jsonPathPersistent;
    private string binaryPath;
    private string settingsPath; // Path for saving settings data

    public bool isSavingToJson;

    // Reference to AudioMixerController
    private AudioMixerController audioMixerController;

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
            audioMixerController = GetComponent<AudioMixerController>(); // Direct reference to the attached AudioMixerController
        }
    }

    private void Start()
    {
        jsonPathProject = Application.dataPath + Path.AltDirectorySeparatorChar + "SaveGame.json";
        jsonPathPersistent = Application.persistentDataPath + Path.AltDirectorySeparatorChar + "SaveGame.json";
        binaryPath = Application.persistentDataPath + "/save_game.bin";
        settingsPath = Application.persistentDataPath + "/settings.json"; // Set the path for the settings file

        LoadAndApplyVolumeSettings();
    }

    #region Save and Load Game Data
    public void SaveGame()
    {
        PlayerState.Instance.SavePlayerData();

        // Log the file paths to the console
        Debug.Log("Saving game data...");
        Debug.Log($"JSON Path (Project): {jsonPathProject}");
        Debug.Log($"JSON Path (Persistent): {jsonPathPersistent}");
        Debug.Log($"Binary Path: {binaryPath}");

        // Save to JSON file in project directory
        string jsonData = JsonUtility.ToJson(PlayerState.Instance);
        File.WriteAllText(jsonPathProject, jsonData);

        // Save to JSON file in persistent data path
        File.WriteAllText(jsonPathPersistent, jsonData);

        // Save to binary file in persistent data path
        using (FileStream fileStream = new FileStream(binaryPath, FileMode.Create))
        {
            using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
            {
                binaryWriter.Write(jsonData);
            }
        }

        Debug.Log("Game saved using files");
    }

    public void StartLoadedGame(string sceneName)
    {
        SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to scene loaded event
        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayerState.Instance.LoadPlayerData();
        SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe to avoid memory leaks
    }

    public void StartNewGame(string sceneName)
    {
        PlayerPrefs.DeleteAll(); // Clear all PlayerPrefs, or selectively delete specific keys if needed
        SceneManager.sceneLoaded += OnNewGameSceneLoaded;
        SceneManager.LoadScene(sceneName);
    }

    private void OnNewGameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayerState.Instance.InitializeNewPlayerData();
        SceneManager.sceneLoaded -= OnNewGameSceneLoaded; // Unsubscribe to avoid memory leaks
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

    public void SaveVolumeSettings(float music, float effects, float master)
    {
        VolumeSettings volumeSettings = new VolumeSettings
        {
            music = music,
            effects = effects,
            master = master
        };

        string settingsJson = JsonUtility.ToJson(volumeSettings);
        File.WriteAllText(settingsPath, settingsJson); // Save settings to a separate file
        Debug.Log("Volume settings saved to file.");
    }

    public VolumeSettings LoadVolumeSettings()
    {
        if (File.Exists(settingsPath))
        {
            string settingsJson = File.ReadAllText(settingsPath);
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
        audioMixerController.SetMasterVolume(volumeSettings.master);
        audioMixerController.SetMusicVolume(volumeSettings.music);
        audioMixerController.SetEffectsVolume(volumeSettings.effects);
    }
    #endregion
}
