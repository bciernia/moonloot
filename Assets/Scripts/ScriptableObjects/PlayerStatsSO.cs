using System.Collections.Generic;
using Mono.Cecil.Cil;
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

    
    [Range(0, 3)]
    public float ShieldResistance = 1; //Wartość procentowa 1 = 100%, 0.5 - 50%, 1.5 - 150%
    
    [FormerlySerializedAs("CurrentWeapon")] public Weapon currentWeapon;

    private Dictionary<BonusType, float> _bonusMultipliers = new();
    private Dictionary<BonusType, float> _flatBonuses = new();
    
    public void ResetPlayerStats()
    {
        HP = GetMaxHp();
        MP = GetMaxMp();
        Level = 1;
        Exp = 0;
        NextLevelExp = InitialNextLevelExp;
    }

    public void RecalculateResistances(float physical, float magic)
    {
        PhysicalResistance = physical;
        MagicResistance = magic;

        GetPhysicalReductionPercent();
        GetMagicReductionPercent();
    }
    
    public float GetPhysicalReductionPercent()
    {
        var armorMultiplier = 100f / (100f + PhysicalResistance);
        var finalMultiplier = armorMultiplier * ShieldResistance;

        return (1f - finalMultiplier) * 100f;
    }
    
    public float GetMagicReductionPercent()
    {
        var magicMultiplier = 100f / (100f + MagicResistance);
        var finalMultiplier = magicMultiplier * ShieldResistance;

        return (1f - finalMultiplier) * 100f;
    }

    public void AddBonus(StatBonus bonus)
    {
        if (bonus.Type == BonusType.Damage ||
            bonus.Type == BonusType.MoveSpeed ||
            bonus.Type == BonusType.Crit)
        {
            var normalized = bonus.Value / 100f;

            if (_bonusMultipliers.ContainsKey(bonus.Type))
                _bonusMultipliers[bonus.Type] += normalized;
            else
                _bonusMultipliers[bonus.Type] = normalized;

            return;
        }

        if (_flatBonuses.ContainsKey(bonus.Type))
            _flatBonuses[bonus.Type] += bonus.Value;
        else
            _flatBonuses[bonus.Type] = bonus.Value;
    }
    
    private float GetMultiplier(BonusType type)
    {
        if (_bonusMultipliers.TryGetValue(type, out float value))
            return 1f + value;

        return 1f;
    }

    private float GetBonus(BonusType type)
    {
        return _bonusMultipliers.GetValueOrDefault(type, 0f);
    }

    public float GetMoveSpeedMultiplier()
    {
        return GetMultiplier(BonusType.MoveSpeed);
    }

    public float GetDamageBonusMultiplier()
    {
        return GetMultiplier(BonusType.Damage);
    }

    public float GetCritBonusMultiplier()
    {
        return GetMultiplier(BonusType.Crit);
    }
    
    public float GetBonusValue(BonusType type)
    {
        return _bonusMultipliers.GetValueOrDefault(type, 0f);
    }
    
    private float GetFlatBonus(BonusType type)
    {
        return _flatBonuses.GetValueOrDefault(type, 0f);
    }

    public float GetMaxHp() => MaxHP + GetFlatBonus(BonusType.MaxHp);
    public float GetMaxMp() => MaxMP + GetFlatBonus(BonusType.MaxMp);
}
