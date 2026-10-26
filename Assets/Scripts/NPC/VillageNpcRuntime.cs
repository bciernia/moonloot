using System.Collections.Generic;

[System.Serializable]
public class VillageNpcRuntime
{
    public VillageNpcData Data;
    public string DataId;

    public string RuntimeID;
    
    public string Name;
    public NPCRole PreferredRole;
    public NPCRole AssignedRole;
    
    public Skill GrantedSkill;
    
    public string Profession;
    public string QuestGiverName;
    public string QuestGiverProfession;
    public List<NPCUpgradeLevel> UpgradeLevels;
    public string AssignedQuestId;

    public WorkerJob CurrentJob;

    public bool IsWorker => Data != null && Data.Type == NPCType.Worker;
    
    public bool IsAssignedToRoom;
    public int AssignedRoomSlotId = -1;
    
    public VillageNpcRuntime(VillageNpcData data)
    {
        Data = data;
        DataId = data != null ? data.name : string.Empty;
        RuntimeID = System.Guid.NewGuid().ToString("N");
        Name = data.Name;
        PreferredRole = data.PreferredRole;
        AssignedRole = data.AssignedRole;
        Profession = data.Profession;
        UpgradeLevels = data.UpgradeLevels;
        
        if (data.PossibleSkills != null && data.PossibleSkills.Count > 0)
        {
            GrantedSkill = data.PossibleSkills[UnityEngine.Random.Range(0, data.PossibleSkills.Count)];
        }
        // else
        // {
            // if (data.PossibleSkills != null) GrantedSkill = data.PossibleSkills[0];
        // }
    }

}
