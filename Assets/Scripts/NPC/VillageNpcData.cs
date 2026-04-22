using UnityEngine;

[System.Serializable]

[CreateAssetMenu(fileName = "VillageNPC_", menuName = "Game/NPC/Village NPC")]
public class VillageNpcData : NPCData
{
    public NPCRole AssignedRole;
    public NPCRole PreferredRole;
}