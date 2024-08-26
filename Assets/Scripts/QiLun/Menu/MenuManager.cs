using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; set; }

    public GameObject menuCanvas;
    public GameObject uiCanvas;

    public GameObject saveMenu;
    public GameObject settingsMenu;
    public GameObject menu;
    public GameObject menuBackground;  // New reference to the menu background object

    public bool isMenuOpen;

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
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMenu();
        }

        // Ensure cursor stays unlocked and visible when the menu is open
        if (isMenuOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void ToggleMenu()
    {
        if (!isMenuOpen)
        {
            // Open the menu
            uiCanvas.SetActive(false);
            menuCanvas.SetActive(true);
            menu.SetActive(true);

            if (menuBackground != null)
            {
                menuBackground.SetActive(true);  // Activate the menu background
            }

            isMenuOpen = true;
        }
        else
        {
            // Close the menu and return to the game
            saveMenu.SetActive(false);
            settingsMenu.SetActive(false);
            menu.SetActive(true);

            uiCanvas.SetActive(true);
            menuCanvas.SetActive(false);

            if (menuBackground != null)
            {
                menuBackground.SetActive(false);  // Deactivate the menu background
            }

            isMenuOpen = false;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void TempSaveGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
        else
        {
            Debug.LogError("SaveManager instance is null. Make sure SaveManager is initialized.");
        }
    }
}
