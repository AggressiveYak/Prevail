using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class ItemDatabaseManager : PersistentSingleton<ItemDatabaseManager>
{
    private ItemDatabase itemDataBase = new ItemDatabase();
    private List<Item> Items { get; set; }

    protected override void Awake()
    {
        base.Awake();
        //BuildDataBase();
    }

    private void BuildDataBase()
    {
        ItemDatabase.LoadFromJson();
        //string itemsAsJson = File.ReadAllText("Assets/Prevail/Resources/ItemDatabase.json");
        //Items = JsonUtility.FromJson<ItemDatabase>(itemsAsJson).items;
    }

    public Item GetItemFromDatabase(string slug)
    {
        return itemDataBase.GetItem(slug);
    }

    public void SaveDatabase()
    {
        itemDataBase.SaveToJson();
    }

    public void UpdateDatabase(ItemDatabase dataBase)
    {
        itemDataBase = dataBase;
    }
}
