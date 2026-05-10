using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

[CreateAssetMenu(fileName = "VillageNPC_", menuName = "Game/NPC/Village NPC")]
public class VillageNpcData : NPCData
{
    public NPCRole AssignedRole;
    public NPCRole PreferredRole;
    public List<Skill> PossibleSkills;
    public List<SkillStatModifier> SkillStatModifiers;
}