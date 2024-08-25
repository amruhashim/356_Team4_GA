using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static SaveManager;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    // Volume Settings UI
    public Button backBTN;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider effectsSlider;

    // Sensitivity Settings UI
    public Button sensitivityBackBTN;
    public Slider sensitivitySlider; // Single slider for both X and Y sensitivity
    public Slider droneSensitivitySlider; // Slider for drone sensitivity

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

        Debug.Log("SettingsManager Awake - Instance set.");
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
            SaveManager.Instance.SaveVolumeSettings(musicSlider.value, effectsSlider.value, masterSlider.value);
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

        VolumeSettings volumeSettings = SaveManager.Instance.LoadVolumeSettings();

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
        SensitivitySettings sensitivitySettings = SaveManager.Instance.LoadSensitivitySettings();

        Debug.Log($"Loaded sensitivity settings: Mouse X={sensitivitySettings.mouseSensitivity.x}, Y={sensitivitySettings.mouseSensitivity.y}, Drone={sensitivitySettings.droneSensitivity}");

        // Set the slider values based on the loaded settings
        sensitivitySlider.value = sensitivitySettings.mouseSensitivity.x; // Assuming X and Y are the same
        droneSensitivitySlider.value = sensitivitySettings.droneSensitivity;

        // Apply the sensitivity immediately
        CameraLook.Instance?.SetSensitivity(sensitivitySettings.mouseSensitivity);
        DroneMovement.Instance?.SetSensitivity(sensitivitySettings.droneSensitivity);

        // Save the settings immediately
        SaveManager.Instance.SaveSensitivitySettings(sensitivitySettings.mouseSensitivity, sensitivitySettings.droneSensitivity);

        Debug.Log("Sensitivity settings applied and saved.");
    }

    private void ApplySensitivity(float mouseSensitivity, float droneSensitivity)
    {
        // Apply the sensitivity to CameraLook and DroneMovement directly
        CameraLook.Instance?.SetSensitivity(new Vector2(mouseSensitivity, mouseSensitivity));
        DroneMovement.Instance?.SetSensitivity(droneSensitivity);

        // Save the settings immediately
        SaveManager.Instance.SaveSensitivitySettings(new Vector2(mouseSensitivity, mouseSensitivity), droneSensitivity);

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
        float dbValue = Mathf.Lerp(-80f, 0f, value / 10f);
        AudioMixerController.Instance.SetEffectsVolume(dbValue);
        SaveManager.Instance.SaveVolumeSettings(masterSlider.value, musicSlider.value, effectsSlider.value);
    }

    public void SetSensitivity(float value)
    {
        ApplySensitivity(value, droneSensitivitySlider.value);
    }
}
