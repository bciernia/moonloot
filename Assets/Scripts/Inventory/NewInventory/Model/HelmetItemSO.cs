using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Helmet", fileName = "Helmet_")]
public class HelmetItemSO : EquippableItemSO, IItemAction
{
    public float AdditionalHp;
    public float PhysicalResistance;
    
    public AudioClip actionSfx { get; }
    public bool PerformAction(GameObject character, InventoryItem inventoryItem, bool isUsingItem = false, string slotName = "")
    {
        var armorSystem = character.transform.parent.GetComponentInChildren<ArmorManager>();

        if (armorSystem != null)
        {
            armorSystem.SetHelmet(this, inventoryItem.itemState ?? DefaultParametersList);
            EquippedItemsManager.Instance.SetItemAsEquipped(this, ItemType);
            return true;
        }

        return false;
    }

    public void Unequip(GameObject character)
    {
        var armorSystem = character.transform.parent.GetComponentInChildren<ArmorManager>();
        armorSystem.SetHelmet(null, null);
    }

    public override string GetStatsDescription()
    {
        var description = "";

        if (PhysicalResistance > 0)
        {
            description = $"Damage resistance: {PhysicalResistance}% \n";
        }
        
        if (AdditionalHp > 0)
        {
            description += $"Health points: +{AdditionalHp} \n";
        }

        return description;
    }
}