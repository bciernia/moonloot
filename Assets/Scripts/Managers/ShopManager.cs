using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ShopManager : Singleton<ShopManager>
{
    [SerializeField] public UIInventoryPage inventoryPage;
    [SerializeField] private GameObject InventoryPanel;
    [SerializeField] private GameObject ShopPanel;
    [SerializeField] private UIInventoryItem itemPrefab;
    [SerializeField] private RectTransform shopContainer;
    [SerializeField] private TextMeshProUGUI PanelNameTMP;
    [SerializeField] private TextMeshProUGUI MoneyAmountTMP;

    public List<UIInventoryItem> listOfSellerItems = new List<UIInventoryItem>();
    
    private InventorySO playerInventory;

    public InventorySO SellerInventory { get; private set; }
    private bool IsShop { get; set; }
    
    public void InitializeShop(InventorySO sellerInventory, string panelName, bool isShop = false)
    {
        SellerInventory = sellerInventory;
        InventoryPanel.SetActive(true);
        ShopPanel.SetActive(true);
        IsShop = isShop;
        SetPanelName(panelName);
        SetMoneyAmountVisibility(isShop);        
        InitializeSellerEquipment(sellerInventory);
        InventoryController.Instance.PrepareSellerInventoryData(sellerInventory);
    }

    private void SetPanelName(string panelName) => PanelNameTMP.text = panelName;
    
    private void SetMoneyAmountVisibility(bool isShop) => MoneyAmountTMP.gameObject.SetActive(isShop);

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
        if (IsShop)
        {
            var playerGoldAmount = InventoryController.Instance.inventoryData.Gold;

            if (itemToBuy.item.BuyPrice > playerGoldAmount) return;

            InventoryController.Instance.ChangeGoldAmount(-itemToBuy.item.BuyPrice);
            SellerInventory.Gold += itemToBuy.item.BuyPrice;
        }
        
        InventoryController.Instance.inventoryData.AddItem(itemToBuy, 1);
        SellerInventory.RemoveItem(itemIndex, 1);
    }

    public void SellItem(InventoryItem itemToSell, int itemIndex)
    {
        if (IsShop)
        {
            var sellerGoldAmount = Instance.SellerInventory.Gold;
        
            if (itemToSell.item.SellPrice > sellerGoldAmount) return;

            InventoryController.Instance.ChangeGoldAmount(itemToSell.item.SellPrice);
            SellerInventory.Gold -= itemToSell.item.SellPrice;
        }
        
        InventoryController.Instance.inventoryData.RemoveItem(itemIndex, 1);
        SellerInventory.AddItem(itemToSell, 1);
    }
}
