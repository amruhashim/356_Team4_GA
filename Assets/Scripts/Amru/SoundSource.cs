using UnityEngine;

public class SoundSource : MonoBehaviour
{
    [Tooltip("The intensity of the sound emitted from this source.")]
    public float soundIntensity = 1.0f;

    private AudioSource audioSource;

    void Start()
    {
        // Search for the AudioSource component in the current object or any of its children
        audioSource = GetComponentInChildren<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource found in this GameObject or its children.");
        }
    }

    public Vector3 GetSoundPosition()
    {
        return transform.position;
    }

    public float GetSoundIntensity()
    {
        return soundIntensity;
    }

    public bool IsPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }
}
