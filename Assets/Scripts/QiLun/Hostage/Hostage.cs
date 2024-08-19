using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hostage : MonoBehaviour
{
    [SerializeField] private string uniqueID;

    public string UniqueID => uniqueID;

    void Awake()
    {
        // Do not generate a unique ID if it's not assigned; leave it as is.
        if (string.IsNullOrEmpty(uniqueID))
        {
            Debug.LogWarning($"Hostage {gameObject.name} does not have a unique ID assigned!");
        }
    }
}

