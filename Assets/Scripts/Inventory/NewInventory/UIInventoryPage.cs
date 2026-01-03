using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

internal enum ItemDraggedFrom {
    Player,
    Shop
}

public class UIInventoryPage : MonoBehaviour
{
    [SerializeField] private UIInventoryItem itemPrefab;
    [SerializeField] private RectTransform contentPanel;
    [SerializeField] private RectTransform shopPanel;
    [SerializeField] private UIInventoryDescription itemDescription;
    [SerializeField] private MouseFollower mouseFollower;
    [SerializeField] private TextMeshProUGUI goldAmountTMP;
    [SerializeField] private TextMeshProUGUI sellerGoldAmountTMP;
    private List<UIInventoryItem> listOfUiItems = new List<UIInventoryItem>();

    [SerializeField] private UIInventoryItem WeaponSlot;

    private int currentlyDraggedItemIndex = -1;

    private ItemDraggedFrom itemDraggedFrom = ItemDraggedFrom.Player;

    public event Action<int> OnItemActionRequested;
    public event Action<int, bool, string> OnDescriptionRequested, OnStartDragging;
    public event Action<int, int, string> OnSwapItems;

    [SerializeField] private UIItemActionPanel actionPanel;

    [SerializeField] private EquippedItemsManager equippedItemsManager;
    
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
            uiItem.tag = "PlayerItem";
            listOfUiItems.Add(uiItem);
            
