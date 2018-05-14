using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentController : MonoBehaviour
{
    public GameObject avatar;

    [Header("Game Objects")]
    public GameObject head;
    public GameObject chest;
    public GameObject arms;
    public GameObject waist;
    public GameObject legs;

    [Header("Item Objects")]
    public Item equippedHead;
    public Item equippedChest;
    public Item equippedArms;
    public Item equippedWaist;
    public Item equippedLegs;

    public Item equippedWeapon;

    private void Update()
    {
         
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EquipItem(ItemDatabaseManager.Instance.GetItemFromDatabase("debugHeadA"));
        }
    }

    public void EquipItem(Item item)
    {
        switch (item.itemType)
        {
            case ItemType.Head:
                if (head != null)
                {
                    
                }

                head = EquipItemHelper(head, item);
                equippedHead = item;

                break;
            case ItemType.Chest:
                chest = EquipItemHelper(chest, item);
                equippedChest = item;
                break;
            case ItemType.Arms:
                break;
            case ItemType.Waist:
                break;
            case ItemType.Legs:
                break;
            case ItemType.Material:
                break;
            case ItemType.Key:
                break;
            default:
                break;
        }
    }

    private GameObject EquipItemHelper(GameObject wornItem, Item itemtoAdd)
    {
        wornItem = Wear((Resources.Load("Prefabs/Armor/" + itemtoAdd.slug)) as GameObject, wornItem);
        wornItem.name = itemtoAdd.slug;
        return wornItem;
    }


    private GameObject Wear(GameObject clothing, GameObject wornClothing)
    {
        if (clothing == null)
        {
            return null;
        }

        clothing = Instantiate(clothing);
        wornClothing = AttachEquipment(clothing, avatar);

        //Stitcher.Instance.Stitch(clothing, avatar);

        return wornClothing;
        //return clothing;
    }

    private GameObject AttachEquipment(GameObject equipment, GameObject character)
    {
        SkinnedMeshRenderer characterMeshRenderer = character.GetComponentInChildren<SkinnedMeshRenderer>();
        SkinnedMeshRenderer equipmentMeshRenderer = equipment.GetComponentInChildren<SkinnedMeshRenderer>();
        //equipmentMeshRenderer.rootBone = equipment.transform.Find("HIPS");
        equipment.transform.parent = character.transform;
        equipmentMeshRenderer.bones = characterMeshRenderer.bones;
        
        //for (int i = 0; i < equipmentMeshRenderer.bones.Length; i++)
        //{
        //    for (int j = 0; j < characterMeshRenderer.bones.Length; j++)
        //    {
        //        if (equipmentMeshRenderer.bones[i].name == characterMeshRenderer.bones[j].name)
        //        {
        //            equipmentMeshRenderer.bones[i] = characterMeshRenderer.bones[i];
        //        }
        //    }

        //}

        //equipmentMeshRenderer.rootBone = characterMeshRenderer.rootBone;
        return equipment;
    }

}
