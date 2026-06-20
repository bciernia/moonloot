using UnityEngine;

[CreateAssetMenu(fileName = "RoomUpgrade", menuName = "Moonloot/Tavern/Room Upgrade")]
public class RoomUpgradeSO : ScriptableObject
{
    public string Id;
    
    public string Name;
    [TextArea]
    public string Description;

    public int Cost;
    public int RequiredVillagers;

    public BonusType BonusType;
    public float Value;
}