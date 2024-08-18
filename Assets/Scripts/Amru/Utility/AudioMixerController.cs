using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerController : MonoBehaviour
{
    public static AudioMixerController Instance { get; private set; }
    public AudioMixer masterMixer;  // Reference to the AudioMixer
     
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Ensure this persists across scenes
        }
    }

    public void SetMasterVolume(float masterLvl)
    {
        masterMixer.SetFloat("MasterVolume", masterLvl);
    }

    public void SetMusicVolume(float musicLvl)
    {
        masterMixer.SetFloat("MusicVolume", musicLvl);
    }

    public void SetEffectsVolume(float effectsLvl)
    {
        masterMixer.SetFloat("EffectsVolume", effectsLvl);
    }
}
