using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class WeaponItemSO : EquippableItemSO, IItemAction
{
    [field: SerializeField]
    public WeaponType WeaponType;

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
    
    [Header("Projectile")]
    [field: SerializeField]
    public Projectile ProjectilePrefab;
    [field: SerializeField]
    public float RequiredMana;

    [field: SerializeField] private WeaponItemSO defaultWeapon;

    [field: SerializeField] public float AmmunitionAmount;

    public override string GetStatsDescription()
    {
        var description = $"Damage: {Damage} \n";
        description += $"Attack cooldown: {timeBetweenAttack} \n";
        
        if (RequiredMana > 0)
        {
            description += $"Required mana: {RequiredMana} \n";
        }
        
        return description;
    }

    [field: SerializeField] public AudioClip actionSfx { get; private set; }
    public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
    {
        var weaponSystem = character.transform.parent.GetComponentInChildren<WeaponManager>();

        if (weaponSystem != null)
        {
            weaponSystem.SetWeapon(this, itemState ?? DefaultParametersList);
            EquippedItemsManager.Instance.SetItemAsEquipped(this);
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