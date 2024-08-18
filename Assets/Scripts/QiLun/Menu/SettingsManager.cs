using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static SaveManager;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; set; }

    public Button backBTN;

    public Slider masterSlider;
    public GameObject masterValue;

    public Slider musicSlider;
    public GameObject musicValue;

    public Slider effectsSlider;
    public GameObject effectsValue;

    private void Start()
    {
        // Attach the listeners for the sliders
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        effectsSlider.onValueChanged.AddListener(SetEffectsVolume);

        backBTN.onClick.AddListener(() =>
        {
            SaveManager.Instance.SaveVolumeSettings(musicSlider.value, effectsSlider.value, masterSlider.value);
        });

        StartCoroutine(LoadAndApplySettings());
    }

    private IEnumerator LoadAndApplySettings()
    {
        LoadAndSetVolume();

        // Load other settings methods if needed

        yield return new WaitForSeconds(0.1f);
    }

    private void LoadAndSetVolume()
    {
        VolumeSettings volumeSettings = SaveManager.Instance.LoadVolumeSettings();

        masterSlider.value = volumeSettings.master;
        musicSlider.value = volumeSettings.music;
        effectsSlider.value = volumeSettings.effects;

        // Apply the loaded values to the AudioMixer
        AudioMixerController.Instance.SetMasterVolume(masterSlider.value);
        AudioMixerController.Instance.SetMusicVolume(musicSlider.value);
        AudioMixerController.Instance.SetEffectsVolume(effectsSlider.value);

        print("Volume Settings are Loaded and Applied");
    }

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
    }

    private void Update()
    {
        masterValue.GetComponent<TextMeshProUGUI>().text = masterSlider.value.ToString("0.0");
        musicValue.GetComponent<TextMeshProUGUI>().text = musicSlider.value.ToString("0.0");
        effectsValue.GetComponent<TextMeshProUGUI>().text = effectsSlider.value.ToString("0.0");
    }

    private void SetMasterVolume(float value)
    {
        AudioMixerController.Instance.SetMasterVolume(value);
    }

    private void SetMusicVolume(float value)
    {
        AudioMixerController.Instance.SetMusicVolume(value);
    }

    private void SetEffectsVolume(float value)
    {
        AudioMixerController.Instance.SetEffectsVolume(value);
    }
}
