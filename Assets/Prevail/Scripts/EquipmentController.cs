using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.ootii.Actors;

public class EquipmentController : MonoBehaviour
{
    public GameObject avatar;
    public MountPoints mountPoints;

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
        //DEBUG
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            EquipItem(ItemDatabaseManager.Instance.GetItemFromDatabase("debugHeadA"));
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            EquipItem(ItemDatabaseManager.Instance.GetItemFromDatabase("debugChestA"));
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            EquipItem(ItemDatabaseManager.Instance.GetItemFromDatabase("debugArmsA"));
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            EquipItem(ItemDatabaseManager.Instance.GetItemFromDatabase("debugWaistA"));
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            EquipItem(ItemDatabaseManager.Instance.GetItemFromDatabase("debugLegsA"));
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            EquipItem(ItemDatabaseManager.Instance.GetItemFromDatabase("debugHeadB"));
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            EquipItem(ItemDatabaseManager.Instance.GetItemFromDatabase("debugChestB"));
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            EquipItem(ItemDatabaseManager.Instance.GetItemFromDatabase("debugArmsB"));
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            EquipItem(ItemDatabaseManager.Instance.GetItemFromDatabase("debugWaistB"));
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            EquipItem(ItemDatabaseManager.Instance.GetItemFromDatabase("debugLegsB"));
        }





    }

    public void EquipItem(Item item)
    {
        switch (item.itemType)
        {
            case ItemType.Head:
                if (head != null)
                {
                    mountPoints.RemoveSkinnedItem("F_" + equippedHead.slug);
                    Destroy(head);
                }

                equippedHead = item;
               
                head = mountPoints.AddSkinnedItem("Prefabs/Armor/F_" + item.slug).GameObject;
                break;
            case ItemType.Chest:

                if (chest != null)
                {
                    mountPoints.RemoveSkinnedItem("F_" + equippedChest.slug);
                    Destroy(chest);
                }

                equippedChest = item;
                chest = mountPoints.AddSkinnedItem("Prefabs/Armor/F_" + item.slug).GameObject;
                break;
            case ItemType.Arms:

                if (arms != null)
                {
                    mountPoints.RemoveSkinnedItem("F_" + equippedArms.slug);
                    Destroy(arms);
                }

                equippedArms = item;
                arms = mountPoints.AddSkinnedItem("Prefabs/Armor/F_" + item.slug).GameObject;

                break;
            case ItemType.Waist:

                if (waist != null)
                {
                    mountPoints.RemoveSkinnedItem("F_" + equippedWaist.slug);
                    Destroy(waist);
                }

                equippedWaist = item;
                waist = mountPoints.AddSkinnedItem("Prefabs/Armor/F_" + item.slug).GameObject;

                break;
            case ItemType.Legs:

                if (legs != null)
                {
                    mountPoints.RemoveSkinnedItem("F_" + equippedLegs.slug);
                    Destroy(legs);
                }

                equippedLegs = item;
                legs = mountPoints.AddSkinnedItem("Prefabs/Armor/F_" + item.slug).GameObject;

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
