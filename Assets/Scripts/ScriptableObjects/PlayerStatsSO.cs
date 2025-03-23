using UnityEngine;

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

    public WeaponSO CurrentWeapon;
    
    public void ResetPlayerStats()
    {
        HP = MaxHP;
        MP = MaxMP;
        Level = 1;
        Exp = 0;
        NextLevelExp = InitialNextLevelExp;
    }
}
