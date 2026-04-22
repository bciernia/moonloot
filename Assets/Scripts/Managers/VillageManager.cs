using System.Collections.Generic;
using UnityEngine;

public class VillageManager : Singleton<VillageManager>
{
    public List<VillageNpcRuntime> npcs = new();

    public void AddNPC(VillageNpcData data)
    {
        if (npcs.Exists(n => n.Data == data))
            return;

        var npc = new VillageNpcRuntime(data);
        npcs.Add(npc);
    }

    public void AssignRole(VillageNpcRuntime npc, NPCRole role)
    {
        npc.AssignedRole = role;
    }
}