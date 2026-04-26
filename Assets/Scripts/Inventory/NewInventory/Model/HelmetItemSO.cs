using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Helmet", fileName = "Helmet_")]
public class HelmetItemSO : EquippableItemSO, IItemAction
{
    public float PhysicalResistance;
    public float MagicResistance;
    
    public AudioClip actionSfx { get; }
    public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
    {
        var armorSystem = character.transform.parent.GetComponentInChildren<ArmorManager>();

        if (armorSystem != null)
        {
            armorSystem.SetHelmet(this, itemState ?? DefaultParametersList);
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
            description = $"Damage resistance: {PhysicalResistance} \n";
        }

        if (MagicResistance > 0)
        {
            description += $"Magic resistance: {MagicResistance} \n";
        }
        
        return description;
    }
}