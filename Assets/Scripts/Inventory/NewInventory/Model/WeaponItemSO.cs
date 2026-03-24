using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Weapon", fileName = "Weapon_")]
public class WeaponItemSO : EquippableItemSO, IItemAction
{
    [field: SerializeField]
    public WeaponType WeaponType;
    [field: SerializeField] private WeaponItemSO defaultWeapon;


    [field: SerializeField]
    public Vector2 SlashSize;
    [field: SerializeField]
    public Vector2 SlashOffset;
    
    [field: SerializeField]
    public float Damage;
    [field: SerializeField]
    public float timeBetweenAttack;
    [field: SerializeField]
    public float RequiredStamina;
    
    [Header("Sound")]
    [field: SerializeField] public AudioClip actionSfx { get; private set; }
    [field: SerializeField] public SoundType AttackSoundType { get; private set; }
    [field: SerializeField] public SoundType HitSoundType { get; private set; }

    [Header("Projectile")]
    [field: SerializeField]
    public Projectile ProjectilePrefab;
    [field: SerializeField] public float AmmunitionAmount;


    [Header("Effect modifiers")]
    [field: SerializeField] public Effect Effect;
    [field: SerializeField] public float EffectChance;

    public override string GetStatsDescription()
    {
        var description = $"Damage: {Damage} \n";
        description += $"Attack cooldown: {timeBetweenAttack} \n";
        
        if (ProjectilePrefab?.ProjectileSo?.ManaCost > 0)
        {
            description += $"Required mana: {ProjectilePrefab.ProjectileSo.ManaCost} \n";
        }

        if (Effect != null)
        {
            description += $"\nAttack effect: {Effect.Name} \n{Effect.Description}\n";
            description += $"Chance for effect: {EffectChance} \n";
        }
        
        return description;
    }

    public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
    {
        var weaponSystem = character.transform.parent.GetComponentInChildren<WeaponManager>();

        if (weaponSystem != null)
        {
            weaponSystem.SetWeapon(this, itemState ?? DefaultParametersList);
            EquippedItemsManager.Instance.SetItemAsEquipped(this, ItemType);
            return true;
        }

        return false;
    }

    public void UnequipWeapon(GameObject character)
    {
        var weaponSystem = character.transform.parent.GetComponentInChildren<WeaponManager>();
        weaponSystem.SetWeapon(defaultWeapon, null);
    }
}