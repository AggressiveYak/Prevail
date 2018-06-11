using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public List<Item> items = new List<Item>();
    public List<Item> equipment = new List<Item>();

    private void Start()
    {
        //AddItem("debugHeadA");
        //AddItem("debugChestA");
        //AddItem("debugArmsA");
        //AddItem("debugWaistA");
        //AddItem("debugLegsA");

        AddItem("debugHeadB");
        AddItem("debugChestB");
        AddItem("debugArmsB");
        AddItem("debugWaistB");
        AddItem("debugLegsB");
    }

    public void AddItem(string slug)
    {
        Item item = ItemDatabaseManager.Instance.GetItemFromDatabase(slug);
        AddItem(item);
    }

    public void AddItem(Item item)
    {
        if (item.itemType == ItemType.Head ||
            item.itemType == ItemType.Chest ||
            item.itemType == ItemType.Arms ||
            item.itemType == ItemType.Waist ||
            item.itemType == ItemType.Legs)
        {
            equipment.Add(item);
            return;
        }
        items.Add(item);
        UIEventHandler.ItemAddedToInventory(item);
    }

    // TODO: make it so items are removed by id
    public void RemoveItem(Item item)
    {
        items.Remove(items.Find(x => x.slug == item.slug));
    }


    private void SortItems()
    {

    }
}