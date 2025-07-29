using System.Collections.Generic;
using System.Text;
using Inventory.NewInventory.Model;
using UnityEngine;

public class InventoryController : Singleton<InventoryController>
{
    [SerializeField] private UIInventoryPage inventoryUI;

    [SerializeField] public InventorySO inventoryData;

    public List<InventoryItem> initialItems = new List<InventoryItem>();

    [SerializeField] private AudioClip audioClip;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private Transform playerTransform;

    [SerializeField] private EquippedItemsManager equippedItemsManager;
    [SerializeField] private EquippedItemsManagerSO equippedItemsManagerSo;
    
    private void Start()
    {
        PrepareUI();
        PrepareInventoryData();
    }

    private void ChangeGoldAmount(GoldItemSO gold)
    {
        var goldAmount = gold.Amount;
        
        if (goldAmount * -1 > inventoryData.Gold)
        {
            Debug.Log("you don't have enough money");
            return;
        }
        
        inventoryData.Gold += goldAmount;

        inventoryUI.UpdateGoldAmount(inventoryData.Gold);
    }

    public void ChangeGoldAmount(int goldAmount)
    {
        if (goldAmount * -1 > inventoryData.Gold)
        {
            Debug.Log("you don't have enough money");
            return;
        }
        
        inventoryData.Gold += goldAmount;

        inventoryUI.UpdateGoldAmount(inventoryData.Gold);
    }
    
    public void AddItem(InventoryItem item)
    {
        if (item.item is GoldItemSO goldItem)
        {
            ChangeGoldAmount(goldItem);
            return;
        }
        
        inventoryData.AddItem(item);
    }
    
    private void PrepareInventoryData()
    {
        inventoryData.Initialize();
        inventoryData.OnInventoryUpdated += UpdateInventoryUI;
        foreach (var item in initialItems)
        {
            if(item.IsEmpty) continue;
            
            inventoryData.AddItem(item);
        }
    }

    private void UpdateInventoryUI(Dictionary<int, InventoryItem> inventoryState)
    {
        inventoryUI.ResetAllData();
        foreach (var item in inventoryState)
        {
            inventoryUI.UpdateData(item.Key, item.Value.item.Image, item.Value.quantity);
        }
        inventoryUI.UpdateGoldAmount(inventoryData.Gold);
    }

    private void PrepareUI()
    {
        inventoryUI.InitializeInventoryUI(inventoryData.Size);
        inventoryUI.OnDescriptionRequested += HandleDescriptionRequest;
        inventoryUI.OnSwapItems += HandleSwapItems;
        inventoryUI.OnStartDragging += HandleDragging;
        inventoryUI.OnItemActionRequested += HandleItemActionRequest;
    }

    private void HandleItemActionRequest(int itemIndex)
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
    
    private void HandleDragging(int itemIndex)
    {
        InventoryItem item;

        if (itemIndex == -1)
        {
            item = equippedItemsManager.EquippedItems[0];
        }
        else
        {
            item = inventoryData.GetItemAt(itemIndex);
        }
        
        if (item.IsEmpty) return;

        inventoryUI.CreateDraggedItem(item.item.Image, item.quantity);
    }
    
    private void HandleSwapItems(int itemIndex_1, int itemIndex_2)
    {
        if (itemIndex_1 == -1 || itemIndex_2 == -1)
        {
            var inventoryItem = inventoryData.GetItemAt(itemIndex_2);
            if (inventoryItem.IsEmpty)
            {
                var item = equippedItemsManager.EquippedItems[0];

                //TODO zmienić na typ, żeby nie porównywać po nazwie
                if (item.item.Name == "Fists") return;
                
                equippedItemsManager.EquippedItems[0] = InventoryItem.GetEmptyItem();
                equippedItemsManager.SetItemAsEquipped(equippedItemsManager.EquippedItems[0].item);
                
                var t = (WeaponItemSO)item.item;
                t.UnequipWeapon(gameObject);
                
                AddItem(item);
            }
            else
            {
                PerformAction(itemIndex_2);
            }            
        }
        
        inventoryData.SwapItems(itemIndex_1, itemIndex_2);
    }
    
    private void HandleDescriptionRequest(int itemIndex)
    {
        InventoryItem inventoryItem;
        if (itemIndex == -1)
        {
            inventoryItem = equippedItemsManager.EquippedItems[0];
        }
        else
        {
            inventoryItem = inventoryData.GetItemAt(itemIndex);
        }
        
        if (inventoryItem.IsEmpty)
        {
            inventoryUI.ResetSelection();
            return;
        }
        var item = inventoryItem.item;
        var description = PrepareDescription(inventoryItem);
        inventoryUI.UpdateDescription(itemIndex, item.Image, item.Name, description);
    }

    private string PrepareDescription(InventoryItem inventoryItem)
    {
        var sb = new StringBuilder();
        sb.Append(inventoryItem.item.Description);
        sb.AppendLine();
        for (var i = 0; i < inventoryItem.itemState.Count; i++)
        {
            sb.Append($"{inventoryItem.itemState[i].itemParameter.ParameterName}: " +
                      $"{inventoryItem.itemState[i].value} / " +
                      
                      $"{inventoryItem.item.DefaultParametersList[i].value}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public bool UseItemById(int itemId)
    {
        return inventoryData.FindItemById(itemId);
    }
}
