using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BaseStatType
{
    Health,
    Energy,
    Attack,
    Defense,
    Speed,
}

[System.Serializable]
public class BaseStat
{
    public BaseStatType statType;

    public List<StatBonus> StatBonuses { get; set; }

    public int baseValue;

    public string statDescription;

    public BaseStat(BaseStatType statType, int baseValue, string statDescription)
    {
        StatBonuses =  new List<StatBonus>();
        this.statType = statType;
        this.baseValue = baseValue;
        this.statDescription = statDescription;
    }


    public void AddStatBonus(StatBonus statBonus)
    {
        StatBonuses.Add(statBonus);
    }

    public void RemoveStatBonus(StatBonus statBonus)
    {
        StatBonuses.Remove(StatBonuses.Find(x => x.BonusValue == statBonus.BonusValue));
    }

    public int GetCalculatedStatValue()
    {
        int finalValue = 0;
        StatBonuses.ForEach(x => finalValue += x.BonusValue);
        finalValue += baseValue;
        return finalValue;
    }

    public int GetTotalBonusValue()
    {
        int finalValue = 0;
        StatBonuses.ForEach(x => finalValue += x.BonusValue);
        return finalValue;
    }

}
