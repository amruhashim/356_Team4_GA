using UnityEngine;

public class PersistentLoader : MonoBehaviour
{
    public GameObject saveManagerPrefab;

    private void Awake()
    {
        if (SaveManager.Instance == null)
        {
            Instantiate(saveManagerPrefab);
        }
    }
}