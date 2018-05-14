using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgrammingTesting : MonoBehaviour
{
    //   public GameObject debug;

    // Use this for initialization
    void Start()
    {
        ItemDatabase database = new ItemDatabase();
        List<Item> items = new List<Item>();

        List<BaseStat> EquipmentStats = new List<BaseStat>();
        BaseStat Health = new BaseStat(BaseStatType.Health, 0, "Health");
        BaseStat Energy = new BaseStat(BaseStatType.Energy, 0, "Energy");
        BaseStat Attack = new BaseStat(BaseStatType.Attack, 0, "Attack");
        BaseStat Defense = new BaseStat(BaseStatType.Defense, 0, "Defense");
        BaseStat Speed = new BaseStat(BaseStatType.Speed, 0, "Speed");
        EquipmentStats.Add(Health);
        EquipmentStats.Add(Energy);
        EquipmentStats.Add(Attack);
        EquipmentStats.Add(Defense);
        EquipmentStats.Add(Speed);

        Item DebugHeadA = new Item("DebugHeadA", "debugHeadA", 5000, ItemType.Head, ItemAction.Equip, EquipmentStats, "It blue.", false, false);
        Item DebugChestA = new Item("DebugChestA", "debugChestA", 5001, ItemType.Chest, ItemAction.Equip, EquipmentStats, "It blue.", false, false);
        Item DebugArmsA = new Item("DebugArmsA", "debugArmsA", 5002, ItemType.Arms, ItemAction.Equip, EquipmentStats, "It blue.", false, false);
        Item DebugWaistA = new Item("DebugWaistA", "debugWaistA", 5003, ItemType.Waist, ItemAction.Equip, EquipmentStats, "It blue.", false, false);
        Item DebugLegsA = new Item("DebugLegsA", "debugLegsA", 5004, ItemType.Legs, ItemAction.Equip, EquipmentStats, "It blue.", false, false);


        Item DebugHeadB = new Item("DebugHeadB", "debugHeadB", 5000, ItemType.Head, ItemAction.Equip, EquipmentStats, "It blue.", false, false);
        Item DebugChestB = new Item("DebugChestB", "debugChestB", 5001, ItemType.Chest, ItemAction.Equip, EquipmentStats, "It blue.", false, false);
        Item DebugArmsB = new Item("DebugArmsB", "debugArmsB", 5002, ItemType.Arms, ItemAction.Equip, EquipmentStats, "It blue.", false, false);
        Item DebugWaistB = new Item("DebugWaistB", "debugWaistB", 5003, ItemType.Waist, ItemAction.Equip, EquipmentStats, "It blue.", false, false);
        Item DebugLegsB = new Item("DebugLegsB", "debugLegsB", 5004, ItemType.Legs, ItemAction.Equip, EquipmentStats, "It blue.", false, false);
        database.AddItem(DebugHeadA);
        database.AddItem(DebugChestA);
        database.AddItem(DebugArmsA);
        database.AddItem(DebugWaistA);
        database.AddItem(DebugLegsA);

        database.AddItem(DebugHeadB);
        database.AddItem(DebugChestB);
        database.AddItem(DebugArmsB);
        database.AddItem(DebugWaistB);
        database.AddItem(DebugLegsB);


        Item Head = new Item("Standard Helmet", "starterHelmet", 0, ItemType.Head, ItemAction.Equip, EquipmentStats, "Standard issue Ranger Helmet", false, false);
        Item Chest = new Item("Standard Chestguard", "starterChest", 1, ItemType.Chest, ItemAction.Equip, EquipmentStats, "Standard issue Ranger Chestguard", false, false);
        Item Arms = new Item("Standard Arm Guards", "starterArms", 2, ItemType.Arms, ItemAction.Equip, EquipmentStats, "Standard issue Ranger Arm Guards", false, false);
        Item Waist = new Item("Standard Belt", "starterWaist", 3, ItemType.Waist, ItemAction.Equip, EquipmentStats, "Standard issue Ranger Belt", false, false);
        Item Legs = new Item("Standard Boots", "starterLegs", 4, ItemType.Legs, ItemAction.Equip, EquipmentStats, "Standard issue Ranger Boots", false, false);

        Item Mineral = new Item("Mineral", "mineral", 5, ItemType.Material,ItemAction.None, null, "Shiny", false, false);
        Item Ivory = new Item("Ivory", "ivory", 6, ItemType.Material, ItemAction.None, null, "It's very white", false, false);
        Item Hide = new Item("Hide", "hide", 7, ItemType.Material, ItemAction.None, null, "Taken from a wild animal.", false, false);


        
        database.AddItem(Head);
        database.AddItem(Chest);
        database.AddItem(Arms);
        database.AddItem(Waist);
        database.AddItem(Legs);
        database.AddItem(Mineral);
        database.AddItem(Ivory);
        database.AddItem(Hide);

        ItemDatabaseManager.Instance.UpdateDatabase(database);
        ItemDatabaseManager.Instance.SaveDatabase();
    }

    //// Update is called once per frame
    //void Update () {

    //       if (Input.GetKeyDown(KeyCode.Space))
    //       {
    //           Instantiate(debug);
    //       }
    //}
}
