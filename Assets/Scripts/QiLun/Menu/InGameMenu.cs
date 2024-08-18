using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class InGameMenu : MonoBehaviour
{
    public static InGameMenu Instance { get; private set; }

    public Slider masterSlider;
    public GameObject masterValue;

    public Slider musicSlider;
    public GameObject musicValue;

    public Slider effectsSlider;
    public GameObject effectsValue;

    private AudioMixerController audioMixerController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        audioMixerController = AudioMixerController.Instance;

        // Load and apply volume settings
        LoadAndApplyVolumeSettings();

        // Add listeners to sliders
        masterSlider.onValueChanged.AddListener(delegate { UpdateAndSaveSettings(); });
        musicSlider.onValueChanged.AddListener(delegate { UpdateAndSaveSettings(); });
        effectsSlider.onValueChanged.AddListener(delegate { UpdateAndSaveSettings(); });
    }

    private void LoadAndApplyVolumeSettings()
    {
        SaveManager.Instance.LoadAndApplyVolumeSettings();
        LoadAndSetVolume();
    }

    private void LoadAndSetVolume()
    {
        var volumeSettings = SaveManager.Instance.LoadVolumeSettings();

        masterSlider.value = volumeSettings.master;
        musicSlider.value = volumeSettings.music;
        effectsSlider.value = volumeSettings.effects;

        UpdateUI();
    }

    private void UpdateUI()
    {
        masterValue.GetComponent<TextMeshProUGUI>().text = masterSlider.value.ToString("F1");
        musicValue.GetComponent<TextMeshProUGUI>().text = musicSlider.value.ToString("F1");
        effectsValue.GetComponent<TextMeshProUGUI>().text = effectsSlider.value.ToString("F1");
    }

    private void UpdateAndSaveSettings()
    {
        audioMixerController.SetMasterVolume(masterSlider.value);
        audioMixerController.SetMusicVolume(musicSlider.value);
        audioMixerController.SetEffectsVolume(effectsSlider.value);

        SaveManager.Instance.SaveVolumeSettings(musicSlider.value, effectsSlider.value, masterSlider.value);

        UpdateUI();
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
