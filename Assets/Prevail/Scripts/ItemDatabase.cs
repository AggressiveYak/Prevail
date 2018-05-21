using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class ItemDatabase
{
    static string path = "Assets/Prevail/Resources/JSON/ItemDatabase.json";
    public List<Item> items = new List<Item>();

    /// <summary>
    /// Saves the data to a Json 
    /// </summary>
    public void SaveToJson()
    {
        string json = JsonUtility.ToJson(this, true);
        File.WriteAllText(path, json);
    }
    public static ItemDatabase LoadFromJson()
    {
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<ItemDatabase>(json);
    }

    /// <summary>
    /// Returns an item from the item Database
    /// </summary>
    /// <param name="anObjectSlug"></param>
    /// <returns></returns>
    public Item GetItem(string anObjectSlug)
    {
        foreach (Item i in items)
        {
            if (i.slug == anObjectSlug)
            {
                return i;
            }
        }

        Debug.Log("Couldn't find item " + anObjectSlug + " in the Item Database");
        return null;
    }

    public void AddItem(Item item)
    {
        items.Add(item);
    }
}
