using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuickItemManager : Singleton<QuickItemManager>
{    
    [SerializeField] private InventorySO _inventoryData;
    [SerializeField] private List<ItemParameter> _parametersToModify, _itemCurrentState;
    [SerializeField] private EdibleItemSO _edibleItemSo1;
    [SerializeField] private EdibleItemSO _edibleItemSo2;
    
    [SerializeField] private Image _quickItem1Image;
    [SerializeField] private Image _quickItem2Image;
    
    [SerializeField] private GameObject _quickItem1GameObject;
    [SerializeField] private GameObject _quickItem2GameObject;
    [SerializeField] private TextMeshProUGUI _quickItem1ItemQuantityText;
    [SerializeField] private TextMeshProUGUI _quickItem2ItemQuantityText;
    
    // private void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Alpha1))
    //         UseSlot(5);
    //
    //     if (Input.GetKeyDown(KeyCode.Alpha2))
    //         UseSlot(6);
    // }

    private void UseSlot(int index)
    {
        var item = EquippedItemsManager.Instance.EquippedItems[index];
        var equipped = EquippedItemsManager.Instance;
        if (item.IsEmpty) return;

        var action = item.item as IItemAction;
        if (action != null)
        {
            action.PerformAction(gameObject, item);

            if (item.quantity > 0) return;
            EquippedItemsManager.Instance.EquippedItems[index] = InventoryItem.GetEmptyItem();
        }
        
        EquippedItemsManager.Instance.InitializeEquippedSlots();
        SkillsManager.Instance.RefreshSlotUI();

    }
    
    // public void SetQuickItem(EdibleItemSO item, List<ItemParameter> itemState, int itemQuantity, int quickItemSlotIndex, bool isFromLoading = false)
    // {
    //     // if (item != null && _edibleItemSo1 != null && !isFromLoading)
    //     // {
    //     //     _inventoryData.AddItem(_edibleItemSo1, itemQuantity, _itemCurrentState);
    //     // }
    //     
    //     _edibleItemSo1 = item;
    //     if (itemState != null)
    //     {
    //         _itemCurrentState = new List<ItemParameter>(itemState);
    //     }
    //
    //     EquipQuickItem1(_edibleItemSo1, itemQuantity);
    // }
    
    public void SetQuickItem(EdibleItemSO item, List<ItemParameter> itemState, int quantity, int slotIndex, bool isFromLoading = false)
    {
        var equipped = EquippedItemsManager.Instance;
    
        if (!isFromLoading)
        {
            var current = equipped.EquippedItems[slotIndex];
    
            if (!current.IsEmpty)
            {
                InventoryController.Instance.AddItem(current);
            }
        }
    
        equipped.EquippedItems[slotIndex] = new InventoryItem()
        {
            item = item,
            quantity = quantity,
            itemState = itemState ?? new List<ItemParameter>()
        };
    
        equipped.InitializeEquippedSlots();
        RefreshUI();
    }
        
    public void RefreshUI()
    {
        UpdateSlotUI(5, _quickItem1Image, _quickItem1GameObject, _quickItem1ItemQuantityText);
        UpdateSlotUI(6, _quickItem2Image, _quickItem2GameObject, _quickItem2ItemQuantityText);
    }

    private void UpdateSlotUI(int index, Image image, GameObject quantityGO, TextMeshProUGUI quantityText)
    {
        var item = EquippedItemsManager.Instance.EquippedItems[index];

        if (item.IsEmpty)
        {
            image.sprite = null;
            image.gameObject.SetActive(false);
            quantityGO.SetActive(false);
            return;
        }

        image.gameObject.SetActive(true);
        image.sprite = item.item.Image;

        if (item.quantity > 1)
        {
            quantityGO.SetActive(true);
            quantityText.text = item.quantity.ToString();
        }
        else
        {
            quantityGO.SetActive(false);
        }
    }

}
