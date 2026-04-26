using System;
using System.Collections.Generic;
using System.Text;
using Inventory.NewInventory.Model;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryController : Singleton<InventoryController>, ISaveable
{
    [SerializeField] private UIInventoryPage inventoryUI;
    [SerializeField] public InventorySO inventoryData;
    public List<InventoryItem> initialItems = new List<InventoryItem>();
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private EquippedItemsManager equippedItemsManager;
    [SerializeField] private EquippedItemsManagerSO equippedItemsManagerSo;
    
    private void Start()
    {
        PrepareUI();
        PrepareInventoryData();
        inventoryUI.UpdateGoldAmount(inventoryData.Lunar);
    }
    
    private void ChangeGoldAmount(GoldItemSO gold)
    {
        var goldAmount = gold.Amount;
        
        if (goldAmount * -1 > inventoryData.Lunar)
        {
            Debug.Log("you don't have enough money");
            return;
        }

        inventoryData.Lunar += goldAmount;

        inventoryUI.UpdateGoldAmount(inventoryData.Lunar);
    }

    public bool ChangeGoldAmount(int goldAmount)
    {
        if (goldAmount * -1 > inventoryData.Lunar)
        {
            Debug.Log("you don't have enough money");
            return false;
        }
        
        inventoryData.Lunar += goldAmount;

        inventoryUI.UpdateGoldAmount(inventoryData.Lunar);

        return true;
    }
    
    public void AddItem(InventoryItem item)
    {
        if (item.item is GoldItemSO _)
        {
            ChangeGoldAmount(item.quantity);
            return;
        }
        
        inventoryData.AddItem(item, item.quantity);
    }

    public void PrepareSellerInventoryData(InventoryRuntime sellerInventory)
    {
        UpdateSellerInventoryUI(sellerInventory.GetCurrentInventoryState());
        sellerInventory.OnInventoryUpdated += UpdateSellerInventoryUI;
    }   
    
    private void PrepareInventoryData()
    {
        inventoryData.Initialize();
        inventoryData.OnInventoryUpdated += UpdateInventoryUI;
        foreach (var item in initialItems)
        {
            if(item.IsEmpty) continue;
            
            inventoryData.AddItem(item, item.quantity);
        }
    }

    private void UpdateSellerInventoryUI(Dictionary<int, InventoryItem> inventoryState)
    {
        ShopManager.Instance.ResetAllData();
        foreach (var item in inventoryState)
        {
            ShopManager.Instance.UpdateData(item.Key, item.Value.item.Image, item.Value.quantity);
        }
        ShopManager.Instance.inventoryPage.UpdateSellerGoldAmount(ShopManager.Instance.SellerInventory.Lunar);
    }
    
    private void UpdateInventoryUI(Dictionary<int, InventoryItem> inventoryState)
    {
        inventoryUI.ResetAllData();
        foreach (var item in inventoryState)
        {
            inventoryUI.UpdateData(item.Key, item.Value.item.Image, item.Value.quantity);
        }
        inventoryUI.UpdateGoldAmount(inventoryData.Lunar);
    }

    private void PrepareUI()
    {
        inventoryUI.InitializeInventoryUI(inventoryData.Size);
        inventoryUI.OnDescriptionRequested += HandleDescriptionRequest;
        inventoryUI.OnSwapItems += HandleSwapItems;
        inventoryUI.OnStartDragging += HandleDragging;
        inventoryUI.OnItemActionRequested += HandleItemActionRequest;
    }

    public void HandleItemActionRequest(int itemIndex)
    {
        var inventoryItem = inventoryData.GetItemAt(itemIndex);
        
        if (inventoryItem.IsEmpty) return;

        var itemAction = inventoryItem.item as IItemAction;
        if (itemAction != null)
        {
            inventoryUI.ShowItemAction(itemIndex);
            inventoryUI.AddAction(itemAction.ActionName, () => PerformAction(itemIndex));
        }

        var destroyableItem = inventoryItem.item as IDestroyableItem;
        if (destroyableItem != null)
        {
            inventoryUI.AddAction("Drop one", () => DropItem(itemIndex, 1));
            inventoryUI.AddAction("Drop all", () => DropItem(itemIndex, inventoryItem.quantity));
        }
    }

    private void DropItem(int itemIndex, int quantity)
    {
        var itemToDrop = inventoryData.GetItemAt(itemIndex).item.ItemToDrop;
        inventoryData.RemoveItem(itemIndex, quantity);
        inventoryUI.ResetSelection();
        if(audioSource) audioSource.PlayOneShot(audioClip);
        var playerTransform = GameObject.FindWithTag("Player").GetComponent<Transform>();
        if (itemToDrop && playerTransform)
        {
            for (var i = 0; i < quantity; i++)
            {
                Instantiate(itemToDrop, playerTransform.position, Quaternion.identity);
            }
        }
    }

    public void PerformAction(int itemIndex)
    {
        var inventoryItem = inventoryData.GetItemAt(itemIndex);
        var isActionPerformed = false;
        
        if (inventoryItem.IsEmpty) return;

        var itemAction = inventoryItem.item as IItemAction;
        if (itemAction != null)
        {
            isActionPerformed = itemAction.PerformAction(gameObject, inventoryItem.itemState);
        }

        if (!isActionPerformed) return;
        
        var destroyableItem = inventoryItem.item as IDestroyableItem;
        if (destroyableItem != null)
        {
            inventoryData.RemoveItem(itemIndex, 1);
            if(audioSource) audioSource.PlayOneShot(itemAction.actionSfx);
            if(inventoryData.GetItemAt(itemIndex).IsEmpty) inventoryUI.ResetSelection();
        }
    }

    public void HandleDragging(int itemIndex, bool isPlayerItem, string inventoryItemUiName = "")
    {
        InventoryItem item;

        if (itemIndex == -1)
        {
            switch (inventoryItemUiName)
            {
                case "WeaponUI":
                    item = equippedItemsManager.EquippedItems[0];
                    break;
                case "ArmorUI":
                    item = equippedItemsManager.EquippedItems[1];
                    break;
                case "OutfitUI":
                    item = equippedItemsManager.EquippedItems[2];
                    break;
                case "HelmetUI":
                    item = equippedItemsManager.EquippedItems[3];
                    break;
                case "ShoesUI":
                    item = equippedItemsManager.EquippedItems[4];
                    break;
                default:
                    throw new ArgumentException("Nie znaleziono przedmiotu do przeciągnięcia");
            }
            
        }
        else if (isPlayerItem)
        {
            item = inventoryData.GetItemAt(itemIndex);
        }
        else
        {
            item = ShopManager.Instance.SellerInventory.GetItemAt(itemIndex);
        }
        
        if (item.IsEmpty) return;

        inventoryUI.CreateDraggedItem(item.item.Image, item.quantity);
    }

    public void HandleSwapItems(int itemIndex_1, int itemIndex_2, string inventoryUiName)
    {
        if (itemIndex_1 == -1 || itemIndex_2 == -1)
        {
            var inventoryItem = inventoryData.GetItemAt(itemIndex_2);
            if (inventoryItem.IsEmpty)
            {
                InventoryItem item;
                
                switch (inventoryUiName)
                {
                    case "WeaponUI":
                        item = equippedItemsManager.EquippedItems[0];
                
                        //TODO zmienić na typ, żeby nie porównywać po nazwie
                        if (item.item.Name == "Fists") return;
                
                        equippedItemsManager.EquippedItems[0] = InventoryItem.GetEmptyItem();
                        equippedItemsManager.SetItemAsEquipped(equippedItemsManager.EquippedItems[0].item, ItemType.Weapon);
                
                        var weaponItemSo = (WeaponItemSO)item.item;
                        weaponItemSo.UnequipWeapon(gameObject);
                        AddItem(item);
                        break;
                    
                    case "ArmorUI":
                        item = equippedItemsManager.EquippedItems[1];
                
                        equippedItemsManager.EquippedItems[1] = InventoryItem.GetEmptyItem();
                        equippedItemsManager.SetItemAsEquipped(equippedItemsManager.EquippedItems[1].item, ItemType.Armor);
                
                        var armorItemSo = (ArmorItemSO)item.item;
                        armorItemSo.Unequip(gameObject);
                        AddItem(item);
                        break;
                    
                    case "OutfitUI":
                        item = equippedItemsManager.EquippedItems[2];
                
                        equippedItemsManager.EquippedItems[2] = InventoryItem.GetEmptyItem();
                        equippedItemsManager.SetItemAsEquipped(equippedItemsManager.EquippedItems[2].item, ItemType.Outfit);
                
                        var outfitItemSo = (OutfitItemSO)item.item;
                        outfitItemSo.UnequipOutfit(gameObject);
                        AddItem(item);
                        
                        break;
                    case "HelmetUI":
                        item = equippedItemsManager.EquippedItems[3];
                        
                        equippedItemsManager.EquippedItems[3] = InventoryItem.GetEmptyItem();
                        equippedItemsManager.SetItemAsEquipped(equippedItemsManager.EquippedItems[3].item, ItemType.Helmet);
                
                        var helmetItemSo = (HelmetItemSO)item.item;
                        helmetItemSo.Unequip(gameObject);
                        AddItem(item);
                        
                        break;
                    case "ShoesUI":
                        item = equippedItemsManager.EquippedItems[4];
                        
                        equippedItemsManager.EquippedItems[4] = InventoryItem.GetEmptyItem();
                        equippedItemsManager.SetItemAsEquipped(equippedItemsManager.EquippedItems[4].item, ItemType.Shoes);
                
                        var shoesItemSo = (ShoesItemSO)item.item;
                        shoesItemSo.Unequip(gameObject);
                        AddItem(item);
                        
                        break;
                    default:
                        throw new ArgumentException("Nie znaleziono przedmiotu do zamiany");
                }
            }
            else
            {
                PerformAction(itemIndex_2);
            }            
        }
        
        inventoryData.SwapItems(itemIndex_1, itemIndex_2);
    }
    
    public void HandleDescriptionRequest(int itemIndex, bool isPlayerItem = true, string inventoryItemUiName = "")
    {
        var isInPlayerEquipment = true;
        InventoryItem inventoryItem;
        if (itemIndex == -1)
        {
            switch (inventoryItemUiName)
            {
                case "WeaponUI":
                    inventoryItem = equippedItemsManager.EquippedItems[0];
                    break;
                case "ArmorUI":
                    inventoryItem = equippedItemsManager.EquippedItems[1];
                    break;
                case "OutfitUI":
                    inventoryItem = equippedItemsManager.EquippedItems[2];
                    break;
                case "HelmetUI":
                    inventoryItem = equippedItemsManager.EquippedItems[3];
                    break;
                case "ShoesUI":
                    inventoryItem = equippedItemsManager.EquippedItems[4];
                    break;
                default:
                    throw new ArgumentException("Nie znaleziono przedmiotu do przeciągnięcia");
            }
        }
        else if(isPlayerItem)
        {
            inventoryItem = inventoryData.GetItemAt(itemIndex);
        }
        else
        {
            inventoryItem = ShopManager.Instance.SellerInventory.GetItemAt(itemIndex);
            isInPlayerEquipment = false;
        }
        
        if (inventoryItem.IsEmpty)
        {
            inventoryUI.ResetSelection();
            return;
        }
        var item = inventoryItem.item;
        var description = PrepareDescription(inventoryItem, isInPlayerEquipment);
        inventoryUI.UpdateDescription(itemIndex, isPlayerItem, item.Image, item.Name, description, inventoryItemUiName);
    }

    private string PrepareDescription(InventoryItem inventoryItem, bool isInPlayerEquipment)
    {
        var sb = new StringBuilder();
        GetDescriptionByType(sb, inventoryItem);
        sb.Append(isInPlayerEquipment
            ? $"Sell price: {inventoryItem.item.SellPrice}"
            : $"Buy price: {inventoryItem.item.BuyPrice}");
        for (var i = 0; i < inventoryItem.itemState.Count; i++)
        {
            sb.AppendLine();
            sb.Append($"{inventoryItem.itemState[i].itemParameter.ParameterName}: " +
                      $"{inventoryItem.itemState[i].value} / " +
                      
                      $"{inventoryItem.item.DefaultParametersList[i].value}");
        }
        
        sb.AppendLine();
        sb.AppendLine();
        sb.Append(inventoryItem.item.Description);

        return sb.ToString();
    }

    private void GetDescriptionByType(StringBuilder sb, InventoryItem inventoryItem)
    {
        switch (inventoryItem.item.ItemType)
        {
            case ItemType.Weapon:
                sb.Append(((WeaponItemSO)inventoryItem.item).GetStatsDescription());
                break;
            case ItemType.Edible:
                sb.Append(((EdibleItemSO)inventoryItem.item).GetStatsDescription());
                break;
            case ItemType.Armor:
                sb.Append(((ArmorItemSO)inventoryItem.item).GetStatsDescription());
                break;
            case ItemType.Letter:
                sb.Append(((LetterItemSO)inventoryItem.item).GetLetterContent());
                break;     
            case ItemType.Helmet:
                sb.Append(((HelmetItemSO)inventoryItem.item).GetStatsDescription());
                break;
            case ItemType.Shoes:
                sb.Append(((ShoesItemSO)inventoryItem.item).GetStatsDescription());
                break;
            default:
                sb.Append(inventoryItem.item.GetStatsDescription());
                break;
        }
    }

    public bool UseItemById(int itemId) => inventoryData.FindItemById(itemId);

    public bool HasUserQuestItem(string itemName, int quantity) =>  inventoryData.FindItemByName(itemName, quantity);

    public void TryRemoveQuestItems(string itemName, int quantity) => inventoryData.RemoveItemByName(itemName, quantity);
    public void Save()
    {
        ES3.Save("playerInventory_items", inventoryData.inventoryItems);
        ES3.Save("playerInventory_gold", inventoryData.Lunar);
        ES3.Save("playerInventory_equippedItems", equippedItemsManager.EquippedItems);
    }

    public void Load()
    {
        if (ES3.KeyExists("playerInventory_items"))
        {
            var items = ES3.Load<List<InventoryItem>>("playerInventory_items");
            inventoryData.inventoryItems = items;
        }

        if (ES3.KeyExists("playerInventory_gold"))
        {
            var gold = ES3.Load<int>("playerInventory_gold");
            inventoryData.Lunar = gold;
        }

        inventoryData.NotifyInventoryUpdated();

        if (ES3.KeyExists("playerInventory_equippedItems"))
        {
           var equipped = ES3.Load<List<InventoryItem>>("playerInventory_equippedItems");
           equippedItemsManager.EquippedItems = equipped;
           equippedItemsManager.InitializeEquippedSlots();
           WeaponManager.Instance.SetWeapon((WeaponItemSO)equippedItemsManager.EquippedItems[0].item, equippedItemsManager.EquippedItems[0].itemState, true);
           ArmorManager.Instance.SetArmor((ArmorItemSO)equippedItemsManager.EquippedItems[1].item, equippedItemsManager.EquippedItems[1].itemState, true);
           OutfitManager.Instance.SetOutfit((OutfitItemSO)equippedItemsManager.EquippedItems[2].item, equippedItemsManager.EquippedItems[2].itemState, true);
           ArmorManager.Instance.SetHelmet((HelmetItemSO)equippedItemsManager.EquippedItems[3].item, equippedItemsManager.EquippedItems[3].itemState, true);
           ArmorManager.Instance.SetShoes((ShoesItemSO)equippedItemsManager.EquippedItems[4].item, equippedItemsManager.EquippedItems[4].itemState, true);
        }
    }
    
    private void OnDestroy()
    {
        if (inventoryData != null)
            inventoryData.OnInventoryUpdated -= UpdateInventoryUI;

        if (inventoryUI != null)
        {
            inventoryUI.OnDescriptionRequested -= HandleDescriptionRequest;
            inventoryUI.OnSwapItems -= HandleSwapItems;
            inventoryUI.OnStartDragging -= HandleDragging;
            inventoryUI.OnItemActionRequested -= HandleItemActionRequest;
        }

        if (ShopManager.Instance != null && ShopManager.Instance.SellerInventory != null)
            ShopManager.Instance.SellerInventory.OnInventoryUpdated -= UpdateSellerInventoryUI;
    }
}
