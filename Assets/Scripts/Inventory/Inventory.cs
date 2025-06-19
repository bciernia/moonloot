using System;
using System.Collections.Generic;
using System.Linq;
using BayatGames.SaveGameFree;
using Unity.VisualScripting;
using UnityEngine;

public class Inventory : Singleton<Inventory>
{
    [Header("Config")] 
    [SerializeField] private GameContent gameContent;
    [SerializeField] private int _inventorySize;
    [SerializeField] private InventoryItem[] _inventoryItems;

    [Header("Testing")] public InventoryItem testItem;

    public int InventorySize => _inventorySize;

    public InventoryItem[] InventoryItems => _inventoryItems;

    private readonly string INVENTORY_KEY_DATA = "PLAYER_INVENTORY";
    
    protected override void Awake()
    {
        base.Awake();
        _inventoryItems = new InventoryItem[_inventorySize];
    }

    private void Start()
    {
        VerifyItemsForDraw();
        LoadInventory();
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
                    SaveInventory();
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

        SaveInventory();
    }

    public void UseItem(int index)
    {
        if (_inventoryItems[index] == null) return;

        if (_inventoryItems[index].UseItem())
        {
            DecreaseItemStack(index);
        }
        
        SaveInventory();
    }

    public void RemoveItem(int index)
    {
        if (!_inventoryItems[index]) return;
        _inventoryItems[index].RemoveItem();
        _inventoryItems[index] = null;
        InventoryUI.Instance.DrawItem(null, index);

        SaveInventory();
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

    private InventoryItem ItemExistsInGameContent(string itemId)
    {
        for (var i = 0; i < _inventorySize; i++)
        {
            if (gameContent.GameItems[i].Id == itemId)
            {
                return gameContent.GameItems[i];
            }
        }

        return null;
    }

    private void LoadInventory()
    {
        if (SaveGame.Exists(INVENTORY_KEY_DATA))
        {
            var loadData = SaveGame.Load<InventoryData>(INVENTORY_KEY_DATA);

            for (var i = 0; i < _inventorySize; i++)
            {
                if (loadData.ItemContent[i] != null)
                {
                    var itemFromContent = ItemExistsInGameContent(loadData.ItemContent[i]);

                    if (itemFromContent != null)
                    {
                        _inventoryItems[i] = itemFromContent.CopyItem();
                        _inventoryItems[i].Quantity = loadData.ItemQuantity[i];
                        InventoryUI.Instance.DrawItem(_inventoryItems[i], i);
                    }
                }
                else
                {
                    _inventoryItems[i] = null;
                }
            }
        }
    }

    private void SaveInventory()
    {
        var saveData = new InventoryData();

        saveData.ItemContent = new string[_inventorySize];
        saveData.ItemQuantity = new int[_inventorySize];

        for (var i = 0; i < _inventorySize; i++)
        {
            if (_inventoryItems[i] == null)
            {
                saveData.ItemContent[i] = null;
                saveData.ItemQuantity[i] = 0;
            }
            else
            {
                saveData.ItemContent[i] = _inventoryItems[i].Id;
                saveData.ItemQuantity[i] = _inventoryItems[i].Quantity;
            }
        }

        SaveGame.Save(INVENTORY_KEY_DATA, saveData);
    }
}
