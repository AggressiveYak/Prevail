using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    Animator anim;

    public GameObject rotator;

    [Header("")]
    public GameObject craftingItemsMenu;
    public GameObject questMenu;
    public GameObject settingsMenu;
    public GameObject infoMenu;

    public enum ControllerState
    {
        Crafting,
        Quest,
        Settings,
        Info,
        Lerping
    }

    bool open = false;

    public ControllerState currentState = ControllerState.Crafting;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rotator.SetActive(false);

        UIEventHandler.OnGameMenuOpened += OpenMenu;
    }

    private void OpenMenu()
    {
        rotator.SetActive(true);
        ToggleCrafting();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("B Button"))
        {
            rotator.SetActive(false);
            UIEventHandler.GameMenuClosed();
        }


        ControllerState newState = currentState;
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetButtonDown("Right Bumper"))
        {
            newState = (ControllerState)(((int)currentState + 1) % 4);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetButtonDown("Left Bumper")) 
        {
            int index = (int)(currentState - 1) % 4;
            if (index < 0)
            {
                index += 4;
            }

            newState = (ControllerState)(index);
        }

        if (currentState != newState)
        {
            currentState = newState;

            switch (currentState)
            {
                case ControllerState.Crafting:
                    ToggleCrafting();
                    break;
                case ControllerState.Quest:
                    ToggleQuestMenu();
                    break;
                case ControllerState.Settings:
                    ToggleSettingsMenu();
                    break;
                case ControllerState.Info:
                    ToggleInfoMenu();
                    break;
                case ControllerState.Lerping:
                    break;
            default:
                    break;
            }
        }
    }

    void ToggleCrafting()
    {
        rotator.transform.rotation = Quaternion.Euler(0, 0, 0);
        craftingItemsMenu.GetComponent<CanvasGroup>().alpha = 1;
        craftingItemsMenu.GetComponent<CanvasGroup>().interactable = true;
        craftingItemsMenu.GetComponent<CanvasGroup>().blocksRaycasts = true;

        questMenu.GetComponent<CanvasGroup>().alpha = 0.3f;
        questMenu.GetComponent<CanvasGroup>().interactable = false;
        questMenu.GetComponent<CanvasGroup>().blocksRaycasts = false;


        settingsMenu.GetComponent<CanvasGroup>().alpha = 0.3f;
        settingsMenu.GetComponent<CanvasGroup>().interactable = false;
        settingsMenu.GetComponent<CanvasGroup>().blocksRaycasts = false;

        infoMenu.GetComponent<CanvasGroup>().alpha = 0.3f;
        infoMenu.GetComponent<CanvasGroup>().interactable = false;
        infoMenu.GetComponent<CanvasGroup>().blocksRaycasts = false;

        craftingItemsMenu.GetComponent<CraftingItemsMenu>().OpenCraftingItemsPanel();
    }

    void ToggleQuestMenu()
    {
        rotator.transform.rotation = Quaternion.Euler(0, 90, 0);
        craftingItemsMenu.GetComponent<CanvasGroup>().alpha = 0.3f;
        craftingItemsMenu.GetComponent<CanvasGroup>().interactable = false;
        craftingItemsMenu.GetComponent<CanvasGroup>().blocksRaycasts = false;

        questMenu.GetComponent<CanvasGroup>().alpha = 1f;
        questMenu.GetComponent<CanvasGroup>().interactable = true;
        questMenu.GetComponent<CanvasGroup>().blocksRaycasts = true;


        settingsMenu.GetComponent<CanvasGroup>().alpha = 0.3f;
        settingsMenu.GetComponent<CanvasGroup>().interactable = false;
        settingsMenu.GetComponent<CanvasGroup>().blocksRaycasts = false;

        infoMenu.GetComponent<CanvasGroup>().alpha = 0.3f;
        infoMenu.GetComponent<CanvasGroup>().interactable = false;
        infoMenu.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }


    void ToggleSettingsMenu()
    {
        rotator.transform.rotation = Quaternion.Euler(0, 180, 0);
        craftingItemsMenu.GetComponent<CanvasGroup>().alpha = 0.3f;
        craftingItemsMenu.GetComponent<CanvasGroup>().interactable = false;
        craftingItemsMenu.GetComponent<CanvasGroup>().blocksRaycasts = false;

        questMenu.GetComponent<CanvasGroup>().alpha = 0.3f;
        questMenu.GetComponent<CanvasGroup>().interactable = false;
        questMenu.GetComponent<CanvasGroup>().blocksRaycasts = false;


        settingsMenu.GetComponent<CanvasGroup>().alpha = 1f;
        settingsMenu.GetComponent<CanvasGroup>().interactable = true;
        settingsMenu.GetComponent<CanvasGroup>().blocksRaycasts = true;

        infoMenu.GetComponent<CanvasGroup>().alpha = 0.3f;
        infoMenu.GetComponent<CanvasGroup>().interactable = false;
        infoMenu.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    void ToggleInfoMenu()
    {
        rotator.transform.rotation = Quaternion.Euler(0, -90, 0);
        craftingItemsMenu.GetComponent<CanvasGroup>().alpha = 0.3f;
        craftingItemsMenu.GetComponent<CanvasGroup>().interactable = false;
        craftingItemsMenu.GetComponent<CanvasGroup>().blocksRaycasts = false;

        questMenu.GetComponent<CanvasGroup>().alpha = 0.3f;
        questMenu.GetComponent<CanvasGroup>().interactable = false;
        questMenu.GetComponent<CanvasGroup>().blocksRaycasts = false;

        settingsMenu.GetComponent<CanvasGroup>().alpha = 0.3f;
        settingsMenu.GetComponent<CanvasGroup>().interactable = false;
        settingsMenu.GetComponent<CanvasGroup>().blocksRaycasts = false;

        infoMenu.GetComponent<CanvasGroup>().alpha = 1f;
        infoMenu.GetComponent<CanvasGroup>().interactable = true;
        infoMenu.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

}
