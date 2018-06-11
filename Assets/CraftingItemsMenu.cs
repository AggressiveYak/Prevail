using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingItemsMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject craftingPanel;
    public GameObject inventoryPanel;
    public GameObject quickPanel;
    public GameObject suitPanel;

    [Header("Buttons")]

    public Button firstSelectable;


    public void OpenCraftingItemsPanel()
    {
        ToggleCraftingPanel();
    }
    public void CloseCraftingItemsPanel()
    {
        craftingPanel.SetActive(true);
        inventoryPanel.SetActive(false);
        quickPanel.SetActive(false);
        suitPanel.SetActive(false);
    }


    public void ToggleCraftingPanel()
    {
        craftingPanel.SetActive(true);
        inventoryPanel.SetActive(false);
        quickPanel.SetActive(false);
        suitPanel.SetActive(false);
        firstSelectable.Select();
    }


    public void ToggleInventoryPanel()
    {
        craftingPanel.SetActive(false);
        inventoryPanel.SetActive(true);
        quickPanel.SetActive(false);
        suitPanel.SetActive(false);
    }

    public void ToggleQuickPanel()
    {
        craftingPanel.SetActive(false);
        inventoryPanel.SetActive(false);
        quickPanel.SetActive(true);
        suitPanel.SetActive(false);
    }

    public void ToggleSuitPanel()
    {
        craftingPanel.SetActive(false);
        inventoryPanel.SetActive(false);
        quickPanel.SetActive(false);
        suitPanel.SetActive(true);
    }

   
}
