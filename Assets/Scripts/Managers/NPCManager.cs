using System.Collections.Generic;
using UnityEngine;

public class NPCManager : Singleton<NPCManager>
{
    [SerializeField] private PlayerStatsSO _playerStats;

    private Dictionary<NPCType, int> _npcLevels = new();
    
    public int GetLevel(NPCType type)
    {
        return _npcLevels.GetValueOrDefault(type, 1);
    }

    public void ApplyNPC(NPCData npc)
    {
        if (npc == null) return;

        var level = GetLevel(npc.Type);

        switch (npc.Group)
        {
            case NPCGroup.Stat:
                ApplyStatNPC(npc, level);
                break;

            case NPCGroup.Merchant:
                Debug.Log("Merchant unlocked");
                break;

            case NPCGroup.Hero:
                Debug.Log("Hero unlocked");
                break;
        }
    }

    private void ApplyStatNPC(NPCData npc, int level)
    {
        var levelData = npc.UpgradeLevels.Find(l => l.Level == level);

        if (levelData == null)
        {
            Debug.LogWarning($"No level data for {npc.Name} at level {level}");
            return;
        }

        foreach (var bonus in levelData.Bonuses)
        {
            _playerStats.AddBonus(bonus);
        }

        Player.Instance.PlayerAttack.RecalculateDamage();
    }

    public bool TryUpgradeNPC(NPCData npc)
    {
        var currentLevel = GetLevel(npc.Type);
        var nextLevel = currentLevel + 1;

        var nextLevelData = npc.UpgradeLevels.Find(l => l.Level == nextLevel);

        if (nextLevelData == null)
        {
            Debug.Log("Max level reached");
            return false;
        }

        if (!InventoryController.Instance.HasUserQuestItem(nextLevelData.ItemName, nextLevelData.RequiredAmount))
        {
            Debug.Log("Not enough items");
            return false;
        }
        
        InventoryController.Instance.TryRemoveQuestItems(nextLevelData.ItemName, nextLevelData.RequiredAmount);
        _npcLevels[npc.Type] = nextLevel;
        Debug.Log($"{npc.Name} upgraded to level {nextLevel}");

        return true;
    }
}