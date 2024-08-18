using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; set; }

    // Json & Binary paths
    private string jsonPathProject;
    private string jsonPathPersistent;
    private string binaryPath;

    public bool isSavingToJson;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Ensure SaveManager persists across scenes
        }
    }

    private void Start()
    {
        jsonPathProject = Application.dataPath + Path.AltDirectorySeparatorChar + "SaveGame.json";
        jsonPathPersistent = Application.persistentDataPath + Path.AltDirectorySeparatorChar + "SaveGame.json";
        binaryPath = Application.persistentDataPath + "/save_game.bin";
    }

    #region Save and Load Game Data
    public void SaveGame()
    {
        PlayerState.Instance.SavePlayerData();
        Debug.Log("Game saved using PlayerPrefs");
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
        SceneManager.LoadScene(sceneName);
        StartCoroutine(DelayedNewGame());
    }

    private IEnumerator DelayedLoading()
    {
        yield return new WaitForSeconds(0.5f);
        PlayerState.Instance.LoadPlayerData();
    }

    private IEnumerator DelayedNewGame()
    {
        yield return new WaitForSeconds(0.5f);
        PlayerState.Instance.InitializeNewPlayerData();
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

        PlayerPrefs.SetString("Volume", JsonUtility.ToJson(volumeSettings));
        PlayerPrefs.Save();
        Debug.Log("Volume settings saved");
    }

    public VolumeSettings LoadVolumeSettings()
    {
        if (PlayerPrefs.HasKey("Volume"))
        {
            return JsonUtility.FromJson<VolumeSettings>(PlayerPrefs.GetString("Volume"));
        }
        else
        {
            return new VolumeSettings { music = 1.0f, effects = 1.0f, master = 1.0f };
        }
    }
    #endregion
}
