using System;
using UnityEngine;
using com.ootii.Actors;

public class SampleUI : MonoBehaviour
{
    private MountPoints mMountPoints = null;

    private float mBtnWidth = 130;
    private float mBtnHeight = 30;

    /// <summary>
    /// Raised when the object becomes active. Initialization can happen here.
    /// </summary>
    void Awake()
    {
        mMountPoints = GameObject.Find("DefaultMale").GetComponent<MountPoints>();

    }
    
    /// <summary>
    /// Raised when the object starts being updated
    /// </summary>
    void Start()
    {
        //mMountPoints.AddSkinnedItem("Prefabs/Armor/Pants/Undies_01");
    }

    /// <summary>
    /// Place the buttons and send messages when clicked
    /// </summary>
    void OnGUI()
    {
        Rect lAttachSword = new Rect(10, 10 + ((mBtnHeight + 10) * 0), mBtnWidth, mBtnHeight);
        if (GUI.Button(lAttachSword, "Attach Sword"))
        {
            mMountPoints.ConnectMountPoints("Right Hand", GameObject.Find("Sword"), "Mount Point");
        }

        Rect lDetachSword = new Rect(10, 10 + ((mBtnHeight + 10) * 1), mBtnWidth, mBtnHeight);
        if (GUI.Button(lDetachSword, "Detach Sword"))
        {
            GameObject lSword = GameObject.Find("Sword");
            Mount lMount = lSword.GetComponent<Mount>();
            MountPoint lSwordMP = lMount.Point;

            mMountPoints.DisconnectMountPoints(lSwordMP);
        }

        Rect lAttachShield = new Rect(10, 10 + ((mBtnHeight + 10) * 2), mBtnWidth, mBtnHeight);
        if (GUI.Button(lAttachShield, "Attach Shield"))
        {
            mMountPoints.ConnectMountPoints("Left Arm", GameObject.Find("Shield"), "Mount Point");
        }

        Rect lCreateHelmet = new Rect(10, 10 + ((mBtnHeight + 10) * 3), mBtnWidth, mBtnHeight);
        if (GUI.Button(lCreateHelmet, "Create Helmet"))
        {
            mMountPoints.ConnectMountPoints("Head", "Prefabs/Armor/Helmets/Helmet", "Head");
            mMountPoints.RemoveSkinnedItem("Hair");
        }

        Rect lCreateShirt = new Rect(10, 10 + ((mBtnHeight + 10) * 4), mBtnWidth, mBtnHeight);
        if (GUI.Button(lCreateShirt, "Create Shirt"))
        {
            mMountPoints.AddSkinnedItem("Prefabs/Armor/Shirts/Shirt_02", "Prefabs/Armor/Shirts/Shirt_02_mask");
        }

        Rect lDeleteShirt = new Rect(10, 10 + ((mBtnHeight + 10) * 5), mBtnWidth, mBtnHeight);
        if (GUI.Button(lDeleteShirt, "Delete Shirt"))
        {
            mMountPoints.RemoveSkinnedItemFromPath("Prefabs/Armor/Shirts/Shirt_02");
        }

        Rect lCreatePants = new Rect(10, 10 + ((mBtnHeight + 10) * 6), mBtnWidth, mBtnHeight);
        if (GUI.Button(lCreatePants, "Create Pants"))
        {
            mMountPoints.AddSkinnedItem("Prefabs/Armor/Pants/Pants_02", "Prefabs/Armor/Pants/Pants_02_mask");
        }

        Rect lCreateBoots = new Rect(10, 10 + ((mBtnHeight + 10) * 7), mBtnWidth, mBtnHeight);
        if (GUI.Button(lCreateBoots, "Create Boots"))
        {
            mMountPoints.AddSkinnedItem("Prefabs/Armor/Shoes/Boots_02", "Prefabs/Armor/Shoes/Boots_02_mask");
        }

        Rect lClearItems = new Rect(10, 10 + ((mBtnHeight + 10) * 8), mBtnWidth, mBtnHeight);
        if (GUI.Button(lClearItems, "Clear Masks"))
        {
            mMountPoints.ClearBodyMasks();
        }

        Rect lMergeItems = new Rect(10, 10 + ((mBtnHeight + 10) * 9), mBtnWidth, mBtnHeight);
        if (GUI.Button(lMergeItems, "Apply Masks"))
        {
            mMountPoints.ApplyBodyMasks();
        }
    }
}
