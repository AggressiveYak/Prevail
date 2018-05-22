using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum WeaponType
{
    Sword,
    Shield,
    Crossbow,
    NULL
}

public enum ItemType
{
    Head,
    Chest,
    Arms,
    Waist,
    Legs,
    MainHand,
    OffHand,
    Material,
    Key,
    NULL
}

public enum ItemAction
{
    Equip,
    Use,
    None,
    NULL
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

    public WeaponType weaponType;

    public ItemAction action;

    //stats the item has
    public List<BaseStat> stats;

    //The Item's description
    public string description;

    //does item modify stats?
    public bool modifier;
    //can the item be stacked
    public bool stackable;

    public Item()
    {
        itemName = "NULL";
        slug = "null";
        ID = 999999999;
        itemType = ItemType.NULL;
        action = ItemAction.NULL;
        stats = null;
        description = "THIS ITEM IS NULL";
        modifier = false;
        stackable = false;
    }

    public Item(string itemName, string slug, int ID, ItemType itemType, WeaponType weaponType, ItemAction action, List<BaseStat> stats, string description, bool modifier, bool stackable)
    {
        this.itemName = itemName;
        this.slug = slug;
        this.ID = ID;
        this.itemType = itemType;
        this.weaponType = weaponType;
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
