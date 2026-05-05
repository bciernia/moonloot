using System.Collections.Generic;
using UnityEngine;

public class ArmorManager : Singleton<ArmorManager>
{
    [Header("Config")]
    [SerializeField] private InventorySO _inventoryData;
    [SerializeField] private List<ItemParameter> _parametersToModify, _itemCurrentState;
    [SerializeField] private ArmorItemSO _armor;
    [SerializeField] private HelmetItemSO _helmet;
    [SerializeField] private ShoesItemSO _shoes;

    #region Armor

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
    
    private void EquipArmor(ArmorItemSO armor)
    {
        RecalculateAllBonusesFromArmor();
        if (armor == null)
        {
            return;
        }

        EquippedItemsManager.Instance.SetItemAsEquipped(armor, ItemType.Armor, 1, 1);
    }

    #endregion

    #region Helmet

    public void SetHelmet(HelmetItemSO armorItem, List<ItemParameter> itemState, bool isFromLoading = false)
    {
        //Tworzy duplikat przy przeładowaniu gry, jesli coś było założone    
        if (armorItem != null && _helmet != null && !isFromLoading)
        {
            _inventoryData.AddItem(_helmet, 1, _itemCurrentState);
        }
        _helmet = armorItem;
        if (itemState != null)
        {
            _itemCurrentState = new List<ItemParameter>(itemState);
        }

        ModifyParameters();
        EquipHelmet(_helmet);
    }
    
    
    private void EquipHelmet(HelmetItemSO helmet)
    {
        RecalculateAllBonusesFromArmor();
        if (helmet == null)
        {
            return;
        }

        EquippedItemsManager.Instance.SetItemAsEquipped(helmet, ItemType.Helmet, 1, 3);
    }

    #endregion

    #region Shoes

    public void SetShoes(ShoesItemSO shoes, List<ItemParameter> itemState, bool isFromLoading = false)
    {
        //Tworzy duplikat przy przeładowaniu gry, jesli coś było założone        

        if (shoes != null && _shoes != null && !isFromLoading)
        {
            _inventoryData.AddItem(_shoes, 1, _itemCurrentState);
        }
        _shoes = shoes;
        if (itemState != null)
        {
            _itemCurrentState = new List<ItemParameter>(itemState);
        }

        ModifyParameters();
        EquipShoes(_shoes);
    }
    
    
    private void EquipShoes(ShoesItemSO shoes)
    {
        RecalculateAllBonusesFromArmor();
        
        if (shoes == null)
        {
            return;
        }

        EquippedItemsManager.Instance.SetItemAsEquipped(shoes, ItemType.Shoes, 1, 4);
    }

    #endregion
    
    #region Modify parameters
    
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
    #endregion

    private void RecalculateAllBonusesFromArmor()
    {
        var stats = GameManager.Instance.Player.PlayerStats;

        stats.ResetEquipmentBonuses();
        stats.ResetEquipmentFlatBonuses();
        
        float physical = 0;

        if (_armor != null)
        {
            physical += _armor.PhysicalResistance;
            
            GameManager.Instance.Player.PlayerStats.AddEquipmentBonus(new StatBonus
            {
                Type = BonusType.MaxHp,
                Value = _armor.AdditionalHp
            });
            
            GameManager.Instance.Player.PlayerStats.AddEquipmentBonus(new StatBonus
            {
                Type = BonusType.MoveSpeed,
                //ZMNIEJSZAMY PRĘDKOŚĆ, DLATEGO JEST "-"
                Value = -_armor.MovementSpeedDisadvantage
            });
        }

        if (_helmet != null)
        {
            physical += _helmet.PhysicalResistance;
            GameManager.Instance.Player.PlayerStats.AddEquipmentBonus(new StatBonus
            {
                Type = BonusType.MaxHp,
                Value = _helmet.AdditionalHp
            });
        }

        if (_shoes != null)
        {
            GameManager.Instance.Player.PlayerStats.AddEquipmentBonus(new StatBonus
            {
                Type = BonusType.MoveSpeed,
                Value = _shoes.MovementSpeedBonus
            });
        }

        GameManager.Instance.Player.PlayerStats.RecalculateResistances(physical);
        GameManager.Instance.Player.GetComponent<PlayerHealth>().RefreshResistanceUI();
    }
    
}
