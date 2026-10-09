using System.Collections.Generic;
using UnityEngine;

public class ItemGiver : MonoBehaviour
{ 
    [SerializeField] private List<InventoryItem> itemToGive;

    public void GiveItemToPlayer(int itemIndex = 0)
    {
        var item = itemToGive[itemIndex];
        if (!InventoryController.Instance.CanAddItem(item.item, item.quantity))
        {
            FloatingTextManager.Instance.ShowWarningText(
                "Inventory is full",
                transform);
            return;
        }

        InventoryController.Instance.AddItem(new InventoryItem()
            { item = item.item, quantity = item.quantity, itemState = item.itemState });
    }
}
