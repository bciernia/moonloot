using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Outfit", fileName = "Outfit_")]
public class OutfitItemSO : EquippableItemSO, IItemAction
{
    public AudioClip actionSfx { get; }
    public RuntimeAnimatorController RuntimeAnimatorController;
    public bool PerformAction(GameObject character, InventoryItem inventoryItem, bool isUsingItem = false, string slotName = "")
    {
        var outfitSystem = character.transform.parent.GetComponentInChildren<OutfitManager>();

        if (outfitSystem != null)
        {
            outfitSystem.SetOutfit(this, inventoryItem.itemState ?? DefaultParametersList);
            EquippedItemsManager.Instance.SetItemAsEquipped(this, ItemType,1,2);
            return true;
        }

        return false;
    }

    public void UnequipOutfit(GameObject character)
    {
        var armorSystem = character.transform.parent.GetComponentInChildren<OutfitManager>();
        armorSystem.SetOutfit(null, null);
    }
}