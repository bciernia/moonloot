using System.Collections.Generic;

[System.Serializable]
public class NPCUpgradeLevel
{
    public int Level;
    public List<StatBonus> Bonuses;
    public string ItemName;
    public int RequiredAmount;
    public int GoldAmount;
}