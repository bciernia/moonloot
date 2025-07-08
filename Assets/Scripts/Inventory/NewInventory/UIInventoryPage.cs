using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIInventoryPage : MonoBehaviour
{
    [SerializeField] private UIInventoryItem itemPrefab;
    [SerializeField] private RectTransform contentPanel;
    [SerializeField] private UIInventoryDescription itemDescription;
    [SerializeField] private MouseFollower mouseFollower;
    [SerializeField] private TextMeshProUGUI goldAmountTMP;
    private List<UIInventoryItem> listOfUiItems = new List<UIInventoryItem>();

    private int currentlyDraggedItemIndex = -1;

    public event Action<int> OnDescriptionRequested, OnItemActionRequested, OnStartDragging;
    public event Action<int, int> OnSwapItems;

    [SerializeField] private UIItemActionPanel actionPanel;
    
    private void Awake()
    {
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
            uiItem.OnLeftMouseBtnClick +=  HandleItemSelection;
            uiItem.OnRightMouseBtnClick += HandleShowItemActions;
            uiItem.OnItemBeginDrag += HandleBeginDrag;
            uiItem.OnItemDroppedOn += HandleSwap;
            uiItem.OnItemEndDrag += HandleEndDrag;
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
        var index = listOfUiItems.IndexOf(inventoryItemUi);
        if (index == -1)
        {
            return;
        }            

        OnItemActionRequested?.Invoke(index);
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

    public void AddAction(string actionName, Action performAction)
    {
        actionPanel.AddButton(actionName, performAction);
    }

    public void ShowItemAction(int itemIndex)
    {
        actionPanel.Toggle(true);
        actionPanel.transform.position = listOfUiItems[itemIndex].transform.position;
    }

    private void DeselectAllItems()
    {
        foreach (var uiInventoryItem in listOfUiItems)
        {
            uiInventoryItem.Deselect();
        }
        
        actionPanel.Toggle(false);
    }
    
    public void Hide()
    {
        actionPanel.Toggle(false);
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

    public void UpdateGoldAmount(int goldAmount)
    {
        goldAmountTMP.text = $"Gold: {goldAmount}";
    }
}
