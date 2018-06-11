using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquippedItemUI : ItemUI
{
    public Text text;

    public override void SetItem(Item newItem)
    {
        base.SetItem(newItem);

        text.text = newItem.itemName;
    }
}
