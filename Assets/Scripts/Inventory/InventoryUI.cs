using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : Singleton<InventoryUI>
{
    [Header("Config")]
    [SerializeField] private GameObject _inventoryPanel;
    [SerializeField] private InventorySlot _slotPrefab;
    [SerializeField] private Transform _container;

    [Header("Description panel")]
    [SerializeField] private GameObject _descriptionPanel;
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TextMeshProUGUI _itemName;
    [SerializeField] private TextMeshProUGUI _itemDescription;

    public InventorySlot CurrentSlot { get; set; }
    public GameObject DescriptionPanel { get; private set; }
    
    private readonly List<InventorySlot> _slotList = new List<InventorySlot>();

    protected override void Awake()
    {
        base.Awake();
        InitInventory();
        DescriptionPanel = _descriptionPanel;
    }

    private void InitInventory()
    {
        for (var i = 0; i < Inventory.Instance.InventorySize; i++)
        {
            var slot = Instantiate(_slotPrefab, _container);
            slot.Index = i;
            _slotList.Add(slot);
        }
    }

    public void UseItem()
    {
        if (CurrentSlot == null) return;
        Inventory.Instance.UseItem(CurrentSlot.Index);
    }

    public void RemoveItem()
    {
        if (!CurrentSlot) return;
        Inventory.Instance.RemoveItem(CurrentSlot.Index);
    }

    public void EquipItem()
    {
        if(CurrentSlot == null) return;
        Inventory.Instance.EquipItem(CurrentSlot.Index);
    }

    public void DrawItem(InventoryItem item, int index)
    {
        if (_slotList.Count <= 0) return;
        
        var slot = _slotList[index];

        if (item == null)
        {
            slot.ShowSlotInformation(false);
            return;
        }
        
        slot.ShowSlotInformation(true);
        slot.UpdateSlot(item);
    }

    private void SlotSelectedCallback(int slotIndex)
    {
        CurrentSlot = _slotList[slotIndex];
        ShowItemDescription(slotIndex);
    }

    private void ShowItemDescription(int index)
    {
        
        if (Inventory.Instance.InventoryItems[index] == null)
        {
            _descriptionPanel.SetActive(false);
            return;
        }
        
        _descriptionPanel.SetActive(true);
        _itemIcon.sprite = Inventory.Instance.InventoryItems[index].Icon;
        _itemName.text = Inventory.Instance.InventoryItems[index].Name;
        _itemDescription.text = Inventory.Instance.InventoryItems[index].Description;
    }
    
    
    private void OnEnable()
    {
        InventorySlot.OnSlotSelectedEvent += SlotSelectedCallback;

        if (_slotList.Count == 0)
        {
            InitInventory();
        }

        RefreshInventory();
    }

    private void OnDisable()
    {
        InventorySlot.OnSlotSelectedEvent -= SlotSelectedCallback;
        _descriptionPanel.SetActive(false);
        CurrentSlot = null;

        foreach (var slot in _slotList)
        {
            if (slot != null)
            {
                slot.ShowSlotInformation(false);
            }
        }
    }
    
    private void RefreshInventory()
    {
        var inventoryItems = Inventory.Instance.InventoryItems;
    
        for (int i = 0; i < _slotList.Count; i++)
        {
            if (i < inventoryItems.Length)
            {
                DrawItem(inventoryItems[i], i);
            }
            else
            {
                DrawItem(null, i);
            }
        }
    }
}