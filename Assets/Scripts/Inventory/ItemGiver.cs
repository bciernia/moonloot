using UnityEngine;

public class ItemGiver : MonoBehaviour
{ 
    [SerializeField] private InventoryItem itemToGive;
    [SerializeField] private int quantity;

    public void GiveItemToPlayer()
    {
        Debug.Log(itemToGive.item.name);
    }   
}