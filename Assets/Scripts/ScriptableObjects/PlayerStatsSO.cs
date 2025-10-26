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

    public float DamageResistance = 0;
    public float MagicResistance = 0;
    
    [FormerlySerializedAs("CurrentWeapon")] public Weapon currentWeapon;
    
    public void ResetPlayerStats()
    {
        HP = MaxHP;
        MP = MaxMP;
        Level = 1;
        Exp = 0;
        NextLevelExp = InitialNextLevelExp;
    }

    public void UpdatePlayerResistances(float damageResistance, float magicResistance)
    {
        DamageResistance += damageResistance;
        MagicResistance += magicResistance;
    }
}
