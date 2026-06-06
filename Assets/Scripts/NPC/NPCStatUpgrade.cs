using Unity.Collections;
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

        if (_runtimeNpc.GrantedSkill != null)
        {
            if (!IsSkillUnlocked())
            {
                TryUnlockSkill();
            }
            else
            {
                TryUpgradeSkill();
            }
        }
        
        Debug.Log("Upgrade + bonus applied");
        return true;
    }

    public string GetItemNameToUpgrade() => npc.UpgradeLevels[GetLevel()].Item.Name;
    public string GetItemAmountToUpgrade() => npc.UpgradeLevels[GetLevel()].RequiredAmount.ToString();
    
    public string GetStatName()
    {
        if (npc.UpgradeLevels.Count == 0)
            return string.Empty;

        if (npc.UpgradeLevels[0].Bonuses.Count == 0)
            return string.Empty;

        return GetBonusDisplayName(
            npc.UpgradeLevels[0].Bonuses[0].Type
        );
    }
    
    public ItemSO GetRequiredItem()
    {
        return npc.UpgradeLevels[GetLevel()].Item;
    }

    public int GetRequiredAmount()
    {
        return npc.UpgradeLevels[GetLevel()].RequiredAmount;
    }

    public int GetRequiredGold()
    {
        return npc.UpgradeLevels[GetLevel()].GoldAmount;
    }
    
    public float GetCurrentBonusValue()
    {
        float total = 0;

        var currentLevel = GetLevel();

        for (var i = 0; i < currentLevel; i++)
        {
            total += npc.UpgradeLevels[i].Bonuses[0].Value;
        }

        return total;
    }
    
    public float GetNextBonusValue()
    {
        var total = GetCurrentBonusValue();

        var currentLevel = GetLevel();

        if (currentLevel >= npc.UpgradeLevels.Count)
            return total;

        total += npc.UpgradeLevels[currentLevel].Bonuses[0].Value;

        return total;
    }
    
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

    public BonusType GetNpcBonus() => npc.UpgradeLevels[0].Bonuses[0].Type;
    
    public string GetNpcName() => npc.Name;
    public string GetNpcProfession() => npc.Profession;

    public bool TryUnlockSkill() => _runtimeNpc.GrantedSkill != null && PlayerSkillManager.Instance.UnlockSkill(_runtimeNpc.GrantedSkill);
    
    public bool TryUpgradeSkill()
    {
        if (_runtimeNpc.GrantedSkill == null)
            return false;

        foreach (var upgrade in npc.SkillStatModifiers)
        {
            PlayerSkillManager.Instance.AddSkillModifier(
                _runtimeNpc.GrantedSkill,
                upgrade.statType,
                upgrade.value
            );
        }

        Debug.Log($"Skill upgraded: {_runtimeNpc.GrantedSkill.Name}");

        return true;
    }
    
    public bool IsSkillUnlocked() => PlayerSkillManager.Instance.IsUnlocked(_runtimeNpc.GrantedSkill);

    public string GetGrantedSkillName() => _runtimeNpc.GrantedSkill.Name;
    public Sprite GetGrantedSkillIcon() => _runtimeNpc.GrantedSkill.Icon;
    public string GetGrantedSkillDescription => _runtimeNpc.GrantedSkill.NpcDescription;
    
    public void OpenStatPanel()
    {
        NPCUpgradePanelManager.Instance.Show(this);
    }
    
    private string GetBonusDisplayName(BonusType bonusType)
    {
        return bonusType switch
        {
            BonusType.Damage => "Damage",
            BonusType.MoveSpeed => "Move Speed",
            BonusType.CritChance => "Critical Chance",
            BonusType.CritMultiplier => "Critical Damage",
            BonusType.MaxHp => "Maximum HP",
            BonusType.MaxMp => "Maximum Mana",
            BonusType.AttackCooldownReduction => "Attack Speed",
            _ => bonusType.ToString()
        };
    }
    
    public int GetMaxLevel()
    {
        return npc.UpgradeLevels.Count;
    }
    
    public SkillStatModifier GetSkillModifier()
    {
        if (npc.SkillStatModifiers == null || npc.SkillStatModifiers.Count == 0)
            return null;

        return npc.SkillStatModifiers[0];
    }
    
    public string GetSkillUpgradeText()
    {
        var modifier = GetSkillModifier();

        if (modifier == null)
            return string.Empty;

        if (!IsSkillUnlocked())
            return "Unlock";

        var currentLevel = GetLevel();

        var currentValue = modifier.value;
        var nextValue = modifier.value * currentLevel;

        return $"{GetSkillStatName(modifier.statType)} {FormatValue(modifier.statType, currentValue)} -> {FormatValue(modifier.statType, nextValue)}";
    }
    
    private string GetSkillStatName(SkillStatType type)
    {
        return type switch
        {
            SkillStatType.Radius => "Radius",
            SkillStatType.Duration => "Duration",
            SkillStatType.Cooldown => "Cooldown",
            SkillStatType.Damage => "Damage",
            SkillStatType.ProjectileCount => "Projectiles",
            SkillStatType.ProjectileInterval => "Interval",
            SkillStatType.TargetCount => "Targets",
            SkillStatType.ShieldReduction => "Shield Reduction",
            SkillStatType.ManaCost => "Mana Cost",
            SkillStatType.DamageMultiplier => "Damage Multiplier",
            SkillStatType.SpeedMultiplier => "Speed Multiplier",
            SkillStatType.HealAmount => "Heal amount",
            _ => type.ToString()
        };
    }
    
    private string FormatValue(SkillStatType type, float value)
    {
        return type switch
        {
            SkillStatType.Duration => $"{value:0.#}s",
            SkillStatType.Cooldown => $"{value:0.#}s",
            SkillStatType.ProjectileInterval => $"{value:0.#}s",

            SkillStatType.DamageMultiplier => $"{value:0.#}%",
            SkillStatType.SpeedMultiplier => $"{value:0.#}%",
            SkillStatType.ShieldReduction => $"{value:0.#}%",

            _ => value.ToString("0.#")
        };
    }
    
    
}
