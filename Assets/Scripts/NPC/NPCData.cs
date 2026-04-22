using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPC_", menuName = "Game/NPC")]
public abstract class NPCData : ScriptableObject
{
    
    public NPCGroup Group;

    public NPCType Type;
    
    public string Name;
    public string Profession;

    public Sprite Portrait;
    public GameObject Character;

    public List<NPCUpgradeLevel> UpgradeLevels;
}