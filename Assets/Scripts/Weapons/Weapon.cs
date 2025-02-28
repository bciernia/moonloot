using UnityEngine;

[CreateAssetMenu(fileName = "Weapon_")]
public class Weapon : ScriptableObject
{
    [Header("Configuration")]
    public Sprite Icon;
    public WeaponType WeaponType;
    public float Damage;
    public float timeBetweenAttack;
    
    [Header("Projectile")]
    public Projectile ProjectilePrefab;
    public float RequiredMana;
    public float AmmunitionAmount;
    
    
}