using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Head,
    Chest,
    Arms,
    Waist,
    Legs,
    Material,
    Key
}

public enum ItemAction
{
    Equip,
    Use,
    None
}

[System.Serializable]
public class Item
{
    //different types an item can be

    //the items name
    public string itemName;
    //the name of the item in the resources folder
    public string slug;
    // the id of this item;
    public int ID;
    //this item's type
    public ItemType itemType;

    public ItemAction action;

    //stats the item has
    public List<BaseStat> stats;

    //The Item's description
    public string description;

    //does item modify stats?
    public bool modifier;
    //can the item be stacked
    public bool stackable;

    public Item(string itemName, string slug, int ID, ItemType itemType, ItemAction action, List<BaseStat> stats, string description, bool modifier, bool stackable)
    {
        this.itemName = itemName;
        this.slug = slug;
        this.ID = ID;
        this.itemType = itemType;
        this.action = action;
        this.stats = stats;
        this.description = description;
        this.modifier = modifier;
        this.stackable = stackable;
    }

    //public Item(string anItemName, string anItemSlug, int ID, ItemType anItemType, string itemDescription, ItemAction action, List<BaseStat> itemStats, bool modifier, bool isStackable)
    //{
    //    this.stats = itemStats;
    //    slug = anItemSlug;
    //    this.ID = ID;
    //    description = itemDescription;
    //    itemType = anItemType;
    //    this.action = action;
    //    itemName = anItemName;
    //    modifier = modifier;
    //    stackable = isStackable;
    //}
}
