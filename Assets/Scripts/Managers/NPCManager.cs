using System.Collections.Generic;
using UnityEngine;

public class NPCManager : Singleton<NPCManager>
{
    [SerializeField] private PlayerStatsSO _playerStats;

    private Dictionary<string, int> _npcLevels = new();
    
    public int GetLevel(VillageNpcRuntime npc)
    {
        return npc == null ? 1 : _npcLevels.GetValueOrDefault(npc.RuntimeID, 1);
    }

    public void ApplyNPC(VillageNpcRuntime npc)
    {
        if (npc == null) return;

        var level = GetLevel(npc);

        switch (npc.Data.Group)
        {
            case NPCGroup.Stat:
                ApplyStatNPC(npc.Data, level);
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
            _playerStats.AddNpcBonus(bonus);
        }

        Player.Instance.PlayerAttack.RecalculateDamage();
    }

    public bool TryUpgradeNPC(VillageNpcRuntime npc)
    {
        var currentLevel = GetLevel(npc);
        var nextLevel = currentLevel + 1;

        var nextLevelData = npc.UpgradeLevels.Find(l => l.Level == nextLevel);

        if (nextLevelData == null)
        {
            Debug.Log("Max level reached");
            return false;
        }

        if (!InventoryController.Instance.HasUserQuestItem(nextLevelData.Item.Name, nextLevelData.RequiredAmount))
        {
            Debug.Log("Not enough items");
            return false;
        }

        if (!InventoryController.Instance.ChangeGoldAmount(-nextLevelData.GoldAmount))
        {
            Debug.Log("Not enough gold");
            return false;
        }
        
        InventoryController.Instance.TryRemoveQuestItems(nextLevelData.Item.Name, nextLevelData.RequiredAmount);
        _npcLevels[npc.RuntimeID] = nextLevel;
        Debug.Log($"{npc.Name} upgraded to level {nextLevel}");

        return true;
    }
}