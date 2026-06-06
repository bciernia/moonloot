using System.Collections.Generic;

[System.Serializable]
public class NPCUpgradeLevel
{
    public int Level;
    public List<StatBonus> Bonuses;
    public ItemSO Item;
    public int RequiredAmount;
    public int GoldAmount;
}