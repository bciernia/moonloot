using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EquippedItemsManager : Singleton<EquippedItemsManager>
{
    public List<UIInventoryItem> EquippedItemsSlots = new List<UIInventoryItem>();
    public List<InventoryItem> EquippedItems = new List<InventoryItem>();

    public InventoryItem defaultWeapon;
    
    [Header("Equipped")]
    [SerializeField] private UIInventoryItem WeaponSlot;
    [SerializeField] private UIInventoryItem ArmorSlot;
    [SerializeField] private UIInventoryItem OutfitSlot;
    [SerializeField] private UIInventoryItem HelmetSlot;
    [SerializeField] private UIInventoryItem ShoesSlot;

    [Header("Usable")]
    [SerializeField] private UIInventoryItem QuickSlot1;
    [SerializeField] private UIInventoryItem QuickSlot2;
    
    [SerializeField] private EquippedItemsManagerSO EquippedItemsManagerSo;
    
    protected override void Awake()
    {
        base.Awake();
        if (Instance != this)
            return;

        while (EquippedItems.Count < 7)
            EquippedItems.Add(InventoryItem.GetEmptyItem());

        EquippedItemsSlots.Add(WeaponSlot);
        EquippedItemsSlots.Add(ArmorSlot);
        EquippedItemsSlots.Add(OutfitSlot);
        EquippedItemsSlots.Add(HelmetSlot);
        EquippedItemsSlots.Add(ShoesSlot);
        EquippedItemsSlots.Add(QuickSlot1);
        EquippedItemsSlots.Add(QuickSlot2);
        
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
        InitializeSlot(EquippedItems[5], QuickSlot1);
        InitializeSlot(EquippedItems[6], QuickSlot2);
    }

    public void RefreshEquippedSlot(UIInventoryItem slot)
    {
        if (slot == null || EquippedItems == null)
            return;

        if (slot == WeaponSlot && EquippedItems.Count > 0)
            InitializeSlot(EquippedItems[0].item == null ? defaultWeapon : EquippedItems[0], WeaponSlot);
        else if (slot == ArmorSlot && EquippedItems.Count > 1)
            InitializeSlot(EquippedItems[1], ArmorSlot);
        else if (slot == OutfitSlot && EquippedItems.Count > 2)
            InitializeSlot(EquippedItems[2], OutfitSlot);
        else if (slot == HelmetSlot && EquippedItems.Count > 3)
            InitializeSlot(EquippedItems[3], HelmetSlot);
        else if (slot == ShoesSlot && EquippedItems.Count > 4)
            InitializeSlot(EquippedItems[4], ShoesSlot);
        else if (slot == QuickSlot1 && EquippedItems.Count > 5)
            InitializeSlot(EquippedItems[5], QuickSlot1);
        else if (slot == QuickSlot2 && EquippedItems.Count > 6)
            InitializeSlot(EquippedItems[6], QuickSlot2);
    }

    private void InitializeSlot(InventoryItem equippedItem, UIInventoryItem slot)
    {
        if (slot == null)
            return;

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
    
    public void SetItemAsEquipped(ItemSO item, ItemType itemType, int quantity = 1, int slotIndex = -1, List<ItemParameter> itemState = null)
    {
        SetEquippedItemByType(item, itemType, quantity, slotIndex, itemState);
    }
    
    private void SetEquippedItemByType(ItemSO item, ItemType itemType, int quantity = 1, int slotIndex = -1, List<ItemParameter> itemState = null)
    {
        switch (itemType)
        {
            case ItemType.Weapon:
                SetItem(WeaponSlot, item, slotIndex, 1, itemState);
                break;
            case ItemType.Armor:
                SetItem(ArmorSlot, item, slotIndex);
                break;
            case ItemType.Outfit:
                SetItem(OutfitSlot, item, slotIndex);
                break;
            case ItemType.Necklace:
                break;
            case ItemType.Helmet:
                SetItem(HelmetSlot, item, slotIndex);
                break;
            case ItemType.Shoes:
                SetItem(ShoesSlot, item, slotIndex);
                break;
            case ItemType.Ammunition:
                break;
            case ItemType.Ring:
                break;
            case ItemType.Bracelet:
                break;
            case ItemType.Edible:
                SetQuickSlot(item, slotIndex, quantity);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(item.ItemType), item.ItemType, null);
        }
    }
    
    public void SetQuickSlot(ItemSO item, int slotIndex, int quantity = 1)
    {
        if (slotIndex == 5)
            SetItem(QuickSlot1, item, 5, quantity);
        else if (slotIndex == 6)
            SetItem(QuickSlot2, item, 6, quantity);
    }

    private void SetItem(UIInventoryItem slotToSet, ItemSO item, int slotIndex, int quantity = 1, List<ItemParameter> itemState = null)
    {
        SetItemInSlot(slotToSet, item, quantity);
        SetItemInList(item, slotIndex, quantity, itemState);
    } 
    
    private void SetItemInSlot(UIInventoryItem slotToSet, ItemSO item, int quantity = 1)
    {
        if (slotToSet == null)
            return;

        if (item == null)
        {
            slotToSet.ResetData();
        }
        else
        {
            slotToSet.SetData(item.Image, quantity);
        }
    }
    
    private void SetItemInList(ItemSO item, int slotIndex, int quantity = 1, List<ItemParameter> itemState = null)
    {
        EquippedItems[slotIndex] = new InventoryItem()
        {
            item = item,
            quantity = quantity,
            itemState = itemState != null
                ? new List<ItemParameter>(itemState)
                : new List<ItemParameter>()
        };
    }
}