            uiItem.OnLeftMouseBtnClick += HandleItemSelection;
            uiItem.OnRightMouseBtnClick += HandleShowItemActions;
            uiItem.OnItemBeginDrag += HandleBeginDrag;
            uiItem.OnItemDroppedOn += HandleSwap;
            uiItem.OnItemEndDrag += HandleEndDrag;
        }

        foreach (var uiItem in EquippedItemsManager.Instance.EquippedItemsSlots)
        {
            uiItem.OnLeftMouseBtnClick += HandleItemSelection;
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

    public void HandleShowItemActions(UIInventoryItem inventoryItemUi)
    {
        var index = listOfUiItems.IndexOf(inventoryItemUi);
        if (index == -1 && !inventoryItemUi.CompareTag("EquippedItem"))
        {
            return;
        }            

        OnItemActionRequested?.Invoke(index);
    }

    public void HandleEndDrag(UIInventoryItem inventoryItemUi)
    {
        ResetDraggedItem();
    }

    public void HandleSwap(UIInventoryItem targetInventoryItemUi, UIInventoryItem draggedInventoryItemUi)
    {
        var index = listOfUiItems.IndexOf(targetInventoryItemUi);
        var indexOfSellerItem = ShopManager.Instance.listOfSellerItems.IndexOf(targetInventoryItemUi);
        var draggedInventoryItemUiName = draggedInventoryItemUi.gameObject.name;

        if (itemDraggedFrom == ItemDraggedFrom.Shop && (targetInventoryItemUi.CompareTag("EquippedItem") || targetInventoryItemUi.CompareTag("PlayerItem")))
        {
            var itemToBuy = ShopManager.Instance.SellerInventory.GetItemAt(currentlyDraggedItemIndex);
            ShopManager.Instance.BuyItem(itemToBuy, currentlyDraggedItemIndex);
            return;
        }
        
        if (itemDraggedFrom == ItemDraggedFrom.Player && targetInventoryItemUi.CompareTag("ShopItem"))
        {
            var itemToSell = InventoryController.Instance.inventoryData.GetItemAt(currentlyDraggedItemIndex);
            ShopManager.Instance.SellItem(itemToSell, currentlyDraggedItemIndex);
            return;
        }
        
        if (index == -1 && indexOfSellerItem == -1 && targetInventoryItemUi.CompareTag("EquippedItem"))
        {
            InventoryController.Instance.PerformAction(currentlyDraggedItemIndex);
            return;
        }   
        
        if (index != -1)
        {
            OnSwapItems?.Invoke(currentlyDraggedItemIndex, index, draggedInventoryItemUiName);
        }
        
        HandleItemSelection(targetInventoryItemUi);
    }

    private void ResetDraggedItem()
    {
        mouseFollower.Toggle(false);
        currentlyDraggedItemIndex = -1;
    }

    public void HandleBeginDrag(UIInventoryItem inventoryItemUi)
    {
        var index = listOfUiItems.IndexOf(inventoryItemUi);
        var indexOfSellerItem = ShopManager.Instance.listOfSellerItems.IndexOf(inventoryItemUi);
        if (index == -1 && indexOfSellerItem == -1 && !inventoryItemUi.CompareTag("EquippedItem"))
            return;
        var inventoryItemUiName = inventoryItemUi.gameObject.name;

        if (index != -1)
        {
            currentlyDraggedItemIndex = index;
            itemDraggedFrom = ItemDraggedFrom.Player;
        } 
        else if (indexOfSellerItem != -1)
        {
            currentlyDraggedItemIndex = indexOfSellerItem;
            itemDraggedFrom = ItemDraggedFrom.Shop;
        }
        
        HandleItemSelection(inventoryItemUi);
        if (index != -1)
        {
            OnStartDragging?.Invoke(index, true, inventoryItemUiName);
        }
        else
        {
            OnStartDragging?.Invoke(indexOfSellerItem, false, inventoryItemUiName);
        }
    }

    public void CreateDraggedItem(Sprite sprite, int quantity)
    {
        mouseFollower.Toggle(true);
        mouseFollower.SetData(sprite, quantity);
    }

    public void HandleItemSelection(UIInventoryItem inventoryItemUi)
    {
        var index = listOfUiItems.IndexOf(inventoryItemUi);
        var indexOfSellerItem = ShopManager.Instance.listOfSellerItems.IndexOf(inventoryItemUi);
        if (index == -1 && indexOfSellerItem == -1 && !inventoryItemUi.CompareTag("EquippedItem"))
            return;
        var inventoryItemUiName = inventoryItemUi.gameObject.name;

        if (indexOfSellerItem != -1)
        {
            OnDescriptionRequested?.Invoke(indexOfSellerItem, false, inventoryItemUiName);
        }
        else
        {
            OnDescriptionRequested?.Invoke(index, true, inventoryItemUiName);
        } 
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
        
        foreach (var uiSellerInventoryItem in ShopManager.Instance.listOfSellerItems)
        {
            uiSellerInventoryItem.Deselect();
        }

        foreach (var uiEquippedItemsSlots in equippedItemsManager.EquippedItemsSlots)
        {
            uiEquippedItemsSlots.Deselect();
        }
        
        actionPanel.Toggle(false);
    }
    
    public void Hide()
    {
        actionPanel.Toggle(false);
        gameObject.SetActive(false);
        ResetDraggedItem();
    }

    public void UpdateDescription(int itemIndex, bool isPlayerItem, Sprite itemImage, string itemName, string description, string inventoryUiName = "")
    {
        itemDescription.SetDescription(itemImage, itemName, description);
        DeselectAllItems();

        if (itemIndex == -1)
        {
            SelectUiSlotByEquipmentUiName(inventoryUiName);
        }
        else if (isPlayerItem)
        {
            listOfUiItems[itemIndex].Select();
        }
        else
        {
            ShopManager.Instance.listOfSellerItems[itemIndex].Select();
        }
    }

    private void SelectUiSlotByEquipmentUiName(string inventoryUiEqName)
    {
        switch (inventoryUiEqName)
        {
            case "WeaponUI":
                equippedItemsManager.EquippedItemsSlots[0].Select();
                break;
            case "ArmorUI":
                equippedItemsManager.EquippedItemsSlots[1].Select();
                break;
            case "OutfitUI":
                equippedItemsManager.EquippedItemsSlots[2].Select();
                break;                
            default:
                throw new ArgumentException("Nie znaleziono slota w ui do zaznaczenia");
        }
    }

    public void ResetAllData()
    {
        foreach (var item in listOfUiItems)
        {
            item.ResetData();
            item.Deselect();
        }
        
        foreach (var item in ShopManager.Instance.listOfSellerItems)
        {
            item.Deselect();
        }
        
        foreach (var uiEquippedItemsSlots in equippedItemsManager.EquippedItemsSlots)
        {
            uiEquippedItemsSlots.Deselect();
        }
    }

    public void UpdateGoldAmount(int goldAmount)
    {
        goldAmountTMP.text = $"Lunar: {goldAmount}";
    }
    
    public void UpdateSellerGoldAmount(int goldAmount)
    {
        sellerGoldAmountTMP.text = $"Lunar: {goldAmount}";
    }
}
