using System.Collections.Generic;
using UnityEngine;

public class ArmorManager : Singleton<ArmorManager>
{
    [Header("Config")]
    [SerializeField] private InventorySO _inventoryData;
    [SerializeField] private List<ItemParameter> _parametersToModify, _itemCurrentState;
    [SerializeField] private ArmorItemSO _armor;
    
    public void SetArmor(ArmorItemSO armorItem, List<ItemParameter> itemState)
    {
        /*
        //Tworzy duplikat przy przeładowaniu gry, jesli coś było założone        

        if (armorItem != null && _armor != null)
        {
            _inventoryData.AddItem(armorItem, 1, _itemCurrentState);
        }
*/
        _armor = armorItem;
        if (itemState != null)
        {
            _itemCurrentState = new List<ItemParameter>(itemState);
        }

        ModifyParameters();
        EquipArmor(_armor);
    }
    
    private void ModifyParameters()
    {
        foreach (var parameter in _parametersToModify)
        {
            if (_itemCurrentState.Contains(parameter))
            {
                var index = _itemCurrentState.IndexOf(parameter);
                var newValue = _itemCurrentState[index].value + parameter.value;
                _itemCurrentState[index] = new ItemParameter()
                {
                    itemParameter = parameter.itemParameter,
                    value = newValue
                };
            }
        }
    }
    
    private void EquipArmor(ArmorItemSO armor)
    {
        GameManager.Instance.Player.PlayerStats.UpdatePlayerResistances(armor?.DamageResistance ?? 0, armor?.MagicResistance ?? 0);
        PlayerStatisticsManager.Instance.SetDmgResistance(armor?.DamageResistance ?? 0);
        PlayerStatisticsManager.Instance.SetMagicResistance(armor?.MagicResistance ?? 0);
        
        if (armor == null)
        {
            return;
        }

        EquippedItemsManager.Instance.SetItemAsEquipped(armor, ItemType.Armor);
    }
}
