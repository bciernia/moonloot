using System.Collections.Generic;

[System.Serializable]
public class VillageNpcRuntime
{
    public VillageNpcData Data;

    public string RuntimeID;
    
    public string Name;
    public NPCRole PreferredRole;
    public NPCRole AssignedRole;
    
    public Skill GrantedSkill;
    
    public string Profession;
    public List<NPCUpgradeLevel> UpgradeLevels;

    public WorkerJob CurrentJob;

    public bool IsWorker => Data.Type == NPCType.Worker;
    
    public VillageNpcRuntime(VillageNpcData data)
    {
        var id = GenerateID(data);
        
        Data = data;
        RuntimeID = id;
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

    private string GenerateID(VillageNpcData data) => $"{data.Name}_{data.Profession}";
}