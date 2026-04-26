using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EquippedItemsManager : Singleton<EquippedItemsManager>
{
    public List<UIInventoryItem> EquippedItemsSlots = new List<UIInventoryItem>();
    public List<InventoryItem> EquippedItems = new List<InventoryItem>();

    public InventoryItem defaultWeapon;
    
    [SerializeField] private UIInventoryItem WeaponSlot;
    [SerializeField] private UIInventoryItem ArmorSlot;
    [SerializeField] private UIInventoryItem OutfitSlot;
    [SerializeField] private UIInventoryItem HelmetSlot;
    [SerializeField] private UIInventoryItem ShoesSlot;
    [SerializeField] private EquippedItemsManagerSO EquippedItemsManagerSo;
    
    protected override void Awake()
    {
        base.Awake();
        EquippedItemsSlots.Add(WeaponSlot);
        EquippedItemsSlots.Add(ArmorSlot);
        EquippedItemsSlots.Add(OutfitSlot);
        EquippedItemsSlots.Add(HelmetSlot);
        EquippedItemsSlots.Add(ShoesSlot);
        
        if (EquippedItems.Count == 0)
            EquippedItems.Add(InventoryItem.GetEmptyItem());
        InitializeEquippedSlots();
    }

    public void InitializeEquippedSlots()
    {
        //Dla pustej broni ustawiany default którym są pięści
        InitializeSlot(EquippedItems[0].item == null ? defaultWeapon : EquippedItems[0], WeaponSlot);
        InitializeSlot(EquippedItems[1], ArmorSlot);
        InitializeSlot(EquippedItems[2], OutfitSlot);
        InitializeSlot(EquippedItems[3], HelmetSlot);
        InitializeSlot(EquippedItems[4], ShoesSlot);
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
            case ItemType.Outfit:
                SetItem(OutfitSlot, item, 2);
                break;
            case ItemType.Necklace:
                break;
            case ItemType.Helmet:
                SetItem(HelmetSlot, item, 3);
                break;
            case ItemType.Shoes:
                SetItem(ShoesSlot, item, 4);
                break;
            case ItemType.Ammunition:
                break;
            case ItemType.Ring:
                break;
            case ItemType.Bracelet:
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
