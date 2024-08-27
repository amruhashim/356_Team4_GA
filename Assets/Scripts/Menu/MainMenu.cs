using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string sceneName;
    public Button LoadGameBTN;

    // Paths to check and delete save game files
    private string jsonPathPersistent;
    private string binaryPath;

    // Paths to check and delete settings files (using Protobuf now)
    private string settingsProtoPath;

    // Paths to check and delete sensitivity files (using Protobuf now)
    private string sensitivityProtoPath;

    private void Awake()
    {
        settingsProtoPath = Path.Combine(Application.persistentDataPath, "settings.proto");
        sensitivityProtoPath = Path.Combine(Application.persistentDataPath, "sensitivity.proto");

        Debug.Log("Paths initialized in Awake:");
        Debug.Log($"jsonPathPersistent: {jsonPathPersistent}");
        Debug.Log($"binaryPath: {binaryPath}");
        Debug.Log($"settingsProtoPath: {settingsProtoPath}");
        Debug.Log($"sensitivityProtoPath: {sensitivityProtoPath}");
    }

    private void Start()
    {
        // Load and apply settings and sensitivity
        LoadAndApplySettingsAndSensitivity();

        // Initially hide the Load Game button
        LoadGameBTN.gameObject.SetActive(false);

        // Add the Load Game button listener
        LoadGameBTN.onClick.AddListener(() =>
        {
            SaveManager.Instance.StartLoadedGame(sceneName);  // Pass the sceneName from MainMenu
        });

        // Check and update the Load Game button state
        UpdateLoadGameButtonState();
    }

    private void OnEnable()
    {
        // Ensure the Load Game button state is refreshed when the MainMenu is enabled
        Debug.Log("MainMenu enabled, updating Load Game button state.");
        UpdateLoadGameButtonState();
    }

    public void NewGame()
    {
        ClearAllSaveData();  // Clear previous save data
        SaveManager.Instance.StartNewGame(sceneName);  // Start a new game
    }

    public void ExitGame()
    {
        Debug.Log("Quitting Game");
        Application.Quit();
    }

    // Method to update the state of the Load Game button
    public void UpdateLoadGameButtonState()
    {
        // Check if the save file exists by using the boolean stored in SaveManager
        bool saveFileExists = SaveManager.Instance.LoadSaveStatus();

        // Update the Load Game button based on the save status
        LoadGameBTN.gameObject.SetActive(saveFileExists);

        Debug.Log(saveFileExists ? "Save file exists. Enabling Load Game button." : "No save file exists. Keeping Load Game button hidden.");
    }

    // Method to clear all saved data
    public void ClearAllSaveData()
    {
        // Clear the save data using SaveManager, which updates the save status to false
        SaveManager.Instance.ClearSaveGame();
    }

    // Method to load and apply settings and sensitivity data
    private void LoadAndApplySettingsAndSensitivity()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.LoadAndApplyVolumeSettings();   // Load and apply volume settings
            SaveManager.Instance.LoadAndApplySensitivitySettings();   // Load and apply sensitivity settings
        }
        else
        {
            Debug.LogError("SaveManager instance is not found. Settings and sensitivity could not be loaded.");
        }
    }
}
