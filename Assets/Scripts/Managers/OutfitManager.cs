using System.Collections.Generic;
using UnityEngine;

public class OutfitManager : Singleton<OutfitManager>
{
    [Header("Config")]
    [SerializeField] private InventorySO _inventoryData;
    [SerializeField] private List<ItemParameter> _parametersToModify, _itemCurrentState;
    [SerializeField] private RuntimeAnimatorController _standardOutfit;
    [SerializeField] private OutfitItemSO _outfit;
    
    public void SetOutfit(OutfitItemSO outfitItem, List<ItemParameter> itemState)
    {
        /*
        //Tworzy duplikat przy przeładowaniu gry, jesli coś było założone        
        if (outfitItem != null && _outfit != null)
        {
            _inventoryData.AddItem(outfitItem, 1, _itemCurrentState);
        }
        */
        
        _outfit = outfitItem;
        
        if (itemState != null)
        {
            _itemCurrentState = new List<ItemParameter>(itemState);
        }

        ModifyParameters();
        ChangeOutfit(_outfit);
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
    
    private void ChangeOutfit(OutfitItemSO outfit)
    {
        if (outfit == null)
        {
            FindAnyObjectByType<Player>().GetComponent<Animator>().runtimeAnimatorController = _standardOutfit;

            return;
        }
        FindAnyObjectByType<Player>().GetComponent<Animator>().runtimeAnimatorController = outfit.RuntimeAnimatorController;
        EquippedItemsManager.Instance.SetItemAsEquipped(outfit, ItemType.Outfit);
    }
}