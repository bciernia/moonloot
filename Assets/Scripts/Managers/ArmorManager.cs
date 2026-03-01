using System.Collections.Generic;
using UnityEngine;

public class ArmorManager : Singleton<ArmorManager>
{
    [Header("Config")]
    [SerializeField] private InventorySO _inventoryData;
    [SerializeField] private List<ItemParameter> _parametersToModify, _itemCurrentState;
    [SerializeField] private ArmorItemSO _armor;
    
    public void SetArmor(ArmorItemSO armorItem, List<ItemParameter> itemState, bool isFromLoading = false)
    {
        //Tworzy duplikat przy przeładowaniu gry, jesli coś było założone        

        if (armorItem != null && _armor != null && !isFromLoading)
        {
            _inventoryData.AddItem(_armor, 1, _itemCurrentState);
        }
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
        GameManager.Instance.Player.PlayerStats.UpdatePlayerResistances(armor?.PhysicalResistance ?? 0, armor?.MagicResistance ?? 0);
        GameManager.Instance.Player.GetComponent<PlayerHealth>().RefreshResistanceUI();
        if (armor == null)
        {
            return;
        }

        EquippedItemsManager.Instance.SetItemAsEquipped(armor, ItemType.Armor);
    }
}
