using System;
using System.Collections.Generic;
using System.Linq;
using Inventory.NewInventory.Model;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu]
public class InventorySO : ScriptableObject
{
    [SerializeField] private List<InventoryItem> inventoryItems;
    
    [field: SerializeField]
    public int Size { get; private set; } = 10;

    public int Gold { get; set; } = 50;

    public event Action<Dictionary<int, InventoryItem>> OnInventoryUpdated; 
    
    public void Initialize()
    {
        inventoryItems = new List<InventoryItem>();
        for (var i = 0; i < Size; i++)
        {
            inventoryItems.Add(InventoryItem.GetEmptyItem());
        }
    }


    public int AddItem(ItemSO item, int quantity, List<ItemParameter> itemState = null)
    {
        if (!item.IsStackable)
        {
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (IsInventoryFull())
                {
                    return quantity;
                }

                while (quantity > 0 && !IsInventoryFull())
                {
                    quantity -= AddItemToFirstFreeSlot(item, 1, itemState);
                }
                
                InformAboutChange();
                return quantity;
            }
        }

        quantity = AddStackableItem(item, quantity);
        InformAboutChange();

        return quantity;
    }

    private int AddItemToFirstFreeSlot(ItemSO item, int quantity, List<ItemParameter> itemState = null)
    {
        var newItem = new InventoryItem()
        {
            item = item,
            quantity = quantity,
            itemState = new List<ItemParameter>(itemState ?? item.DefaultParametersList)
        };

        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].IsEmpty)
            {
                inventoryItems[i] = newItem;
                return quantity;
            }
        }

        return 0;
    }

    private bool IsInventoryFull() => inventoryItems.Where(item => item.IsEmpty).Any() == false;

    private int AddStackableItem(ItemSO item, int quantity)
    {
        for (var i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].IsEmpty) 
                continue;

            if (inventoryItems[i].item.Id == item.Id)
            {
                var amountPossibleToTake = inventoryItems[i].item.MaxStackSize - inventoryItems[i].quantity;

                if (quantity > amountPossibleToTake)
                {
                    inventoryItems[i] = inventoryItems[i].ChangeQuantity(inventoryItems[i].item.MaxStackSize);
                    quantity -= amountPossibleToTake;
                }
                else
                {
                    inventoryItems[i] = inventoryItems[i].ChangeQuantity(inventoryItems[i].quantity + quantity);
                    InformAboutChange();
                    return 0;
                }
            }
        }

        while (quantity > 0 && !IsInventoryFull())
        {
            var newQuantity = Mathf.Clamp(quantity, 0, item.MaxStackSize);
            quantity -= newQuantity;
            AddItemToFirstFreeSlot(item, newQuantity);
        }

        return quantity;
    }

    public void AddItem(InventoryItem item)
    {
        AddItem(item.item, item.quantity);
    }

    public Dictionary<int, InventoryItem> GetCurrentInventoryState()
    {
        Dictionary<int, InventoryItem> returnValue = new Dictionary<int, InventoryItem>();

        for (var i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].IsEmpty)
                continue;
            returnValue[i] = inventoryItems[i];
        }

        return returnValue;
    }

    public InventoryItem GetItemAt(int itemIndex) => inventoryItems[itemIndex];

    public void SwapItems(int itemIndex1, int itemIndex2)
    {
        (inventoryItems[itemIndex1], inventoryItems[itemIndex2]) = (inventoryItems[itemIndex2], inventoryItems[itemIndex1]);
        InformAboutChange();
    }

    private void InformAboutChange()
    {
        OnInventoryUpdated?.Invoke(GetCurrentInventoryState());
    }

    public void RemoveItem(int itemIndex, int amount)
    {
        if (inventoryItems.Count > itemIndex)
        {
            if (inventoryItems[itemIndex].IsEmpty) return;

            var reminder = inventoryItems[itemIndex].quantity - amount;
            if (reminder <= 0)
                inventoryItems[itemIndex] = InventoryItem.GetEmptyItem();
            else
                inventoryItems[itemIndex] = inventoryItems[itemIndex].ChangeQuantity(reminder);
            
            InformAboutChange();
        }
    }

    public bool FindItemById(int itemId)
    {
        for (var i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].IsEmpty)
                continue;

            if (inventoryItems[i].item.Id == itemId)
            {
                RemoveItem(i, 1);
                return true;
            }
        }

        return false;
    }
}

[Serializable]
public struct InventoryItem
{
    public int quantity;
    public ItemSO item;
    public List<ItemParameter> itemState;
    public bool IsEmpty => item == null;

    public InventoryItem ChangeQuantity(int newQuantity)
    {
        return new InventoryItem()
        {
            item = item,
            quantity = newQuantity,
            itemState = new List<ItemParameter>(itemState)
        };
    }

    public static InventoryItem GetEmptyItem()
    {
        return new InventoryItem()
        {
            item = null,
            quantity = 0,
            itemState = new List<ItemParameter>()
        };
    }
}
