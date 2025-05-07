using UnityEngine;

[CreateAssetMenu(fileName = "Weapon_")]
public class Weapon : ScriptableObject
{
    [Header("Configuration")]
    public WeaponType WeaponType;
    public Vector2 SlashSize;
    public Vector2 SlashOffset;
    public Sprite Icon;
    
    public float Damage;
    public float timeBetweenAttack;

    public float RequiredStamina;
    
    [Header("Projectile")]
    public Projectile ProjectilePrefab;
    public float RequiredMana;
    public float AmmunitionAmount;
}