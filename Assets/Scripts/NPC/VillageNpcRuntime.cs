[System.Serializable]
public class VillageNpcRuntime
{
    public VillageNpcData Data;

    public string Name;
    public NPCRole PreferredRole;
    public NPCRole AssignedRole;

    public VillageNpcRuntime(VillageNpcData data)
    {
        Data = data;
        Name = data.Name;
        PreferredRole = data.PreferredRole;
        AssignedRole = data.AssignedRole;
    }
}