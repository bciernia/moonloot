using UnityEngine;

[CreateAssetMenu(menuName = "Items/Weapon", fileName = "ItemWeapon_")]
public class ItemWeapon : InventoryItem
{
    [Header("Weapon")] public Weapon Weapon;
}