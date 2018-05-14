using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{

    public List<Item> items = new List<Item>();

    public void AddItem(string slug)
    {
        Item item = ItemDatabaseManager.Instance.GetItemFromDatabase(slug);
        
    }

    public void AddItem(Item item)
    {
        items.Add(item);
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