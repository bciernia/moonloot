using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class OutfitItemSO : EquippableItemSO, IItemAction
{
    public AudioClip actionSfx { get; }
    public RuntimeAnimatorController RuntimeAnimatorController;
    public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
    {
        var outfitSystem = character.transform.parent.GetComponentInChildren<OutfitManager>();

        if (outfitSystem != null)
        {
            outfitSystem.SetOutfit(this, itemState ?? DefaultParametersList);
            EquippedItemsManager.Instance.SetItemAsEquipped(this, ItemType);
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