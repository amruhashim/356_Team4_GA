using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameMenu : MonoBehaviour
{
    // Volume Settings UI
    public Button backBTN;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider effectsSlider;

    // Sensitivity Settings UI
    public Button sensitivityBackBTN;
    public Slider sensitivitySlider; // Single slider for both X and Y sensitivity
    public Slider droneSensitivitySlider; // Slider for drone sensitivity

    // References to CameraLook and DroneMovement scripts
    private CameraLook cameraLook;
    private DroneMovement droneMovement;

    private void Awake()
    {
        // Optionally, find the CameraLook and DroneMovement scripts in the scene
        cameraLook = FindObjectOfType<CameraLook>();
        droneMovement = FindObjectOfType<DroneMovement>();

        Debug.Log("InGameMenu Awake - References set.");
    }

    private void Start()
    {
        Debug.Log("InGameMenu Start - Initializing Settings.");

        // Attach the listeners for the volume sliders
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        effectsSlider.onValueChanged.AddListener(SetEffectsVolume);

        // Attach the listeners for the sensitivity sliders
        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
        droneSensitivitySlider.onValueChanged.AddListener(SetDroneSensitivity);

        // Back button for volume settings
        backBTN.onClick.AddListener(() =>
        {
            SaveManager.Instance.SaveVolumeSettings(masterSlider.value, musicSlider.value, effectsSlider.value);
        });

        // Back button for sensitivity settings
        sensitivityBackBTN.onClick.AddListener(() =>
        {
            ApplySensitivity(sensitivitySlider.value, droneSensitivitySlider.value);
        });

        StartCoroutine(LoadAndApplySettings());
    }

    private IEnumerator LoadAndApplySettings()
    {
        // Load volume and sensitivity settings
        LoadAndSetVolume();
        LoadAndSetSensitivity();

        // Optional delay to ensure everything is properly set up before continuing
        yield return new WaitForSeconds(0.1f);

        // Additional logic after the delay (if any)
        Debug.Log("Settings loaded and applied after delay.");
    }

    private void LoadAndSetVolume()
    {
        Debug.Log("Loading volume settings from SaveManager...");

        SaveManager.VolumeSettings volumeSettings = SaveManager.Instance.LoadVolumeSettings();

        masterSlider.value = volumeSettings.master;
        musicSlider.value = volumeSettings.music;
        effectsSlider.value = volumeSettings.effects;

        SetMasterVolume(masterSlider.value);
        SetMusicVolume(musicSlider.value);
        SetEffectsVolume(effectsSlider.value);

        Debug.Log("Volume settings applied to AudioMixer.");
    }

    private void LoadAndSetSensitivity()
    {
        Debug.Log("Loading sensitivity settings from SaveManager...");

        // Load the sensitivity settings
        SaveManager.SensitivitySettings sensitivitySettings = SaveManager.Instance.LoadSensitivitySettings();

        Debug.Log($"Loaded sensitivity settings: Mouse X={sensitivitySettings.mouseSensitivity.x}, Y={sensitivitySettings.mouseSensitivity.y}, Drone={sensitivitySettings.droneSensitivity}");

        // Set the slider values based on the loaded settings
        sensitivitySlider.value = sensitivitySettings.mouseSensitivity.x; // Assuming X and Y are the same
        droneSensitivitySlider.value = sensitivitySettings.droneSensitivity;

        // Apply the sensitivity immediately
        cameraLook?.SetSensitivity(sensitivitySettings.mouseSensitivity.ToVector2());
        droneMovement?.SetSensitivity(sensitivitySettings.droneSensitivity);

        // Save the settings immediately
        SaveManager.Instance.SaveSensitivitySettings(sensitivitySettings.mouseSensitivity, sensitivitySettings.droneSensitivity);

        Debug.Log("Sensitivity settings applied and saved.");
    }

    private void ApplySensitivity(float mouseSensitivity, float droneSensitivity)
    {
        // Apply the sensitivity to CameraLook and DroneMovement directly
        cameraLook?.SetSensitivity(new Vector2(mouseSensitivity, mouseSensitivity));
        droneMovement?.SetSensitivity(droneSensitivity);

        // Save the settings immediately
        SaveManager.Instance.SaveSensitivitySettings(new SaveManager.SerializableVector2(mouseSensitivity, mouseSensitivity), droneSensitivity);

        Debug.Log($"Sensitivity applied and saved: Mouse X={mouseSensitivity}, Y={mouseSensitivity}, Drone={droneSensitivity}");
    }

    private void ApplySensitivity(float value)
    {
        ApplySensitivity(value, droneSensitivitySlider.value);
    }

    public void SetDroneSensitivity(float value)
    {
        ApplySensitivity(sensitivitySlider.value, value);
    }

    public void SetMasterVolume(float value)
    {
        float dbValue = Mathf.Lerp(-30f, 0f, value / 10f);
        AudioMixerController.Instance.SetMasterVolume(dbValue);
        SaveManager.Instance.SaveVolumeSettings(masterSlider.value, musicSlider.value, effectsSlider.value);
    }

    public void SetMusicVolume(float value)
    {
        float dbValue = Mathf.Lerp(-80f, 0f, value / 10f);
        AudioMixerController.Instance.SetMusicVolume(dbValue);
        SaveManager.Instance.SaveVolumeSettings(masterSlider.value, musicSlider.value, effectsSlider.value);
    }

    public void SetEffectsVolume(float value)
    {
        float dbValue = Mathf.Lerp(-30f, 0f, value / 10f);
        AudioMixerController.Instance.SetEffectsVolume(dbValue);
        SaveManager.Instance.SaveVolumeSettings(masterSlider.value, musicSlider.value, effectsSlider.value);
    }

    public void SetSensitivity(float value)
    {
        ApplySensitivity(value, droneSensitivitySlider.value);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
