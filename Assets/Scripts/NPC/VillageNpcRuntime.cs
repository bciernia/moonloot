using System.Collections.Generic;

[System.Serializable]
public class VillageNpcRuntime
{
    public VillageNpcData Data;

    public string RuntimeID;
    
    public string Name;
    public NPCRole PreferredRole;
    public NPCRole AssignedRole;
    
    public string Profession;
    public List<NPCUpgradeLevel> UpgradeLevels;

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
    }

    private string GenerateID(VillageNpcData data) => $"{data.Name}_{data.Profession}";
}