using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu]
public class InventorySO : ScriptableObject
{
    [SerializeField] private List<InventoryItemm> inventoryItems;
    
    [field: SerializeField]
    public int Size { get; private set; } = 10;

    public event Action<Dictionary<int, InventoryItemm>> OnInventoryUpdated; 
    
    public void Initialize()
    {
        inventoryItems = new List<InventoryItemm>();
        for (var i = 0; i < Size; i++)
        {
            inventoryItems.Add(InventoryItemm.GetEmptyItem());
        }
    }

    public int AddItem(ItemSO item, int quantity)
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
                    quantity -= AddItemToFirstFreeSlot(item, 1);
                }
                
                InformAboutChange();
                return quantity;
            }
        }

        quantity = AddStackableItem(item, quantity);
        InformAboutChange();

        return quantity;
    }

    private int AddItemToFirstFreeSlot(ItemSO item, int quantity)
    {
        var newItem = new InventoryItemm()
        {
            item = item,
            quantity = quantity
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

    public void AddItem(InventoryItemm item)
    {
        AddItem(item.item, item.quantity);
    }

    public Dictionary<int, InventoryItemm> GetCurrentInventoryState()
    {
        Dictionary<int, InventoryItemm> returnValue = new Dictionary<int, InventoryItemm>();

        for (var i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].IsEmpty)
                continue;
            returnValue[i] = inventoryItems[i];
        }

        return returnValue;
    }

    public InventoryItemm GetItemAt(int itemIndex) => inventoryItems[itemIndex];

    public void SwapItems(int itemIndex1, int itemIndex2)
    {
        (inventoryItems[itemIndex1], inventoryItems[itemIndex2]) = (inventoryItems[itemIndex2], inventoryItems[itemIndex1]);
        InformAboutChange();
    }

    private void InformAboutChange()
    {
        OnInventoryUpdated?.Invoke(GetCurrentInventoryState());
    }
}

[Serializable]
public struct InventoryItemm
{
    public int quantity;
    public ItemSO item;
    public bool IsEmpty => item == null;

    public InventoryItemm ChangeQuantity(int newQuantity)
    {
        return new InventoryItemm()
        {
            item = item,
            quantity = newQuantity
        };
    }

    public static InventoryItemm GetEmptyItem()
    {
        return new InventoryItemm()
        {
            item = null,
            quantity = 0
        };
    }
}
