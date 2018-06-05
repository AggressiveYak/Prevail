using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public ItemUI itemContainer;

    public RectTransform inventoryItemsGroup;
    public List<ItemUI> itemUIInInventory = new List<ItemUI>();

    // Use this for initialization
    void Awake ()
    {
        UIEventHandler.OnItemAddedToInventory += AddItem;
        //UIEventHandler.OnItemRemovedFromInventory += RemoveItem;
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    private void AddItem(Item item)
    {
        ItemUI newItem = Instantiate(itemContainer, inventoryItemsGroup);
        newItem.SetItem(item);
        newItem.name = item.itemName + " UI Container";
        itemUIInInventory.Add(newItem);
    }
}
