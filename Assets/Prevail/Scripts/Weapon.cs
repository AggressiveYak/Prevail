using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Item item;


    public void AssignItem(Item i)
    {
        item = i;
    }
}
