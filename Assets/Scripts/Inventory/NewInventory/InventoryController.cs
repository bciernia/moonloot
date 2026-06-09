using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inventory.NewInventory.Model;
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
    
    public Action OnInventoryChanged;
    
    private void Start()
    {
        PrepareUI();
        PrepareInventoryData();
        inventoryData.Lunar = 50;
        inventoryUI.UpdateGoldAmount(inventoryData.Lunar);
        Debug.Log("TEST");
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
        
        OnInventoryChanged?.Invoke();
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

        OnInventoryChanged?.Invoke();
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

    public void DropItem(int itemIndex, int quantity)
    {
        var itemToDrop = inventoryData.GetItemAt(itemIndex).item.ItemToDrop;
        inventoryData.RemoveItem(itemIndex, quantity);
        OnInventoryChanged?.Invoke();
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

    public void PerformAction(int itemIndex, string slotName = "")
    {
        var inventoryItem = inventoryData.GetItemAt(itemIndex);
        var isActionPerformed = false;
        
        if (inventoryItem.IsEmpty) return;

        var itemAction = inventoryItem.item as IItemAction;
        if (itemAction != null)
        {
            isActionPerformed = itemAction.PerformAction(gameObject, inventoryItem, false, slotName);
        }

        if (!isActionPerformed) return;
        
        var destroyableItem = inventoryItem.item as IDestroyableItem;
        if (destroyableItem != null)
        {
            inventoryData.RemoveItem(itemIndex, inventoryItem.quantity);
            if(audioSource) audioSource.PlayOneShot(itemAction.actionSfx);
            if(inventoryData.GetItemAt(itemIndex).IsEmpty) inventoryUI.ResetSelection();
            OnInventoryChanged?.Invoke();
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
                case "QuickSlot1":
                    item = equippedItemsManager.EquippedItems[5];
                    break;
                case "QuickSlot2":
                    item = equippedItemsManager.EquippedItems[6];
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

    private void HandleSwapItems(int itemIndex_1, int itemIndex_2, string inventoryUiName)
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
                        equippedItemsManager.SetItemAsEquipped(equippedItemsManager.EquippedItems[0].item, ItemType.Weapon,1 ,0);
                
                        var weaponItemSo = (WeaponItemSO)item.item;
                        weaponItemSo.UnequipWeapon(gameObject);
                        AddItem(item);
                        break;
                    
                    case "ArmorUI":
                        item = equippedItemsManager.EquippedItems[1];
                
                        equippedItemsManager.EquippedItems[1] = InventoryItem.GetEmptyItem();
                        equippedItemsManager.SetItemAsEquipped(equippedItemsManager.EquippedItems[1].item, ItemType.Armor,1 ,1);
                
                        var armorItemSo = (ArmorItemSO)item.item;
                        armorItemSo.Unequip(gameObject);
                        AddItem(item);
                        break;
                    
                    case "OutfitUI":
                        item = equippedItemsManager.EquippedItems[2];
                
                        equippedItemsManager.EquippedItems[2] = InventoryItem.GetEmptyItem();
                        equippedItemsManager.SetItemAsEquipped(equippedItemsManager.EquippedItems[2].item, ItemType.Outfit,1 ,2);
                
                        var outfitItemSo = (OutfitItemSO)item.item;
                        outfitItemSo.UnequipOutfit(gameObject);
                        AddItem(item);
                        
                        break;
                    case "HelmetUI":
                        item = equippedItemsManager.EquippedItems[3];
                        
                        equippedItemsManager.EquippedItems[3] = InventoryItem.GetEmptyItem();
                        equippedItemsManager.SetItemAsEquipped(equippedItemsManager.EquippedItems[3].item, ItemType.Helmet,1 ,3);
                
                        var helmetItemSo = (HelmetItemSO)item.item;
                        helmetItemSo.Unequip(gameObject);
                        AddItem(item);
                        
                        break;
                    case "ShoesUI":
                        item = equippedItemsManager.EquippedItems[4];
                        
                        equippedItemsManager.EquippedItems[4] = InventoryItem.GetEmptyItem();
                        equippedItemsManager.SetItemAsEquipped(equippedItemsManager.EquippedItems[4].item, ItemType.Shoes,1 ,4);
                
                        var shoesItemSo = (ShoesItemSO)item.item;
                        shoesItemSo.Unequip(gameObject);
                        AddItem(item);
                        
                        break;                    
                    case "QuickSlot1":
                        item = equippedItemsManager.EquippedItems[5];
                        
                        equippedItemsManager.EquippedItems[5] = InventoryItem.GetEmptyItem();
                        equippedItemsManager.SetItemAsEquipped(equippedItemsManager.EquippedItems[5].item, ItemType.Edible,1 ,5);
                
                        var edibleItemSo1 = (EdibleItemSO)item.item;
                        edibleItemSo1.Unequip(gameObject);
                        AddItem(item);
                        
                        break;          
                    case "QuickSlot2":
                        item = equippedItemsManager.EquippedItems[6];
                        
                        equippedItemsManager.EquippedItems[6] = InventoryItem.GetEmptyItem();
                        equippedItemsManager.SetItemAsEquipped(equippedItemsManager.EquippedItems[6].item, ItemType.Edible,1 ,6);
                
                        var edibleItemSo2 = (EdibleItemSO)item.item;
                        edibleItemSo2.Unequip(gameObject);
                        AddItem(item);
                        
                        break;
                    default:
                        throw new ArgumentException("Nie znaleziono przedmiotu do zamiany");
                }
            }
            else
            {
                
                var item = inventoryData.GetItemAt(itemIndex_2);

                if (item.item.ItemType == ItemType.Edible)
                {
                    EquipToQuickSlot(item, inventoryUiName);
                    inventoryData.RemoveItem(itemIndex_2, item.quantity);
                    return;
                }

                PerformAction(itemIndex_2);
            }            
        }
        
        inventoryData.SwapItems(itemIndex_1, itemIndex_2);
    }
    
    private void EquipToQuickSlot(InventoryItem newItem, string slotName)
    {
        var slotIndex = slotName == "QuickSlot1" ? 5 : 6;

        var equipped = equippedItemsManager.EquippedItems[slotIndex];

        if (!equipped.IsEmpty)
        {
            AddItem(equipped);
        }

        equippedItemsManager.EquippedItems[slotIndex] = new InventoryItem()
        {
            item = newItem.item,
            quantity = newItem.quantity,
            itemState = newItem.itemState
        };

        equippedItemsManager.InitializeEquippedSlots();
        SkillsManager.Instance.RefreshSlotUI();
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
                case "QuickSlot1":
                    inventoryItem = equippedItemsManager.EquippedItems[5];
                    break;
                case "QuickSlot2":
                    inventoryItem = equippedItemsManager.EquippedItems[6];
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
        foreach (var currentParam in inventoryItem.itemState)
        {
            var param = currentParam;
            var defaultParam = inventoryItem.item.DefaultParametersList
                .FirstOrDefault(p => p.itemParameter == param.itemParameter);

            sb.AppendLine();

            if (defaultParam.itemParameter != null)
            {
                sb.Append(
                    $"{currentParam.itemParameter.ParameterName}: " +
                    $"{currentParam.value} / {defaultParam.value}");
            }
            else
            {
                sb.Append(
                    $"{currentParam.itemParameter.ParameterName}: +{currentParam.value}");
            }
        }
        
        sb.AppendLine();
        sb.AppendLine();
        sb.Append(inventoryItem.item.Description);

        if (Player.Instance.IsNearBlacksmith &&
            inventoryItem.item.ItemType == ItemType.Weapon)
        {
            sb.AppendLine();
            sb.AppendLine();

            if (WorkManager.Instance.CanUpgrade())
            {
                var cost =
                    WorkManager.Instance.GetUpgradeCost(inventoryItem);

                var remaining =
                    WorkManager.Instance.GetRemainingUpgrades();

                sb.Append(
                    $"<color=#4CFF7A>" +
                    $"-{cost} gold → +2 damage" +
                    $"</color>"
                );

                sb.AppendLine();

                sb.Append(
                    $"<color=#AAAAAA>" +
                    $"{remaining} upgrades remaining" +
                    $"</color>"
                );
            }
            else
            {
                sb.Append(
                    "<color=#FFD700>" +
                    "NO UPGRADES REMAINING" +
                    "</color>"
                );
            }
        }
        
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

        // inventoryData.NotifyInventoryUpdated();

        if (ES3.KeyExists("playerInventory_equippedItems"))
        {
           var equipped = ES3.Load<List<InventoryItem>>("playerInventory_equippedItems");
           equippedItemsManager.EquippedItems = equipped;
           // equippedItemsManager.InitializeEquippedSlots();
           WeaponManager.Instance.SetWeapon((WeaponItemSO)equippedItemsManager.EquippedItems[0].item, equippedItemsManager.EquippedItems[0].itemState, true);
           ArmorManager.Instance.SetArmor((ArmorItemSO)equippedItemsManager.EquippedItems[1].item, equippedItemsManager.EquippedItems[1].itemState, true);
           OutfitManager.Instance.SetOutfit((OutfitItemSO)equippedItemsManager.EquippedItems[2].item, equippedItemsManager.EquippedItems[2].itemState, true);
           ArmorManager.Instance.SetHelmet((HelmetItemSO)equippedItemsManager.EquippedItems[3].item, equippedItemsManager.EquippedItems[3].itemState, true);
           ArmorManager.Instance.SetShoes((ShoesItemSO)equippedItemsManager.EquippedItems[4].item, equippedItemsManager.EquippedItems[4].itemState, true);
           QuickItemManager.Instance.SetQuickItem((EdibleItemSO)equippedItemsManager.EquippedItems[5].item, equippedItemsManager.EquippedItems[5].itemState, equippedItemsManager.EquippedItems[5].quantity,5, true);
           QuickItemManager.Instance.SetQuickItem((EdibleItemSO)equippedItemsManager.EquippedItems[6].item, equippedItemsManager.EquippedItems[6].itemState,equippedItemsManager.EquippedItems[6].quantity, 6,true);
        }
        
        inventoryData.NotifyInventoryUpdated();
        Debug.Log("Equipped loaded");
        equippedItemsManager.InitializeEquippedSlots();
        Debug.Log("Equipped UI refreshed");
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

    public void SwapQuickSlots(string targetSlotName, string draggedSlotName)
    {
        var targetIndex = GetQuickSlotIndex(targetSlotName);
        var draggedIndex = GetQuickSlotIndex(draggedSlotName);

        if (targetIndex == -1 || draggedIndex == -1) return;

        var equipped = equippedItemsManager.EquippedItems;

        var temp = equipped[targetIndex];
        equipped[targetIndex] = equipped[draggedIndex];
        equipped[draggedIndex] = temp;

        equippedItemsManager.InitializeEquippedSlots();
        QuickItemManager.Instance.RefreshUI();
        SkillsManager.Instance.RefreshSlotUI();
    }

    private int GetQuickSlotIndex(string slotName)
    {
        return slotName switch
        {
            "QuickSlot1" => 5,
            "QuickSlot2" => 6,
            _ => -1
        };
    }
    
    public int GetItemCount(InventoryItem item)
    {
        var count = 0;

        foreach (var inventoryItem in inventoryData.inventoryItems)
        {
            if (inventoryItem.IsEmpty)
                continue;

            if (inventoryItem.item.Name == item.item.Name)
            {
                count += inventoryItem.quantity;
            }
        }

        return count;
    }
    
    public bool IsEmptySlotInEquipment() => inventoryData.inventoryItems.Any(item => item.IsEmpty);
    
    public void ResetSelectedItem()
    {
        inventoryUI.ResetSelection();
    }
}
