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

    private void Awake()
    {
        if (_itemIcon == null || _itemQuantityTMP == null)
        {
            Debug.LogWarning($"InventorySlot: UI elements not assigned in the Inspector for slot index {Index}. Assign them to avoid null reference exceptions.");
        }
    }

    public void UpdateSlot(InventoryItem item)
    {
        if (_itemIcon == null || _itemQuantityTMP == null) return;

        if (item != null)
        {
            _itemIcon.sprite = item.Icon;
            _itemQuantityTMP.text = item.Quantity.ToString();
            ShowSlotInformation(true);
        }
        else
        {
            ShowSlotInformation(false);
        }
    }

    public void ShowSlotInformation(bool value)
    {
        if (_itemIcon != null && _quantityContainer != null)
        {
            _itemIcon.gameObject.SetActive(value);
            _quantityContainer.gameObject.SetActive(value);
        }
    }
}