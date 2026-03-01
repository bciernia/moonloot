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
    
    public void ResetPlayerStats()
    {
        HP = MaxHP;
        MP = MaxMP;
        Level = 1;
        Exp = 0;
        NextLevelExp = InitialNextLevelExp;
    }

    public void UpdatePlayerResistances(float physicalResistance, float magicResistance)
    {
        PhysicalResistance = physicalResistance;
        MagicResistance = magicResistance;

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
}
