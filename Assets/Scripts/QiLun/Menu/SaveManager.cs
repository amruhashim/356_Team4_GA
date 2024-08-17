using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{

    public static SaveManager Instance { get; set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        DontDestroyOnLoad(gameObject);
    }


    //Json Project Save Path
    string jsonPathProject;
    //Json External/Real Save Path
    string jsonPathPersistent;
    //Binary Save Path

    string binaryPath;



    public bool isSavingToJson;

    private void Start()
    {
        jsonPathProject = Application.dataPath + Path.AltDirectorySeparatorChar + "SaveGame.json";
        jsonPathPersistent = Application.persistentDataPath + Path.AltDirectorySeparatorChar + "SaveGame.json";
        binaryPath = Application.persistentDataPath + "/save_game.bin";
    }


    #region || ---------- General Section ---------- ||


    #region || ---------- Saving ---------- ||
    public void SaveGame()
    {
        AllGameData data = new AllGameData();

        data.playerData = GetPlayerData();

        SavingTypeSwitch(data);
    }

    private PlayerData GetPlayerData()
    {
        float[] playerStats = new float[3];
        playerStats[0] = PlayerState.Instance.currentHealth;

        float[] playerPosAndRot = new float[6];
        playerPosAndRot[0] = PlayerState.Instance.transform.position.x;
        playerPosAndRot[1] = PlayerState.Instance.transform.position.y;
        playerPosAndRot[2] = PlayerState.Instance.transform.position.z;

        playerPosAndRot[3] = PlayerState.Instance.transform.rotation.eulerAngles.x;
        playerPosAndRot[4] = PlayerState.Instance.transform.rotation.eulerAngles.y;
        playerPosAndRot[5] = PlayerState.Instance.transform.rotation.eulerAngles.z;

        return new PlayerData(playerStats, playerPosAndRot);
    }


    public void SavingTypeSwitch(AllGameData gameData)
    {
        if (isSavingToJson)
        {
            SaveGameDataToJsonFile(gameData);
        }
        else
        {
            SaveGameDataToBinaryFile(gameData);
        }
    }
    #endregion

    #region || ---------- Loading ---------- ||

    public AllGameData LoadingTypeSwitch()
    {
        if(isSavingToJson)
        {
            AllGameData gameData = LoadGameDataFromJsonFile();
            return gameData;


        }
        else
        {
            AllGameData gameData = LoadGameDataFromBinaryFile();
            return gameData;
        }
    }

    public void LoadGame()
    {
        //Player Data
        SetPlayerData(LoadingTypeSwitch().playerData);

        //Environment Data
        //setEnvironment
    }

    private void SetPlayerData(PlayerData playerData)
    {
        // Setting Player Stats
        PlayerState.Instance.currentHealth = playerData.playerStats[0];


        // Setting Player Position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 loadedPosition = new Vector3(
                playerData.playerPositionAndRotation[0],
                playerData.playerPositionAndRotation[1],
                playerData.playerPositionAndRotation[2]
            );

            Debug.Log("Loaded position: " + loadedPosition);

            player.transform.position = loadedPosition;

            // Setting Player Rotation
            Vector3 loadedRotation = new Vector3(
                playerData.playerPositionAndRotation[3],
                playerData.playerPositionAndRotation[4],
                playerData.playerPositionAndRotation[5]
            );

            Debug.Log("Loaded rotation: " + loadedRotation);

            Quaternion loadedQuaternion = Quaternion.Euler(loadedRotation);

            player.transform.rotation = loadedQuaternion;
        }
        else
        {
            Debug.LogError("Player object not found!");
        }
    }


    public void StartLoadedGame(string sceneName)
    {
        SceneManager.LoadScene(sceneName);

        StartCoroutine(DelayedLoading());
    }

    private IEnumerator DelayedLoading()
    {
        yield return new WaitForSeconds(1f);

        LoadGame();
    }
    #endregion



    #endregion

    #region || ---------- To Binary Section---------- ||

    public void SaveGameDataToBinaryFile(AllGameData gameData) 
    {

        BinaryFormatter formatter = new BinaryFormatter();

        FileStream stream = new FileStream(binaryPath, FileMode.Create);

        formatter.Serialize(stream, gameData);
        stream.Close();


        print("Data saved to" + binaryPath);



    }

    public AllGameData LoadGameDataFromBinaryFile()
    {


        if (File.Exists(binaryPath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(binaryPath, FileMode.Open);

            AllGameData data = formatter.Deserialize(stream) as AllGameData;
            stream.Close();

            print("Data Loaded from" + binaryPath);

            return data;
        }
        else           
        { 
            return null; 
        }
    }

    #endregion

    #region || ---------- To JSON Section---------- ||

    public void SaveGameDataToJsonFile(AllGameData gameData)
    {
        string json = JsonUtility.ToJson(gameData);

        using (StreamWriter writer = new StreamWriter(jsonPathProject))
        {
            writer.Write(json);
            print("Saved Game to Json file at :" + jsonPathProject);
        };



    }

    public AllGameData LoadGameDataFromJsonFile()
    {
        using (StreamReader reader = new StreamReader(jsonPathProject))
        {
            string json = reader.ReadToEnd();

            AllGameData data = JsonUtility.FromJson<AllGameData>(json);
            return data;
        }
    }

    #endregion

    #region || ---------- Settings Section---------- ||

    #region || ---------- Volume Settings---------- ||

    [System.Serializable]
    public class VolumeSettings
    {
        public float music;
        public float effects;
        public float master;
    }

    public void SaveVolumeSettings(float _music, float _effects, float _master)
    {
        VolumeSettings volumeSettings = new VolumeSettings()
        {
            music = _music,
            effects = _effects,
            master = _master
        };

        PlayerPrefs.SetString("Volume", JsonUtility.ToJson(volumeSettings));
        PlayerPrefs.Save();


        print("Saved to Player Pref");
    }

    public VolumeSettings LoadVolumeSettings()
    {
        return JsonUtility.FromJson<VolumeSettings>(PlayerPrefs.GetString ("Volume"));
    }

    public float LoadMusicVolume()
    {
        var volumeSettings = JsonUtility.FromJson<VolumeSettings>(PlayerPrefs.GetString("Volume"));
        return volumeSettings.music;
    }
    #endregion

    #endregion

    #region || ---------- Encrypt & Decrypt Json File---------- ||

    public string EncryptionDecryption(string data)
    {
        string keyword = "1234567";

        string result = "";

        for (int i = 0; i < data.Length; i++)
        {
            result += (char)(data[i] ^ keyword[i % keyword.Length]);
        }
        return result;
    }
    #endregion
}
