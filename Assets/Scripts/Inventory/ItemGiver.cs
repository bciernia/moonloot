using System.Collections.Generic;
using UnityEngine;

public class ItemGiver : MonoBehaviour
{ 
    [SerializeField] private List<InventoryItem> itemToGive;

    public void GiveItemToPlayer(int itemIndex = 0)
    {
        InventoryController.Instance.AddItem(new InventoryItem()
            { item = itemToGive[itemIndex].item, quantity = itemToGive[itemIndex].quantity });
    }
}