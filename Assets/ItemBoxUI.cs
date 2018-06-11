using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBoxUI : Singleton<ItemBoxUI>
{
    public GameObject owner;
    public GameObject itemBoxMenu;
    public EquipmentUI equipmentMenu;
    
    public void OpenUI(GameObject opener)
    {
        itemBoxMenu.SetActive(true);
        owner = opener;
    }

    public void OpenEquipmentMenu()
    {
        itemBoxMenu.SetActive(false);
        equipmentMenu.OpenEquipmentMenu(owner);
    }
}
