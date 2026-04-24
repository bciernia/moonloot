using UnityEngine;

public class NPCInstance : MonoBehaviour
{
    public VillageNpcRuntime Runtime { get; private set; }

    public void Initialize(VillageNpcRuntime runtime)
    {
        Runtime = runtime;
    }

    public string GetRuntimeID()
    {
        return Runtime?.RuntimeID;
    }

    public VillageNpcData GetData()
    {
        return Runtime?.Data;
    }
}