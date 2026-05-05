using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Shoes", fileName = "Shoes_")]
public class ShoesItemSO : EquippableItemSO, IItemAction
{
    public float MovementSpeedBonus;
    
    public AudioClip actionSfx { get; }
    public bool PerformAction(GameObject character, InventoryItem inventoryItem, bool isUsingItem = false, string slotName = "")
    {
        var armorSystem = character.transform.parent.GetComponentInChildren<ArmorManager>();

        if (armorSystem != null)
        {
            armorSystem.SetShoes(this, inventoryItem.itemState ?? DefaultParametersList);
            EquippedItemsManager.Instance.SetItemAsEquipped(this, ItemType, 1, 4);
            return true;
        }

        return false;
    }

    public void Unequip(GameObject character)
    {
        var armorSystem = character.transform.parent.GetComponentInChildren<ArmorManager>();
        armorSystem.SetShoes(null, null);
    }

    public override string GetStatsDescription()
    {
        var description = "";

        if (MovementSpeedBonus > 0)
        {
            description = $"Movement speed bonus: {MovementSpeedBonus}% \n";
        }
        
        return description;
    }
}