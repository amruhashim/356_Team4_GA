using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string sceneName;
    public Button LoadGameBTN;

    // Paths to check and delete save game files
    private string jsonPathProject;
    private string jsonPathPersistent;
    private string binaryPath;

    private void Start()
    {
        // Define the paths where your save files might be stored
        jsonPathProject = Application.dataPath + Path.AltDirectorySeparatorChar + "SaveGame.json";
        jsonPathPersistent = Application.persistentDataPath + Path.AltDirectorySeparatorChar + "SaveGame.json";
        binaryPath = Application.persistentDataPath + Path.AltDirectorySeparatorChar + "save_game.bin";

        // Initially hide the Load Game button
        LoadGameBTN.gameObject.SetActive(false);

        // Add the LoadGame button listener
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
        bool jsonExistsInProject = File.Exists(jsonPathProject);
        bool jsonExistsInPersistent = File.Exists(jsonPathPersistent);
        bool binaryExists = File.Exists(binaryPath);

        Debug.Log($"Checking save files: JSON in Project exists: {jsonExistsInProject}, JSON in Persistent exists: {jsonExistsInPersistent}, Binary exists: {binaryExists}");

        return jsonExistsInProject || jsonExistsInPersistent || binaryExists;
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

        // Delete the JSON save file in project path if it exists
        if (File.Exists(jsonPathProject))
        {
            File.Delete(jsonPathProject);
            Debug.Log("Project SaveGame.json file deleted.");
        }

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

        Debug.Log("All save game files cleared.");
    }
}
