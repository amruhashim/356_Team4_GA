using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeVisibility : MonoBehaviour
{
    [SerializeField] private GameObject grenadeObject; // Reference to the grenade object in the hands
    [SerializeField] private GameObject grenadePinObject; // Reference to the grenade object in the hands


    void Start()
    {
        UpdateGrenadeStatus();
    }

    void Update()
    {
        UpdateGrenadeStatus();
    }

    private void UpdateGrenadeStatus()
    {
        // Check the grenade count from PlayerState and deactivate the grenade object if there are no grenades
        if (PlayerState.Instance.grenadeCount <= 0)
        {
            grenadeObject.SetActive(false);
            grenadePinObject.SetActive(false);
        }
        else
        {
            grenadeObject.SetActive(true);
            grenadePinObject.SetActive(true);
        }
    }
}
