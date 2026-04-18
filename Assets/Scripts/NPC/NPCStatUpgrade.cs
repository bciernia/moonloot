using UnityEngine;

public class NPCStatUpgrade : MonoBehaviour
{ 
    [SerializeField] private NPCData npc;

    //Musi być public bo użyte w dialogach
    public bool TryUpgrade()
    {
        var success = NPCManager.Instance.TryUpgradeNPC(npc);

        if (!success) return false;
        
        NPCManager.Instance.ApplyNPC(npc);
        Debug.Log("Upgrade + bonus applied");
        return true;
    }

    public string GetItemNameToUpgrade() => npc.UpgradeLevels[GetLevel()].ItemName;
    public string GetItemAmountToUpgrade() => npc.UpgradeLevels[GetLevel()].RequiredAmount.ToString();
    
    //Musi być public bo użyte w dialogach
    public int GetLevel()
    {
        return NPCManager.Instance.GetLevel(npc.Type);
    }

    public string GetLevelAsText() => GetLevel().ToString();

    //Musi być public bo użyte w dialogach
    public string GetNewLevel()
    {
        var currentLevel = NPCManager.Instance.GetLevel(npc.Type) + 1;

        return currentLevel.ToString();
    }

    public string GetNpcName() => npc.Name;
}