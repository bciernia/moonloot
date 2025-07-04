using UnityEngine;

public class ItemGiver : MonoBehaviour
{ 
    [SerializeField] private InventoryItem itemToGive;

    public void GiveItemToPlayer()
    {
        InventoryController.Instance.AddItem(new InventoryItem() { item = itemToGive.item, quantity = itemToGive.quantity});
    }   
}