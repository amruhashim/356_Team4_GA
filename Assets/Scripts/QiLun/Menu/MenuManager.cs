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
        if (Input.GetKeyDown(KeyCode.M) && !isMenuOpen)
        {
            // Open the menu
            uiCanvas.SetActive(false);
            menuCanvas.SetActive(true);
            menu.SetActive(true); // Set menu to true here

            isMenuOpen = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Input.GetKeyDown(KeyCode.M) && isMenuOpen)
        {
            // Close the menu and return to the game
            saveMenu.SetActive(false);
            settingsMenu.SetActive(false);
            menu.SetActive(true); // Keep the main menu active to ensure it's ready when toggling again

            uiCanvas.SetActive(true);
            menuCanvas.SetActive(false);

            isMenuOpen = false;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void TempSaveGame()
    {
        SaveManager.Instance.SaveGame();
    }
}
