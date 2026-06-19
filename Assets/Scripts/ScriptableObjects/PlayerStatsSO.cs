using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Player Stats")]
public class PlayerStatsSO : ScriptableObject
{
    [Header("Configuration")] public int Level;

    [Header("Health")] public float HP;
    public float MaxHP;

    [Header("Mana")] public float MP;
    public float MaxMP;
    
    [Header("Mana")] public float Stamina;
    public float MaxStamina;
    
    [Header("EXP")] public float Exp;
    public float NextLevelExp;
    public float InitialNextLevelExp;
    [Range(1f, 100f)] public float ExpMultiplier;

    public float BaseDamage;
    public float TotalDamage;

    public float PhysicalResistance = 0;
    public float MagicResistance = 0;

    public float AttackCooldownReduction { get; set; } = 1f;
    
    [Range(0, 3)]
    public float ShieldResistance = 1; //Wartość procentowa 1 = 100%, 0.5 - 50%, 1.5 - 150%
    
    [FormerlySerializedAs("CurrentWeapon")] public Weapon currentWeapon;

    private Dictionary<BonusType, float> _npcBonuses = new();
    private Dictionary<BonusType, float> _eqBonuses = new();
    private Dictionary<BonusType, float> _npcFlatBonuses = new();
    private Dictionary<BonusType, float> _eqFlatBonuses = new();
    private Dictionary<BonusType, float> _levelBonuses = new();
    private Dictionary<BonusType, float> _levelFlatBonuses = new();
    
    
    public void ResetPlayerStats()
    {
        HP = GetMaxHp();
        MP = GetMaxMp();
        Level = 1;
        Exp = 0;
        NextLevelExp = InitialNextLevelExp;
    }

    public void RecalculateResistances(float physical)
    {
        PhysicalResistance = physical;

        GetPhysicalReductionPercent();
    }
    
    public float GetPhysicalReductionPercent()
    {
        return PhysicalResistance;
        
        var armorMultiplier = 100f / (100f + PhysicalResistance);
        var finalMultiplier = armorMultiplier * ShieldResistance;

        return (1f - finalMultiplier) * 100f;
    }
    
    // public float GetMagicReductionPercent()
    // {
    //     var magicMultiplier = 100f / (100f + MagicResistance);
    //     var finalMultiplier = magicMultiplier * ShieldResistance;
    //
    //     return (1f - finalMultiplier) * 100f;
    // }

    public void AddNpcBonus(StatBonus bonus)
    {
        if (bonus.Type == BonusType.Damage ||
            bonus.Type == BonusType.MoveSpeed ||
            bonus.Type == BonusType.CritChance ||
            bonus.Type == BonusType.AttackCooldownReduction)
        {
            var normalized = bonus.Value / 100f;

            if (_npcBonuses.ContainsKey(bonus.Type))
                _npcBonuses[bonus.Type] += normalized;
            else
                _npcBonuses[bonus.Type] = normalized;

            return;
        }

        if (_npcFlatBonuses.ContainsKey(bonus.Type))
            _npcFlatBonuses[bonus.Type] += bonus.Value;
        else
            _npcFlatBonuses[bonus.Type] = bonus.Value;
    }
    
    public void AddLevelBonus(StatBonus bonus)
    {
        if (bonus.Type == BonusType.Damage ||
            bonus.Type == BonusType.MoveSpeed ||
            bonus.Type == BonusType.CritChance ||
            bonus.Type == BonusType.AttackCooldownReduction)
        {
            var normalized = bonus.Value / 100f;

            if (_levelBonuses.ContainsKey(bonus.Type))
                _levelBonuses[bonus.Type] += normalized;
            else
                _levelBonuses[bonus.Type] = normalized;

            return;
        }

        if (_levelFlatBonuses.ContainsKey(bonus.Type))
            _levelFlatBonuses[bonus.Type] += bonus.Value;
        else
            _levelFlatBonuses[bonus.Type] = bonus.Value;
    }
    
    private float GetMultiplier(BonusType type)
    {
        var total = 1f;

        if (_npcBonuses.TryGetValue(type, out var npc))
            total += npc;

        if (_eqBonuses.TryGetValue(type, out var eq))
            total += eq;
        
        if (_levelBonuses.TryGetValue(type, out var level))
            total += level;

        return total;
    }

    private float GetNpcBonus(BonusType type)
    {
        return _npcBonuses.GetValueOrDefault(type, 0f);
    }

    public float GetMoveSpeedMultiplier()
    {
        return GetMultiplier(BonusType.MoveSpeed);
    }

    public float GetDamageBonusMultiplier()
    {
        return GetMultiplier(BonusType.Damage);
    }

    public float GetCritChanceBonusMultiplier()
    {
        return GetMultiplier(BonusType.CritChance);
    }

    public float GetCritMultiplier()
    {
        var baseCrit = 1.5f;

        var bonus = 0f;

        bonus += _npcFlatBonuses.GetValueOrDefault(BonusType.CritMultiplier, 0f);
        bonus += _eqFlatBonuses.GetValueOrDefault(BonusType.CritMultiplier, 0f);

        return baseCrit + bonus;
    }
    
    public float GetBonusValue(BonusType type)
    {
        var total = 0f;

        total += _npcBonuses.GetValueOrDefault(type, 0f);
        total += _eqBonuses.GetValueOrDefault(type, 0f);

        return total;
    }
    
    
    private float GetNpcFlatBonus(BonusType type)
    {
        return _npcFlatBonuses.GetValueOrDefault(type, 0f);
    }
    
    private float GetEqFlatBonus(BonusType type)
    {
        return _eqFlatBonuses.GetValueOrDefault(type, 0f);
    }    
    
    private float GetLevelFlatBonus(BonusType type)
    {
        return _levelFlatBonuses.GetValueOrDefault(type, 0f);
    }

    public float GetMaxHp() => MaxHP + GetNpcFlatBonus(BonusType.MaxHp) + GetEqFlatBonus(BonusType.MaxHp) + GetLevelFlatBonus(BonusType.MaxHp);
    public float GetMaxMp() => MaxMP + GetNpcFlatBonus(BonusType.MaxMp);
    
    public void ResetEquipmentBonuses()
    {
        _eqBonuses.Clear();
    }
    
    public void ResetEquipmentFlatBonuses()
    {
        _eqFlatBonuses.Clear();
    }
    
    public void AddEquipmentBonus(StatBonus bonus)
    {
        var normalized = bonus.Value / 100f;

        if (bonus.Type == BonusType.Damage ||
            bonus.Type == BonusType.MoveSpeed ||
            bonus.Type == BonusType.CritChance ||
            bonus.Type == BonusType.AttackCooldownReduction)
        {
            if (_eqBonuses.ContainsKey(bonus.Type))
                _eqBonuses[bonus.Type] += normalized;
            else
                _eqBonuses[bonus.Type] = normalized;

            return;
        }

        if (_eqFlatBonuses.ContainsKey(bonus.Type))
            _eqFlatBonuses[bonus.Type] += bonus.Value;
        else
            _eqFlatBonuses[bonus.Type] = bonus.Value;
    }
    
    public void ResetNpcBonuses()
    {
        _npcBonuses.Clear();
        _npcFlatBonuses.Clear();
    }
}
