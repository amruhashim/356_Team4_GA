using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
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

    // References to other components
    private CameraLook cameraLook;
    private DroneMovement droneMovement;
    private SaveManager saveManager;
    private AudioMixerController audioMixerController;

    private void Awake()
    {
        // Find or assign references
        cameraLook = FindObjectOfType<CameraLook>();
        droneMovement = FindObjectOfType<DroneMovement>();
        saveManager = FindObjectOfType<SaveManager>();
        audioMixerController = FindObjectOfType<AudioMixerController>();

        Debug.Log("SettingsManager Awake - References set.");
    }

    private void Start()
    {
        Debug.Log("SettingsManager Start - Initializing Settings.");

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
            saveManager?.SaveVolumeSettings(masterSlider.value, musicSlider.value, effectsSlider.value);
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

    public void LoadAndSetVolume()
    {
        Debug.Log("Loading volume settings from SaveManager...");

        var volumeSettings = saveManager?.LoadVolumeSettings();

        if (volumeSettings != null)
        {
            masterSlider.value = volumeSettings.master;
            musicSlider.value = volumeSettings.music;
            effectsSlider.value = volumeSettings.effects;

            SetMasterVolume(masterSlider.value);
            SetMusicVolume(musicSlider.value);
            SetEffectsVolume(effectsSlider.value);

            Debug.Log("Volume settings applied to AudioMixer.");
        }
    }

    public void LoadAndSetSensitivity()
    {
        Debug.Log("Loading sensitivity settings from SaveManager...");

        var sensitivitySettings = saveManager?.LoadSensitivitySettings();

        if (sensitivitySettings != null)
        {
            Debug.Log($"Loaded sensitivity settings: Mouse X={sensitivitySettings.mouseSensitivity.x}, Y={sensitivitySettings.mouseSensitivity.y}, Drone={sensitivitySettings.droneSensitivity}");

            // Set the slider values based on the loaded settings
            sensitivitySlider.value = sensitivitySettings.mouseSensitivity.x; // Assuming X and Y are the same
            droneSensitivitySlider.value = sensitivitySettings.droneSensitivity;

            // Apply the sensitivity immediately
            cameraLook?.SetSensitivity(sensitivitySettings.mouseSensitivity.ToVector2());
            droneMovement?.SetSensitivity(sensitivitySettings.droneSensitivity);

            // Save the settings immediately
            saveManager?.SaveSensitivitySettings(sensitivitySettings.mouseSensitivity, sensitivitySettings.droneSensitivity);

            Debug.Log("Sensitivity settings applied and saved.");
        }
    }

    private void ApplySensitivity(float mouseSensitivity, float droneSensitivity)
    {
        // Apply the sensitivity to CameraLook and DroneMovement directly
        cameraLook?.SetSensitivity(new Vector2(mouseSensitivity, mouseSensitivity));
        droneMovement?.SetSensitivity(droneSensitivity);

        // Save the settings immediately
        saveManager?.SaveSensitivitySettings(new SaveManager.SerializableVector2(mouseSensitivity, mouseSensitivity), droneSensitivity);

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
        float dbValue = Mathf.Lerp(-80f, 0f, value / 10f);
        audioMixerController?.SetMasterVolume(dbValue);
        saveManager?.SaveVolumeSettings(masterSlider.value, musicSlider.value, effectsSlider.value);
    }

    public void SetMusicVolume(float value)
    {
        float dbValue = Mathf.Lerp(-80f, 0f, value / 10f);
        audioMixerController?.SetMusicVolume(dbValue);
        saveManager?.SaveVolumeSettings(masterSlider.value, musicSlider.value, effectsSlider.value);
    }

    public void SetEffectsVolume(float value)
    {
        float dbValue = Mathf.Lerp(-80f, 0f, value / 10f);
        audioMixerController?.SetEffectsVolume(dbValue);
        saveManager?.SaveVolumeSettings(masterSlider.value, musicSlider.value, effectsSlider.value);
    }

    public void SetSensitivity(float value)
    {
        ApplySensitivity(value, droneSensitivitySlider.value);
    }
}
