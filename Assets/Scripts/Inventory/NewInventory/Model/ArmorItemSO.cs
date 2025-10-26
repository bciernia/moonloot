using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ArmorItemSO : EquippableItemSO, IItemAction
{
    public float DamageResistance;
    public float MagicResistance;
    
    public AudioClip actionSfx { get; }
    public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
    {
        var armorSystem = character.transform.parent.GetComponentInChildren<ArmorManager>();

        if (armorSystem != null)
        {
            armorSystem.SetArmor(this, itemState ?? DefaultParametersList);
            EquippedItemsManager.Instance.SetItemAsEquipped(this, ItemType);
            return true;
        }

        return false;
    }

    public void UnequipWeapon(GameObject character)
    {
        var armorSystem = character.transform.parent.GetComponentInChildren<ArmorManager>();
        armorSystem.SetArmor(null, null);
    }
}