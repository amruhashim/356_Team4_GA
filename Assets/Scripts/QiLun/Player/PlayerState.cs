using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public static PlayerState Instance { get; set; }

    // ---- Player Health ---- //
    public float currentHealth;
    public float maxHealth;


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

    private void Start()
    {
        currentHealth = maxHealth;
    }


    // Update is called once per frame
    void Update()
    {
        //FOR TESTING THE HEALTH BAR
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (currentHealth > 0)
            {
                currentHealth -= 10;
                if (currentHealth < 0)
                {
                    currentHealth = 0;  // Ensure health doesn't go below 0
                }
            }
        }
    }
}


