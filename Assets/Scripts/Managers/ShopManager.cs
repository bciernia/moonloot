using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class ShopManager : Singleton<ShopManager>
{
    [SerializeField] public UIInventoryPage inventoryPage;
    [SerializeField] private GameObject InventoryPanel;
    [SerializeField] private GameObject ShopPanel;
    [SerializeField] private UIInventoryItem itemPrefab;
    [SerializeField] private RectTransform shopContainer;

    public List<UIInventoryItem> listOfSellerItems = new List<UIInventoryItem>();
    
    private InventorySO playerInventory;

    public InventorySO SellerInventory { get; private set; }
    
    public void InitializeShop(InventorySO sellerInventory)
    {
        SellerInventory = sellerInventory;
        InventoryPanel.SetActive(true);
        ShopPanel.SetActive(true);
        InitializeSellerEquipment(sellerInventory);
        InventoryController.Instance.PrepareSellerInventoryData(sellerInventory);
    }

    public void ResetSellerInventory()
    {
        SellerInventory = null;
        listOfSellerItems.Clear();
        inventoryPage.ResetSelection();
    }

    private void InitializeSellerEquipment(InventorySO inventorySize)
    {
        listOfSellerItems.Clear();

        foreach (Transform child in shopContainer.transform)
        {
            Destroy(child.gameObject);
        }
        
        for (var i = 0; i < inventorySize.Size; i++)
        {
            var uiItem = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
            uiItem.transform.SetParent(shopContainer);
            uiItem.transform.localScale = new Vector3(1,1,1);
            uiItem.tag = "ShopItem";
            listOfSellerItems.Add(uiItem);
            uiItem.OnLeftMouseBtnClick += inventoryPage.HandleItemSelection;
            uiItem.OnRightMouseBtnClick += inventoryPage.HandleShowItemActions;
            uiItem.OnItemBeginDrag += inventoryPage.HandleBeginDrag;
            uiItem.OnItemDroppedOn += inventoryPage.HandleSwap;
            uiItem.OnItemEndDrag += inventoryPage.HandleEndDrag; 
            
            inventoryPage.OnDescriptionRequested += InventoryController.Instance.HandleDescriptionRequest;
            inventoryPage.OnStartDragging += InventoryController.Instance.HandleDragging;
            inventoryPage.OnItemActionRequested += InventoryController.Instance.HandleItemActionRequest;
        }
    }
    
    public void ResetAllData()
    {
        foreach (var item in listOfSellerItems)
        {
            item.ResetData();
            item.Deselect();
        }
    }
    
    public void UpdateData(int itemIndex, Sprite itemImage, int itemQuantity)
    {
        if (listOfSellerItems.Count > itemIndex)
        {
            listOfSellerItems[itemIndex].SetData(itemImage, itemQuantity);
        }
    }

    public void BuyItem(InventoryItem itemToBuy, int itemIndex)
    {
        var playerGoldAmount = InventoryController.Instance.inventoryData.Gold;

        if (itemToBuy.item.BuyPrice > playerGoldAmount) return;

        InventoryController.Instance.inventoryData.AddItem(itemToBuy, 1);
        InventoryController.Instance.ChangeGoldAmount(-itemToBuy.item.BuyPrice);
        SellerInventory.Gold += itemToBuy.item.BuyPrice;
        SellerInventory.RemoveItem(itemIndex, 1);
    }

    public void SellItem(InventoryItem itemToSell, int itemIndex)
    {
        var sellerGoldAmount = Instance.SellerInventory.Gold;
        
        if (itemToSell.item.SellPrice > sellerGoldAmount) return;

        InventoryController.Instance.inventoryData.RemoveItem(itemIndex, 1);
        InventoryController.Instance.ChangeGoldAmount(itemToSell.item.SellPrice);
        SellerInventory.Gold -= itemToSell.item.SellPrice;
        SellerInventory.AddItem(itemToSell, 1);
    }
}
