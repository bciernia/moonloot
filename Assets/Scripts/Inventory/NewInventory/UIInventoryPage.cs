using System;
using System.Collections.Generic;
using UnityEngine;

public class UIInventoryPage : MonoBehaviour
{
    [SerializeField] private UIInventoryItem itemPrefab;
    [SerializeField] private RectTransform contentPanel;
    [SerializeField] private UIInventoryDescription itemDescription;
    [SerializeField] private MouseFollower mouseFollower;
    private List<UIInventoryItem> listOfUiItems = new List<UIInventoryItem>();

    private int currentlyDraggedItemIndex = -1;

    public event Action<int> OnDescriptionRequested, OnItemActionRequested, OnStartDragging;
    public event Action<int, int> OnSwapItems;
    private void Awake()
    {
        Hide();
        mouseFollower.Toggle(false);
        itemDescription.ResetDescription();
    }

    public void InitializeInventoryUI(int inventorySize)
    {
        for (var i = 0; i < inventorySize; i++)
        {
            var uiItem = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
            uiItem.transform.SetParent(contentPanel);
            uiItem.transform.localScale = new Vector3(1,1,1);
            listOfUiItems.Add(uiItem);
            uiItem.OnItemClicked += HandleItemSelection;
            uiItem.OnItemBeginDrag += HandleBeginDrag;
            uiItem.OnItemDroppedOn += HandleSwap;
            uiItem.OnItemEndDrag += HandleEndDrag;
            uiItem.OnRightMouseBtnClick += HandleShowItemActions;
        }
    }

    public void UpdateData(int itemIndex, Sprite itemImage, int itemQuantity)
    {
        if (listOfUiItems.Count > itemIndex)
        {
            listOfUiItems[itemIndex].SetData(itemImage, itemQuantity);
        }
    }

    private void HandleShowItemActions(UIInventoryItem inventoryItemUi)
    {
        
    }

    private void HandleEndDrag(UIInventoryItem inventoryItemUi)
    {
        ResetDraggedItem();
    }

    private void HandleSwap(UIInventoryItem inventoryItemUi)
    {
        var index = listOfUiItems.IndexOf(inventoryItemUi);
        if (index == -1)
        {
            return;
        }            
        
        OnSwapItems?.Invoke(currentlyDraggedItemIndex, index);
        HandleItemSelection(inventoryItemUi);
    }

    private void ResetDraggedItem()
    {
        mouseFollower.Toggle(false);
        currentlyDraggedItemIndex = -1;
    }

    private void HandleBeginDrag(UIInventoryItem inventoryItemUi)
    {
        var index = listOfUiItems.IndexOf(inventoryItemUi);
        if (index == -1)
            return;
        currentlyDraggedItemIndex = index;
        HandleItemSelection(inventoryItemUi);
        OnStartDragging?.Invoke(index);
    }

    public void CreateDraggedItem(Sprite sprite, int quantity)
    {
        mouseFollower.Toggle(true);
        mouseFollower.SetData(sprite, quantity);
    }

    private void HandleItemSelection(UIInventoryItem inventoryItemUi)
    {
        var index = listOfUiItems.IndexOf(inventoryItemUi);
        if (index == -1)
            return;
        OnDescriptionRequested?.Invoke(index);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        ResetSelection();
    }

    public void ResetSelection()
    {
        itemDescription.ResetDescription();
        DeselectAllItems();
    }

    private void DeselectAllItems()
    {
        foreach (var uiInventoryItem in listOfUiItems)
        {
            uiInventoryItem.Deselect();
        }
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
        ResetDraggedItem();
    }

    public void UpdateDescription(int itemIndex, Sprite itemImage, string itemName, string description)
    {
        itemDescription.SetDescription(itemImage, itemName, description);
        DeselectAllItems();
        listOfUiItems[itemIndex].Select();
    }

    public void ResetAllData()
    {
        foreach (var item in listOfUiItems)
        {
            item.ResetData();
            item.Deselect();
        }
    }
}
