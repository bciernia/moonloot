using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Inventory : Singleton<Inventory>
{
    [Header("Config")]
    [SerializeField] private int _inventorySize;
    [SerializeField] private InventoryItem[] _inventoryItems;

    [Header("Testing")] public InventoryItem testItem;

    public int InventorySize => _inventorySize;

    public InventoryItem[] InventoryItems => _inventoryItems;

    protected override void Awake()
    {
        base.Awake();
        _inventoryItems = new InventoryItem[_inventorySize];
    }

    private void Start()
    {
        VerifyItemsForDraw();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            AddItem(testItem, 1);
        }
    }

    public void AddItem(InventoryItem item, int quantity)
    {
        if (item == null || quantity <= 0) return;

        var itemIndexes = CheckItemStock(item.Id);
        if (item.IsStackable && itemIndexes.Count > 0)
        {
            foreach (var index in itemIndexes)
            {
                var maxStack = item.MaxStack;
                if (_inventoryItems[index].Quantity < maxStack)
                {
                    _inventoryItems[index].Quantity += quantity;
                    if (_inventoryItems[index].Quantity > maxStack)
                    {
                        var dif = _inventoryItems[index].Quantity - maxStack;
                        _inventoryItems[index].Quantity = maxStack;
                        AddItem(item, dif);
                    }
                    
                    InventoryUI.Instance.DrawItem(_inventoryItems[index], index);
                    return;
                }
            }
        }

        var quantityToAdd = quantity > item.MaxStack ? item.MaxStack : quantity;
        AddItemFreeSlot(item, quantityToAdd);
        var remainingAmount = quantity - quantityToAdd;

        if (remainingAmount > 0)
        {
            AddItem(item, remainingAmount);
        }
    }

    public void UseItem(int index)
    {
        if (_inventoryItems[index] == null) return;

        if (_inventoryItems[index].UseItem())
        {
            DecreaseItemStack(index);
        }
    }

    public void RemoveItem(int index)
    {
        if (!_inventoryItems[index]) return;
        _inventoryItems[index].RemoveItem();
        _inventoryItems[index] = null;
        InventoryUI.Instance.DrawItem(null, index);
    }

    public void EquipItem(int index)
    {
        if (_inventoryItems[index] == null) return;
        if (_inventoryItems[index].ItemType != ItemTypes.Weapon) return;
        
        _inventoryItems[index].EquipItem();
    }

    private void AddItemFreeSlot(InventoryItem item, int quantity)
    {
        for (var i = 0; i < _inventorySize; i++)
        {
            if(_inventoryItems[i] != null) continue;

            _inventoryItems[i] = item.CopyItem();
            _inventoryItems[i].Quantity = quantity;
            InventoryUI.Instance.DrawItem(_inventoryItems[i], i);

            return;
        }
    }

    private void DecreaseItemStack(int index)
    {
        _inventoryItems[index].Quantity--;
        if (_inventoryItems[index].Quantity <= 0)
        {
            _inventoryItems[index] = null;
            InventoryUI.Instance.DrawItem(null, index);
        }
        else
        {
            InventoryUI.Instance.DrawItem(_inventoryItems[index], index);
        }
    }

    private List<int> CheckItemStock(string itemId)
    {
        var itemIndexes = new List<int>();

        for (var i = 0; i < _inventoryItems.Length; i++)
        {
            if(_inventoryItems[i] == null) continue;
            if (_inventoryItems[i].Id == itemId)
            {
                itemIndexes.Add(i);
            }
        }

        return itemIndexes;
    }

    private void VerifyItemsForDraw()
    {
        for (var i = 0; i < _inventorySize; i++)
        {
            if (_inventoryItems[i] == null)
            {
                InventoryUI.Instance.DrawItem(null, i);
            }
        }
    }
}
