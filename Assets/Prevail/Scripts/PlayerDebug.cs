using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDebug : MonoBehaviour
{
    public List<string> startingItems = new List<string>();


    private void Start()
    {



        foreach (string i in startingItems)
        {
            GetComponent<InventoryController>().AddItem(i);
        }
    }

}
