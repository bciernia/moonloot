using System;
using System.Collections.Generic;
using UnityEngine;

public class EquippedItemsManager : Singleton<EquippedItemsManager>
{
    public List<UIInventoryItem> EquippedItemsSlots = new List<UIInventoryItem>();
    public List<InventoryItem> EquippedItems = new List<InventoryItem>();
    
    [SerializeField] private UIInventoryItem WeaponSlot;
    [SerializeField] private UIInventoryItem ArmorSlot;
    [SerializeField] private EquippedItemsManagerSO EquippedItemsManagerSo;
    
    protected override void Awake()
    {
        base.Awake();

        EquippedItemsSlots.Add(WeaponSlot);
        EquippedItemsSlots.Add(ArmorSlot);
        
        if (EquippedItems.Count == 0)
            EquippedItems.Add(InventoryItem.GetEmptyItem());
        InitializeEquippedSlots();
    }

    private void InitializeEquippedSlots()
    {
        InitializeSlot(EquippedItems[0], WeaponSlot);
        InitializeSlot(EquippedItems[1], ArmorSlot);
    }

    private void InitializeSlot(InventoryItem equippedItem, UIInventoryItem slot)
    {
        var itemToSet = equippedItem.item;
        
        if (itemToSet != null && itemToSet != InventoryItem.GetEmptyItem().item)
        {
            slot.SetData(itemToSet.Image, equippedItem.quantity);
        }
        else
        {
            slot.ResetData();
        }
    }
    
    public void SetItemAsEquipped(ItemSO item, ItemType itemType)
    {
        SetEquippedItemByType(item, itemType);
    }
    
    private void SetEquippedItemByType(ItemSO item, ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Weapon:
                SetItem(WeaponSlot, item, 0);
                break;
            case ItemType.Armor:
                SetItem(ArmorSlot, item, 1);
                break;
            case ItemType.Ammunition:
                break;
            case ItemType.Ring:
                break;
            case ItemType.Bracelet:
                break;
            case ItemType.Necklace:
                break;
            case ItemType.Shoes:
                break;
            case ItemType.Helmet:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(item.ItemType), item.ItemType, null);
        }
    }

    private void SetItem(UIInventoryItem slotToSet, ItemSO item, int slotIndex)
    {
        SetItemInSlot(slotToSet, item);
        SetItemInList(item, slotIndex);
    } 
    
    private void SetItemInSlot(UIInventoryItem slotToSet, ItemSO item)
    {
        if (item == null)
        {
            slotToSet.ResetData();
        }
        else
        {
            slotToSet.SetData(item.Image, 1);
        }
    }
    
    private void SetItemInList(ItemSO item, int slotIndex)
    {
        EquippedItems[slotIndex] = new InventoryItem()
        {
            item = item,
            quantity = 1,
            itemState = new List<ItemParameter>(),
        };
    }
}
