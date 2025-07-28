using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WeaponManager: Singleton<WeaponManager>
{
    [Header("Config")]
    [SerializeField] private Image _weaponIcon;
    [SerializeField] private TextMeshProUGUI _weaponManaTMP;

    [SerializeField] private WeaponItemSO _weapon;
    [SerializeField] private InventorySO _inventoryData;
    [SerializeField] private List<ItemParameter> _parametersToModify, _itemCurrentState;

    public void SetWeapon(WeaponItemSO weaponItem, List<ItemParameter> itemState)
    {
        if (weaponItem != null)
        {
            _inventoryData.AddItem(_weapon, 1, _itemCurrentState);
        }

        _weapon = weaponItem;
        if (itemState != null)
        {
            _itemCurrentState = new List<ItemParameter>(itemState);
        }

        ModifyParameters();
        EquipWeapon(_weapon);
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
    
    private void EquipWeapon(WeaponItemSO weapon)
    {
        _weaponIcon.sprite = weapon.Image;
        _weaponIcon.gameObject.SetActive(true);

        if (weapon.RequiredMana != 0)
        {
            _weaponManaTMP.text = weapon.RequiredMana.ToString(CultureInfo.InvariantCulture);
            _weaponManaTMP.gameObject.SetActive(true);
        }
        else
        {
            _weaponManaTMP.gameObject.SetActive(false);
        }

        GameManager.Instance.Player.PlayerAttack.EquipWeapon(weapon);
        EquippedItemsManager.Instance.SetItemAsEquipped(weapon);
    }
}
