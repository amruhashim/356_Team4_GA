using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class GameStateManager : MonoBehaviour
{
    public Image gameOverImage;        // Reference to the Game Over UI Image
    public Image missionCompleteImage; // Reference to the Mission Complete UI Image
    public GameObject backgroundImage; // Reference to the background image object

    // Paths to check and delete save game files
    private string jsonPathProject;
    private string jsonPathPersistent;
    private string binaryPath;

    private InGameMenu inGameMenu;

    private void Start()
    {
        // Define the paths where your save files might be stored
        jsonPathProject = Application.dataPath + Path.AltDirectorySeparatorChar + "SaveGame.json";
        jsonPathPersistent = Application.persistentDataPath + Path.AltDirectorySeparatorChar + "SaveGame.json";
        binaryPath = Application.persistentDataPath + Path.AltDirectorySeparatorChar + "save_game.bin";

        // Make sure the Game Over and Mission Complete images are initially hidden
        if (gameOverImage != null)
        {
            gameOverImage.gameObject.SetActive(false);
        }
        if (missionCompleteImage != null)
        {
            missionCompleteImage.gameObject.SetActive(false);
        }

        // Ensure the background image is initially hidden
        if (backgroundImage != null)
        {
            backgroundImage.SetActive(false);
        }

        // Find the InGameMenu component, even if it's on an inactive GameObject
        GameObject inGameMenuObject = GameObject.Find("InGameMenu");
        if (inGameMenuObject != null)
        {
            inGameMenu = inGameMenuObject.GetComponent<InGameMenu>();
        }
        if (inGameMenu == null)
        {
            Debug.LogError("InGameMenu component not found in the scene, or GameObject is inactive!");
        }
    }

    public void TriggerGameOver()
    {
        StartCoroutine(ShowGameOverSequence());
    }

    public void TriggerMissionComplete()
    {
        StartCoroutine(ShowMissionCompleteSequence());
    }

    private IEnumerator ShowGameOverSequence()
    {
        // Show the background image
        if (backgroundImage != null)
        {
            backgroundImage.SetActive(true);
        }

        // Show the Game Over image
        if (gameOverImage != null)
        {
            gameOverImage.gameObject.SetActive(true);
        }

        // Unlock the cursor and make it visible
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Wait for 3 seconds
        yield return new WaitForSeconds(3f);

        // Clear all saved data
        ClearAllSaveData();

        // Go back to the main menu
        if (inGameMenu != null)
        {
            inGameMenu.BackToMainMenu();
        }
        else
        {
            Debug.LogError("InGameMenu reference is missing!");
        }
    }

    private IEnumerator ShowMissionCompleteSequence()
    {
        // Show the background image
        if (backgroundImage != null)
        {
            backgroundImage.SetActive(true);
        }

        // Show the Mission Complete image
        if (missionCompleteImage != null)
        {
            missionCompleteImage.gameObject.SetActive(true);
        }

        // Unlock the cursor and make it visible
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Wait for 3 seconds
        yield return new WaitForSeconds(3f);

        // Clear all saved data
        ClearAllSaveData();

        // Go back to the main menu
        if (inGameMenu != null)
        {
            inGameMenu.BackToMainMenu();
        }
        else
        {
            Debug.LogError("InGameMenu reference is missing!");
        }
    }

    // Method to clear all saved data
    private void ClearAllSaveData()
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
