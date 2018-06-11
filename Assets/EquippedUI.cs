using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquippedUI : MonoBehaviour
{
    public EquippedItemUI equippedHead;
    public EquippedItemUI equippedChest;
    public EquippedItemUI equippedArms;
    public EquippedItemUI equippedWaist;
    public EquippedItemUI equippedLegs;

    public void SetItems(GameObject character)
    {
        EquipmentController ec = character.GetComponent<EquipmentController>();

        equippedHead.SetItem(ec.equippedHead);
        equippedChest.SetItem(ec.equippedChest);
        equippedArms.SetItem(ec.equippedArms);
        equippedWaist.SetItem(ec.equippedWaist);
        equippedLegs.SetItem(ec.equippedLegs);
    }
}
