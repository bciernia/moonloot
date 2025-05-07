using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public static event Action<int> OnSlotSelectedEvent;
    
    [Header("Config")]
    [SerializeField] private Image _itemIcon;
    [SerializeField] private Image _quantityContainer;
    [SerializeField] private TextMeshProUGUI _itemQuantityTMP;
    
    public int Index { get; set; }

    public void ClickSlot()
    {
        OnSlotSelectedEvent?.Invoke(Index);
    }

    public void UpdateSlot(InventoryItem item)
    {
        _itemIcon.sprite = item.Icon;
        _itemQuantityTMP.text = item.Quantity.ToString();
    }

    public void ShowSlotInformation(bool value)
    {
        _itemIcon.gameObject.SetActive(value);
        _quantityContainer.gameObject.SetActive(value);
    }
}