using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentUI : MonoBehaviour
{
    public GameObject itemUIPrefab;
    public GameObject itemContent;
    public List<GameObject> itemUIPrefabs;

    public EquippedUI equippedUI;

    public void OpenEquipmentMenu(GameObject character)
    {
        for (int i = 0; i < itemUIPrefabs.Count; i++)
        {
            Destroy(itemUIPrefabs[i]);
            itemUIPrefabs.RemoveAt(i);
        }
        //itemUIPrefabs.Clear();

        gameObject.SetActive(true);

        SetEquippedItems(character);

        InventoryController inventory = character.GetComponent<InventoryController>();

        foreach (Item i in inventory.equipment)
        {
            GameObject go = Instantiate(itemUIPrefab, itemContent.transform);
            go.GetComponent<ItemUI>().SetItem(i);

            itemUIPrefabs.Add(go);
        }
    }

    private void SetEquippedItems(GameObject character)
    {
        equippedUI.SetItems(character);   
    }
}
