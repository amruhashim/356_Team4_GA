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

    // Paths to check and delete settings files
    private string settingsJsonPathPersistent;
    private string settingsBinaryPath;

    // Paths to check and delete sensitivity files
    private string sensitivityJsonPathPersistent;
    private string sensitivityBinaryPath;

    private void Start()
    {
        // Define the paths where your save, settings, and sensitivity files might be stored
        jsonPathPersistent = Path.Combine(Application.persistentDataPath, "SaveGame.json");
        binaryPath = Path.Combine(Application.persistentDataPath, "save_game.bin");

        settingsJsonPathPersistent = Path.Combine(Application.persistentDataPath, "Settings.json");
        settingsBinaryPath = Path.Combine(Application.persistentDataPath, "settings.bin");

        sensitivityJsonPathPersistent = Path.Combine(Application.persistentDataPath, "Sensitivity.json");
        sensitivityBinaryPath = Path.Combine(Application.persistentDataPath, "sensitivity.bin");

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

    // Method to check if a save file exists
    private bool SaveFileExists()
    {
        bool jsonExistsInPersistent = File.Exists(jsonPathPersistent);
        bool binaryExists = File.Exists(binaryPath);

        bool settingsJsonExists = File.Exists(settingsJsonPathPersistent);
        bool sensitivityJsonExists = File.Exists(sensitivityJsonPathPersistent);

        Debug.Log($"Checking save files: JSON in Persistent exists: {jsonExistsInPersistent}, Binary exists: {binaryExists}, Settings JSON exists: {settingsJsonExists}, Sensitivity JSON exists: {sensitivityJsonExists}");

        return jsonExistsInPersistent || binaryExists || settingsJsonExists || sensitivityJsonExists;
    }

    // Method to update the state of the Load Game button
    public void UpdateLoadGameButtonState()
    {
        if (SaveFileExists())
        {
            Debug.Log("Save files found. Enabling Load Game button.");
            LoadGameBTN.gameObject.SetActive(true);  // Show the Load Game button if save files exist
        }
        else
        {
            Debug.Log("No save files found. Keeping Load Game button hidden.");
            LoadGameBTN.gameObject.SetActive(false);  // Hide the Load Game button if no save files exist
        }
    }

    // Method to clear all saved data
    public void ClearAllSaveData()
    {
        // Delete PlayerPrefs data
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("All PlayerPrefs data cleared.");

        // Delete the JSON save file in persistent path if it exists
        if (File.Exists(jsonPathPersistent))
        {
            File.Delete(jsonPathPersistent);
            Debug.Log("Persistent SaveGame.json file deleted.");
        }

        // Delete the binary save file if it exists
        if (File.Exists(binaryPath))
        {
            File.Delete(binaryPath);
            Debug.Log("save_game.bin file deleted.");
        }
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
