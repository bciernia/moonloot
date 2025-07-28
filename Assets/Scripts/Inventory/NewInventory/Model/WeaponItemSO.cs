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

    [field: SerializeField] public float AmmunitionAmount;
    
    
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
}