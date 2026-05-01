using System;
using UnityEngine;

public class NPCStatUpgrade : MonoBehaviour
{ 
    [SerializeField] private VillageNpcData npc;

    public string Id { get; set; }

    private VillageNpcRuntime _runtimeNpc;

    private void Awake()
    {
        Id = $"{npc.Name}_{npc.Type.ToString()}";
        _runtimeNpc = new VillageNpcRuntime(npc);
    }

    //Musi być public bo użyte w dialogach
    public bool TryUpgrade()
    {
        var success = NPCManager.Instance.TryUpgradeNPC(_runtimeNpc);

        if (!success) return false;
        
        NPCManager.Instance.ApplyNPC(_runtimeNpc);
        Debug.Log("Upgrade + bonus applied");
        return true;
    }

    public string GetItemNameToUpgrade() => npc.UpgradeLevels[GetLevel()].ItemName;
    public string GetItemAmountToUpgrade() => npc.UpgradeLevels[GetLevel()].RequiredAmount.ToString();
    
    //Musi być public bo użyte w dialogach
    public int GetLevel()
    {
        return NPCManager.Instance.GetLevel(_runtimeNpc);
    }

    public string GetLevelAsText() => GetLevel().ToString();

    //Musi być public bo użyte w dialogach
    public string GetNewLevel()
    {
        var currentLevel = NPCManager.Instance.GetLevel(_runtimeNpc) + 1;

        return currentLevel.ToString();
    }

    public string GetNpcName() => npc.Name;

    public bool TryUnlockSkill() => _runtimeNpc.GrantedSkill != null && PlayerSkillManager.Instance.UnlockSkill(_runtimeNpc.GrantedSkill);
    
    
    public bool IsSkillUnlocked() => PlayerSkillManager.Instance.IsUnlocked(_runtimeNpc.GrantedSkill);

    public string GetGrantedSkillName()
    {
        var test = _runtimeNpc.GrantedSkill.Name;
        return test;
    }
    
    public string GetGrantedSkillDescription => _runtimeNpc.GrantedSkill.NpcDescription;
}