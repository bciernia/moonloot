using System.Collections.Generic;
using Inventory.NewInventory.Model;
using TMPro;
using UnityEngine;

public class ShopManager : Singleton<ShopManager>
{
    [SerializeField] public UIInventoryPage inventoryPage;
    [SerializeField] private GameObject InventoryPanel;
    [SerializeField] public GameObject ShopPanel;
    [SerializeField] private UIInventoryItem itemPrefab;
    [SerializeField] private RectTransform shopContainer;
    [SerializeField] private TextMeshProUGUI PanelNameTMP;
    [SerializeField] private TextMeshProUGUI MoneyAmountTMP;
    [SerializeField] private GameObject TabPanel;

    public List<UIInventoryItem> listOfSellerItems = new List<UIInventoryItem>();
    
    private InventorySO playerInventory;

    public InventorySO SellerInventory { get; private set; }
    private InventoryType InventoryType { get; set; }
    
    public void InitializeShop(InventorySO sellerInventory, string panelName, InventoryType inventoryType)
    {
        SellerInventory = sellerInventory;
        InventoryPanel.SetActive(true);
        ShopPanel.SetActive(true);
        InventoryType = inventoryType;
        SetPanelName(panelName);
        SetMoneyAmountVisibility();        
        InitializeSellerEquipment(SellerInventory);
        InventoryController.Instance.PrepareSellerInventoryData(sellerInventory);
        TabMenuManager.Instance.OpenMenu();
        TabMenuManager.Instance.SwitchToTab(0);
        TabPanel.SetActive(true); 
        PauseManager.Instance.RequestPause();
    }

    private void SetPanelName(string panelName) => PanelNameTMP.text = panelName;
    
    private void SetMoneyAmountVisibility()
    {
        if(InventoryType == InventoryType.Shop) MoneyAmountTMP.gameObject.SetActive(true);
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
        
        inventoryPage.OnDescriptionRequested -= InventoryController.Instance.HandleDescriptionRequest;
        inventoryPage.OnStartDragging -= InventoryController.Instance.HandleDragging;
        inventoryPage.OnItemActionRequested -= InventoryController.Instance.HandleItemActionRequest;

        inventoryPage.OnDescriptionRequested += InventoryController.Instance.HandleDescriptionRequest;
        inventoryPage.OnStartDragging += InventoryController.Instance.HandleDragging;
        inventoryPage.OnItemActionRequested += InventoryController.Instance.HandleItemActionRequest;
        
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
        if (InventoryType == InventoryType.Shop)
        {
            var playerGoldAmount = InventoryController.Instance.inventoryData.Lunar;

            if (itemToBuy.item.BuyPrice > playerGoldAmount) return;

            InventoryController.Instance.ChangeGoldAmount(-itemToBuy.item.BuyPrice);
            SellerInventory.Lunar += itemToBuy.item.BuyPrice;
        }

        if (itemToBuy.item.ItemType == ItemType.Gold)
        {
            InventoryController.Instance.ChangeGoldAmount(((GoldItemSO)itemToBuy.item).Amount);
        }
        else
        {
            InventoryController.Instance.inventoryData.AddItem(itemToBuy, 1);
        }

        SellerInventory.RemoveItem(itemIndex, 1);
    }

    public void SellItem(InventoryItem itemToSell, int itemIndex)
    {
        if (InventoryType == InventoryType.Shop)
        {
            var sellerGoldAmount = Instance.SellerInventory.Lunar;
        
            if (itemToSell.item.SellPrice > sellerGoldAmount) return;

            InventoryController.Instance.ChangeGoldAmount(itemToSell.item.SellPrice);
            SellerInventory.Lunar -= itemToSell.item.SellPrice;
        }
        
        InventoryController.Instance.inventoryData.RemoveItem(itemIndex, 1);
        SellerInventory.AddItem(itemToSell, 1);
    }
}
