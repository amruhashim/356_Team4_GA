using UnityEngine;

public class PersistentCinemachineCamera : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
