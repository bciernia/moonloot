using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquippedItemsManager : Singleton<EquippedItemsManager>
{
    public List<UIInventoryItem> EquippedItemsSlots = new List<UIInventoryItem>();
    public List<InventoryItem> EquippedItems = new List<InventoryItem>();
    
    [SerializeField] private UIInventoryItem WeaponSlot;

    [SerializeField] private EquippedItemsManagerSO EquippedItemsManagerSo;
    
    protected override void Awake()
    {
        base.Awake();

        EquippedItemsSlots.Add(WeaponSlot);
        
        if (EquippedItems.Count == 0)
            EquippedItems.Add(InventoryItem.GetEmptyItem());
    }
    public void SetItemAsEquipped(ItemSO item)
    {
        SetEquippedItemByType(item, ItemType.Weapon);
    }
    
    private void SetEquippedItemByType(ItemSO item, ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Weapon:
                SetItem(WeaponSlot, item);
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
            case ItemType.Armor:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null);
        }
    }

    private void SetItem(UIInventoryItem slotToSet, ItemSO item)
    {
        SetItemInSlot(slotToSet, item);
        SetItemInList(item);
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

    private void SetItemInList(ItemSO item)
    {
        EquippedItems[0] = new InventoryItem()
        {
            item = item,
            quantity = 1,
            itemState = new List<ItemParameter>(),
        };
    }
}
