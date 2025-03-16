using UnityEngine;

[CreateAssetMenu(fileName = "Weapon_")]
public class WeaponSO : ScriptableObject
{
    [Header("Configuration")]
    public Sprite Icon;
    public WeaponType WeaponType;
    public Vector2 SlashSize;
    public Vector2 SlashOffset;
    
    public float Damage;
    public float timeBetweenAttack;

    public float RequiredStamina;
    
    [Header("Projectile")]
    public Projectile ProjectilePrefab;
    public float RequiredMana;
    public float AmmunitionAmount;
}