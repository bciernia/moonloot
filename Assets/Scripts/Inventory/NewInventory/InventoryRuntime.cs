using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryRuntime 
{
    public List<InventoryItem> Items { get; private set; }
    public int Size { get; private set; }
    public int Lunar { get; set; }

    public event Action<Dictionary<int, InventoryItem>> OnInventoryUpdated;

    public InventoryRuntime(InventorySO template)
    {
        Size = template.Size;
        Lunar = template.Lunar;

        Items = new List<InventoryItem>();

        if (template.inventoryItems != null && template.inventoryItems.Count > 0)
        {
            foreach (var item in template.inventoryItems)
            {
                Items.Add(item);
            }
        }

        while (Items.Count < Size)
        {
            Items.Add(InventoryItem.GetEmptyItem());
        }
    }
    
    public void AddItem(InventoryItem item, int quantity)
    {
        AddItem(item.item, quantity);
    }
    
    public int AddItem(ItemSO item, int quantity, List<ItemParameter> itemState = null)
    {
        if (!item.IsStackable)
        {
            for (var i = 0; i < Items.Count;)
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
    
    private bool IsInventoryFull() => Items.Any(item => item.IsEmpty) == false;

    private int AddItemToFirstFreeSlot(ItemSO item, int quantity, List<ItemParameter> itemState = null)
    {
        var newItem = new InventoryItem()
        {
            item = item,
            quantity = quantity,
            itemState = new List<ItemParameter>(itemState ?? item.DefaultParametersList)
        };

        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i].IsEmpty)
            {
                Items[i] = newItem;
                return quantity;
            }
        }

        return 0;
    }
    
    private void InformAboutChange()
    {
        OnInventoryUpdated?.Invoke(GetCurrentInventoryState());
    }
    
    public Dictionary<int, InventoryItem> GetCurrentInventoryState()
    {
        Dictionary<int, InventoryItem> returnValue = new Dictionary<int, InventoryItem>();

        for (var i = 0; i < Items.Count; i++)
        {
            if (Items[i].IsEmpty)
                continue;
            returnValue[i] = Items[i];
        }

        return returnValue;
    }

    private int AddStackableItem(ItemSO item, int quantity)
    {
        for (var i = 0; i < Items.Count; i++)
        {
            if (Items[i].IsEmpty) 
                continue;

            if (Items[i].item.Id == item.Id)
            {
                var amountPossibleToTake = Items[i].item.MaxStackSize - Items[i].quantity;

                if (quantity > amountPossibleToTake)
                {
                    Items[i] = Items[i].ChangeQuantity(Items[i].item.MaxStackSize);
                    quantity -= amountPossibleToTake;
                }
                else
                {
                    Items[i] = Items[i].ChangeQuantity(Items[i].quantity + quantity);
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
    
    public void RemoveItem(int itemIndex, int amount)
    {
        if (Items.Count > itemIndex)
        {
            if (Items[itemIndex].IsEmpty) return;

            var reminder = Items[itemIndex].quantity - amount;
            if (reminder <= 0)
                Items[itemIndex] = InventoryItem.GetEmptyItem();
            else
                Items[itemIndex] = Items[itemIndex].ChangeQuantity(reminder);
            
            InformAboutChange();
        }
    }
    
    public InventoryItem GetItemAt(int itemIndex)
    {
        InventoryItem item = default;
        try
        {
            item = Items[itemIndex];
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        return item;
    }
    
    public void NotifyInventoryUpdated()
    {
        OnInventoryUpdated?.Invoke(GetCurrentInventoryState());
    }
    
    public void LoadData(List<InventoryItem> items, int lunar)
    {
        Items = new List<InventoryItem>(items);
        Lunar = lunar;

        NotifyInventoryUpdated();
    }
}